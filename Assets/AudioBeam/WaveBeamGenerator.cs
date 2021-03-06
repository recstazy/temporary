﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// GPU driven beam generator
/// </summary>
public class WaveBeamGenerator : MonoBehaviour
{
    public enum UVMode 
    {
        /// <summary>
        /// Repeat uv per segment
        /// </summary>
        PerSegment,

        /// <summary>
        /// Stretch uv along whole beam
        /// </summary>
        Stretch
    }

    #region Fields

    [SerializeField]
    [Tooltip("Repeat uv per segment or stretch along whole beam")]
    private UVMode uvMode;

    [SerializeField]
    [Tooltip("Enabling mades beam to not rotate axes between points but it will become flat when alignet to local XZ plane")]
    private bool avoidRotation;

    [SerializeField]
    private List<Transform> points;

    private GameObject rendererObject;
    private MeshFilter filter;
    private new MeshRenderer renderer;

    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    // Used to provide node information to shader
    private Vector2[] uv1;

    private bool enoughPoints = false;
    private int pointsCount = 0;
    private UVMode currentUVMode;

    #endregion

    #region Properties

    /// <summary>
    /// Add or remove points between which beam is rendered
    /// </summary>
    public List<Transform> Points => points;

    /// <summary>
    /// Set uv wrapping mode
    /// </summary>
    public UVMode CurrentUVMode { get => uvMode; set => uvMode = value; }

    public MeshRenderer BeamRenderer => renderer;
    public MeshFilter BeamMeshFilter => filter;

    #endregion

    private void Awake()
    {
        CreateRenederer();
    }

    private void Update()
    {
        if (pointsCount != points.Count)
        {
            RegenerateAll();
        }

        if (enoughPoints)
        {
            bool uvModeChanged = uvMode != currentUVMode;

            if (uvModeChanged)
            {
                currentUVMode = uvMode;
            }

            UpdateVertices();
            UpdateUV(uvModeChanged, false);
            filter.sharedMesh.RecalculateBounds();
        }
    }

    private void OnDrawGizmos()
    {
        if (points.Count > 1)
        {
            var lastColor = Gizmos.color;
            Gizmos.color = new Color(1f, 0f, 0.5f, 0.5f);

            for (int i = 0; i < points.Count - 1; i++)
            {
                var point = points[i];
                var next = points[i + 1];

                if (point != null && next != null)
                {
                    Gizmos.DrawLine(point.position, next.position);
                }
            }

            Gizmos.color = lastColor;
        }
    }

    private void UpdateVertices()
    {
        Transform startPoint;
        Transform next;
        Vector3 directionToNext;
        Vector3 right = transform.right * 0.1f;

        for (int i = 0; i < pointsCount - 1; i++)
        {
            int startI = i * 3;

            startPoint = points[i];
            next = points[i + 1];

            if (startPoint == null || next == null)
            {
                return;
            }

            Vector3 startPosition = startPoint.localPosition;
            directionToNext = next.localPosition - startPosition;

            if (!avoidRotation)
            {
                float dot = Mathf.Abs(Vector3.Dot(directionToNext.normalized, transform.up));
                Vector3 upFwdBlended = Vector3.Lerp(transform.up, transform.forward, dot);
                right = Vector3.Lerp(upFwdBlended, right, dot).normalized * 0.1f;
            }

            vertices[startI] = startPosition - right;
            vertices[startI + 1] = startPosition + right;
            vertices[startI + 2] = startPosition + directionToNext;
        }

        filter.sharedMesh.vertices = vertices;
    }

    private void GenerateTriangles()
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (points[i] == null)
            {
                continue;
            }

            triangles[i * 3] = i * 3;
            triangles[i * 3 + 1] = i * 3 + 1;
            triangles[i * 3 + 2] = i * 3 + 2;
        }

        filter.sharedMesh.triangles = triangles;
    }

    private void RegenerateAll()
    {
        pointsCount = points.Count;
        enoughPoints = pointsCount > 1;
        filter.sharedMesh.Clear();

        if (enoughPoints)
        {
            vertices = new Vector3[(pointsCount - 1) * 3];
            triangles = new int[(pointsCount - 1) * 3];
            uv = new Vector2[vertices.Length];
            uv1 = new Vector2[vertices.Length];

            UpdateVertices();
            UpdateUV(true, true);
            GenerateTriangles();
        }
        else
        {
            vertices = null;
            triangles = null;
            uv = null;
            uv1 = null;
        }
    }

    private void CreateRenederer()
    {
        rendererObject = new GameObject("WaveBeamRenderer");
        rendererObject.transform.SetParent(transform);
        rendererObject.transform.localPosition = Vector3.zero;
        rendererObject.transform.localRotation = Quaternion.identity;

        filter = rendererObject.AddComponent<MeshFilter>();
        renderer = rendererObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Resources.Load<Material>("BeamProceduralGen");
        filter.sharedMesh = new Mesh();
        filter.sharedMesh.name = "WaveBeamGenerated";
        filter.sharedMesh.MarkDynamic();
    }

    private void UpdateUV(bool updateUV, bool updateUV1)
    {
        for (int i = 0; i < pointsCount - 1; i++)
        {
            int startI = i * 3;

            if (updateUV)
            {
                if (uvMode == UVMode.PerSegment)
                {
                    FillUV(ref uv, i, startI, true);
                }
                else
                {
                    FillUV(ref uv, i, startI, false);
                }
            }

            if (updateUV1)
            {
                FillUV(ref uv1, i, startI, true);
            }
        }

        if (updateUV)
        {
            filter.sharedMesh.uv = uv;
        }

        if (updateUV1)
        {
            filter.sharedMesh.uv2 = uv1;
        }
    }

    private void FillUV(ref Vector2[] uv, int i, int startI, bool perSegment)
    {
        if (perSegment)
        {
            uv[startI] = Vector2.zero;
            uv[startI + 1] = Vector2.up;
            uv[startI + 2] = new Vector2(1f, 0.5f);
        }
        else
        {
            uv[startI] = new Vector2((float)i / (pointsCount - 1), 0f);
            uv[startI + 1] = new Vector2((float)i / (pointsCount - 1), 1f);
            uv[startI + 2] = new Vector2((float)(i + 1) / (pointsCount - 1), 1f);
        }
    }
}
