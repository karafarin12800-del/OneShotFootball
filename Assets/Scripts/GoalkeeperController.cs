using UnityEngine;

namespace OneShotFootball
{
    [DisallowMultipleComponent]
    public sealed class GoalkeeperController : MonoBehaviour
    {
        private OneShotFootballGame _game;
        private Rigidbody _body;
        private Vector3 _homePosition;
        private float _clock;

        public void Initialize(OneShotFootballGame game, Transform root, Rigidbody body)
        {
            _game = game;
            _body = body;
            _homePosition = root.position;
        }

        private void FixedUpdate()
        {
            if (_game == null || _body == null || !_game.KeeperShouldMove)
                return;

            _clock += Time.fixedDeltaTime;
            float x = Mathf.Sin(_clock * 0.82f + 0.25f) * 2.0f;
            _body.MovePosition(new Vector3(_homePosition.x + x, _homePosition.y, _homePosition.z));
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
