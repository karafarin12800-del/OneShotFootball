using System.IO;
using UnityEditor;
using UnityEngine;

namespace OneShotFootball.Editor
{
    /// <summary>
    /// Applies Humanoid import settings to athlete FBX files placed in the project art folder.
    /// Unity generates the Avatar; this tool never fabricates a skeleton or animation clip.
    /// </summary>
    public sealed class HumanoidCharacterAssetPostprocessor : AssetPostprocessor
    {
        private const string CharacterRoot = "Assets/Art/Characters/";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(CharacterRoot, System.StringComparison.OrdinalIgnoreCase))
                return;

            var importer = assetImporter as ModelImporter;
            if (importer == null)
                return;

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.optimizeGameObjects = false;
        }

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(CharacterRoot, System.StringComparison.OrdinalIgnoreCase))
                return;

            Animator animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null)
                Debug.LogWarning("OneShot Football: imported character has no Animator: " + assetPath);
            else if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                Debug.LogError("OneShot Football: invalid Humanoid Avatar on: " + assetPath);
        }
    }

    public static class HumanoidCharacterAssetValidator
    {
        private const string CharacterRoot = "Assets/Art/Characters/";

        [MenuItem("OneShot Football/Validate Humanoid Character Assets")]
        public static void Validate()
        {
            if (!Directory.Exists(CharacterRoot))
            {
                Debug.LogWarning("OneShot Football: no character assets found. Add licensed FBX files under " + CharacterRoot);
                return;
            }

            string[] files = Directory.GetFiles(CharacterRoot, "*.fbx", SearchOption.AllDirectories);
            int valid = 0;
            int invalid = 0;

            foreach (string absolutePath in files)
            {
                string path = absolutePath.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Animator animator = model != null ? model.GetComponentInChildren<Animator>(true) : null;

                bool isValid = importer != null
                    && importer.animationType == ModelImporterAnimationType.Human
                    && animator != null
                    && animator.avatar != null
                    && animator.avatar.isValid
                    && animator.avatar.isHuman;

                if (isValid)
                {
                    valid++;
                    Debug.Log("OneShot Football: Humanoid asset valid: " + path);
                }
                else
                {
                    invalid++;
                    Debug.LogError("OneShot Football: Humanoid import validation failed: " + path);
                }
            }

            Debug.Log("OneShot Football Humanoid validation complete. Valid: " + valid + ", Invalid: " + invalid);
        }
    }
}