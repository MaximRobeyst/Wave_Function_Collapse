using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarchingCubeWaveFunctionCollapse : MonoBehaviour
{
    private WaveFunctionCollapseData _waveFunctionCollapseData;

    public WaveFunctionCollapseData WaveFunctionCollapseData
    {
        get
        {
            if (_waveFunctionCollapseData == null)
                _waveFunctionCollapseData = GetComponent<WaveFunctionCollapseData>();
            return _waveFunctionCollapseData;
        }
    }

    private MarchingCubeWFCPosibilities[,,] _modulePosibilities;
    [ReadOnly, SerializeField] private bool _isRunning;

    private int _width;
    private int _height;
    private int _depth;

    private List<GameObject> _instances = new();

    [Button]
    public void RunAlgorithm()
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

    IEnumerator Alogirthm()
    {
        Clear();
        _isRunning = true;
        WaveFunctionCollapseData.DebugData = false;
        InitializeAlgorithm();

        while (!IsWaveFunctionCollapsed())
        {
            Observe();

            yield return null;
        }

        _isRunning = false;
    }

    void InitializeAlgorithm()
    {
        AlgorithmCombine combine = GetComponent<AlgorithmCombine>();
        if (combine != null)
            combine.SetupPosibilities();

        _modulePosibilities = WaveFunctionCollapseData.WaveFunctionCollapsePosibilities;

        _width = WaveFunctionCollapseData.Width;
        _height = WaveFunctionCollapseData.Height;
        _depth = WaveFunctionCollapseData.Depth;
    }

    bool IsWaveFunctionCollapsed()
    {
        return !_modulePosibilities.Cast<MarchingCubeWFCPosibilities>().Where(slot => !slot.Collapsed).Any();
    }

    void Observe()
    {
        Vector3 startPosition = new Vector3(-_width / 2.0f, -_height / 2.0f, -_depth / 2.0f);
        var moduleResult = FindLowestEntropy();

        if (moduleResult == null || moduleResult.Modules.Count == 0)
        {
            return;
        }
        int index = UnityEngine.Random.Range(0, moduleResult.Modules.Count);

        MarchingCubeMeshes marchingCubeMesh = moduleResult.Modules[index];

        moduleResult.SpawnModule(startPosition + moduleResult.Coords, index, _instances);
        moduleResult.Collapsed = true;
        moduleResult.Entropy = 0;
        moduleResult.Modules.Clear();
        moduleResult.Modules.Add(marchingCubeMesh);

        Propogate(moduleResult.Coords, moduleResult);
    }
    /// <summary>
    /// Recursion method to propogate
    /// </summary>
    /// <param name="coords"></param>
    void Propogate(Vector3 coords, MarchingCubeWFCPosibilities collapsedCell)
    {
        Stack<MarchingCubeWFCPosibilities> changedCells = new();
        changedCells.Push(collapsedCell);

        while (changedCells.Count > 0)
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

    bool RemoveImpossibleStates(MarchingCubeWFCPosibilities modulePosibilities1, MarchingCubeWFCPosibilities modulePosibilities2)
    {
        // TODO: Finish this 

        var changed = false;
        
        float time = 2.0f;
        
        SocketDirection checkDirection = SocketDirection.Left;
        if (modulePosibilities1.Coords.x - modulePosibilities2.Coords.x == 1)
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
        
        var otherPosibilityTiles = new List<MarchingCubeMeshes>(modulePosibilities2.Modules);
        
        var currentPossibleTiles = new List<MarchingCubeMeshes>(modulePosibilities1.Modules);
        
        int currentTileId = 0;
        int neighbourTileId = 0;
        
        foreach (var currentTile in currentPossibleTiles)
        {
            List<MarchingCubeMeshes> compatibleTiles = new List<MarchingCubeMeshes>();
        
            neighbourTileId = 0;
            foreach (var neighborTile in otherPosibilityTiles)
            {
                if (currentTile.FitsDirection(checkDirection, neighborTile))
                    compatibleTiles.Add(neighborTile);
        
                neighbourTileId++;
            }
            currentTileId++;
        
            foreach (var tile in compatibleTiles)
            {
                otherPosibilityTiles.Remove(tile);
            }
        }
        
        if (otherPosibilityTiles.Count > 0)
            changed = true;
        
        foreach (var tile in otherPosibilityTiles)
        {
            modulePosibilities2.Modules.Remove(tile);
        }


        if (modulePosibilities2.Modules.Count == 0)
        {
            Debug.Log("all modules were removed");
            Debug.Log("Which were: ");

            foreach (var tile in otherPosibilityTiles)
            {
                Debug.Log(tile.Mesh.name, tile.Mesh);
            }

            Debug.Log("Removed all possiblities with placemet of: " + modulePosibilities1.Coords);

            string modules = "which had these modules: ";
            for (int i =0;i < modulePosibilities1.Modules.Count; ++i)
            {
                modules += $"{modulePosibilities1.Modules[i].Mesh.name}, ";
            }
            Debug.Log(modules);
        }


        modulePosibilities2.Entropy = modulePosibilities2.Modules.Count;
        
        return changed;

        //return false;
    }

    List<MarchingCubeWFCPosibilities> GetNeighbors(MarchingCubeWFCPosibilities cell)
    {
        List<MarchingCubeWFCPosibilities> modulePosibilities = new();

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
            modulePosibilities.Add(_modulePosibilities[cell.Coords.x, cell.Coords.y, cell.Coords.z - 1]);
        if (cell.Coords.z + 1 < _depth)
            modulePosibilities.Add(_modulePosibilities[cell.Coords.x, cell.Coords.y, cell.Coords.z + 1]);
        return modulePosibilities;
    }

    MarchingCubeWFCPosibilities FindLowestEntropy()
    {
        float lowestEntropy = float.MaxValue;
        List<MarchingCubeWFCPosibilities> list = new();
        foreach (MarchingCubeWFCPosibilities posibilities in _modulePosibilities)
        {
            if (posibilities.Entropy > lowestEntropy || posibilities.Collapsed || posibilities.Modules.Count == 0) continue;

            if (lowestEntropy != posibilities.Entropy && list.Count != 0)
                list.Clear();

            lowestEntropy = posibilities.Entropy;
            list.Add(posibilities);
        }

        if (list.Count == 0)
        {
            Debug.LogError("no posibilites found");
            return null;
        }

        return list[Random.Range(0, list.Count)];
    }

    private void OnDrawGizmos()
    {
        if (!_isRunning) return;
        Vector3 position = new Vector3(-_width / 2.0f, -_height / 2.0f, -_depth / 2.0f);

        Gizmos.color = Color.gray;
        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                for (int z = 0; z < _depth; ++z)
                {
                    if (_modulePosibilities == null || _modulePosibilities.Length == 0 ||
                        _modulePosibilities[x, y, z] == null || _modulePosibilities[x, y, z].Collapsed)
                        continue;
                    float percentage = _modulePosibilities[x, y, z].Modules.Count;

                    Gizmos.DrawSphere(position + new Vector3(x, y, z), percentage * 0.25f);
                }
            }
        }
    }

    [Button]
    private void StopAlgorithm()
    {
        StopAllCoroutines();
        _isRunning = false;
    }

    [Button]
    public void Clear()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if(meshRenderer != null)
            meshRenderer.enabled = false;

        foreach(GameObject instance in _instances)
        {
            DestroyImmediate(instance);
        }
        _instances.Clear();
    }
}
