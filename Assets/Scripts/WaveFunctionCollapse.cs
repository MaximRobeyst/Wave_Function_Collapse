using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

[System.Serializable]
class ModulePosibilities
{
    public List<ModuleDescriptor> Modules = new();
    public float Entropy = 0.0f;
    public bool Collapsed = false;

    public Vector3 Coords;

    public bool PropogateLeft(ModulePosibilities module)
    {
        bool removed = false;
        for (int i = 0; i < Modules.Count; ++i)
        {
            ModuleDescriptor moduleDescriptor = Modules[i];
            var NonFittingResults = module.Modules.Where(descriptor => descriptor.FitsLeft(moduleDescriptor));
            if (!NonFittingResults.Any()) continue;

            removed = true;
            for (int j = 0; j < NonFittingResults.Count(); ++j)
            {
                module.Modules.Remove(NonFittingResults.ElementAt(j));
            }

            Entropy = Modules.Count;
        }
        return removed;
    }

    public bool PropogateRight(ModulePosibilities module)
    {
        bool removed = false;
        for (int i = 0; i < Modules.Count; ++i)
        {
            ModuleDescriptor moduleDescriptor = Modules[i];
            var NonFittingResults = module.Modules.Where(descriptor => descriptor.FitsRight(moduleDescriptor));
            if (!NonFittingResults.Any()) continue;

            removed = true;
            for (int j = 0; j < NonFittingResults.Count(); ++j)
            {
                module.Modules.Remove(NonFittingResults.ElementAt(j));
            }
            Entropy = Modules.Count;
        }
        return removed;
    }

    public bool PropogateForward(ModulePosibilities module)
    {
        bool removed = false;
        for (int i = 0; i < Modules.Count; ++i)
        {
            ModuleDescriptor moduleDescriptor = Modules[i];
            var NonFittingResults = module.Modules.Where(descriptor => descriptor.FitsForward(moduleDescriptor));
            if (!NonFittingResults.Any()) continue;

            removed = true;
            for (int j = 0; j < NonFittingResults.Count(); ++j)
            {
                module.Modules.Remove(NonFittingResults.ElementAt(j));
            }
            Entropy = Modules.Count;
        }
        return removed;
    }

    public bool PropogateBackward(ModulePosibilities module)
    {
        bool removed = false;
        for (int i = 0; i < Modules.Count; ++i)
        {
            ModuleDescriptor moduleDescriptor = Modules[i];
            var NonFittingResults = module.Modules.Where(descriptor => descriptor.FitsBackward(moduleDescriptor));
            if (!NonFittingResults.Any()) continue;

            removed = true;
            for (int j = 0; j < NonFittingResults.Count(); ++j)
            {
                module.Modules.Remove(NonFittingResults.ElementAt(j));
            }
            Entropy = Modules.Count;
        }

        return removed;

    }
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

