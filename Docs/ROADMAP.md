# Roadmap and acceptance criteria

## Milestone 0 — Repository foundation
- [x] Named repository: OneShotFootball
- [x] Unity 6.3 LTS project baseline
- [x] Source/license register and competitor research
- [x] GitHub source validation

## Milestone 1 — Offline playable vertical slice
- [x] Runtime-created 3D pitch, markings, goal frame and net
- [x] Stylized player and moving goalkeeper from project-owned primitives
- [x] Ball physics and swipe-release shooting
- [x] Visible aim-trajectory preview
- [x] Goal/miss/save outcomes and five-shot round
- [x] Pause/resume, replay and locally persisted best score
- [x] Editor bootstrap creates the initial scene and build-scene list
- [ ] Unity Console clean compile (not yet verified in Editor)
- [ ] Confirm touch/GUI controls do not conflict on a physical Android device
- [ ] Tune physics, camera framing and goalkeeper difficulty from real play tests

## Milestone 2 — Feel and presentation
- [x] Add optional Humanoid Animator bridge for kicker and goalkeeper
- [x] Add goalkeeper shot-target reaction and left/right dive trigger hooks
- [ ] Import licensed Humanoid athlete FBX files and verify Avatar mapping
- [ ] Import or create football-specific kick and keeper-dive clips
- [ ] Polish ball spin and goal/save feedback
- [ ] Improved seating, goal-net movement and lightweight VFX
- [ ] Original sound effects with rights documented
- [ ] Settings: sound, haptics and aim sensitivity
- [ ] Persian and English localization

## Milestone 3 — Content
- [ ] Challenge levels: target zones, moving targets, wall and time limit
- [x] Cosmetic shop UI for players, kits, equipment and balls
- [x] Free starter cosmetic per category; additional cosmetics use deterministic token prices
- [x] Earned-token reward after completing a five-shot round
- [x] Local cosmetic ownership and equipped-state persistence
- [ ] Replace player appearance presets with separately imported rigged player prefabs
- [ ] Connect token packs to authenticated player accounts and a production receipt-validation service
- [ ] Persist a server-authoritative purchased-token balance
- [ ] Additional stadium themes after profiling

## Milestone 4 — Release readiness
- [ ] Produce a signed, installable APK and test on devices
- [ ] Confirm package ID, versioning, privacy, permission list and store assets
- [ ] Create Persian/English listings and authentic gameplay screenshots
- [ ] Check the store's current target API requirements
- [ ] Complete the rights audit for all bundled content
- [ ] Decide on online features only after the offline release is stable

## Definition of done
Source-check success alone is not a playable-build sign-off. Milestone 1 is validated only when Unity has no compilation errors, aim/physics/keeper behavior are tested in Play Mode, and a real Android device completes multiple five-shot rounds without UI/input conflicts.
