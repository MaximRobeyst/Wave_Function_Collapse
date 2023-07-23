using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    Forward = 0,
    Left = 1,
    Backward = 2,
    Right = 3,

    Up = 4,
    Down = 5
}

public class Module : MonoBehaviour
{
    [SerializeField] private bool _3D;

    private Socket[] _sockets;

    [SerializeField, OnValueChanged(nameof(SetupSockets))]Socket Left;
    [SerializeField, OnValueChanged(nameof(SetupSockets))]Socket Forward;
    [SerializeField, OnValueChanged(nameof(SetupSockets))]Socket Right;
    [SerializeField, OnValueChanged(nameof(SetupSockets))]Socket Backward;
    [SerializeField, OnValueChanged(nameof(SetupSockets)), ShowIf(nameof(_3D))]Socket Up;
    [SerializeField, OnValueChanged(nameof(SetupSockets)), ShowIf(nameof(_3D))] Socket Down;

    public Socket[] Sockets 
    { 
        get
        {
            SetupSockets();
            return _sockets;
        } 
    }

    public void SetupSockets()
    {
        if(_sockets == null || _sockets.Length < (_3D ? 6 : 4) ) _sockets = new Socket[_3D ? 6 : 4];

        _sockets[(int)SocketDirection.Left] = Left;
        _sockets[(int)SocketDirection.Forward] = Forward;
        _sockets[(int)SocketDirection.Right] = Right;
        _sockets[(int)SocketDirection.Backward] = Backward;

        if (!_3D) return;
        _sockets[(int)SocketDirection.Up] = Up;
        _sockets[(int)SocketDirection.Down] = Down;
    }

    private void OnDrawGizmos()
    {
        if (_sockets == null || _sockets.Length < (_3D ? 6 : 4)) SetupSockets();
        if (_3D) 
            DebugGizmos.DrawCube(transform.position, 1.0f, 1.0f, 1.0f, Color.white);

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.magenta;
        Handles.Label(transform.position + -transform.right * 0.5f, _sockets[(int)SocketDirection.Left].Name, style);
        style.normal.textColor = Color.green;
        Handles.Label(transform.position + transform.forward * 0.5f, _sockets[(int)SocketDirection.Forward].Name, style);
        style.normal.textColor = Color.blue;
        Handles.Label(transform.position + transform.right * 0.5f, _sockets[(int)SocketDirection.Right].Name, style);
        style.normal.textColor = Color.red;
        Handles.Label(transform.position + -transform.forward * 0.5f, _sockets[(int)SocketDirection.Backward].Name, style);

        if (!_3D && _sockets.Count() >= (int)SocketDirection.Up) return;

        Handles.Label(transform.position + transform.up * 0.5f, _sockets[4].Name);
        Handles.Label(transform.position + -transform.up * 0.5f, _sockets[5].Name);
#endif
    }

    public Vector3 GetLeft()
    {
        return transform.position + -Vector3.right * 0.5f;
    }

    public Vector3 GetForward()
    {
        return transform.position + Vector3.forward * 0.5f;
    }

    public Vector3 GetRight()
    {
        return transform.position + Vector3.right * 0.5f;
    }

    public Vector3 GetBackward()
    {
        return transform.position + -Vector3.forward * 0.5f;
    }

    public Vector3 GetUp()
    {
        return transform.position + Vector3.up * 0.5f;
    }

    public Vector3 GetDown()
    {
        return transform.position + Vector3.down * 0.5f;
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
