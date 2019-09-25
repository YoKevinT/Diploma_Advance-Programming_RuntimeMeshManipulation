using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomHeart : MonoBehaviour
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

    // For Editor
    public enum EditType
    {
        AddIndices, RemoveIndices, None
    }

    public EditType editType;

    public bool showTransformHandle = true;
    public List<int> selectedIndices = new List<int>();
    public float pickSize = 0.01f;

    // Deforming settings
    public float radiusofeffect = 0.3f;
    public float pullvalue = 0.3f;

    // Animation settings
    public float duration = 1.2f;
    bool isAnimate = false;
    float starttime = 0f;
    float runtime = 0f;
    int currentIndex = 0;

    public enum CurveType
    {
        Curve1, Curve2
    }
    public CurveType curveType;
    Curve curve;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        oFilter = GetComponent<MeshFilter>();
        currentIndex = 0;

        if (editType == EditType.AddIndices || editType == EditType.RemoveIndices)
        {
            oMesh = oFilter.sharedMesh;
            cMesh = new Mesh();
            cMesh.name = "clone";
            cMesh.vertices = oMesh.vertices;
            cMesh.triangles = oMesh.triangles;
            cMesh.normals = oMesh.normals;
            oFilter.mesh = cMesh;
            // update local vars...
            oVertices = cMesh.vertices;

            normals = cMesh.normals;
            Debug.Log("Init & Cloned");
        }
        else
        {
            oMesh = oFilter.sharedMesh;
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
        targetVertex = mVertices[selectedIndices[currentIndex]];
        starttime = Time.time;
        isAnimate = true;

        //Here, you check the curveType option the user had selected and you generate the curve accordingly
        if (curveType == CurveType.Curve1)
        {
            CurveType1();
        }
        else if (curveType == CurveType.Curve2)
        {
            CurveType2();
        }
    }

    void FixedUpdate()
    {
        if (!isAnimate)
        {
            return;
        }

        runtime = Time.time - starttime;

        if (runtime < duration)
        {
            Vector3 relativePoint = oFilter.transform.InverseTransformPoint(targetVertex);
            DisplaceVertices(relativePoint, pullvalue, radiusofeffect);
        }
        else
        {
            currentIndex++;
            if (currentIndex < selectedIndices.Count)
            {
                StartDisplacement();
                Debug.Log("next");
            }
            else
            {
                oMesh = GetComponent<MeshFilter>().sharedMesh;
                isAnimate = false;
                Debug.Log("done");
            }
        }
    }

    void DisplaceVertices(Vector3 pos, float force, float radius)
    {
        Vector3 vert = Vector3.zero;
        float sqrRadius = radius * radius;

        for (int i = 0; i < mVertices.Length; i++)
        {
            float sqrMagnitude = (mVertices[i] - pos).sqrMagnitude;
            if (sqrMagnitude > sqrRadius)
            {
                continue;
            }
            vert = mVertices[i];

            float distance = Mathf.Sqrt(sqrMagnitude);
            oMesh.vertices = mVertices;
            oMesh.RecalculateNormals();

            float increment = curve.GetPoint(distance).y * force; //Get the curve's position at the given distance and multiply its y value by force to get increment
            Vector3 translate = (vert * increment) * Time.deltaTime; //Create a new Vector3 data type to store the new position for the current vertex and apply its Transform accordingly
            Quaternion rotation = Quaternion.Euler(translate);
            Matrix4x4 m = Matrix4x4.TRS(translate, rotation, Vector3.one);
            mVertices[i] = m.MultiplyPoint3x4(mVertices[i]);
        }
    }


    public void ClearAllData()
    {
        selectedIndices = new List<int>();
        targetIndex = 0;
        targetVertex = Vector3.zero;
    }

    void CurveType1()
    {
        Vector3[] curvepoints = new Vector3[3]; //The basic curve consists of three points. Set and plot the points for the first curve
        curvepoints[0] = new Vector3(0, 1, 0);
        curvepoints[1] = new Vector3(0.5f, 0.5f, 0);
        curvepoints[2] = new Vector3(1, 0, 0);
        curve = new Curve(curvepoints[0], curvepoints[1], curvepoints[2], false); //Generate the 1st curve with Curve() and assign its values to curve. The curve drawn can be a preview if you set the last parameter to true
    }

    void CurveType2()
    {
        Vector3[] curvepoints = new Vector3[3]; //Set and plot the points for the second curve
        curvepoints[0] = new Vector3(0, 0, 0);
        curvepoints[1] = new Vector3(0.5f, 1, 0);
        curvepoints[2] = new Vector3(1, 0, 0);
        curve = new Curve(curvepoints[0], curvepoints[1], curvepoints[2], false); //Generate the second curve with Curve() and assign its values to curve. The curve drawn can be a preview if you set the last parameter to "True"
    }

    public void ShowNormals()
    {
        for (int i = 0; i < mVertices.Length; i++)
        {
            Debug.DrawLine(transform.TransformPoint(mVertices[i]), transform.TransformPoint(normals[i]), Color.green, 4.0f, false);
        }
    }
}

