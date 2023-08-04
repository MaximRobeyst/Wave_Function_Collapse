using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CubeTest : MonoBehaviour
{
    [SerializeField, OnValueChanged(nameof(UpdateCube))] private bool[] _points = new bool[8];
    [SerializeField] private Material _material;
    [SerializeField] private bool _useMeshes;

    [ReadOnly, SerializeField] private int _currentIndex;

    private List<Vector3> _vertices = new List<Vector3>();

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private Mesh _mesh;

    private List<GameObject> _instances = new();

    void ClearInstances()
    {
        foreach(GameObject instance in _instances)
        {
            DestroyImmediate(instance);
        }
        _instances.Clear();
    }

    void UpdateCube()
    {
        ClearInstances();
        _vertices.Clear();
        SetupMeshes();

        int instancesNow = _instances.Count;

        MarchingCubes.MarchCube(_points, Vector3.zero, _vertices, _instances, _useMeshes);
        if (instancesNow < _instances.Count)
        {
            _instances[_instances.Count - 1].transform.parent = transform;
            _instances[_instances.Count - 1].transform.position = transform.position + Vector3.one * 0.5f;
        }

        _mesh= new Mesh();
        _mesh.name = "Marching Cubes";

        if (_mesh == null)
            _mesh = _meshFilter.mesh;

        _mesh.vertices = _vertices.ToArray();

        List<int> indices = new List<int>();
        for (int i = 0; i < _vertices.Count; ++i)
            indices.Add(i);

        _mesh.triangles = indices.ToArray();

        List<Vector3> normals = new List<Vector3>();
        for (int i = 0; i < _vertices.Count; i += 3)
        {
            Vector3 A = _vertices[i + 1] - _vertices[i];
            Vector3 B = _vertices[i + 2] - _vertices[i];

            Vector3 normal = Vector3.Cross(A, B).normalized;

            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
        }
        _mesh.normals = normals.ToArray();

        _meshFilter.mesh = _mesh;
        _currentIndex = MarchingCubes.GetLookUpIndex(_points);
    }

    [Button]
    void FlipX()
    {
        _points = MarchingCubeModule.FlipPointsX(_points);
        UpdateCube();
    }

    [Button]
    void FlipY()
    {
        _points = MarchingCubeModule.FlipPointsY(_points);
        UpdateCube();
    }

    [Button]
    void FlipZ()
    {
        _points = MarchingCubeModule.FlipPointsZ(_points);
        UpdateCube();
    }

    [Button]
    void Rotate1()
    {
        _points = MarchingCubeModule.RotatePoints(_points, 1);
        UpdateCube();
    }

    [Button]
    void Rotate2()
    {
        _points = MarchingCubeModule.RotatePoints(_points, 2);
        UpdateCube();
    }

    [Button]
    void Rotate3()
    {
        _points = MarchingCubeModule.RotatePoints(_points, 3);
        UpdateCube();
    }

    [Button]
    void UpdateIndex()
    {
        _points = MarchingCubeModule.GetPoints(_currentIndex + 1);
        UpdateCube();
    }


    private void OnDrawGizmos()
    {
        for(int i = 0; i < MarchingCubes._corners.Length; i++)
        {
            Gizmos.color = _points[i] ? Color.white : Color.black;
            Gizmos.DrawSphere(transform.position + MarchingCubes._corners[i], 0.1f);
        }

#if UNITY_EDITOR
        Handles.Label(transform.position + new Vector3(-0.0f, -0.0f, -0.0f) + new Vector3(0,0.2f,0.0f), "A");
        Handles.Label(transform.position + new Vector3(1.0f, -0.0f, -0.0f) + new Vector3(0,0.2f,0.0f), "B");
        Handles.Label(transform.position + new Vector3(1.0f, -0.0f, 1.0f) + new Vector3(0,0.2f,0.0f), "C");
        Handles.Label(transform.position + new Vector3(-0.0f, -0.0f, 1.0f) + new Vector3(0,0.2f,0.0f), "D");
        Handles.Label(transform.position + new Vector3(-0.0f, 1.0f, -0.0f) + new Vector3(0,0.2f,0.0f), "E");
        Handles.Label(transform.position + new Vector3(1.0f, 1.0f, -0.0f) + new Vector3(0,0.2f,0.0f), "F");
        Handles.Label(transform.position + new Vector3(1.0f, 1.0f, 1.0f) + new Vector3(0,0.2f,0.0f), "G");
        Handles.Label(transform.position + new Vector3(-0.0f, 1.0f, 1.0f) + new Vector3(0,0.2f,0.0f), "H");
#endif

        //for (int i = 0; i < _vertices.Count; i += 3)
        //{
        //    Gizmos.color = Color.white;
        //    Gizmos.DrawLine(_vertices[i], _vertices[i + 1]);
        //    Gizmos.DrawLine(_vertices[i + 1], _vertices[i + 2]);
        //    Gizmos.DrawLine(_vertices[i], _vertices[i + 2]);
        //}
    }

    void SetupMeshes()
    {
        if (_meshFilter != null) return;
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null) _meshFilter = gameObject.AddComponent<MeshFilter>();

        if (_meshRenderer != null) return;
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null) _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = _material;

    }
}
