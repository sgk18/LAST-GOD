using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to enter Play Mode via the Unity menu or keyboard shortcut.
/// Use: Tools > Auto Play > Enter Play Mode
/// Or press Ctrl+P (default Unity shortcut) while the editor is focused.
/// </summary>
[InitializeOnLoad]
public static class AutoPlay
{
    static AutoPlay()
    {
        // Optionally auto-play on domain reload (editor start / compile finish).
        // Disabled by default — use the menu item or shortcut instead.
        // EditorApplication.delayCall += EnterPlayMode;
    }

    [MenuItem("Tools/Auto Play/Enter Play Mode %#p")]
    public static void EnterPlayMode()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.Log("[AutoPlay] Entering Play Mode...");
            EditorApplication.isPlaying = true;
        }
        else
        {
            Debug.Log("[AutoPlay] Already in Play Mode.");
        }
    }

    [MenuItem("Tools/Auto Play/Exit Play Mode %#o")]
    public static void ExitPlayMode()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.Log("[AutoPlay] Exiting Play Mode...");
            EditorApplication.isPlaying = false;
        }
    }
}
