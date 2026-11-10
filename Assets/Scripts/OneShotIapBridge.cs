using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;

namespace OneShotFootball
{
    /// <summary>
    /// Unity IAP 5.4 native-store bridge.
    /// Money purchases are deliberately disabled until an HTTPS receipt-validation backend is configured.
    /// Never grant consumable currency merely because the client received an order callback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OneShotIapBridge : MonoBehaviour
    {
        public const string SmallPackId = "oneshot_tokens_250";
        public const string MediumPackId = "oneshot_tokens_700";
        public const string LargePackId = "oneshot_tokens_1800";

        [Header("Configure after creating the secure receipt-validation service")]
        [SerializeField] private string receiptValidationEndpoint = string.Empty;
        [SerializeField] private bool showFamilyApprovalNotice = true;
        private string _authenticatedPlayerSessionToken = string.Empty;

        private StoreController _store;
        private CosmeticShopEconomy _economy;
        private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>();
        private bool _connected;
        private bool _productsFetched;
        private bool _processingOrder;
        private string _status = "Store not configured.";

        public bool IsConnected => _connected;
        public bool CanPurchaseTokenPacks =>
            _connected && _productsFetched && _economy != null && IsSecureEndpointConfigured &&
            !string.IsNullOrWhiteSpace(_authenticatedPlayerSessionToken);
        public string Status => _status;

        private bool IsSecureEndpointConfigured
        {
            get
            {
                if (!Uri.TryCreate(receiptValidationEndpoint, UriKind.Absolute, out Uri uri))
                    return false;
                return uri.Scheme == Uri.UriSchemeHttps && !string.IsNullOrWhiteSpace(uri.Host);
            }
        }

        public void Initialize(CosmeticShopEconomy economy)
        {
            _economy = economy;
        }

        /// <summary>
        /// Called by a trusted account-authentication integration after sign-in.
        /// The bearer token is kept in memory only and is never serialized or logged.
        /// </summary>
        public void SetAuthenticatedPlayerSession(string bearerToken)
        {
            _authenticatedPlayerSessionToken = bearerToken ?? string.Empty;
        }

        public void ClearAuthenticatedPlayerSession()
        {
            _authenticatedPlayerSessionToken = string.Empty;
        }

        private async void Start()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (showFamilyApprovalNotice)
                _status = "Purchases use the official platform flow and its family approval rules.";

            try
            {
                _store = UnityIAPServices.StoreController();
                _store.OnProductsFetched += HandleProductsFetched;
                _store.OnProductsFetchFailed += HandleProductsFetchFailed;
                _store.OnPurchasePending += HandlePurchasePending;
                _store.OnStoreDisconnected += HandleStoreDisconnected;

                await _store.Connect();
                _connected = true;
                _status = IsSecureEndpointConfigured
                    ? "Connected to store. Loading token packs..."
                    : "Store connected. Purchases remain disabled until HTTPS receipt verification is configured.";

                _store.FetchProducts(new List<ProductDefinition>
                {
                    new ProductDefinition(SmallPackId, ProductType.Consumable),
                    new ProductDefinition(MediumPackId, ProductType.Consumable),
                    new ProductDefinition(LargePackId, ProductType.Consumable)
                });
            }
            catch (Exception exception)
            {
                _connected = false;
                _status = "Store connection failed.";
                Debug.LogWarning("OneShot Football IAP initialization failed: " + exception.Message);
            }
#else
            _status = "Token pack checkout is available only in an Android/iOS store build.";
#endif
        }

        public bool IsProductAvailable(string productId)
        {
            return _productsFetched && _products.ContainsKey(productId);
        }

        public string GetLocalizedPrice(string productId)
        {
            if (!_products.TryGetValue(productId, out Product product) || product == null)
                return "Unavailable";
            return product.metadata.localizedPriceString;
        }

        public void PurchaseTokenPack(string productId)
        {
            if (!CanPurchaseTokenPacks)
            {
                _status = "Checkout is disabled until store products and secure receipt verification are ready.";
                return;
            }

            if (!_products.TryGetValue(productId, out Product product) || product == null)
            {
                _status = "This token pack is not available in the current store.";
                return;
            }

            if (CosmeticShopEconomy.TokensForStoreProduct(productId) <= 0)
            {
                _status = "Unknown token pack.";
                return;
            }

            _status = "Waiting for the official store purchase flow...";
            _store.PurchaseProduct(product);
        }

