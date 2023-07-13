using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointDistribution : MonoBehaviour
{
    [SerializeField] private int _Size = 8;
    [SerializeField, Range(0, 1)] private float _surfaceLevel = 0.0f;

    private float[] _weights;

    public float[] Weights => _weights;
    public float SurfaceLevel => _surfaceLevel;
    public int Size => _Size;



    private void OnDrawGizmos()
    {
        Vector3 startPosition = new Vector3(-_Size / 2.0f, -_Size / 2.0f, -_Size / 2.0f);

        for(int i = 0; i < _Size; ++i)
        {
            for(int j = 0; j < _Size; ++j)
            {
                for(int k = 0; k < _Size; ++k)
                {
                    Vector3 newPosition = startPosition + new Vector3(i, j, k);
                    int index = GetIndex(i, j, k);

                    if (index > _weights.Length || _weights[index] < _surfaceLevel) continue;
                    Gizmos.color = new Color(_weights[index], _weights[index], _weights[index]);
                    Gizmos.DrawSphere(newPosition, 0.1f);
                }
            }
        }
    }

    public int GetIndex(int x, int y, int z)
    {
        return (x * _Size * _Size) + (y * _Size) + z;
    }

    public Vector3 GetPosition(int x , int y, int z)
    {
        Vector3 startPosition = new Vector3(-_Size / 2.0f, -_Size / 2.0f, -_Size / 2.0f);
        return startPosition + new Vector3(x, y, z);
    }

    [Button]
    private void GeneratePoints()
    {
        _weights = new float[_Size * _Size * _Size];
        for (int i = 0; i < _weights.Length; ++i)
        {
            _weights[i] = Random.value;
        }
    }
}
