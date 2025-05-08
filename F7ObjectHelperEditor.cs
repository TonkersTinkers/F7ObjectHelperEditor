// Place in Assets/Editor/F7ObjectHelperEditor.cs
// https://tonkerstinkers.github.io/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class F7ObjectHelperEditor
{
    const string PrefKey = "F7ObjectName";
    const string StorePath = "Tools/F7 Select F7 Object";
    const string InspectPath = "Tools/F7 Inspect Target _F7";

    [MenuItem(StorePath)]
    // This method runs when you choose "Tools → F7 Select F7 Object"
    // It remembers the name of the currently selected object in the editor.
    static void StoreCurrentObject()
    {
        if (Selection.activeGameObject != null)
        {
            // If there is a selected object, save its name so we can find it later
            var name = Selection.activeGameObject.name;
            EditorPrefs.SetString(PrefKey, name);
            Debug.LogWarning($"[F7Helper] Stored '{name}' as F7 target.");
        }
        else
        {
            // If nothing is selected, clear any stored name
            EditorPrefs.DeleteKey(PrefKey);
            Debug.LogWarning("[F7Helper] Cleared F7 target.");
        }
    }

    [MenuItem(InspectPath)]
    // This method runs when you choose "Tools → F7 Inspect Target"
    // It looks up the stored name, finds that object in the scene (even if inactive), and selects it
    static void OnF7Key()
    {
        // Get the name we saved earlier
        var name = EditorPrefs.GetString(PrefKey, "");
        if (string.IsNullOrEmpty(name))
        {
            // If we never stored a name, tell the user to store one first
            Debug.LogError("[F7Helper] No target stored. Use Tools → F7 Select F7 Object first.");
            return;
        }

        // Try to find the object in any loaded scene, even if it is disabled
        var go = FindInSceneIncludingInactive(name);
        if (go == null)
        {
            // If we could not find it, show an error
            Debug.LogError($"[F7Helper] GameObject '{name}' not found in scene.");
            return;
        }

        // If we found it, select it in the editor and open the inspector window
        Selection.activeGameObject = go;
        F7InspectorWindow.Open(go);
    }

    [MenuItem(InspectPath, true)]
    // This method just tells Unity the "Inspect Target" menu item is always valid
    static bool OnF7_Validate() => true;

    // Searches all loaded scenes for a GameObject with the given name, including inactive ones.
    // It ignores prefab assets so it only returns objects that are actually in the scene.
    static GameObject FindInSceneIncludingInactive(string name)
    {
        // FindObjectsOfTypeAll will return every GameObject, even if it is disabled
        return Resources
            .FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(g =>
                // Match by name
                g.name == name &&
                // Make sure it belongs to a valid, loaded scene
                g.scene.IsValid() &&
                g.scene.isLoaded &&
                // Exclude objects that are not scene instances (like prefabs in the project)
                string.IsNullOrEmpty(AssetDatabase.GetAssetPath(g))
            );
    }
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

    // Open a floating window to inspect the given GameObject
    public static void Open(GameObject go)
    {
        // Get or create the window and set it to utility mode so it floats
        var win = GetWindow<F7InspectorWindow>(true, "F7 Inspector", true);
        // Remember which object we want to inspect
        win.target = go;
        win.titleContent = new GUIContent($"F7 Inspector: {go.name}");

        // If we saved a previous window position and size, restore it
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
            // Otherwise use a sensible default size and position
            win.position = new Rect(200, 200, 350, 600);
        }

        // Build the internal list of editors so we can draw all components
        win.CreateEditors();

        // Show the window and give it focus
        win.ShowUtility();
        win.Focus();
    }

    // Build or rebuild the list of component editors for the target object
    void CreateEditors()
    {
        // If we already had editors, destroy them so we start fresh
        if (editors != null)
        {
            foreach (var e in editors) DestroyImmediate(e);
        }

        editors = new List<Editor>();
        if (target == null) return;

        // For each component on the target object, make an Editor adapter
        foreach (var comp in target.GetComponents<Component>())
        {
            if (comp == null) continue;
            editors.Add(Editor.CreateEditor(comp));
        }
    }

    // Called when the window is closed or disabled
    void OnDisable()
    {
        // Save the current window position and size before it goes away
        SaveWindowRect();

        // Clean up any editors we created
        if (editors != null)
        {
            foreach (var ed in editors)
                DestroyImmediate(ed);
            editors = null;
        }
    }

    // Also save position when the window is destroyed
    void OnDestroy()
    {
        SaveWindowRect();
    }

    // Save the window position and size to EditorPrefs so it will remember next time
    void SaveWindowRect()
    {
        var r = position;
        EditorPrefs.SetFloat(KeyX, r.x);
        EditorPrefs.SetFloat(KeyY, r.y);
        EditorPrefs.SetFloat(KeyW, r.width);
        EditorPrefs.SetFloat(KeyH, r.height);
    }

    // Draw the contents of our custom inspector window
    void OnGUI()
    {
        if (target == null)
        {
            // If for some reason we have no target, show a message
            EditorGUILayout.LabelField("No target to inspect.");
            return;
        }

        // Make sure our editors list exists
        if (editors == null)
            CreateEditors();

        // Begin a scroll view so long inspectors scroll
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var ed in editors)
        {
            if (ed == null) continue;
            // Draw the header (component name) and default inspector UI
            ed.DrawHeader();
            ed.OnInspectorGUI();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndScrollView();
    }
}
