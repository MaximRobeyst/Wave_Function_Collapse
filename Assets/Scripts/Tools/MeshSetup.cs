using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Diagnostics;

public class MeshSetup : MonoBehaviour
{
    [Button]
    private void SetupBasedOnName()
    {
        for(int i =0;i < transform.childCount; ++i)
        {
            string numberString = Regex.Match(transform.GetChild(i).name, @"\d+").Value;
            MarchingCubeModule marchingCubeModule = transform.GetChild(i).GetComponent<MarchingCubeModule>();
            if (marchingCubeModule == null)
                marchingCubeModule = transform.GetChild(i).AddComponent<MarchingCubeModule>();

            marchingCubeModule.SetIndex(Int32.Parse(numberString));
             
        }
    }

    [MenuItem("Assets/Algorithm/SetupModules", priority = 3)]
    public static void SetupForCombinedAlgorithm()
    {
        // Get current selected path
        var activeObject = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(activeObject.GetInstanceID());

        if (!Directory.Exists(path)) return;
        Debug.Log("path: " + path);

        var objects = Directory.GetFiles(path, "*.prefab");

        for (int i = 0; i < objects.Length; ++i)
        {
            var prefab = AssetDatabase.LoadAssetAtPath(objects[i], typeof(GameObject));

            MarchingCubeModule marchingCubeModule = prefab.GetComponent<MarchingCubeModule>();
            if (marchingCubeModule == null)
            {
                Debug.Log("No marching cube module found", prefab);
                return;
            }
            Module module = prefab.GetComponent<Module>();
            if (module == null) marchingCubeModule.AddComponent<Module>();
        }
    }


    [MenuItem("Assets/Algorithm/Add To Mesh Table", priority = 3)]
    public static void AddToMeshTable()
    {
        // Get current selected path
        var activeObject = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(activeObject.GetInstanceID());

        if (!Directory.Exists(path)) return;

        var objects = Directory.GetFiles(path, "*.prefab");

        for (int i = 0; i < objects.Length; ++i)
        {
            var prefab = AssetDatabase.LoadAssetAtPath(objects[i], typeof(GameObject));

            MarchingCubeModule marchingCubeModule = prefab.GetComponent<MarchingCubeModule>();
            if (marchingCubeModule.MarchingCubePrefab == null)
                marchingCubeModule.MarchingCubePrefab = prefab.GetComponent<MarchingCubeModule>();

            if (marchingCubeModule == null)
            {
                Debug.Log("No marching cube module found", prefab);
                return;
            }
            marchingCubeModule.AddToTable();
        }
    }


    [MenuItem("Assets/Algorithm/Setup sockets", priority = 3)]
    public static void SetupSockets()
    {
        SetupSockets(false);
    }

    [MenuItem("Assets/Algorithm/Setup sockets (override previous)", priority = 3)]
    public static void SetupSocketsOverride()
    {
        SetupSockets(true);
    }

    public static void SetupSockets(bool overridePrevious = true)
    {
        // Get current selected path
        var activeObject = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(activeObject.GetInstanceID());

        if (!Directory.Exists(path)) return;

        var objects = Directory.GetFiles(path, "*.prefab");

        for (int i = 0; i < objects.Length; ++i)
        {
            var prefab = AssetDatabase.LoadAssetAtPath(objects[i], typeof(GameObject));

            Module module = prefab.GetComponent<Module>();
            if (module == null)
            {
                Debug.Log("No module found", prefab);
                return;
            }
            module._3D = true;
            module.SetupSockets();

            if (overridePrevious)
            {
                module.AutoSetupSockets();
                continue;
            }

            if (!module.AnyEmpty())
                continue;

            module.AutoSetupSockets();
        }
    }

}
