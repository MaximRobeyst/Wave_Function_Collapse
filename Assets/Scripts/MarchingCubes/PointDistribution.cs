using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointDistribution : MonoBehaviour
{
    [SerializeField] private bool _Is2D = false;

    [SerializeField] private int _Size = 8;
    [SerializeField] private int _step = 0;
    [SerializeField, Range(0, 1)] private float _surfaceLevel = 0.0f;
    [SerializeField] private Material _material;

    private float[] _weights;

    public float[] Weights => _weights;
    public float SurfaceLevel => _surfaceLevel;
    public int Size => _Size;

    private void OnDrawGizmos()
    {
        if (_weights == null || _weights.Length == 0) return;
        Vector3 startPosition = new Vector3(-_Size / 2.0f, -_Size / 2.0f, -_Size / 2.0f);

        int step = 0;
        for (int i = 0; i < _Size; ++i)
        {
            for(int j = 0; j < _Size; ++j)
            {
                for(int k = 0; k < _Size; ++k)
                {
                    Vector3 newPosition = startPosition + new Vector3(i, j, k);
                    int index = GetIndex(i, j, k);

                    if (_step == step)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(GetPosition(i, j, k) + (Vector3.one / 2.0f), Vector3.one);
                    }
                    ++step;

                    if (index > _weights.Length || _weights[index] < _surfaceLevel) continue;
                    Gizmos.color = new Color(_weights[index], _weights[index], _weights[index]);
                    Gizmos.DrawSphere(newPosition, 0.1f);
                }
            }
        }
    }

    public int GetIndex(int x, int y, int z)
    {
        if (_Is2D)
            return (y * _Size) + x;
        else
            return (z * _Size * _Size) + (y * _Size) + x;
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
