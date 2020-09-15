using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxFOVUpdate : MonoBehaviour
{
    [SerializeField] private Camera farCamera;
    [SerializeField] private Camera nearCamera;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        UpdateParallaxFieldOfView();
        UpdateParallaxClipPlane();
    }

    private void UpdateParallaxFieldOfView()
    {
        float distanceFromOrigin = Mathf.Abs(mainCamera.transform.position.z);
        float cameraSize = mainCamera.orthographicSize;

        float fov = Mathf.Atan(cameraSize / distanceFromOrigin) * Mathf.Rad2Deg * 2;
        farCamera.fieldOfView = fov;
        nearCamera.fieldOfView = fov;
    }

    private void UpdateParallaxClipPlane()
    {
        float distanceFromOrigin = Mathf.Abs(mainCamera.transform.position.z);

        farCamera.nearClipPlane = distanceFromOrigin;
        farCamera.farClipPlane = mainCamera.farClipPlane;

        nearCamera.nearClipPlane = mainCamera.nearClipPlane;
        nearCamera.farClipPlane = distanceFromOrigin;
    }
}
