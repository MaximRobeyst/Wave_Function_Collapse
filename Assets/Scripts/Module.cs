using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Socket
{
    public string Name;

    public bool Flipped;
    public bool Symetrical;

    public override string ToString()
    {
        return Name + (Flipped ? "F" : "");
    }
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

    [SerializeField] SocketInfo SocketLeft;
    [SerializeField] SocketInfo SocketForward;
    [SerializeField] SocketInfo SocketRight;
    [SerializeField] SocketInfo SocketBack;

    public Socket[] Sockets 
    { 
        get
        {
            SetupSockets();
            return _sockets;
        } 
    }

    [Button]
    public void AutoSetupSockets()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();

        List<Vector2> left = new();
        List<Vector2> right = new();
        List<Vector2> down = new();
        List<Vector2> up = new();
        List<Vector2> forward = new();
        List<Vector2> back = new();

        //Mesh mesh = meshFilter.sharedMesh;
        //foreach (Vector3 vertex in mesh.vertices)
        //{
        //    if(vertex.x <= mesh.bounds.min.x + 0.001f)
        //        left.Add(vertex.z);
        //    if (vertex.z <= mesh.bounds.min.z + 0.001f)
        //        back.Add(vertex.x);
        //
        //    if (vertex.x >= mesh.bounds.max.x - 0.001f)
        //        right.Add(vertex.z);
        //    if (vertex.z >= mesh.bounds.max.z - 0.001f)
        //        forward.Add(vertex.x);
        //
        //}
        //
        //SocketLeft = SetupHorizontalSocketX(left);
        //SocketDictionary.Instance.AddSocket(SocketLeft);
        //SocketBack = SetupHorizontalSocketZ(back);
        //SocketDictionary.Instance.AddSocket(SocketBack);
        //
        //SocketRight = SetupHorizontalSocketX(right);
        //SocketDictionary.Instance.AddSocket(SocketRight);
        //SocketForward = SetupHorizontalSocketZ(forward);
        //SocketDictionary.Instance.AddSocket(SocketForward);
    }

    //SocketInfo SetupHorizontalSocketX(List<float> left)
    //{
    //    SocketInfo socketInfo = SocketDictionary.Instance.FindSocket(left.ToArray());
    //    if (socketInfo != null)
    //        return socketInfo;
    //    socketInfo = SocketDictionary.Instance.FindSocket(SocketDictionary.FlipVertices(left, true, false).ToArray());
    //    if(socketInfo != null)
    //    {
    //        socketInfo.Flipped = true;
    //
    //        return socketInfo;
    //    }
    //
    //
    //    socketInfo = new SocketInfo();
    //    socketInfo.Name = SocketDictionary.Instance.GetNextSocketName();
    //    socketInfo.Vertices = left.ToArray();
    //
    //    List<Vector3> flippedVertices = SocketDictionary.FlipVertices(left, false, true);
    //
    //    if (SocketDictionary.AreVerticesTheSame(left, flippedVertices))
    //        socketInfo.Symmetrical = true;
    //
    //    return socketInfo;
    //}

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
        Handles.Label(transform.position + -transform.right * 0.5f, _sockets[(int)SocketDirection.Left].ToString(), style);
        style.normal.textColor = Color.green;
        Handles.Label(transform.position + transform.forward * 0.5f, _sockets[(int)SocketDirection.Forward].ToString(), style);
        style.normal.textColor = Color.blue;
        Handles.Label(transform.position + transform.right * 0.5f, _sockets[(int)SocketDirection.Right].ToString(), style);
        style.normal.textColor = Color.red;
        Handles.Label(transform.position + -transform.forward * 0.5f, _sockets[(int)SocketDirection.Backward].ToString(), style);

        if (!_3D && _sockets.Count() >= (int)SocketDirection.Up) return;

        Handles.Label(transform.position + transform.up * 0.5f, _sockets[4].ToString());
        Handles.Label(transform.position + -transform.up * 0.5f, _sockets[5].ToString());
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
