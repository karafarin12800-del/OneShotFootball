# Asset and license register

Updated: 2026-10-23

## Assets currently included

| Asset / category | Origin | License | Included |
| --- | --- | --- | --- |
| Pitch, markings, stands, goal, net, ball, player, goalkeeper and aim arc | Original meshes/materials created at runtime in C# | Project code: MIT | Yes |
| HUD and in-game visual effects | Original code and Unity built-in GUI | Project code: MIT | Yes |

No third-party model, texture, animation, audio, font or screenshot binary has been copied into the initial game. Runtime-created geometry is intentional: a clean clone can play without hunting for download links, importing paid packages, or discovering halfway through that a model is unavailable.

## Verified free sources considered for later art expansion

### Kenney — Sports Pack
- Source: https://kenney.nl/assets/sports-pack
- The page labels it 2D and Creative Commons CC0.
- Kenney's own terms: https://kenney.nl/support — assets on asset pages are public-domain/CC0 and may be used commercially without attribution.
- Decision: not bundled in this 3D prototype. 2D elements may help a separate menu/icon sheet later, but they do not replace 3D players or a 3D stadium.

### Kenney — Mini Arena
- Source: https://kenney.nl/assets/mini-arena
- The page lists a 3D pack and CC0.
- Decision: not bundled; its theme does not directly match a football stadium.

### Quaternius — Universal Base Characters
- Source: https://quaternius.com/packs/universalbasecharacters.html
- The pack page lists FBX/OBJ/Blend/glTF and displays CC0.
- Decision: not bundled. Before adding any binary in a future commit, re-check the exact download's current license and redistribution rules. The current prototype uses original low-poly character geometry and does not depend on downloading this pack.

## Rules for adding a third-party source asset

Before importing any future asset:
1. Record exact URL, creator, date accessed, license and individual files used.
2. Keep required license notices.
3. Confirm commercial use, modification and distribution inside a game.
4. Do not copy rival game screenshots, player likenesses, team logos, copyrighted kits or store artwork.
5. If terms are ambiguous, do not bundle the file; keep the game playable with original procedural assets.

This reduces legal and cost surprises.
