# F7 Inspector Window for Unity Editor

Open-source Unity Editor extension that lets you:

- Quickly select any GameObject in your scene with F7 (or via the Tools menu)
- Pop up a dedicated, floating Inspector window showing all components and properties exactly like the built-in Inspector 
- Remember your last window position and size across Unity sessions 
- Store your “target” object by name once, then just hit F7 to jump right to it.

---

## Key Features and Benefits

- Global F7 Shortcut  
  Press F7 anywhere (edit or play mode) to quickly select your stored GameObject and (optionally) open its properties.

- Global Control + F7 Shortcut  
  Press Control + F7 (edit or play mode) to quickly store which object will be selected 

- Tools Menu Integration  
  - **Tools | F7 Select F7 Object**: Store the currently selected GameObject’s name via menu or shortcut  
  - **Tools | F7 Inspect Target (F7)**: Invoke the F7 command via menu or shortcut
  - **Tools | F7 Show Window**: Toggle whether pressing F7 also opens the floating Inspector window (default = on)

- Floating Inspector Panel  
  A utility window that mirrors Unity’s Inspector:  
  - Draws a header and full inspector GUI for every Component (including Transform)  
  - Scrollable for objects with many components

- Persistent Window Layout  
  - Saves X, Y, Width, Height of the Inspector window in EditorPrefs  
  - Restores last position and size when reopened, even after restarting Unity

- Easy Installation  
  Copy a single `F7ObjectHelperEditor.cs` file into your `Assets/Editor/` folder.  
  No dependencies, no setup scripts, no extra plugins.

---

## Installation

1. Download or copy `F7ObjectHelperEditor.cs` into your Unity project under `Assets/Editor/`.  
2. Let Unity compile. You’ll see three new menu entries:
   - **Tools | F7 Select F7 Object**  
   - **Tools | F7 Inspect Target (F7)**  
   - **Tools | F7 Show Window**  
3. Select a GameObject in your scene, then click **Tools | F7 Select F7 Object**.  
4. Press F7 (or choose **Tools | F7 Inspect Target**) to select your object, if **F7 Show Window** is enabled, pop up the floating inspector.

---

## Usage

1. **Storing Your Target**  
   - In the Hierarchy, select the GameObject you want to debug.  
   - Go to **Tools | F7 Select F7 Object** or press Control+F7.  
   - A console warning confirms the name is stored.

2. **Inspecting the Target**  
   - Press F7 at any time (edit-mode or play-mode).  
   - The tool will:  
     1. Look up the stored name via EditorPrefs  
     2. Select the GameObject in the Hierarchy  
     3. If **Tools | F7 Show Window** is on, open a floating window titled “F7 Inspector: [GameObjectName]”  
     4. Render each Component in its own header plus inspector GUI

3. **Toggling the Inspector Window**  
   - Use **Tools | F7 Show Window** to turn the popup on or off.  
   - When off, F7 just selects the object in the Hierarchy.  
   - Default state is **on**.

4. **Moving and Resizing**  
   - Drag or resize the floating window.  
   - On close (or Unity reload), your window’s position and size are saved.  
   - Next time you press F7 (with Show Window on), the window reopens in the same location and dimensions.

---

## How It Works

- Uses Unity’s `[MenuItem(...)]` attribute with `_F7` suffix to bind the F7 key globally  
- Stores the target GameObject’s name in `EditorPrefs` under key `F7ObjectName`  
- On F7 invocation:  
  1. `GameObject.Find(storedName)` locates the object in the scene  
  2. `Selection.activeGameObject` sets the Hierarchy selection  
  3. If Show Window is enabled, `EditorWindow.GetWindow<F7InspectorWindow>(true).ShowUtility()` opens a utility panel  
  4. The window’s `OnGUI()` loops through all `GetComponents<Component>()`, creates an `Editor` for each, and calls `DrawHeader()` plus `OnInspectorGUI()` to replicate the built-in Inspector view  
- Saves and restores window Rect (`x`, `y`, `width`, `height`) using `EditorPrefs.SetFloat` and `GetFloat`

---

## Keywords and Search Terms

Unity Editor extension, Unity custom inspector window, Unity floating inspector, EditorWindow scrollable inspector, EditorPrefs window position, F7 shortcut Unity, debug play-mode selection, select GameObject hotkey, Unity play-mode debugging tool, Unity Hierarchy quick-select

---

## Requirements and Compatibility

- Unity 2019.4 or newer (tested on 2020, 2021, 2022)  
- No external packages required  
- Copy into one `.cs` file under `Assets/Editor/`

---

## Why Use This

- Speed up debugging in complex scenes, minimize repetitive manual Hierarchy searches  
- Inspect objects at runtime without opening the full Inspector panel  
- Lightweight: single script, no overhead, free to use and modify  
- Persistent layout: your pop-up stays where you put it, across sessions

---

## License

Free for personal or commercial use. See LICENSE for details.

---

## Contributing and Feedback

- Found a bug? Please open an Issue.  
- Want a feature or improvement? Submit a Pull Request.  
- Share your tips and workflows in the Discussions tab.

---

**Note:** This is a standalone Editor utility. It does not affect your game’s runtime build or performance. It lives entirely in the Unity Editor domain and can be safely removed before building your game.  
