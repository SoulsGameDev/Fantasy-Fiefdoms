using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class HexTerrain : MonoBehaviour
{
    public event Action OnMouseEnterAction;
    public event Action OnMouseExitAction;

    private Collider parentCollider;

    private void Start()
    {
        parentCollider = GetComponent<Collider>();

        // Disable collisions between the parent collider and all child colliders
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider childCollider in childColliders)
        {
            childCollider.enabled = false;
        }
        parentCollider.enabled = true;
    }

    private void OnMouseEnter()
    {
        Debug.Log("Mouse enter");
        OnMouseEnterAction?.Invoke();
    }

    private void OnMouseExit()
    {
        Debug.Log("Mouse exit");
        OnMouseExitAction?.Invoke();
    }
}
