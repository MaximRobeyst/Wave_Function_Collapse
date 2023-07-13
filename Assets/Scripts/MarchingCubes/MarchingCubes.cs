using EasyButtons.Editor;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    [SerializeField] private int _step;

    List<Vector3> vertices = new List<Vector3>();

    private Vector3[] _corners = new Vector3[8]
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

    private void MarchCubes(PointDistribution pointDistribution)
    {
        vertices.Clear();
        float[] cubeValues = new float[8];
        for(int i = 0; i < pointDistribution.Size - 1; ++i)
        {
            for (int j = 0; j < pointDistribution.Size - 1; ++j)
            {
                for (int k = 0; k < pointDistribution.Size - 1; ++k)
                {
                    cubeValues[0] = pointDistribution.Weights[pointDistribution.GetIndex(i     , j     , k     )];
                    cubeValues[1] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1 , j     , k     )];
                    cubeValues[2] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1 , j     , k + 1 )];
                    cubeValues[3] = pointDistribution.Weights[pointDistribution.GetIndex(i     , j     , k + 1 )];
                    cubeValues[4] = pointDistribution.Weights[pointDistribution.GetIndex(i     , j + 1 , k     )];
                    cubeValues[5] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1 , j + 1 , k     )];
                    cubeValues[6] = pointDistribution.Weights[pointDistribution.GetIndex(i + 1 , j + 1 , k + 1 )];
                    cubeValues[7] = pointDistribution.Weights[pointDistribution.GetIndex(i     , j + 1 , k + 1 )];

                    MarchCube(cubeValues, pointDistribution.SurfaceLevel, pointDistribution.GetPosition(i,j,k), vertices);
                }
            }
        }
    }

    private void MarchCubes(PointDistribution pointDistribution, int step)
    {
        vertices.Clear();
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

                    MarchCube(cubeValues, pointDistribution.SurfaceLevel, pointDistribution.GetPosition(i, j, k), vertices);
                    ++stepCount;
                    if (stepCount > _step)
                        return;
                }
            }
        }
    }

    void MarchCube(float[] cubeValues, float surfaceLevel, Vector3 point, List<Vector3> vertices)
    {
        int[] triagulation = LookUpTable.triangulation[GetLookUpIndex(cubeValues, surfaceLevel)];

        foreach(int edgeIndex in triagulation)
        {
            if (edgeIndex < 0) continue;

            int indexA = LookUpTable.cornerIndexAFromEdge[edgeIndex];
            int indexB = LookUpTable.cornerIndexBFromEdge[edgeIndex];

            Vector3 vertexPos = point + (_corners[indexA] + _corners[indexB]);

            vertices.Add(vertexPos);
        }
    }

    private int GetLookUpIndex(float[] cubeValues, float surfaceLevel)
    {
        int cubeIndex = 0;
        for(int i = 0; i < 8; ++i)
        {
            if (cubeValues[i] < surfaceLevel)
                cubeIndex |= 1 << i;
        }

        return cubeIndex;
    }

    private void OnDrawGizmos()
    {
        for(int i =0; i < vertices.Count; i += 3)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(vertices[i], vertices[i + 1]);
            Gizmos.DrawLine(vertices[i + 1], vertices[i + 2]);
            Gizmos.DrawLine(vertices[i], vertices[i + 2]);
        }
    }
}
