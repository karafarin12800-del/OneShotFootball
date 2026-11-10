# OneShot Football

**AIM. CURVE. SCORE.** — بازی فوتبال آرکید سه‌بعدی برای اندروید با کنترل لمسی و چالش ضربه پنالتی.

## وضعیت پروژه / Project status

این مخزن نسخه اولیه یک بازی آفلاین را تعریف می‌کند. زمین، دروازه، توپ، بازیکن، دروازه‌بان متحرک، مسیر نشانه‌گیری، امتیاز، دورهای پنج‌شاتی، مکث و رکورد با کد Unity ساخته می‌شوند؛ بنابراین شروع بازی وابسته به خرید یا دانلود یک بسته دارایی نیست.

> شفافیت: سورس و ساختار اولیه در مخزن ثبت شده‌اند؛ این commit هنوز خروجی Unity یا APK آزمایش‌شده نیست. اجرای واقعی نیاز به بازکردن پروژه در Unity و آزمایش روی دستگاه دارد. ساخت Android در GitHub Actions به مجوز معتبر Unity نیاز دارد.

## Game loop

1. پنج شوت در هر دور.
2. کشیدن انگشت برای تعیین جهت و ارتفاع توپ.
3. رها کردن انگشت برای شوت.
4. دروازه‌بان به‌صورت خودکار در امتداد دروازه حرکت می‌کند.
5. گل‌ها امتیاز می‌دهند و رکورد روی دستگاه ذخیره می‌شود.
6. بازی آفلاین است. فروشگاه تزئینی و اقتصاد توکنِ قابل‌کسب در نسخه آزمایشی پیاده‌سازی شده‌اند. خرید واقعی توکن تا زمان اتصال حساب کاربر و تأیید امن رسید خرید در سرور غیرفعال است.

## Tech stack

- Unity 6000.3.27f2 (Unity 6.3 LTS patched baseline)
- C#; runtime-generated 3D meshes/materials from original code
- Android portrait mode; touch controls and mouse controls for Editor/desktop
- GitHub Actions source checks
- Optional GameCI Android workflow; requires a valid Unity license secret

## Open in Unity

1. Clone this repository or download its ZIP.
2. Open the project root in Unity Hub (folder containing Assets, Packages and ProjectSettings).
3. Install the exact Editor version in ProjectSettings/ProjectVersion.txt.
4. Wait for import. The Editor bootstrap creates Assets/Scenes/Main.unity and registers it in Build Settings if missing.
5. Open Assets/Scenes/Main.unity and press Play.
6. For Android, install Android Build Support, SDK/NDK tools and OpenJDK, then build for Android.

The stadium, ball and fallback characters are generated from project-owned code at runtime. The store now supports player appearance presets, kits, equipment and ball cosmetics bought with earned tokens. Optional Humanoid prefab and Animator hooks are ready, but no licensed character or animation binaries are bundled yet. See Docs/CHARACTER_ASSET_PIPELINE.md. The procedural fallback remains low-poly.

## Controls

- Swipe left/right to aim horizontally.
- Swipe upward to raise the shot.
- Release to shoot.
- Pause is in the top-right HUD.
- On desktop, click-drag-release with the mouse.

## Source and licensing

See ASSET_LICENSES.md and Docs/COMPETITOR_RESEARCH.md. The original store-icon vector source is in BrandAssets/OneShotFootballIcon.svg; it is not yet wired into Unity player icon settings. No third-party art/model binaries are bundled yet. Quaternius CC0 assets are documented as the first rigged-character candidate; the import and validation pipeline is prepared, but actual FBX files still need to be downloaded, inspected and imported. See ASSET_LICENSES.md.

## Build status

- Source checks: .github/workflows/source-check.yml
- Android build and test guide: Docs/BUILD_AND_TEST.md

A green source-check workflow is not proof that Unity compiled the game. A successful Android build and a real-device test are separate acceptance gates.

## Planned milestones

- [x] Create the named GitHub repository and define scope.
- [x] Add Unity project baseline, procedural gameplay, Editor bootstrap and source/license documentation.
- [ ] Verify Unity compilation in Editor and fix any Console errors.
- [ ] Test touch aim, save/goal detection, pause/resume and five-shot completion on Android.
- [x] Add shop/token economy and optional Humanoid Animator integration points.
- [ ] Import and verify the real rigged athlete models and football-specific animation clips.
- [ ] Connect production token purchases to authenticated receipt verification.
- [ ] Improve stadium detail, sound and accessibility.
- [ ] Produce and device-test an APK.
- [ ] Only after offline gameplay is stable, evaluate online features.

## English

OneShot Football is an original portrait-mode 3D arcade penalty challenge. Aim with a swipe, release to shoot, beat the moving goalkeeper and finish a five-shot round. The first version is offline and uses code-generated geometry so its first playable build has no mandatory paid art dependencies.
