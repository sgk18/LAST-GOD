using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LastGod.Editor
{
    public static class BuildStandalone
    {
        [MenuItem("Tools/LAST-GOD/Build Standalone EXE")]
        public static void BuildAndRunExe()
        {
            string buildFolder = "Builds";
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string exePath = Path.Combine(buildFolder, "TheLastGod.exe");

            string[] scenes = new string[]
            {
                "Assets/Scenes/MainMenu_Origin.unity",
                "Assets/Scenes/Act1_Origin.unity"
            };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.AutoRunPlayer
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildStandalone] Build succeeded: {summary.totalSize} bytes at {exePath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[BuildStandalone] Build failed with {summary.totalErrors} errors!");
            }
        }
    }
}
