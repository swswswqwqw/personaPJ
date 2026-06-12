using UnityEngine;
using UnityEngine.UI;
using Amane.Core;
using Amane.Core.Audio;
using Amane.UI;
using Amane.UI.Effects;
using Amane.Field;
using Amane.Battle;
using Amane.Echo;

namespace Amane.Prototype
{
    [DefaultExecutionOrder(-900)]
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        private Canvas _canvas;
        private GameManager _gm;

        // Panels
        private GameObject _titlePanel;
        private GameObject _fieldPanel;
        private GameObject _battlePanel;
        private GameObject _dungeonPanel;

        // Controllers
        private TitleScreenController _titleCtrl;
        private FieldController _fieldCtrl;
        private BattleController _battleCtrl;
        private DungeonController _dungeonCtrl;

        // Effects (shared across panels)
        private TransitionEffect _transitionEffect;
        private BattleEffects _battleEffects;
        private BondRankUpEffect _bondRankUpEffect;

        // 3D Field
        private GameObject _field3DRoot;
        private FieldManager3D _fieldManager3D;
        private FieldEnvironment _fieldEnvironment;

        // Audio
        private AudioManager _audioManager;

        // EXP/LevelUp
        private LevelUpEffect _levelUpEffect;

        private void Awake()
        {
            CreateCanvas();
            CreateAudioManager(); // GameManagerの前に作成してBGM再生に備える

            // InventoryManager（アイテム・所持金管理）
            if (InventoryManager.Instance == null)
            {
                var invObj = new GameObject("InventoryManager");
                invObj.AddComponent<InventoryManager>();
            }

            var gmObj = new GameObject("GameManager");
            _gm = gmObj.AddComponent<GameManager>();
            _gm.Machine.OnStateChanged += OnStateChanged;

            CreateFieldPanel();
            CreateTitlePanel();
            CreateBattlePanel();
            CreateDungeonPanel();
            CreateEffectsOverlay();
            Create3DField();
            CreateDungeonSystems();

            // 初期状態の同期（すでにTitleStateになっている可能性があるため）
            SyncCurrentState();
        }

        private void SyncCurrentState()
        {
            if (_gm != null && _gm.Machine != null)
            {
                OnStateChanged(null, _gm.Machine.Current);
            }
        }

        private void OnStateChanged(IState from, IState to)
        {
            if (to is TitleState)
            {
                ShowOnly(_titlePanel);
                _audioManager?.PlayBGM(BGMStyle.Title);
            }
            else if (to is FieldState)
            {
                ShowOnly(_fieldPanel);
                _audioManager?.PlayBGM(BGMStyle.Field);
            }
            else if (to is BattleState)
            {
                ShowOnly(_battlePanel);
                _audioManager?.PlayBGM(BGMStyle.Battle);
            }
            else if (to is DungeonState)
            {
                ShowOnly(_dungeonPanel);
                _audioManager?.PlayBGM(BGMStyle.Battle); // TODO: ダンジョン専用BGM
            }
        }

        private Camera _mainCam; // シーン既存のカメラ

        private void ShowOnly(GameObject panel)
        {
            _titlePanel.SetActive(panel == _titlePanel);
            _fieldPanel.SetActive(panel == _fieldPanel);
            _battlePanel.SetActive(panel == _battlePanel);
            _dungeonPanel.SetActive(panel == _dungeonPanel);

            bool showField = (panel == _fieldPanel);

            // 3Dフィールドの表示切替
            if (_field3DRoot != null)
                _field3DRoot.SetActive(showField);

            // カメラ切替: フィールド時は3Dカメラ、それ以外はメインカメラ
            if (_mainCam != null)
                _mainCam.gameObject.SetActive(!showField);
        }

