using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Amane.Field
{
    /// <summary>
    /// 3Dフィールドをコードだけで構築するビルダー。
    /// Capsule=キャラクター、Cube=建物、Plane=地面。
    /// PrototypeBootstrapから呼ばれる。
    /// </summary>
    public static class Field3DBuilder
    {
        // DESIGN.md カラーパレット
        private static readonly Color Pink = new(1f, 0.243f, 0.541f);
        private static readonly Color DeepBlue = new(0.106f, 0.165f, 0.29f);
        private static readonly Color ReadBlue = new(0.31f, 0.66f, 1f);
        private static readonly Color InkBlack = new(0.078f, 0.067f, 0.059f);

        public struct BuildResult
        {
            public GameObject Root;
            public PlayerController3D Player;
            public ThirdPersonCamera Camera;
            public List<NPC3D> NPCs;
            public List<LocationMarker3D> Locations;
        }

        public static BuildResult Build()
        {
            var root = new GameObject("Field3D");
            var result = new BuildResult
            {
                Root = root,
                NPCs = new List<NPC3D>(),
                Locations = new List<LocationMarker3D>()
            };

            // ---- 地面 ----
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.localScale = new Vector3(8, 1, 8);
            ground.transform.position = Vector3.zero;
            SetColor(ground, new Color(0.15f, 0.17f, 0.25f));

            // 水路（DESIGN.md: 水路と霧の多い地方都市）
            CreateWaterChannel(root.transform, new Vector3(0, 0.01f, 10), new Vector3(80, 0.1f, 3));
            CreateWaterChannel(root.transform, new Vector3(-15, 0.01f, 0), new Vector3(3, 0.1f, 40));

            // ---- 道路 ----
            CreateRoad(root.transform, new Vector3(0, 0.02f, 0), new Vector3(60, 0.05f, 4));
            CreateRoad(root.transform, new Vector3(0, 0.02f, -15), new Vector3(60, 0.05f, 4));
            CreateRoad(root.transform, new Vector3(10, 0.02f, -7.5f), new Vector3(4, 0.05f, 30));

            // ---- 建物 ----
            CreateBuilding(root.transform, "雨音西高校", new Vector3(-20, 3.5f, 20), new Vector3(12, 7, 8),
                new Color(0.25f, 0.3f, 0.45f), result.Locations, LocationType.Study);
            CreateBuilding(root.transform, "古書堂「八雲」", new Vector3(20, 2f, 15), new Vector3(5, 4, 5),
                new Color(0.4f, 0.3f, 0.2f), result.Locations, LocationType.Shop);
            CreateBuilding(root.transform, "喫茶バイト", new Vector3(25, 1.5f, -10), new Vector3(5, 3, 5),
                new Color(0.3f, 0.5f, 0.3f), result.Locations, LocationType.Job);
            CreateBuilding(root.transform, "水神社", new Vector3(-25, 2f, -15), new Vector3(6, 4, 5),
                new Color(0.4f, 0.35f, 0.5f), result.Locations, LocationType.Meditate);
            CreateBuilding(root.transform, "自宅", new Vector3(28, 2f, 22), new Vector3(5, 4, 5),
                new Color(0.3f, 0.3f, 0.35f), result.Locations, LocationType.Home);

            // 未言界の入口（赤い光を放つ柱）
            var dungeonEntry = CreateDungeonEntrance(root.transform, new Vector3(0, 0, 25));
            var dungeonLoc = dungeonEntry.AddComponent<LocationMarker3D>();
            dungeonLoc.Id = "dungeon";
            dungeonLoc.DisplayName = "未言界の入口";
            dungeonLoc.Type = LocationType.Dungeon;
            dungeonLoc.InteractRadius = 4f;
            result.Locations.Add(dungeonLoc);

            // ---- NPC ----
            result.NPCs.Add(CreateNPC(root.transform, "akari", "灯里", new Vector3(-8, 0, 5),
                new Color(1f, 0.8f, 0.3f)));
            result.NPCs.Add(CreateNPC(root.transform, "ritsu", "律", new Vector3(12, 0, 8),
                new Color(0.3f, 0.4f, 0.7f)));
            result.NPCs.Add(CreateNPC(root.transform, "ren", "蓮", new Vector3(5, 0, -12),
                new Color(0.8f, 0.25f, 0.25f)));

            // ---- プレイヤー ----
            result.Player = CreatePlayer(root.transform);

            // ---- カメラ ----
            result.Camera = CreateCamera(root.transform, result.Player.transform);

            // ---- 環境光 ----
            SetupLighting(root.transform);

            // ---- 霧（雨音市の雰囲気）----
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.1f, 0.12f, 0.18f);
            RenderSettings.fogDensity = 0.02f;
            RenderSettings.fogMode = FogMode.Exponential;

            return result;
        }

        // ---- プレイヤー ----
        private static PlayerController3D CreatePlayer(Transform parent)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.name = "Player_Yomi";
            obj.transform.SetParent(parent);
            obj.transform.position = new Vector3(0, 1, 0);
            obj.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            SetColor(obj, Pink);

            // 頭部（球）
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(obj.transform);
            head.transform.localPosition = new Vector3(0, 0.8f, 0);
            head.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
            SetColor(head, new Color(0.95f, 0.85f, 0.75f)); // 肌色
            Object.Destroy(head.GetComponent<Collider>());

            // 名前表示用の3D Text は別途UIで表示

            var ctrl = obj.AddComponent<PlayerController3D>();
            var rb = obj.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            return ctrl;
        }

        // ---- NPC ----
        private static NPC3D CreateNPC(Transform parent, string id, string displayName,
            Vector3 position, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.name = $"NPC_{id}";
            obj.transform.SetParent(parent);
            obj.transform.position = position + new Vector3(0, 1, 0);
            obj.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            SetColor(obj, color);

            // 頭
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(obj.transform);
            head.transform.localPosition = new Vector3(0, 0.8f, 0);
            head.transform.localScale = new Vector3(0.65f, 0.5f, 0.65f);
            SetColor(head, new Color(0.95f, 0.85f, 0.75f));
            Object.Destroy(head.GetComponent<Collider>());

            var npc = obj.AddComponent<NPC3D>();
            npc.Id = id;
            npc.DisplayName = displayName;
            npc.Type = LocationType.NPC;
            npc.InteractRadius = 3f;

            // NPCはゆっくり浮遊アニメーション
            var anim = obj.AddComponent<FloatAnimation>();
            anim.Amplitude = 0.1f;
            anim.Speed = 1.5f;

            return npc;
        }

        // ---- 建物 ----
        private static void CreateBuilding(Transform parent, string name, Vector3 position,
            Vector3 scale, Color color, List<LocationMarker3D> locations, LocationType type)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = $"Building_{name}";
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            SetColor(obj, color);

            // 屋根
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(obj.transform);
            roof.transform.localPosition = new Vector3(0, 0.55f, 0);
            roof.transform.localScale = new Vector3(1.1f, 0.1f, 1.1f);
            SetColor(roof, new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f));
            Object.Destroy(roof.GetComponent<Collider>());

            // ロケーションマーカー
            var loc = obj.AddComponent<LocationMarker3D>();
            loc.Id = name;
            loc.DisplayName = name;
            loc.Type = type;
            loc.InteractRadius = 5f;
            locations.Add(loc);
        }

        // ---- 未言界の入口 ----
        private static GameObject CreateDungeonEntrance(Transform parent, Vector3 position)
        {
            var obj = new GameObject("DungeonEntrance");
            obj.transform.SetParent(parent);
            obj.transform.position = position;

            // 赤い柱
            var pillar1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar1.transform.SetParent(obj.transform);
            pillar1.transform.localPosition = new Vector3(-1.5f, 2, 0);
            pillar1.transform.localScale = new Vector3(0.5f, 4, 0.5f);
            SetColor(pillar1, new Color(0.6f, 0.1f, 0.2f));
            Object.Destroy(pillar1.GetComponent<Collider>());

            var pillar2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar2.transform.SetParent(obj.transform);
            pillar2.transform.localPosition = new Vector3(1.5f, 2, 0);
            pillar2.transform.localScale = new Vector3(0.5f, 4, 0.5f);
            SetColor(pillar2, new Color(0.6f, 0.1f, 0.2f));
            Object.Destroy(pillar2.GetComponent<Collider>());

            // 上の横棒
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.transform.SetParent(obj.transform);
            beam.transform.localPosition = new Vector3(0, 4.2f, 0);
            beam.transform.localScale = new Vector3(4, 0.4f, 0.5f);
            SetColor(beam, new Color(0.5f, 0.08f, 0.15f));
            Object.Destroy(beam.GetComponent<Collider>());

            // 光る球（入口の核）
            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "DungeonGlow";
            glow.transform.SetParent(obj.transform);
            glow.transform.localPosition = new Vector3(0, 2, 0);
            glow.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            SetColor(glow, Pink);
            Object.Destroy(glow.GetComponent<Collider>());
            glow.AddComponent<FloatAnimation>().Speed = 2f;

            // コライダー
            var col = obj.AddComponent<BoxCollider>();
            col.size = new Vector3(4, 5, 2);
            col.center = new Vector3(0, 2.5f, 0);
            col.isTrigger = true;

            return obj;
        }

        // ---- 水路 ----
        private static void CreateWaterChannel(Transform parent, Vector3 pos, Vector3 scale)
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.name = "Water";
            water.transform.SetParent(parent);
            water.transform.position = pos;
            water.transform.localScale = scale;
            SetColor(water, new Color(0.15f, 0.25f, 0.45f, 0.7f));
            Object.Destroy(water.GetComponent<Collider>());
        }

        // ---- 道路 ----
        private static void CreateRoad(Transform parent, Vector3 pos, Vector3 scale)
        {
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road";
            road.transform.SetParent(parent);
            road.transform.position = pos;
            road.transform.localScale = scale;
            SetColor(road, new Color(0.2f, 0.22f, 0.28f));
            Object.Destroy(road.GetComponent<Collider>());
        }

        // ---- カメラ ----
        private static ThirdPersonCamera CreateCamera(Transform parent, Transform target)
        {
            var camObj = new GameObject("FollowCamera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(parent);
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.16f);
            cam.fieldOfView = 50;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 200f;

            var follow = camObj.AddComponent<ThirdPersonCamera>();
            follow.Target = target;
            follow.Distance = 10f;
            follow.Height = 7f;
            follow.LookAtOffset = new Vector3(0, 1, 0);
            follow.SmoothSpeed = 5f;

            return follow;
        }

        // ---- ライティング ----
        private static void SetupLighting(Transform parent)
        {
            // 既存のDirectional Lightを調整
            var existingLight = Object.FindAnyObjectByType<Light>();
            if (existingLight != null)
            {
                existingLight.color = new Color(0.6f, 0.65f, 0.8f); // 冷たい青白い光
                existingLight.intensity = 0.8f;
                existingLight.transform.rotation = Quaternion.Euler(45, -30, 0);
            }

            // 補助光（暖色のポイントライト）
            var warmLight = new GameObject("WarmLight");
            warmLight.transform.SetParent(parent);
            warmLight.transform.position = new Vector3(0, 8, 0);
            var light = warmLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.7f, 0.5f);
            light.intensity = 0.5f;
            light.range = 50f;
        }

        // ---- ユーティリティ ----
        private static Shader _cachedShader;

        private static Shader FindURPShader()
        {
            if (_cachedShader != null) return _cachedShader;

            // Unity 6 + URP で利用可能なシェーダーを優先順に探す
            string[] candidates = new[]
            {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Simple Lit",
                "Universal Render Pipeline/Unlit",
                "Sprites/Default",    // URP互換の安全なフォールバック
                "UI/Default",         // 最終フォールバック
            };

            foreach (var name in candidates)
            {
                var s = Shader.Find(name);
                if (s != null)
                {
                    Debug.Log($"[Field3D] Using shader: {name}");
                    _cachedShader = s;
                    return s;
                }
            }

            // 最終手段: Standard（URPではピンクになるが、nullよりはマシ）
            Debug.LogWarning("[Field3D] No URP shader found, falling back to Standard (may render pink)");
            _cachedShader = Shader.Find("Standard");
            return _cachedShader;
        }

        private static void SetColor(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = FindURPShader();
                var mat = new Material(shader);

                // URP Lit は _BaseColor, Simple Lit / Unlit も _BaseColor
                // Sprites/Default は _Color
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                else
                    mat.color = color;

                if (color.a < 1f)
                {
                    // URP Lit の透過設定
                    if (mat.HasProperty("_Surface"))
                    {
                        mat.SetFloat("_Surface", 1); // Transparent
                        mat.SetFloat("_Blend", 0);   // Alpha
                        mat.SetOverrideTag("RenderType", "Transparent");
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.renderQueue = 3000;
                    }
                    else
                    {
                        // Sprites/Default等は自動で透過サポート
                        mat.renderQueue = 3000;
                    }
                }

                renderer.material = mat;
            }
        }
    }
}
