# CUE LEGENDS

**AIM. STRIKE. CONQUER.** — a 3D mobile billiards project built with Unity.

## Product direction
CUE LEGENDS converts the former OneShotFootball prototype into a mobile 8-ball pool game. The first playable slice focuses on a generated 3D table, a 16-ball rack, touch/mouse pull-back aiming, Rigidbody collisions, pocket triggers, pause and a local cosmetic cue shop. Practice credits are earned only from valid object-ball pockets; scratch shots do not earn credits, and the 8 ball must be pocketed last to win the solo clear-table round.

## Current status
This work is on the feature/cue-legends-billiards branch. It is not yet store-ready. Unity Editor compilation, Play Mode, Android build and physical-device testing must pass before release claims are made.

**Online matchmaking and store purchases are not active.** They require a configured Unity Multiplayer Services project, a trusted authoritative match service, store product setup and server-side receipt validation. No wagering, paid match entry or cash-out is part of the product; tokens are for direct cosmetic unlocks only. See Docs/CUE_LEGENDS_PRODUCT_PLAN.md.

## Controls
- Drag backward from the cue ball to aim and set power.
- Release to strike.
- Wait until all balls stop before the next shot.
- Practice credits are local prototype values, not purchased currency or match stakes.

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
- [x] Add basic solo clear-table rules and EditMode tests for practice-credit rewards and 8-ball finish conditions.
- [ ] Implement complete tournament-standard 8-ball rules and expand physics/rules tests.
- [ ] Select and integrate an online provider and authoritative backend.
- [ ] Integrate store billing with server-side receipt verification.
- [ ] Complete Android device QA, release signing and store assets.
