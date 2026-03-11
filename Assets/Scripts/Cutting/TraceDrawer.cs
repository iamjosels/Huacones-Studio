using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TraceDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();

    public float minDistance = 0.05f; // distancia m�nima entre puntos

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // profundidad para la c�mara

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);

            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPoint) > minDistance)
            {
                points.Add(worldPoint);
                lineRenderer.positionCount = points.Count;
                lineRenderer.SetPositions(points.ToArray());
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            points.Clear();
            lineRenderer.positionCount = 0;
        }
    }
}