using NaughtyAttributes;
using System;
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

public enum SocketDirection
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
    [SerializeField] public bool _3D;
    [SerializeField] private float _bounds = 1.0f;

    private SocketInfo[] _sockets;

    [SerializeField] SocketInfo SocketLeft;
    [SerializeField] SocketInfo SocketForward;
    [SerializeField] SocketInfo SocketRight;
    [SerializeField] SocketInfo SocketBack;
    [SerializeField, OnValueChanged(nameof(SetupSockets)), ShowIf(nameof(_3D))] SocketInfo Up;
    [SerializeField, OnValueChanged(nameof(SetupSockets)), ShowIf(nameof(_3D))] SocketInfo Down;

    public SocketInfo[] Sockets 
    { 
        get
        {
            SetupSockets();
            return _sockets;
        }
        set
        {
            _sockets = value;
            UpdateSockets();
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

        Mesh mesh = meshFilter.sharedMesh;

        float minBounds = -_bounds / 2.0f;
        float maxBounds = _bounds / 2.0f;

        foreach (Vector3 vertex in mesh.vertices)
        {
            if (vertex.x <= minBounds + 0.001f)
                left.Add(new Vector2(vertex.z, vertex.y));
            if (vertex.z <= minBounds + 0.001f)
                back.Add(new Vector2(vertex.x, vertex.y));
            if (vertex.y <= minBounds + 0.001f)
                down.Add(new Vector2(vertex.x, vertex.z));

            if (vertex.x >= maxBounds - 0.001f)
                right.Add(new Vector2(vertex.z, vertex.y));
            if (vertex.z >= maxBounds - 0.001f)
                forward.Add(new Vector2(vertex.x, vertex.y));
            if (vertex.y >= maxBounds - 0.001f)
                up.Add(new Vector2(vertex.x, vertex.z));

        }

        SocketLeft = SetupHorizontalSocket(left);
        SocketDictionary.Instance.AddSocket(SocketLeft);
        SocketBack = SetupHorizontalSocket(back);
        SocketDictionary.Instance.AddSocket(SocketBack);
        
        SocketRight = SetupHorizontalSocket(right);
        SocketDictionary.Instance.AddSocket(SocketRight);
        SocketForward = SetupHorizontalSocket(forward);
        SocketDictionary.Instance.AddSocket(SocketForward);

        Up = SetupVerticalSocket(up);
        SocketDictionary.Instance.AddSocket(Up);

        Down = SetupVerticalSocket(down);
        SocketDictionary.Instance.AddSocket(Down);

        SetupSockets();
    }

    SocketInfo SetupHorizontalSocket(List<Vector2> list)
    {
        list = SocketDictionary.RemoveDuplicates(list);

        // if we find socket in socketdictionary we have found our socket
        SocketInfo socketInfo = SocketDictionary.Instance.FindSocket(list.ToArray());
        if (socketInfo != null)
        {
            socketInfo = new SocketInfo(socketInfo);
            socketInfo.Vertices = list.ToArray();
            return socketInfo;
        }

        // if we find a flipped version of our socket in the socket dictionary we have found a flipped socket
        List<Vector2> flippedVertices = SocketDictionary.FlipVertices(list, true);
        socketInfo = SocketDictionary.Instance.FindSocket(flippedVertices.ToArray());
        if(socketInfo != null)
        {
            SocketInfo flippedSocket = new SocketInfo();
            flippedSocket.Vertices = flippedVertices.ToArray();
            flippedSocket.Name = socketInfo.Name;
        
            flippedSocket.Symmetrical = false;
            flippedSocket.Flipped = true;
        
            return flippedSocket;
        }
        
        // if we haven't found any results we have to make a new entry in the socket dictionary
        socketInfo = new SocketInfo();
        socketInfo.Name = SocketDictionary.Instance.GetNextSocketName();
        socketInfo.Vertices = list.ToArray();
    
        // if our flipped version is the same as our normal one then our socket is symmetrical
        if (SocketDictionary.AreVerticesTheSame(list, flippedVertices))
            socketInfo.Symmetrical = true;
    
        return socketInfo;
    }

    SocketInfo SetupVerticalSocket(List<Vector2> list)
    {
        list = SocketDictionary.RemoveDuplicates(list);

        SocketInfo socketInfo = SocketDictionary.Instance.FindSocket(list.ToArray());
        if (socketInfo != null)
            return socketInfo;

        socketInfo = new SocketInfo();
        socketInfo.Name = SocketDictionary.Instance.GetNextSocketName();
        socketInfo.Vertices = list.ToArray();

        //if (SocketDictionary.AreVerticesTheSame(left, flippedVertices))
        //    socketInfo.Symmetrical = true;

        return socketInfo;
    }

    public bool AnyEmpty()
    {
        if (Sockets.Length < 6) return true;

        return Sockets.Any(socket => socket.Name == "");
    }

    public void UpdateSockets()
    {
        SocketLeft = _sockets[(int)SocketDirection.Left];
        SocketForward = _sockets[(int)SocketDirection.Forward];
        SocketRight = _sockets[(int)SocketDirection.Right];
        SocketBack = _sockets[(int)SocketDirection.Backward];

        Up = _sockets[(int)SocketDirection.Up];
        Down = _sockets[(int)SocketDirection.Down];
    }

    public void SetupSockets()
    {
        if(_sockets == null || _sockets.Length < (_3D ? 6 : 4) ) _sockets = new SocketInfo[_3D ? 6 : 4];

        _sockets[(int)SocketDirection.Left] = SocketLeft;
        _sockets[(int)SocketDirection.Forward] = SocketForward;
        _sockets[(int)SocketDirection.Right] = SocketRight;
        _sockets[(int)SocketDirection.Backward] = SocketBack;

        if (!_3D) return;
        _sockets[(int)SocketDirection.Up] = Up;
        _sockets[(int)SocketDirection.Down] = Down;
    }

    private void OnDrawGizmos()
    {
        if (_sockets == null || _sockets.Length < (_3D ? 6 : 4)) SetupSockets();
        if (_3D) 
            DebugGizmos.DrawCube(transform.position, _bounds, _bounds, _bounds, Color.white);

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

    public static SocketInfo[] TransformSockets(int rotation, FlipValues flipValues, SocketInfo[] oldSockets)
    {
        var newSockets = RotatePoints(oldSockets, rotation);
        if ((flipValues & FlipValues.FlipX) != 0)
            newSockets = FlipPointsX(newSockets);
        if ((flipValues & FlipValues.FlipY) != 0)
            newSockets = FlipPointsY(newSockets);
        if ((flipValues & FlipValues.FlipZ) != 0)
            newSockets = FlipPointsZ(newSockets);

        return newSockets;
    }

    private static SocketInfo[] RotatePoints(SocketInfo[] oldSockets, int rotationIndex)
    {
        SocketInfo[] points = new SocketInfo[oldSockets.Length];
        Array.Copy(oldSockets, points, oldSockets.Length);

        for (int i = 0; i < 4; ++i)
        {
            points[i] = oldSockets[(i + rotationIndex) % 4];
        }

        return points;
    }

    private static SocketInfo[] FlipPointsX(SocketInfo[] oldSockets)
    {
        SocketInfo[] points = new SocketInfo[oldSockets.Length];
        Array.Copy(oldSockets, points, oldSockets.Length);

        points[(int)SocketDirection.Left] = oldSockets[(int)SocketDirection.Right];
        points[(int)SocketDirection.Right] = oldSockets[(int)SocketDirection.Left];

        return points;
    }

    private static SocketInfo[] FlipPointsY(SocketInfo[] oldSockets)
    {
        SocketInfo[] points = new SocketInfo[oldSockets.Length];
        Array.Copy(oldSockets, points, oldSockets.Length);

        points[(int)SocketDirection.Up] = oldSockets[(int)SocketDirection.Down];
        points[(int)SocketDirection.Down] = oldSockets[(int)SocketDirection.Up];

        return points;
    }

    [Button]
    private void FlipX()
    {
        _sockets = FlipPointsX(_sockets);
    }

    [Button]
    private void FlipY()
    {
        _sockets = FlipPointsY(_sockets);
    }

    [Button]
    private void FlipZ()
    {
        _sockets = FlipPointsZ(_sockets);
    }

    [Button]
    private void Rotate()
    {
        _sockets = RotatePoints(_sockets, 1);
    }

    private static SocketInfo[] FlipPointsZ(SocketInfo[] oldSockets)
    {
        SocketInfo[] points = new SocketInfo[oldSockets.Length];
        Array.Copy(oldSockets, points, oldSockets.Length);

        points[(int)SocketDirection.Forward] = oldSockets[(int)SocketDirection.Backward];
        points[(int)SocketDirection.Backward] = oldSockets[(int)SocketDirection.Forward];

        return points;
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