    public bool FitsLeft(ModuleDescriptor modulePosibilities2)
    {
        return GetLeft() == modulePosibilities2.GetRight();
    }
    public bool FitsRight(ModuleDescriptor modulePosibilities2)
    {
        return GetRight() == modulePosibilities2.GetLeft();
    }
    public bool FitsForward(ModuleDescriptor modulePosibilities2)
    {
        return GetForward() == modulePosibilities2.GetBackwards();
    }
    public bool FitsBackward(ModuleDescriptor modulePosibilities2)
    {
        return GetBackwards() == modulePosibilities2.GetForward();
    }
}
enum State
{
    Propogate,
    Propogating,
    Observe,
}

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField] private bool _Is2D = true;

    [SerializeField] private int _width = 16;
    [SerializeField] private int _depth = 16;
    [SerializeField, HideIf(nameof(_Is2D))] private int _height = 16;

    [SerializeField] private ModuleDescriptor[] _modules;

    [SerializeField] private Vector2Int _checkCoords;

    [ReadOnly, SerializeField] private bool _isRunning;

    private ModulePosibilities[] _modulePosibilities;

    private List<Module> _instances = new();
    private List<Module> _checkInstances = new();

    private State _currentState = State.Observe;

    private Vector3 _coords;
    private Vector3 _currentPropogateCoords;
    private ModuleDescriptor _moduleDescriptor;

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
                if (_modulePosibilities == null || _modulePosibilities.Length == 0 || _modulePosibilities[GetIndex(i,j)] == null) return;
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
    void CheckTile()
    {
        foreach(var instance in _checkInstances)
        {
            DestroyImmediate(instance.gameObject);
        }
        _checkInstances.Clear();

        ModulePosibilities posibilities = _modulePosibilities[GetIndex(_checkCoords.x, _checkCoords.y)];

        int steps = 0;
        float distance = 1.5f;
        foreach(ModuleDescriptor moduleDescriptor in posibilities.Modules)
        {
            moduleDescriptor.SpawnModule(new Vector3(_width + (steps * distance), 0, 0), _checkInstances);
            steps++;
        }
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
            Observe(ref _coords, ref _moduleDescriptor);
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
        _currentState = State.Observe;
        RemovePreviousResult();

        foreach (var instance in _checkInstances)
        {
            DestroyImmediate(instance.gameObject);
        }
        _checkInstances.Clear();
    }

    bool IsWaveFunctionCollapsed()
    {
        return !_modulePosibilities.Where(slot => !slot.Collapsed).Any();

        //foreach(ModulePosibilities posibilities in _modulePosibilities)
        //{
        //    if (!posibilities.Collapsed)
        //        return false;
        //}
        //
        //return true;
    }

    void Observe(ref Vector3 coords, ref ModuleDescriptor module)
    {
        Vector3 startPosition = new Vector3(-_width / 2.0f, 0, -_depth / 2.0f);
        var moduleResult = FindLowestEntropy();

        if(moduleResult.Modules.Count == 0)
        {
            Debug.LogError("no modules found");
            return;
        }
        ModuleDescriptor moduleDescriptor = moduleResult.Modules[UnityEngine.Random.Range(0, moduleResult.Modules.Count)];
        Debug.Log("Cell " + new Vector2(moduleResult.Coords.x, moduleResult.Coords.z) + " Collapsed");
        Debug.Log("Spawn at: " + (startPosition + moduleResult.Coords + new Vector3(.5f, .0f, .5f)));

        moduleDescriptor.SpawnModule(startPosition + moduleResult.Coords + new Vector3(.5f, .0f, .5f), _instances);
        moduleResult.Collapsed = true;
        moduleResult.Entropy = 0;
        var modulesToRemove = moduleResult.Modules.Where(discriptor => discriptor != moduleDescriptor);

        for (int i = 0; i < modulesToRemove.Count(); ++i)
        {
            moduleResult.Modules.Remove(modulesToRemove.ElementAt(i));
        }
        Propogate(coords, null);

        coords = moduleResult.Coords;
        module = moduleDescriptor;
    }

    /// <summary>
    /// Recursion method to propogate
    /// </summary>
    /// <param name="coords"></param>
    void Propogate(Vector3 coords, ModulePosibilities collapsedCell)
    {
        Stack<ModulePosibilities> changedCells = new();
        changedCells.Push(collapsedCell);



        while(changedCells.Count > 0)
        {
            var currentCell = changedCells.Pop();
            var neighbors = GetNeighbors(currentCell);

            foreach (var neighbor in neighbors)
            {
                //Disregard cells that have already been collapsed
                if (_modulePosibilities[GetIndex(neighbor.Coords.x, neighbor.Coords.z)].Collapsed)
                    continue;

                //Detect the compatibilities and check if something changed
                var somethingChanged = RemoveImpossibleStates(currentCell, neighbor);

                if (somethingChanged)
                {
                    changedCells.Push(neighbor);
                }
            }
        }
    }

    bool RemoveImpossibleStates(ModulePosibilities modulePosibilities1, ModulePosibilities modulePosibilities2)
    {
        var changed = false;

        return changed;
    }

    List<ModulePosibilities> GetNeighbors(ModulePosibilities cell)
    {
        List<ModulePosibilities> modulePosibilities = new();

        if (cell.Coords.x - 1 >= 0)
            modulePosibilities.Add(_modulePosibilities[GetIndex(cell.Coords.x - 1, cell.Coords.z)]);
        if (cell.Coords.x + 1 < _width)
            modulePosibilities.Add(_modulePosibilities[GetIndex(cell.Coords.x + 1, cell.Coords.z)]);
        if (cell.Coords.y - 1 >= 0)
            modulePosibilities.Add(_modulePosibilities[GetIndex(cell.Coords.x, cell.Coords.z - 1)]);
        if (cell.Coords.y + 1 < _depth)
            modulePosibilities.Add(_modulePosibilities[GetIndex(cell.Coords.x, cell.Coords.z + 1)]);
        return modulePosibilities;
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

        Vector3 coords = new Vector3();
        ModuleDescriptor module = null;

        while (!IsWaveFunctionCollapsed())
        {
            Observe(ref coords, ref module);

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

        for(int i = 0; i < _width; ++i)
        {
            for (int j = 0; j < _depth; ++j)
            {
                int index = GetIndex(i, j);
                _modulePosibilities[index] = new ModulePosibilities();

                _modulePosibilities[index].Modules = new();
                _modulePosibilities[index].Modules.AddRange(_modules);
                _modulePosibilities[index].Entropy = _modules.Length;
                _modulePosibilities[index].Collapsed = false;
                _modulePosibilities[index].Coords = new Vector3(i, 0, j);
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


        Gizmos.color = Color.red;

        Gizmos.DrawLine(position + new Vector3(_checkCoords.x, 0, _checkCoords.y), position + new Vector3(_checkCoords.x, 0, _checkCoords.y + 1));
        Gizmos.DrawLine(position + new Vector3(_checkCoords.x, 0, _checkCoords.y), position + new Vector3(_checkCoords.x + 1, 0, _checkCoords.y));
        Gizmos.DrawLine(position + new Vector3(_checkCoords.x + 1, 0, _checkCoords.y + 1), position + new Vector3(_checkCoords.x + 1, 0, _checkCoords.y));
        Gizmos.DrawLine(position + new Vector3(_checkCoords.x + 1, 0, _checkCoords.y + 1), position + new Vector3(_checkCoords.x, 0, _checkCoords.y + 1));

    }
}
