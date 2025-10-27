using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

public class CameraController : Singleton<CameraController>
{
    private const int DEFAULT_PRIORITY = 10;

    [Header("Camera References")]
    [SerializeField]
    private GameObject cameraTarget;
    [SerializeField]
    private CinemachineVirtualCamera topDownCamera;
    [SerializeField]
    private CinemachineVirtualCamera focusCamera;
    [SerializeField]
    private CameraMode defaultMode = CameraMode.TopDown;
    [SerializeField]
    private CameraMode currentMode;

    [Header("Movement Settings")]
    [SerializeField] private float cameraSpeed = 10f;
    [SerializeField] private float cameraDamping = 5f;
    [SerializeField] private Vector2 cameraBoundsMin = new Vector2(-100, -100);
    [SerializeField] private Vector2 cameraBoundsMax = new Vector2(100, 100);

    [Header("Zoom Settings")]
    [SerializeField] private float cameraZoomSpeed = 1f;
    [SerializeField] private float cameraZoomMin = 15f;
    [SerializeField] private float cameraZoomMax = 100f;
    [SerializeField] private float cameraZoomDefault = 50f;

    [Header("Rotation Settings")]
    [SerializeField] private bool enableRotation = false;
    [SerializeField] private float cameraRotationSpeed = 50f;

    [Header("Control Settings")]
    [SerializeField] private bool isLocked = false;

    // Cached references
    private Transform cameraTargetTransform;

    private Coroutine panCoroutine;
    private Coroutine zoomCoroutine;
    private Coroutine rotateCoroutine;

    // Events
    public event Action<CinemachineVirtualCamera> onCameraChanged;
    public event Action onSelectAction;
    public event Action onDeselectAction;
    public event Action onFocusAction;

    // Public properties
    public bool IsLocked { get => isLocked; set => isLocked = value; }
    public GameObject CameraTarget { get => cameraTarget; }

    void Start()
    {
        cameraTargetTransform = cameraTarget.transform;
        topDownCamera.m_Lens.FieldOfView = cameraZoomDefault;
        ChangeCamera(defaultMode);
    }

    private void OnValidate()
    {
        // Ensure valid zoom range
        cameraZoomMin = Mathf.Max(1f, cameraZoomMin);
        cameraZoomMax = Mathf.Clamp(cameraZoomMax, cameraZoomMin + 1, 179f);
        cameraZoomDefault = Mathf.Clamp(cameraZoomDefault, cameraZoomMin, cameraZoomMax);

        // Ensure valid camera bounds
        cameraBoundsMax.x = Mathf.Max(cameraBoundsMin.x + 1, cameraBoundsMax.x);
        cameraBoundsMax.y = Mathf.Max(cameraBoundsMin.y + 1, cameraBoundsMax.y);
    }

    public void ChangeCamera(CameraMode mode)
    {
        currentMode = mode;
        CinemachineVirtualCamera camera = GetCamera(mode);
        onCameraChanged?.Invoke(camera);
        camera.Priority = DEFAULT_PRIORITY;
        camera.MoveToTopOfPrioritySubqueue();
    }

    private CinemachineVirtualCamera GetCamera(CameraMode mode)
    {
        switch (mode)
        {
            case CameraMode.TopDown:
                return topDownCamera;
            case CameraMode.Focus:
                return focusCamera;
            default:
                return null;
        }
    }

    /// <summary>
    /// Safely stops a coroutine and sets the reference to null
    /// </summary>
    private void SafeStopCoroutine(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }


    public void OnPanChange(InputAction.CallbackContext context)
    {
        if (isLocked)
            return;

        if (context.performed)
        {
            SafeStopCoroutine(ref panCoroutine);
            panCoroutine = StartCoroutine(ProcessPan(context));
            ChangeCamera(CameraMode.TopDown);
        }
        else if (context.canceled)
        {
            SafeStopCoroutine(ref panCoroutine);
        }
    }

    public void OnZoomChanged(InputAction.CallbackContext context)
    {
        if (isLocked)
            return;

        if (context.performed)
        {
            SafeStopCoroutine(ref zoomCoroutine);
            zoomCoroutine = StartCoroutine(ProcessZoom(context));
        }
        else if (context.canceled)
        {
            SafeStopCoroutine(ref zoomCoroutine);
        }
    }

    public void OnRotateChange(InputAction.CallbackContext context)
    {
        if (!enableRotation || isLocked)
            return;

        if (context.performed)
        {
            SafeStopCoroutine(ref rotateCoroutine);
            rotateCoroutine = StartCoroutine(ProcessRotate(context));
        }
        else if (context.canceled)
        {
            SafeStopCoroutine(ref rotateCoroutine);
        }
    }

    public void OnFocusChange(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //Debug.Log("Focus button pressed... What's it gonna be?");
        }
        else if (context.performed)
        {
            //Debug.Log("Double tapped - Focus");
            ChangeCamera(CameraMode.Focus);
            onFocusAction?.Invoke();
        }
        else if (context.canceled)
        {
            //Debug.Log("Single tap - Select");
            onSelectAction?.Invoke();
        }
    }



    public IEnumerator ProcessPan(InputAction.CallbackContext context)
    {
        while (true)
        {
            Vector2 inputVector = context.ReadValue<Vector2>();

            // Calculate target position with smooth damping
            Vector3 moveVector = new Vector3(inputVector.x, 0, inputVector.y);
            Vector3 targetPosition = cameraTargetTransform.position + moveVector * cameraSpeed * Time.deltaTime;

            // Apply camera boundaries
            targetPosition.x = Mathf.Clamp(targetPosition.x, cameraBoundsMin.x, cameraBoundsMax.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, cameraBoundsMin.y, cameraBoundsMax.y);

            // Smooth movement with damping
            cameraTargetTransform.position = Vector3.Lerp(
                cameraTargetTransform.position,
                targetPosition,
                cameraDamping * Time.deltaTime
            );

            yield return null;
        }
    }

    public IEnumerator ProcessZoom(InputAction.CallbackContext context)
    {
        float zoomInput = context.ReadValue<float>();

        while (true)
        {
            float zoomAmount = topDownCamera.m_Lens.FieldOfView + zoomInput * cameraZoomSpeed * Time.deltaTime;
            topDownCamera.m_Lens.FieldOfView = Mathf.Clamp(zoomAmount, cameraZoomMin, cameraZoomMax);

            yield return null;
        }
    }

    public IEnumerator ProcessRotate(InputAction.CallbackContext context)
    {
        float rotateInput = context.ReadValue<float>();

        while (true)
        {
            // Rotate the camera target around the Y axis
            cameraTargetTransform.Rotate(Vector3.up, rotateInput * cameraRotationSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cameraTarget.transform.position + Vector3.up*3, 3f);
    }
}

public enum CameraMode
{
    TopDown,
    Focus
}
