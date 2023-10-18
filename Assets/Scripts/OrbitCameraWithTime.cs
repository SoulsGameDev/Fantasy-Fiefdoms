using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OrbitCameraWithTime : MonoBehaviour
{
    [SerializeField]
    CinemachineVirtualCamera orbitalCamera;
    CinemachineOrbitalTransposer orbitalTransposer;
    float rotationSpeed;


    Coroutine rotationCoroutine;

    private void Awake()
    {
        if(orbitalCamera == null)
        {
            orbitalCamera = GetComponent<CinemachineVirtualCamera>();
        }

        if(orbitalCamera == null)
        {
            Debug.LogError("No orbital camera found");
        }

        orbitalTransposer = orbitalCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();

        if(orbitalTransposer == null)
        {
            Debug.LogError("No orbital transposer found");
        }

        rotationSpeed = orbitalTransposer.m_XAxis.m_MaxSpeed;

        CameraController.Instance.onCameraChanged += OnCameraChange;
    }

    private void OnCameraChange(CinemachineVirtualCamera camera)
    {
        if(camera != orbitalCamera)
        {
            return;
        }
        CinemachineOrbitalTransposer newTransposer = camera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        if(orbitalTransposer == newTransposer)
        {
            if(rotationCoroutine != null)
            {
                Debug.Log("Stop rotation");
                StopCoroutine(rotationCoroutine);
            }
            Debug.Log("Start rotation");
            rotationCoroutine = StartCoroutine(Rotate());
        }
        else
        {
            Debug.Log("Stop rotation");
            StopCoroutine(rotationCoroutine);
        }
    }

    public IEnumerator Rotate()
    {
        while (true)
        {
            orbitalTransposer.m_XAxis.Value +=  rotationSpeed * Time.deltaTime;
            yield return null;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OrbitCameraWithTime))]
    public class OrbitCameraEditor : Editor
    {
        private float prevMaxSpeed;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            OrbitCameraWithTime orbitCameraScript = (OrbitCameraWithTime)target;

            if (Application.isPlaying)
            {
                // Check if the maxSpeed has changed
                if (orbitCameraScript.orbitalTransposer != null &&
                    orbitCameraScript.orbitalTransposer.m_XAxis.m_MaxSpeed != prevMaxSpeed)
                {
                    orbitCameraScript.rotationSpeed = orbitCameraScript.orbitalTransposer.m_XAxis.m_MaxSpeed;
                    prevMaxSpeed = orbitCameraScript.orbitalTransposer.m_XAxis.m_MaxSpeed;
                }
            }
        }
    }
#endif
}
