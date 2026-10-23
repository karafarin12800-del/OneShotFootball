# Product research: OneShot Football

Research date: 2026-10-23. This is product-pattern research, not a claim that private analytics, internal source code or competitor assets were inspected.

## Foreign reference products

### Football Strike — Miniclip
https://play.google.com/store/apps/details?id=com.miniclip.footballstrike

Useful pattern to study: quick free-kick and goalkeeping challenges. OneShot may borrow only broad genre conventions, not its code, models, UI, logos, animations or screenshots.

### Score! Hero — First Touch Games
https://play.google.com/store/apps/details?id=com.firsttouchgames.hero

Useful pattern: level-based progression through decisive ball actions. The first OneShot milestone uses five shots per round to keep the loop fast and testable.

### Head Ball 2 — Masomo
https://play.google.com/store/apps/details?id=com.masomo.headball2

Its public Play listing describes real-time 1-v-1 matches, progression, character unlocks and optional purchases. These show possible future expansion paths but also add online, service and content-production complexity. The first OneShot build is intentionally offline.

## Iranian reference products

### سرضرب ۲ — Midopia
- Publisher channel: https://t.me/s/SarZarb_Mobile
- Bazaar: https://cafebazaar.ir/app/com.midopia.Sarzarb2
- Myket: https://myket.ir/app/com.midopia.Sarzarb2

Useful pattern: a localized Iranian football game with stadium customization and progression/community features. Current availability and listing details should be rechecked before release comparisons. No assets or source code are copied.

### سوپرشوت (historical reference)
A report from 2014 described an Android game about shooting for distance and comparing fan-team scores:
https://www.bazicenter.com/news/game-news/سوپرشوت-برای-موبایل-و-تبلت-منتشر-شد/

This is an old report, not evidence that the game is still available or active. It is only a historical genre reference.

## Product direction selected

**OneShot Football = offline-first, 3D arcade penalty challenge.** The user has five shots per round; a moving goalkeeper adds timing and placement skill. Short rounds keep the gameplay simple enough to validate before online features and monetization.

### Differentiation priorities
1. Portrait-first touch input: swipe aim, release shoot, visible trajectory preview.
2. Fast reset and five-shot rounds.
3. Local high-score persistence that works offline.
4. Self-contained procedural art baseline with no mandatory paid models.
5. Persian/English localization after gameplay stability.
6. Future modes: corner targets, moving targets, timed challenges and stadium palettes.

## Out of scope for the first prototype

Online multiplayer, global leaderboards, user accounts, live events, advertisements, in-app purchases, licensed clubs, real player likenesses and commentary are out of scope until the offline gameplay works and costs/rights are understood.

## Risks and mitigations

- Controls vary by screen size: test on multiple portrait aspect ratios.
- Procedural characters are stylized rather than photorealistic: add rigged models only after exact license review and device validation.
- Moving keeper may feel unfair: tune it from real play tests.
- Physics can feel different by device: use Rigidbody physics and test on a phone.
- Do not imply online or monetization features before they are implemented.