        private void CreateCanvas()
        {
            var canvasObj = new GameObject("UI Canvas");
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            var evtSys = new GameObject("EventSystem");
            evtSys.AddComponent<UnityEngine.EventSystems.EventSystem>();

            // Unity 6 + Both Mode: StandaloneInputModule is generally safer for a prototype 
            // unless a full InputActionAsset is assigned and configured.
            evtSys.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // =============================================
        //  EFFECTS OVERLAY（全画面に重ねる最上位レイヤー）
        // =============================================
        private void CreateEffectsOverlay()
        {
            // --- Transition Effect (最上位) ---
            var transGroup = CreateFullscreenGroup("TransitionGroup");
            transGroup.alpha = 0;
            transGroup.blocksRaycasts = false;
            var transOverlay = MakeFullscreenImage(transGroup.transform, "TransOverlay", new Color(0, 0, 0, 0));
            var transText = MakeText(transGroup.transform, "", 24, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(500, 50));
            transText.color = Color.white;

            _transitionEffect = transGroup.gameObject.AddComponent<TransitionEffect>();
            SetPrivateField(_transitionEffect, "_group", transGroup);
            SetPrivateField(_transitionEffect, "_overlay", transOverlay);
            SetPrivateField(_transitionEffect, "_transitionText", transText);

            // --- Bond Rank Up Effect ---
            var bondGroup = CreateFullscreenGroup("BondRankUpGroup");
            bondGroup.alpha = 0;
            bondGroup.gameObject.SetActive(false);
            var bondBg = MakeFullscreenImage(bondGroup.transform, "BondBg", new Color(0, 0, 0, 0.9f));
            var envelope = MakeImage(bondGroup.transform, "Envelope", new Vector2(0, 30), new Vector2(120, 80),
                new Color(0.93f, 0.91f, 0.85f));
            var glowImg = MakeImage(bondGroup.transform, "Glow", new Vector2(0, 30), new Vector2(200, 200),
                new Color(1f, 0.243f, 0.541f, 0));
            var bondTitle = MakeText(bondGroup.transform, "", 32, TextAnchor.MiddleCenter,
                new Vector2(0, -30), new Vector2(300, 45));
            bondTitle.color = Color.white;
            var rankTxt = MakeText(bondGroup.transform, "", 48, TextAnchor.MiddleCenter,
                new Vector2(0, -80), new Vector2(300, 60));
            rankTxt.color = new Color(1f, 0.243f, 0.541f);
            var charNameTxt = MakeText(bondGroup.transform, "", 22, TextAnchor.MiddleCenter,
                new Vector2(0, -120), new Vector2(300, 35));
            charNameTxt.color = Color.white;

            _bondRankUpEffect = bondGroup.gameObject.AddComponent<BondRankUpEffect>();
            SetPrivateField(_bondRankUpEffect, "_group", bondGroup);
            SetPrivateField(_bondRankUpEffect, "_background", bondBg);
            SetPrivateField(_bondRankUpEffect, "_envelope", envelope);
            SetPrivateField(_bondRankUpEffect, "_glowEffect", glowImg);
            SetPrivateField(_bondRankUpEffect, "_bondTitle", bondTitle);
            SetPrivateField(_bondRankUpEffect, "_rankText", rankTxt);
            SetPrivateField(_bondRankUpEffect, "_characterName", charNameTxt);

            // --- Battle Effects (screen-level overlays) ---
            var fxObj = new GameObject("BattleEffects");
            fxObj.transform.SetParent(_canvas.transform, false);
            var fxRt = fxObj.AddComponent<RectTransform>();
            fxRt.anchorMin = Vector2.zero;
            fxRt.anchorMax = Vector2.one;
            fxRt.offsetMin = Vector2.zero;
            fxRt.offsetMax = Vector2.zero;

            // Screen flash
            var screenFlash = MakeFullscreenImage(fxObj.transform, "ScreenFlash", new Color(1, 1, 1, 0));
            screenFlash.gameObject.SetActive(false);

            // WEAK banner
            var weakBanner = MakeImage(fxObj.transform, "WeakBanner", new Vector2(0, 60), new Vector2(300, 50),
                new Color(1f, 0.243f, 0.541f));
            weakBanner.gameObject.SetActive(false);
            var weakText = MakeText(weakBanner.transform, "WEAK!", 28, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(280, 45));
            weakText.color = Color.white;
            weakText.fontStyle = FontStyle.Bold;

            // Crack overlay
            var crackOverlay = MakeFullscreenImage(fxObj.transform, "CrackOverlay", new Color(1, 1, 1, 0));
            crackOverlay.gameObject.SetActive(false);

            // All-Out Confession group
            var allOutGroup = CreateSubCanvasGroup(fxObj.transform, "AllOutGroup");
            allOutGroup.alpha = 0;
            allOutGroup.gameObject.SetActive(false);
            var allOutBg = MakeFullscreenImage(allOutGroup.transform, "AllOutBg", new Color(0.078f, 0.067f, 0.059f));
            var allOutTitle = MakeText(allOutGroup.transform, "ALL-OUT CONFESSION", 36, TextAnchor.MiddleCenter,
                new Vector2(0, 60), new Vector2(500, 50));
            allOutTitle.color = new Color(1f, 0.243f, 0.541f);
            allOutTitle.fontStyle = FontStyle.Bold;
            var allOutQuote = MakeText(allOutGroup.transform, "", 20, TextAnchor.MiddleCenter,
                new Vector2(0, -20), new Vector2(500, 60));
            allOutQuote.color = Color.white;
            var finisherPortrait = MakeImage(allOutGroup.transform, "FinisherPortrait", new Vector2(0, -90),
                new Vector2(80, 80), new Color(0.5f, 0.5f, 0.5f, 0.5f));

            // Kotsugi group
            var kotsugiGroup = CreateSubCanvasGroup(fxObj.transform, "KotsugiGroup");
            kotsugiGroup.alpha = 0;
            kotsugiGroup.gameObject.SetActive(false);
            var kotsugiText = MakeText(kotsugiGroup.transform, "", 28, TextAnchor.MiddleCenter,
                new Vector2(0, 20), new Vector2(400, 40));
            kotsugiText.color = Color.white;
            kotsugiText.fontStyle = FontStyle.Bold;
            var kotsugiBonus = MakeText(kotsugiGroup.transform, "", 22, TextAnchor.MiddleCenter,
                new Vector2(0, -25), new Vector2(300, 35));
            kotsugiBonus.color = new Color(1f, 0.7f, 0.2f);

            // Damage pop template (hidden)
            var dmgPop = MakeText(fxObj.transform, "", 24, TextAnchor.MiddleCenter,
                new Vector2(0, 0), new Vector2(100, 40));
            dmgPop.gameObject.SetActive(false);

            // --- Perfect Kotsugi group (DESIGN.md 9-1) ---
            var perfectKotsugiGroup = CreateSubCanvasGroup(fxObj.transform, "PerfectKotsugiGroup");
            perfectKotsugiGroup.alpha = 0;
            perfectKotsugiGroup.gameObject.SetActive(false);
            var perfectKtTitle = MakeText(perfectKotsugiGroup.transform, "", 34, TextAnchor.MiddleCenter,
                new Vector2(0, 30), new Vector2(500, 50));
            perfectKtTitle.color = new Color(1f, 0.243f, 0.541f);
            perfectKtTitle.fontStyle = FontStyle.Bold;
            var perfectKtSubtext = MakeText(perfectKotsugiGroup.transform, "", 20, TextAnchor.MiddleCenter,
                new Vector2(0, -20), new Vector2(500, 40));
            perfectKtSubtext.color = Color.white;

            // --- Reverse All-Out Calling group (DESIGN.md 9-1) ---
            var reverseAllOutGroup = CreateSubCanvasGroup(fxObj.transform, "ReverseAllOutGroup");
            reverseAllOutGroup.alpha = 0;
            reverseAllOutGroup.gameObject.SetActive(false);
            var reverseAllOutText = MakeText(reverseAllOutGroup.transform, "", 30, TextAnchor.MiddleCenter,
                new Vector2(0, 0), new Vector2(500, 50));
            reverseAllOutText.color = new Color(1f, 0.3f, 0.3f);
            reverseAllOutText.fontStyle = FontStyle.Bold;
            // 逆総告白フラッシュ（赤）
            var reverseFlash = MakeFullscreenImage(fxObj.transform, "ReverseFlash", new Color(0.8f, 0.1f, 0.1f, 0));
            reverseFlash.gameObject.SetActive(false);

            _battleEffects = fxObj.AddComponent<BattleEffects>();
            SetPrivateField(_battleEffects, "_screenFlash", screenFlash);
            SetPrivateField(_battleEffects, "_weakBanner", weakBanner);
            SetPrivateField(_battleEffects, "_weakText", weakText);
            SetPrivateField(_battleEffects, "_crackOverlay", crackOverlay);
            SetPrivateField(_battleEffects, "_allOutGroup", allOutGroup);
            SetPrivateField(_battleEffects, "_allOutBackground", allOutBg);
            SetPrivateField(_battleEffects, "_allOutTitle", allOutTitle);
            SetPrivateField(_battleEffects, "_allOutQuote", allOutQuote);
            SetPrivateField(_battleEffects, "_finisherPortrait", finisherPortrait);
            SetPrivateField(_battleEffects, "_kotsugiGroup", kotsugiGroup);
            SetPrivateField(_battleEffects, "_kotsugiText", kotsugiText);
            SetPrivateField(_battleEffects, "_kotsugiBonus", kotsugiBonus);
            SetPrivateField(_battleEffects, "_perfectKotsugiGroup", perfectKotsugiGroup);
            SetPrivateField(_battleEffects, "_perfectKotsugiTitle", perfectKtTitle);
            SetPrivateField(_battleEffects, "_perfectKotsugiSubtext", perfectKtSubtext);
            SetPrivateField(_battleEffects, "_reverseAllOutGroup", reverseAllOutGroup);
            SetPrivateField(_battleEffects, "_reverseAllOutText", reverseAllOutText);
            SetPrivateField(_battleEffects, "_reverseFlash", reverseFlash);
            SetPrivateField(_battleEffects, "_damagePopTemplate", dmgPop);

            // --- Level Up Effect ---
            var lvGroup = CreateFullscreenGroup("LevelUpGroup");
            lvGroup.alpha = 0;
            lvGroup.gameObject.SetActive(false);

            var lvBg = MakeFullscreenImage(lvGroup.transform, "LvBg", new Color(0, 0, 0, 0.85f));

            var lvExpText = MakeText(lvGroup.transform, "", 28, TextAnchor.MiddleCenter,
                new Vector2(0, 40), new Vector2(300, 40));
            lvExpText.color = new Color(1f, 0.9f, 0.3f);
            lvExpText.fontStyle = FontStyle.Bold;

            var lvLevelText = MakeText(lvGroup.transform, "", 36, TextAnchor.MiddleCenter,
                new Vector2(0, -10), new Vector2(200, 50));
            lvLevelText.color = Color.white;

            // EXPバー
            var lvBarBg = MakeImage(lvGroup.transform, "ExpBarBg", new Vector2(0, -55),
                new Vector2(250, 12), new Color(0.2f, 0.2f, 0.25f));
            var lvBarFill = MakeImage(lvBarBg.transform, "ExpBarFill", Vector2.zero,
                new Vector2(250, 12), new Color(1f, 0.243f, 0.541f));
            lvBarFill.type = Image.Type.Filled;
            lvBarFill.fillMethod = Image.FillMethod.Horizontal;
            lvBarFill.fillAmount = 0;
            var lvBarFillRt = lvBarFill.GetComponent<RectTransform>();
            lvBarFillRt.anchorMin = Vector2.zero;
            lvBarFillRt.anchorMax = Vector2.one;
            lvBarFillRt.offsetMin = Vector2.zero;
            lvBarFillRt.offsetMax = Vector2.zero;

            var lvBanner = MakeText(lvGroup.transform, "", 42, TextAnchor.MiddleCenter,
                new Vector2(0, -100), new Vector2(400, 55));
            lvBanner.color = new Color(1f, 0.243f, 0.541f);
            lvBanner.fontStyle = FontStyle.Bold;
            lvBanner.gameObject.SetActive(false);

            var lvFlash = MakeFullscreenImage(lvGroup.transform, "LvFlash", new Color(1f, 0.243f, 0.541f, 0));
            lvFlash.gameObject.SetActive(false);

            _levelUpEffect = lvGroup.gameObject.AddComponent<LevelUpEffect>();
            SetPrivateField(_levelUpEffect, "_group", lvGroup);
            SetPrivateField(_levelUpEffect, "_expGainedText", lvExpText);
            SetPrivateField(_levelUpEffect, "_levelText", lvLevelText);
            SetPrivateField(_levelUpEffect, "_expBar", lvBarFill);
            SetPrivateField(_levelUpEffect, "_levelUpBanner", lvBanner);
            SetPrivateField(_levelUpEffect, "_flashOverlay", lvFlash);

            // Wire effects into BattleController
            SetPrivateField(_battleCtrl, "_effects", _battleEffects);
            SetPrivateField(_battleCtrl, "_levelUpEffect", _levelUpEffect);

            // Wire bond rank up into FieldController
            SetPrivateField(_fieldCtrl, "_bondRankUp", _bondRankUpEffect);
        }

        // =============================================
        //  TITLE
        // =============================================
        private void CreateTitlePanel()
        {
            _titlePanel = MakePanel("TitlePanel", new Color(0.106f, 0.165f, 0.29f));

            var titleText = MakeText(_titlePanel.transform, "残響都市アマネ", 42, TextAnchor.MiddleCenter,
                new Vector2(0, 80), new Vector2(600, 60));
            titleText.color = new Color(1f, 0.243f, 0.541f);

            var subText = MakeText(_titlePanel.transform, "AMANE: City of Echoes", 18, TextAnchor.MiddleCenter,
                new Vector2(0, 30), new Vector2(400, 30));
            subText.color = new Color(0.9f, 0.9f, 0.85f);

            var catchText = MakeText(_titlePanel.transform, "「言えなかった言葉が、世界を喰っていく。」", 14, TextAnchor.MiddleCenter,
                new Vector2(0, -10), new Vector2(500, 25));
            catchText.fontStyle = FontStyle.Italic;
            catchText.color = new Color(0.7f, 0.7f, 0.65f);

            var newGameBtn = MakeButton(_titlePanel.transform, "未読を開く", new Vector2(0, -80), new Vector2(200, 45),
                new Color(1f, 0.243f, 0.541f));
            var continueBtn = MakeButton(_titlePanel.transform, "日記を読む", new Vector2(0, -135), new Vector2(200, 45),
                new Color(0.31f, 0.66f, 1f));
            var quitBtn = MakeButton(_titlePanel.transform, "閉じる", new Vector2(0, -190), new Vector2(200, 45),
                new Color(0.4f, 0.4f, 0.4f));

            _titleCtrl = _titlePanel.AddComponent<TitleScreenController>();
            SetPrivateField(_titleCtrl, "_newGameButton", newGameBtn);
            SetPrivateField(_titleCtrl, "_continueButton", continueBtn);
            SetPrivateField(_titleCtrl, "_quitButton", quitBtn);
        }

        // =============================================
        //  FIELD — 2Dマップ探索
        // =============================================
        private void CreateFieldPanel()
        {
            _fieldPanel = MakePanel("FieldPanel", new Color(0.12f, 0.14f, 0.22f));

            // ---- 2Dマップエリア（中央の大きな探索空間） ----
            var mapArea = MakeSubPanel(_fieldPanel.transform, "MapArea", Vector2.zero,
                new Vector2(800, 500), new Color(0.08f, 0.10f, 0.18f));
            var mapRect = mapArea.GetComponent<RectTransform>();
            mapRect.anchorMin = new Vector2(0.5f, 0.5f);
            mapRect.anchorMax = new Vector2(0.5f, 0.5f);

            // 地面の装飾ライン（道路）
            DrawRoad(mapArea.transform, new Vector2(0, 0), new Vector2(700, 4), new Color(0.2f, 0.22f, 0.3f));
            DrawRoad(mapArea.transform, new Vector2(0, -80), new Vector2(700, 4), new Color(0.2f, 0.22f, 0.3f));
            DrawRoad(mapArea.transform, new Vector2(-100, 60), new Vector2(4, 400), new Color(0.2f, 0.22f, 0.3f));
            DrawRoad(mapArea.transform, new Vector2(200, 60), new Vector2(4, 400), new Color(0.2f, 0.22f, 0.3f));

            // ---- プレイヤーキャラ ----
            var playerObj = new GameObject("Player", typeof(RectTransform));
            playerObj.transform.SetParent(mapArea.transform, false);
            var playerRect = playerObj.GetComponent<RectTransform>();
            playerRect.anchoredPosition = new Vector2(0, -30);
            playerRect.sizeDelta = new Vector2(30, 30);
            var playerImg = playerObj.AddComponent<Image>();
            playerImg.color = new Color(1f, 0.243f, 0.541f); // 蛍光ピンク
            // プレイヤー名
            var playerLabel = MakeText(playerObj.transform, "詠", 12, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(30, 30));
            playerLabel.color = Color.white;
            playerLabel.raycastTarget = false;

            var player = playerObj.AddComponent<FieldPlayer2D>();
            SetPrivateField(player, "_playerRect", playerRect);
            SetPrivateField(player, "_playerImage", playerImg);
            SetPrivateField(player, "_playerLabel", playerLabel);
            SetPrivateField(player, "_fieldBounds", mapRect);

            // ---- ロケーション配置 ----
            var fieldMap = mapArea.AddComponent<FieldMap2D>();
            SetPrivateField(fieldMap, "_mapArea", mapRect);
            SetPrivateField(fieldMap, "_player", player);

            // ロケーション名テキスト（画面下部）
            var locNameText = MakeText(_fieldPanel.transform, "", 18, TextAnchor.MiddleCenter,
                new Vector2(0, -220), new Vector2(400, 30));
            locNameText.color = new Color(1f, 0.243f, 0.541f);
            SetPrivateField(fieldMap, "_locationNameText", locNameText);

            // 各ロケーションを生成・配置
            CreateLocation(fieldMap, mapArea.transform, "school", "雨音西高校", LocationType.Study,
                new Vector2(-250, 150), new Vector2(80, 50), new Color(0.31f, 0.66f, 1f, 0.8f), "校");
            CreateLocation(fieldMap, mapArea.transform, "akari", "灯里", LocationType.NPC,
                new Vector2(-100, 100), new Vector2(28, 28), new Color(1f, 0.8f, 0.3f), "灯");
            CreateLocation(fieldMap, mapArea.transform, "ritsu", "律", LocationType.NPC,
                new Vector2(300, 130), new Vector2(28, 28), new Color(0.4f, 0.5f, 0.8f), "律");
            CreateLocation(fieldMap, mapArea.transform, "ren", "蓮", LocationType.NPC,
                new Vector2(200, -100), new Vector2(28, 28), new Color(0.8f, 0.3f, 0.3f), "蓮");
            CreateLocation(fieldMap, mapArea.transform, "dungeon", "未言界の入口", LocationType.Dungeon,
                new Vector2(0, 180), new Vector2(60, 40), new Color(0.5f, 0.1f, 0.3f, 0.9f), "潜");
            CreateLocation(fieldMap, mapArea.transform, "shop", "古書堂「八雲」", LocationType.Shop,
                new Vector2(-300, -80), new Vector2(60, 40), new Color(0.5f, 0.4f, 0.2f, 0.8f), "書");
            CreateLocation(fieldMap, mapArea.transform, "cafe", "喫茶バイト", LocationType.Job,
                new Vector2(300, -60), new Vector2(60, 40), new Color(0.4f, 0.7f, 0.4f, 0.8f), "喫");
            CreateLocation(fieldMap, mapArea.transform, "shrine", "水神社（瞑想）", LocationType.Meditate,
                new Vector2(-200, -150), new Vector2(50, 40), new Color(0.6f, 0.5f, 0.8f, 0.8f), "社");
            CreateLocation(fieldMap, mapArea.transform, "home", "自宅", LocationType.Home,
                new Vector2(250, 170), new Vector2(50, 40), new Color(0.4f, 0.4f, 0.4f, 0.8f), "家");

            // 操作説明
            var helpText = MakeText(_fieldPanel.transform, "WASD/矢印: 移動　Space: 調べる　Tab: ステータス", 12, TextAnchor.MiddleCenter,
                new Vector2(0, -240), new Vector2(500, 20));
            helpText.color = new Color(0.5f, 0.5f, 0.5f);

            // ---- Calendar UI (top right) ----
            var calPanel = MakeSubPanel(_fieldPanel.transform, "CalendarPanel", new Vector2(300, 210),
                new Vector2(200, 120), new Color(0.106f, 0.165f, 0.29f, 0.9f));
            var calUI = calPanel.AddComponent<CalendarUI>();
            var dateText = MakeText(calPanel.transform, "4月1日", 20, TextAnchor.MiddleCenter,
                new Vector2(0, 35), new Vector2(180, 30));
            dateText.color = Color.white;
            var dowText = MakeText(calPanel.transform, "月", 14, TextAnchor.MiddleCenter,
                new Vector2(0, 12), new Vector2(180, 20));
            dowText.color = new Color(0.8f, 0.8f, 0.8f);
            var weatherText = MakeText(calPanel.transform, "晴", 14, TextAnchor.MiddleCenter,
                new Vector2(0, -5), new Vector2(180, 20));
            weatherText.color = new Color(0.7f, 0.9f, 1f);
            var slotText = MakeText(calPanel.transform, "放課後", 16, TextAnchor.MiddleCenter,
                new Vector2(0, -25), new Vector2(180, 25));
            slotText.color = new Color(1f, 0.243f, 0.541f);
            var apText = MakeText(calPanel.transform, "AP 2/2", 14, TextAnchor.MiddleCenter,
                new Vector2(0, -45), new Vector2(180, 20));
            apText.color = Color.white;
            SetPrivateField(calUI, "_dateText", dateText);
            SetPrivateField(calUI, "_dayOfWeekText", dowText);
            SetPrivateField(calUI, "_weatherText", weatherText);
            SetPrivateField(calUI, "_timeSlotText", slotText);
            SetPrivateField(calUI, "_apText", apText);

            // ---- ActionSelectUI (深夜 + 昼休み 兼用メニュー) ----
            // スロットによって表示ボタンが切り替わる（ActionSelectUI.Show()で制御）
            var actionPanel = MakeSubPanel(_fieldPanel.transform, "ActionPanel", new Vector2(0, -80),
                new Vector2(260, 290), new Color(0.04f, 0.02f, 0.08f, 0.95f));
            actionPanel.SetActive(false);

            // 深夜ラベル
            var lateNightLabel = MakeText(actionPanel.transform, "── 深夜 ──", 15, TextAnchor.MiddleCenter,
                new Vector2(0, 126), new Vector2(240, 22));
            lateNightLabel.color = new Color(0.5f, 0.4f, 0.8f);

            var sleepBtn = MakeButton(actionPanel.transform, "眠る（翌朝へ）",
                new Vector2(0, 95), new Vector2(220, 38),
                new Color(0.106f, 0.165f, 0.29f));

            var midnightDiveBtn = MakeButton(actionPanel.transform, "強行潜行（翌日AP消滅）",
                new Vector2(0, 50), new Vector2(220, 38),
                new Color(0.25f, 0.04f, 0.12f));
            var midnightBtnText = midnightDiveBtn.GetComponentInChildren<Text>();
            if (midnightBtnText != null) midnightBtnText.color = new Color(1f, 0.6f, 0.6f);

            // 昼休みラベル
            var lunchLabel = MakeText(actionPanel.transform, "── 昼休み ──", 15, TextAnchor.MiddleCenter,
                new Vector2(0, 5), new Vector2(240, 22));
            lunchLabel.color = new Color(1f, 0.85f, 0.4f);

            var lunchChatBtn = MakeButton(actionPanel.transform, "キャラと話す",
                new Vector2(0, -28), new Vector2(220, 36),
                new Color(0.106f, 0.165f, 0.29f));

            var lunchLibBtn = MakeButton(actionPanel.transform, "図書室（知性+3）",
                new Vector2(0, -70), new Vector2(220, 36),
                new Color(0.106f, 0.165f, 0.29f));

            var lunchCanteenBtn = MakeButton(actionPanel.transform, "購買（アイテム入手・度胸+1）",
                new Vector2(0, -112), new Vector2(220, 36),
                new Color(0.106f, 0.165f, 0.29f));

            var lunchRooftopBtn = MakeButton(actionPanel.transform, "屋上（静けさ+5）",
                new Vector2(0, -154), new Vector2(220, 36),
                new Color(0.04f, 0.12f, 0.20f));
            var rooftopText = lunchRooftopBtn.GetComponentInChildren<Text>();
            if (rooftopText != null) rooftopText.color = new Color(0.7f, 0.95f, 1f);

            var lunchSkipBtn = MakeButton(actionPanel.transform, "スキップ（放課後へ）",
                new Vector2(0, -196), new Vector2(220, 36),
                new Color(0.08f, 0.08f, 0.08f));

            var actionUI = actionPanel.AddComponent<ActionSelectUI>();
            SetPrivateField(actionUI, "_panel", actionPanel);
            SetPrivateField(actionUI, "_goHomeButton", sleepBtn);
            SetPrivateField(actionUI, "_midnightDiveButton", midnightDiveBtn);
            SetPrivateField(actionUI, "_lunchChatButton", lunchChatBtn);
            SetPrivateField(actionUI, "_lunchLibraryButton", lunchLibBtn);
            SetPrivateField(actionUI, "_lunchCanteenButton", lunchCanteenBtn);
            SetPrivateField(actionUI, "_lunchRooftopButton", lunchRooftopBtn);
            SetPrivateField(actionUI, "_lunchSkipButton", lunchSkipBtn);

            // ---- Dialogue UI ----
            var dialoguePanel = MakeSubPanel(_fieldPanel.transform, "DialoguePanel", new Vector2(0, -160),
                new Vector2(750, 150), new Color(0f, 0f, 0f, 0.9f));
            dialoguePanel.SetActive(false);
            var speakerText = MakeText(dialoguePanel.transform, "", 18, TextAnchor.MiddleLeft,
                new Vector2(-280, 45), new Vector2(200, 30));
            speakerText.color = new Color(1f, 0.243f, 0.541f);
            var bodyText = MakeText(dialoguePanel.transform, "", 16, TextAnchor.UpperLeft,
                new Vector2(0, 10), new Vector2(700, 70));
            bodyText.color = Color.white;
            var typewriter = bodyText.gameObject.AddComponent<TypewriterText>();
            SetPrivateField(typewriter, "_text", bodyText);

            var choicePanel = MakeSubPanel(dialoguePanel.transform, "ChoicePanel", new Vector2(0, -55),
                new Vector2(700, 40), new Color(0, 0, 0, 0));
            choicePanel.SetActive(false);
            var choiceButtons = new Button[3];
            var choiceTexts = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                choiceButtons[i] = MakeButton(choicePanel.transform, $"選択肢{i + 1}",
                    new Vector2(-230 + i * 230, 0), new Vector2(220, 36),
                    new Color(0.106f, 0.165f, 0.29f));
                choiceTexts[i] = choiceButtons[i].GetComponentInChildren<Text>();
            }
            var dialogueUI = dialoguePanel.AddComponent<DialogueUI>();
            SetPrivateField(dialogueUI, "_panel", dialoguePanel);
            SetPrivateField(dialogueUI, "_speakerText", speakerText);
            SetPrivateField(dialogueUI, "_bodyText", bodyText);
            SetPrivateField(dialogueUI, "_choicePanel", choicePanel);
            SetPrivateField(dialogueUI, "_choiceButtons", choiceButtons);
            SetPrivateField(dialogueUI, "_choiceTexts", choiceTexts);
            SetPrivateField(dialogueUI, "_typewriter", typewriter);

            // ---- Deadline UI ----
            var deadlinePanel = MakeSubPanel(_fieldPanel.transform, "DeadlinePanel", new Vector2(0, 210),
                new Vector2(400, 35), new Color(0.9f, 0.1f, 0.1f, 0.9f));
            deadlinePanel.SetActive(false);
            var deadlineText = MakeText(deadlinePanel.transform, "", 14, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(380, 30));
            deadlineText.color = Color.white;
            var deadlineUI = _fieldPanel.AddComponent<DeadlineUI>();
            SetPrivateField(deadlineUI, "_warningPanel", deadlinePanel);
            SetPrivateField(deadlineUI, "_warningText", deadlineText);

            // ---- Status UI ----
            var statusPanel = MakeSubPanel(_fieldPanel.transform, "StatusPanel", new Vector2(-300, 0),
                new Vector2(220, 300), new Color(0.106f, 0.165f, 0.29f, 0.95f));
            statusPanel.SetActive(false);
            var statTitle = MakeText(statusPanel.transform, "── ステータス ──", 16, TextAnchor.MiddleCenter,
                new Vector2(0, 120), new Vector2(200, 25));
            statTitle.color = new Color(1f, 0.243f, 0.541f);
            var courageText = MakeText(statusPanel.transform, "度胸: —", 13, TextAnchor.MiddleLeft,
                new Vector2(-40, 90), new Vector2(180, 20));
            courageText.color = Color.white;
            var intellectText = MakeText(statusPanel.transform, "知性: —", 13, TextAnchor.MiddleLeft,
                new Vector2(-40, 68), new Vector2(180, 20));
            intellectText.color = Color.white;
            var empathyText = MakeText(statusPanel.transform, "慈しみ: —", 13, TextAnchor.MiddleLeft,
                new Vector2(-40, 46), new Vector2(180, 20));
            empathyText.color = Color.white;
            var expressionText = MakeText(statusPanel.transform, "ことのは: —", 13, TextAnchor.MiddleLeft,
                new Vector2(-40, 24), new Vector2(180, 20));
            expressionText.color = Color.white;
            var composureText = MakeText(statusPanel.transform, "静けさ: —", 13, TextAnchor.MiddleLeft,
                new Vector2(-40, 2), new Vector2(180, 20));
            composureText.color = Color.white;
            var bondSeparator = MakeText(statusPanel.transform, "── 言伝 ──", 14, TextAnchor.MiddleCenter,
                new Vector2(0, -25), new Vector2(200, 22));
            bondSeparator.color = new Color(1f, 0.243f, 0.541f);
            var bondsText = MakeText(statusPanel.transform, "", 11, TextAnchor.UpperLeft,
                new Vector2(-40, -90), new Vector2(180, 110));
            bondsText.color = Color.white;
            var statusUI = statusPanel.AddComponent<StatusUI>();
            SetPrivateField(statusUI, "_courageText", courageText);
            SetPrivateField(statusUI, "_intellectText", intellectText);
            SetPrivateField(statusUI, "_empathyText", empathyText);
            SetPrivateField(statusUI, "_expressionText", expressionText);
            SetPrivateField(statusUI, "_composureText", composureText);
            SetPrivateField(statusUI, "_bondsText", bondsText);

            // ---- FusionSelectUI（語り手融合: 古書店「言ノ葉」）----
            var fusionPanel = MakeSubPanel(_fieldPanel.transform, "FusionPanel", new Vector2(0, 0),
                new Vector2(380, 440), new Color(0.06f, 0.04f, 0.12f, 0.97f));
            fusionPanel.SetActive(false);

            // タイトル
            var fusionTitle = MakeText(fusionPanel.transform, "── 語り手融合 ──", 20, TextAnchor.MiddleCenter,
                new Vector2(0, 195), new Vector2(340, 28));
            fusionTitle.color = new Color(1f, 0.243f, 0.541f);

            // ステップラベル
            var fusionStepLabel = MakeText(fusionPanel.transform, "", 14, TextAnchor.MiddleCenter,
                new Vector2(0, 165), new Vector2(340, 22));
            fusionStepLabel.color = new Color(0.9f, 0.9f, 0.85f);

            // 費用テキスト
            var fusionCostText = MakeText(fusionPanel.transform, "", 13, TextAnchor.MiddleCenter,
                new Vector2(0, 142), new Vector2(340, 20));
            fusionCostText.color = new Color(1f, 0.85f, 0.3f);

            // 語り手ボタン（最大6）
            var fusionNarratorBtns = new Button[6];
            var fusionNarratorLabels = new Text[6];
            for (int i = 0; i < 6; i++)
            {
                float y = 100 - i * 44;
                fusionNarratorBtns[i] = MakeButton(fusionPanel.transform, "",
                    new Vector2(0, y), new Vector2(340, 38),
                    new Color(0.12f, 0.08f, 0.22f));
                fusionNarratorLabels[i] = fusionNarratorBtns[i].GetComponentInChildren<Text>();
                if (fusionNarratorLabels[i] != null)
                    fusionNarratorLabels[i].color = new Color(0.9f, 0.85f, 1f);
            }

            // プレビューテキスト（確認ステップ）
            var fusionPreviewText = MakeText(fusionPanel.transform, "", 14, TextAnchor.MiddleCenter,
                new Vector2(0, -130), new Vector2(340, 50));
            fusionPreviewText.color = Color.white;
            fusionPreviewText.gameObject.SetActive(false);

            // 確認ボタン
            var fusionConfirmBtn = MakeButton(fusionPanel.transform, "融合する",
                new Vector2(-70, -185), new Vector2(140, 40),
                new Color(0.5f, 0.1f, 0.3f));
            fusionConfirmBtn.GetComponentInChildren<Text>().color = new Color(1f, 0.6f, 0.8f);
            fusionConfirmBtn.gameObject.SetActive(false);

            // キャンセルボタン
            var fusionCancelBtn = MakeButton(fusionPanel.transform, "戻る",
                new Vector2(70, -185), new Vector2(140, 40),
                new Color(0.25f, 0.25f, 0.3f));

            var fusionUI = fusionPanel.AddComponent<FusionSelectUI>();
            SetPrivateField(fusionUI, "_panel", fusionPanel);
            SetPrivateField(fusionUI, "_stepLabel", fusionStepLabel);
            SetPrivateField(fusionUI, "_previewText", fusionPreviewText);
            SetPrivateField(fusionUI, "_costText", fusionCostText);
            SetPrivateField(fusionUI, "_narratorButtons", fusionNarratorBtns);
            SetPrivateField(fusionUI, "_narratorLabels", fusionNarratorLabels);
            SetPrivateField(fusionUI, "_confirmButton", fusionConfirmBtn);
            SetPrivateField(fusionUI, "_cancelButton", fusionCancelBtn);

            // ---- FieldController ----
            _fieldCtrl = _fieldPanel.AddComponent<FieldController>();
            SetPrivateField(_fieldCtrl, "_calendar", calUI);
            SetPrivateField(_fieldCtrl, "_actionSelect", actionUI);
            SetPrivateField(_fieldCtrl, "_dialogueUI", dialogueUI);
            SetPrivateField(_fieldCtrl, "_statusUI", statusUI);
            SetPrivateField(_fieldCtrl, "_fieldMap", fieldMap);
            SetPrivateField(_fieldCtrl, "_fusionSelectUI", fusionUI);
        }

