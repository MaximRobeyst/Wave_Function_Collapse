using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestModule : MonoBehaviour
{
    [SerializeField]ModuleDescriptor _currentModule;
    [SerializeField]ModuleDescriptor _otherModule;

    private List<Module> _instances = new();

    [Button]
    private void Clear()
    {
        if (_instances.Count > 0)
        {
            foreach (Module module in _instances)
            {
                if (module == null) continue;
                DestroyImmediate(module.gameObject);
            }
            _instances.Clear();
        }
    }

    [Button]
    private void CheckLeft()
    {
        Clear();

        _currentModule.SpawnModule(transform.position + Vector3.right * .5f, _instances);
        _otherModule.SpawnModule(transform.position + Vector3.left * .5f, _instances);

        Debug.Log("Checking currentModule left: " + _currentModule.GetLeft() + " other module right: " + _otherModule.GetRight());
        DebugGizmos.DrawSpehere(_instances[0].GetLeft(), 0.25f, _currentModule.FitsLeft(_otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckRight()
    {
        Clear();

        _currentModule.SpawnModule(transform.position + Vector3.left * .5f, _instances);
        _otherModule.SpawnModule(transform.position + Vector3.right * .5f, _instances);

        Debug.Log("Checking currentModule left: " + _currentModule.GetRight() + " other module right: " + _otherModule.GetLeft());
        DebugGizmos.DrawSpehere(_instances[0].GetRight(), 0.25f, _currentModule.FitsRight(_otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckForward()
    {
        Clear();

        _currentModule.SpawnModule(transform.position + Vector3.back * .5f, _instances);
        _otherModule.SpawnModule(transform.position + Vector3.forward * .5f, _instances);

        Debug.Log("Checking currentModule left: " + _currentModule.GetForward() + " other module right: " + _otherModule.GetBackwards());
        DebugGizmos.DrawSpehere(_instances[0].GetForward(), 0.25f, _currentModule.FitsForward(_otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckBackward()
    {
        Clear();

        _currentModule.SpawnModule(transform.position + Vector3.forward * .5f, _instances);
        _otherModule.SpawnModule(transform.position + Vector3.back * .5f, _instances);

        Debug.Log("Checking currentModule left: " + _currentModule.GetBackwards() + " other module right: " + _otherModule.GetForward());
        DebugGizmos.DrawSpehere(_instances[0].GetBackward(), 0.25f, _currentModule.FitsBackward(_otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckUp()
    {
        Clear();

        _currentModule.SpawnModule(transform.position + Vector3.down * .5f, _instances);
        _otherModule.SpawnModule(transform.position + Vector3.up * .5f, _instances);

        //Debug.Log("Checking currentModule left: " + _currentModule.GetBackwards() + " other module right: " + _otherModule.GetForward());
        DebugGizmos.DrawSpehere(_instances[0].GetUp(), 0.25f, _currentModule.FitsUp(_otherModule) ? Color.green : Color.red, 1.0f);
    }

    [Button]
    private void CheckDown()
    {
        Clear();

        _currentModule.SpawnModule(transform.position + Vector3.up * .5f, _instances);
        _otherModule.SpawnModule(transform.position + Vector3.down * .5f, _instances);

        //Debug.Log("Checking currentModule left: " + _currentModule.GetBackwards() + " other module right: " + _otherModule.GetForward());
        DebugGizmos.DrawSpehere(_instances[0].GetDown(), 0.25f, _currentModule.FitsDown(_otherModule) ? Color.green : Color.red, 1.0f);
    }
}
