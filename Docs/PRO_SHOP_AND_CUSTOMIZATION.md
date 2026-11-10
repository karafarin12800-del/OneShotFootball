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

## Current implementation boundary
CosmeticShopEconomy.cs provides a catalog, free starter ownership, token purchase/equip logic, local persistence, and a verified-purchase integration seam. It is not yet connected to the gameplay result event, character/ball renderers, shop UI, Unity IAP, or store product configuration.

## Art pipeline
Professional athlete models and animations must be imported as licensed FBX/GLB assets with humanoid rigs. The reviewed Quaternius Universal Base Characters and Universal Animation Library are candidate CC0 sources, but neither is currently bundled. Before integration, validate the exact files, humanoid avatar mapping, kick/run/idle clips, goalkeeper dive clips, materials, scale, Android memory and draw-call cost. Keep a working procedural fallback until the imported rig is verified in Unity 6.3.

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
