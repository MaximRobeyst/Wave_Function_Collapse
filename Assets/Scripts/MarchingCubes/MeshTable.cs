using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MarchingCubeMeshes
{
    [EnumFlags]
    public MarchingCubeValues MarchingCubeValues;

    public GameObject Mesh;
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

    public GameObject GetMesh(MarchingCubeValues description)
    {
        foreach(MarchingCubeMeshes cubeMeshes in Values)
        {
            if (cubeMeshes.MarchingCubeValues == description)
                return cubeMeshes.Mesh;
        }

        return null;
    }

}
