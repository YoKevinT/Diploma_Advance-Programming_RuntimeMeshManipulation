using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeartMesh : MonoBehaviour
{
    Mesh oMesh;
    Mesh cMesh;
    MeshFilter oFilter;

    [HideInInspector]
    public int targetIndex;

    [HideInInspector]
    public Vector3 targetVertex;

    [HideInInspector]
    public Vector3[] oVertices;

    [HideInInspector]
    public Vector3[] mVertices;

    [HideInInspector]
    public Vector3[] normals;

    [HideInInspector]
    public bool isMeshReady = false;
    public bool isEditMode = true;
    public bool showTransformHandle = true;
    public List<int> selectedIndices = new List<int>();
    public float pickSize = 0.01f;

    public float radiusofeffect = 0.3f; //Radius of area affected by the targeted vertex
    public float pullvalue = 0.3f; //The strength of the pull
    public float duration = 1.2f; //How long the animation will run
    int currentIndex = 0; //Current index of the selectedIndices list
    bool isAnimate = false;
    float starttime = 0f;
    float runtime = 0f;


    void Start()
    {
        Init();
    }

    public void Init()
    {
        oFilter = GetComponent<MeshFilter>();
        isMeshReady = false;
        currentIndex = 0;

        if (isEditMode)
        {
            oMesh = oFilter.sharedMesh;
            cMesh = new Mesh();
            cMesh.name = "clone";
            cMesh.vertices = oMesh.vertices;
            cMesh.triangles = oMesh.triangles;
            cMesh.normals = oMesh.normals;
            oFilter.mesh = cMesh;

            oVertices = cMesh.vertices;
            normals = cMesh.normals;
            Debug.Log("Init & Cloned");
        }
        else
        {
            oMesh = oFilter.mesh;
            oVertices = oMesh.vertices;
            normals = oMesh.normals;
            mVertices = new Vector3[oVertices.Length];
            for (int i = 0; i < oVertices.Length; i++)
            {
                mVertices[i] = oVertices[i];
            }
            StartDisplacement();
        }

    }

    public void StartDisplacement()
    {
        targetVertex = oVertices[selectedIndices[currentIndex]]; //Single out the targetVertex to start the animation
        starttime = Time.time; //Set the start time and change isAnimate to true
        isAnimate = true;
    }

    void FixedUpdate()
    {
        if (!isAnimate)
        {
            return;
        }

        runtime = Time.time - starttime; //Updates the runtime of the animation

        //If runtime is within the duration limit, get the world space coordinates of targetVertex and DisplaceVertices() surrounding the target vertex with the pullvalue and radiusofeffect as params
        if (runtime < duration)
        {
            Vector3 targetVertexPos = oFilter.transform.InverseTransformPoint(targetVertex);
            DisplaceVertices(targetVertexPos, pullvalue, radiusofeffect);
        }
        else //Otherwise, time is up. Add one to currentIndex
        {
            currentIndex++;
            //Checks if currentIndex is within the number of selectedIndices. Move on to the next vertex in the list with StartDisplacement()
            if (currentIndex < selectedIndices.Count)
            {
                StartDisplacement();
            }
            //Otherwise, at the end of the list, update oMesh data with the current mesh and set isAnimate to false to stop the animation
            else
            {
                oMesh = GetComponent<MeshFilter>().mesh;
                isAnimate = false;
                isMeshReady = true;
            }
        }
    }

    void DisplaceVertices(Vector3 targetVertexPos, float force, float radius)
    {
        Vector3 currentVertexPos = Vector3.zero;
        float sqrRadius = radius * radius; //The square of the radius

        for (int i = 0; i < mVertices.Length; i++) //Loops through each vertex in the mesh
        {
            currentVertexPos = mVertices[i];
            float sqrMagnitute = (currentVertexPos - targetVertexPos).sqrMagnitude; //Gets sqrMagnitude between currentVertexPos and targetVertexPos
            if (sqrMagnitute > sqrRadius)
            {
                continue; //If sqrMagnitude exceeds sqrRadius, continue to the next vertex
            }
            float distance = Mathf.Sqrt(sqrMagnitute); //Otherwise, proceed on to determine the falloff value, based on the current vertex distance from the center point of area of effect
            float falloff = GaussFalloff(distance, radius);
            Vector3 translate = (currentVertexPos * force) * falloff; //Sums up the new Vector3 position and applies its Transform to the current vertex
            translate.z = 0f;
            Quaternion rotation = Quaternion.Euler(translate);
            Matrix4x4 m = Matrix4x4.TRS(translate, rotation, Vector3.one);
            mVertices[i] = m.MultiplyPoint3x4(currentVertexPos);
        }
        oMesh.vertices = mVertices; //On exiting the loop, assign the updated mVertices to oMesh data, and have Unity adjust the normals
        oMesh.RecalculateNormals();
    }

    public void ClearAllData()
    {
        // This clears the values in selectedIndices and targetIndex. It also resets targetVertex to zero
        selectedIndices = new List<int>();
        targetIndex = 0;
        targetVertex = Vector3.zero;
    }

    public Mesh SaveMesh()
    {
        Mesh nMesh = new Mesh();

        nMesh.name = "HeartMesh";
        nMesh.vertices = oMesh.vertices;
        nMesh.triangles = oMesh.triangles;
        nMesh.normals = oMesh.normals;

        return nMesh;
    }

    #region HELPER FUNCTIONS

    static float LinearFalloff(float dist, float inRadius)
    {
        return Mathf.Clamp01(0.5f + (dist / inRadius) * 0.5f);
    }

    static float GaussFalloff(float dist, float inRadius)
    {
        return Mathf.Clamp01(Mathf.Pow(360, -Mathf.Pow(dist / inRadius, 2.5f) - 0.01f));
    }

    static float NeedleFalloff(float dist, float inRadius)
    {
        return -(dist * dist) / (inRadius * inRadius) + 1.0f;
    }

    #endregion
}
