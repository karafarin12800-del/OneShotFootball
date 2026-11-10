namespace OneShotFootball
{
    /// <summary>
    /// Rules for the offline clear-the-table practice mode.
    /// Practice credits are earned only for valid object-ball pockets; they are not wagers.
    /// </summary>
    public static class PoolPracticeRules
    {
        public static bool IsRewardEligibleBall(int ballNumber)
        {
            return ballNumber >= 1 && ballNumber <= 15 && ballNumber != 8;
        }

        public static int RewardTokensForPocket(int ballNumber, bool shotWasFoul)
        {
            return !shotWasFoul && IsRewardEligibleBall(ballNumber) ? 2 : 0;
        }

        public static bool IsEightBallWin(bool allObjectBallsCleared, bool shotWasFoul)
        {
            return allObjectBallsCleared && !shotWasFoul;
        }
    }
}
