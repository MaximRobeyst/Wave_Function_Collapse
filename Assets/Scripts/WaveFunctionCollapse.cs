using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct ModulePosibilities
{
    List<Module> _modules;
    float entropy;
}

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField] private bool _Is2D = true;

    [SerializeField] private int _width = 16;
    [SerializeField] private int _depth = 16;
    [SerializeField, HideIf(nameof(_Is2D))] private int _height = 16;

    [SerializeField] private Module[] _modules;

    private ModulePosibilities[] _modulePosibilities;

    private void OnDrawGizmos()
    {
        if (_Is2D)
            DrawGrid2D();
        else
            DrawGrid3D();
    }

    [Button]
    void RunAlgorithm()
    {
        _modulePosibilities = new ModulePosibilities[_width * _depth];
    }

    void DrawGrid3D()
    {
        Vector3 position = new Vector3(-_width / 2.0f, -_height / 2.0f, -_depth / 2.0f);

        Gizmos.color = Color.white;
        for (int i = 0; i < _width; ++i)
        {
            for (int j = 0; j < _height; ++j)
            {
                for (int k = 0; k < _depth; ++k)
                {
                    Gizmos.DrawLine(position + new Vector3(i, j, k), position + new Vector3(i, j + 1, k));
                    Gizmos.DrawLine(position + new Vector3(i, j, k), position + new Vector3(i + 1, j, k));
                    Gizmos.DrawLine(position + new Vector3(i, j, k), position + new Vector3(i, j, k+1));

                    Gizmos.DrawLine(position + new Vector3(i + 1, j + 1, k + 1), position + new Vector3(i + 1, j + 1, k));
                    Gizmos.DrawLine(position + new Vector3(i + 1, j + 1, k + 1), position + new Vector3(i + 1, j, k + 1));
                    Gizmos.DrawLine(position + new Vector3(i + 1, j + 1, k + 1), position + new Vector3(i, j + 1, k + 1));

                    Gizmos.DrawLine(position + new Vector3(i, j + 1, k), position + new Vector3(i, j + 1, k + 1));
                    Gizmos.DrawLine(position + new Vector3(i, j + 1, k), position + new Vector3(i+1, j + 1, k));

                    Gizmos.DrawLine(position + new Vector3(i + 1, j, k + 1), position + new Vector3(i + 1, j, k));
                    Gizmos.DrawLine(position + new Vector3(i + 1, j, k + 1), position + new Vector3(i, j, k + 1));

                    Gizmos.DrawLine(position + new Vector3(i + 1, j+1, k), position + new Vector3(i+1, j, k));
                    Gizmos.DrawLine(position + new Vector3(i, j+1, k + 1), position + new Vector3(i, j, k + 1));
                }
            }
        }
    }

    void DrawGrid2D()
    {
        Vector3 position = new Vector3(-_width / 2.0f, 0, -_depth / 2.0f);

        Gizmos.color = Color.white;
        for (int i = 0; i < _width; ++i)
        {
            for (int j = 0; j < _depth; ++j)
            {
                Gizmos.DrawLine(position + new Vector3(i, 0, j), position + new Vector3(i, 0, j+1));
                Gizmos.DrawLine(position + new Vector3(i, 0, j), position + new Vector3(i+1, 0, j));
                Gizmos.DrawLine(position + new Vector3(i+1, 0, j+1), position + new Vector3(i+1, 0, j));
                Gizmos.DrawLine(position + new Vector3(i+1, 0, j+1), position + new Vector3(i, 0, j+1));
            }
        }

    }
}
