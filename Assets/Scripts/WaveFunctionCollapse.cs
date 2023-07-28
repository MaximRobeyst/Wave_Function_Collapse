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

    public Vector3Int Coords;
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

    public void SpawnModule(Vector3 location, List<Module> instances = null)
    {
        var newInstance = GameObject.Instantiate(Module, location, Quaternion.Euler(0, 90 * Rotation, 0));
        Module.SetupSockets();

        if(instances != null)
        {
            instances.Add(newInstance);
        }
    }

    public string GetSlot(SocketDirection direction)
    {
        switch(direction)
        {
            case SocketDirection.Left:
                return GetLeft();
            case SocketDirection.Right:
                return GetRight();
            case SocketDirection.Forward:
                return GetForward();
            case SocketDirection.Backward:
                return GetBackwards();
            case SocketDirection.Up:
                return GetUp();
            case SocketDirection.Down:
                return GetDown();
            default:
                return "";
        }
    }

    public string GetLeft()
    {
        return Module.Sockets[(int)(SocketDirection.Left + (Rotation)) % 4].ToString();
    }

    public string GetForward()
    {
        return Module.Sockets[(int)(SocketDirection.Forward + (Rotation)) % 4].ToString();
    }

    public string GetRight()
    {
        return Module.Sockets[(int)(SocketDirection.Right + (Rotation)) % 4].ToString();
    }

    public string GetBackwards()
    {
        return Module.Sockets[(int)(SocketDirection.Backward + (Rotation)) % 4].ToString();
    }

    public string GetUp()
    {
        return Module.Sockets[(int)SocketDirection.Up].ToString();
    }

    public string GetDown()
    {
        return Module.Sockets[(int)SocketDirection.Down].ToString();
    }

    public bool FitsDirection(SocketDirection direction, ModuleDescriptor moduleDescriptor)
    {
        switch(direction)
        {
            case SocketDirection.Left:
                return FitsLeft(moduleDescriptor);
            case SocketDirection.Right:
                return FitsRight(moduleDescriptor);
            case SocketDirection.Forward:
                return FitsForward(moduleDescriptor);
            case SocketDirection.Backward:
                return FitsBackward(moduleDescriptor);
            case SocketDirection.Up:
                return FitsUp(moduleDescriptor);
            case SocketDirection.Down:
                return FitsDown(moduleDescriptor);
        }
        return false;
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
    public bool FitsUp(ModuleDescriptor modulePosibilities2)
    {
        return GetUp() == modulePosibilities2.GetDown();
    }
    public bool FitsDown(ModuleDescriptor modulePosibilities2)
    {
        return GetDown() == modulePosibilities2.GetUp();
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

    [SerializeField] private Vector3Int _checkCoords;

    [ReadOnly, SerializeField] private bool _isRunning;

    private ModulePosibilities[,,] _modulePosibilities;

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

    int GetIndex(float x, float y, float z)
    {
        return 0;
    }

    private void OnDrawGizmos()
    {
        if (_Is2D)
            DrawGrid2D();
        else
            DrawGrid3D();

        Vector3 position = new Vector3(-_width / 2.0f, -_height / 2.0f, -_depth / 2.0f);

        Gizmos.color = Color.gray;
        for (int x = 0; x < _width; ++x)
        {
            for(int y = 0; y < _height; ++y)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    if (_modulePosibilities == null || _modulePosibilities.Length == 0 || _modulePosibilities[x, y, z] == null) continue;
                    float percentage = _modulePosibilities[x, y, z].Entropy / _modules.Length;
#if UNITY_EDITOR
                    Handles.Label(position + new Vector3(x, y, z) + transform.up, "Entropy: " + percentage);
#endif

                    Gizmos.DrawSphere(position + new Vector3(x, y, z), percentage * 0.25f);
                }
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

        ModulePosibilities posibilities = _modulePosibilities[_checkCoords.x, _checkCoords.y, _checkCoords.z];

        int steps = 0;
        float distance = 1.5f;
        foreach(ModuleDescriptor moduleDescriptor in posibilities.Modules)
        {
            moduleDescriptor.SpawnModule(new Vector3(_width + (steps * distance), 0, 0), _checkInstances);
            steps++;
        }
    }

    [Button]
    void ObserveTile()
    {
        Observe();
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
        _currentState = State.Observe;
        RemovePreviousResult();

        foreach (var instance in _checkInstances)
        {
            if (instance == null) continue;
            DestroyImmediate(instance.gameObject);
        }
        _checkInstances.Clear();
    }

    bool IsWaveFunctionCollapsed()
    {
        return !_modulePosibilities.Cast<ModulePosibilities>().Where(slot => !slot.Collapsed).Any();
    }

    void Observe()
    {
        Vector3 startPosition = new Vector3(-_width / 2.0f, -_height / 2.0f, -_depth / 2.0f);
        var moduleResult = FindLowestEntropy();

        if(moduleResult.Modules.Count == 0)
        {
            return;
        }
        ModuleDescriptor moduleDescriptor = moduleResult.Modules[UnityEngine.Random.Range(0, moduleResult.Modules.Count)];

        moduleDescriptor.SpawnModule(startPosition + moduleResult.Coords, _instances);
        moduleResult.Collapsed = true;
        moduleResult.Entropy = 0;
        moduleResult.Modules.Clear();
        moduleResult.Modules.Add(moduleDescriptor);

        Propogate(moduleResult.Coords, moduleResult);
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
                if (_modulePosibilities[neighbor.Coords.x, neighbor.Coords.y, neighbor.Coords.z].Collapsed)
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

        float time = 2.0f;

        SocketDirection checkDirection = SocketDirection.Left;
        if(modulePosibilities1.Coords.x - modulePosibilities2.Coords.x == 1)
        {
            // Right
            checkDirection = SocketDirection.Left;
            DebugGizmos.DrawSpehere(modulePosibilities2.Coords - new Vector3(_width / 2.0f, _height / 2.0f, _depth / 2.0f), 0.5f, Color.magenta, time);
        }
        if (modulePosibilities1.Coords.x - modulePosibilities2.Coords.x == -1)
        {
            // Left
            checkDirection = SocketDirection.Right;
            DebugGizmos.DrawSpehere(modulePosibilities2.Coords - new Vector3(_width / 2.0f, _height / 2.0f, _depth / 2.0f), 0.5f, Color.blue, time);
        }
        if (modulePosibilities1.Coords.z - modulePosibilities2.Coords.z == 1)
        {
            // Forward
            checkDirection = SocketDirection.Backward;
            DebugGizmos.DrawSpehere(modulePosibilities2.Coords - new Vector3(_width / 2.0f, _height / 2.0f, _depth / 2.0f), 0.5f, Color.green, time);
        }
        if (modulePosibilities1.Coords.z - modulePosibilities2.Coords.z == -1)
        {
            // Backward
            checkDirection = SocketDirection.Forward;
            DebugGizmos.DrawSpehere(modulePosibilities2.Coords - new Vector3(_width / 2.0f, _height / 2.0f, _depth / 2.0f), 0.5f, Color.red, time);
        }
        if (modulePosibilities1.Coords.y - modulePosibilities2.Coords.y == -1)
        {
            // Backward
            checkDirection = SocketDirection.Up;
            DebugGizmos.DrawSpehere(modulePosibilities2.Coords - new Vector3(_width / 2.0f, _height / 2.0f, _depth / 2.0f), 0.5f, Color.red, time);
        }
        if (modulePosibilities1.Coords.y - modulePosibilities2.Coords.y == 1)
        {
            // Backward
            checkDirection = SocketDirection.Down;
            DebugGizmos.DrawSpehere(modulePosibilities2.Coords - new Vector3(_width / 2.0f, _height / 2.0f, _depth / 2.0f), 0.5f, Color.red, time);
        }

        var otherPosibilityTiles = new List<ModuleDescriptor>(modulePosibilities2.Modules);

        var currentPossibleTiles = new List<ModuleDescriptor>(modulePosibilities1.Modules);

        int currentTileId = 0;
        int neighbourTileId = 0;

        foreach(var currentTile in currentPossibleTiles)
        {
            List<ModuleDescriptor> compatibleTiles = new List<ModuleDescriptor>();

            neighbourTileId = 0;
            foreach(var neighborTile in otherPosibilityTiles)
            {
                if (currentTile.FitsDirection(checkDirection, neighborTile))
                    compatibleTiles.Add(neighborTile);

                neighbourTileId++;
            }
            currentTileId++;

            foreach(var tile in compatibleTiles)
            {
                otherPosibilityTiles.Remove(tile);
            }
        }

        if (otherPosibilityTiles.Count > 0)
            changed = true;

        foreach(var tile in otherPosibilityTiles)
        {
            modulePosibilities2.Modules.Remove(tile);
        }

        modulePosibilities2.Entropy = modulePosibilities2.Modules.Count;

        return changed;
    }

    List<ModulePosibilities> GetNeighbors(ModulePosibilities cell)
    {
        List<ModulePosibilities> modulePosibilities = new();

        // LEFT AND RIGHT
        if (cell.Coords.x - 1 >= 0)
            modulePosibilities.Add(_modulePosibilities[cell.Coords.x - 1, cell.Coords.y, cell.Coords.z]);
        if (cell.Coords.x + 1 < _width)
            modulePosibilities.Add(_modulePosibilities[cell.Coords.x + 1, cell.Coords.y, cell.Coords.z]);

        // UP AND DOWN
        if (cell.Coords.y - 1 >= 0)
            modulePosibilities.Add(_modulePosibilities[cell.Coords.x, cell.Coords.y - 1, cell.Coords.z]);
        if (cell.Coords.y + 1 < _height)
            modulePosibilities.Add(_modulePosibilities[cell.Coords.x, cell.Coords.y + 1, cell.Coords.z]);
        
        // FRONT AND BACK
        if (cell.Coords.z - 1 >= 0)
            modulePosibilities.Add(_modulePosibilities[cell.Coords.x, cell.Coords.y , cell.Coords.z - 1]);
        if (cell.Coords.z + 1 < _depth)
            modulePosibilities.Add(_modulePosibilities[cell.Coords.x, cell.Coords.y , cell.Coords.z + 1]);
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
            Observe();

            yield return null;
        }

        _isRunning = false;
    }

    void RemovePreviousResult()
    {
        foreach(Module instance in _instances)
        {
            if (instance == null) continue;
            DestroyImmediate(instance.gameObject);
        }
        _instances.Clear();
    }

    void InitializeAlgorithm()
    {
        RemovePreviousResult();
        if(_Is2D)
            _modulePosibilities = new ModulePosibilities[_width, 1, _depth];
        else
            _modulePosibilities = new ModulePosibilities[_width, _height, _depth];

        for(int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    _modulePosibilities[x,y,z] = new ModulePosibilities();

                    _modulePosibilities[x,y,z].Modules = new();
                    _modulePosibilities[x,y,z].Modules.AddRange(_modules);
                    _modulePosibilities[x,y,z].Entropy = _modules.Length;
                    _modulePosibilities[x,y,z].Collapsed = false;
                    _modulePosibilities[x,y,z].Coords = new Vector3Int(x, y, z);
                }
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
                    Gizmos.DrawWireCube(position + new Vector3(i,j,k), Vector3.one);
                }
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position + _checkCoords, Vector3.one);
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

        Gizmos.DrawLine(position + new Vector3(_checkCoords.x, 0, _checkCoords.z), position + new Vector3(_checkCoords.x, 0, _checkCoords.z + 1));
        Gizmos.DrawLine(position + new Vector3(_checkCoords.x, 0, _checkCoords.z), position + new Vector3(_checkCoords.x + 1, 0, _checkCoords.z));
        Gizmos.DrawLine(position + new Vector3(_checkCoords.x + 1, 0, _checkCoords.z + 1), position + new Vector3(_checkCoords.x + 1, 0, _checkCoords.z));
        Gizmos.DrawLine(position + new Vector3(_checkCoords.x + 1, 0, _checkCoords.z + 1), position + new Vector3(_checkCoords.x, 0, _checkCoords.z + 1));

    }
}
