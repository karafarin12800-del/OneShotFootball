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
6. بازی ابتدا آفلاین است. آنلاین، جدول سراسری، خرید درون‌برنامه‌ای و تبلیغات تا زمان طراحی و آزمایش جداگانه فعال نیستند.

## Tech stack

- Unity 6000.3.27f1 (Unity 6.3 LTS baseline)
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

The stadium, ball and stylized characters are generated from project-owned code at runtime. The first clone therefore has the required 3D geometry without paid assets. The resulting art is a low-poly prototype, not yet a polished, rigged character pack.

## Controls

- Swipe left/right to aim horizontally.
- Swipe upward to raise the shot.
- Release to shoot.
- Pause is in the top-right HUD.
- On desktop, click-drag-release with the mouse.

## Source and licensing

See ASSET_LICENSES.md and Docs/COMPETITOR_RESEARCH.md. The original store-icon vector source is in BrandAssets/OneShotFootballIcon.svg; it is not yet wired into Unity player icon settings. No third-party art/model binaries are bundled in this initial commit. We reviewed public asset sources as candidates, but this baseline deliberately stays playable without downloading uncertain assets or buying asset packs.

## Build status

- Source checks: .github/workflows/source-check.yml
- Android build and test guide: Docs/BUILD_AND_TEST.md

A green source-check workflow is not proof that Unity compiled the game. A successful Android build and a real-device test are separate acceptance gates.

## Planned milestones

- [x] Create the named GitHub repository and define scope.
- [x] Add Unity project baseline, procedural gameplay, Editor bootstrap and source/license documentation.
- [ ] Verify Unity compilation in Editor and fix any Console errors.
- [ ] Test touch aim, save/goal detection, pause/resume and five-shot completion on Android.
- [ ] Improve character animation, stadium detail, sound and accessibility.
- [ ] Produce and device-test an APK.
- [ ] Only after offline gameplay is stable, evaluate online features.

## English

OneShot Football is an original portrait-mode 3D arcade penalty challenge. Aim with a swipe, release to shoot, beat the moving goalkeeper and finish a five-shot round. The first version is offline and uses code-generated geometry so its first playable build has no mandatory paid art dependencies.
