using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveBeamGen : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Transform pointsParent;

    [SerializeField]
    private float width = 1f;

    private MeshFilter filter;
    private new MeshRenderer renderer;
    private Vector3[] vertices;
    private int[] triangles;

    private bool initilized = false;

    #endregion

    #region Properties
    
    #endregion

    private void Start()
    {
        if (pointsParent.childCount > 1)
        {
            filter = GetComponentInChildren<MeshFilter>();
            renderer = GetComponentInChildren<MeshRenderer>();
            filter.mesh = new Mesh();
            filter.mesh.MarkDynamic();
            vertices = new Vector3[(pointsParent.childCount - 1) * 2 + 1];
            triangles = new int[(pointsParent.childCount - 1) * 3];
            initilized = true;
        }
        
    }

    private void Update()
    {
        if (initilized)
        {
            GenerateTriangles();
        }
    }

    private void GenerateTriangles()
    {
        Vector3 directionToNext;
        Vector3 right = Camera.main.transform.right;
        vertices[0] = pointsParent.GetChild(0).localPosition - right * 0.5f * width;

        for (int i = 0; i < pointsParent.childCount - 1; i++)
        {
            int startI = i * 2;
            directionToNext = pointsParent.GetChild(i + 1).localPosition - pointsParent.GetChild(i).localPosition;
            vertices[startI + 1] = vertices[startI] + right * width;
            vertices[startI + 2] = vertices[startI] + directionToNext;
        }

        for (int i = 0, j = 0; i < triangles.Length; i += 3, j++)
        {
            triangles[i] = j * 2;
            triangles[i + 1] = j * 2 + 1;
            triangles[i + 2] = j * 2 + 2;
        }

        filter.mesh.Clear();
        filter.mesh.vertices = vertices;
        filter.mesh.triangles = triangles;
        filter.mesh.RecalculateNormals();
        filter.mesh.RecalculateBounds();
    }
}
