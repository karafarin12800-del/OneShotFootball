using NUnit.Framework;
using UnityEngine;
using OneShotFootball;

namespace OneShotFootball.Tests
{
    public sealed class ShotScoringRulesTests
    {
        [TestCase(0, 100)]
        [TestCase(1, 100)]
        [TestCase(2, 125)]
        [TestCase(3, 150)]
        [TestCase(5, 200)]
        public void PointsIncreaseWithGoalStreak(int streak, int expected)
        {
            Assert.AreEqual(expected, ShotScoringRules.PointsForStreak(streak));
        }

        [Test]
        public void CentreOfGoalIsAccepted()
        {
            Assert.IsTrue(ShotScoringRules.IsGoalCandidate(new Vector3(0f, 1.2f, 14f)));
        }

        [Test]
        public void BallOutsidePostIsRejected()
        {
            Assert.IsFalse(ShotScoringRules.IsGoalCandidate(new Vector3(3.5f, 1.2f, 14f)));
        }

        [Test]
        public void BallAboveCrossbarIsRejected()
        {
            Assert.IsFalse(ShotScoringRules.IsGoalCandidate(new Vector3(0f, 2.3f, 14f)));
        }

        [Test]
        public void BallBelowPlayingSurfaceIsRejected()
        {
            Assert.IsFalse(ShotScoringRules.IsGoalCandidate(new Vector3(0f, -0.1f, 14f)));
        }
    }
}
