using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        PointDistribution pointDistribution = data.GetComponent<PointDistribution>();

        data.DebugData = true;
        if (pointDistribution.Weights == null || pointDistribution.Weights.Length <= 0) pointDistribution.GeneratePoints();

        data.SetupData(pointDistribution.Size-1, pointDistribution.Size-1, pointDistribution.Size - 1, pointDistribution);
    }
}
