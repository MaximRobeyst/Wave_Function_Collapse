using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[System.Serializable]
public class SocketInfo
{
    public Vector3[] Vertices;
    public string Name;
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

    public string AddSocket(Vector3[] vertices)
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

    public Vector3[] GetVerticesForSocket(string name)
    {
        return SocketInfo.Where(socket => (socket.Name == name)).First().Vertices;
    }
}
