# Product research: OneShot Football

**Review date:** 2026-10-23. Public store pages and asset-source pages were reviewed for feature patterns and stated licensing. Store descriptions are publisher-supplied, can change, and do not reveal private retention, revenue, or conversion data. This is not a claim that competitor source code or private analytics were inspected.

## 1. Foreign references

### Football Strike — Miniclip
Official listing: https://play.google.com/store/apps/details?id=com.miniclip.footballstrike

The Google Play listing describes fast free-kick action, online matches against friends or players worldwide, career medals, and player/equipment upgrades. It states that an internet connection is required and discloses optional in-app purchases, including random items. The listing showed an update date of 12 August 2026 at review time.

**Takeaway:** keep our first release playable without an account or network. If an online mode is later added, it must be a deliberate milestone with server costs and anti-cheat considered.

### Score! Hero — First Touch Games
Official listing: https://play.google.com/store/apps/details?id=com.firsttouchgames.story

The listing promotes puzzle-like football actions, a customizable hero, passing/shooting/scoring, single-player play and offline support. It showed 100M+ downloads and an update date of 1 June 2026 at review time, alongside ads and in-app purchases.

**Takeaway:** authored moments and a sense of progression are proven genre patterns. OneShot can add structured challenge scenarios after the basic shot physics are enjoyable.

### Head Ball 2 — Masomo
Official listing: https://play.google.com/store/apps/details?id=com.masomo.headball2

The listing describes real-time 1-v-1 online play, 125 unlockable characters, leagues/brackets, upgradable powers, daily missions, and optional purchases including random items. It showed an update date of 28 September 2026 and states that a network connection is required.

**Takeaway:** character collection and recurring challenges are future options, but they should not delay or complicate the first offline game.

## 2. Iranian references

### سرضرب ۲ — Midopia
- Cafe Bazaar listing: https://cafebazaar.ir/app/com.midopia.Sarzarb2
- Myket listing: https://myket.ir/app/com.midopia.Sarzarb2
- Publisher channel: https://t.me/s/SarZarb_Mobile

This is a relevant local football-game reference and a source for checking Iranian player expectations, localization and store presentation. Current availability, monetization and latest feature status should be rechecked from the live store listings before any direct commercial comparison; no current sales or download figures are asserted here.

### سوپرشوت — historical reference
Historical report: https://www.bazicenter.com/news/game-news/سوپرشوت-برای-موبایل-و-تبلت-منتشر-شد/

A 2014 report discussed a mobile football-distance-shooting game. This is a historical genre precedent only, not evidence that the title is still available or active.

## 3. Competitive feature matrix

| Pattern | Foreign examples | Initial OneShot decision |
| --- | --- | --- |
| Fast free-kick / shot loop | Football Strike | Adopt the broad category, with swipe-release aim and visible trajectory |
| Authored challenge progression | Score! Hero | Add levels only after the repeatable shot loop works |
| Real-time head-to-head | Football Strike, Head Ball 2 | Out of scope for the first milestone |
| Collectible characters and upgrades | Head Ball 2, Score! Hero | Cosmetic unlocks may come later; no pay-to-win design in the prototype |
| Offline availability | Score! Hero listing | Keep local practice playable offline from first launch |
| Ads / in-app purchases | Current foreign listings disclose them | Do not integrate before game quality, privacy, store rules and user experience are reviewed |
| Persian-first positioning | Iranian listing references | Add professionally reviewed Persian localization after controls/UI layout stabilize |

## 4. Chosen product proposition

**OneShot Football** is a portrait-mode, 3D arcade penalty challenge. A short round contains five shots. The player aims by dragging, sees a predicted ball path and releases to shoot against a moving goalkeeper. The best score is stored locally.

The intended difference is a short, low-friction offline session—not a clone of another game's screens, branded content, levels, assets or monetization. The five-shot round is an engineering choice for the first vertical slice, not a claim that it will outperform competitors without play tests.

## 5. Free asset sources and license decisions

| Source | Verified source-page description | Use decision |
| --- | --- | --- |
| Quaternius Universal Base Characters — https://quaternius.com/packs/universalbasecharacters.html | Page lists six game-ready humanoid models, a Humanoid rig, FBX/OBJ/Blend/glTF, and CC0 | Candidate for a later character/animation import; exact downloaded files still require an import and device-performance check |
| Quaternius Universal Animation Library — https://quaternius.com/packs/universalanimationlibrary.html | Page lists 120+ animations, FBX/GLB/Blend and CC0 | Candidate for later run/kick/reaction animation, after retargeting tests |
| Kenney Sports Pack — https://kenney.nl/assets/sports-pack | Page lists a 2D pack (380+ files) with CC0 | Appropriate for some 2D menu elements, not a 3D stadium or 3D footballer |
| Kenney Prototype Kit — https://kenney.nl/assets/prototype-kit | Page lists a 3D pack with 145 files and CC0 | Candidate for generic environment props; its theme is not specifically a football stadium |
| Kenney license FAQ — https://kenney.nl/support | States assets on asset pages are CC0, commercially usable, and attribution is not required | Verify the exact pack page and included license at every integration |

For the initial clone, the playable 3D geometry is generated by original C# at runtime. These third-party binary packs are **not included in the current commit**. That is deliberate: the prototype does not depend on a paid package, a fragile external download or an asset whose import has not been tested. The listed packs are real free options for upgrading presentation later, but listing a pack is not the same as integrating it.

## 6. Quality gates before a public release

1. Unity Editor compile without errors.
2. PlayMode: five-shot round, keeper collision, goal detection, restart, pause and local record.
3. Physical Android test on at least one phone and a second aspect ratio.
4. Performance profile before importing higher-polygon character packs.
5. Review every source file's license and retain required notices.
6. Create genuine gameplay screenshots only after a real build; do not use concept art as if it were gameplay.
7. Recheck the marketplace's current technical, privacy and content requirements before submission.

## 7. Key risks

- Primitive-based characters are intentionally a low-poly blockout; they are not final-quality rigged athletes.
- Physics and swipe sensitivity must be tuned from real device play, not judged from code alone.
- Online services, leaderboards, ads, purchases and analytics add cost and privacy requirements; they are absent from this first version.
- A passing source-check workflow does not prove Unity compilation, playable behavior or release readiness.
