using UnityEngine;

namespace OneShotFootball
{
    /// <summary>
    /// Applies store cosmetics to explicitly assigned or conventionally named renderer slots
    /// on imported Humanoid athlete prefabs. Uses MaterialPropertyBlock to avoid material clones.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FootballerAppearance : MonoBehaviour
    {
        [Header("Optional explicit renderer slots")]
        [SerializeField] private Renderer jersey;
        [SerializeField] private Renderer shorts;
        [SerializeField] private Renderer boots;
        [SerializeField] private Renderer skin;
        [SerializeField] private Renderer hair;
        [SerializeField] private Renderer captainBand;

        private Renderer[] _renderers;
        private MaterialPropertyBlock _block;

        public void RefreshRenderers()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            if (_block == null) _block = new MaterialPropertyBlock();

            if (jersey == null) jersey = FindRenderer("jersey", "shirt", "top");
            if (shorts == null) shorts = FindRenderer("shorts");
            if (boots == null) boots = FindRenderer("boot", "shoe");
            if (skin == null) skin = FindRenderer("skin", "body");
            if (hair == null) hair = FindRenderer("hair");
            if (captainBand == null) captainBand = FindRenderer("armband", "captain");
        }

        public void Apply(string playerLookId, string kitId, string equipmentId)
        {
            if (_renderers == null) RefreshRenderers();

            Color skinColor = playerLookId == "player.pro.forward"
                ? new Color(0.72f, 0.49f, 0.32f)
                : playerLookId == "player.street.legend"
                    ? new Color(0.55f, 0.34f, 0.23f)
                    : new Color(0.68f, 0.43f, 0.27f);

            Color jerseyColor = kitId == "kit.neon"
                ? new Color(0.02f, 0.83f, 0.70f)
                : kitId == "kit.midnight"
                    ? new Color(0.08f, 0.09f, 0.17f)
                    : playerLookId == "player.pro.forward"
                        ? new Color(0.04f, 0.55f, 0.68f)
                        : playerLookId == "player.street.legend"
                            ? new Color(0.36f, 0.08f, 0.58f)
                            : new Color(0.05f, 0.28f, 0.91f);

            Color shortsColor = kitId == "kit.neon"
                ? new Color(0.02f, 0.08f, 0.11f)
                : kitId == "kit.midnight"
                    ? new Color(0.03f, 0.04f, 0.08f)
                    : new Color(0.04f, 0.08f, 0.18f);
            Color bootColor = equipmentId == "equipment.speed.boots"
                ? new Color(0.10f, 0.92f, 0.34f)
                : new Color(0.06f, 0.08f, 0.12f);

            SetColor(jersey, jerseyColor);
            SetColor(shorts, shortsColor);
            SetColor(boots, bootColor);
            SetColor(skin, skinColor);
            SetColor(hair, new Color(0.10f, 0.07f, 0.05f));

            if (captainBand != null)
                captainBand.gameObject.SetActive(equipmentId == "equipment.captain.band");
        }

        private Renderer FindRenderer(params string[] tokens)
        {
            if (_renderers == null) return null;
            foreach (Renderer candidate in _renderers)
            {
                if (candidate == null) continue;
                string candidateName = candidate.gameObject.name.ToLowerInvariant();
                for (int i = 0; i < tokens.Length; i++)
                    if (candidateName.Contains(tokens[i]))
                        return candidate;
            }
            return null;
        }

        private void SetColor(Renderer target, Color color)
        {
            if (target == null) return;

            target.GetPropertyBlock(_block);
            Material shared = target.sharedMaterial;
            if (shared != null && shared.HasProperty("_BaseColor"))
                _block.SetColor("_BaseColor", color);
            if (shared != null && shared.HasProperty("_Color"))
                _block.SetColor("_Color", color);
            target.SetPropertyBlock(_block);
        }
    }
}