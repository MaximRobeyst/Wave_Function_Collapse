using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class MarchingCubeMeshDescriptor
{
    public int Index;

    public Mesh Mesh;

    public int RotationIndex;
    public bool FlippedX;
    public bool FlippedY;
    public bool FlippedZ;
}

public class MarchingCubeExtractor : MonoBehaviour
{
    [SerializeField] private string _name = "MarcingCubeMesh_Green_";

    private List<GameObject> _list = new();
    [SerializeField] private Material _material;

    [SerializeField] private MarchingCubeMeshDescriptor[] _marchingCubeMeshDescriptor = new MarchingCubeMeshDescriptor[256];
    [SerializeField] private List<Mesh> _meshes = new();
    [SerializeField] private int _uniqueMeshes = 0;

    [SerializeField] private bool _allowXFlipping = true;
    [SerializeField] private bool _allowYFlipping = true;
    [SerializeField] private bool _allowZFlipping = true;

    [Button]
    void SpawnMeshes()
    {
        Clear();

        for (int i =0; i < Mathf.Pow(2, 8); ++i)
        {
            GameObject newObject = new GameObject(_name + i);
            newObject.transform.position = transform.position + transform.right * (i * 2);
            newObject.transform.SetParent(transform);

            bool[] points = MarchingCubeModule.GetPoints(i);
            bool foundDuplicate = false;
            for(int rotation =0; rotation < 4; ++rotation)
            {
                MarchingCubeMeshDescriptor cubeMeshDescriptor = GetDescriptor(MarchingCubes.GetLookUpIndex(MarchingCubeModule.RotatePoints(points, rotation)));

                if (cubeMeshDescriptor != null)
                {
                    foundDuplicate = true;

                    _marchingCubeMeshDescriptor[i] = new MarchingCubeMeshDescriptor();
                    _marchingCubeMeshDescriptor[i].Index = i;
                    _marchingCubeMeshDescriptor[i].Mesh = cubeMeshDescriptor.Mesh;

                    _marchingCubeMeshDescriptor[i].RotationIndex = rotation;
                    _marchingCubeMeshDescriptor[i].FlippedX = false;
                    _marchingCubeMeshDescriptor[i].FlippedY = false;
                    _marchingCubeMeshDescriptor[i].FlippedZ = false;

                    break;
                }

                cubeMeshDescriptor = GetDescriptor(MarchingCubes.GetLookUpIndex(MarchingCubeModule.FlipPointsY(MarchingCubeModule.RotatePoints(points, rotation))));

                if(cubeMeshDescriptor != null && _allowYFlipping)
                {
                    foundDuplicate = true;

                    _marchingCubeMeshDescriptor[i] = new MarchingCubeMeshDescriptor();
                    _marchingCubeMeshDescriptor[i].Index = i;
                    _marchingCubeMeshDescriptor[i].Mesh = cubeMeshDescriptor.Mesh;

                    _marchingCubeMeshDescriptor[i].RotationIndex = rotation;
                    _marchingCubeMeshDescriptor[i].FlippedX = false;
                    _marchingCubeMeshDescriptor[i].FlippedY = true;
                    _marchingCubeMeshDescriptor[i].FlippedZ = false;

                    break;
                }

                cubeMeshDescriptor = GetDescriptor(MarchingCubes.GetLookUpIndex(MarchingCubeModule.FlipPointsX(MarchingCubeModule.RotatePoints(points, rotation))));

                if (cubeMeshDescriptor != null && _allowXFlipping)
                {
                    foundDuplicate = true;

                    _marchingCubeMeshDescriptor[i] = new MarchingCubeMeshDescriptor();
                    _marchingCubeMeshDescriptor[i].Index = i;
                    _marchingCubeMeshDescriptor[i].Mesh = cubeMeshDescriptor.Mesh;

                    _marchingCubeMeshDescriptor[i].RotationIndex = rotation;
                    _marchingCubeMeshDescriptor[i].FlippedX = true;
                    _marchingCubeMeshDescriptor[i].FlippedY = false;
                    _marchingCubeMeshDescriptor[i].FlippedZ = false;

                    break;
                }

                cubeMeshDescriptor = GetDescriptor(MarchingCubes.GetLookUpIndex(MarchingCubeModule.FlipPointsZ(MarchingCubeModule.RotatePoints(points, rotation))));

                if (cubeMeshDescriptor != null && _allowZFlipping)
                {
                    foundDuplicate = true;

                    _marchingCubeMeshDescriptor[i] = new MarchingCubeMeshDescriptor();
                    _marchingCubeMeshDescriptor[i].Index = i;
                    _marchingCubeMeshDescriptor[i].Mesh = cubeMeshDescriptor.Mesh;

                    _marchingCubeMeshDescriptor[i].RotationIndex = rotation;
                    _marchingCubeMeshDescriptor[i].FlippedX = false;
                    _marchingCubeMeshDescriptor[i].FlippedY = false;
                    _marchingCubeMeshDescriptor[i].FlippedZ = true;

                    break;
                }
            }
            if (foundDuplicate)
                continue;

            ++_uniqueMeshes;
            Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Art/MarchingCubeResults/" + ("CubeResult"+i.ToString()) + ".asset", typeof(Mesh));
            if (mesh == null)
            {
                SetupMesh(i);
                mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Art/MarchingCubeResults/" + "CubeResult" + i, typeof(Mesh));
            }
            _meshes.Add(mesh);

            _marchingCubeMeshDescriptor[i] = new MarchingCubeMeshDescriptor();
            _marchingCubeMeshDescriptor[i].Index = i;
            _marchingCubeMeshDescriptor[i].Mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Art/MarchingCubeResults/" + ("CubeResult" + i.ToString()) + ".asset", typeof(Mesh));

            _marchingCubeMeshDescriptor[i].RotationIndex = 0;
            _marchingCubeMeshDescriptor[i].FlippedY = false;

            MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();
            meshRenderer.material = _material;

            newObject.transform.position = transform.position + transform.right * (_uniqueMeshes * 2);

            _list.Add( newObject );
        }
    }

    public MarchingCubeMeshDescriptor GetDescriptor( int index )
    {
        return Array.Find(_marchingCubeMeshDescriptor, (descriptor => descriptor != null && descriptor.Index == index)); 
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
        _uniqueMeshes = 0;
        _marchingCubeMeshDescriptor = new MarchingCubeMeshDescriptor[256];
        foreach (GameObject obj in _list)
        {
            if (obj == null) continue;
            DestroyImmediate(obj);
        }
        _list.Clear();
        _meshes.Clear();
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