        private void CreateLocation(FieldMap2D map, Transform parent, string id, string name,
            LocationType type, Vector2 pos, Vector2 size, Color color, string icon)
        {
            var obj = MakeSubPanel(parent, $"Loc_{id}", pos, size, color);
            var iconText = MakeText(obj.transform, icon, (int)(size.y * 0.5f), TextAnchor.MiddleCenter,
                Vector2.zero, size);
            iconText.color = Color.white;
            iconText.raycastTarget = false;

            var nameText = MakeText(obj.transform, name, 10, TextAnchor.MiddleCenter,
                new Vector2(0, -size.y / 2 - 10), new Vector2(100, 16));
            nameText.color = new Color(0.7f, 0.7f, 0.7f);
            nameText.raycastTarget = false;

            var promptText = MakeText(obj.transform, "", 11, TextAnchor.MiddleCenter,
                new Vector2(0, size.y / 2 + 12), new Vector2(120, 18));
            promptText.color = new Color(1f, 0.9f, 0.3f);
            promptText.gameObject.SetActive(false);
            promptText.raycastTarget = false;

            var loc = obj.AddComponent<FieldLocation>();
            loc.Id = id;
            loc.DisplayName = name;
            loc.Type = type;
            SetPrivateField(loc, "_rect", obj.GetComponent<RectTransform>());
            SetPrivateField(loc, "_icon", obj.GetComponent<Image>());
            SetPrivateField(loc, "_label", iconText);
            SetPrivateField(loc, "_promptText", promptText);

            map.RegisterLocation(loc);
        }

