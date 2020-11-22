using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveBeamGen : MonoBehaviour
{
    #region Fields

    [SerializeField]
    [HideInInspector]
    private Transform pointsParent;

    [SerializeField]
    [HideInInspector]
    private GameObject rendererObject;

    [SerializeField]
    [HideInInspector]
    private MeshFilter filter;

    [SerializeField]
    [HideInInspector]
    private new MeshRenderer renderer;

    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    private bool enoughPoints = false;
    private int pointsCount = 0;

    #endregion

    #region Properties

    public List<Transform> Pivots { get; set; } = new List<Transform>();

    #endregion

    private void Update()
    {
        if (pointsParent == null)
        {
            CreatePivots();
        }

        if (rendererObject == null)
        {
            CreateRenederer();
        }

        if (pointsCount != pointsParent.childCount)
        {
            RegenerateAll();
        }

        if (enoughPoints)
        {
            UpdateVertices();
        }
    }

    private void UpdateVertices()
    {
        Transform startPoint = pointsParent.GetChild(0);
        Vector3 distanceToCamera = startPoint.InverseTransformPoint(Camera.main.transform.position) - startPoint.localPosition;
        Vector3 directionToNext = pointsParent.GetChild(1).localPosition - startPoint.localPosition;

        //Vector3 right = Vector3.Cross(distanceToCamera.normalized, directionToNext.normalized);
        Vector3 right = transform.right;
        //vertices[0] = pointsParent.GetChild(0).localPosition - right * 0.5f;

        for (int i = 0; i < pointsCount - 1; i++)
        {
            int startI = i * 3;

            startPoint = pointsParent.GetChild(i);
            Vector3 startPosition = startPoint.localPosition;

            directionToNext = pointsParent.GetChild(i + 1).localPosition - startPosition;
            distanceToCamera = startPoint.InverseTransformPoint(Camera.main.transform.position) - startPoint.localPosition;

            //right = Vector3.Cross(distanceToCamera.normalized, directionToNext.normalized);
            right = transform.right;
            vertices[startI] = startPosition - right;
            vertices[startI + 1] = startPosition + right;
            vertices[startI + 2] = startPosition + directionToNext;
        }

        filter.sharedMesh.vertices = vertices;
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

            UpdateVertices();
            GenerateTriangles();
        }
        else
        {
            vertices = null;
            triangles = null;
            uv = null;
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
}
