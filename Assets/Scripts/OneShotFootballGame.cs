using UnityEngine;
using UnityEngine.Rendering;

namespace OneShotFootball
{
    public sealed class OneShotFootballGame : MonoBehaviour
    {
        private const int MaxShots = 5;
        private const float GoalLineZ = 14.0f;
        private const float BallStartZ = -8.15f;
        private const float BallRadius = 0.23f;

        private enum GameState { Menu, Ready, InFlight, ShotResult, Paused, Results, Shop }

        private GameState _state = GameState.Menu;
        private GameState _stateBeforePause;
        private int _shotsTaken;
        private int _score;
        private int _bestScore;
        private int _streak;
        private float _stateTimer;
        private float _shotTimer;
        private int _pointsEarned;
        private string _lastResult = string.Empty;
        private Color _lastResultColor = Color.white;

        private CosmeticShopEconomy _shopEconomy;
        private OneShotIapBridge _iapBridge;
        private bool _showTokenPacks;
        private GoalkeeperController _goalkeeperController;
        private FootballerAppearance _playerAppearance;
        private GameObject _playerRoot;
        private string _activePlayerPrefabId = string.Empty;
        private Renderer _captainBandRenderer;
        private FootballerAnimationDriver _kickerAnimation;
        private GameState _stateBeforeShop;
        private CosmeticCategory _shopCategory = CosmeticCategory.Player;
        private string _shopStatus = "Earn tokens by completing five-shot rounds.";
        private int _goalsThisRound;
        private int _roundTokenReward;
        private bool _roundRewardGranted;

        private Material _grass;
        private Material _grassAlt;
        private Material _line;
        private Material _goalWhite;
        private Material _netMaterial;
        private Material _blueKit;
        private Material _blueAccent;
        private Material _orangeKit;
        private Material _orangeAccent;
        private Material _skin;
        private Material _hair;
        private Material _shorts;
        private Material _shoes;
        private Material _ballWhite;
        private Material _ballBlack;
        private Material _standMaterial;
        private Material _seatBlue;
        private Material _seatRed;
        private Material _seatWhite;
        private Material _aimMaterial;

        private Transform _ballTransform;
        private Rigidbody _ballBody;
        private LineRenderer _aimLine;
        private GameObject _aimMarker;
        private Transform _kickLegPivot;
        private float _kickAnimationTimer;
        private bool _pointerHeld;
        private bool _aiming;
        private int _activeFingerId = -1;
        private Vector2 _pointerStart;
        private Vector2 _pointerCurrent;

        private GUIStyle _panelStyle;
        private GUIStyle _hudStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _resultStyle;
        private Texture2D _panelTexture;
        private Texture2D _hudTexture;
        private Texture2D _buttonTexture;
        private Texture2D _buttonPressedTexture;

        public bool KeeperShouldMove => _state == GameState.Ready || _state == GameState.InFlight || _state == GameState.ShotResult;
        public bool IsShotInFlight => _state == GameState.InFlight;

        private void Awake()
        {
            Application.targetFrameRate = 60;
#if UNITY_ANDROID || UNITY_IOS
            Screen.orientation = ScreenOrientation.Portrait;
#endif
            Physics.gravity = new Vector3(0f, -9.81f, 0f);
            _bestScore = PlayerPrefs.GetInt("OneShotFootball.BestScore", 0);
            _shopEconomy = GetComponent<CosmeticShopEconomy>();
            if (_shopEconomy == null) _shopEconomy = gameObject.AddComponent<CosmeticShopEconomy>();
            _shopEconomy.StateChanged += OnShopStateChanged;
            _iapBridge = GetComponent<OneShotIapBridge>();
            if (_iapBridge == null) _iapBridge = gameObject.AddComponent<OneShotIapBridge>();
            _iapBridge.Initialize(_shopEconomy);
            CreateMaterials();
            BuildWorld();
        }

        private void Update()
        {
            UpdateKickAnimation();

            if (_state == GameState.InFlight)
            {
                _shotTimer += Time.deltaTime;
                if (_ballTransform != null && _ballTransform.position.z >= GoalLineZ - 0.03f)
                {
                    Vector3 crossing = _ballTransform.position;
                    bool goal = ShotScoringRules.IsGoalCandidate(crossing);
                    ResolveShot(goal, goal ? "GOAL!" : crossing.y > ShotScoringRules.GoalHeight ? "TOO HIGH!" : "WIDE!");
                }
                else if (_shotTimer > 3.1f)
                {
                    ResolveShot(false, "NO GOAL");
                }
            }
            else if (_state == GameState.ShotResult)
            {
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    if (_shotsTaken >= MaxShots)
                    {
                        if (!_roundRewardGranted && _shopEconomy != null)
                        {
                            _roundTokenReward = _shopEconomy.AwardCompletedRoundTokens(_score, _goalsThisRound);
                            _roundRewardGranted = true;
                        }
                        _state = GameState.Results;
                    }
                    else
                    {
                        ResetBall();
                        _state = GameState.Ready;
                    }
                }
            }

            if (_state == GameState.Ready)
                HandlePointerInput();
            else if (_aiming)
                CancelAim();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            DestroyGuiTexture(_panelTexture);
            DestroyGuiTexture(_hudTexture);
            DestroyGuiTexture(_buttonTexture);
            DestroyGuiTexture(_buttonPressedTexture);
            if (_shopEconomy != null) _shopEconomy.StateChanged -= OnShopStateChanged;
        }

        private void CreateMaterials()
        {
            _grass = MakeMaterial(new Color(0.10f, 0.34f, 0.15f));
            _grassAlt = MakeMaterial(new Color(0.13f, 0.40f, 0.18f));
            _line = MakeMaterial(new Color(0.88f, 0.95f, 0.87f), 0.0f, 0.2f);
            _goalWhite = MakeMaterial(new Color(0.94f, 0.97f, 1f), 0.05f, 0.45f);
            _netMaterial = MakeMaterial(new Color(0.78f, 0.88f, 0.91f));
            _blueKit = MakeMaterial(new Color(0.05f, 0.28f, 0.91f), 0f, 0.28f);
            _blueAccent = MakeMaterial(new Color(0.92f, 0.96f, 1f));
            _orangeKit = MakeMaterial(new Color(1f, 0.30f, 0.07f), 0f, 0.25f);
            _orangeAccent = MakeMaterial(new Color(1f, 0.83f, 0.12f));
            _skin = MakeMaterial(new Color(0.68f, 0.43f, 0.27f));
            _hair = MakeMaterial(new Color(0.10f, 0.07f, 0.05f));
            _shorts = MakeMaterial(new Color(0.04f, 0.08f, 0.18f));
            _shoes = MakeMaterial(new Color(0.06f, 0.08f, 0.12f), 0.05f, 0.35f);
            _ballWhite = MakeMaterial(new Color(0.96f, 0.97f, 0.93f), 0f, 0.35f);
            _ballBlack = MakeMaterial(new Color(0.025f, 0.035f, 0.055f), 0f, 0.25f);
            _standMaterial = MakeMaterial(new Color(0.07f, 0.12f, 0.22f));
            _seatBlue = MakeMaterial(new Color(0.05f, 0.34f, 0.78f));
            _seatRed = MakeMaterial(new Color(0.90f, 0.22f, 0.20f));
            _seatWhite = MakeMaterial(new Color(0.88f, 0.91f, 0.94f));
            _aimMaterial = MakeMaterial(new Color(1f, 0.82f, 0.10f), 0f, 0.2f);
        }

