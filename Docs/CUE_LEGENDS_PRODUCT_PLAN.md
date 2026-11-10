# CUE LEGENDS — Product and Engineering Plan

## Product identity
- Public name: CUE LEGENDS
- Category: 3D mobile pool / billiards
- First platform: Android; iOS follows after input, performance and store validation.
- Core mode: 8-ball pool; offline practice remains available without an account.
- Visual direction: premium dark tournament room, emerald cloth, walnut rails and warm brass accents.

## Current implementation milestone
PoolGameController replaces the football prototype as the scene entry point and generates a 3D pool-table blockout at runtime. It includes a cue ball, 15 numbered balls, rack setup, touch/mouse pull-back aiming, Rigidbody collisions, six pocket triggers, pause, a local practice-credit counter and a local cosmetic cue shop.

The practice loop now settles rewards after each shot: only valid object-ball pockets earn practice credits; a cue-ball scratch invalidates that shot's credit reward; and the 8 ball wins only when it is pocketed after the other object balls, without a scratch on that shot. These are solo clear-table practice rules, not a claim of complete tournament-standard 8-ball rules.

This remains a prototype. Full WPA/BCA rules, legal-break validation, first-contact checks, called shots, ball-in-hand, solids/stripes assignment, opponent turn ownership and authoritative online results are not complete.

## Online architecture — required before enabling multiplayer
Use Unity Multiplayer Services (MPS) Sessions with Netcode for GameObjects (NGO) as the initial Unity-side connection/session integration for this GameObject-based project. MPS Sessions can coordinate player grouping and connection setup; Relay can route peer traffic, but a Relay/P2P host is not a trusted dedicated game server.

For production competitive matches and any currency grants, use a trusted authoritative server/service. A client submits only intended cue input (aim vector, power, spin); it must not submit authoritative ball positions, pocket outcomes, match winner or currency rewards. The authority validates turn ownership and legal state, simulates or verifies physics, and broadcasts sequenced state snapshots/events. Reconnects restore the authoritative match snapshot. Matchmaking filters by region, latency, skill rating and queue timeout. Add rate limits, replay detection, input bounds and match audit logs.

Do not enable token wagering, paid match entry, cash-out, or prize payouts. Purchased or earned tokens are limited to direct cosmetic unlocks; no random paid rewards.

Staged rollout:
1. Local practice and deterministic rules tests.
2. Select a networking provider and complete a two-device proof.
3. Implement authoritative match state, matchmaking, private rooms and reconnect.
4. Add ranked seasons, leaderboard and anti-cheat telemetry.
5. Add production monitoring, moderation, incident response and regional capacity.

No networking SDK, backend endpoint, credentials or production server is currently configured. The menu labels online matchmaking as not enabled rather than pretending a live match exists.

## Tokens, shop and real-money purchases
- Prototype tokens are local practice credits stored with PlayerPrefs. They are not secure and must never be treated as purchased currency.
- Production wallet balance and every grant/spend must be server-side and recorded in an append-only transaction ledger with idempotency keys.
- The client may request a purchase; it must never award premium currency merely because a local callback says success.
- Integrate Unity In-App Purchasing only after store products, merchant accounts, region availability, restore flow and backend receipt validation are configured.
- Suggested catalog: small/medium/large token packs; optional cosmetic cue/table bundles; no paid gameplay advantage. Prices are configured in each store, not hard-coded in client code.
- Add restore, pending transaction handling, refund/revocation handling, receipt validation, duplicate callback protection and purchase audit events.

## Store and release checklist
- Android application id: com.cuelegends.pool
- Configure app icon, adaptive icon, splash screen, signing, privacy policy and support contact.
- Validate current Google Play Billing and target API requirements before release.
- Test offline practice, suspend/resume, touch accuracy, memory, frame pacing and multiple aspect ratios.
- Test online with at least two physical devices and poor-network/reconnect scenarios before claiming multiplayer works.
- Do not publish concept images as gameplay screenshots.

## Known prototype limitations
- Physics uses Unity Rigidbody and needs tuning on devices.
- The first pass has simplified 8-ball rules and a local-only shop/wallet.
- Online multiplayer and real-money IAP are not active.
- The game scene is generated procedurally; final art, audio, accessibility, localization and store assets remain separate tasks.
