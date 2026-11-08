using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Items;
using ActionRPG.Pets;

namespace ActionRPG.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Trạng Thái Trò Chơi")]
        public bool isGamePaused = false;
        public GameObject playerObject;
        public string currentZoneName = "Làng Khởi Nguyên (Beginner Town)";

        private void Awake()
        {
            Time.timeScale = 1f;
            if (Instance == null)
            {
                Instance = this;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }
            LoadGameProfile();

            // Tự động mở màn hình chuyển nghề khi nhân vật đạt đủ cấp
            EventBus.OnPlayerLevelUp += OnPlayerLevelUp;
        }

        private void OnDestroy()
        {
            EventBus.OnPlayerLevelUp -= OnPlayerLevelUp;
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            // Kiểm tra sau mỗi lần lên cấp – nếu đủ điều kiện thì mở màn hình chuyển nghề
            if (ActionRPG.UI.ClassChangeUI.Instance != null)
            {
                ActionRPG.UI.ClassChangeUI.Instance.TryOpenPromotion();
            }
        }


        private void OnApplicationQuit()
        {
            SaveGameProfile();
        }

        public void SaveGameProfile()
        {
            if (playerObject == null) return;

            CharacterStats stats = playerObject.GetComponent<CharacterStats>();
            ClassSystem classSys = playerObject.GetComponent<ClassSystem>();
            HungerSystem hunger = playerObject.GetComponent<HungerSystem>();

            PlayerSaveData data = new PlayerSaveData();
            if (stats != null)
            {
                data.level = stats.level;
                data.currentExp = stats.currentExp;
                data.currentHp = stats.currentHp;
                data.currentMp = stats.currentMp;
            }
            if (classSys != null && classSys.currentClass != null)
            {
                data.currentClassName = classSys.currentClass.className;
            }
            if (hunger != null)
            {
                data.hunger = hunger.currentHunger;
            }
            if (Inventory.Instance != null)
            {
                data.gold = Inventory.Instance.gold;
            }

            SaveSystem.Save(data);
        }

        public void LoadGameProfile()
        {
            PlayerSaveData data = SaveSystem.Load();
            if (playerObject != null)
            {
                CharacterStats stats = playerObject.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    stats.level = data.level;
                    stats.currentExp = data.currentExp;
                }
                HungerSystem hunger = playerObject.GetComponent<HungerSystem>();
                if (hunger != null)
                {
                    hunger.currentHunger = data.hunger;
                }
            }
            if (Inventory.Instance != null)
            {
                Inventory.Instance.gold = data.gold;
            }
        }
    }
}
