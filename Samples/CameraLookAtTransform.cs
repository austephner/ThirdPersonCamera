using UnityEngine;

public class CameraLookAtTransform : MonoBehaviour
{
    public new ThirdPersonCamera camera;
    public new Transform transform;

    private void Update()
    {
        camera.lookTarget = transform;
    }
}