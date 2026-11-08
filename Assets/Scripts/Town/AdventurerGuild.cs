using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Core;
using ActionRPG.Characters;
using ActionRPG.Items;

namespace ActionRPG.Town
{
    public enum AdventurerRank
    {
        RankF,
        RankE,
        RankD,
        RankC,
        RankB,
        RankA,
        RankS
    }

    [Serializable]
    public class GuildQuest
    {
        public string questId;
        public string questTitle;
        public string targetEnemyName;
        public int requiredKills = 5;
        public int currentKills = 0;
        public int rewardExp = 80;
        public int rewardGold = 50;
        public bool isCompleted = false;
        public bool isClaimed = false;
    }

    public class AdventurerGuild : MonoBehaviour
    {
        public static AdventurerGuild Instance { get; private set; }

        [Header("Hiệp Hội Mạo Hiểm Giả")]
        public AdventurerRank currentRank = AdventurerRank.RankF;
        public int guildReputation = 0;
        public List<GuildQuest> activeQuests = new List<GuildQuest>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            EventBus.OnEnemyKilled += OnEnemyDefeated;
            InitializeDefaultQuests();
        }

        private void OnDestroy()
        {
            EventBus.OnEnemyKilled -= OnEnemyDefeated;
        }

        private void InitializeDefaultQuests()
        {
            activeQuests.Add(new GuildQuest
            {
                questId = "quest_slime_hunt",
                questTitle = "Nhiệm vụ Tân Thủ: Quét sạch 5 Slime Rừng",
                targetEnemyName = "Slime",
                requiredKills = 5,
                rewardExp = 80,
                rewardGold = 50
            });

            activeQuests.Add(new GuildQuest
            {
                questId = "quest_wolf_hunt",
                questTitle = "Bảo vệ Làng: Tiêu diệt 3 Sói Hoang",
                targetEnemyName = "Sói Hoang",
                requiredKills = 3,
                rewardExp = 120,
                rewardGold = 80
            });
        }

        private void OnEnemyDefeated(GameObject enemyObj, int exp, int gold)
        {
            string enemyName = enemyObj.name;
            foreach (var q in activeQuests)
            {
                if (!q.isCompleted && enemyName.Contains(q.targetEnemyName))
                {
                    q.currentKills++;
                    Debug.Log($"[Adventurer's Guild] Tiến độ: {q.questTitle} ({q.currentKills}/{q.requiredKills})");

                    if (q.currentKills >= q.requiredKills)
                    {
                        q.isCompleted = true;
                        Debug.Log($"<color=yellow>[Guild Quest] HOÀN THÀNH: {q.questTitle}! Hãy đến nhận thưởng!</color>");
                    }
                }
            }
        }

        public bool ClaimReward(GuildQuest quest)
        {
            if (quest == null || !quest.isCompleted || quest.isClaimed) return false;

            quest.isClaimed = true;
            if (CharacterStats.PlayerInstance != null)
            {
                CharacterStats.PlayerInstance.GainExp(quest.rewardExp);
            }
            if (Inventory.Instance != null)
            {
                Inventory.Instance.AddGold(quest.rewardGold);
            }

            guildReputation += 25;
            CheckRankPromotion();

            Debug.Log($"<color=green>[Adventurer's Guild] Đã nhận thưởng: +{quest.rewardExp} EXP, +{quest.rewardGold} Vàng!</color>");
            return true;
        }

        private void CheckRankPromotion()
        {
            if (guildReputation >= 100 && currentRank == AdventurerRank.RankF)
            {
                currentRank = AdventurerRank.RankE;
                Debug.Log("<color=cyan>[Guild] Chúc mừng bạn đã thăng lên Mạo Hiểm Giả Hạng E!</color>");
            }
            else if (guildReputation >= 250 && currentRank == AdventurerRank.RankE)
            {
                currentRank = AdventurerRank.RankD;
                Debug.Log("<color=cyan>[Guild] Chúc mừng bạn đã thăng lên Mạo Hiểm Giả Hạng D!</color>");
            }
        }
    }
}
