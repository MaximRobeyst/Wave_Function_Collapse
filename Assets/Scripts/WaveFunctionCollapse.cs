using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Random = UnityEngine.Random;

[System.Serializable]
class ModulePosibilities
{
    public List<ModuleDescriptor> Modules = new();
    public float Entropy = 0.0f;
    public bool Collapsed = false;

    public Vector3 Coords;
}

//[System.Serializable]
//class PossibleNeighbour
//{
//    public Vector3 Direction;
//    public Module[] Neighbours;
//}

[System.Serializable]
class ModuleDescriptor
{
    [ShowAssetPreview]
    public Module Module;
    public int Rotation = 0;

    //public List<PossibleNeighbour> PossibleNeighbours;

    public void SpawnModule(Vector3 location, List<Module> instances = null)
    {
        var newInstance = GameObject.Instantiate(Module, location, Quaternion.Euler(0, 90 * Rotation, 0));

        if(instances != null)
        {
            instances.Add(newInstance);
        }
    }

    public string GetLeft()
    {
        return Module.Sockets[(int)(SocketDirection.Left + Rotation) % 4].Name;
    }

    public string GetForward()
    {
        //Debug.Log("GetForward() Socket ID: " + (int)(SocketDirection.Forward + Rotation) % 4);

        return Module.Sockets[(int)(SocketDirection.Forward + Rotation) % 4].Name;
    }

    public string GetRight()
    {
        return Module.Sockets[(int)(SocketDirection.Right + Rotation) % 4].Name;
    }

