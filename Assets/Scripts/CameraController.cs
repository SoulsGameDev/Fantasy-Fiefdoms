using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

public class CameraController : Singleton<CameraController>
{
    [SerializeField]
    private const int DEFAULT_PRIORITY = 10;

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

    [SerializeField] private float cameraSpeed = 10f;
    [SerializeField] private float cameraZoomSpeed = 1f;
    [SerializeField] private float cameraZoomMin = 15f;
    [SerializeField] private float cameraZoomMax = 100f;
    [SerializeField] private float cameraZoomDefault = 50f;
    
    [SerializeField] private bool isLocked = false;


    private Coroutine panCoroutine;
    private Coroutine zoomCoroutine;

    public event Action<CinemachineVirtualCamera> onCameraChanged;
    public event Action onSelectAction;
    public event Action onDeselectAction;
    public event Action onFocusAction;

    public bool IsLocked { get => isLocked; set => isLocked = value; }
    public GameObject CameraTarget { get => cameraTarget; }

    void Start()
    {
        topDownCamera.m_Lens.FieldOfView = cameraZoomDefault;
        ChangeCamera(defaultMode);
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


    public void OnPanChange(InputAction.CallbackContext context)
    {
        if(isLocked)
        {
            return;
        }
        if (context.performed)
        {
            if(panCoroutine != null)
            {
                StopCoroutine(panCoroutine);
            }
            panCoroutine = StartCoroutine(ProcessPan(context));
        }
        else if (context.canceled)
        {
            if(panCoroutine != null)
            {
                StopCoroutine(panCoroutine);
            }
        }
        //ChangeCamera(CameraMode.TopDown);
    }

    public void OnZoomChanged(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //Debug.Log("Pressed Zoom key");
        }
        if (context.performed)
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
            zoomCoroutine = StartCoroutine(ProcessZoom(context));
        }
        else if (context.canceled)
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
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
            //ChangeCamera(CameraMode.Focus);
            onFocusAction?.Invoke();
        }
        else if (context.canceled)
        {
            //Debug.Log("Single tap - Select");
            onSelectAction?.Invoke();
        }
    }

    public void OnDeselectAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("Double tapped - Deselect");
            onDeselectAction?.Invoke();
        }
    }


    public IEnumerator ProcessPan(InputAction.CallbackContext context)
    {
        while (true)
        {
            //Move the camera target in the direction of the input (2D Vector)
            Vector2 inputVector = context.ReadValue<Vector2>();
            //Debug.Log("Moving: " + inputVector);

            //Move the camera target in the direction of the input (2D Vector)
            Vector3 moveVector = new Vector3(inputVector.x, 0, inputVector.y);
            cameraTarget.transform.position += moveVector * cameraSpeed * Time.deltaTime;

            yield return null;
        }
    }

    public IEnumerator ProcessZoom(InputAction.CallbackContext context)
    {
        //Change the FOV of the camera based on the input. If not keyboard, then adjust the value based on the scrollWheelZoomSpeed
        float zoomInput = context.ReadValue<float>();

        //Debug.Log("Zooming: " + zoomInput);
        while (true)
        {
            //Change the FOV of the camera based on the input. If not keyboard, then adjust the value based on the scrollWheelZoomSpeed
            float zoomAmount = topDownCamera.m_Lens.FieldOfView + zoomInput * cameraZoomSpeed * Time.deltaTime;
            topDownCamera.m_Lens.FieldOfView = Mathf.Clamp(zoomAmount, cameraZoomMin, cameraZoomMax);

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
