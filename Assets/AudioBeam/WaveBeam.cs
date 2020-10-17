using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaveBeam: MonoBehaviour
{
    #region Fields

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private LineRenderer lineRenderer;

    [SerializeField]
    private AudioClip pattern;

    [SerializeField]
    private Vector2 amplitude = Vector2.right;

    [SerializeField]
    [Range(2, 2048)]
    private int sampleLength = 1024;

    [SerializeField]
    [Range(2, 100)]
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

    private Mesh mesh;
    private Material material;
    private int[] trianglePattern = new int[] { 0, 1, 0 };

    private int[] triangulationPattern = new int[] { 2, 3, 0, 1, 0, 3 };
    private Vector3[] subdivided;

    #endregion

    private void Awake()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        meshFilter.mesh = mesh;
        material = meshFilter.GetComponent<MeshRenderer>().material;

        InitializeAudioData();
        InitializePositions();

        CreateSubdividedLine();
    }

    private void Update()
    {
        return;
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
            //lineRenderer.positionCount = pointsCount;
            lastPointsPerSegment = pointsPerSegment;
        }

        RenderBetweenPositions();

        startT += speed * Time.deltaTime;

        if (startT >= 1f)
        {
            startT -= 1f;
        }
    }


    private void CreateSubdividedLine()
    {
        if (audioData == null || audioData.Length == 0)
        {
            return;
        }

        pointsCount = pointsPerSegment * (positions.Length - 1);

        Vector2[] uvs0 = new Vector2[pointsCount];
        // x is index offset
        // y is offsetWeight

        Vector3[] points = new Vector3[pointsCount];
        Vector3[] localPositions = new Vector3[positions.Length];

        for (int i = 0; i < positions.Length - 1; i++)
        {
            localPositions[i] = positions[i].localPosition;

            for (int j = 0; j < pointsPerSegment; j++)
            {
                int index = i * pointsPerSegment + j;

                float indexOffset = index * ((float)sampleLength / audioData.Length) / pointsPerSegment;
                float offsetWeight = Mathf.Abs((j - (pointsPerSegment - 1) * 0.5f) / ((pointsPerSegment - 1) * 0.5f));
                offsetWeight = 1f - offsetWeight * offsetWeight;

                Vector2 uv0 = new Vector2();
                uv0.x = i * segmentsOffset + indexOffset;
                uv0.y = offsetWeight;

                uvs0[index] = uv0;

                Vector3 direction = positions[i + 1].localPosition - positions[i].localPosition;
                Vector3 basePosition = positions[i].localPosition + direction * ((float)j / (pointsPerSegment - 1));
                points[index] = basePosition;
            }
        }

        CreateMesh(points, uvs0);
    }

    private void CreateMesh(Vector3[] path, Vector2[] uv0)
    {
        mesh.Clear();

        Vector3[] vertices = new Vector3[path.Length * 2];

        var direction = path[1] - path[0];
        var right = Vector3.Cross(Vector3.forward, direction.normalized);

        vertices[0] = path[0] - right;
        vertices[1] = path[0] + right;

        for (int i = 1; i < path.Length; i++)
        {
            direction = path[i] - path[i - 1];
            right = Vector3.Cross(Vector3.forward, direction.normalized);

            vertices[i * 2] = path[i] - right;
            vertices[i * 2 + 1] = path[i] + right;
        }

        mesh.vertices = vertices;
        mesh.uv = CreateUV(uv0);
        var tris = Triangulate(path.Length);
        mesh.triangles = tris;
    }

    private Vector2[] CreateUV(Vector2[] pathUV)
    {
        var result = new Vector2[pathUV.Length * 2];

        for (int i = 0; i < pathUV.Length; i++)
        {
            result[i * 2] = pathUV[i];
            result[i * 2 + 1] = pathUV[i];
        }

        return result;
    }

    private int[] Triangulate(int pathLength)
    {
        int[] triangles = new int[(pathLength - 1) * 6];

        for (int i = 0; i < pathLength - 1; i++)
        {
            for (int n = 0; n < 6; n++)
            {
                triangles[i * 6 + n] = i * 2 + triangulationPattern[n];
            }
        }

        return triangles;
    }

    private void SetPositions(Vector3[] localPositions)
    {
        mesh.Clear();
        mesh.vertices = localPositions;

        var colors = new Color[localPositions.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color((float)i / colors.Length, 0f, 0f, 1f);
        }

        mesh.colors = colors;

        int[] tris = new int[localPositions.Length * 3];

        for (int i = 0; i < localPositions.Length - 1; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                tris[3 * i + j] = i + trianglePattern[j];
            }
        }

        mesh.triangles = tris;
    }

    private void InitializeAudioData()
    {
        audioData = new float[(int)(pattern.length * 2 * pattern.frequency)];
        pattern.GetData(audioData, 0);
        lastPattern = pattern;
        material.SetFloatArray("_Samples", audioData);
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
        Vector3[] localPositions = new Vector3[positions.Length];

        for (int i = 0; i < positions.Length - 1; i++)
        {
            localPositions[i] = positions[i].localPosition;

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

        localPositions[localPositions.Length - 1] = positions[positions.Length - 1].localPosition;

        //lineRenderer.SetPositions(points);
        SetPositions(points);
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