        private static Material MakeMaterial(Color color, float metallic = 0f, float smoothness = 0.18f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            var material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private void BuildWorld()
        {
            RenderSettings.ambientLight = new Color(0.62f, 0.68f, 0.72f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.49f, 0.72f, 0.82f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 80f;

            var sunlight = new GameObject("Stadium Sun");
            sunlight.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            var light = sunlight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.96f, 0.86f);
            light.shadows = LightShadows.Soft;

            BuildPitch();
            BuildStands();
            BuildGoal();
            BuildPlayer();
            BuildGoalkeeper();
            BuildBall();
            BuildCamera();
            BuildAimGuide();
            ApplyEquippedCosmetics();
        }

        private void BuildPitch()
        {
            CreatePrimitive(PrimitiveType.Plane, "Pitch Base", Vector3.zero, new Vector3(2f, 1f, 3.6f), _grass, true);

            for (int i = 0; i < 12; i++)
            {
                float z = -16.5f + i * 3f;
                Material stripe = i % 2 == 0 ? _grass : _grassAlt;
                CreatePrimitive(PrimitiveType.Cube, "Mowed Stripe " + (i + 1), new Vector3(0f, 0.012f, z),
                    new Vector3(19.8f, 0.014f, 3.02f), stripe, false);
            }

            DrawRect(-9.4f, 9.4f, -16.8f, 16.8f, 0.035f, _line, "Touchline");
            DrawLine(new Vector3(-9.4f, 0.036f, 0f), new Vector3(9.4f, 0.036f, 0f), 0.035f, _line, "Halfway Line");
            DrawCircle(Vector3.zero, 2.65f, 0.036f, _line, "Centre Circle");
            CreatePrimitive(PrimitiveType.Cylinder, "Centre Spot", new Vector3(0f, 0.035f, 0f),
                new Vector3(0.11f, 0.006f, 0.11f), _line, false);

            DrawRect(-5.7f, 5.7f, 8.2f, 13.96f, 0.035f, _line, "Penalty Box");
            DrawRect(-2.05f, 2.05f, 11.9f, 13.96f, 0.035f, _line, "Six Yard Box");
            DrawCircle(new Vector3(0f, 0.04f, 10.1f), 0.11f, 0.035f, _line, "Penalty Spot");

            CreatePrimitive(PrimitiveType.Cube, "Left Touchline Board", new Vector3(-10f, 0.48f, 3.3f),
                new Vector3(0.24f, 0.92f, 5.8f), _standMaterial, false);
            CreatePrimitive(PrimitiveType.Cube, "Right Touchline Board", new Vector3(10f, 0.48f, 3.3f),
                new Vector3(0.24f, 0.92f, 5.8f), _standMaterial, false);
            CreatePrimitive(PrimitiveType.Cube, "Left Board Accent", new Vector3(-10.14f, 0.49f, 3.3f),
                new Vector3(0.025f, 0.08f, 5.7f), _seatBlue, false);
            CreatePrimitive(PrimitiveType.Cube, "Right Board Accent", new Vector3(10.14f, 0.49f, 3.3f),
                new Vector3(0.025f, 0.08f, 5.7f), _seatRed, false);
        }

        private void BuildStands()
        {
            CreatePrimitive(PrimitiveType.Cube, "Far Stand Structure", new Vector3(0f, 2.4f, 20.3f),
                new Vector3(26f, 4.3f, 2.7f), _standMaterial, false);
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 29; column++)
                {
                    float x = -12.6f + column * 0.9f;
                    float y = 3.1f + row * 0.50f;
                    float z = 18.9f + row * 0.42f;
                    Material seat = (column + row * 3) % 9 == 0 ? _seatRed :
                        ((column + row) % 5 == 0 ? _seatWhite : _seatBlue);
                    CreatePrimitive(PrimitiveType.Cube, "Crowd Seat " + row + "-" + column,
                        new Vector3(x, y, z), new Vector3(0.55f, 0.32f, 0.28f), seat, false);
                }
            }

