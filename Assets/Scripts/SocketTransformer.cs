using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketTransformer : MonoBehaviour
{
    [SerializeField] private int _rotation;
    [SerializeField] private FlipValues _flipped;

    Module _module;

    [Button]
    void TransformSockets()
    {
        if(_module == null) _module = GetComponent<Module>();


        var sockets = Module.TransformSockets(_rotation, _flipped, _module.Sockets);
        _module.Sockets = sockets;
    }
}
