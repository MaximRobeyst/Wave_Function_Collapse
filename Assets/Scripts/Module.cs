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
}
