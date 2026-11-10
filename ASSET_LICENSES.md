# Asset and license register

Updated: 2026-11-10

## Assets currently included

| Asset / category | Origin | License | Included |
| --- | --- | --- | --- |
| Pitch, markings, stands, goal, net, ball, player, goalkeeper and aim arc | Original meshes/materials created at runtime in C# | Project code: MIT | Yes |
| HUD and in-game visual effects | Original code and Unity built-in GUI | Project code: MIT | Yes |
| App identity mark | Original SVG vector authored for this repository | Project art: MIT | Source SVG only; not yet configured as Unity launcher icon |

No third-party model, texture, animation, audio, font or screenshot binary is included in this first commit. The runtime-created geometry is intentional: a clean clone can launch the prototype without paid packages, external downloads or broken asset references. The current 3D characters are low-poly prototype geometry, not polished rigged athlete models.

## Free source packs selected for the first rigged-art integration

### Quaternius — Universal Base Characters
- Official pack: https://quaternius.com/packs/universalbasecharacters.html
- Official free download: https://quaternius.itch.io/universal-base-characters
- The author describes six Humanoid-rigged game-ready character models, FBX/glTF support, and CC0 use including commercial projects. The page gives an average of roughly 13k triangles.
- Planned use: first stylized rigged kicker and goalkeeper models, with additional variants for player selection.
- Status: **selected candidate, not downloaded or bundled**. The existing code has Resources prefab loader and Humanoid import validator, but actual FBX files/prefabs are still missing. The models are stylized low-poly, not photoreal athletes.

### Quaternius — Universal Animation Library
- Official pack: https://quaternius.com/packs/universalanimationlibrary.html
- Official free download: https://quaternius.itch.io/universal-animation-library
- The author describes 120+ general Humanoid animations, retargeting and CC0 commercial use. The free Standard archive is a subset of the complete library.
- Planned use: locomotion, idle, jog/run and suitable emotes.
- Status: **selected candidate, not downloaded or bundled**. The pack description does not promise a penalty kick or goalkeeper dive clip. Those specific football animations still need to be created or sourced under a separately checked license.

### Kenney — Sports Pack
- Source: https://kenney.nl/assets/sports-pack
- The source page lists a 2D pack of 380+ files with Creative Commons CC0 licensing.
- Kenney FAQ: https://kenney.nl/support — says assets on its asset pages are public-domain/CC0, may be used commercially, and do not require attribution.
- Planned use: optional 2D menu icons or UI accents only; it does not replace a 3D stadium or 3D footballers.
- Status: **not bundled or integrated yet**.

### Kenney — Prototype Kit
- Source: https://kenney.nl/assets/prototype-kit
- The source page lists a generic 3D pack with 145 files and CC0 licensing.
- Planned use: optional generic environment props if they suit the final art direction.
- Status: **not bundled or integrated yet**; it is not football-specific.

### Kenney — Mini Arena
- Source: https://kenney.nl/assets/mini-arena
- The source page lists a 3D pack and CC0 licensing.
- Decision: reviewed, but not selected for the first game because the arena style does not directly match a football stadium.

## Asset integration rules

Before adding any external asset binary:
1. Record the exact source URL, creator, date accessed, license and files included.
2. Preserve any license notice required by the exact download.
3. Confirm commercial use, modification and distribution inside a mobile game.
4. Check the exact archive; a general website or an older cached page is not proof that every optional file is free.
5. Import it in the selected Unity version, fix materials/textures and verify scale, rig and performance on Android.
6. Do not copy competitors' screenshots, logos, uniforms, player likenesses, levels or source code.
7. If licensing or compatibility is uncertain, do not bundle that file; keep the procedural asset as a safe fallback.

These decisions intentionally avoid surprise asset purchases. The repository currently has **no mandatory paid asset dependency**. No third-party 3D model or animation binary has yet been imported into the project.
