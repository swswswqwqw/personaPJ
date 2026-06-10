using UnityEngine;
using System.Collections.Generic;

namespace Amane.Field
{
    /// <summary>
    /// 雨音市の環境演出: 雨（GameObject落下）、霧、時間帯ライティング。
    /// DESIGN.md: 「常に小雨が似合う『言葉が湿る街』」
    /// ParticleSystem モジュール不要 — プリミティブのみで雨を表現。
    /// </summary>
    public sealed class FieldEnvironment : MonoBehaviour
    {
        private Light _sunLight;
        private Light _warmLight;
        private SimpleRain _rain;

        // 時間帯ごとのライティング設定
        private static readonly LightingPreset[] Presets = new[]
        {
            // 朝: 淡いオレンジ
            new LightingPreset(new Color(0.8f, 0.7f, 0.5f), 0.6f,
                new Color(0.15f, 0.13f, 0.2f), 0.015f, new Color(0.12f, 0.14f, 0.2f)),
            // 昼: 青白い光（雨音市は常に曇り気味）
            new LightingPreset(new Color(0.6f, 0.65f, 0.8f), 0.8f,
                new Color(0.1f, 0.12f, 0.18f), 0.02f, new Color(0.15f, 0.17f, 0.25f)),
            // 夕方: オレンジ〜紫
            new LightingPreset(new Color(0.9f, 0.5f, 0.3f), 0.5f,
                new Color(0.12f, 0.08f, 0.15f), 0.025f, new Color(0.1f, 0.08f, 0.15f)),
            // 夜: 深い藍
            new LightingPreset(new Color(0.2f, 0.25f, 0.4f), 0.3f,
                new Color(0.05f, 0.06f, 0.1f), 0.03f, new Color(0.06f, 0.07f, 0.12f)),
            // 深夜: ほぼ闇＋蛍光ピンクのアクセント
            new LightingPreset(new Color(0.15f, 0.1f, 0.2f), 0.15f,
                new Color(0.03f, 0.03f, 0.06f), 0.04f, new Color(0.04f, 0.04f, 0.08f)),
        };

        public void Initialize(Transform fieldRoot)
        {
            CreateRainSystem(fieldRoot);
            FindLights(fieldRoot);
            CreateNPCNamePlates(fieldRoot);
        }

        /// <summary>時間帯に応じてライティングを変更</summary>
        public void SetTimeOfDay(int slotIndex)
        {
            int idx = Mathf.Clamp(slotIndex, 0, Presets.Length - 1);
            var preset = Presets[idx];

            if (_sunLight != null)
            {
                _sunLight.color = preset.SunColor;
                _sunLight.intensity = preset.SunIntensity;
            }

            RenderSettings.fogColor = preset.FogColor;
            RenderSettings.fogDensity = preset.FogDensity;

            if (Camera.main != null)
                Camera.main.backgroundColor = preset.SkyColor;
        }

        // =============================================
        //  雨（プリミティブ落下方式 — ParticleSystem不要）
        // =============================================
        private void CreateRainSystem(Transform parent)
        {
            var rainObj = new GameObject("Rain");
            rainObj.transform.SetParent(parent);
            _rain = rainObj.AddComponent<SimpleRain>();
        }

        // =============================================
        //  NPC頭上ネームプレート（3D Text）
        // =============================================
        private void CreateNPCNamePlates(Transform fieldRoot)
        {
            var npcs = fieldRoot.GetComponentsInChildren<NPC3D>(true);
            foreach (var npc in npcs)
            {
                CreateNamePlate(npc.transform, npc.DisplayName, new Color(1f, 0.9f, 0.3f));
            }

            // プレイヤーにもネームプレート
            var player = fieldRoot.GetComponentInChildren<PlayerController3D>(true);
            if (player != null)
                CreateNamePlate(player.transform, "詠", new Color(1f, 0.243f, 0.541f));
        }

        private void CreateNamePlate(Transform parent, string text, Color color)
        {
            var plateObj = new GameObject($"NamePlate_{text}");
            plateObj.transform.SetParent(parent);
            plateObj.transform.localPosition = new Vector3(0, 2.2f, 0);

            var billboard = plateObj.AddComponent<BillboardText>();
            billboard.Initialize(text, color, 0.015f);
        }

