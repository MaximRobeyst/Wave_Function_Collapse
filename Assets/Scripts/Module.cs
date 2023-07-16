using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Socket
{
    public string Name;

    public bool Symetrical;
}

enum SocketDirection
{ 
    Left = 0,
    Forward = 1,
    Right = 2,
    Backward = 3,
}

[RequireComponent(typeof(MeshFilter))]
public class Module : MonoBehaviour
{
    [SerializeField]
    private Socket[] _sockets = new Socket[4];

    public Socket[] Sockets => _sockets;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Handles.Label(transform.position + -transform.right * 0.5f, _sockets[0].Name);
        Handles.Label(transform.position + transform.forward * 0.5f, _sockets[1].Name);
        Handles.Label(transform.position + transform.right * 0.5f, _sockets[2].Name);
        Handles.Label(transform.position + -transform.forward * 0.5f, _sockets[3].Name);
#endif
    }



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
