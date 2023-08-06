using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

[System.Serializable]
public class MarchingCubeWFCPosibilities
{
    public List<MarchingCubeMeshes> Modules = new();
    public float Entropy = 0.0f;
    public bool Collapsed = false;

    public Vector3Int Coords;

    public void SpawnModule(Vector3 position, int index, List<GameObject> instances = null)
    {
        MarchingCubeMeshes mesh = Modules[index];

        GameObject newGameobject = new GameObject("Cube_" + mesh.MarchingCubeValue);
        GameObject instance = GameObject.Instantiate(mesh.Mesh, Vector3.zero, Quaternion.identity, newGameobject.transform);

        newGameobject.transform.position = position;
        newGameobject.transform.localScale = new Vector3(
            (mesh.Flipped & FlipValues.FlipX) != 0 ? -1.0f : 1.0f,
            (mesh.Flipped & FlipValues.FlipY) != 0 ? -1.0f : 1.0f,
            (mesh.Flipped & FlipValues.FlipZ) != 0 ? -1.0f : 1.0f);

        instance.transform.localRotation = Quaternion.Euler(0.0f, mesh.RotationIndex * 90.0f, 0.0f);

        MarchingCubeDescriptor marchingCubeDescriptor = instance.AddComponent<MarchingCubeDescriptor>();
        marchingCubeDescriptor.MarchingCubeMesh = new MarchingCubeMeshes();
        marchingCubeDescriptor.MarchingCubeMesh.MarchingCubeValues = (MarchingCubeValues)mesh.MarchingCubeValues;
        marchingCubeDescriptor.MarchingCubeMesh.MarchingCubeValue = mesh.MarchingCubeValue;
        marchingCubeDescriptor.MarchingCubeMesh.Flipped = mesh.Flipped;
        marchingCubeDescriptor.MarchingCubeMesh.RotationIndex = mesh.RotationIndex;

        instances.Add(newGameobject);
    }
}

public class WaveFunctionCollapseData : MonoBehaviour
{
    private MarchingCubeWFCPosibilities[,,] _modulePosibilities;

    private int _width;
    private int _height;
    private int _depth;

    public bool DebugData = true;

    [SerializeField]private Vector3Int _checkCoords;

    private List<GameObject> _checkInstances = new();

    public MarchingCubeWFCPosibilities[,,] WaveFunctionCollapsePosibilities => _modulePosibilities;

    public int Width => _width;
    public int Height => _height;
    public int Depth => _depth;

    public void SetupData(int width, int height, int depth, PointDistribution pointDistribution)
    {
        _modulePosibilities = new MarchingCubeWFCPosibilities[width, height, depth];

        _width = width;
        _height = height;
        _depth = depth;

        float[] cubeValues = new float[8];
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                for (int k = 0; k < depth; ++k)
                {
                    cubeValues[0] = pointDistribution.Weights[pointDistribution.GetIndex(i, j, k)];
                    cubeValues[1] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j, k)];
                    cubeValues[2] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j, k + 1)];
                    cubeValues[3] = pointDistribution.Weights[pointDistribution.GetIndex(i, j, k + 1)];
                    cubeValues[4] = pointDistribution.Weights[pointDistribution.GetIndex(i, j + 1, k)];
                    cubeValues[5] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j + 1, k)];
                    cubeValues[6] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1, j + 1, k + 1)];
                    cubeValues[7] = pointDistribution.Weights[pointDistribution.GetIndex(i, j + 1, k + 1)];

                    if (!DebugData) continue;

                    Vector3 point = pointDistribution.GetPosition(i, j, k);
                    MarchingCubeValues index = (MarchingCubeValues)MarchingCubes.GetLookUpIndex(cubeValues, pointDistribution.SurfaceLevel);
                    if (LookUpTable.HasMesh(index))
                    {
                        var instances = LookUpTable.GetMeshes(index);
                        _modulePosibilities[i, j, k] = new();
                        _modulePosibilities[i, j, k].Modules.AddRange(instances);
                        _modulePosibilities[i, j, k].Entropy = _modulePosibilities[i,j,k].Modules.Count;
                        _modulePosibilities[i, j, k].Collapsed = false;

                        _modulePosibilities[i, j, k].Coords = new Vector3Int(i, j, k);
                    }
                 }
            }
        }

    }

    private void OnDrawGizmos()
    {
        Vector3 position = new Vector3(-_width / 2.0f, -_height / 2.0f, -_depth / 2.0f);

        Gizmos.color = Color.gray;
        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    if (_modulePosibilities == null || _modulePosibilities.Length == 0 ||
                        _modulePosibilities[x, y, z] == null || _modulePosibilities[x,y,z].Collapsed) 
                        continue;
                    float percentage = _modulePosibilities[x, y, z].Modules.Count;

                    Gizmos.DrawSphere(position + new Vector3(x, y, z) , percentage * 0.25f);
                }
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position + new Vector3(_checkCoords.x, _checkCoords.y, _checkCoords.z), Vector3.one);
    }

    [Button]
    void CheckTile()
    {
        foreach (var instance in _checkInstances)
        {
            DestroyImmediate(instance.gameObject);
        }
        _checkInstances.Clear();

        MarchingCubeWFCPosibilities posibilities = _modulePosibilities[_checkCoords.x, _checkCoords.y, _checkCoords.z];

        int steps = 0;
        float distance = 1.5f;
        for (int i = 0; i < posibilities.Modules.Count; ++i)
        {
            posibilities.SpawnModule(new Vector3(_width + (steps * distance), 0, 0), i, _checkInstances);
            steps++;
        }
    }
}
