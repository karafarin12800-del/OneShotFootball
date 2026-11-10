# Token Pack Store and Receipt Validation Contract

## Product IDs

All three store products are consumables. Prices are not hardcoded in the game; Unity IAP retrieves localized prices from each platform store.

| Product ID | Tokens |
|---|---:|
| oneshot_tokens_250 | 250 |
| oneshot_tokens_700 | 700 |
| oneshot_tokens_1800 | 1800 |

The IDs must be created with the same spelling in Google Play Console and App Store Connect before the products can be fetched.

## Client behavior

- Unity IAP 5.4.4 is referenced by Packages/manifest.json.
- The client fetches native-store products and displays the localized platform price when available.
- The checkout button remains disabled unless the store is connected, product data has loaded, an HTTPS validation endpoint is configured, and a trusted account-authentication integration supplies a session.
- OnPurchasePending sends the store, SKU, transaction ID, receipt payload and installation ID to the backend.
- The client-side receipt request/response contract is scaffolded, but token-pack checkout is hard-disabled by a compile-time gate. It must stay disabled until the backend response also reconciles an authoritative account wallet and all token spending. The current local PlayerPrefs balance is not a production paid-currency wallet.
- Unity IAP confirms the consumable order only after verification and local credit persistence succeed. Failed or unavailable verification leaves the order pending for retry.

## Required backend security

The game repository does not contain a production receipt-validation server. Do not set the client endpoint or enable checkout until that server exists and has been security-reviewed.

The endpoint must:
1. Require an authenticated player session. The installation ID sent by the client is only a correlation value; it is not authentication.
2. Validate the Google Play purchase token or Apple's signed transaction/receipt with the relevant platform service.
3. Check that the application/bundle ID, store, product ID, transaction state and SKU match the release configuration.
4. Reject refunded, revoked, expired where applicable, forged and replayed transactions.
5. Enforce a unique database constraint on (store, transaction ID) and make retries idempotent.
6. Bind the transaction and token credit to the authenticated player account.
7. Maintain an authoritative server-side wallet and synchronize its balance with the client. Do not rely on PlayerPrefs as the authoritative source for real-money currency.
8. Return JSON with approved, store, transactionId, productId and tokenAmount. These values must exactly match the verified purchase and fixed SKU mapping.
9. Use HTTPS, rate limits, audit logging and safe error responses. Never put service-account JSON, Apple private keys, API secrets or backend credentials in the Unity client or Git repository.

## Current release gate

The local token economy and cosmetic shop work offline, but the account/authentication system, production receipt-validation backend and server-authoritative token wallet do not exist. Therefore real-money token checkout is hard-disabled in code. The installation ID by itself is not a valid user identity. Do not enable this feature until purchases, cosmetic debits, restore/reinstall/refund behavior and wallet synchronization have been tested against the same server-side balance.

Platform parental approval and purchase-confirmation rules must be respected. The game must not bypass family controls or store safeguards.
