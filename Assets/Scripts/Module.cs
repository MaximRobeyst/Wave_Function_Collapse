using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Socket
{
    string name;

    public bool Symetrical;
}

[RequireComponent(typeof(MeshFilter))]
public class Module : MonoBehaviour
{
    private Socket[] Sockets = new Socket[6];




    //[Button]
    //void SetupSockets()
    //{
    //    Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
    //
    //    SocketDictionary socketDictionary = SocketDictionary.Instance;
    //
    //    List<Vector3> rightPositions = new();
    //    List<Vector3> leftPositions = new();
    //    List<Vector3> forwardPositions = new();
    //    List<Vector3> backwardPositions = new();
    //    foreach (Vector3 vertex in mesh.vertices)
    //    {
    //        if (vertex.x >= mesh.bounds.max.x)
    //            rightPositions.Add(vertex);
    //        if (vertex.x <= mesh.bounds.min.x)
    //            leftPositions.Add(vertex);
    //
    //
    //        if (vertex.z >= mesh.bounds.max.z)
    //            forwardPositions.Add(vertex);
    //        if (vertex.z <= mesh.bounds.min.z)
    //            backwardPositions.Add(vertex);
    //
    //    }
    //
    //    socketDictionary.AddSocket(rightPositions.ToArray());
    //    socketDictionary.AddSocket(leftPositions.ToArray());
    //    socketDictionary.AddSocket(forwardPositions.ToArray());
    //    socketDictionary.AddSocket(backwardPositions.ToArray());
    //}
}
