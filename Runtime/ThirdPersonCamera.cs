using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

/// <summary>
/// Third person camera that can follow a target transform. Works best with character controllers and game objects that
/// are moved on the standard "Update" call stack. Can be configured to pick up input on its own or be controlled
/// externally from another script.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    #region Settings

    [Header("General"), SerializeField, Tooltip("Enables debug logging and additional debug lines/gizmos.")] 
    private bool _drawDebug;

    [SerializeField, Tooltip("Enables debug gizmos.")] 
    private bool _drawGizmos;

    public Transform followTarget;

    [FormerlySerializedAs("_selfGetPlayerLookInput"), SerializeField, 
     Tooltip("Allows this third person camera to get Input data and update itself accordingly.")] 
    private bool _selfGetPlayerInput = true;

    [SerializeField, Tooltip("Inverts all horizontal input values.")] 
    private bool _invertHorizontalCameraInput;

    [SerializeField, Tooltip("Inverts all vertical input values.")] 
    private bool _invertVerticalCameraInput = true;

    [SerializeField, Tooltip("The movement speed when a follow target has been assigned.")]
    private float _moveLerpSpeed = 1.0f;

    [SerializeField, Tooltip("The zoom speed.")] 
    private float _zoomLerpSpeed = 1.0f;

    [SerializeField] 
    private float _minZoomIn = 1, _maxZoomOut = 30;

    [Header("Components"), SerializeField] 
    private Camera _camera;
    
    [SerializeField, Tooltip("The transform which controls the Y position of the camera.")] 
    private Transform _heightTransform;

    [SerializeField, Tooltip("The horizontal offset transform. Must be a child object of the height transform.")] 
    private Transform _horizontalOffsetTransform;

    [SerializeField, Tooltip("The X rotation axis transform. Must be a child object of the horizontal offset transform.")] 
    private Transform _xAxis;

    [SerializeField, Tooltip("The camera's transform. Must be a child object of the X Axis transform.")] 
    private Transform _cameraTransform;

    [Header("Rotation"), SerializeField, 
     Tooltip("The angle in degrees at which the camera can no longer be rotated to look upwards. Often this is a negative value like \"-60\".")] 
    private float _lookUpMaxAngle = -60;

    [SerializeField, 
     Tooltip("The angle in degrees at which the camera can no longer be rotated to look downwards. Often this is a positive value like \"90\".")]
    private float _lookDownMinAngle = 90;

    [SerializeField, Tooltip("How quickly the camera rotates horizontally.")] 
    private float _horizontalRotationLerpSpeed = 1.0f;

    [SerializeField, Tooltip("How sensitive horizontal rotations are.")] 
    private float _horizontalRotationSensitivity = 1.0f;

    [SerializeField, Tooltip("How quickly the camera rotates vertically.")] 
    private float _verticalRotationLerpSpeed = 1.0f;

    [SerializeField, Tooltip("How sensitive vertical rotations are.")] 
    private float _verticalRotationSensitivity = 1.0f;

    [Header("Clipping"), SerializeField, Tooltip("All layers that the camera cannot clip through.")] 
    private LayerMask _cameraClippingLayermask;
    
    [FormerlySerializedAs("_cameraClipRadius"), SerializeField, 
     Tooltip("The spherical forward-direction raycast radius of the camera clipping. Higher values will cause more prominent camera adjustment when testing for collidable objects.")] 
    private float _forwardCameraClipRadius = 0.1f;

    [SerializeField, 
     Tooltip("The spherical horizontal-direction raycast radius of the camera clipping. Higher values will cause more prominent camera adjustment when testing for collidable objects.")] 
    private float _horizontalCameraClipRadius = 0.1f;

    [SerializeField, 
     Tooltip("When the camera clip raycast detects something, it will offset itself in the normal direction to a position by this distance value. Basically, it's \"how far away\" the camera is positioned from a detected object's normal point.")] 
    private float _cameraClipPointOffset = 0.1f;

    [SerializeField, Tooltip("How fast the camera lerps to a clipped offset position.")] 
    private float _cameraClippingLerpSpeed = 10.0f;

    [Header("Properties"), SerializeField, Tooltip("Used only in the inspector to show properties.")] 
    private bool _showProperties = true;
    
    #endregion

    #region Properties

    public float zoom
    {
        get => _zoom;
        set => _targetZoom = value;
    }

    public float horizontalOffset
    {
        get => _horizontalOffset;
        set => _horizontalOffset = value;
    }
    
    public float height
    {
        get => _heightTransform.localPosition.y;
        set => _heightTransform.localPosition = new Vector3(0, value, 0);
    }
    
    /// <summary>
    /// Allows for X and Y adjustment to the camera's look direction. It's overridden if <see cref="lookTarget"/> isn't
    /// null or unassigned.
    /// </summary>
    public Vector2 lookInput { get; set; }
    
    /// <summary>
    /// Forces the camera's look position, disregarding all look input.
    /// </summary>
    public Transform lookTarget { get; set; }

    public Transform cameraTransform => _cameraTransform;

    public new Camera camera => _camera;

    public bool forwardCameraClipping => _forwardCameraClipping;

    public bool horizontalCameraClipping => _horizontalCameraClipping;

    public float horizontalLookSensitivity
    {
        get => _horizontalRotationSensitivity;
        set => _horizontalRotationSensitivity = value;
    }

    public float verticalLookSensitivity
    {
        get => _verticalRotationSensitivity;
        set => _verticalRotationSensitivity = value;
    }

    public bool invertHorizontalLook
    {
        get => _invertHorizontalCameraInput;
        set => _invertHorizontalCameraInput = value;
    }
    
    public bool invertVerticalLook
    {
        get => _invertVerticalCameraInput;
        set => _invertVerticalCameraInput = value;
    }

    #endregion

    #region Private

    private Transform _transform;

    /// <summary>
    /// Whether or not the camera has detected forward camera clipping. 
    /// </summary>
    private bool _forwardCameraClipping;

    /// <summary>
    /// Whether or not the camera has detected horizontal clipping.
    /// </summary>
    private bool _horizontalCameraClipping;

    private Vector3 
        _targetPosition,
        _forwardCameraClippingPosition,
        _horizontalCameraClippingPosition;

    private Quaternion
        _targetRotationY,
        _targetRotationX;

    private float 
        _zoom,
        _targetZoom,
        _horizontalOffset;

    #endregion

    #region Unity Events

    private void OnEnable()
    {
        _zoom = _targetZoom = _cameraTransform.localPosition.z;
        _horizontalOffset = _horizontalOffsetTransform.localPosition.x;
        _transform = transform;
        _targetRotationY = _transform.rotation;
        _targetRotationX = _xAxis.rotation;
    }

    private void Update()
    {
        GatherPlayerInputs();
        CalculateTargetFields();
    }

    private void LateUpdate()
    {
        ClampZoom();
        CalculateZoom();
        UpdateTransform();
        UpdateAxisX();
        UpdateForwardCameraClipping();
        UpdateHorizontalCameraClipping();
        UpdateHorizontalTransform();
        UpdateCameraTransform();
    }
    
    protected void OnDrawGizmos()
    {
        if (!_drawGizmos)
        {
            return;
        }
        
        // draw height 
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _heightTransform.position);
        
        // draw zoom

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(_horizontalOffsetTransform.position, _cameraTransform.position);
        
        // draw horizontal offset

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_heightTransform.position, _horizontalOffsetTransform.position);
            
        Gizmos.color = Color.blue;;
        Gizmos.DrawLine(_heightTransform.position, _cameraTransform.position);
        
        // draw cipping settings

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_cameraTransform.position, _forwardCameraClipRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(_cameraTransform.position, _horizontalCameraClipRadius);
    }
    
    private void OnValidate()
    {
        if (_heightTransform && _xAxis && _horizontalOffsetTransform)
        {
            Assert.IsTrue(_horizontalOffsetTransform.parent == _heightTransform);
            Assert.IsTrue(_xAxis.parent == _horizontalOffsetTransform);
        }

        if (_cameraTransform)
        {
            Assert.IsTrue(_cameraTransform.parent == _xAxis);
        }
    }

    #endregion

    #region Private Utilities

    private void GatherPlayerInputs()
    {
        if (!_selfGetPlayerInput) return;
        lookInput = GetPlayerLookInput();
        if (GetPlayerAlternateHorizontalOffsetInput()) horizontalOffset *= -1;
        zoom += GetPlayerZoomDeltaInput() * _zoomLerpSpeed;
    }

    #endregion

    #region Protected Utilities

    protected virtual void UpdateTransform()
    {
        var deltaTime = Time.deltaTime;
        
        _transform.position = Vector3.MoveTowards(
            _transform.position, 
            _targetPosition, 
            _moveLerpSpeed * deltaTime);
        
        _transform.rotation = Quaternion.RotateTowards(
            _transform.rotation, 
            _targetRotationY,
            _horizontalRotationLerpSpeed);
    }

    protected virtual void UpdateAxisX()
    {
        _xAxis.localRotation = Quaternion.RotateTowards(
            _xAxis.localRotation, 
            _targetRotationX, 
            _verticalRotationLerpSpeed);
    }

    protected virtual void UpdateForwardCameraClipping()
    {
        if (_drawDebug)
        {
            Debug.DrawLine(_heightTransform.position, _cameraTransform.position, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.right * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.left * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.back * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.forward * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.up * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.down * _forwardCameraClipRadius, Color.yellow);
        }

        var raycastTargetPosition = _xAxis.TransformPoint(new Vector3(0, 0, _zoom));
        
        if (Physics.SphereCast(
            _heightTransform.position,
            _forwardCameraClipRadius,
            (raycastTargetPosition - _heightTransform.position).normalized,
            out RaycastHit spherecastHit,
            Mathf.Abs(_zoom),
            _cameraClippingLayermask))
        {
            _forwardCameraClipping = true;
            _forwardCameraClippingPosition = spherecastHit.point + spherecastHit.normal * _cameraClipPointOffset;
        }
        else
        {
            _forwardCameraClipping = false;
        }
    }

    protected virtual void UpdateHorizontalCameraClipping()
    {
        if (_drawDebug)
        {
            Debug.DrawLine(_heightTransform.position, _cameraTransform.position, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.right * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.left * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.back * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.forward * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.up * _forwardCameraClipRadius, Color.yellow);
            Debug.DrawLine(_cameraTransform.position, _cameraTransform.position + Vector3.down * _forwardCameraClipRadius, Color.yellow);
        }

        var raycastTargetPosition = _heightTransform.TransformPoint(new Vector3(_horizontalOffset, 0, 0));
        
        if (Physics.SphereCast(
            _heightTransform.position,
            _horizontalCameraClipRadius,
            (raycastTargetPosition - _heightTransform.position).normalized,
            out RaycastHit spherecastHit,
            Mathf.Abs(_horizontalOffset),
            _cameraClippingLayermask))
        {
            _horizontalCameraClipping = true;
            _horizontalCameraClippingPosition = spherecastHit.point + spherecastHit.normal * _cameraClipPointOffset;
        }
        else
        {
            _horizontalCameraClipping = false;
        }
    }

    protected virtual void UpdateCameraTransform()
    {
        _cameraTransform.position = Vector3.Lerp(
            _cameraTransform.position, 
            _forwardCameraClipping 
                ? _forwardCameraClippingPosition 
                : _xAxis.TransformPoint(new Vector3(0, 0, _zoom)), 
            _cameraClippingLerpSpeed * Time.deltaTime);
    }

    protected virtual void UpdateHorizontalTransform()
    {
        _horizontalOffsetTransform.position = Vector3.Lerp(
            _horizontalOffsetTransform.position, 
            _horizontalCameraClipping 
                ? _horizontalCameraClippingPosition 
                : _heightTransform.TransformPoint(new Vector3(_horizontalOffset, 0, 0)), 
            _cameraClippingLerpSpeed * Time.deltaTime);
    }

    protected virtual void ClampZoom()
    {
        _zoom = -Mathf.Clamp(Mathf.Abs(_zoom), Mathf.Abs(_minZoomIn), Mathf.Abs(_maxZoomOut));
        _targetZoom = -Mathf.Clamp(Mathf.Abs(_targetZoom), Mathf.Abs(_minZoomIn), Mathf.Abs(_maxZoomOut));
    }
    
    protected virtual void CalculateTargetFields()
    {
        _targetPosition = followTarget ? followTarget.position : _transform.position;

        if (lookTarget)
        {
            var direction = lookTarget.position - _heightTransform.position;
            var rotation = Quaternion.LookRotation(direction);
            var eulerAngles = ClampEulerAngles(rotation.eulerAngles);
            _targetRotationY = Quaternion.Euler(CalculateRotationY(eulerAngles, Vector2.zero));
            _targetRotationX = Quaternion.Euler(CalculateRotationX(eulerAngles, Vector2.zero));
        }
        else if (lookInput != Vector2.zero)
        {
            _targetRotationY = Quaternion.Euler(CalculateRotationY(_transform.rotation.eulerAngles, lookInput));
            _targetRotationX = Quaternion.Euler(CalculateRotationX(_xAxis.localRotation.eulerAngles, lookInput));
        }
    }

    protected virtual Vector2 GetPlayerLookInput()
    {
        return new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y"));
    }

    protected virtual float GetPlayerZoomDeltaInput()
    {
        return Input.GetAxis("Mouse ScrollWheel");
    }

    protected virtual bool GetPlayerAlternateHorizontalOffsetInput()
    {
        return Input.GetKeyDown(KeyCode.LeftAlt);
    }

    protected virtual void CalculateZoom()
    {
        _zoom = Mathf.Lerp(_zoom, _targetZoom, _zoomLerpSpeed * Time.deltaTime);
    }

    protected virtual Vector3 CalculateRotationY(Vector3 eulerAngles, Vector2 input)
    {
        eulerAngles.y += input.x * _horizontalRotationSensitivity * (_invertHorizontalCameraInput ? -1.0f : 1.0f);
        eulerAngles.x = 0;
        eulerAngles.z = 0;

        return eulerAngles;
    }

    protected virtual Vector3 CalculateRotationX(Vector3 eulerAngles, Vector2 input)
    {
        eulerAngles.x += input.y * _verticalRotationSensitivity * (_invertVerticalCameraInput ? -1.0f : 1.0f);
        eulerAngles.y = 0;
        eulerAngles.z = 0;
        
        return ClampEulerAngles(eulerAngles);
    }

    #endregion

    #region Public Utilities
    
    public Vector3 ClampEulerAngles(Vector3 eulerAngles)
    {
        if (eulerAngles.x > 180f)
        {
            eulerAngles.x -= 360;
        }

        eulerAngles.x = Mathf.Clamp(eulerAngles.x, _lookUpMaxAngle, _lookDownMinAngle);

        return eulerAngles;
    }

    public Vector3 GetHorizontalTransformDirection(Vector3 direction)
    {
        return _horizontalOffsetTransform.TransformDirection(direction);
    }
    
    public Vector3 GetCenterScreenPosition(LayerMask layerMask, float sphereCastRadius)
    {
        var centerScreen = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f);
        
        if (Physics.Raycast(
            _camera.ScreenPointToRay(centerScreen),
            out RaycastHit rh,
            Mathf.Infinity,
            layerMask))
        {
            return rh.point;
        }
        
        if (Physics.SphereCast(
            _camera.ScreenPointToRay(centerScreen), 
            sphereCastRadius, 
            out RaycastHit sh, 
            Mathf.Infinity, 
            layerMask))
        {
            return sh.point;
        }

        return _camera.ScreenToWorldPoint(centerScreen) +
               _cameraTransform.forward * 10.0f;
    }
    
    /// <summary>
    /// Starts a raycast at the <see cref="horizontalOffset"/> transform position and points forward using the
    /// camera's transform forward direction. This is useful for avoiding raycasting anything that would
    /// otherwise appear between that transform and the camera's transform. 
    /// </summary>
    /// <param name="layerMask"></param>
    /// <param name="sphereCastRadius"></param>
    /// <returns></returns>
    public Vector3 GetForwardHorizontalTransformRaycastPosition(LayerMask layerMask, float sphereCastRadius)
    {
        var centerScreen = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f);
        var origin = _horizontalOffsetTransform.position;
        var direction = _cameraTransform.forward;
        
        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit rh,
            Mathf.Infinity,
            layerMask))
        {
            return rh.point;
        }
        
        if (Physics.SphereCast(
            origin,
            sphereCastRadius,
            direction,
            out RaycastHit sh, 
            Mathf.Infinity, 
            layerMask))
        {
            return sh.point;
        }

        return _camera.ScreenToWorldPoint(centerScreen) +
               _cameraTransform.forward * 10.0f;
    }

    #endregion
}