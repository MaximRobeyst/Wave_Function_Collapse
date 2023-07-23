using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingCamera : MonoBehaviour
{
    [SerializeField] private float _speed = 1.0f;
    [SerializeField, Range(5, 20)] private float _radius = 10.0f;

    private float _timer;
    private Camera _camera;

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        _timer += Time.deltaTime * _speed;
        Vector3 position = Vector3.zero + new Vector3(Mathf.Sin(_timer) * _radius, 0, Mathf.Cos(_timer) * _radius);

        _camera.transform.position = position;
        _camera.transform.forward = (Vector3.zero - position).normalized;
    }
}
