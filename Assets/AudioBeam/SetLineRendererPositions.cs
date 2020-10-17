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

    [SerializeField]
    private AnimationCurve curve = SineCurve();

    private float[] curveValues = new float[1024];

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

    public static AnimationCurve SineCurve()
    {
        var curve = new AnimationCurve();
        var keys = new Keyframe[5];

        keys[0] = new Keyframe(0f, 0f, 5f, 5f);
        keys[1] = new Keyframe(0.25f, 1f, 0f, 0f);
        keys[2] = new Keyframe(0.5f, 0f, -5f, -5f);
        keys[3] = new Keyframe(0.75f, -1f, 0f, 0f);
        keys[4] = new Keyframe(1f, 0f, 5f, 5f);
        curve.keys = keys;

        curve.preWrapMode = WrapMode.Loop;
        curve.postWrapMode = WrapMode.Loop;

        return curve;
    }

    private float Evaluate(float t, Keyframe keyframe0, Keyframe keyframe1)
    {
        float dt = keyframe1.time - keyframe0.time;

        float m0 = keyframe0.outTangent * dt;
        float m1 = keyframe1.inTangent * dt;

        float t2 = t * t;
        float t3 = t2 * t;

        float a = 2 * t3 - 3 * t2 + 1;
        float b = t3 - 2 * t2 + t;
        float c = t3 - t2;
        float d = -2 * t3 + 3 * t2;

        return a * keyframe0.value + b * m0 + c * m1 + d * keyframe1.value;
    }
}
