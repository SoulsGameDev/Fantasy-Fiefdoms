using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject cameraTarget;
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField] private float cameraSpeed = 10f;
    [SerializeField] private float cameraZoomSpeed = 1f;
    [SerializeField] private float cameraZoomMin = 5f;
    [SerializeField] private float cameraZoomMax = 120f;
    [SerializeField] private float cameraZoomDefault = 40f;


    private Coroutine panCoroutine;
    private Coroutine zoomCoroutine;

    private void Start()
    {
        virtualCamera.m_Lens.FieldOfView = cameraZoomDefault;
    }


    public void OnPanChange(InputAction.CallbackContext context)
    {
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
    }

    public void OnZoomChanged(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("Pressed Zoom key");
        }
        if (context.performed)
        {
            Debug.Log("Zooming: " + context.ReadValue<float>());
            float zoomInput = context.ReadValue<float>();
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
            Debug.Log("Focus button pressed... What's it gonna be?");
        }
        else if (context.performed)
        {
            Debug.Log("Double tapped - Focus");
        }
        else if (context.canceled)
        {
            Debug.Log("Single tap - Select");
        }
    }



    public IEnumerator ProcessPan(InputAction.CallbackContext context)
    {
        while (true)
        {
            //Move the camera target in the direction of the input (2D Vector)
            Vector2 inputVector = context.ReadValue<Vector2>();
            Debug.Log("Moving: " + inputVector);

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

        Debug.Log("Zooming: " + zoomInput);
        while (true)
        {
            //Change the FOV of the camera based on the input. If not keyboard, then adjust the value based on the scrollWheelZoomSpeed
            float zoomAmount = virtualCamera.m_Lens.FieldOfView + zoomInput * cameraZoomSpeed * Time.deltaTime;
            virtualCamera.m_Lens.FieldOfView = Mathf.Clamp(zoomAmount, cameraZoomMin, cameraZoomMax);

            yield return null;
        }
    }

}
