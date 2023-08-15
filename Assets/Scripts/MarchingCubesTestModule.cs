using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MarchingCubesTestModule : MonoBehaviour
{
    [SerializeField, OnValueChanged(nameof(HandleCurrentModuleChanged))] private int _currentModuleIndex;
    [SerializeField, OnValueChanged(nameof(HandleOtherModuleChanged))] private int _otherModuleIndex;

    [SerializeField, ReadOnly]private MarchingCubeMeshes _currentModule;
    [SerializeField, ReadOnly]private MarchingCubeMeshes _otherModule;

    [SerializeField] private bool _debug;

    void HandleCurrentModuleChanged()
    {
        var currentModule = MeshTable.Instance.GetMesh((MarchingCubeValues)_currentModuleIndex);

        MarchingCubeMeshes newMesh = new();
        newMesh.MarchingCubeValues = currentModule.MarchingCubeValues;
        newMesh.MarchingCubeValue = currentModule.MarchingCubeValue;
        newMesh.Mesh = currentModule.Mesh;

        newMesh.Flipped = currentModule.Flipped;
        newMesh.RotationIndex = currentModule.RotationIndex;
        _currentModule = newMesh;
        //Debug.Log("current module changed to : " + newMesh.Mesh);
    }

    void HandleOtherModuleChanged()
    {
        var otherModule = MeshTable.Instance.GetMesh((MarchingCubeValues)_otherModuleIndex);

        MarchingCubeMeshes newMesh = new();
        newMesh.MarchingCubeValues = otherModule.MarchingCubeValues;
        newMesh.MarchingCubeValue = otherModule.MarchingCubeValue;
        newMesh.Mesh = otherModule.Mesh;

        newMesh.Flipped = otherModule.Flipped;
        newMesh.RotationIndex = otherModule.RotationIndex;
        _otherModule = newMesh;
        //Debug.Log("Other module changed to : " + newMesh.Mesh);
    }

    private List<GameObject> _instances = new();

    [Button]
    private void Clear()
    {
        if (_instances.Count > 0)
        {
            foreach (GameObject module in _instances)
            {
                if (module == null) continue;
                DestroyImmediate(module.gameObject);
            }
            _instances.Clear();
        }

        HandleCurrentModuleChanged();
        HandleOtherModuleChanged();
    }

    [Button]
    private void CheckLeft()
    {
        Clear();


        SpawnModule(transform.position + Vector3.right * .5f, _currentModule, _instances);
        SpawnModule(transform.position + Vector3.left * .5f,_otherModule, _instances);

        if (_debug)
            Debug.Log("Checking currentModule left: " + _currentModule.Mesh.Module.Sockets[(int)SocketDirection.Left] + " other module right: " + _otherModule.Mesh.Module.Sockets[(int)SocketDirection.Right]);
        DebugGizmos.DrawSpehere(transform.position, 0.25f, _currentModule.FitsDirection(SocketDirection.Left, _otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckRight()
    {
        Clear();

        SpawnModule(transform.position + Vector3.left * .5f, _currentModule, _instances);
        SpawnModule(transform.position + Vector3.right * .5f,_otherModule, _instances);

        if (_debug)
            Debug.Log("Checking currentModule right: " + _currentModule.Mesh.Module.Sockets[(int)SocketDirection.Right] + " other module left: " + _otherModule.Mesh.Module.Sockets[(int)SocketDirection.Left]);
        DebugGizmos.DrawSpehere(transform.position, 0.25f, _currentModule.FitsDirection(SocketDirection.Right, _otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckForward()
    {
        Clear();

        SpawnModule(transform.position + Vector3.back * .5f, _currentModule, _instances);
        SpawnModule(transform.position + Vector3.forward * .5f,_otherModule, _instances);

        if (_debug)
            Debug.Log("Checking currentModule forward: " + _currentModule.Mesh.Module.Sockets[(int)SocketDirection.Forward] + " other module right: " + _otherModule.Mesh.Module.Sockets[(int)SocketDirection.Backward]);
        DebugGizmos.DrawSpehere(transform.position, 0.25f, _currentModule.FitsDirection(SocketDirection.Forward, _otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckBackward()
    {
        Clear();

        SpawnModule(transform.position + Vector3.forward * .5f, _currentModule, _instances);
        SpawnModule(transform.position + Vector3.back * .5f,_otherModule, _instances);

        if (_debug)
            Debug.Log("Checking currentModule backwards: " + _currentModule.Mesh.Module.Sockets[(int)SocketDirection.Backward] + " other module forward: " + _otherModule.Mesh.Module.Sockets[(int)SocketDirection.Forward]);
        DebugGizmos.DrawSpehere(transform.position, 0.25f, _currentModule.FitsDirection(SocketDirection.Backward, _otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckUp()
    {
        Clear();

        SpawnModule(transform.position + Vector3.down * .5f, _currentModule, _instances);
        SpawnModule(transform.position + Vector3.up * .5f,_otherModule, _instances);

        if (_debug)
            Debug.Log("Checking currentModule up: " + _currentModule.Mesh.Module.Sockets[(int)SocketDirection.Up] + " other module down: " + _otherModule.Mesh.Module.Sockets[(int)SocketDirection.Down]);
        DebugGizmos.DrawSpehere(transform.position, 0.25f, _currentModule.FitsDirection(SocketDirection.Up, _otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckDown()
    {
        Clear();

        SpawnModule(transform.position + Vector3.up * .5f, _currentModule, _instances);
        SpawnModule(transform.position + Vector3.down * .5f, _otherModule, _instances);

        if (_debug)
            Debug.Log("Checking currentModule down: " + _currentModule.Mesh.Module.Sockets[(int)SocketDirection.Down] + " other module up: " + _otherModule.Mesh.Module.Sockets[(int)SocketDirection.Up]);
        DebugGizmos.DrawSpehere(transform.position, 0.25f, _currentModule.FitsDirection(SocketDirection.Down, _otherModule) ? Color.green : Color.red, 1.0f);
    }

    public void SpawnModule(Vector3 position, MarchingCubeMeshes mesh, List<GameObject> instances = null)
    {
        GameObject newGameobject = new GameObject("Cube_" + mesh.MarchingCubeValue);
        GameObject instance = GameObject.Instantiate(mesh.Mesh.gameObject, Vector3.zero, Quaternion.identity, newGameobject.transform);

        newGameobject.transform.position = position;
        newGameobject.transform.localScale = new Vector3(
            (mesh.Flipped & FlipValues.FlipX) != 0 ? -1.0f : 1.0f,
            (mesh.Flipped & FlipValues.FlipY) != 0 ? -1.0f : 1.0f,
            (mesh.Flipped & FlipValues.FlipZ) != 0 ? -1.0f : 1.0f);

        instance.transform.localRotation = Quaternion.Euler(0.0f, mesh.RotationIndex * 90.0f, 0.0f);

        MarchingCubeDescriptor marchingCubeDescriptor = instance.AddComponent<MarchingCubeDescriptor>();
        marchingCubeDescriptor.MarchingCubeMesh = new MarchingCubeMeshes();
        marchingCubeDescriptor.MarchingCubeMesh.MarchingCubeValues = (MarchingCubeValues)mesh.MarchingCubeValues;
        marchingCubeDescriptor.MarchingCubeMesh.MarchingCubeValue = mesh.MarchingCubeValue;
        marchingCubeDescriptor.MarchingCubeMesh.Flipped = mesh.Flipped;
        marchingCubeDescriptor.MarchingCubeMesh.RotationIndex = mesh.RotationIndex;

        Module module = marchingCubeDescriptor.GetComponent<Module>();
        var sockets = Module.TransformSockets(mesh.RotationIndex, mesh.Flipped, module.Sockets);
        module.Sockets = sockets;
        instances.Add(newGameobject);
    }
}
