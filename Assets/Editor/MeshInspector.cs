using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(MeshStudy))]
public class MeshInspector : Editor
{
    private MeshStudy mesh;
    private Transform handleTransform;
    private Quaternion handleRotation;
    string triangleIdx;

    void OnSceneGUI()
    {
        mesh = target as MeshStudy;
        Debug.Log("Custom editor is running");

        EditMesh();
    }

    void EditMesh()
    {
        handleTransform = mesh.transform; //The handleTransform gets Transform values from mesh
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity; //The handleRotation gets the current pivot Rotation mode
        for (int i = 0; i < mesh.vertices.Length; i++) //Loops through the mesh vertices and draws dots with ShowPoint()
        {
            ShowPoint(i);
        }
    }

    private void ShowPoint(int index)
    {
        if (mesh.moveVertexPoint)
        {
            //draw dot
            //Converts the vertex local position into world space
            Vector3 point = handleTransform.TransformPoint(mesh.vertices[index]);
            //Set color, size and position of the dot and makes an unconstrained movement handle to facilitate the dragging action
            Handles.color = Color.blue;
            point = Handles.FreeMoveHandle(point, handleRotation, mesh.handleSize, Vector3.zero, Handles.DotHandleCap);

            //drag
            if (GUI.changed) //Monitors any changes made to the dots
            {
                mesh.DoAction(index, handleTransform.InverseTransformPoint(point)); //Receive its index and Transform values as params
            }
        }
        else
        {
            //click
        }
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        mesh = target as MeshStudy;

        if (GUILayout.Button("Reset")) //This code draws a Reset button in the Inspector
        {
            mesh.Reset(); //When pressed, it calls the Reset() function in MeshStudy.cs
        }

        // For testing Reset function
        if (mesh.isCloned)
        {
            if (GUILayout.Button("Test Edit"))
            {
                mesh.EditMesh();
            }
        }
    }


}
