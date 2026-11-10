# Pro Shop and Player Customization

## Product direction
The shop is a cosmetic collection system for the single-player five-shot game. It must not sell gameplay advantages, random loot boxes, gambling-style rewards, or paid chances. All prices and token costs are visible before confirmation.

## Categories
- Players
- Kits / uniforms
- Equipment and accessories
- Footballs

Each category includes a free starter item. Additional deterministic cosmetic items are bought with earned in-game tokens. Ownership and equipped selection persist locally for the offline prototype.

## Economy
- Complete one five-shot round to receive a transparent token reward.
- Reward formula in the current foundation: 20 base + 8 per goal + up to 25 score-derived tokens.
- Tokens are soft currency; cosmetic items only.
- No random rewards and no pay-to-win attributes.
- Token pack purchases are not enabled by the offline wallet. A production Unity IAP adapter must verify store transactions, handle restore/refund, and prevent duplicate grants before calling the verified grant entry point.
- Before real-money purchase is enabled, add clear localized prices, parental approval flow where required, platform-compliant purchase/restore handling, privacy disclosures, and customer support/refund guidance. Never represent local PlayerPrefs as secure purchase validation.

## Current implementation status
- The main menu opens the in-game shop.
- Category tabs cover Players, Kits, Equipment and Balls.
- Each category has a free starter item and additional deterministic token-priced cosmetics.
- Buying an item deducts tokens once, saves ownership, and allows equip/unequip.
- Cosmetic selection updates the current procedural appearance (player items are appearance presets until separate 3D player prefabs are imported).
- Completing the fifth shot in a round grants one token reward and the result panel shows the reward/balance.
- PlayerPrefs persistence is adequate only for the offline prototype; it is not secure storage for purchased currency.
- A token-pack page and Unity IAP 5.4.4 native-store bridge are present. Product IDs are defined, and localized prices can be read from the platform store. The checkout buttons remain hard-disabled until the account service, secure receipt-verification backend and server-authoritative token wallet (including cosmetic debit/synchronization) are implemented and verified.
- Cash checkout remains disabled unless an HTTPS receipt-validation endpoint and trusted account-authentication session are configured. The repository does not include the backend or account provider.

## Art pipeline
The Humanoid importer, validator, optional Resources prefab loader, appearance binder and Animator parameter bridge are in place. Quaternius Universal Base Characters and Universal Animation Library are documented CC0 candidates, but the actual FBX binaries, Animator Controllers and football-specific kick/dive clips are not yet imported. Follow Docs/CHARACTER_ASSET_PIPELINE.md. Keep the procedural fallback until the imported rig is verified in Unity.

## Acceptance criteria
1. Shop opens from the main menu and category tabs work on touch.
2. Each category has at least one free starter cosmetic.
3. Locked items show exact token cost; insufficient balance never deducts tokens.
4. Purchase deducts exactly once and grants permanent ownership.
5. Owned items can be equipped and the selected visual changes in the live scene.
6. State survives app restart and malformed save data safely resets/migrates.
7. Completing a five-shot round grants one reward only.
8. Store purchases use verified platform receipts, restore correctly, and never grant twice.
9. No randomized paid content, pay-to-win stats, or misleading pricing.
10. EditMode tests, Unity compile, PlayMode shop flow, Android build, and physical-device touch tests pass.
