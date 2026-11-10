using UnityEngine;

namespace OneShotFootball
{
    [DisallowMultipleComponent]
    public sealed class GoalkeeperController : MonoBehaviour
    {
        private OneShotFootballGame _game;
        private Rigidbody _body;
        private FootballerAnimationDriver _animation;
        private Vector3 _homePosition;
        private Quaternion _homeRotation;
        private float _clock;
        private bool _reacting;
        private float _reactionTimer;
        private float _reactionTargetX;

        public void Initialize(OneShotFootballGame game, Transform root, Rigidbody body)
        {
            _game = game;
            _body = body;
            _homePosition = root.position;
            _homeRotation = root.rotation;
            _animation = root.GetComponent<FootballerAnimationDriver>();
        }

        private void FixedUpdate()
        {
            if (_game == null || _body == null || !_game.KeeperShouldMove)
                return;

            if (_reacting)
            {
                Vector3 current = _body.position;
                current.x = Mathf.MoveTowards(current.x, _reactionTargetX, 7.2f * Time.fixedDeltaTime);
                _body.MovePosition(current);
                _reactionTimer -= Time.fixedDeltaTime;
                if (_reactionTimer <= 0f)
                    _reacting = false;
                return;
            }

            _clock += Time.fixedDeltaTime;
            float x = Mathf.Sin(_clock * 0.82f + 0.25f) * 2.0f;
            _body.MovePosition(new Vector3(_homePosition.x + x, _homePosition.y, _homePosition.z));
        }

        public void ReactToShot(float targetX)
        {
            if (_body == null)
                return;

            float currentX = _body.position.x;
            // The keeper reacts toward the target with a slight prediction error.
            // This keeps the save probability fair instead of making every shot a save.
            _reactionTargetX = Mathf.Clamp(targetX * 0.68f, -2.85f, 2.85f);
            _reactionTimer = 0.62f;
            _reacting = true;
            if (_animation != null)
                _animation.PlayDive(_reactionTargetX - currentX);
        }

        public void ResetForNextShot()
        {
            if (_body == null)
                return;

            _reacting = false;
            _reactionTimer = 0f;
            _body.position = _homePosition;
            _body.rotation = _homeRotation;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody == null)
                return;

            if (collision.rigidbody.GetComponent<FootballBall>() != null)
                _game?.RegisterKeeperSave();
        }
    }
}