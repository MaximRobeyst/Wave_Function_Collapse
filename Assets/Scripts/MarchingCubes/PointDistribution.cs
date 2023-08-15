using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PointDistribution : MonoBehaviour
{
    [SerializeField] private bool _Is2D = false;

    [SerializeField] private int _size = 8;
    [SerializeField] private int _step = 0;
    [SerializeField, Range(0, 1)] private float _surfaceLevel = 0.0f;
    [SerializeField] private Material _material;

    [SerializeField] private bool _debugInverse = false;
    [SerializeField] private bool _drawStep = false;
    [SerializeField] private bool _paintable = false;

    [SerializeField] private float _radius = 1.0f;
    [SerializeField] private float _weight = 0.25f;

    private float[] _weights;

    public float[] Weights => _weights;
    public float SurfaceLevel => _surfaceLevel;
    public int Size => _size;

    private MarchingCubes _marchingCubes;
    private bool _update = false;
    private Vector3 _drawPoint;

    private void OnDrawGizmos()
    {
        if (_weights == null || _weights.Length == 0) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_drawPoint, 1.0f);

        Vector3 startPosition = new Vector3(-_size / 2.0f, -_size / 2.0f, -_size / 2.0f);

        int step = 0;
        for (int i = 0; i < _size; ++i)
        {
            for(int j = 0; j < _size; ++j)
            {
                for(int k = 0; k < _size; ++k)
                {
                    Vector3 newPosition = startPosition + new Vector3(i, j, k);
                    int index = GetIndex(i, j, k);

                    if (_step == step && _drawStep)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(GetPosition(i, j, k) + (Vector3.one / 2.0f), Vector3.one);
                    }
                    ++step;

                    if (_debugInverse && (index > _weights.Length || _weights[index] < _surfaceLevel)) continue;
                    if (!_debugInverse && (index < _weights.Length || _weights[index] > _surfaceLevel)) continue;
                    Gizmos.color = new Color(_weights[index], _weights[index], _weights[index]);
                    Gizmos.DrawSphere(newPosition, 0.1f);
                }
            }
        }

    }

    public void SetupPointDistribution(int size)
    {
        _size = size;
        GeneratePoints();
    }

    public int GetIndex(int x, int y, int z)
    {
        if (_Is2D)
            return (y * _size) + x;
        else
            return (z * _size * _size) + (y * _size) + x;
    }

    public Vector3 GetPosition(int x , int y, int z)
    {
        Vector3 startPosition = new Vector3(-_size / 2.0f, -_size / 2.0f, -_size / 2.0f);
        return startPosition + new Vector3(x, y, z);
    }

    [Button]
    public void GeneratePoints()
    {
        _weights = new float[_size * _size * _size];
        for (int i = 0; i < _weights.Length; ++i)
        {
            _weights[i] = Random.value;
        }
    }

    private void Update()
    {
        if (!_paintable) return;

        if(_marchingCubes == null) _marchingCubes = GetComponent<MarchingCubes>();
        if(Weights == null) _weights = new float[_size * _size * _size];


        if (Input.GetMouseButton(0))
        {
            _update = true;
            Vector3 startPosition = new Vector3(-_size / 2.0f, -_size / 2.0f, -_size / 2.0f);
            Vector3 endPosition = new Vector3(_size / 2.0f, _size / 2.0f, _size / 2.0f);

            Ray ray =  Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit, 100.0f))
            {
                _drawPoint = hit.point;
                DrawPoints(_drawPoint, _radius, -_weight);
            }

        }

        if(Input.GetMouseButton(2))
        {
            _update = true;
            Vector3 startPosition = new Vector3(-_size / 2.0f, -_size / 2.0f, -_size / 2.0f);
            Vector3 endPosition = new Vector3(_size / 2.0f, _size / 2.0f, _size / 2.0f);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100.0f))
            {
                _drawPoint = hit.point;
                DrawPoints(_drawPoint, _radius, _weight);
            }
        }

        if (_update)
        {
            _marchingCubes.MarchCubes(this);
            _marchingCubes.GenerateMesh();

            MeshCollider meshCollider = _marchingCubes.GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = _marchingCubes.AddComponent<MeshCollider>();

            meshCollider.sharedMesh = _marchingCubes.GetComponent<MeshFilter>().sharedMesh;

        }
    }

    private void DrawPoints(Vector3 point, float radius, float influence)
    {
        Vector3 startPosition = new Vector3(-_size / 2.0f, -_size / 2.0f, -_size / 2.0f);
        Vector3 endPosition = new Vector3(_size / 2.0f, _size / 2.0f, _size / 2.0f);

        int flooredRadius = Mathf.FloorToInt(radius);

        for(int x =0;x < Mathf.FloorToInt(radius); ++x)
        {
            for(int y = 0; y < Mathf.FloorToInt(radius); ++y)
            {
                for(int z = 0; z < Mathf.FloorToInt(radius); ++z)
                {
                    int index = GetIndex(
                        Mathf.Clamp(Mathf.FloorToInt(_drawPoint.x + x + endPosition.x), 0, Size),
                        Mathf.Clamp(Mathf.FloorToInt(_drawPoint.y + y + endPosition.y), 0, Size),
                        Mathf.Clamp(Mathf.FloorToInt(_drawPoint.z + z + endPosition.z), 0, Size));
                    if (index >= 0 && index < _weights.Length)
                        _weights[index] += influence * Time.deltaTime;

                    index = GetIndex(
                        Mathf.Clamp(Mathf.FloorToInt(_drawPoint.x - x + endPosition.x), 0, Size),
                        Mathf.Clamp(Mathf.FloorToInt(_drawPoint.y - y + endPosition.y), 0, Size),
                        Mathf.Clamp(Mathf.FloorToInt(_drawPoint.z - z + endPosition.z), 0, Size));
                    if (index >= 0 && index < _weights.Length)
                        _weights[index] += influence * Time.deltaTime;
                }
            }
        }



    }
}
