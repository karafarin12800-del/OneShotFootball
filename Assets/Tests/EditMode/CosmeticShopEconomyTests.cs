using NUnit.Framework;
using UnityEngine;
using OneShotFootball;

namespace OneShotFootball.Tests
{
    public sealed class CosmeticShopEconomyTests
    {
        private GameObject _host;
        private CosmeticShopEconomy _shop;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("OneShotFootball.ShopState.v1");
            _host = new GameObject("Shop Economy Test");
            _shop = _host.AddComponent<CosmeticShopEconomy>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            PlayerPrefs.DeleteKey("OneShotFootball.ShopState.v1");
        }

        [Test]
        public void StarterItemsAreOwnedAndEquipped()
        {
            Assert.IsTrue(_shop.IsOwned("player.striker.blue"));
            Assert.IsTrue(_shop.IsOwned("kit.classic.blue"));
            Assert.AreEqual("ball.classic", _shop.GetEquippedId(CosmeticCategory.Ball));
        }

        [Test]
        public void PurchaseFailsWhenTokenBalanceIsInsufficient()
        {
            Assert.IsFalse(_shop.TryPurchaseWithTokens("kit.neon"));
            Assert.AreEqual(0, _shop.TokenBalance);
            Assert.IsFalse(_shop.IsOwned("kit.neon"));
        }

        [Test]
        public void CompletedRoundsAwardTokensAndAllowCosmeticPurchase()
        {
            for (int i = 0; i < 8; i++)
                _shop.AwardCompletedRoundTokens(0, 0);

            Assert.AreEqual(160, _shop.TokenBalance);
            Assert.IsTrue(_shop.TryPurchaseWithTokens("ball.neon"));
            Assert.IsTrue(_shop.IsOwned("ball.neon"));
            Assert.AreEqual(0, _shop.TokenBalance);
        }

        [Test]
        public void OwnedCosmeticCanBeEquipped()
        {
            Assert.IsTrue(_shop.TryEquip("kit.classic.blue"));
            Assert.AreEqual("kit.classic.blue", _shop.GetEquippedId(CosmeticCategory.Kit));
        }
    }
}