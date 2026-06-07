using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Data;
using EchoesOfArcadia.Battle;
using EchoesOfArcadia.TimeSystem;
using EchoesOfArcadia.Field;
using EchoesOfArcadia.Social;
using EchoesOfArcadia.Dialogue;
using EchoesOfArcadia.UI;

namespace EchoesOfArcadia.Core
{
    public class GameFlowController : MonoBehaviour
    {
        public static GameFlowController Instance { get; private set; }

        [Header("Party Data")]
        [SerializeField] private CharacterData protagonistData;
        [SerializeField] private List<CharacterData> partyMembers = new();

        [Header("Social Link Characters")]
        [SerializeField] private CharacterData[] socialLinkCharacters;

        [Header("Debug / Test")]
        [SerializeField] private EnemyData[] testEnemies;

        private bool gameStarted;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<NewGameStartedEvent>(OnNewGameStarted);
            GameEventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
            GameEventBus.Subscribe<DungeonCompletedEvent>(OnDungeonCompleted);
            GameEventBus.Subscribe<DialogueChoiceEvent>(OnDialogueChoice);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<NewGameStartedEvent>(OnNewGameStarted);
            GameEventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
            GameEventBus.Unsubscribe<DungeonCompletedEvent>(OnDungeonCompleted);
            GameEventBus.Unsubscribe<DialogueChoiceEvent>(OnDialogueChoice);
        }

        private void OnNewGameStarted(NewGameStartedEvent e)
        {
            if (gameStarted) return;
            gameStarted = true;

            InitializeSocialLinks();
            StartPrologue();
        }

        public void StartBattle(List<EnemyData> enemies)
        {
            if (BattleManager.Instance == null) return;

            var activeParty = new List<CharacterData>();
            if (protagonistData != null) activeParty.Add(protagonistData);
            foreach (var member in partyMembers)
            {
                if (member != null) activeParty.Add(member);
                if (activeParty.Count >= 4) break;
            }

            BattleManager.Instance.StartBattle(activeParty, enemies);
            GameManager.Instance?.ChangePhase(GamePhase.Battle);
        }

        public void StartRandomBattle()
        {
            if (testEnemies == null || testEnemies.Length == 0) return;

            int enemyCount = Random.Range(1, 4);
            var enemies = new List<EnemyData>();
            for (int i = 0; i < enemyCount; i++)
            {
                enemies.Add(testEnemies[Random.Range(0, testEnemies.Length)]);
            }

            StartBattle(enemies);
        }

        public void AddPartyMember(CharacterData character)
        {
            if (!partyMembers.Contains(character))
                partyMembers.Add(character);
        }

        public void TriggerSocialLinkEvent(Arcana arcana, int rank)
        {
            string fileName = GetBondDialogueFile(arcana, rank);
            if (string.IsNullOrEmpty(fileName)) return;

            var jsonData = DialogueDataLoader.LoadFromStreamingAssets(fileName);
            if (jsonData == null) return;

            var dialogueData = DialogueDataLoader.ConvertToScriptableObject(jsonData);
            dialogueData.relatedArcana = arcana;
            dialogueData.requiredBondRank = rank;

            if (TimeManager.Instance != null)
                TimeManager.Instance.SpendActionPoint();

            DialogueSystem.Instance?.StartDialogue(dialogueData);
        }

        private void InitializeSocialLinks()
        {
            if (SocialLinkManager.Instance == null || socialLinkCharacters == null) return;

            foreach (var character in socialLinkCharacters)
            {
                if (character != null)
                    SocialLinkManager.Instance.RegisterBond(character);
            }
        }

        private void StartPrologue()
        {
            var jsonData = DialogueDataLoader.LoadFromStreamingAssets("prologue_train.json");
            if (jsonData != null)
            {
                var dialogueData = DialogueDataLoader.ConvertToScriptableObject(jsonData);
                DialogueSystem.Instance?.StartDialogue(dialogueData);
            }
        }

        private void OnBattleEnded(BattleEndedEvent e)
        {
            GameManager.Instance?.ChangePhase(GamePhase.Field);
        }

        private void OnDungeonCompleted(Echo.DungeonCompletedEvent e)
        {
            if (e.VictimRescued)
                Debug.Log($"Victim rescued in {e.DungeonName}!");
        }

        private void OnDialogueChoice(DialogueChoiceEvent e)
        {
            SocialLinkManager.Instance?.AddBondPoints(e.Arcana, e.BondPoints);
        }

        private static string GetBondDialogueFile(Arcana arcana, int rank)
        {
            string prefix = arcana switch
            {
                Arcana.Priestess => "shiori",
                Arcana.Emperor => "gai",
                Arcana.HangedMan => "mikoto",
                Arcana.Death => "akira",
                Arcana.Strength => "aoi",
                Arcana.Chariot => "yusuke",
                Arcana.Sun => "hana",
                Arcana.Fortune => "ryuichi",
                Arcana.Moon => "saya",
                Arcana.Justice => "hina",
                Arcana.Hermit => "genzo",
                _ => null
            };

            if (prefix == null) return null;
            return $"{prefix}_bond_rank{rank}.json";
        }
    }
}
