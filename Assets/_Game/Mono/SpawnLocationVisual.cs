using System;
using UnityEngine;

#if UNITY_EDITOR

public class SpawnLocationVisual : MonoBehaviour
{
    public float radius = 5.0f; // Radius to draw around each child

    // Draw Gizmos in the editor
    void OnDrawGizmos()
    {
        foreach (Transform child in transform)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(child.position, radius);
        }
    }
}
#endif