using UnityEngine;

namespace OneShotFootball
{
    /// <summary>
    /// Optional animation bridge for imported Humanoid athlete prefabs.
    /// It safely no-ops while the project is using its procedural fallback characters.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FootballerAnimationDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private static readonly int KickHash = Animator.StringToHash("Kick");
        private static readonly int CelebrateHash = Animator.StringToHash("Celebrate");
        private static readonly int DiveLeftHash = Animator.StringToHash("DiveLeft");
        private static readonly int DiveRightHash = Animator.StringToHash("DiveRight");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private bool _hasKick;
        private bool _hasCelebrate;
        private bool _hasDiveLeft;
        private bool _hasDiveRight;
        private bool _hasSpeed;

        private void Awake()
        {
            RefreshAnimator();
        }

        public void RefreshAnimator()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            _hasKick = HasParameter(KickHash, AnimatorControllerParameterType.Trigger);
            _hasCelebrate = HasParameter(CelebrateHash, AnimatorControllerParameterType.Trigger);
            _hasDiveLeft = HasParameter(DiveLeftHash, AnimatorControllerParameterType.Trigger);
            _hasDiveRight = HasParameter(DiveRightHash, AnimatorControllerParameterType.Trigger);
            _hasSpeed = HasParameter(SpeedHash, AnimatorControllerParameterType.Float);
        }

        public void PlayKick()
        {
            if (_hasKick) animator.SetTrigger(KickHash);
        }

        public void PlayCelebrate()
        {
            if (_hasCelebrate) animator.SetTrigger(CelebrateHash);
        }

        public void PlayDive(float horizontalDirection)
        {
            if (horizontalDirection < -0.05f && _hasDiveLeft)
                animator.SetTrigger(DiveLeftHash);
            else if (horizontalDirection > 0.05f && _hasDiveRight)
                animator.SetTrigger(DiveRightHash);
        }

        public void SetMoveSpeed(float speed)
        {
            if (_hasSpeed) animator.SetFloat(SpeedHash, Mathf.Max(0f, speed));
        }

        private bool HasParameter(int hash, AnimatorControllerParameterType type)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return false;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == hash && parameter.type == type)
                    return true;
            }
            return false;
        }
    }
}