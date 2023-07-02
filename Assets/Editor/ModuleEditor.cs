using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Module))]
public class ModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Module module = (Module)target;
        Mesh mesh = module.GetComponent<MeshFilter>().sharedMesh;



        if(GUILayout.Button("Save Sockets"))
        {
            SetupSockets(mesh);
        }
    }

    void SetupSockets(Mesh mesh)
    {
        SocketDictionary socketDictionary = SocketDictionary.Instance;

        List<Vector3> rightPositions = new();
        List<Vector3> leftPositions = new();
        List<Vector3> forwardPositions = new();
        List<Vector3> backwardPositions = new();
        foreach (Vector3 vertex in mesh.vertices)
        {
            if (vertex.x >= mesh.bounds.max.x)
                rightPositions.Add(vertex);
            if (vertex.x <= mesh.bounds.min.x)
                leftPositions.Add(vertex);


            if (vertex.z >= mesh.bounds.max.z)
                forwardPositions.Add(vertex);
            if (vertex.z <= mesh.bounds.min.z)
                backwardPositions.Add(vertex);

        }

        socketDictionary.AddSocket(rightPositions.ToArray());
        socketDictionary.AddSocket(leftPositions.ToArray());
        socketDictionary.AddSocket(forwardPositions.ToArray());
        socketDictionary.AddSocket(backwardPositions.ToArray());


        //socketDictionary.AddSocket()
    }
}
