// Place in Assets/Editor/F7ObjectHelperEditor.cs
// https://tonkerstinkers.github.io/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class F7ObjectHelperEditor
{
    const string PrefKey = "F7ObjectName";
    const string StorePath = "Tools/F7 Select F7 Object";
    const string InspectPath = "Tools/F7 Inspect Target _F7";

    [MenuItem(StorePath)]
    static void StoreCurrentObject()
    {
        if (Selection.activeGameObject != null)
        {
            var name = Selection.activeGameObject.name;
            EditorPrefs.SetString(PrefKey, name);
            Debug.LogWarning($"[F7Helper] Stored '{name}' as F7 target.");
        }
        else
        {
            EditorPrefs.DeleteKey(PrefKey);
            Debug.LogWarning("[F7Helper] Cleared F7 target.");
        }
    }

    [MenuItem(InspectPath)]
    static void OnF7Key()
    {
        var name = EditorPrefs.GetString(PrefKey, "");
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("[F7Helper] No target stored. Use Tools → F7 Select F7 Object first.");
            return;
        }

        var go = GameObject.Find(name);
        if (go == null)
        {
            Debug.LogError($"[F7Helper] GameObject '{name}' not found in scene.");
            return;
        }

        Selection.activeGameObject = go;
        F7InspectorWindow.Open(go);
    }

    [MenuItem(InspectPath, true)]
    static bool OnF7_Validate() => true;
}


public class F7InspectorWindow : EditorWindow
{
    GameObject target;
    List<Editor> editors;
    Vector2 scroll;

    const string KeyX = "F7Inspector_x";
    const string KeyY = "F7Inspector_y";
    const string KeyW = "F7Inspector_w";
    const string KeyH = "F7Inspector_h";

    public static void Open(GameObject go)
    {
        // create (or reuse) window
        var win = GetWindow<F7InspectorWindow>(true, "F7 Inspector", true);
        win.target = go;
        win.titleContent = new GUIContent($"F7 Inspector: {go.name}");

        // restore saved position/size if present
        if (EditorPrefs.HasKey(KeyX))
        {
            win.position = new Rect(
                EditorPrefs.GetFloat(KeyX),
                EditorPrefs.GetFloat(KeyY),
                EditorPrefs.GetFloat(KeyW),
                EditorPrefs.GetFloat(KeyH)
            );
        }
        else
        {
            win.position = new Rect(200, 200, 350, 600);
        }

        // rebuild editors list fresh every time
        win.CreateEditors();

        win.ShowUtility();
        win.Focus();
    }

    void CreateEditors()
    {
        // always rebuild
        if (editors != null)
        {
            foreach (var e in editors) DestroyImmediate(e);
        }

        editors = new List<Editor>();
        if (target == null) return;

        foreach (var comp in target.GetComponents<Component>())
        {
            if (comp == null) continue;
            editors.Add(Editor.CreateEditor(comp));
        }
    }

    void OnDisable()
    {
        SaveWindowRect();

        if (editors != null)
        {
            foreach (var ed in editors)
                DestroyImmediate(ed);
            editors = null;
        }
    }

    void OnDestroy()
    {
        SaveWindowRect();
    }

    void SaveWindowRect()
    {
        var r = position;
        EditorPrefs.SetFloat(KeyX, r.x);
        EditorPrefs.SetFloat(KeyY, r.y);
        EditorPrefs.SetFloat(KeyW, r.width);
        EditorPrefs.SetFloat(KeyH, r.height);
    }

    void OnGUI()
    {
        if (target == null)
        {
            EditorGUILayout.LabelField("No target to inspect.");
            return;
        }

        if (editors == null)
            CreateEditors();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var ed in editors)
        {
            if (ed == null) continue;
            ed.DrawHeader();
            ed.OnInspectorGUI();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndScrollView();
    }
}