    public string GetBackwards()
    {
        return Module.Sockets[(int)(SocketDirection.Backward + Rotation) % 4].Name;
    }
}

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField] private bool _Is2D = true;

    [SerializeField] private int _width = 16;
    [SerializeField] private int _depth = 16;
    [SerializeField, HideIf(nameof(_Is2D))] private int _height = 16;

    [SerializeField] private ModuleDescriptor[] _modules;

    [ReadOnly, SerializeField] private bool _isRunning;

    private ModulePosibilities[] _modulePosibilities;

    private List<Module> _instances = new();

    int GetIndex(float x, float y)
    {
        return (int)(x * _width + y);
    }

    private void OnDrawGizmos()
    {
        if (_Is2D)
            DrawGrid2D();
        else
            DrawGrid3D();

        Vector3 position = new Vector3(-_width / 2.0f, 0, -_depth / 2.0f);

        Gizmos.color = Color.gray;
        for (int i = 0; i < _width; ++i)
        {
            for (int j = 0; j < _depth; ++j)
            {
                if (_modulePosibilities.Length == 0 || _modulePosibilities[GetIndex(i,j)] == null) return;
                float percentage = _modulePosibilities[GetIndex(i,j)].Entropy / _modules.Length;
#if UNITY_EDITOR
                Handles.Label(position + new Vector3(i + .5f, 0, j + .5f) + transform.up, "Entropy: " + percentage);
#endif

                Gizmos.DrawSphere(position + new Vector3(i + .5f, 0, j +.5f), percentage * 0.25f);
            }
        }
    }

    [Button]
    void RunAlgorithm()
    {
        StartCoroutine(Alogirthm());
    }

    [Button]
    void RunStep()
    {
        if (!_isRunning)
        {
            InitializeAlgorithm();
            _isRunning = true;
        }

        if (!IsWaveFunctionCollapsed())
        {
            Observe();
        }
    }

    [Button]
    void StopRunning()
    {
        StopAllCoroutines();
        _isRunning = false;
    }

    [Button]
    void Clear()
    {
        RemovePreviousResult();
    }

    bool IsWaveFunctionCollapsed()
    {
        foreach(ModulePosibilities posibilities in _modulePosibilities)
        {
            if (!posibilities.Collapsed)
                return false;
        }

        return true;
    }

    void Observe()
    {
        Vector3 startPosition = new Vector3(-_width / 2.0f, 0, -_depth / 2.0f);
        var moduleResult = FindLowestEntropy();

        var module = moduleResult.Modules[UnityEngine.Random.Range(0, moduleResult.Modules.Count)];
        Debug.Log("Cell " + new Vector2(moduleResult.Coords.x, moduleResult.Coords.z) + " Collapsed");
        Debug.Log("Spawn at: " + (startPosition + moduleResult.Coords + new Vector3(.5f, .0f, .5f)));

        module.SpawnModule(startPosition + moduleResult.Coords + new Vector3(.5f, .0f, .5f), _instances);
        moduleResult.Collapsed = true;
        moduleResult.Modules.Clear();
        moduleResult.Entropy = 0;

        Propogate(moduleResult.Coords, module);
    }

    /// <summary>
    /// Recursion method to propogate
    /// </summary>
    /// <param name="coords"></param>
    void Propogate(Vector3 coords, ModuleDescriptor chosenModule)
    {
        for (int y = (int)coords.z; y < _depth - coords.z; ++y)
        {
            string forward = chosenModule.GetForward();
            ModulePosibilities posibilities = _modulePosibilities[GetIndex(coords.x, y)];

            for (int descriptor = 0; descriptor < posibilities.Modules.Count; ++descriptor)
            {
                if (posibilities.Modules[descriptor].GetBackwards() != forward)
                {
                    int index = GetIndex(coords.x, y); //(int)(coords.x * _width + y);
                    _modulePosibilities[index].Modules.Remove(posibilities.Modules[descriptor]);
                    _modulePosibilities[index].Entropy = _modulePosibilities[index].Modules.Count;
                }
            }
        }

        for (int y = (int)coords.z; y > 0; --y)
        {
            string backward = chosenModule.GetBackwards();
            ModulePosibilities posibilities = _modulePosibilities[GetIndex(coords.x, y)];

            for (int descriptor = 0; descriptor < posibilities.Modules.Count; ++descriptor)
            {
                if (posibilities.Modules[descriptor].GetForward() != backward)
                {
                    int index = GetIndex(coords.x, y); // //(int)(coords.x * _width + y);

                    _modulePosibilities[index].Modules.Remove(posibilities.Modules[descriptor]);
                    _modulePosibilities[index].Entropy = _modulePosibilities[index].Modules.Count;
                }
            }
        }

        for (int x = (int)coords.x; x < _width - coords.x; ++x)
        {
            string right = chosenModule.GetRight();
            ModulePosibilities posibilities = _modulePosibilities[GetIndex(x, coords.z)];

            for (int descriptor = 0; descriptor < posibilities.Modules.Count; ++descriptor)
            {
                if (posibilities.Modules[descriptor].GetLeft() != right)
                {
                    int index = GetIndex(x, coords.z); // //(int)(x * _width + coords.y);

                    _modulePosibilities[index].Modules.Remove(posibilities.Modules[descriptor]);
                    _modulePosibilities[index].Entropy = _modulePosibilities[index].Modules.Count;
                }
            }
        }

        for (int x = (int)coords.x; x > 0; --x)
        {
            string left = chosenModule.GetLeft();
            ModulePosibilities posibilities = _modulePosibilities[GetIndex(x, coords.z)];

            for (int descriptor = 0; descriptor < posibilities.Modules.Count; ++descriptor)
            {
                if (posibilities.Modules[descriptor].GetLeft() != left)
                {
                    int index = GetIndex(x, coords.z); //(int)(x * _width + coords.y);

                    _modulePosibilities[index].Modules.Remove(posibilities.Modules[descriptor]);
                    _modulePosibilities[index].Entropy = _modulePosibilities[index].Modules.Count;
                }
            }
        }

    }

    ModulePosibilities FindLowestEntropy()
    {
        float lowestEntropy = float.MaxValue;
        List<ModulePosibilities> list = new();
        foreach (ModulePosibilities posibilities in _modulePosibilities)
        {
            if (posibilities.Entropy > lowestEntropy || posibilities.Collapsed) continue;

            if (lowestEntropy != posibilities.Entropy && list.Count != 0)
                list.Clear();

            lowestEntropy = posibilities.Entropy;
            list.Add(posibilities);
        }

        if (list.Count == 0)
        {
            Debug.LogError("no posibilites found");
            return new ModulePosibilities();
        }

        return list[Random.Range(0, list.Count)];
    }


    IEnumerator Alogirthm()
    {
        _isRunning = true;
        InitializeAlgorithm();
        while (!IsWaveFunctionCollapsed())
        {
            Vector3 coordChanged;
            Observe();

            yield return null;
        }

        _isRunning = false;
    }

    void RemovePreviousResult()
    {
        foreach(Module instance in _instances)
        {
            DestroyImmediate(instance.gameObject);
        }
        _instances.Clear();
    }

    void InitializeAlgorithm()
    {
        RemovePreviousResult();
        _modulePosibilities = new ModulePosibilities[_width * _depth];

        for(int i = 0; i < _depth; ++i)
        {
            for (int j = 0; j < _width; ++j)
            {
                _modulePosibilities[i * _width + j] = new ModulePosibilities();

                _modulePosibilities[i * _width + j].Modules = new();
                _modulePosibilities[i * _width + j].Modules.AddRange(_modules);
                _modulePosibilities[i * _width + j].Entropy = _modules.Length;
                _modulePosibilities[i * _width + j].Collapsed = false;
                _modulePosibilities[i * _width + j].Coords = new Vector3(j, 0, i);
            }
        }
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
                    Gizmos.DrawWireCube(position, Vector3.one);
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
