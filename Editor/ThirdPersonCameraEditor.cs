using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ThirdPersonCamera))]
    public class ThirdPersonCameraEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thirdPersonCamera = (ThirdPersonCamera)target;

            if (!serializedObject.FindProperty("_heightTransform").objectReferenceValue &&
                !serializedObject.FindProperty("_horizontalOffsetTransform").objectReferenceValue &&
                !serializedObject.FindProperty("_xAxis").objectReferenceValue &&
                !serializedObject.FindProperty("_cameraTransform").objectReferenceValue &&
                GUILayout.Button("Create Game Object Hierarchy"))
            {
                var heightTransform = new GameObject("Height");
                heightTransform.transform.SetParent(thirdPersonCamera.transform);
                heightTransform.transform.localPosition = new Vector3(0, 1, 0);
                serializedObject.FindProperty("_heightTransform").objectReferenceValue = heightTransform;
                
                var horizontalOffsetTransform = new GameObject("Horizontal Offset");
                horizontalOffsetTransform.transform.SetParent(heightTransform.transform);
                serializedObject.FindProperty("_horizontalOffsetTransform").objectReferenceValue = horizontalOffsetTransform;
                
                var xAxis = new GameObject("X Axis");
                xAxis.transform.SetParent(horizontalOffsetTransform.transform);
                serializedObject.FindProperty("_xAxis").objectReferenceValue = xAxis;
                
                var cameraTransform = new GameObject("Camera");
                cameraTransform.transform.SetParent(xAxis.transform);
                var camera = cameraTransform.AddComponent<Camera>();
                cameraTransform.transform.localPosition = new Vector3(0, 0, -10);
                serializedObject.FindProperty("_cameraTransform").objectReferenceValue = cameraTransform;
                serializedObject.FindProperty("_camera").objectReferenceValue = camera; 

                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                throw new ExitGUIException();
            }
            
            base.OnInspectorGUI();

            var showProperties = serializedObject.FindProperty("_showProperties");

            if (!showProperties.boolValue)
            {
                return;
            }

            // EDITABLE
            thirdPersonCamera.zoom = EditorGUILayout.FloatField("Zoom", thirdPersonCamera.zoom);
            thirdPersonCamera.horizontalOffset = EditorGUILayout.FloatField("Horizontal Offset", thirdPersonCamera.horizontalOffset);
            thirdPersonCamera.height = EditorGUILayout.FloatField("Height", thirdPersonCamera.height);
            
            // READONLY
            GUI.enabled = false;
            EditorGUILayout.Toggle("Forward Camera Clipping", thirdPersonCamera.forwardCameraClipping);
            EditorGUILayout.Toggle("Horizontal Camera Clipping", thirdPersonCamera.horizontalCameraClipping);
            EditorGUILayout.ObjectField("Camera Transform", thirdPersonCamera.cameraTransform, typeof(Transform));
            EditorGUILayout.ObjectField("Camera", thirdPersonCamera.camera, typeof(Camera));
        }
    }
}