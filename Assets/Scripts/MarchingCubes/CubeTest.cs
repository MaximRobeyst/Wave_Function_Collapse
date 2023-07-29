using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CubeTest : MonoBehaviour
{
    [SerializeField, OnValueChanged(nameof(UpdateCube))] private bool[] _points = new bool[8];
    [SerializeField] private Material _material;

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

        MarchingCubes.MarchCube(_points, transform.position + Vector3.one * 0.5f, _vertices, _instances);

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
