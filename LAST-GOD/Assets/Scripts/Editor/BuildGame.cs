using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LastGod.Editor
{
    public static class BuildGame
    {
        [MenuItem("Tools/LAST-GOD/Build Standalone Windows Game")]
        public static void BuildWindowsExecutable()
        {
            string buildPath = "Builds/LAST-GOD.exe";
            string[] scenes = new string[] { "Assets/Scenes/Act1_Scene1.unity" };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[BuildGame] Starting build to: {buildPath}...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildGame] ✅ Build succeeded! File size: {summary.totalSize} bytes. Path: {buildPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[BuildGame] ❌ Build failed with {summary.totalErrors} errors.");
            }
        }
    }
}
