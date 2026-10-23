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

        static ProjectBootstrap()
        {
            EditorApplication.delayCall += EnsureProjectReady;
        }

        [MenuItem("OneShot Football/Generate or Repair Main Scene")]
        public static void GenerateMainSceneFromMenu()
        {
            EnsureMainScene(true);
        }

        private static void EnsureProjectReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            EnsureMainScene(false);
        }

        public static void EnsureMainScene(bool openCreatedScene)
        {
            ConfigurePlayerSettings();

            string directory = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(ScenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject("OneShotFootballGame");
                root.AddComponent<OneShotFootballGame>();
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            if (openCreatedScene)
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                    Debug.LogError("OneShot Football: failed to open main scene.");
            }

            AssetDatabase.SaveAssets();
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "OneShot Studio";
            PlayerSettings.productName = "OneShot Football";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.oneshotfootball.game");
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        }
    }
}
