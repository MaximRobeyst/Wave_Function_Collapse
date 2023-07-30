using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using NaughtyAttributes;

[System.Serializable]
public class SocketInfo
{
    [ReadOnly] public Vector2[] Vertices;
    public string Name;

    public bool Symmetrical;
    public bool Flipped;
}

[CreateAssetMenu(fileName = "Socket dictionary", menuName = "Socket Dictionary")]
public class SocketDictionary : ScriptableObject
{
    [SerializeField] private List<SocketInfo> SocketInfo = new List<SocketInfo>();

    private static SocketDictionary _instance;
    public static SocketDictionary Instance
    {
        get 
        {
            if (_instance == null)
                _instance = Resources.Load("SocketDictionary") as SocketDictionary;

            return _instance; 
        }
    }

    public string AddSocket(Vector2[] vertices)
    {
        var result = SocketInfo.Find(socket => (socket.Vertices == vertices));
        if (result != null)
            return result.Name;

        SocketInfo newSocket = new SocketInfo();
        newSocket.Vertices = vertices;
        newSocket.Name = SocketInfo.Count.ToString();

        SocketInfo.Add(newSocket);
        return newSocket.Name;
    }

    public void AddSocket(SocketInfo socketInfo)
    {
        if (SocketInfo.Contains(socketInfo)) return;

        SocketInfo.Add(socketInfo);
    }

    public string GetNextSocketName()
    {
        return SocketInfo.Count.ToString();
    }

    //public SocketInfo FindSocket(Vector2[] vertices)
    //{
    //    foreach( var socket in SocketInfo)
    //    {
    //        if (AreVerticesTheSame(vertices, socket.Vertices))
    //            return socket;
    //    }
    //
    //    return null;
    //}

    public Vector2[] GetVerticesForSocket(string name)
    {
        return SocketInfo.Where(socket => (socket.Name == name)).First().Vertices;
    }

    public static List<Vector2> FlipVertices(List<Vector2> list, bool flipX, bool flipZ)
    {
        List<Vector2> flippedVertices = new();
        foreach (Vector2 point in list)
        {
            Vector2 flippedVertex = new Vector2(
                flipX ? -point.x : point.x, 
                point.y);
            flippedVertices.Add(flippedVertex);
        }

        return flippedVertices;
    }

    public static bool AreVerticesTheSame(List<Vector3> list1, List<Vector3> list2)
    {
        return AreVerticesTheSame(list1.ToArray(), list2.ToArray());
    }

    public static bool AreVerticesTheSame(Vector3[] list1, Vector3[] list2)
    {
        foreach (Vector3 ist1Vector in list1)
        {
            bool containsList1Vector = false;
            foreach (Vector3 list2vector in list2)
            {
                if (Vector3.Distance(ist1Vector, list2vector) < 0.001f)
                {
                    containsList1Vector = true;
                    break;
                }
            }
            if (!containsList1Vector) return false;
        }
        return true;
    }
}
