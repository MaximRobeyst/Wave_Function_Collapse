using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MarchingCubeModule : MonoBehaviour
{
    [SerializeField, OnValueChanged(nameof(UpdateCube))] private bool[] _points = new bool[8];

    [ReadOnly, SerializeField] private List<int> _indices = new();
    [ReadOnly, SerializeField] private int _currentIndex;

    [SerializeField] private bool _flipX = true;
    [SerializeField] private bool _flipY = true;
    [SerializeField] private bool _flipZ = true;
    [SerializeField] private MarchingCubeModule _prefab;
    [SerializeField] private MarchingCubeMeshes _mesh;

    [SerializeField, OnValueChanged(nameof(ChangeDrawInfo))] private bool _drawPoints = true;

    private static bool _staticDrawPoints = false;
    private Module _module;

    public MarchingCubeModule MarchingCubePrefab 
    {
        get => _prefab;
        set => _prefab = value;
    }

    public Module Module
    {
        get
        {
            if (_module == null) _module = GetComponent<Module>();
            return _module;
        }
    }

    [Button]
    void ConfigurePoints()
    {
        _indices.Clear();

        _mesh.MarchingCubeValues = (MarchingCubeValues)MarchingCubes.GetLookUpIndex(_points);

        for(int i =0;i < 4; ++i)
        {
            int index = MarchingCubes.GetLookUpIndex(RotatePoints(i));

            if(!_indices.Contains(index))
                _indices.Add(index);

            for (int flipIndex = 0; flipIndex <= (int)FlipValues.All; ++flipIndex)
            {
                bool[] result = RotatePoints(i);
                if (_flipX && (flipIndex & (int)FlipValues.FlipX) != 0)
                    result = FlipPointsX(result);
                if (_flipY && (flipIndex & (int)FlipValues.FlipY) != 0)
                    result = FlipPointsY(result);
                if (_flipZ && (flipIndex & (int)FlipValues.FlipZ) != 0)
                    result = FlipPointsZ(result);

                int flippedIndex = MarchingCubes.GetLookUpIndex(result);

                if (!_indices.Contains(flippedIndex))
                    _indices.Add(flippedIndex);
            }
        }

    }

    private void ChangeDrawInfo()
    {
        _staticDrawPoints = _drawPoints;
    }

    public void SetIndex(int index)
    {
        _currentIndex = index;
        _points = GetPoints(index);
    }

    [Button]
    public void AddToTable()
    {
        _indices.Clear();
        for (int i = 0; i < 4; ++i)
        {
            int index = MarchingCubes.GetLookUpIndex(RotatePoints(i));


            if (!_indices.Contains(index))
                _indices.Add(index);

            MarchingCubeMeshes marchingCubeMeshes = new();
            marchingCubeMeshes.MarchingCubeValues = (MarchingCubeValues)index;
            marchingCubeMeshes.MarchingCubeValue = index;
            marchingCubeMeshes.Mesh = _prefab;
            marchingCubeMeshes.Flipped = FlipValues.None;
            marchingCubeMeshes.RotationIndex = i;

            if (!MeshTable.Instance.Contains((MarchingCubeValues)index, _prefab))
            {
                MeshTable.Instance.Values.Add(marchingCubeMeshes);
            }


            for (int flipIndex = 0; flipIndex <= (int)FlipValues.All; ++flipIndex)
            {
                bool[] result = RotatePoints(i);
                if (_flipX && (flipIndex & (int)FlipValues.FlipX) != 0)
                    result = FlipPointsX(result);
                if (_flipY && (flipIndex & (int)FlipValues.FlipY) != 0)
                    result = FlipPointsY(result);
                if (_flipZ && (flipIndex & (int)FlipValues.FlipZ) != 0)
                    result = FlipPointsZ(result);

                int flippedIndex = MarchingCubes.GetLookUpIndex(result);

                if (!_indices.Contains(flippedIndex))
                    _indices.Add(flippedIndex);

                MarchingCubeMeshes flippedMarchingCubeMeshes = new();
                flippedMarchingCubeMeshes.MarchingCubeValues = (MarchingCubeValues)flippedIndex;
                flippedMarchingCubeMeshes.MarchingCubeValue = flippedIndex;
                flippedMarchingCubeMeshes.Mesh = _prefab;
                flippedMarchingCubeMeshes.Flipped = (FlipValues)flipIndex;
                flippedMarchingCubeMeshes.RotationIndex = i;

                if (!MeshTable.Instance.Contains((MarchingCubeValues)flippedIndex, _prefab))
                {
                    MeshTable.Instance.Values.Add(flippedMarchingCubeMeshes);
                }

            }
        }
    }

    void UpdateCube()
    {
        _currentIndex = MarchingCubes.GetLookUpIndex(_points);
    }

    bool[] RotatePoints(int rotateIndex)
    {
        bool[] points = new bool[_points.Length];

        for (int i = 0; i < 4; ++i)
        {
            points[i] = _points[(i + rotateIndex) % 4];
            points[4 + i] = _points[4 + ((i + rotateIndex) % 4)];
        }

        return points;
    }

    public static bool[] RotatePoints(bool[] points, int rotateIndex)
    {
        bool[] newPoints = new bool[points.Length];

        for (int i = 0; i < 4; ++i)
        {
            newPoints[i] = points[(i + rotateIndex) % 4];
            newPoints[4 + i] = points[4 + ((i + rotateIndex) % 4)];
        }

        return newPoints;
    }

    public static bool[] FlipPointsX(bool[] oldPoints)
    {
        bool[] points = new bool[oldPoints.Length];

        //Left
        points[2] = oldPoints[3];
        points[1] = oldPoints[0];
        points[6] = oldPoints[7];
        points[5] = oldPoints[4];

        //Right
        points[3] = oldPoints[2];
        points[0] = oldPoints[1];
        points[7] = oldPoints[6];
        points[4] = oldPoints[5];

        return points;
    }

    public static bool[] FlipPointsY(bool[] oldPoints)
    {
        bool[] points = new bool[oldPoints.Length];

        for (int i = 0; i < 4; ++i)
        {
            points[4 + i] = oldPoints[i];
            points[i] = oldPoints[4 + i];
        }

        return points;
    }

    public static bool[] FlipPointsZ(bool[] oldPoints)
    {
        bool[] points = new bool[oldPoints.Length];

        //Front
        points[3] = oldPoints[0];
        points[2] = oldPoints[1];
        points[7] = oldPoints[4];
        points[6] = oldPoints[5];

        //Back
        points[0] = oldPoints[3];
        points[1] = oldPoints[2];
        points[4] = oldPoints[7];
        points[5] = oldPoints[6];



        ////Front
        //points[(int)MarchingCubeValues.D_RightBottomForward] = oldPoints[(int)MarchingCubeValues.A_RightBottomBack ];
        //points[(int)MarchingCubeValues.C_LeftBottomForward ] = oldPoints[(int)MarchingCubeValues.B_LeftBottomBack  ];
        //
        //points[(int)MarchingCubeValues.H_RightUpForward     ] = oldPoints[(int)MarchingCubeValues.E_RightUpBack   ];
        //points[(int)MarchingCubeValues.G_LeftUpForward      ] = oldPoints[(int)MarchingCubeValues.F_LeftUpBack      ];

        ////back
        //points[(int)MarchingCubeValues.A_RightBottomBack] = oldPoints[(int)MarchingCubeValues.D_RightBottomForward];
        //points[(int)MarchingCubeValues.B_LeftBottomBack] = oldPoints[(int)MarchingCubeValues.C_LeftBottomForward];
        //
        //points[(int)MarchingCubeValues.E_RightUpBack] = oldPoints[(int)MarchingCubeValues.H_RightUpForward];
        //points[(int)MarchingCubeValues.F_LeftUpBack] = oldPoints[(int)MarchingCubeValues.G_LeftUpForward];

        return points;
    }

    public static bool[] GetPoints(int index)
    {
        bool[] points = new bool[8];

        for(int i =0; i < 8; ++i)
        {
            points[i] = (index & (1 << i)) != 0;
        }

        return points;
    }

    private void OnDrawGizmos()
    {
        if (!_staticDrawPoints) return;

        for (int i = 0; i < MarchingCubes._corners.Length; i++)
        {
            Gizmos.color = _points[i] ? Color.white : Color.black;
            Gizmos.DrawSphere(transform.position + MarchingCubes._corners[i] - new Vector3(0.5f, 0.5f, 0.5f), 0.1f);
        }

#if UNITY_EDITOR
        Handles.Label(transform.position + new Vector3(-0.5f, -0.5f, -0.5f) + new Vector3(0, 0.2f, 0.0f), "A");
        Handles.Label(transform.position + new Vector3(0.5f, -0.5f, -0.5f) + new Vector3(0, 0.2f, 0.0f), "B");
        Handles.Label(transform.position + new Vector3(0.5f, -0.5f, 0.5f) + new Vector3(0, 0.2f, 0.0f), "C");
        Handles.Label(transform.position + new Vector3(-0.5f, -0.5f, 0.5f) + new Vector3(0, 0.2f, 0.0f), "D");
        Handles.Label(transform.position + new Vector3(-0.5f, 0.5f, -0.5f) + new Vector3(0, 0.2f, 0.0f), "E");
        Handles.Label(transform.position + new Vector3(0.5f, 0.5f, -0.5f) + new Vector3(0, 0.2f, 0.0f), "F");
        Handles.Label(transform.position + new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(0, 0.2f, 0.0f), "G");
        Handles.Label(transform.position + new Vector3(-0.5f, 0.5f, 0.5f) + new Vector3(0, 0.2f, 0.0f), "H");
#endif
    }
}