            CreatePrimitive(PrimitiveType.Cube, "Far Stand Roof", new Vector3(0f, 6.1f, 20.2f),
                new Vector3(28f, 0.28f, 3.5f), _standMaterial, false);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int pole = 0; pole < 2; pole++)
                {
                    float z = 4f + pole * 10f;
                    CreatePrimitive(PrimitiveType.Cube, "Stadium Light Tower",
                        new Vector3(side * 12.0f, 5.1f, z), new Vector3(0.25f, 10.2f, 0.25f), _standMaterial, false);
                    CreatePrimitive(PrimitiveType.Cube, "Stadium Light Panel",
                        new Vector3(side * 12.0f, 10.1f, z), new Vector3(1.2f, 0.3f, 0.7f), _goalWhite, false);
                }
            }
        }

        private void BuildGoal()
        {
            const float halfWidth = ShotScoringRules.GoalHalfWidth;
            const float height = ShotScoringRules.GoalHeight;
            const float goalZ = GoalLineZ;

            CreatePrimitive(PrimitiveType.Cylinder, "Goal Post Left", new Vector3(-halfWidth, height * 0.5f, goalZ),
                new Vector3(0.075f, height * 0.5f, 0.075f), _goalWhite, true);
            CreatePrimitive(PrimitiveType.Cylinder, "Goal Post Right", new Vector3(halfWidth, height * 0.5f, goalZ),
                new Vector3(0.075f, height * 0.5f, 0.075f), _goalWhite, true);
            CreatePrimitive(PrimitiveType.Cube, "Goal Crossbar", new Vector3(0f, height, goalZ),
                new Vector3(halfWidth * 2f, 0.13f, 0.13f), _goalWhite, true);

            const float backZ = 15.25f;
            for (int i = 0; i <= 12; i++)
            {
                float x = Mathf.Lerp(-halfWidth, halfWidth, i / 12f);
                DrawLine(new Vector3(x, 0.05f, goalZ + 0.05f), new Vector3(x, height - 0.03f, backZ),
                    0.018f, _netMaterial, "Goal Net Vertical");
            }

            for (int i = 0; i <= 5; i++)
            {
                float y = Mathf.Lerp(0.1f, height - 0.08f, i / 5f);
                DrawLine(new Vector3(-halfWidth, y, goalZ + 0.06f), new Vector3(halfWidth, y, goalZ + 0.06f),
                    0.018f, _netMaterial, "Goal Net Front");
                DrawLine(new Vector3(-halfWidth, y, backZ), new Vector3(halfWidth, y, backZ),
                    0.018f, _netMaterial, "Goal Net Back");
            }

            DrawLine(new Vector3(-halfWidth, height, goalZ), new Vector3(-halfWidth, height, backZ), 0.035f, _netMaterial, "Goal Net Side");
            DrawLine(new Vector3(halfWidth, height, goalZ), new Vector3(halfWidth, height, backZ), 0.035f, _netMaterial, "Goal Net Side");
            DrawLine(new Vector3(-halfWidth, 0.05f, goalZ), new Vector3(-halfWidth, 0.05f, backZ), 0.02f, _netMaterial, "Goal Net Base");
            DrawLine(new Vector3(halfWidth, 0.05f, goalZ), new Vector3(halfWidth, 0.05f, backZ), 0.02f, _netMaterial, "Goal Net Base");
        }

        private void BuildPlayer()
        {
            var player = new GameObject("Blue Team Kicker");
            _playerRoot = player;
            player.transform.position = new Vector3(0f, 0f, -9.1f);
            _kickerAnimation = player.AddComponent<FootballerAnimationDriver>();

            MakePart(player.transform, PrimitiveType.Capsule, "Jersey", new Vector3(0f, 1.25f, 0f),
                new Vector3(0.68f, 0.66f, 0.36f), _blueKit, false);
            MakePart(player.transform, PrimitiveType.Cube, "Shorts", new Vector3(0f, 0.66f, 0.015f),
                new Vector3(0.63f, 0.34f, 0.38f), _shorts, false);
            MakePart(player.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 2.08f, 0f),
                new Vector3(0.42f, 0.45f, 0.40f), _skin, false);
            MakePart(player.transform, PrimitiveType.Sphere, "Hair", new Vector3(0f, 2.23f, -0.025f),
                new Vector3(0.43f, 0.20f, 0.42f), _hair, false);
            MakePart(player.transform, PrimitiveType.Cube, "Jersey Back Stripe", new Vector3(0f, 1.36f, -0.187f),
                new Vector3(0.07f, 0.54f, 0.025f), _blueAccent, false);
            GameObject captainBand = MakePart(player.transform, PrimitiveType.Cube, "Captain Armband",
                new Vector3(-0.405f, 1.51f, 0.01f), new Vector3(0.205f, 0.09f, 0.205f), _orangeAccent, false);
            _captainBandRenderer = captainBand.GetComponent<Renderer>();
            _captainBandRenderer.enabled = false;

            MakePart(player.transform, PrimitiveType.Capsule, "Left Arm", new Vector3(-0.39f, 1.30f, 0.01f),
                new Vector3(0.18f, 0.46f, 0.18f), _blueKit, false, new Vector3(0f, 0f, 10f));
            MakePart(player.transform, PrimitiveType.Capsule, "Right Arm", new Vector3(0.39f, 1.30f, 0.01f),
                new Vector3(0.18f, 0.46f, 0.18f), _blueKit, false, new Vector3(0f, 0f, -10f));

            MakePart(player.transform, PrimitiveType.Capsule, "Left Leg", new Vector3(-0.19f, 0.34f, 0.015f),
                new Vector3(0.18f, 0.42f, 0.18f), _skin, false);
            MakePart(player.transform, PrimitiveType.Cube, "Left Boot", new Vector3(-0.19f, 0.10f, 0.14f),
                new Vector3(0.30f, 0.14f, 0.38f), _shoes, false);

            var kickPivot = new GameObject("Kicking Leg Pivot");
            kickPivot.transform.SetParent(player.transform, false);
            kickPivot.transform.localPosition = new Vector3(0.19f, 0.52f, 0.04f);
            MakePart(kickPivot.transform, PrimitiveType.Capsule, "Right Leg", new Vector3(0f, -0.22f, 0f),
                new Vector3(0.18f, 0.40f, 0.18f), _skin, false);
            MakePart(kickPivot.transform, PrimitiveType.Cube, "Right Boot", new Vector3(0f, -0.44f, 0.15f),
                new Vector3(0.31f, 0.13f, 0.38f), _shoes, false);
            _kickLegPivot = kickPivot.transform;
            _playerAppearance = player.AddComponent<FootballerAppearance>();
            _playerAppearance.RefreshRenderers();
        }

        private void BuildGoalkeeper()
        {
            GameObject goalkeeperPrefab = Resources.Load<GameObject>("Characters/Goalkeeper");
            var keeperRoot = goalkeeperPrefab != null
                ? Instantiate(goalkeeperPrefab)
                : new GameObject("Moving Goalkeeper");
            keeperRoot.name = "Moving Goalkeeper";
            keeperRoot.transform.SetPositionAndRotation(
                new Vector3(0f, 0f, GoalLineZ - 0.62f),
                goalkeeperPrefab != null ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity);

            var keeperBody = keeperRoot.GetComponent<Rigidbody>();
            if (keeperBody == null) keeperBody = keeperRoot.AddComponent<Rigidbody>();
            keeperBody.isKinematic = true;
            keeperBody.useGravity = false;
            keeperBody.constraints = RigidbodyConstraints.FreezeRotation;

            var keeperCollider = keeperRoot.GetComponent<CapsuleCollider>();
            if (keeperCollider == null && keeperRoot.GetComponent<Collider>() == null)
                keeperCollider = keeperRoot.AddComponent<CapsuleCollider>();
            if (keeperCollider != null)
            {
                keeperCollider.radius = 0.59f;
                keeperCollider.height = 1.95f;
                keeperCollider.center = new Vector3(0f, 0.98f, 0f);
            }

            if (goalkeeperPrefab == null)
            {
                MakePart(keeperRoot.transform, PrimitiveType.Capsule, "Orange Jersey", new Vector3(0f, 1.22f, 0f),
                new Vector3(0.67f, 0.64f, 0.37f), _orangeKit, false);
            MakePart(keeperRoot.transform, PrimitiveType.Cube, "Keeper Shorts", new Vector3(0f, 0.64f, 0.015f),
                new Vector3(0.63f, 0.32f, 0.38f), _shorts, false);
            MakePart(keeperRoot.transform, PrimitiveType.Sphere, "Keeper Head", new Vector3(0f, 2.04f, 0f),
                new Vector3(0.42f, 0.44f, 0.40f), _skin, false);
            MakePart(keeperRoot.transform, PrimitiveType.Sphere, "Keeper Hair", new Vector3(0f, 2.20f, -0.02f),
                new Vector3(0.43f, 0.19f, 0.42f), _hair, false);
            MakePart(keeperRoot.transform, PrimitiveType.Capsule, "Keeper Left Arm", new Vector3(-0.43f, 1.34f, 0f),
                new Vector3(0.19f, 0.47f, 0.19f), _orangeKit, false, new Vector3(0f, 0f, 12f));
            MakePart(keeperRoot.transform, PrimitiveType.Capsule, "Keeper Right Arm", new Vector3(0.43f, 1.34f, 0f),
                new Vector3(0.19f, 0.47f, 0.19f), _orangeKit, false, new Vector3(0f, 0f, -12f));
            MakePart(keeperRoot.transform, PrimitiveType.Capsule, "Keeper Left Leg", new Vector3(-0.19f, 0.32f, 0f),
                new Vector3(0.19f, 0.40f, 0.19f), _blueAccent, false);
            MakePart(keeperRoot.transform, PrimitiveType.Capsule, "Keeper Right Leg", new Vector3(0.19f, 0.32f, 0f),
                new Vector3(0.19f, 0.40f, 0.19f), _blueAccent, false);
            }

            var keeperDriver = keeperRoot.GetComponent<FootballerAnimationDriver>();
            if (keeperDriver == null) keeperDriver = keeperRoot.AddComponent<FootballerAnimationDriver>();
            keeperDriver.RefreshAnimator();
            _goalkeeperController = keeperRoot.GetComponent<GoalkeeperController>();
            if (_goalkeeperController == null) _goalkeeperController = keeperRoot.AddComponent<GoalkeeperController>();
            _goalkeeperController.Initialize(this, keeperRoot, keeperBody);
        }

        private void BuildBall()
        {
            var ball = CreatePrimitive(PrimitiveType.Sphere, "MatchBall", new Vector3(0f, BallRadius + 0.025f, BallStartZ),
                new Vector3(BallRadius * 2f, BallRadius * 2f, BallRadius * 2f), _ballWhite, true);
            _ballTransform = ball.transform;

            var body = ball.AddComponent<Rigidbody>();
            body.mass = 0.43f;
            body.drag = 0.03f;
            body.angularDrag = 0.05f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.useGravity = true;
            _ballBody = body;
            ball.AddComponent<FootballBall>();

            Vector3[] directions =
            {
                Vector3.up, Vector3.down, Vector3.left,
                Vector3.right, Vector3.forward, Vector3.back
            };

            for (int i = 0; i < directions.Length; i++)
            {
                var patch = new GameObject("Ball Panel " + i);
                patch.transform.SetParent(ball.transform, false);
                patch.transform.localPosition = directions[i] * 0.475f;
                patch.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, directions[i]);
                var filter = patch.AddComponent<MeshFilter>();
                filter.sharedMesh = CreateHexPatchMesh(0.18f);
                var renderer = patch.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _ballBlack;
            }
        }

        private static Mesh CreateHexPatchMesh(float radius)
        {
            var mesh = new Mesh { name = "Original Ball Panel Mesh" };
            var vertices = new Vector3[7];
            vertices[0] = Vector3.zero;
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (i * 60f);
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }

            var triangles = new int[18];
            for (int i = 0; i < 6; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % 6 + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private void BuildCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 6.2f, -15.7f);
            cameraObject.transform.LookAt(new Vector3(0f, 1.7f, 8.5f));
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 56f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.44f, 0.71f, 0.86f);
            camera.allowHDR = false;
            camera.allowMSAA = true;
            cameraObject.AddComponent<AudioListener>();
        }

        private void BuildAimGuide()
        {
            _aimLine = CreateLineRenderer("Ballistic Aim Guide", new Color(1f, 0.85f, 0.16f, 0.95f), 0.045f);
            _aimLine.enabled = false;
            _aimMarker = CreatePrimitive(PrimitiveType.Sphere, "Aim Target Marker", Vector3.zero,
                new Vector3(0.15f, 0.15f, 0.15f), _aimMaterial, false);
            _aimMarker.SetActive(false);
        }

        private void HandlePointerInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (Input.GetMouseButtonDown(0))
                BeginAim(Input.mousePosition);

            if (_pointerHeld && Input.GetMouseButton(0))
                MoveAim(Input.mousePosition);

            if (_pointerHeld && Input.GetMouseButtonUp(0))
                EndAim(Input.mousePosition);
