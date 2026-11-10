using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneShotFootball
{
    public enum CosmeticCategory { Player, Kit, Equipment, Ball }

    [Serializable]
    public sealed class CosmeticItem
    {
        public string id;
        public string displayName;
        public CosmeticCategory category;
        [Min(0)] public int tokenPrice;
        public bool starterFree;
        [TextArea] public string description;

        public CosmeticItem(string id, string displayName, CosmeticCategory category, int tokenPrice, bool starterFree, string description)
        {
            this.id = id; this.displayName = displayName; this.category = category;
            this.tokenPrice = tokenPrice; this.starterFree = starterFree; this.description = description;
        }
    }

    /// <summary>
    /// Offline cosmetic ownership and token ledger. Real-money token packs must be granted
    /// only by a verified store receipt adapter; PlayerPrefs is not purchase verification.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CosmeticShopEconomy : MonoBehaviour
    {
        private const string SaveKey = "OneShotFootball.ShopState.v1";
        private const int TokensPerRoundBase = 20;

        [Serializable]
        private sealed class SaveData
        {
            public int tokens;
            public List<string> ownedIds = new List<string>();
            public List<string> processedStoreTransactions = new List<string>();
            public string equippedPlayer;
            public string equippedKit;
            public string equippedEquipment;
            public string equippedBall;
            public int schemaVersion = 1;
        }

        [SerializeField] private List<CosmeticItem> catalog = new List<CosmeticItem>();
        private SaveData _save;
        public int TokenBalance => _save == null ? 0 : _save.tokens;
        public IReadOnlyList<CosmeticItem> Catalog => catalog;
        public event Action StateChanged;

        private void Awake()
        {
            if (catalog == null || catalog.Count == 0) BuildDefaultCatalog();
            Load();
            EnsureStarterOwnership();
        }

        private void BuildDefaultCatalog()
        {
            catalog = new List<CosmeticItem>
            {
                new CosmeticItem("player.striker.blue", "Academy Striker", CosmeticCategory.Player, 0, true, "Starter player"),
                new CosmeticItem("player.pro.forward", "Pro Forward Look", CosmeticCategory.Player, 600, false, "Appearance preset for the current player model"),
                new CosmeticItem("player.street.legend", "Street Legend Look", CosmeticCategory.Player, 900, false, "Appearance preset for the current player model"),
                new CosmeticItem("kit.classic.blue", "Classic Blue", CosmeticCategory.Kit, 0, true, "Starter home kit"),
                new CosmeticItem("kit.neon", "Neon Pulse", CosmeticCategory.Kit, 180, false, "Alternate kit appearance"),
                new CosmeticItem("kit.midnight", "Midnight Gold", CosmeticCategory.Kit, 300, false, "Alternate kit appearance"),
                new CosmeticItem("equipment.basic.boots", "Training Boots", CosmeticCategory.Equipment, 0, true, "Starter equipment appearance"),
                new CosmeticItem("equipment.speed.boots", "Velocity Boots", CosmeticCategory.Equipment, 220, false, "Cosmetic boots"),
                new CosmeticItem("equipment.captain.band", "Captain Armband", CosmeticCategory.Equipment, 140, false, "Cosmetic accessory"),
                new CosmeticItem("ball.classic", "Classic Match Ball", CosmeticCategory.Ball, 0, true, "Starter football"),
                new CosmeticItem("ball.neon", "Neon Match Ball", CosmeticCategory.Ball, 160, false, "Alternate ball appearance"),
                new CosmeticItem("ball.gold", "Gold Edition Ball", CosmeticCategory.Ball, 280, false, "Alternate ball appearance")
            };
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            try
            {
                _save = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("OneShot Football: shop save is invalid and was reset. " + exception.Message);
                _save = new SaveData();
            }
            if (_save == null || _save.schemaVersion != 1) _save = new SaveData();
            if (_save.ownedIds == null) _save.ownedIds = new List<string>();
            if (_save.processedStoreTransactions == null) _save.processedStoreTransactions = new List<string>();
            _save.tokens = Mathf.Max(0, _save.tokens);
        }

        private void EnsureStarterOwnership()
        {
            bool changed = false;
            foreach (CosmeticItem item in catalog)
            {
                if (item == null || !item.starterFree || string.IsNullOrWhiteSpace(item.id)) continue;
                if (!_save.ownedIds.Contains(item.id)) { _save.ownedIds.Add(item.id); changed = true; }
                if (string.IsNullOrEmpty(GetEquippedId(item.category))) { SetEquippedId(item.category, item.id); changed = true; }
            }
            if (changed) Save();
        }

        public bool IsOwned(string itemId) => _save != null && !string.IsNullOrEmpty(itemId) && _save.ownedIds.Contains(itemId);

        public bool TryPurchaseWithTokens(string itemId)
        {
            CosmeticItem item = FindItem(itemId);
            if (item == null || item.starterFree || IsOwned(itemId) || item.tokenPrice <= 0 || _save.tokens < item.tokenPrice)
                return false;
            _save.tokens -= item.tokenPrice;
            _save.ownedIds.Add(item.id);
            Save();
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEquip(string itemId)
        {
            CosmeticItem item = FindItem(itemId);
            if (item == null || !IsOwned(itemId)) return false;
            SetEquippedId(item.category, item.id);
            Save();
            StateChanged?.Invoke();
            return true;
        }

        public string GetEquippedId(CosmeticCategory category)
        {
            if (_save == null) return string.Empty;
            switch (category)
            {
                case CosmeticCategory.Player: return _save.equippedPlayer;
                case CosmeticCategory.Kit: return _save.equippedKit;
                case CosmeticCategory.Equipment: return _save.equippedEquipment;
                case CosmeticCategory.Ball: return _save.equippedBall;
                default: return string.Empty;
            }
        }

        /// <summary>Call once after a completed five-shot round, never per frame or per shot.</summary>
        public int AwardCompletedRoundTokens(int score, int goals)
        {
            if (_save == null) return 0;
            int reward = TokensPerRoundBase + Mathf.Clamp(goals, 0, 5) * 8 + Mathf.Clamp(score / 100, 0, 25);
            _save.tokens = Mathf.Min(999999, _save.tokens + reward);
            Save();
            StateChanged?.Invoke();
            return reward;
        }

        public static int TokensForStoreProduct(string productId)
        {
            switch (productId)
            {
                case "oneshot_tokens_250": return 250;
                case "oneshot_tokens_700": return 700;
                case "oneshot_tokens_1800": return 1800;
                default: return 0;
            }
        }

        /// <summary>
        /// Grants a fixed token pack only after the receipt-validation backend approves
        /// the exact product and transaction. This method is internal to the runtime assembly.
        /// </summary>
        internal bool GrantVerifiedStoreTokenPack(string productId, string transactionId, int tokenAmount)
        {
            int expectedAmount = TokensForStoreProduct(productId);
            if (_save == null || expectedAmount <= 0 || tokenAmount != expectedAmount ||
                string.IsNullOrWhiteSpace(transactionId) || transactionId.Length > 256)
                return false;

            string transactionKey = productId + ":" + transactionId;
            if (_save.processedStoreTransactions.Contains(transactionKey))
                return true;

            _save.tokens = Mathf.Min(999999, _save.tokens + expectedAmount);
            _save.processedStoreTransactions.Add(transactionKey);
            Save();
            StateChanged?.Invoke();
            return true;
        }

        private CosmeticItem FindItem(string id) => string.IsNullOrEmpty(id) ? null : catalog.Find(item => item != null && item.id == id);

        private void SetEquippedId(CosmeticCategory category, string id)
        {
            switch (category)
            {
                case CosmeticCategory.Player: _save.equippedPlayer = id; break;
                case CosmeticCategory.Kit: _save.equippedKit = id; break;
                case CosmeticCategory.Equipment: _save.equippedEquipment = id; break;
                case CosmeticCategory.Ball: _save.equippedBall = id; break;
            }
        }

        private void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_save));
            PlayerPrefs.Save();
        }
    }
}