        private void HandleProductsFetched(List<Product> products)
        {
            _products.Clear();
            foreach (Product product in products)
            {
                if (product != null && !string.IsNullOrEmpty(product.definition.id))
                    _products[product.definition.id] = product;
            }

            _productsFetched = true;
            _status = IsSecureEndpointConfigured
                ? "Token packs loaded. Prices are supplied by the store."
                : "Prices loaded. Checkout disabled until HTTPS receipt verification is configured.";

            // Fetching outstanding transactions allows Unity IAP to redeliver unprocessed orders.
            _store?.FetchPurchases();
        }

        private void HandleProductsFetchFailed(ProductFetchFailed failure)
        {
            _productsFetched = false;
            _status = "Could not load token pack prices from the store.";
            Debug.LogWarning("OneShot Football IAP product fetch failed: " + failure);
        }

        private void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            _connected = false;
            _status = "Store connection lost. Try again later.";
            Debug.LogWarning("OneShot Football IAP disconnected: " + failure);
        }

        private void HandlePurchasePending(PendingOrder order)
        {
            if (_processingOrder)
            {
                _status = "A previous purchase is still being verified.";
                return;
            }

            if (!IsSecureEndpointConfigured || _economy == null)
            {
                _status = "Purchase is pending. Configure receipt verification before currency can be granted.";
                return;
            }

            var items = order.CartOrdered.Items().ToList();
            if (items.Count != 1 || items[0].Product == null)
            {
                _status = "Unsupported purchase order. No tokens were granted.";
                return;
            }

            Product product = items[0].Product;
            int expectedTokens = CosmeticShopEconomy.TokensForStoreProduct(product.definition.id);
            string transactionId = order.Info != null ? order.Info.TransactionID : string.Empty;
            string receipt = order.Info != null ? order.Info.Receipt : string.Empty;

            if (expectedTokens <= 0 || string.IsNullOrWhiteSpace(transactionId) || string.IsNullOrWhiteSpace(receipt))
            {
                _status = "Purchase details are incomplete. The order will remain pending.";
                return;
            }

            StartCoroutine(ValidateReceiptAndFulfil(order, product.definition.id, transactionId, receipt, expectedTokens));
        }

        private IEnumerator ValidateReceiptAndFulfil(PendingOrder order, string productId,
            string transactionId, string receipt, int expectedTokens)
        {
            _processingOrder = true;
            _status = "Verifying purchase with the secure service...";

            var payload = new ReceiptValidationRequest
            {
                store = Application.platform == RuntimePlatform.Android ? "GooglePlay" : "AppleAppStore",
                productId = productId,
                transactionId = transactionId,
                receipt = receipt,
                installationId = GetInstallationId()
            };

            byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
            using (var request = new UnityWebRequest(receiptValidationEndpoint, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + _authenticatedPlayerSessionToken);
                request.timeout = 25;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    _status = "Receipt verification unavailable. No tokens were granted; the order remains pending.";
                    _processingOrder = false;
                    yield break;
                }

                ReceiptValidationResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<ReceiptValidationResponse>(request.downloadHandler.text);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("OneShot Football receipt response could not be parsed: " + exception.Message);
                }

                bool responseMatches = response != null
                    && response.approved
                    && response.store == payload.store
                    && response.transactionId == transactionId
                    && response.productId == productId
                    && response.tokenAmount == expectedTokens;

                if (!responseMatches || !_economy.GrantVerifiedStoreTokenPack(productId, transactionId, expectedTokens))
                {
                    _status = "Receipt was not approved. No tokens were granted.";
                    _processingOrder = false;
                    yield break;
                }

                // Confirm only after the backend has verified the exact SKU and transaction
                // and the local idempotent credit has been persisted.
                _store.ConfirmPurchase(order);
                _status = "Purchase verified. " + expectedTokens + " tokens added.";
            }

            _processingOrder = false;
        }

        private static string GetInstallationId()
        {
            const string key = "OneShotFootball.InstallationId.v1";
            string id = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(key, id);
                PlayerPrefs.Save();
            }
            return id;
        }

        private void OnDestroy()
        {
            if (_store == null) return;
            _store.OnProductsFetched -= HandleProductsFetched;
            _store.OnProductsFetchFailed -= HandleProductsFetchFailed;
            _store.OnPurchasePending -= HandlePurchasePending;
            _store.OnStoreDisconnected -= HandleStoreDisconnected;
        }

        [Serializable]
        private sealed class ReceiptValidationRequest
        {
            public string store;
            public string productId;
            public string transactionId;
            public string receipt;
            public string installationId;
        }

        [Serializable]
        private sealed class ReceiptValidationResponse
        {
            public bool approved;
            public string store;
            public string transactionId;
            public string productId;
            public int tokenAmount;
        }
    }
}