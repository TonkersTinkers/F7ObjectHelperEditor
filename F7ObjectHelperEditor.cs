// Place in Assets/Editor/F7ObjectHelperEditor.cs
// https://tonkerstinkers.github.io/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class F7ObjectHelperEditor
{
    private const string PrefKey = "F7ObjectName";
    private const string PrefKeyShowWindow = "F7SelectorShowWindow";
    private const string StorePath = "Tools/F7 Select F7 Object _%F7";
    private const string InspectPath = "Tools/F7 Inspect Target Now _F7";
    private const string ShowWindowToggle = "Tools/F7 Show Window";

    [MenuItem(StorePath)]
    private static void StoreCurrentObject()
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

    [MenuItem(ShowWindowToggle)]
    private static void OnShowWindowToggle()
    {
        // default true if key not set
        bool showWindow = EditorPrefs.GetBool(PrefKeyShowWindow, true);
        showWindow = !showWindow;
        EditorPrefs.SetBool(PrefKeyShowWindow, showWindow);
        UnityEditor.Menu.SetChecked(ShowWindowToggle, showWindow);
    }

    [MenuItem(ShowWindowToggle, true)]
    private static bool OnShowWindowToggleValidate()
    {
        bool showWindow = EditorPrefs.GetBool(PrefKeyShowWindow, true);
        UnityEditor.Menu.SetChecked(ShowWindowToggle, showWindow);
        return true;
    }

    [MenuItem(InspectPath)]
    private static void OnF7Key()
    {
        var name = EditorPrefs.GetString(PrefKey, "");
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("[F7Helper] No target stored. Use Tools/F7 Select F7 Object first.");
            return;
        }

        var go = FindInSceneIncludingInactive(name);
        if (go == null)
        {
            Debug.LogError($"[F7Helper] GameObject '{name}' not found in scene.");
            return;
        }

        Selection.activeGameObject = go;

        // only open inspector window if enabled
        if (EditorPrefs.GetBool(PrefKeyShowWindow, true))
            F7InspectorWindow.Open(go);
    }

    [MenuItem(InspectPath, true)]
    private static bool OnF7_Validate() => true;

    // Searches all loaded scenes for a GameObject with the given name, including inactive ones.
    private static GameObject FindInSceneIncludingInactive(string name)
    {
        // Includes inactive and Editor-only objects, then filter to real scene objects
        return Resources
            .FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(g =>
                g.name == name &&
                g.scene.IsValid() &&
                g.scene.isLoaded &&
                string.IsNullOrEmpty(AssetDatabase.GetAssetPath(g)) // exclude prefabs/assets
            );
    }
}

public class F7InspectorWindow : EditorWindow
{
    private GameObject target;
    private List<Editor> editors;
    private Vector2 scroll;

    private const string KeyX = "F7Inspector_x";
    private const string KeyY = "F7Inspector_y";
    private const string KeyW = "F7Inspector_w";
    private const string KeyH = "F7Inspector_h";

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

    private void CreateEditors()
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

    private void OnDisable()
    {
        SaveWindowRect();

        if (editors != null)
        {
            foreach (var ed in editors)
                DestroyImmediate(ed);
            editors = null;
        }
    }

    private void OnDestroy()
    {
        SaveWindowRect();
    }

    private void SaveWindowRect()
    {
        var r = position;
        EditorPrefs.SetFloat(KeyX, r.x);
        EditorPrefs.SetFloat(KeyY, r.y);
        EditorPrefs.SetFloat(KeyW, r.width);
        EditorPrefs.SetFloat(KeyH, r.height);
    }

    private void OnGUI()
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
