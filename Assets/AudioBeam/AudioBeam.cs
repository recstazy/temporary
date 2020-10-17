using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioBeam : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private AudioClip pattern;

    [SerializeField]
    private LineRenderer lineRenderer;

    [SerializeField]
    private Vector2 amplitude = Vector2.right;

    [SerializeField]
    private int sampleLength = 128;

    [SerializeField]
    private int pointsPerSegment = 20;

    [SerializeField]
    private float speed = 0.5f;

    [SerializeField]
    [Range(0f, 1f)]
    private float xyPhase = 0f;

    [SerializeField]
    [Range(0f, 1f)]
    private float segmentsOffset = 0f;

    private float startT = 0f;
    private int lastPointsPerSegment = -1;
    private Transform[] positions;
    private int pointsCount = 0;
    private float[] audioData;
    private AudioClip lastPattern;

    #endregion

    private void Update()
    {
        if (pattern != lastPattern)
        {
            InitializeAudioData();
        }

        bool positionsChanged = false;

        if (positions == null || transform.childCount != positions.Length)
        {
            InitializePositions();
            positionsChanged = true;
        }

        pointsPerSegment = Mathf.Max(pointsPerSegment, 2);
        sampleLength = Mathf.Max(sampleLength, 2);

        pointsCount = pointsPerSegment * (positions.Length - 1);

        if (lastPointsPerSegment != pointsPerSegment || positionsChanged)
        {
            lineRenderer.positionCount = pointsCount;
            lastPointsPerSegment = pointsPerSegment;
        }

        RenderBetweenPositions();

        startT += speed * Time.deltaTime;

        if (startT >= 1f)
        {
            startT -= 1f;
        }
    }

    private void InitializeAudioData()
    {
        audioData = new float[(int)(pattern.length * 2 * pattern.frequency)];
        pattern.GetData(audioData, 0);
        lastPattern = pattern;
    }

    private void InitializePositions()
    {
        positions = new Transform[transform.childCount];

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.GetChild(i);
        }
    }

    private void RenderBetweenPositions()
    {
        if (audioData == null || audioData.Length == 0)
        {
            return;
        }

        Vector3[] points = new Vector3[pointsCount];

        for (int i = 0; i < positions.Length - 1; i++)
        {
            for (int j = 0; j < pointsPerSegment; j++)
            {
                int index = i * pointsPerSegment + j;
                Vector3 offset = Vector3.zero;

                float indexOffset = index * ((float)sampleLength / audioData.Length) / pointsPerSegment;
                float t = (startT + i * segmentsOffset) + indexOffset;

                offset += transform.right * amplitude.x * GetAudioSample(t);
                offset += transform.forward * amplitude.y * GetAudioSample(t + xyPhase);

                float offsetWeight = Mathf.Abs((j - (pointsPerSegment - 1) * 0.5f) / ((pointsPerSegment - 1) * 0.5f));
                offsetWeight = 1f - offsetWeight * offsetWeight;
                offset *= offsetWeight;

                Vector3 direction = positions[i + 1].localPosition - positions[i].localPosition;
                Vector3 basePosition = positions[i].localPosition + direction * ((float)j / (pointsPerSegment - 1));
                points[index] = basePosition + offset;
            }
        }

        lineRenderer.SetPositions(points);
    }

    private float GetAudioSample(float normalizedT)
    {
        if (audioData == null || audioData.Length == 0)
        {
            return 0f;
        }

        int index = (int)(normalizedT * (audioData.Length - 1));
        return audioData[GetindexLooped(index)];
    }

    private int GetindexLooped(int index)
    {
        if (index >= audioData.Length)
        {
            index %= audioData.Length;
        }

        return index;
    }
}