        private void DrawRoad(Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            var road = new GameObject("Road", typeof(RectTransform));
            road.transform.SetParent(parent, false);
            var rt = road.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = road.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }

        // =============================================
        //  BATTLE
        // =============================================
        private void CreateBattlePanel()
        {
            _battlePanel = MakePanel("BattlePanel", new Color(0.08f, 0.06f, 0.12f));

            // Party status (left side)
            var partyNames = new Text[4];
            var partyHp = new Text[4];
            var partySp = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                float y = 160 - i * 55;
                var bg = MakeSubPanel(_battlePanel.transform, $"PartySlot{i}", new Vector2(-260, y),
                    new Vector2(220, 50), new Color(0.106f, 0.165f, 0.29f, 0.8f));
                partyNames[i] = MakeText(bg.transform, "", 14, TextAnchor.MiddleLeft,
                    new Vector2(-40, 10), new Vector2(150, 20));
                partyNames[i].color = Color.white;
                partyHp[i] = MakeText(bg.transform, "", 12, TextAnchor.MiddleLeft,
                    new Vector2(-40, -8), new Vector2(100, 18));
                partyHp[i].color = new Color(0.4f, 1f, 0.4f);
                partySp[i] = MakeText(bg.transform, "", 12, TextAnchor.MiddleRight,
                    new Vector2(40, -8), new Vector2(100, 18));
                partySp[i].color = new Color(0.31f, 0.66f, 1f);
            }