#else
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (!_pointerHeld && touch.phase == TouchPhase.Began)
                {
                    BeginAim(touch.position, touch.fingerId);
                }
                else if (_pointerHeld && touch.fingerId == _activeFingerId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        MoveAim(touch.position);
                    else if (touch.phase == TouchPhase.Ended)
                        EndAim(touch.position);
                    else if (touch.phase == TouchPhase.Canceled)
                        CancelAim();
                }
            }
#endif
        }

        private void BeginAim(Vector2 position, int fingerId = -1)
        {
            if (_state != GameState.Ready || position.y > Screen.height * 0.82f)
                return;

            _pointerHeld = true;
            _aiming = true;
            _activeFingerId = fingerId;
            _pointerStart = position;
            _pointerCurrent = position;
            UpdateAimGuide();
        }

        private void MoveAim(Vector2 position)
        {
            if (!_pointerHeld || !_aiming)
                return;

            _pointerCurrent = position;
            UpdateAimGuide();
        }

        private void EndAim(Vector2 position)
        {
            if (!_pointerHeld || !_aiming)
                return;

            _pointerCurrent = position;
            Vector2 swipe = _pointerCurrent - _pointerStart;
            CancelAim();
            ShootFromSwipe(swipe);
        }

        private void CancelAim()
        {
            _pointerHeld = false;
            _aiming = false;
            _activeFingerId = -1;
            if (_aimLine != null) _aimLine.enabled = false;
            if (_aimMarker != null) _aimMarker.SetActive(false);
        }

        private Vector3 GetTarget(Vector2 swipe)
        {
            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);
            float targetX = Mathf.Clamp((swipe.x / (width * 0.28f)) * 4.1f,
                -ShotScoringRules.GoalHalfWidth + 0.28f, ShotScoringRules.GoalHalfWidth - 0.28f);
            float targetY = Mathf.Clamp(0.35f + Mathf.Max(0f, swipe.y) / height * 11.5f, 0.30f,
                ShotScoringRules.GoalHeight - 0.22f);
            return new Vector3(targetX, targetY, GoalLineZ - 0.28f);
        }

        private float GetFlightTime(Vector2 swipe)
        {
            float normalizedLength = swipe.magnitude / Mathf.Max(1f, Screen.height);
            return Mathf.Clamp(1.48f - normalizedLength * 0.72f, 1.02f, 1.48f);
        }

        private Vector3 CalculateLaunchVelocity(Vector3 target, float flightTime)
        {
            Vector3 start = _ballTransform.position;
            float time = Mathf.Max(0.25f, flightTime);
            float verticalVelocity = (target.y - start.y - 0.5f * Physics.gravity.y * time * time) / time;
            return new Vector3((target.x - start.x) / time, verticalVelocity, (target.z - start.z) / time);
        }

        private void UpdateAimGuide()
        {
            if (_aimLine == null || _ballTransform == null)
                return;

            Vector2 swipe = _pointerCurrent - _pointerStart;
            Vector3 target = GetTarget(swipe);
            Vector3 velocity = CalculateLaunchVelocity(target, GetFlightTime(swipe));
            const int samples = 22;
            _aimLine.positionCount = samples;
            float duration = GetFlightTime(swipe);
            for (int i = 0; i < samples; i++)
            {
                float t = duration * i / (samples - 1f);
                Vector3 position = _ballTransform.position + velocity * t + 0.5f * Physics.gravity * t * t;
                _aimLine.SetPosition(i, position);
            }

            _aimLine.enabled = true;
            _aimMarker.transform.position = target;
            _aimMarker.SetActive(true);
        }

        private void ShootFromSwipe(Vector2 swipe)
        {
            if (_state != GameState.Ready || _ballBody == null)
                return;

            _shotsTaken++;
            _shotTimer = 0f;
            _state = GameState.InFlight;
            _ballBody.isKinematic = false;
            _ballBody.velocity = Vector3.zero;
            _ballBody.angularVelocity = Vector3.zero;
            _ballBody.WakeUp();

            Vector3 target = GetTarget(swipe);
            float flightTime = GetFlightTime(swipe);
            _ballBody.velocity = CalculateLaunchVelocity(target, flightTime);
            _ballBody.angularVelocity = new Vector3(7f + Mathf.Abs(swipe.y) * 0.025f, 0f, -swipe.x * 0.025f);

            if (_kickLegPivot != null)
                _kickAnimationTimer = 0.32f;
            if (_kickerAnimation != null) _kickerAnimation.PlayKick();
            if (_goalkeeperController != null) _goalkeeperController.ReactToShot(target.x);
        }

        private void UpdateKickAnimation()
        {
            if (_kickLegPivot == null)
                return;

            if (_kickAnimationTimer > 0f)
            {
                _kickAnimationTimer -= Time.deltaTime;
                float elapsed = 0.32f - Mathf.Max(0f, _kickAnimationTimer);
                float angle = elapsed < 0.12f
                    ? Mathf.Lerp(0f, -58f, elapsed / 0.12f)
                    : Mathf.Lerp(-58f, 0f, Mathf.Clamp01((elapsed - 0.12f) / 0.20f));
                _kickLegPivot.localRotation = Quaternion.Euler(angle, 0f, 0f);
            }
            else
            {
                _kickLegPivot.localRotation = Quaternion.identity;
            }
        }

        public void RegisterKeeperSave()
        {
            ResolveShot(false, "SAVED!");
        }

        private void ResolveShot(bool goal, string result)
        {
            if (_state != GameState.InFlight)
                return;

            CancelAim();
            if (_ballBody != null)
            {
                _ballBody.velocity = Vector3.zero;
                _ballBody.angularVelocity = Vector3.zero;
                _ballBody.isKinematic = true;
            }

            _pointsEarned = 0;
            if (goal)
            {
                _goalsThisRound++;
                if (_kickerAnimation != null) _kickerAnimation.PlayCelebrate();
                _streak++;
                _pointsEarned = ShotScoringRules.PointsForStreak(_streak);
                _score += _pointsEarned;
                if (_score > _bestScore)
                {
                    _bestScore = _score;
                    PlayerPrefs.SetInt("OneShotFootball.BestScore", _bestScore);
                    PlayerPrefs.Save();
                }
                _lastResultColor = new Color(0.37f, 1f, 0.58f);
            }
            else
            {
                _streak = 0;
                _lastResultColor = result == "SAVED!" ? new Color(1f, 0.66f, 0.23f) : new Color(1f, 0.43f, 0.38f);
            }

            _lastResult = result;
            _stateTimer = 1.10f;
            _state = GameState.ShotResult;
        }

        private void ResetBall()
        {
            if (_ballBody == null || _ballTransform == null)
                return;

            _ballBody.isKinematic = true;
            _ballBody.velocity = Vector3.zero;
            _ballBody.angularVelocity = Vector3.zero;
            _ballTransform.SetPositionAndRotation(new Vector3(0f, BallRadius + 0.025f, BallStartZ), Quaternion.identity);
            _ballBody.isKinematic = false;
            if (_goalkeeperController != null) _goalkeeperController.ResetForNextShot();
        }

        private void BeginRound()
        {
            Time.timeScale = 1f;
            _shotsTaken = 0;
            _score = 0;
            _streak = 0;
            _pointsEarned = 0;
            _goalsThisRound = 0;
            _roundTokenReward = 0;
            _roundRewardGranted = false;
            _lastResult = string.Empty;
            _stateTimer = 0f;
            CancelAim();
            ResetBall();
            _state = GameState.Ready;
        }

        private void TogglePause()
        {
            if (_state == GameState.Paused)
            {
                Time.timeScale = 1f;
                _state = _stateBeforePause;
                return;
            }

            if (_state != GameState.Ready && _state != GameState.InFlight && _state != GameState.ShotResult)
                return;

            _stateBeforePause = _state;
            CancelAim();
            Time.timeScale = 0f;
            _state = GameState.Paused;
        }

        private void OpenShop()
        {
            if (_state == GameState.Shop) return;
            _stateBeforeShop = _state;
            CancelAim();
            _state = GameState.Shop;
        }

        private void CloseShop()
        {
            _state = _stateBeforeShop;
            _shopStatus = "Earn tokens by completing five-shot rounds.";
        }

        private void OnShopStateChanged()
        {
            ApplyEquippedCosmetics();
        }

        private void ApplyEquippedCosmetics()
        {
            if (_shopEconomy == null) return;

            string playerLook = _shopEconomy.GetEquippedId(CosmeticCategory.Player);
            ApplyPlayerPrefabIfAvailable(playerLook);
            if (playerLook == "player.pro.forward")
            {
                _skin.color = new Color(0.72f, 0.49f, 0.32f);
                _hair.color = new Color(0.12f, 0.08f, 0.05f);
                _blueKit.color = new Color(0.04f, 0.55f, 0.68f);
            }
            else if (playerLook == "player.street.legend")
            {
                _skin.color = new Color(0.55f, 0.34f, 0.23f);
                _hair.color = new Color(0.04f, 0.035f, 0.06f);
                _blueKit.color = new Color(0.36f, 0.08f, 0.58f);
            }
            else
            {
                _skin.color = new Color(0.68f, 0.43f, 0.27f);
                _hair.color = new Color(0.10f, 0.07f, 0.05f);
                _blueKit.color = new Color(0.05f, 0.28f, 0.91f);
            }

            string kit = _shopEconomy.GetEquippedId(CosmeticCategory.Kit);
            if (kit == "kit.neon")
            {
                _blueKit.color = new Color(0.02f, 0.83f, 0.70f);
                _blueAccent.color = Color.white;
                _shorts.color = new Color(0.02f, 0.08f, 0.11f);
            }
            else if (kit == "kit.midnight")
            {
                _blueKit.color = new Color(0.08f, 0.09f, 0.17f);
                _blueAccent.color = new Color(0.95f, 0.70f, 0.20f);
                _shorts.color = new Color(0.03f, 0.04f, 0.08f);
            }
            else
            {
                _blueAccent.color = new Color(0.92f, 0.96f, 1f);
                _shorts.color = new Color(0.04f, 0.08f, 0.18f);
            }

            string equipment = _shopEconomy.GetEquippedId(CosmeticCategory.Equipment);
            _shoes.color = equipment == "equipment.speed.boots"
                ? new Color(0.10f, 0.92f, 0.34f)
                : new Color(0.06f, 0.08f, 0.12f);
            if (_captainBandRenderer != null)
                _captainBandRenderer.enabled = equipment == "equipment.captain.band";

            if (_playerAppearance != null)
                _playerAppearance.Apply(playerLook, kit, equipment);

            string ball = _shopEconomy.GetEquippedId(CosmeticCategory.Ball);
            if (ball == "ball.neon")
            {
                _ballWhite.color = new Color(0.10f, 0.88f, 0.80f);
                _ballBlack.color = new Color(0.015f, 0.035f, 0.10f);
            }
            else if (ball == "ball.gold")
            {
                _ballWhite.color = new Color(0.91f, 0.65f, 0.14f);
                _ballBlack.color = new Color(0.06f, 0.045f, 0.025f);
            }
            else
            {
                _ballWhite.color = new Color(0.96f, 0.97f, 0.93f);
                _ballBlack.color = new Color(0.025f, 0.035f, 0.055f);
            }
        }

        private void ApplyPlayerPrefabIfAvailable(string playerLookId)
        {
            if (_playerRoot == null || string.IsNullOrEmpty(playerLookId) || _activePlayerPrefabId == playerLookId)
                return;

            string resourcePath = null;
            if (playerLookId == "player.striker.blue") resourcePath = "Characters/Kicker";
            else if (playerLookId == "player.pro.forward") resourcePath = "Characters/ProForward";
            else if (playerLookId == "player.street.legend") resourcePath = "Characters/StreetLegend";
            if (string.IsNullOrEmpty(resourcePath))
                return;

            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
                return;

            for (int i = _playerRoot.transform.childCount - 1; i >= 0; i--)
                Destroy(_playerRoot.transform.GetChild(i).gameObject);

            GameObject instance = Instantiate(prefab, _playerRoot.transform);
            instance.name = prefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            _kickLegPivot = null;
            _kickerAnimation = _playerRoot.GetComponent<FootballerAnimationDriver>();
            if (_kickerAnimation == null) _kickerAnimation = _playerRoot.AddComponent<FootballerAnimationDriver>();
            _kickerAnimation.RefreshAnimator();

            _playerAppearance = _playerRoot.GetComponent<FootballerAppearance>();
            if (_playerAppearance == null) _playerAppearance = _playerRoot.AddComponent<FootballerAppearance>();
            _playerAppearance.RefreshRenderers();
            _captainBandRenderer = null;
            _activePlayerPrefabId = playerLookId;
        }

        private void DrawTokenPacks(Rect panel, float scale)
        {
            GUI.Label(new Rect(panel.x + 14f * scale, panel.y + 118f * scale,
                panel.width - 28f * scale, 34f * scale), "TOKEN PACKS", _titleStyle);

            GUI.Label(new Rect(panel.x + 16f * scale, panel.y + 154f * scale,
                panel.width - 32f * scale, 52f * scale),
                "Optional in-app token packs. Final prices are supplied by Google Play or Apple and shown in your local currency.",
                _smallStyle);

            string[] ids =
            {
                OneShotIapBridge.SmallPackId,
                OneShotIapBridge.MediumPackId,
                OneShotIapBridge.LargePackId
            };
            int[] amounts = { 250, 700, 1800 };
            float top = panel.y + 220f * scale;
            float rowHeight = Mathf.Min(92f * scale, (panel.height - 330f * scale) / 3f);
            for (int i = 0; i < ids.Length; i++)
            {
                Rect row = new Rect(panel.x + 14f * scale, top + i * (rowHeight + 10f * scale),
                    panel.width - 28f * scale, rowHeight);
                GUI.Box(row, GUIContent.none, _hudStyle);
                GUI.Label(new Rect(row.x + 12f * scale, row.y + 5f * scale,
                    row.width - 160f * scale, 28f * scale),
                    amounts[i] + " TOKENS", _labelStyle);
                string price = _iapBridge != null ? _iapBridge.GetLocalizedPrice(ids[i]) : "Unavailable";
                GUI.Label(new Rect(row.x + 12f * scale, row.y + 34f * scale,
                    row.width - 160f * scale, 24f * scale), "STORE PRICE  " + price, _smallStyle);

                bool canBuy = _iapBridge != null && _iapBridge.CanPurchaseTokenPacks &&
                    _iapBridge.IsProductAvailable(ids[i]);
                GUI.enabled = canBuy;
                if (GUI.Button(new Rect(row.x + row.width - 144f * scale, row.y + 14f * scale,
                    132f * scale, 44f * scale), "BUY PACK", _buttonStyle))
                    _iapBridge.PurchaseTokenPack(ids[i]);
                GUI.enabled = true;
            }

            GUI.Label(new Rect(panel.x + 14f * scale, panel.y + panel.height - 118f * scale,
                panel.width - 28f * scale, 42f * scale),
                _iapBridge != null ? _iapBridge.Status : "Store service is not available.", _smallStyle);
            GUI.Label(new Rect(panel.x + 14f * scale, panel.y + panel.height - 72f * scale,
                panel.width - 28f * scale, 38f * scale),
                "Token checkout is disabled until account sign-in, secure receipt verification and a server wallet are implemented. Platform approval rules apply.",
                _smallStyle);

            if (GUI.Button(new Rect(panel.x + 14f * scale, panel.y + 64f * scale,
                132f * scale, 40f * scale), "BACK TO SHOP", _buttonStyle))
                _showTokenPacks = false;
        }

        private void DrawShop()
        {
            float w = Screen.width;
            float h = Screen.height;
            float scale = Mathf.Clamp(w / 390f, 0.78f, 1.25f);
            Rect panel = new Rect(w * 0.045f, h * 0.045f, w * 0.91f, h * 0.91f);
            GUI.Box(panel, GUIContent.none, _panelStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 12f * scale, panel.width - 125f * scale, 38f * scale),
                "PLAYER STORE", _titleStyle);
            GUI.Label(new Rect(panel.x + panel.width - 118f * scale, panel.y + 20f * scale, 106f * scale, 32f * scale),
                "TOKENS  " + _shopEconomy.TokenBalance, _labelStyle);
            if (GUI.Button(new Rect(panel.x + panel.width - 126f * scale, panel.y + 58f * scale,
                114f * scale, 38f * scale), "GET TOKENS", _buttonStyle))
                _showTokenPacks = true;

            if (GUI.Button(new Rect(panel.x + 12f, panel.y + 58f * scale, 90f * scale, 38f * scale), "BACK", _buttonStyle))
            {
                CloseShop();
                return;
            }

            GUI.Label(new Rect(panel.x + 108f * scale, panel.y + 62f * scale, panel.width - 245f * scale, 30f * scale),
                "Cosmetic items only • no gameplay advantage", _smallStyle);

            if (_showTokenPacks)
            {
                DrawTokenPacks(panel, scale);
                return;
            }

            CosmeticCategory[] categories =
            {
                CosmeticCategory.Player, CosmeticCategory.Kit,
                CosmeticCategory.Equipment, CosmeticCategory.Ball
            };
            string[] categoryNames = { "PLAYERS", "KITS", "EQUIPMENT", "BALLS" };
            float tabY = panel.y + 106f * scale;
            float tabW = (panel.width - 36f * scale) * 0.5f;
            float tabH = 39f * scale;
            for (int i = 0; i < categories.Length; i++)
            {
                float x = panel.x + 12f * scale + (i % 2) * (tabW + 8f * scale);
                float y = tabY + (i / 2) * (tabH + 6f * scale);
                string label = (categories[i] == _shopCategory ? "• " : "") + categoryNames[i];
                if (GUI.Button(new Rect(x, y, tabW, tabH), label, _buttonStyle))
                {
                    _shopCategory = categories[i];
                    _shopStatus = "Select an item to equip or purchase.";
                }
            }

            float listY = tabY + (tabH + 6f * scale) * 2f + 8f * scale;
            float footerY = panel.y + panel.height - 54f * scale;
            int itemCount = 0;
            foreach (CosmeticItem catalogItem in _shopEconomy.Catalog)
                if (catalogItem != null && catalogItem.category == _shopCategory) itemCount++;
            float availableListHeight = Mathf.Max(0f, footerY - listY - 8f * scale);
            float rowH = itemCount > 0
                ? Mathf.Clamp((availableListHeight - Mathf.Max(0, itemCount - 1) * 7f * scale) / itemCount,
                    54f * scale, 104f * scale)
                : 72f * scale;
            int shown = 0;
            foreach (CosmeticItem item in _shopEconomy.Catalog)
            {
                if (item == null || item.category != _shopCategory) continue;
                Rect row = new Rect(panel.x + 12f * scale, listY + shown * (rowH + 7f * scale),
                    panel.width - 24f * scale, rowH);
                GUI.Box(row, GUIContent.none, _hudStyle);
                GUI.Label(new Rect(row.x + 10f * scale, row.y + 5f * scale, row.width - 20f * scale, 24f * scale),
                    item.displayName, _labelStyle);
                GUI.Label(new Rect(row.x + 10f * scale, row.y + 31f * scale, row.width - 20f * scale, 28f * scale),
                    item.description, _smallStyle);

                bool owned = _shopEconomy.IsOwned(item.id);
                bool equipped = _shopEconomy.GetEquippedId(item.category) == item.id;
                string action = owned ? (equipped ? "EQUIPPED" : "EQUIP") :
                    (item.starterFree ? "FREE" : "BUY  " + item.tokenPrice + " TOKENS");
                Rect actionRect = new Rect(row.x + row.width - 142f * scale, row.y + row.height - 36f * scale,
                    132f * scale, 30f * scale);
                GUI.Label(new Rect(row.x + 10f * scale, row.y + row.height - 34f * scale,
                    row.width - 160f * scale, 28f * scale),
                    owned ? "OWNED" : (item.starterFree ? "STARTER ITEM" : "PRICE  " + item.tokenPrice + " TOKENS"), _smallStyle);

                if (!owned && !item.starterFree && GUI.Button(actionRect, action, _buttonStyle))
                {
                    bool purchased = _shopEconomy.TryPurchaseWithTokens(item.id);
                    _shopStatus = purchased ? item.displayName + " unlocked." : "Not enough tokens or item unavailable.";
                }
                else if (owned && !equipped && GUI.Button(actionRect, action, _buttonStyle))
                {
                    bool equippedNow = _shopEconomy.TryEquip(item.id);
                    _shopStatus = equippedNow ? item.displayName + " equipped." : "Could not equip this item.";
                }
                else if (equipped)
                {
                    GUI.Label(actionRect, action, _subtitleStyle);
                }
                shown++;
            }

            GUI.Label(new Rect(panel.x + 12f * scale, footerY, panel.width - 24f * scale, 22f * scale),
                _shopStatus, _smallStyle);
            GUI.Label(new Rect(panel.x + 12f * scale, footerY + 22f * scale, panel.width - 24f * scale, 22f * scale),
                "Token packs: real-money checkout is disabled until store products and purchase verification are configured.", _smallStyle);
        }

        private GameObject CreatePrimitive(PrimitiveType type, string objectName, Vector3 position,
            Vector3 scale, Material material, bool keepCollider)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = objectName;
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            if (!keepCollider)
            {
                var collider = gameObject.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
            }
            return gameObject;
        }

        private GameObject MakePart(Transform parent, PrimitiveType type, string objectName, Vector3 localPosition,
            Vector3 scale, Material material, bool keepCollider, Vector3 localEuler = default)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = objectName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;
            part.transform.localEulerAngles = localEuler;
            var renderer = part.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            if (!keepCollider)
            {
                var collider = part.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
            }
            return part;
        }

        private void DrawLine(Vector3 start, Vector3 end, float width, Material material, string objectName)
        {
            LineRenderer line = CreateLineRenderer(objectName, material.color, width, material);
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }

        private void DrawRect(float minX, float maxX, float minZ, float maxZ, float y, Material material, string objectName)
        {
            DrawLine(new Vector3(minX, y, minZ), new Vector3(maxX, y, minZ), 0.045f, material, objectName + " A");
            DrawLine(new Vector3(maxX, y, minZ), new Vector3(maxX, y, maxZ), 0.045f, material, objectName + " B");
            DrawLine(new Vector3(maxX, y, maxZ), new Vector3(minX, y, maxZ), 0.045f, material, objectName + " C");
            DrawLine(new Vector3(minX, y, maxZ), new Vector3(minX, y, minZ), 0.045f, material, objectName + " D");
        }

        private void DrawCircle(Vector3 center, float radius, float y, Material material, string objectName)
        {
            const int segments = 48;
            LineRenderer circle = CreateLineRenderer(objectName, material.color, 0.035f, material);
            circle.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                circle.SetPosition(i, new Vector3(center.x + Mathf.Cos(angle) * radius, y, center.z + Mathf.Sin(angle) * radius));
            }
        }

        private LineRenderer CreateLineRenderer(string objectName, Color color, float width, Material material = null)
        {
            var lineObject = new GameObject(objectName);
            var lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.sharedMaterial = material != null ? material : _aimMaterial;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            return lineRenderer;
        }

        private void EnsureGuiStyles()
        {
            if (_titleStyle != null)
                return;

            float scale = Mathf.Clamp(Screen.width / 390f, 0.78f, 1.45f);
            _panelTexture = MakeGuiTexture(new Color(0.025f, 0.055f, 0.10f, 0.94f));
            _hudTexture = MakeGuiTexture(new Color(0.02f, 0.055f, 0.09f, 0.86f));
            _buttonTexture = MakeGuiTexture(new Color(0.06f, 0.67f, 0.43f, 1f));
            _buttonPressedTexture = MakeGuiTexture(new Color(0.04f, 0.48f, 0.33f, 1f));

            _panelStyle = new GUIStyle(GUI.skin.box);
            _panelStyle.normal.background = _panelTexture;
            _panelStyle.border = new RectOffset(18, 18, 18, 18);
            _hudStyle = new GUIStyle(GUI.skin.box);
            _hudStyle.normal.background = _hudTexture;
            _hudStyle.border = new RectOffset(0, 0, 0, 0);

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(42f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            _titleStyle.normal.textColor = Color.white;

            _subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(15f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            _subtitleStyle.normal.textColor = new Color(0.64f, 0.91f, 0.94f);

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(19f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            _labelStyle.normal.textColor = Color.white;

            _smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(12f * scale),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            _smallStyle.normal.textColor = new Color(0.79f, 0.85f, 0.91f);

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(16f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            _buttonStyle.normal.background = _buttonTexture;
            _buttonStyle.hover.background = _buttonTexture;
            _buttonStyle.active.background = _buttonPressedTexture;
            _buttonStyle.focused.background = _buttonTexture;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.hover.textColor = Color.white;
            _buttonStyle.active.textColor = Color.white;

            _resultStyle = new GUIStyle(_titleStyle)
            {
                fontSize = Mathf.RoundToInt(31f * scale)
            };
        }

        private static Texture2D MakeGuiTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static void DestroyGuiTexture(Texture2D texture)
        {
            if (texture != null)
                Destroy(texture);
        }

        private void OnGUI()
        {
            EnsureGuiStyles();
            if (_state == GameState.Shop)
            {
                DrawShop();
                return;
            }
            float w = Screen.width;
            float h = Screen.height;
            float scale = Mathf.Clamp(w / 390f, 0.78f, 1.45f);

            if (_state == GameState.Menu)
            {
                Rect card = new Rect(w * 0.07f, h * 0.17f, w * 0.86f, h * 0.62f);
                GUI.Box(card, GUIContent.none, _panelStyle);
                GUI.Label(new Rect(card.x + 12f, card.y + 24f * scale, card.width - 24f, 60f * scale), "ONE SHOT", _titleStyle);
                GUI.Label(new Rect(card.x + 16f, card.y + 87f * scale, card.width - 32f, 28f * scale),
                    "FOOTBALL SKILL CHALLENGE", _subtitleStyle);
                GUI.Label(new Rect(card.x + 24f, card.y + 127f * scale, card.width - 48f, 52f * scale),
                    "AIM. CURVE. SCORE.", _labelStyle);
                GUI.Label(new Rect(card.x + 24f, card.y + 184f * scale, card.width - 48f, 56f * scale),
                    "Five shots. One keeper. Can you find the corners?", _smallStyle);

                if (GUI.Button(new Rect(card.x + card.width * 0.10f, card.y + card.height - 156f * scale,
                    card.width * 0.80f, 54f * scale), "PLAY 5-SHOT CHALLENGE", _buttonStyle))
                    BeginRound();

                if (GUI.Button(new Rect(card.x + card.width * 0.10f, card.y + card.height - 92f * scale,
                    card.width * 0.80f, 44f * scale), "PLAYER STORE  •  " + (_shopEconomy != null ? _shopEconomy.TokenBalance : 0) + " TOKENS", _buttonStyle))
                    OpenShop();

                GUI.Label(new Rect(card.x + 12f, card.y + card.height - 34f * scale, card.width - 24f, 24f * scale),
                    "BEST SCORE  " + _bestScore, _smallStyle);
            }
            else
            {
                GUI.Box(new Rect(0f, 0f, w, 82f * scale), GUIContent.none, _hudStyle);
                GUI.Label(new Rect(12f * scale, 9f * scale, w * 0.32f, 28f * scale), "SCORE  " + _score, _labelStyle);
                GUI.Label(new Rect(w * 0.36f, 9f * scale, w * 0.32f, 28f * scale),
                    "SHOT  " + Mathf.Min(_shotsTaken + (_state == GameState.Ready ? 1 : 0), MaxShots) + "/" + MaxShots, _labelStyle);
                GUI.Label(new Rect(10f * scale, 42f * scale, w * 0.56f, 23f * scale), "BEST  " + _bestScore, _smallStyle);

                if (_state != GameState.Results && _state != GameState.Paused &&
                    GUI.Button(new Rect(w - 87f * scale, 17f * scale, 76f * scale, 42f * scale), "PAUSE", _buttonStyle))
                    TogglePause();

                if (_state == GameState.Ready)
                {
                    GUI.Label(new Rect(w * 0.06f, h - 62f * scale, w * 0.88f, 43f * scale),
                        "SWIPE TO AIM  •  RELEASE TO SHOOT", _smallStyle);
                }
                else if (_state == GameState.ShotResult)
                {
                    Rect card = new Rect(w * 0.12f, h * 0.39f, w * 0.76f, 106f * scale);
                    GUI.Box(card, GUIContent.none, _panelStyle);
                    _resultStyle.normal.textColor = _lastResultColor;
                    GUI.Label(new Rect(card.x + 8f, card.y + 8f * scale, card.width - 16f, 44f * scale), _lastResult, _resultStyle);
                    string points = _pointsEarned > 0 ? "+" + _pointsEarned + " POINTS" : (_streak > 0 ? "STREAK  " + _streak : "NEXT SHOT");
                    GUI.Label(new Rect(card.x + 8f, card.y + 58f * scale, card.width - 16f, 30f * scale), points, _smallStyle);
                }
                else if (_state == GameState.Paused)
                {
                    Rect card = new Rect(w * 0.10f, h * 0.31f, w * 0.80f, h * 0.38f);
                    GUI.Box(card, GUIContent.none, _panelStyle);
                    GUI.Label(new Rect(card.x + 10f, card.y + 22f * scale, card.width - 20f, 52f * scale), "PAUSED", _titleStyle);
                    if (GUI.Button(new Rect(card.x + 26f, card.y + 104f * scale, card.width - 52f, 52f * scale), "RESUME", _buttonStyle))
                        TogglePause();
                    if (GUI.Button(new Rect(card.x + 26f, card.y + 166f * scale, card.width - 52f, 46f * scale), "RESTART ROUND", _buttonStyle))
                        BeginRound();
                }
                else if (_state == GameState.Results)
                {
                    Rect card = new Rect(w * 0.08f, h * 0.27f, w * 0.84f, h * 0.47f);
                    GUI.Box(card, GUIContent.none, _panelStyle);
                    GUI.Label(new Rect(card.x + 12f, card.y + 24f * scale, card.width - 24f, 54f * scale), "ROUND COMPLETE", _titleStyle);
                    GUI.Label(new Rect(card.x + 12f, card.y + 91f * scale, card.width - 24f, 40f * scale),
                        "SCORE  " + _score, _labelStyle);
                    GUI.Label(new Rect(card.x + 12f, card.y + 133f * scale, card.width - 24f, 32f * scale),
                        "BEST  " + _bestScore, _subtitleStyle);
                    GUI.Label(new Rect(card.x + 12f, card.y + 169f * scale, card.width - 24f, 24f * scale),
                        "TOKENS EARNED  +" + _roundTokenReward + "     BALANCE  " + (_shopEconomy != null ? _shopEconomy.TokenBalance : 0), _smallStyle);
                    if (GUI.Button(new Rect(card.x + 28f, card.y + card.height - 82f * scale, card.width - 56f, 54f * scale),
                        "PLAY AGAIN", _buttonStyle))
                        BeginRound();
                    GUI.Label(new Rect(card.x + 10f, card.y + card.height - 24f * scale, card.width - 20f, 20f * scale),
                        "Your best score is saved on this device.", _smallStyle);
                }
            }
        }
    }

    public static class ShotScoringRules
    {
        public const float GoalHalfWidth = 3.66f;
        public const float GoalHeight = 2.44f;
        public const float BallRadius = 0.23f;

        public static int PointsForStreak(int streak)
        {
            return 100 + Mathf.Max(0, streak - 1) * 25;
        }

        public static bool IsGoalCandidate(Vector3 ballPosition)
        {
            return ballPosition.x >= -GoalHalfWidth + BallRadius
                && ballPosition.x <= GoalHalfWidth - BallRadius
                && ballPosition.y >= BallRadius + 0.02f
                && ballPosition.y <= GoalHeight - BallRadius;
        }
    }
}
