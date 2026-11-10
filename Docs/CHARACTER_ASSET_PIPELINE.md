# Humanoid Character and Animation Pipeline

## Current runtime contract

The game loads optional prefabs from these exact Resources paths:

| Runtime resource key | Unity prefab asset path |
|---|---|
| Characters/Kicker | Assets/Resources/Characters/Kicker.prefab |
| Characters/ProForward | Assets/Resources/Characters/ProForward.prefab |
| Characters/StreetLegend | Assets/Resources/Characters/StreetLegend.prefab |
| Characters/Goalkeeper | Assets/Resources/Characters/Goalkeeper.prefab |

If a prefab is absent, the game retains the original procedural character as a fallback. No imported character binary is currently committed to this repository.

## Recommended free Humanoid source

Quaternius Universal Base Characters is the selected candidate for the first real rig integration:
- Official pack: https://quaternius.com/packs/universalbasecharacters.html
- Official download page: https://quaternius.itch.io/universal-base-characters
- The pack page describes six game-ready humanoid characters, a Humanoid rig, FBX/glTF formats, and CC0 licensing for commercial use.
- The characters are stylized, low-poly assets (the pack page gives an average of about 13k triangles). They are useful game-ready rigs but are not photorealistic athletes.
- Download the Standard/free archive from the author page. Do not include the paid Source archive unless separately licensed and approved.

Quaternius Universal Animation Library:
- Official pack: https://quaternius.com/packs/universalanimationlibrary.html
- Official download page: https://quaternius.itch.io/universal-animation-library
- Provides general Humanoid animation clips and locomotion/emotes for retargeting. It does not guarantee a specific penalty-kick or goalkeeper-dive clip. The exact Standard archive contents must be inspected before assigning clips.

Both packs are CC0 according to their official pack pages. Re-check the license and the exact archive contents when downloaded, and preserve this document and the source URLs in the project.

## Import procedure

1. Put licensed base character FBX files under Assets/Art/Characters/Models/.
2. Put compatible animation FBX files under Assets/Art/Characters/Animations/.
3. Let Unity import them. HumanoidCharacterAssetPipeline.cs applies Humanoid import settings to FBX files under Assets/Art/Characters/.
4. Open each FBX's Rig tab and confirm Animation Type = Humanoid and Avatar Definition is appropriate. For separate animation-only FBX files, use Copy From Other Avatar and select the base character Avatar when required.
5. Run OneShot Football > Validate Humanoid Character Assets. Every intended character should report a valid Humanoid Avatar.
6. Create prefabs at the four exact Resources paths above.
7. Keep the visual root at local position (0,0,0), scale one, and orient the kicker toward +Z. The goalkeeper should face the incoming ball (the current loader rotates a loaded keeper prefab by 180 degrees).
8. Add an Animator Controller to each character. The controller must expose the parameters below. Use in-place clips; the gameplay controller owns root movement and shot physics.

## Animator contract

| Parameter | Type | Purpose |
|---|---|---|
| Kick | Trigger | Kicker strike presentation |
| Celebrate | Trigger | Goal celebration |
| DiveLeft | Trigger | Keeper left dive |
| DiveRight | Trigger | Keeper right dive |
| Speed | Float | Optional locomotion blend |

FootballerAnimationDriver safely ignores missing parameters, but a missing parameter means that animation will not play. The fallback procedural character still uses its basic leg-kick pivot.

FootballerAppearance can color renderers by explicit Inspector assignment or by renderer names containing jersey/shirt/top, shorts, boot/shoe, skin/body, hair, and armband/captain. Prefer explicit references for production prefabs.

## Animation quality gate

- Idle and ready stance do not move the gameplay root.
- Kick trigger plays one complete strike without double-triggering.
- Celebrate trigger can be interrupted by the next round.
- Goalkeeper dive triggers in the correct direction and returns to a ready state.
- Avatar mapping, foot orientation, scale, clipping and materials are visually inspected in Unity.
- Physics remains independent from animation root motion.
- Profile draw calls, skinning cost, memory and frame pacing on target Android devices.

## Honest status

The code integration points, Humanoid import settings, validation command, appearance binder and Animator parameter bridge are present. Actual FBX/GLB character files, Animator Controllers and verified football-specific kick/dive clips have not yet been imported or tested. The current procedural players remain the fallback. The free Quaternius models are stylized, not the same photoreal level as the generated cinematic reference image.
