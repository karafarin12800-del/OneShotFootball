using System;
using System.IO;
using System.Linq;
using OneShotFootball;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OneShotFootball.Editor
{
    public static class AndroidBuild
    {
        [MenuItem("OneShot Football/Build Android APK")]
        public static void BuildAndroidApk()
        {
            ProjectBootstrap.EnsureMainScene(false);
            EditorUserBuildSettings.buildAppBundle = false;

            string outputDirectory = Path.Combine("Builds", "Android");
            Directory.CreateDirectory(outputDirectory);

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled Unity scenes are configured.");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(outputDirectory, "OneShotFootball.apk"),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Android build did not succeed. Result: " + report.summary.result +
                    "; errors: " + report.summary.totalErrors);
            }

            Debug.Log("OneShot Football APK built: " + options.locationPathName +
                      " (" + report.summary.totalSize + " bytes).");
        }
    }
}
