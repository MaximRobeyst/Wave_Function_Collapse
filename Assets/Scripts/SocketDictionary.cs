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

    public SocketInfo()
    {

    }

    public SocketInfo(SocketInfo socketInfo)
    {
        Vertices = socketInfo.Vertices;
        Name = socketInfo.Name;
        Symmetrical = socketInfo.Symmetrical;
        Flipped = socketInfo.Flipped;
    }

    public override string ToString()
    {
        string name = Name;
        if (Symmetrical)
            name = name + "S";
        if (Flipped)
            name = name + "F";

        return name;
    }

    public bool Fits(SocketInfo info)
    {
        if (Name == "()" || info.Name == "()") return true;

        if (Name != info.Name)
            return false;

        if (Symmetrical) return Name == info.Name;

        return (Name == info.Name) && (info.Flipped != Flipped);
    }
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
        if (SocketInfo.Any(socket => socket.Name == socketInfo.Name || AreVerticesTheSame(socket.Vertices, socketInfo.Vertices))) return;

        SocketInfo.Add(socketInfo);
    }

    public string GetNextSocketName()
    {
        return SocketInfo.Count.ToString();
    }

    public SocketInfo FindSocket(Vector2[] vertices)
    {
        foreach( var socket in SocketInfo)
        {
            if (AreVerticesTheSame(vertices, socket.Vertices))
                return socket;
        }
    
        return null;
    }

    public Vector2[] GetVerticesForSocket(string name)
    {
        return SocketInfo.Where(socket => (socket.Name == name)).First().Vertices;
    }

    public static List<Vector2> FlipVertices(List<Vector2> list, bool flipX)
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

    public static bool AreVerticesTheSame(List<Vector2> list1, List<Vector2> list2)
    {
        return AreVerticesTheSame(list1.ToArray(), list2.ToArray());
    }

    public static List<Vector2> RemoveDuplicates(List<Vector2> list1, float distance = 0.001f)
    {
        List<Vector2> newList = new();
        foreach(Vector2 point in list1)
        {
            if (newList.Any(listPoint => Vector3.Distance(listPoint, point) < distance)) continue;
            newList.Add(point);

        }

        return newList;
    }

    public static bool AreVerticesTheSame(Vector2[] list1, Vector2[] list2)
    {
        if (list1.Length != list2.Length) return false;

        foreach (Vector2 ist1Vector in list1)
        {
            bool containsList1Vector = false;
            foreach (Vector2 list2vector in list2)
            {
                if (Vector2.Distance(ist1Vector, list2vector) < 0.001f)
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
