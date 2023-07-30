using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MarchingCubeExtractor : MonoBehaviour
{
    [SerializeField] private string _name = "MarcingCubeMesh_Green_";

    private List<GameObject> _list = new();
    [SerializeField] private Material _material;

    [Button]
    void SpawnMeshes()
    {
        Clear();

        for(int i =0; i < Mathf.Pow(2, 8); ++i)
        {
            GameObject newObject = new GameObject(_name + i);
            newObject.transform.position = transform.position + transform.right * (i * 2);
            newObject.transform.SetParent(transform);

            Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Art/MarchingCubeResults/" + ("CubeResult"+i.ToString()) + ".asset", typeof(Mesh));
            if (mesh == null)
            {
                SetupMesh(i);
                mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Art/MarchingCubeResults/" + "CubeResult" + i, typeof(Mesh));
            }

            MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();
            meshRenderer.material = _material;

            _list.Add( newObject );
        }
    }

    void SetupMesh(int i)
    {
        List<Vector3> vertices = new();

        Mesh mesh = new Mesh();
        mesh.name = "CubeResult" + i;
        MarchingCubes.MarchCube(GenerateArrayFromIndex(i), Vector3.zero, vertices, _list, false);

        mesh.vertices = vertices.ToArray();

        List<int> indices = new List<int>();
        for (int index = 0; index < vertices.Count; ++index)
            indices.Add(index);

        mesh.triangles = indices.ToArray();

        List<Vector3> normals = new List<Vector3>();
        for (int normalIndex = 0; normalIndex < vertices.Count; normalIndex += 3)
        {
            Vector3 A = vertices[normalIndex + 1] - vertices[normalIndex];
            Vector3 B = vertices[normalIndex + 2] - vertices[normalIndex];

            Vector3 normal = Vector3.Cross(A, B).normalized;

            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
        }
        mesh.normals = normals.ToArray();

        SaveMesh(mesh.name, mesh);

    }

    void SaveMesh(string name, Mesh mesh)
    {
        string path = EditorUtility.SaveFilePanel("Save separate Mesh Asset", "Assets/Art/MarchingCubeResults/", name, "asset");

        path = FileUtil.GetProjectRelativePath(path);

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
    }

    [Button]
    void Clear()
    {
        foreach(GameObject obj in _list)
        {
            if (obj == null) continue;
            DestroyImmediate(obj);
        }
        _list.Clear();
    }

    bool[] GenerateArrayFromIndex(int index)
    {
        bool[] array = new bool[8];

        array[0] = ((int)MarchingCubeValues.A_RightBottomBack & index) != 0;
        array[1] = ((int)MarchingCubeValues.B_LeftBottomBack & index) != 0;
        array[2] = ((int)MarchingCubeValues.C_LeftBottomForward & index) != 0;
        array[3] = ((int)MarchingCubeValues.D_RightBottomForward & index) != 0;
        array[4] = ((int)MarchingCubeValues.E_RightUpBack & index) != 0;
        array[5] = ((int)MarchingCubeValues.F_LeftUpBack & index) != 0;
        array[6] = ((int)MarchingCubeValues.G_LeftUpForward & index) != 0;
        array[7] = ((int)MarchingCubeValues.H_RightUpForward & index) != 0;

        return array;
    }
}
