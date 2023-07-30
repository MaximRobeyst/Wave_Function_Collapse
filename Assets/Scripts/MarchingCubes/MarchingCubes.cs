using EasyButtons.Editor;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    [SerializeField] private int _step;
    [SerializeField] private Material _material;

    List<Vector3> _vertices = new List<Vector3>();

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;

    [SerializeField] private bool _drawMesh = false;
    [SerializeField] private bool _useMeshes = false;

    private List<GameObject> _instances = new List<GameObject>();

    public static Vector3[] _corners = new Vector3[8]
    {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(1,0,1),
        new Vector3(0,0,1),
        new Vector3(0,1,0),
        new Vector3(1,1,0),
        new Vector3(1,1,1),
        new Vector3(0,1,1),
    };

    [Button]
    void MarchCubes()
    {
        MarchCubes(GetComponent<PointDistribution>());
    }

    [Button]
    void MarchCubesWithStep()
    {
        MarchCubes(GetComponent<PointDistribution>(), _step);
    }

    [Button]
    void MarchCubesNextStep()
    {
        _step += 1;
        MarchCubes(GetComponent<PointDistribution>(), _step);
    }

    [Button]
    private void Clear()
    {
        ClearInstances();
        _mesh.Clear();

        _vertices.Clear();
    }

    private void MarchCubes(PointDistribution pointDistribution)
    {
        ClearInstances();
        _vertices.Clear();
        float[] cubeValues = new float[8];
        for(int i = 0; i < pointDistribution.Size - 1; ++i)
        {
            for (int j = 0; j < pointDistribution.Size - 1; ++j)
            {
                for (int k = 0; k < pointDistribution.Size - 1; ++k)
                {
                    cubeValues[0] = pointDistribution.Weights[pointDistribution.GetIndex(i      , j     , k     )];
                    cubeValues[1] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j     , k     )];
                    cubeValues[2] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j     , k + 1)];
                    cubeValues[3] = pointDistribution.Weights[pointDistribution.GetIndex(i      , j     , k + 1)];
                    cubeValues[4] = pointDistribution.Weights[pointDistribution.GetIndex(i      , j + 1, k     )];
                    cubeValues[5] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j + 1, k     )];
                    cubeValues[6] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j + 1, k + 1 )];
                    cubeValues[7] = pointDistribution.Weights[pointDistribution.GetIndex(i      , j + 1, k + 1 )];

                    MarchCube(cubeValues, pointDistribution.SurfaceLevel, pointDistribution.GetPosition(i,j,k), _vertices, _instances, _useMeshes);
                }
            }
        }
    }


    void ClearInstances()
    {
        foreach (GameObject instance in _instances)
        {
            if (instance == null) continue;
            DestroyImmediate(instance);
        }
        _instances.Clear();
    }

    private void MarchCubes(PointDistribution pointDistribution, int step)
    {
        _vertices.Clear();
        float[] cubeValues = new float[8];

        int stepCount = 0;
        for (int i = 0; i < pointDistribution.Size - 1; ++i)
        {
            for (int j = 0; j < pointDistribution.Size - 1; ++j)
            {
                for (int k = 0; k < pointDistribution.Size - 1; ++k)
                {
                    cubeValues[0] = pointDistribution.Weights[pointDistribution.GetIndex(i, j, k)];
                    cubeValues[1] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j, k)];
                    cubeValues[2] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j, k + 1)];
                    cubeValues[3] = pointDistribution.Weights[pointDistribution.GetIndex(i, j, k + 1)];
                    cubeValues[4] = pointDistribution.Weights[pointDistribution.GetIndex(i, j + 1, k)];
                    cubeValues[5] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j + 1, k)];
                    cubeValues[6] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j + 1, k + 1)];
                    cubeValues[7] = pointDistribution.Weights[pointDistribution.GetIndex(i, j + 1, k + 1)];

                    MarchCube(cubeValues, pointDistribution.SurfaceLevel, pointDistribution.GetPosition(i, j, k), _vertices);
                    ++stepCount;
                    if (stepCount > _step)
                        return;
                }
            }
        }
    }

    public static void MarchCube(float[] cubeValues, float surfaceLevel, Vector3 point, List<Vector3> vertices, List<GameObject> instances = null, bool useCustomMeshes = true)
    {
        MarchingCubeValues index = (MarchingCubeValues)MarchingCubes.GetLookUpIndex(cubeValues, surfaceLevel);
        if (useCustomMeshes && LookUpTable.HasMesh(index) && instances != null)
        {
            GameObject instance = LookUpTable.GetMesh(index, point + new Vector3(0.5f, 0.5f, 0.5f));

            instances.Add(instance);
            return;
        }

        int[] triagulation = LookUpTable.triangulation[GetLookUpIndex(cubeValues, surfaceLevel)];

        foreach(int edgeIndex in triagulation)
        {
            if (edgeIndex < 0) continue;

            int indexA = LookUpTable.cornerIndexAFromEdge[edgeIndex];
            int indexB = LookUpTable.cornerIndexBFromEdge[edgeIndex];

            Vector3 vertexPos = (point) + ((_corners[indexA] + _corners[indexB]) / 2);

            vertices.Add(vertexPos);
        }
    }

    public static void MarchCube(bool[] cubeValues, Vector3 point, List<Vector3> vertices, List<GameObject> instances = null, bool useCustomMeshes = true)
    {
        MarchingCubeValues index = (MarchingCubeValues)MarchingCubes.GetLookUpIndex(cubeValues);
        if (useCustomMeshes && LookUpTable.HasMesh(index) && instances != null)
        {
            GameObject instance = LookUpTable.GetMesh(index, point);

            instances.Add(instance);
            return;
        }

        int[] triagulation = LookUpTable.triangulation[(int)index];

        foreach (int edgeIndex in triagulation)
        {
            if (edgeIndex < 0) continue;

            int indexA = LookUpTable.cornerIndexAFromEdge[edgeIndex];
            int indexB = LookUpTable.cornerIndexBFromEdge[edgeIndex];

            Vector3 vertexPos = (point) + ((_corners[indexA] + _corners[indexB]) / 2);

            vertices.Add(vertexPos);
        }
    }

    public static int GetLookUpIndex(float[] cubeValues, float surfaceLevel)
    {
        int cubeIndex = 0;
        for(int i = 0; i < 8; ++i)
        {
            if (cubeValues[i] < surfaceLevel)
                cubeIndex |= 1 << i;
        }

        return cubeIndex;
    }

    public static int GetLookUpIndex(bool[] cubeValues)
    {
        int cubeIndex = 0;
        for (int i = 0; i < 8; ++i)
        {
            if (cubeValues[i])
                cubeIndex |= 1 << i;
        }

        return cubeIndex;
    }

    [Button]
    private void GenerateMesh()
    {
        SetupMesh();

        _mesh = new Mesh();
        _mesh.name = "Marching Cubes";

        if (_mesh == null)
            _mesh = _meshFilter.mesh;

        _mesh.vertices = _vertices.ToArray();

        List<int> indices = new List<int>();
        for (int i = 0; i < _vertices.Count; ++i)
            indices.Add(i);

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

        _mesh.triangles = indices.ToArray();
        _mesh.normals = normals.ToArray();

        _meshFilter.mesh = _mesh;
    }

    private void SetupMesh()
    {
        if (_meshFilter != null) return;
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null) _meshFilter = gameObject.AddComponent<MeshFilter>();

        if (_meshRenderer != null) return;
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null) _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = _material;
    }

    private void OnDrawGizmos()
    {
        if (!_drawMesh) return;
        for(int i =0; i < _vertices.Count; i += 3)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(_vertices[i], _vertices[i + 1]);
            Gizmos.DrawLine(_vertices[i + 1], _vertices[i + 2]);
            Gizmos.DrawLine(_vertices[i], _vertices[i + 2]);
        }
    }
}
