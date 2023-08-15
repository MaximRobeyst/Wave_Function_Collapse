using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MarchingCubeMeshes
{
    [EnumFlags]
    public MarchingCubeValues MarchingCubeValues;
    public int MarchingCubeValue;
    [ShowAssetPreview(128, 128)]
    public MarchingCubeModule Mesh;

    public FlipValues Flipped;

    public int RotationIndex;

    public bool FitsDirection(SocketDirection socketDirection, MarchingCubeMeshes marchingCubeMeshes)
    {
        var sockets1 = Module.TransformSockets(RotationIndex, Flipped, Mesh.Module.Sockets);
        if (marchingCubeMeshes.Mesh.Module.AnyEmpty())
            Debug.LogError("no module sockets found in: " + marchingCubeMeshes.Mesh.name, marchingCubeMeshes.Mesh.gameObject);

        var sockets2 = Module.TransformSockets(marchingCubeMeshes.RotationIndex, marchingCubeMeshes.Flipped, marchingCubeMeshes.Mesh.Module.Sockets);

        switch (socketDirection)
        {
            case SocketDirection.Left:
                return sockets1[(int)SocketDirection.Left].Fits(sockets2[(int)SocketDirection.Right]);
            case SocketDirection.Right:
                return sockets1[(int)SocketDirection.Right].Fits(sockets2[(int)SocketDirection.Left]);
            case SocketDirection.Forward:
                return sockets1[(int)SocketDirection.Forward].Fits(sockets2[(int)SocketDirection.Backward]);
            case SocketDirection.Backward:
                return sockets1[(int)SocketDirection.Backward].Fits(sockets2[(int)SocketDirection.Forward]);
            case SocketDirection.Up:
                return sockets1[(int)SocketDirection.Up].Fits(sockets2[(int)SocketDirection.Down]);
            case SocketDirection.Down:
                return sockets1[(int)SocketDirection.Down].Fits(sockets2[(int)SocketDirection.Up]);

        }

        return false;
    }


}

[CreateAssetMenu(fileName = "MeshLookUpTable", menuName = "MeshLookUpTable")]
public class MeshTable : ScriptableObject
{
    private static MeshTable _instance;
    public static MeshTable Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load("MeshLookUpTable") as MeshTable;

            return _instance;
        }
    }

    public List<MarchingCubeMeshes> Values;

    public MarchingCubeMeshes GetMesh(MarchingCubeValues description)
    {
        foreach(MarchingCubeMeshes cubeMeshes in Values)
        {
            if (cubeMeshes.MarchingCubeValues == description)
                return cubeMeshes;
        }

        return null;
    }

    public List<MarchingCubeMeshes> GetMeshes(MarchingCubeValues description)
    {
        List<MarchingCubeMeshes> meshes = new();
        foreach (MarchingCubeMeshes cubeMeshes in Values)
        {
            if (cubeMeshes.MarchingCubeValues == description)
                meshes.Add(cubeMeshes);
        }

        return meshes;
    }

    public bool Contains(MarchingCubeValues description)
    {
        foreach(MarchingCubeMeshes cubeMesh in Values)
        {
            if (cubeMesh.MarchingCubeValues == description)
                return true;
        }

        return false;
    }

    public bool Contains(MarchingCubeValues description, MarchingCubeModule prefab)
    {
        foreach (MarchingCubeMeshes cubeMesh in Values)
        {
            if (cubeMesh.MarchingCubeValues == description && cubeMesh.Mesh == prefab)
                return true;
        }

        return false;
    }


    [Button]
    private void CheckForMissingValues()
    {
        for(int i =0;i < Mathf.Pow(2, 8); ++i)
        {
            if (Values.Find(marchingCubeMesh => marchingCubeMesh.MarchingCubeValue == i) == null)
                Debug.Log("no mesh found for " + i);
        }
    }

    [Button]
    private void CheckForMissingPrefabs()
    {
        for(int i =0;i < Values.Count; ++i)
        {
            if (Values[i].Mesh == null)
                Debug.Log("No mesh found for index : " + i);
        }
    }
}