            // Enemy status (right side)
            var enemyNames = new Text[4];
            var enemyHp = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                float y = 160 - i * 55;
                var bg = MakeSubPanel(_battlePanel.transform, $"EnemySlot{i}", new Vector2(260, y),
                    new Vector2(220, 50), new Color(0.3f, 0.05f, 0.05f, 0.8f));
                enemyNames[i] = MakeText(bg.transform, "", 14, TextAnchor.MiddleLeft,
                    new Vector2(-40, 10), new Vector2(150, 20));
                enemyNames[i].color = Color.white;
                enemyHp[i] = MakeText(bg.transform, "", 12, TextAnchor.MiddleLeft,
                    new Vector2(-40, -8), new Vector2(180, 18));
                enemyHp[i].color = new Color(1f, 0.4f, 0.4f);
            }

            // Command panel (bottom)
            var cmdPanel = MakeSubPanel(_battlePanel.transform, "CmdPanel", new Vector2(0, -160),
                new Vector2(700, 60), new Color(0, 0, 0, 0.7f));
            var attackBtn = MakeButton(cmdPanel.transform, "攻撃", new Vector2(-290, 0), new Vector2(90, 45),
                new Color(0.8f, 0.3f, 0.3f));
            var skillBtn = MakeButton(cmdPanel.transform, "スキル", new Vector2(-190, 0), new Vector2(90, 45),
                new Color(0.31f, 0.66f, 1f));
            var guardBtn = MakeButton(cmdPanel.transform, "防御", new Vector2(-90, 0), new Vector2(90, 45),
                new Color(0.5f, 0.5f, 0.5f));
            var kotsugiBtn = MakeButton(cmdPanel.transform, "言継ぎ", new Vector2(10, 0), new Vector2(90, 45),
                new Color(1f, 0.7f, 0.2f));
            var allOutBtn = MakeButton(cmdPanel.transform, "総告白", new Vector2(110, 0), new Vector2(90, 45),
                new Color(1f, 0.243f, 0.541f));
            var fleeBtn = MakeButton(cmdPanel.transform, "逃走", new Vector2(210, 0), new Vector2(90, 45),
                new Color(0.4f, 0.4f, 0.4f));

            // Battle log
            var logText = MakeText(_battlePanel.transform, "", 13, TextAnchor.UpperLeft,
                new Vector2(0, -60), new Vector2(680, 130));
            logText.color = new Color(0.9f, 0.9f, 0.85f);

            // BattleHUD
            var hud = _battlePanel.AddComponent<BattleHUD>();
            SetPrivateField(hud, "_partyNames", partyNames);
            SetPrivateField(hud, "_partyHpTexts", partyHp);
            SetPrivateField(hud, "_partySpTexts", partySp);
            SetPrivateField(hud, "_enemyNames", enemyNames);
            SetPrivateField(hud, "_enemyHpTexts", enemyHp);
            SetPrivateField(hud, "_commandPanel", cmdPanel);
            SetPrivateField(hud, "_attackButton", attackBtn);
            SetPrivateField(hud, "_skillButton", skillBtn);
            SetPrivateField(hud, "_guardButton", guardBtn);
            SetPrivateField(hud, "_kotsugiButton", kotsugiBtn);
            SetPrivateField(hud, "_allOutButton", allOutBtn);
            SetPrivateField(hud, "_fleeButton", fleeBtn);
            SetPrivateField(hud, "_logText", logText);

            // Skill select
            var skillPanel = MakeSubPanel(_battlePanel.transform, "SkillPanel", new Vector2(0, 0),
                new Vector2(350, 260), new Color(0.106f, 0.165f, 0.29f, 0.95f));
            skillPanel.SetActive(false);
            var skillTitle = MakeText(skillPanel.transform, "スキル", 20, TextAnchor.MiddleCenter,
                new Vector2(0, 100), new Vector2(300, 30));
            skillTitle.color = Color.white;
            var skillButtons = new Button[6];
            var skillLabels = new Text[6];
            var spCostLabels = new Text[6];
            for (int i = 0; i < 6; i++)
            {
                skillButtons[i] = MakeButton(skillPanel.transform, "", new Vector2(0, 60 - i * 35),
                    new Vector2(300, 30), new Color(0.2f, 0.2f, 0.35f));
                skillLabels[i] = skillButtons[i].GetComponentInChildren<Text>();
                spCostLabels[i] = MakeText(skillButtons[i].transform, "", 11, TextAnchor.MiddleRight,
                    new Vector2(100, 0), new Vector2(80, 25));
                spCostLabels[i].color = new Color(0.31f, 0.66f, 1f);
            }
            var skillBack = MakeButton(skillPanel.transform, "戻る", new Vector2(0, -110),
                new Vector2(100, 30), new Color(0.4f, 0.4f, 0.4f));

            var skillUI = skillPanel.AddComponent<SkillSelectUI>();
            SetPrivateField(skillUI, "_panel", skillPanel);
            SetPrivateField(skillUI, "_skillButtons", skillButtons);
            SetPrivateField(skillUI, "_skillLabels", skillLabels);
            SetPrivateField(skillUI, "_spCostLabels", spCostLabels);
            SetPrivateField(skillUI, "_backButton", skillBack);

            // Target select
            var targetPanel = MakeSubPanel(_battlePanel.transform, "TargetPanel", new Vector2(0, 0),
                new Vector2(300, 200), new Color(0.3f, 0.05f, 0.05f, 0.95f));
            targetPanel.SetActive(false);
            var targetTitle = MakeText(targetPanel.transform, "対象選択", 18, TextAnchor.MiddleCenter,
                new Vector2(0, 75), new Vector2(260, 25));
            targetTitle.color = Color.white;
            var targetButtons = new Button[4];
            var targetLabels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                targetButtons[i] = MakeButton(targetPanel.transform, "", new Vector2(0, 40 - i * 35),
                    new Vector2(260, 30), new Color(0.4f, 0.1f, 0.1f));
                targetLabels[i] = targetButtons[i].GetComponentInChildren<Text>();
            }
            var targetBack = MakeButton(targetPanel.transform, "戻る", new Vector2(0, -75),
                new Vector2(100, 30), new Color(0.4f, 0.4f, 0.4f));

            var targetUI = targetPanel.AddComponent<TargetSelectUI>();
            SetPrivateField(targetUI, "_panel", targetPanel);
            SetPrivateField(targetUI, "_targetButtons", targetButtons);
            SetPrivateField(targetUI, "_targetLabels", targetLabels);
            SetPrivateField(targetUI, "_backButton", targetBack);

            // Kotsugi select
            var kotsugiPanel = MakeSubPanel(_battlePanel.transform, "KotsugiPanel", new Vector2(0, 0),
                new Vector2(300, 180), new Color(0.3f, 0.25f, 0.05f, 0.95f));
            kotsugiPanel.SetActive(false);
            var kotsugiTitle = MakeText(kotsugiPanel.transform, "言継ぎ先", 18, TextAnchor.MiddleCenter,
                new Vector2(0, 65), new Vector2(260, 25));
            kotsugiTitle.color = Color.white;
            var kotsugiButtons = new Button[3];
            var kotsugiLabels = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                kotsugiButtons[i] = MakeButton(kotsugiPanel.transform, "", new Vector2(0, 30 - i * 35),
                    new Vector2(260, 30), new Color(0.4f, 0.35f, 0.1f));
                kotsugiLabels[i] = kotsugiButtons[i].GetComponentInChildren<Text>();
            }
            var kotsugiBack = MakeButton(kotsugiPanel.transform, "戻る", new Vector2(0, -65),
                new Vector2(100, 30), new Color(0.4f, 0.4f, 0.4f));

            var kotsugiUI = kotsugiPanel.AddComponent<KotsugiSelectUI>();
            SetPrivateField(kotsugiUI, "_panel", kotsugiPanel);
            SetPrivateField(kotsugiUI, "_memberButtons", kotsugiButtons);
            SetPrivateField(kotsugiUI, "_memberLabels", kotsugiLabels);
            SetPrivateField(kotsugiUI, "_backButton", kotsugiBack);

            // Battle result
            var resultPanel = MakeSubPanel(_battlePanel.transform, "ResultPanel", new Vector2(0, 0),
                new Vector2(400, 200), new Color(0, 0, 0, 0.9f));
            resultPanel.SetActive(false);
            var resultTitle = MakeText(resultPanel.transform, "", 24, TextAnchor.MiddleCenter,
                new Vector2(0, 50), new Vector2(360, 40));
            resultTitle.color = Color.white;
            var resultDetail = MakeText(resultPanel.transform, "", 16, TextAnchor.MiddleCenter,
                new Vector2(0, 0), new Vector2(360, 60));
            resultDetail.color = new Color(0.8f, 0.8f, 0.8f);
            var continueBtn = MakeButton(resultPanel.transform, "続ける", new Vector2(0, -60),
                new Vector2(150, 40), new Color(0.106f, 0.165f, 0.29f));

            var resultUI = resultPanel.AddComponent<BattleResultUI>();
            SetPrivateField(resultUI, "_panel", resultPanel);
            SetPrivateField(resultUI, "_resultTitle", resultTitle);
            SetPrivateField(resultUI, "_resultDetail", resultDetail);
            SetPrivateField(resultUI, "_continueButton", continueBtn);

            // Action Command（タイミングゲージ）
            var acGroup = CreateSubCanvasGroup(_battlePanel.transform, "ActionCommandGroup");
            acGroup.alpha = 0;
            acGroup.gameObject.SetActive(false);
            var acBar = MakeSubPanel(acGroup.transform, "ACBar", new Vector2(0, 80),
                new Vector2(300, 30), new Color(0.15f, 0.15f, 0.2f));
            var acBarRect = acBar.GetComponent<RectTransform>();
            var acFill = MakeImage(acBar.transform, "ACFill", Vector2.zero, new Vector2(300, 30),
                new Color(0.2f, 0.25f, 0.35f));
            // Excellentゾーン（中央の狭い範囲）
            var acExcellent = MakeImage(acBar.transform, "Excellent", new Vector2(300 * (0.475f - 0.5f), 0),
                new Vector2(300 * 0.15f, 30), new Color(1f, 0.243f, 0.541f, 0.6f));
            // Goodゾーン
            var acGood = MakeImage(acBar.transform, "Good", new Vector2(300 * (0.48f - 0.5f), 0),
                new Vector2(300 * 0.4f, 30), new Color(1f, 0.7f, 0.2f, 0.3f));
            acGood.transform.SetAsFirstSibling(); // Goodを後ろに
            // カーソル
            var acCursor = MakeImage(acBar.transform, "Cursor", new Vector2(-150, 0),
                new Vector2(6, 36), Color.white);
            var acResult = MakeText(acGroup.transform, "", 28, TextAnchor.MiddleCenter,
                new Vector2(0, 120), new Vector2(300, 40));
            acResult.fontStyle = FontStyle.Bold;
            var acInstruction = MakeText(acGroup.transform, "Spaceでタイミング！", 14, TextAnchor.MiddleCenter,
                new Vector2(0, 55), new Vector2(300, 20));
            acInstruction.color = new Color(0.7f, 0.7f, 0.7f);

            var actionCmd = acGroup.gameObject.AddComponent<ActionCommand>();
            SetPrivateField(actionCmd, "_group", acGroup);
            SetPrivateField(actionCmd, "_gaugeBar", acBarRect);
            SetPrivateField(actionCmd, "_gaugeFill", acFill);
            SetPrivateField(actionCmd, "_cursor", acCursor);
            SetPrivateField(actionCmd, "_excellentZone", acExcellent);
            SetPrivateField(actionCmd, "_goodZone", acGood);
            SetPrivateField(actionCmd, "_resultText", acResult);

            // BattleController
            _battleCtrl = _battlePanel.AddComponent<BattleController>();
            SetPrivateField(_battleCtrl, "_hud", hud);
            SetPrivateField(_battleCtrl, "_skillSelect", skillUI);
            SetPrivateField(_battleCtrl, "_targetSelect", targetUI);
            SetPrivateField(_battleCtrl, "_kotsugiSelect", kotsugiUI);
            SetPrivateField(_battleCtrl, "_resultUI", resultUI);
            SetPrivateField(_battleCtrl, "_actionCommand", actionCmd);
        }

        // =============================================
        //  DUNGEON EXPLORATION
        // =============================================
        private void CreateDungeonPanel()
        {
            // 未言界の群青背景
            _dungeonPanel = MakePanel("DungeonPanel", new Color(0.06f, 0.08f, 0.14f));

            // フロア名（上部中央）
            var floorText = MakeText(_dungeonPanel.transform, "未言界 B1F", 22, TextAnchor.MiddleCenter,
                new Vector2(0, 200), new Vector2(500, 35));
            floorText.color = new Color(1f, 0.243f, 0.541f);
            floorText.fontStyle = FontStyle.Bold;

            // 部屋の説明（中央上）
            var descBg = MakeSubPanel(_dungeonPanel.transform, "DescBg", new Vector2(0, 80),
                new Vector2(650, 100), new Color(0.08f, 0.10f, 0.18f, 0.9f));
            var roomDescText = MakeText(descBg.transform, "……残響が漂う。", 15, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(620, 90));
            roomDescText.color = new Color(0.88f, 0.88f, 0.88f);

            // メッセージ（中央）
            var msgBg = MakeSubPanel(_dungeonPanel.transform, "MsgBg", new Vector2(0, -30),
                new Vector2(650, 55), new Color(0, 0, 0, 0.7f));
            var messageText = MakeText(msgBg.transform, "", 14, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(620, 48));
            messageText.color = new Color(1f, 0.9f, 0.3f);

            // 移動ボタン（下部・最大4つ）
            var moveButtons = new Button[4];
            var moveLabels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                float x = -220 + i * 150f;
                moveButtons[i] = MakeButton(_dungeonPanel.transform, $"→ 部屋{i + 1}",
                    new Vector2(x, -130), new Vector2(135, 45),
                    new Color(0.15f, 0.20f, 0.35f));
                moveLabels[i] = moveButtons[i].GetComponentInChildren<Text>();
                moveLabels[i].fontSize = 12;
                moveButtons[i].gameObject.SetActive(false);
            }

            // 階段ボタン（特殊）
            var stairsBtn = MakeButton(_dungeonPanel.transform, "▼ 次の層へ",
                new Vector2(0, -180), new Vector2(180, 42),
                new Color(0.4f, 0.15f, 0.35f));
            stairsBtn.GetComponentInChildren<Text>().color = new Color(1f, 0.243f, 0.541f);
            stairsBtn.gameObject.SetActive(false);

            // 撤退ボタン（右下）
            var retreatBtn = MakeButton(_dungeonPanel.transform, "撤退する",
                new Vector2(270, -210), new Vector2(130, 38),
                new Color(0.35f, 0.15f, 0.15f));
            retreatBtn.GetComponentInChildren<Text>().fontSize = 13;

            // 操作説明
            var helpText = MakeText(_dungeonPanel.transform, "ボタンで部屋へ移動　撤退でフィールドに戻る", 11,
                TextAnchor.MiddleCenter, new Vector2(0, -240), new Vector2(500, 20));
            helpText.color = new Color(0.4f, 0.4f, 0.4f);

            // DungeonController
            _dungeonCtrl = _dungeonPanel.AddComponent<DungeonController>();
            SetPrivateField(_dungeonCtrl, "_floorText", floorText);
            SetPrivateField(_dungeonCtrl, "_roomDescText", roomDescText);
            SetPrivateField(_dungeonCtrl, "_messageText", messageText);
            SetPrivateField(_dungeonCtrl, "_moveButtons", moveButtons);
            SetPrivateField(_dungeonCtrl, "_moveLabels", moveLabels);
            SetPrivateField(_dungeonCtrl, "_stairsButton", stairsBtn);
            SetPrivateField(_dungeonCtrl, "_retreatButton", retreatBtn);

            _dungeonPanel.SetActive(false);
        }

        private void CreateDungeonSystems()
        {
            // MigenkaiManager（DontDestroyOnLoad）
            if (MigenkaiManager.Instance == null)
            {
                var mgr = new GameObject("MigenkaiManager");
                mgr.AddComponent<MigenkaiManager>();
            }

            // DungeonExplorer（ローカル）
            if (DungeonExplorer.Instance == null)
            {
                var exp = new GameObject("DungeonExplorer");
                exp.AddComponent<DungeonExplorer>();
            }
        }

        // =============================================
        //  AUDIO
        // =============================================
        private void CreateAudioManager()
        {
            var audioObj = new GameObject("AudioManager");
            _audioManager = audioObj.AddComponent<AudioManager>();
            // 初期BGM: タイトル
            _audioManager.PlayBGM(BGMStyle.Title);
        }

        // =============================================
        //  3D FIELD
        // =============================================
        private void Create3DField()
        {
            var result = Field3DBuilder.Build();
            _field3DRoot = result.Root;

            // FieldManager3D をルートに配置
            var mgrObj = new GameObject("FieldManager3D");
            mgrObj.transform.SetParent(_field3DRoot.transform);
            _fieldManager3D = mgrObj.AddComponent<FieldManager3D>();

            // プロンプト・ロケーション名テキスト用のOverlay Canvas（3Dの上に表示）
            var overlayCanvasObj = new GameObject("Field3D_OverlayCanvas");
            overlayCanvasObj.transform.SetParent(_field3DRoot.transform);
            var overlayCanvas = overlayCanvasObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = -1; // メインUIの下
            overlayCanvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // プロンプトテキスト（画面下部）
            var promptText = MakeText(overlayCanvasObj.transform, "", 18, TextAnchor.MiddleCenter,
                new Vector2(0, -200), new Vector2(500, 35));
            promptText.color = new Color(1f, 0.9f, 0.3f);
            promptText.gameObject.SetActive(false);

            // ロケーション名テキスト（画面上部左）
            var locText = MakeText(overlayCanvasObj.transform, "雨音市", 16, TextAnchor.MiddleLeft,
                new Vector2(-300, 230), new Vector2(300, 25));
            locText.color = new Color(0.8f, 0.8f, 0.8f);

            // FieldManager3Dにプレイヤー・テキストを設定
            SetPrivateField(_fieldManager3D, "_player", result.Player);
            SetPrivateField(_fieldManager3D, "_promptText", promptText);
            SetPrivateField(_fieldManager3D, "_locationText", locText);

            // NPC・ロケーションを登録
            foreach (var npc in result.NPCs)
                _fieldManager3D.RegisterNPC(npc);
            foreach (var loc in result.Locations)
                _fieldManager3D.RegisterLocation(loc);

            // FieldControllerに3Dマネージャーを接続
            SetPrivateField(_fieldCtrl, "_fieldManager3D", _fieldManager3D);

            // フィールドパネルの背景を透明にして3Dが見えるようにする
            var fieldPanelImg = _fieldPanel.GetComponent<Image>();
            if (fieldPanelImg != null)
                fieldPanelImg.color = new Color(0, 0, 0, 0);

            // 2Dマップエリアを非表示（3Dで代替）
            var mapArea = _fieldPanel.transform.Find("MapArea");
            if (mapArea != null)
                mapArea.gameObject.SetActive(false);

            // 操作説明テキストを非表示（2D用の説明）
            // → 3D用の操作説明はオーバーレイに表示
            var helpText3D = MakeText(overlayCanvasObj.transform, "WASD: 移動　Space: 調べる　Tab: ステータス", 12,
                TextAnchor.MiddleCenter, new Vector2(0, -235), new Vector2(500, 20));
            helpText3D.color = new Color(0.5f, 0.5f, 0.5f);

            // 3Dカメラの参照を保存（状態切替時のON/OFF用）
            // Field3DBuilder内でMainCameraを無効化済み

            // 環境演出（雨・ライティング・ネームプレート）
            var envObj = new GameObject("FieldEnvironment");
            envObj.transform.SetParent(_field3DRoot.transform);
            _fieldEnvironment = envObj.AddComponent<FieldEnvironment>();
            _fieldEnvironment.Initialize(_field3DRoot.transform);

            // シーン既存のメインカメラを保存（タイトル/バトル用）
            // Field3DBuilderが新カメラを作る前にCamera.mainを取得済みのはずだが、
            // タグ競合の可能性があるのでFollowCamera以外を探す
            foreach (var cam in Camera.allCameras)
            {
                if (cam.gameObject.name != "FollowCamera")
                {
                    _mainCam = cam;
                    _mainCam.clearFlags = CameraClearFlags.SolidColor;
                    _mainCam.backgroundColor = new Color(0.08f, 0.1f, 0.16f);
                    break;
                }
            }
            // メインカメラが無い場合は作成（UIパネル背景があるので映す必要がある）
            if (_mainCam == null)
            {
                var camObj = new GameObject("UICamera");
                _mainCam = camObj.AddComponent<Camera>();
                _mainCam.clearFlags = CameraClearFlags.SolidColor;
                _mainCam.backgroundColor = new Color(0.08f, 0.1f, 0.16f);
                _mainCam.cullingMask = 0; // 何も描画しない（UIはOverlayで表示）
            }

            // 初期状態では非表示（タイトル画面から始まる）
            _field3DRoot.SetActive(false);

            Debug.Log("[Bootstrap] 3Dフィールド構築完了");
        }

        // =============================================
        //  UI HELPERS
        // =============================================
        private GameObject MakePanel(string name, Color bgColor)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(_canvas.transform, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = panel.AddComponent<Image>();
            img.color = bgColor;
            return panel;
        }

        private GameObject MakeSubPanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = panel.AddComponent<Image>();
            img.color = color;
            return panel;
        }

        private Text MakeText(Transform parent, string text, int fontSize, TextAnchor alignment,
                              Vector2 pos, Vector2 size)
        {
            var obj = new GameObject("Text", typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var t = obj.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                  ?? Resources.GetBuiltinResource<Font>("Arial.ttf")
                  ?? Font.CreateDynamicFontFromOSFont("Arial", 14);
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            return t;
        }

        private Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, Color color)
        {
            var obj = new GameObject("Button", typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = obj.AddComponent<Image>();
            img.color = color;

            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.highlightedColor = new Color(
                Mathf.Min(1, color.r + 0.15f),
                Mathf.Min(1, color.g + 0.15f),
                Mathf.Min(1, color.b + 0.15f));
            colors.pressedColor = new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f);
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            btn.colors = colors;

            var text = MakeText(obj.transform, label, 14, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.raycastTarget = false;

            return btn;
        }

        private Image MakeImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private Image MakeFullscreenImage(Transform parent, string name, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = obj.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private CanvasGroup CreateFullscreenGroup(string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(_canvas.transform, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return obj.AddComponent<CanvasGroup>();
        }

        private CanvasGroup CreateSubCanvasGroup(Transform parent, string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return obj.AddComponent<CanvasGroup>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
            else
                Debug.LogWarning($"Field '{fieldName}' not found on {type.Name}");
        }
    }
}
