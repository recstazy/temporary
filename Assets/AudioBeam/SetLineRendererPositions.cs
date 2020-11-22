using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class SetLineRendererPositions : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    [SerializeField]
    private int pointsPerSegment;
    private Transform[] positions;
    private Vector3[] lastPositions;

    private void OnTransformChildrenChanged()
    {
        Execute();
    }

    private void Update()
    {
        if (positions != null)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                if (i < lastPositions.Length)
                {
                    if (positions[i].localPosition != lastPositions[i])
                    {
                        Execute();
                    }
                }

            }
        }
        else
        {
            Execute();
        }
    }

    [ContextMenu("Execute")]
    private void Execute()
    {
        InitPositions();
    }

    private void InitPositions()
    {
        positions = new Transform[transform.childCount];

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.GetChild(i);
        }

        lastPositions = positions.Select(p => p.localPosition).ToArray();

        SubdivideAndSet(positions);
    }

    private void SubdivideAndSet(Transform[] positions)
    {
        var pointsCount = pointsPerSegment * (positions.Length - 1);
        Vector3[] points = new Vector3[pointsCount];

        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 direction = positions[i + 1].localPosition - positions[i].localPosition;

            for (int j = 0; j < pointsPerSegment; j++)
            {
                int index = i * pointsPerSegment + j;

                Vector3 basePosition = positions[i].localPosition + direction * ((float)j / (pointsPerSegment));
                points[index] = basePosition;
            }
        }

        lineRenderer.positionCount = pointsCount;
        lineRenderer.SetPositions(points);

#if UNITY_EDITOR

        UnityEditor.EditorUtility.SetDirty(lineRenderer);

#endif
    }
}
