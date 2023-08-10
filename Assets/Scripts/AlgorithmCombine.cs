using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class MarcingCubeModuleDescriptor
{
    [ShowAssetPreview]
    MarchingCubeModule Module;
}

public class AlgorithmCombine : MonoBehaviour
{
    [SerializeField] private int _size = 16;
    [SerializeField] private List<MeshTable> _lookupTable = new();

    [Button]
    public void SetupPosibilities()
    {
        WaveFunctionCollapseData data = GetComponent<WaveFunctionCollapseData>();
        PointDistribution pointDistribution = GetComponent<PointDistribution>();

        data.DebugData = true;

        pointDistribution.SetupPointDistribution(_size+1);
        data.SetupData(_size, _size, _size, pointDistribution);
    }
}
