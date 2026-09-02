using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AutoPlay
{
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
