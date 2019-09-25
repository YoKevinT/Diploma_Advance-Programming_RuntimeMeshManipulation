using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class MeshStudy : MonoBehaviour
{
    Mesh oMesh;
    Mesh cMesh;
    MeshFilter oMeshFilter;
    int[] triangles;

    [HideInInspector]
    public Vector3[] vertices;

    [HideInInspector]
    public bool isCloned = false;

    // For Editor
    public float radius = 0.2f;
    public float pull = 0.3f;
    public float handleSize = 0.03f;
    public List<int>[] connectedVertices;
    public List<Vector3[]> allTriangleList;
    public bool moveVertexPoint = true;

    void Start()
    {
        InitMesh();
    }

    public void InitMesh()
    {
        oMeshFilter = GetComponent<MeshFilter>();
        oMesh = oMeshFilter.sharedMesh; //Grabs original mesh oMesh from the MeshFilter component

        cMesh = new Mesh(); //Copies to a new mesh instance cMesh
        cMesh.name = "clone";
        cMesh.vertices = oMesh.vertices;
        cMesh.triangles = oMesh.triangles;
        cMesh.normals = oMesh.normals;
        cMesh.uv = oMesh.uv;
        oMeshFilter.mesh = cMesh;  //Assigns the copied mesh back to the mesh filter

        vertices = cMesh.vertices; //Updates local variables
        triangles = cMesh.triangles;
        isCloned = true;
        Debug.Log("Init & Cloned");
    }

    public void Reset()
    {
        if (cMesh != null && oMesh != null) //Checks that both original and clone mesh exists
        {
            cMesh.vertices = oMesh.vertices; //Resets cMesh to the original mesh
            cMesh.triangles = oMesh.triangles;
            cMesh.normals = oMesh.normals;
            cMesh.uv = oMesh.uv;
            oMeshFilter.mesh = cMesh; //Assigns cMesh back to oMeshFilter

            vertices = cMesh.vertices; //Updates local variables
            triangles = cMesh.triangles;
        }
    }

    public void GetConnectedVertices()
    {
        connectedVertices = new List<int>[vertices.Length];
    }

    public void DoAction(int index, Vector3 localPos)
    {
        PullSimilarVertices(index, localPos);
    }

    // returns List of int that is related to the targetPt.
    private List<int> FindRelatedVertices(Vector3 targetPt, bool findConnected)
    {
        // list of int
        List<int> relatedVertices = new List<int>();

        int idx = 0;
        Vector3 pos;

        // loop through triangle array of indices
        for (int t = 0; t < triangles.Length; t++)
        {
            // current idx return from tris
            idx = triangles[t];
            // current pos of the vertex
            pos = vertices[idx];
            // if current pos is same as targetPt
            if (pos == targetPt)
            {
                // add to list
                relatedVertices.Add(idx);
                // if find connected vertices
                if (findConnected)
                {
                    // min
                    // - prevent running out of count
                    if (t == 0)
                    {
                        relatedVertices.Add(triangles[t + 1]);
                    }
                    // max 
                    // - prevent runnign out of count
                    if (t == triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                    }
                    // between 1 ~ max-1 
                    // - add idx from triangles before t and after t 
                    if (t > 0 && t < triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                        relatedVertices.Add(triangles[t + 1]);
                    }
                }
            }
        }
        // return compiled list of int
        return relatedVertices;
    }

    public void BuildTriangleList()
    {
    }

    public void ShowTriangle(int idx)
    {
    }

    // Pulling only one vertex pt, results in broken mesh.
    private void PullOneVertex(int index, Vector3 newPos)
    {
    }

    private void PullSimilarVertices(int index, Vector3 newPos)
    {
        Vector3 targetVertexPos = vertices[index]; //Gets the target vertex position, to be used as an argument to the FindRelatedVertices() method
        List<int> relatedVertices = FindRelatedVertices(targetVertexPos, false); //This method returns a list of indices (that correspond to vertices) that share the same position as the target vertex
        foreach (int i in relatedVertices) //Loops through that list and update the related vertices with newPos
        {
            vertices[i] = newPos;
        }
        cMesh.vertices = vertices; //Assigns the updated vertices back to cMesh.vertices. Then RecalculateNormals() to re-draw the mesh with the new values
        cMesh.RecalculateNormals();
    }

    // To test Reset function
    public void EditMesh()
    {
        vertices[2] = new Vector3(2, 3, 4);
        vertices[3] = new Vector3(1, 2, 4);
        cMesh.vertices = vertices;
        cMesh.RecalculateNormals();
    }


}
