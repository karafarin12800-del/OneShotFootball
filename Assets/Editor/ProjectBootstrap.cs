using System.IO;
using OneShotFootball;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneShotFootball.Editor
{
    [InitializeOnLoad]
    public static class ProjectBootstrap
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        static ProjectBootstrap() { EditorApplication.delayCall += EnsureProjectReady; }

        [MenuItem("CUE LEGENDS/Generate or Repair Main Scene")]
        public static void GenerateMainSceneFromMenu() { EnsureMainScene(true); }

        private static void EnsureProjectReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureMainScene(false);
        }

        public static void EnsureMainScene(bool openCreatedScene)
        {
            ConfigurePlayerSettings();
            string directory = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            if (!File.Exists(ScenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject("CueLegendsGame");
                root.AddComponent<PoolGameController>();
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            if (openCreatedScene)
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid()) Debug.LogError("CUE LEGENDS: failed to open main scene.");
            }
            AssetDatabase.SaveAssets();
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Cue Legends Studio";
            PlayerSettings.productName = "CUE LEGENDS";
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Landscape;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.cuelegends.pool");
            PlayerSettings.Android.bundleVersionCode = 2;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        }
    }
}
