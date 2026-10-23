# Main scene generation

The Unity Editor bootstrap in `Assets/Editor/ProjectBootstrap.cs` creates `Main.unity` on first project open if it is missing and registers it in Build Settings. The scene is built by Unity Editor so object references are serialized consistently, rather than hand-writing raw scene YAML.

After opening the project, confirm that `Assets/Scenes/Main.unity` appears, press Play, and inspect the Console. A GitHub source-check does not validate generated scenes or play behavior.
