using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneShotFootball
{
    public sealed class PoolGameController : MonoBehaviour
    {
        private const float Radius = 0.115f;
        private const float ClothY = 0.48f;
        private enum View { Menu, Playing, Shop, Results }
        private View _view = View.Menu;
        private readonly List<PoolBall> _balls = new List<PoolBall>();
        private readonly HashSet<int> _ownedCues = new HashSet<int> { 0 };
        private Camera _camera;
        private Rigidbody _cueBall;
        private Transform _cueStick;
        private PhysicMaterial _ballPhysics;
        private Material _cloth, _wood, _gold, _white, _black, _cueMat;
        private int _tokens, _pocketed, _shots, _cueStyle;
        private bool _paused, _dragging;
        private Vector2 _dragStart, _dragNow;
        private Vector3 _aim = Vector3.forward;
        private float _power = 0.6f;
        private string _status = "DRAG BACK TO AIM • RELEASE TO SHOOT";
        private GUIStyle _heading, _body, _button, _small;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            _tokens = PlayerPrefs.GetInt("CueLegends.Tokens", 250);
            CreateMaterials();
            BuildTable();
            BuildCamera();
            BuildRack();
            BuildCueStick();
        }

        private void CreateMaterials()
        {
            _cloth = Mat(new Color(0.025f, 0.30f, 0.22f), 0.05f, 0.35f);
            _wood = Mat(new Color(0.12f, 0.075f, 0.045f), 0.28f, 0.5f);
            _gold = Mat(new Color(0.82f, 0.57f, 0.2f), 0.7f, 0.7f);
            _white = Mat(new Color(0.96f, 0.95f, 0.89f), 0.04f, 0.3f);
            _black = Mat(new Color(0.012f, 0.016f, 0.02f), 0.1f, 0.2f);
            _cueMat = Mat(new Color(0.68f, 0.37f, 0.12f), 0.3f, 0.5f);
            _ballPhysics = new PhysicMaterial("CueLegendsBallPhysics")
            {
                dynamicFriction = 0.16f, staticFriction = 0.2f, bounciness = 0.91f,
                frictionCombine = PhysicMaterialCombine.Average, bounceCombine = PhysicMaterialCombine.Maximum
            };
        }

        private static Material Mat(Color color, float metallic, float smoothness)
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

        private void BuildTable()
        {
            RenderSettings.ambientLight = new Color(0.48f, 0.51f, 0.55f);
            var lightObject = new GameObject("Cue Legends Studio Light");
            lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;

            Box("Table Base", new Vector3(0f, 0.22f, 0f), new Vector3(5.25f, 0.42f, 9.35f), _wood);
            Box("Emerald Cloth", new Vector3(0f, ClothY - 0.035f, 0f), new Vector3(4.65f, 0.08f, 8.72f), _cloth);
            for (int side = -1; side <= 1; side += 2)
                Box("Side Cushion", new Vector3(side * 2.39f, ClothY + 0.16f, 0f), new Vector3(0.2f, 0.31f, 8.55f), _wood);
            for (int end = -1; end <= 1; end += 2)
                Box("End Cushion", new Vector3(0f, ClothY + 0.16f, end * 4.39f), new Vector3(4.65f, 0.31f, 0.2f), _wood);

            float[] xs = { -2.38f, 0f, 2.38f };
            foreach (float z in new[] { -4.38f, 4.38f })
                foreach (float x in xs) MakePocket(new Vector3(x, ClothY - 0.03f, z));

            for (int i = 0; i < 6; i++)
            {
                float z = -3.3f + i * 1.32f;
                Box("Rail Marker", new Vector3(-2.51f, 0.27f, z), new Vector3(0.025f, 0.025f, 0.48f), _gold);
                Box("Rail Marker", new Vector3(2.51f, 0.27f, z), new Vector3(0.025f, 0.025f, 0.48f), _gold);
            }
            Box("Studio Floor", new Vector3(0f, -0.12f, 0f), new Vector3(200f, 0.1f, 200f), Mat(new Color(0.04f, 0.05f, 0.065f), 0.1f, 0.25f));
        }

        private void BuildCamera()
        {
            var go = new GameObject("Cue Legends Camera");
            _camera = go.AddComponent<Camera>();
            _camera.fieldOfView = 42f;
            _camera.nearClipPlane = 0.05f;
            _camera.farClipPlane = 100f;
            go.transform.position = new Vector3(0f, 10.7f, -7.1f);
            go.transform.LookAt(new Vector3(0f, ClothY, 0.35f));
            _camera.backgroundColor = new Color(0.025f, 0.035f, 0.05f);
        }

        private void BuildRack()
        {
            foreach (PoolBall ball in _balls) if (ball != null) Destroy(ball.gameObject);
            _balls.Clear();
            _pocketed = 0;
            _shots = 0;
            _cueBall = null;
            MakeBall(0, new Vector3(0f, ClothY + Radius, -2.75f));
            int[] order = { 1, 9, 2, 10, 8, 3, 11, 4, 12, 5, 13, 6, 14, 7, 15 };
            int n = 0;
            float spacing = Radius * 2.02f;
            for (int row = 0; row < 5; row++)
                for (int col = 0; col <= row; col++)
                {
                    float x = (col - row * 0.5f) * spacing;
                    float z = 1.25f + row * spacing * 0.88f;
                    MakeBall(order[n++], new Vector3(x, ClothY + Radius, z));
                }
            _status = "BREAK SHOT • CLEAR THE TABLE";
        }

        private void MakeBall(int number, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = number == 0 ? "Cue Ball" : "Pool Ball " + number;
            go.transform.position = position;
            go.transform.localScale = Vector3.one * Radius * 2f;
            go.GetComponent<Renderer>().material = BallMat(number);
            go.GetComponent<SphereCollider>().material = _ballPhysics;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.17f;
            rb.drag = 0.12f;
            rb.angularDrag = 0.08f;
            rb.maxAngularVelocity = 45f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            var ball = go.AddComponent<PoolBall>();
            ball.Initialize(number, this, rb);
            _balls.Add(ball);
            if (number == 0) _cueBall = rb;
        }

        private Material BallMat(int number)
        {
            if (number == 0) return _white;
            if (number == 8) return _black;
            Color[] palette =
            {
                new Color(0.95f, 0.78f, 0.08f), new Color(0.12f, 0.27f, 0.86f),
                new Color(0.82f, 0.08f, 0.10f), new Color(0.42f, 0.1f, 0.63f),
                new Color(0.98f, 0.34f, 0.06f), new Color(0.1f, 0.5f, 0.22f),
                new Color(0.55f, 0.08f, 0.12f)
            };
            return Mat(palette[(number - 1) % palette.Length], 0.12f, 0.38f);
        }

        private void BuildCueStick()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Cue Stick";
            go.transform.localScale = new Vector3(0.035f, 1.9f, 0.035f);
            go.GetComponent<Collider>().enabled = false;
            go.GetComponent<Renderer>().material = _cueMat;
            _cueStick = go.transform;
            go.SetActive(false);
        }

        private void MakePocket(Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Pocket";
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.58f, 0.035f, 0.58f);
            go.GetComponent<Renderer>().material = _black;
            var trigger = go.AddComponent<SphereCollider>();
            trigger.radius = 0.62f;
            trigger.isTrigger = true;
            go.AddComponent<PoolPocket>().Initialize(this);
        }

        private static void Box(string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = material;
        }

        private void Update()
        {
            if (_view != View.Playing || _paused || _cueBall == null) return;
            if (AllStopped()) ReadAimInput();
            UpdateCue();
        }

        private void ReadAimInput()
        {
            Vector2 point;
            bool down, held, up;
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                point = t.position;
                down = t.phase == TouchPhase.Began;
                held = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
                up = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            }
            else
            {
                point = Input.mousePosition;
                down = Input.GetMouseButtonDown(0);
                held = Input.GetMouseButton(0);
                up = Input.GetMouseButtonUp(0);
            }
            if (down) { _dragging = true; _dragStart = point; _dragNow = point; }
            else if (_dragging && held) { _dragNow = point; CalculateAim(); }
            else if (_dragging && up) { _dragNow = point; CalculateAim(); Strike(); _dragging = false; }
        }

        private Vector3 ToTable(Vector2 point)
        {
            Ray ray = _camera.ScreenPointToRay(point);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, ClothY + Radius, 0f));
            float distance;
            return plane.Raycast(ray, out distance) ? ray.GetPoint(distance) : _cueBall.position;
        }

        private void CalculateAim()
        {
            Vector3 pull = ToTable(_dragStart) - ToTable(_dragNow);
            pull.y = 0f;
            if (pull.sqrMagnitude > 0.0025f)
            {
                _aim = pull.normalized;
                _power = Mathf.Clamp01(pull.magnitude / 2.3f);
            }
        }

        private void Strike()
        {
            if (!AllStopped() || _power < 0.04f) return;
            _cueBall.AddForce(_aim * Mathf.Lerp(1.8f, 8.5f, _power), ForceMode.Impulse);
            _cueBall.AddTorque(Vector3.up * (_power * 0.35f), ForceMode.Impulse);
            _shots++;
            _status = "SHOT " + _shots;
        }

        private void UpdateCue()
        {
            if (_cueStick == null) return;
            bool visible = AllStopped() && !_paused && _view == View.Playing;
            _cueStick.gameObject.SetActive(visible);
            if (!visible) return;
            _cueStick.position = _cueBall.position - _aim * (0.8f + _power * 0.25f);
            _cueStick.rotation = Quaternion.FromToRotation(Vector3.up, _aim);
        }

        private bool AllStopped()
        {
            foreach (PoolBall ball in _balls)
                if (ball != null && !ball.IsPocketed && ball.Body != null &&
                    (ball.Body.velocity.sqrMagnitude > 0.0025f || ball.Body.angularVelocity.sqrMagnitude > 0.08f))
                    return false;
            return true;
        }

        internal void OnPocket(PoolBall ball)
        {
            if (ball == null || ball.IsPocketed) return;
            ball.MarkPocketed();
            _pocketed++;
            _tokens += 2;
            PlayerPrefs.SetInt("CueLegends.Tokens", _tokens);
            if (ball.Number == 0)
            {
                _status = "FOUL • CUE BALL SCRATCH";
                StartCoroutine(RespawnCue());
            }
            else if (ball.Number == 8)
            {
                _status = "8 BALL POCKETED • ROUND COMPLETE";
                _view = View.Results;
                PlayerPrefs.Save();
            }
            else _status = "BALL POCKETED • +2 TOKENS";
        }

        private System.Collections.IEnumerator RespawnCue()
        {
            yield return new WaitForSeconds(0.8f);
            if (_cueBall != null)
            {
                _cueBall.velocity = Vector3.zero;
                _cueBall.angularVelocity = Vector3.zero;
                _cueBall.position = new Vector3(0f, ClothY + Radius, -2.75f);
            }
        }

        private void OnGUI()
        {
            Styles();
            GUI.Box(new Rect(12, 10, 250, 52), GUIContent.none);
            GUI.Label(new Rect(20, 13, 235, 42), "CUE LEGENDS", _heading);
            GUI.Label(new Rect(Screen.width - 190, 16, 180, 34), "◆ " + _tokens + " TOKENS", _body);

            if (_view == View.Menu)
            {
                Panel("CUE LEGENDS", "THE PREMIUM POOL ROOM", "8 BALL • PRACTICE", "START PRACTICE", () => _view = View.Playing);
                if (GUI.Button(new Rect(Screen.width - 165, Screen.height - 65, 140, 42), "CUE SHOP", _button)) _view = View.Shop;
                GUI.Label(new Rect(20, Screen.height - 45, Screen.width - 40, 28), "ONLINE MATCHMAKING — NOT ENABLED YET", _small);
            }
            else if (_view == View.Playing)
            {
                GUI.Label(new Rect(18, 74, 330, 34), _status, _body);
                if (GUI.Button(new Rect(Screen.width - 138, 78, 112, 42), _paused ? "RESUME" : "PAUSE", _button))
                {
                    _paused = !_paused;
                    Time.timeScale = _paused ? 0f : 1f;
                }
                if (GUI.Button(new Rect(Screen.width - 138, 128, 112, 42), "SHOP", _button)) { _view = View.Shop; }
                GUI.Label(new Rect(18, Screen.height - 48, 380, 28), "DRAG BACK TO AIM • RELEASE TO STRIKE", _small);
                if (_paused) GUI.Label(new Rect(Screen.width / 2 - 80, Screen.height / 2 - 20, 160, 40), "PAUSED", _heading);
            }
            else if (_view == View.Shop) DrawShop();
            else if (_view == View.Results)
            {
                Panel("ROUND COMPLETE", _status, "BALLS POCKETED: " + _pocketed, "PLAY AGAIN", () => { BuildRack(); _view = View.Playing; });
                if (GUI.Button(new Rect(Screen.width / 2 - 90, Screen.height * 0.75f, 180, 42), "MAIN MENU", _button)) _view = View.Menu;
            }
        }

        private void DrawShop()
        {
            GUI.Box(new Rect(Screen.width / 2 - 245, Screen.height / 2 - 185, 490, 370), "CUE LEGENDS • CUE SHOP");
            string[] names = { "STARTER MAPLE", "ROYAL CARBON", "GOLDEN ELITE" };
            int[] prices = { 0, 120, 300 };
            for (int i = 0; i < names.Length; i++)
            {
                int item = i;
                float y = Screen.height / 2 - 105 + i * 72;
                GUI.Label(new Rect(Screen.width / 2 - 210, y, 250, 42), names[i] + " • " + prices[i] + " TOKENS", _body);
                bool owned = _ownedCues.Contains(i);
                string action = owned ? (_cueStyle == i ? "EQUIPPED" : "EQUIP") : "UNLOCK";
                if (GUI.Button(new Rect(Screen.width / 2 + 80, y, 120, 42), action, _button))
                {
                    if (owned) _cueStyle = item;
                    else if (_tokens >= prices[item])
                    {
                        _tokens -= prices[item];
                        _ownedCues.Add(item);
                        _cueStyle = item;
                        PlayerPrefs.SetInt("CueLegends.Tokens", _tokens);
                        PlayerPrefs.Save();
                    }
                }
            }
            GUI.Label(new Rect(Screen.width / 2 - 215, Screen.height / 2 + 112, 430, 36),
                "Prototype credits only. Real-money purchases are not active.", _small);
            if (GUI.Button(new Rect(Screen.width / 2 - 85, Screen.height / 2 + 145, 170, 38), "BACK", _button))
                _view = View.Playing;
        }

        private void Panel(string title, string subtitle, string detail, string action, Action onClick)
        {
            Rect r = new Rect(Screen.width / 2 - 225, Screen.height / 2 - 145, 450, 290);
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 20, r.y + 30, r.width - 40, 55), title, _heading);
            GUI.Label(new Rect(r.x + 20, r.y + 95, r.width - 40, 35), subtitle, _body);
            GUI.Label(new Rect(r.x + 20, r.y + 145, r.width - 40, 30), detail, _small);
            if (GUI.Button(new Rect(r.x + 55, r.y + 205, r.width - 110, 50), action, _button)) onClick?.Invoke();
        }

        private void Styles()
        {
            if (_heading != null) return;
            _heading = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _body = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _small = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold };
        }
    }

    public sealed class PoolBall : MonoBehaviour
    {
        public int Number { get; private set; }
        public bool IsPocketed { get; private set; }
        public Rigidbody Body { get; private set; }
        private PoolGameController _owner;

        public void Initialize(int number, PoolGameController owner, Rigidbody body)
        {
            Number = number;
            _owner = owner;
            Body = body;
        }

        public void MarkPocketed()
        {
            IsPocketed = true;
            if (Body != null) { Body.velocity = Vector3.zero; Body.angularVelocity = Vector3.zero; }
            gameObject.SetActive(false);
        }

        public void NotifyPocket() { if (_owner != null) _owner.OnPocket(this); }
    }

    public sealed class PoolPocket : MonoBehaviour
    {
        private PoolGameController _owner;
        public void Initialize(PoolGameController owner) { _owner = owner; }
        private void OnTriggerEnter(Collider other)
        {
            PoolBall ball = other.GetComponent<PoolBall>();
            if (ball != null && !ball.IsPocketed) ball.NotifyPocket();
        }
    }
}
