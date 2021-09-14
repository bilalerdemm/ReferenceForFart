using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    [HideInInspector] public List<Vector3> points;
    public int lineRendererLength;

    public void Update()
    {
        AddPosToList();
        DrawLines();
    }
    public void AddPosToList()
    {
        points.Add(transform.position);
        if (lineRenderer.positionCount >= lineRendererLength)
        {
            points.RemoveAt(0);
        }
        lineRenderer.positionCount = points.Count;
    }
    public void DrawLines()
    {
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
    }

}