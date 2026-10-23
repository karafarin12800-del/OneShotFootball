# Build, validation and mobile test plan

## Unity setup

- Editor baseline: 6000.3.0f1 (Unity 6.3 LTS).
- Install Android Build Support, Android SDK/NDK tools and OpenJDK.
- Main scene: Assets/Scenes/Main.unity (created by the Editor bootstrap if absent).
- Game entry: Assets/Scripts/OneShotFootballGame.cs.
- No networking package, analytics SDK, purchase SDK, web API key or paid art pack is required.

## Local Editor test

1. Open the repository root using the exact Unity version.
2. Wait for import and C# compilation.
3. Check Console for red errors.
4. Open Assets/Scenes/Main.unity, press Play and click PLAY 5-SHOT CHALLENGE.
5. Drag the mouse in the lower screen, then release. The aim arc should move with the drag and the ball should travel toward the predicted area.
6. Complete a round; verify goal/save/miss feedback, attempts, final score and replay.
7. Pause/resume during a round and check that physics resumes without teleporting.
8. Stop and restart Play Mode. The best score should persist through PlayerPrefs.

## Android device test

1. Select Android Build Target, portrait orientation and ARM64.
2. Build a development APK and install it on a physical phone.
3. Test full screen on a portrait 720p device and a different aspect ratio.
4. Complete 10 rounds. Ensure HUD taps do not fire a shot, swipes do not double-fire and pause/resume works.
5. Test short/long swipes, left/right targeting, low/high shots and keeper collisions.
6. Profile frame pacing, heat and memory after five minutes.
7. Verify offline play and high-score persistence after force-close and reopen.

## GitHub Actions

- source-check.yml validates project structure and JSON metadata; it does not compile Unity code.
- android-build.yml runs Unity in CI via GameCI and requires a valid license serialized as repository secret UNITY_LICENSE. Obtain it through Unity Hub/Unity documentation and add it under Settings → Secrets and variables → Actions. Never commit license data, credentials, a keystore or API keys.
- Without the license secret, the workflow explicitly fails with setup guidance rather than claiming an APK was built.

## Build artifacts

The Android workflow exports an APK as a GitHub Actions artifact if the licensed Unity build succeeds. Install and test that APK before calling the project release-ready. Store AAB packaging and a signed release keystore are separate release tasks.
