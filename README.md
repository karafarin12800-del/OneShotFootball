# CUE LEGENDS

**AIM. STRIKE. CONQUER.** — a 3D mobile billiards project built with Unity.

## Product direction
CUE LEGENDS converts the former OneShotFootball prototype into a mobile 8-ball pool game. The first playable slice focuses on a generated 3D table, a 16-ball rack, touch/mouse pull-back aiming, Rigidbody collisions, pocket triggers, pause and a local practice-token cue shop.

## Current status
This work is on the feature/cue-legends-billiards branch. It is not yet store-ready. Unity Editor compilation, Play Mode, Android build and physical-device testing must pass before release claims are made.

**Online matchmaking and real-money in-app purchases are not active.** They require a production backend, a server-authoritative match model, a selected networking provider, store product configuration and server-side receipt validation. See Docs/CUE_LEGENDS_PRODUCT_PLAN.md.

## Controls
- Drag backward from the cue ball to aim and set power.
- Release to strike.
- Wait until all balls stop before the next shot.
- Practice tokens are local prototype credits, not purchased currency.

## Tech
- Unity 6000.3.27f1
- C# and Unity Physics
- Runtime-generated table and ball geometry
- Android-first, landscape orientation
- GitHub Actions source validation; Unity Android build requires a valid UNITY_LICENSE secret.

## Open
1. Open the repository root in Unity Hub using version 6000.3.27f1.
2. Wait for import and C# compilation.
3. Open Assets/Scenes/Main.unity and press Play.
4. Test aim, collisions, pockets, pause and shop.
5. For Android, install Android Build Support, SDK/NDK and OpenJDK, then use CUE LEGENDS/Build Android APK.

## Milestones
- [x] Establish CUE LEGENDS identity and Android package id.
- [x] Replace the football scene bootstrap with a generated billiards prototype.
- [x] Add local practice-token and cosmetic cue-shop prototype.
- [ ] Validate compilation, physics and UI in Unity Editor.
- [ ] Tune 8-ball rules and add automated rules tests.
- [ ] Select and integrate an online provider and authoritative backend.
- [ ] Integrate store billing with server-side receipt verification.
- [ ] Complete Android device QA, release signing and store assets.
