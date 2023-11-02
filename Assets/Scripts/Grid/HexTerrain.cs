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
    private void OnMouseEnter()
    {
        OnMouseEnterAction?.Invoke();
    }

    private void OnMouseExit()
    {
        OnMouseExitAction?.Invoke();
    }
}