        private void FindLights(Transform root)
        {
            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                if (light.type == LightType.Directional || light.type == LightType.Spot)
                    _sunLight = light;
                else if (light.type == LightType.Point)
                    _warmLight = light;
            }

            if (_sunLight == null)
                _sunLight = Object.FindAnyObjectByType<Light>();
        }

        private struct LightingPreset
        {
            public readonly Color SunColor;
            public readonly float SunIntensity;
            public readonly Color FogColor;
            public readonly float FogDensity;
            public readonly Color SkyColor;

            public LightingPreset(Color sun, float intensity, Color fog, float fogDensity, Color sky)
            {
                SunColor = sun;
                SunIntensity = intensity;
                FogColor = fog;
                FogDensity = fogDensity;
                SkyColor = sky;
            }
        }
    }

    /// <summary>
    /// ParticleSystem不要の簡易雨システム。
    /// 薄い縦長Cubeをプールし、上から落とす。
    /// </summary>
    public sealed class SimpleRain : MonoBehaviour
    {
        private const int DropCount = 120;
        private const float SpawnRadius = 30f;
        private const float SpawnHeight = 18f;
        private const float FallSpeed = 20f;
        private const float DropLength = 0.8f;
        private const float ResetY = -1f;

        private Transform[] _drops;
        private float[] _speeds;

        private void Start()
        {
            // 雨粒用のマテリアル（半透明の白〜青）
            var shader = Shader.Find("Sprites/Default")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("UI/Default");
            var mat = new Material(shader);
            mat.color = new Color(0.6f, 0.75f, 1f, 0.2f);

            // 共有メッシュ（Cubeの代わりにQuad的な薄板）
            // Cubeは重いのでLineRendererでもなく直接メッシュを薄く
            _drops = new Transform[DropCount];
            _speeds = new float[DropCount];

            for (int i = 0; i < DropCount; i++)
            {
                var drop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                drop.name = "Raindrop";
                drop.transform.SetParent(transform);
                drop.transform.localScale = new Vector3(0.02f, DropLength, 0.02f);

                // コライダー不要
                Object.Destroy(drop.GetComponent<Collider>());

                // マテリアル設定
                var rend = drop.GetComponent<Renderer>();
                rend.material = mat;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rend.receiveShadows = false;

                // ランダム位置に配置
                ResetDrop(drop.transform, true);
                _drops[i] = drop.transform;
                _speeds[i] = FallSpeed + Random.Range(-4f, 4f);
            }
        }

        private void Update()
        {
            if (_drops == null) return;

            // プレイヤー追従
            var player = Object.FindAnyObjectByType<PlayerController3D>();
            Vector3 center = player != null ? player.transform.position : Vector3.zero;
            transform.position = new Vector3(center.x, 0, center.z);

            for (int i = 0; i < _drops.Length; i++)
            {
                if (_drops[i] == null) continue;
                _drops[i].localPosition += Vector3.down * _speeds[i] * UnityEngine.Time.deltaTime;

                if (_drops[i].localPosition.y < ResetY)
                    ResetDrop(_drops[i], false);
            }
        }

        private void ResetDrop(Transform drop, bool randomY)
        {
            float x = Random.Range(-SpawnRadius, SpawnRadius);
            float z = Random.Range(-SpawnRadius, SpawnRadius);
            float y = randomY ? Random.Range(ResetY, SpawnHeight) : SpawnHeight + Random.Range(0f, 3f);
            drop.localPosition = new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// 3DテキストをBillboard表示するコンポーネント。
    /// TextMeshPro不要 — 3D TextMeshで表示。
    /// </summary>
    public sealed class BillboardText : MonoBehaviour
    {
        private TextMesh _textMesh;

        public void Initialize(string text, Color color, float charSize = 0.02f)
        {
            _textMesh = gameObject.AddComponent<TextMesh>();
            _textMesh.text = text;
            _textMesh.characterSize = charSize;
            _textMesh.fontSize = 80;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.color = color;
            _textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null && _textMesh.font != null)
                renderer.material = _textMesh.font.material;
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;

            var lookDir = cam.transform.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }
}
