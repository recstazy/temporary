using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveBeamGen : MonoBehaviour
{
    public enum UVMode { PerSegment, WholeBeam }
    public enum PointsMode { Node, Direction }

    #region Fields

    [SerializeField]
    private UVMode uvMode;

    [SerializeField]
    private PointsMode pointsMode;

    private Transform pointsParent;
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
    private PointsMode currentPointsMode;

    #endregion

    #region Properties

    public List<Transform> Pivots { get; set; } = new List<Transform>();
    public UVMode CurrentUVMode { get => uvMode; set => uvMode = value; }
    public PointsMode CurrentPointsMode { get => pointsMode; set => pointsMode = value; }

    #endregion

    private void Awake()
    {
        CreatePivots();
        CreateRenederer();
    }

    private void Update()
    {
        if (pointsCount != pointsParent.childCount)
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

            bool pointsModeChanged = currentPointsMode != pointsMode;

            if (pointsModeChanged)
            {
                currentPointsMode = pointsMode;
            }

            UpdateVertices(uvModeChanged, pointsModeChanged);
        }
    }

    private void UpdateVertices(bool updateUV, bool updateUV1)
    {
        Transform startPoint;
        Vector3 directionToNext;

        for (int i = 0; i < pointsCount - 1; i++)
        {
            int startI = i * 3;

            startPoint = pointsParent.GetChild(i);
            Vector3 startPosition = startPoint.localPosition;

            directionToNext = pointsParent.GetChild(i + 1).localPosition - startPosition;

            vertices[startI] = startPosition - transform.right;
            vertices[startI + 1] = startPosition + transform.right;
            vertices[startI + 2] = startPosition + directionToNext;

            if (updateUV)
            {
                if (uvMode == UVMode.PerSegment)
                {
                    FillUV(ref uv, i, startI, true);
                }
                else
                {
                    FillUV(ref uv, i, startI, true);
                }
            }

            if (updateUV1)
            {
                if (pointsMode == PointsMode.Node)
                {
                    FillUV(ref uv1, i, startI, true);
                }
                else
                {
                    FillUV(ref uv1, i, startI, false);
                }
            }
        }

        filter.sharedMesh.vertices = vertices;

        if (updateUV)
        {
            filter.sharedMesh.uv = uv;
        }

        if (updateUV1)
        {
            filter.sharedMesh.uv2 = uv1;
        }
    }

    private void GenerateTriangles()
    {
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        filter.sharedMesh.triangles = triangles;
    }

    private void RegenerateAll()
    {
        pointsCount = pointsParent.childCount;
        enoughPoints = pointsCount > 1;
        filter.sharedMesh.Clear();

        if (enoughPoints)
        {
            vertices = new Vector3[(pointsCount - 1) * 3];
            triangles = new int[(pointsCount - 1) * 3];
            uv = new Vector2[vertices.Length];
            uv1 = new Vector2[vertices.Length];

            UpdateVertices(true, true);
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
        filter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one);
        filter.sharedMesh.name = "WaveBeamGenerated";
        filter.sharedMesh.MarkDynamic();
    }

    private void CreatePivots()
    {
        var pointsParentObject = new GameObject("PointsParent");
        pointsParent = pointsParentObject.transform;
        pointsParent.SetParent(transform);
        pointsParent.localPosition = Vector3.zero;
        pointsParent.localRotation = Quaternion.identity;

        var pointA = new GameObject("PointA");
        pointA.transform.SetParent(pointsParent);
        pointA.transform.localPosition = Vector3.zero;
        pointA.transform.localRotation = Quaternion.identity;
        Pivots.Add(pointA.transform);

        var pointB = new GameObject("PointB");
        pointB.transform.SetParent(pointsParent);
        pointB.transform.localPosition = Vector3.up;
        pointB.transform.localRotation = Quaternion.identity;
        Pivots.Add(pointB.transform);
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
