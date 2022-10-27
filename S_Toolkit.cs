using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections.Generic;


public class S_Toolkit : EditorWindow
{
    
    private string ObjectForLODs ;
    private float ParametrLODs = 0.02f, ParametrValveLODs = 0.02f;
    
    [MenuItem("Window/S_toolkit")]
    private static void Init()
    {
        S_Toolkit window = (S_Toolkit)EditorWindow.GetWindow(typeof(S_Toolkit));
        window.Show();  
    }

    private void OnGUI()
    {
        ObjectForLODs = EditorGUILayout.TextField("Custom LOD for:", ObjectForLODs);
        ParametrLODs = EditorGUILayout.Slider("%LOD", ParametrLODs, 0, 0.3f);

        if (GUILayout.Button("Setup custom LODs"))
        {
            SetupCustomLODs(ObjectForLODs, ParametrLODs);
            Debug.Log("successful!!!");
        }

        ParametrValveLODs = EditorGUILayout.Slider("%LOD for Valves", ParametrValveLODs, 0, 0.1f);
        if (GUILayout.Button("Setup LODs to Valves"))
        {
            SetupLODsToValves(ParametrValveLODs);
            Debug.Log("successful!!!");
        }

        if (GUILayout.Button("Setup LODs to OZL"))
        {
            SetupLODsToOZLs();
            Debug.Log("successful!!!");
        }

        if (GUILayout.Button("Setup doors"))
        {
            SetupDoors();
            Debug.Log("successful!!!");
        }

        if (GUILayout.Button("Setup LODs from .txt file (скоро)"))
        {
            GetWindow(typeof(SetupTXTFile), true, "Setup .txt file", true);
        }
    }

    private class SetupTXTFile : EditorWindow
    {
        private Object file;

        private static void Init()
        {
            var window = GetWindowWithRect<SetupTXTFile>(new Rect(0, 0, 165, 100));
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            file = EditorGUILayout.ObjectField(file, typeof(Object), true);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("setup LODs"))
            {

            }
        }
    }

    //метод установления кастомных лодов
    private static void SetupCustomLODs(string keyname, float value)
    {
        int addedCount = 0;
        int customizedCount = 0;
        var allObjects = FindObjectsOfType<Transform>().Where(t => t.name.Contains(keyname));


        foreach (var obj in allObjects)
        {
            LODGroup group;
            if (!obj.TryGetComponent(out group))
            {
                group = obj.gameObject.AddComponent<LODGroup>();
                addedCount++;
            }

            var lods = new LOD[1];
            lods[0] = new LOD(value, obj.GetComponents<Renderer>());
            group.SetLODs(lods);
            customizedCount++;
        }

        Debug.Log($"Added LODs: {addedCount}");
        Debug.Log($"Customized LODs: {customizedCount}");
    }

    //метод для перечисления имен задвижек на которые LODsForValves установит лоды. Может пополняться по мере появления новых задвижек
    private static void SetupLODsToValves(float value)
    {
        LODsForValves("ValveFWManipulator", value);
        LODsForValves("Flap", value);
        LODsForValves("Base", value);
        LODsForValves("ppk", value);
        LODsForValves("manometr", value);
        LODsForValves("checkV", value);
        LODsForValves("Flange", value);
    }

    //метод установки лодов на задвижки
    private static void LODsForValves(string keyname, float value = 0.02f)
    {
        var allObjects = FindObjectsOfType<Transform>().Where(h => h.name.Contains(keyname));

        int addedCount = 0;
        int customizedCount = 0;

        foreach (var obj in allObjects)
        {
            LODGroup group;
            if (!obj.TryGetComponent(out group))
            {
                group = obj.gameObject.AddComponent<LODGroup>();
                addedCount++;
            }

            var lods = new LOD[1];
            lods[0] = new LOD(value, obj.GetComponents<Renderer>());
            group.SetLODs(lods);
            customizedCount++;
        }

        Debug.Log($"Added LODs: {addedCount}");
        Debug.Log($"Customized LODs: {customizedCount}");
    }

    //метод для проверки имён OZL
    private static void SetupLODsToOZLs()
    {
        int count = 0;
        var ozls = FindObjectsOfType<Transform>().Where(h => h.name.Contains("OZL"));
        
        foreach (var ozl in ozls)
        {
            var arguments = ozl.name.Split('_');
            float lod = 0.02f;
            if (arguments != null && arguments.Length > 0)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i] == "OZL" && i < arguments.Length - 1)
                    {
                        if (float.TryParse(arguments[i + 1], out float argument))
                        {
                            lod = argument * 0.01f;
                        }
                    }
                }

                LODForOZL(ozl, lod);
                count++;
            }
        }

        Debug.Log("OZL objects get LOD: " + count);
    }
    
    //метод установления лодов на OZL
    private static void LODForOZL(Transform obj, float value = 0.02f)
    {
        LODGroup group;
        if (!obj.TryGetComponent(out group))
        {
            group = obj.gameObject.AddComponent<LODGroup>();
        }

        var lods = new LOD[1];
        lods[0] = new LOD(value, obj.GetComponents<Renderer>());
        group.SetLODs(lods);
    }

    //метод установки дверь-контроллеров и правильного вращения при открытии
    private static void SetupDoors()
    {
        int collidersCount = 0;
        int doorsCount = 0;
        int customizedDoor = 0;
        var doorsObjectsr = FindObjectsOfType<Transform>().Where(h => h.name.ToLower().Contains("interactdoorr")).ToArray();
        var doorsObjectsl = FindObjectsOfType<Transform>().Where(h => h.name.ToLower().Contains("interactdoorl")).ToArray();
        
        for (int i = 0; i < doorsObjectsr.Length; i++)
        {
            if (!doorsObjectsr[i].TryGetComponent<MeshCollider>(out var collider))
            {
                doorsObjectsr[i].gameObject.AddComponent<MeshCollider>();
                collidersCount++;
            }
            else
            {
                if (!collider.enabled)
                {
                    collider.enabled = true;
                }
            }

            if (!doorsObjectsr[i].TryGetComponent<DoorController>(out _))
            {
                doorsObjectsr[i].gameObject.AddComponent<DoorController>();
                doorsObjectsr[i].gameObject.GetComponent<DoorController>().AngleRotation = -120.0f;
                doorsCount++;
            }
            if(doorsObjectsr[i].gameObject.GetComponent<DoorController>().AngleRotation != -120.0f)
            {
                doorsObjectsr[i].gameObject.GetComponent<DoorController>().AngleRotation = -120.0f;
                customizedDoor++;
            }
        }
        
        for (int i = 0; i < doorsObjectsl.Length; i++)
        {
            if (!doorsObjectsl[i].TryGetComponent<MeshCollider>(out var collider))
            {
                doorsObjectsl[i].gameObject.AddComponent<MeshCollider>();
                collidersCount++;
            }
            else
            {
                if (!collider.enabled)
                {
                    collider.enabled = true;
                }
            }

            if (!doorsObjectsl[i].TryGetComponent<DoorController>(out _))
            {
                doorsObjectsl[i].gameObject.AddComponent<DoorController>();
                doorsCount++;
            }
            if(doorsObjectsl[i].gameObject.GetComponent<DoorController>().AngleRotation != 120.0f)
            {
                doorsObjectsl[i].gameObject.GetComponent<DoorController>().AngleRotation = 120.0f;
                customizedDoor++;
            }
        }

        Debug.Log($"Installed colliders on doors: {collidersCount}.");
        Debug.Log($"Installed Doors: {doorsCount}.");
        Debug.Log($"Customized Doors: {customizedDoor}");
    }
}