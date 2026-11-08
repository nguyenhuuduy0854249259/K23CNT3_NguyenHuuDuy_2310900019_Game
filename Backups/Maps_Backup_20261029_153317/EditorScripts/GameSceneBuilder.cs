#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ActionRPG.Core;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Combat;
using ActionRPG.Pets;
using ActionRPG.Enemies;
using ActionRPG.Items;
using ActionRPG.Town;
using ActionRPG.UI;
using ActionRPG.Visuals;
using ActionRPG.Crafting;

public class GameSceneBuilder
{
    private static readonly string SoPath = "Assets/ScriptableObjects";

    [MenuItem("Tools/ActionRPG/0. BUILD EVERYTHING (Sprites + Database + Main Scene)")]
    public static void BuildEverything()
    {
        EnsureRequiredLayers();
        PixelArtAssetGenerator.GenerateAllAssets();
        BuildDatabase();
        SetupMainGameScene();
        Debug.Log("<color=green>★★★★★ ACTION RPG PET SYSTEM: HOÀN TẤT TOÀN BỘ KHỞI TẠO DỰ ÁN! ★★★★★</color>");
    }

    private static void EnsureRequiredLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        if (layersProp == null)
        {
            Debug.LogWarning("[GameSceneBuilder] Không tìm thấy TagManager.layers. Bỏ qua tạo layer tùy chỉnh.");
            return;
        }

        EnsureLayer(layersProp, "Player");
        EnsureLayer(layersProp, "Enemy");
        EnsureLayer(layersProp, "NPC");
        EnsureLayer(layersProp, "World");
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureLayer(SerializedProperty layersProp, string layerName)
    {
        for (int i = 8; i < layersProp.arraySize; i++)
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
            if (sp.stringValue == layerName)
            {
                return;
            }
        }

        for (int i = 8; i < layersProp.arraySize; i++)
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = layerName;
                return;
            }
        }

        Debug.LogWarning($"[GameSceneBuilder] Không còn chỗ trống để tạo layer '{layerName}'. Hãy tạo thủ công trong Project Settings > Tags and Layers.");
    }

    [InitializeOnLoadMethod]
    private static void AutoInitializeOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists("Assets/Scenes/MainGame.unity"))
            {
                Debug.Log("[AutoSetup] Phát hiện khởi chạy lần đầu! Đang tự động tạo toàn bộ Sprites, Database và Scene MainGame...");
                BuildEverything();
            }
        };
    }

    [MenuItem("Tools/ActionRPG/1. Build ScriptableObjects Database")]
    public static void BuildDatabase()
    {
        Directory.CreateDirectory(SoPath);
        Directory.CreateDirectory(Path.Combine(SoPath, "Classes"));
        Directory.CreateDirectory(Path.Combine(SoPath, "Items"));
        Directory.CreateDirectory(Path.Combine(SoPath, "Pets"));
        Directory.CreateDirectory(Path.Combine(SoPath, "Enemies"));

        // 1. Tạo Items: Thức ăn (3 loại) & Giấy Thu Phục & Trang bị Độc Bản
        FoodItemData foodNormal = ScriptableObject.CreateInstance<FoodItemData>();
        foodNormal.itemId = "food_bread";
        foodNormal.itemName = "Bánh Mì Lúa Mạch";
        foodNormal.category = FoodCategory.NormalFood;
        foodNormal.hungerRestoreAmount = 35f;
        foodNormal.buyPrice = 10;
        foodNormal.description = "Thực phẩm bình thường giúp no bụng, không tăng chỉ số.";
        AssetDatabase.CreateAsset(foodNormal, $"{SoPath}/Items/Food_Normal_Bread.asset");

        FoodItemData foodCombo = ScriptableObject.CreateInstance<FoodItemData>();
        foodCombo.itemId = "food_steak";
        foodCombo.itemName = "Thịt Bò Rừng Nướng Thảo Mộc";
        foodCombo.category = FoodCategory.BuffAndHungerFood;
        foodCombo.hungerRestoreAmount = 45f;
        foodCombo.buffStat = BuffStatType.AttackPower;
        foodCombo.buffValue = 8f;
        foodCombo.buffDurationSeconds = 180f;
        foodCombo.buyPrice = 45;
        foodCombo.description = "Vừa giải trừ cơn đói vừa tăng 8 điểm Công trong 3 phút!";
        AssetDatabase.CreateAsset(foodCombo, $"{SoPath}/Items/Food_Combo_Steak.asset");

        FoodItemData foodPureBuff = ScriptableObject.CreateInstance<FoodItemData>();
        foodPureBuff.itemId = "food_elixir";
        foodPureBuff.itemName = "Dược Liệu Tinh Hoa Viêm Long";
        foodPureBuff.category = FoodCategory.PureBuffFood;
        foodPureBuff.hungerRestoreAmount = 0f; // 0 điểm đói
        foodPureBuff.buffStat = BuffStatType.CritRate;
        foodPureBuff.buffValue = 0.20f; // +20% Crit
        foodPureBuff.buffDurationSeconds = 300f;
        foodPureBuff.buyPrice = 90;
        foodPureBuff.description = "Thực phẩm thuần thuộc tính không chống đói, tăng 20% Tỉ Lệ Bạo Kích trong 5 phút.";
        AssetDatabase.CreateAsset(foodPureBuff, $"{SoPath}/Items/Food_PureBuff_Elixir.asset");

        ItemData captureScroll = ScriptableObject.CreateInstance<ItemData>();
        captureScroll.itemId = "item_capture_scroll";
        captureScroll.itemName = "Giấy Thu Phục";
        captureScroll.itemType = ItemType.CaptureScroll;
        captureScroll.rarity = ItemRarity.Uncommon;
        captureScroll.buyPrice = 25;
        captureScroll.description = "Dùng để thu phục quái vật khi máu của chúng giảm xuống dưới 30%.";
        AssetDatabase.CreateAsset(captureScroll, $"{SoPath}/Items/Item_Capture_Scroll.asset");

        EquipmentData uniqueCloak = ScriptableObject.CreateInstance<EquipmentData>();
        uniqueCloak.itemId = "equip_phoenix_cloak";
        uniqueCloak.itemName = "Áo Choàng Phượng Hoàng [Độc Bản]";
        uniqueCloak.equipSlot = EquipSlot.Armor;
        uniqueCloak.rarity = ItemRarity.Unique;
        uniqueCloak.bonusHp = 120f;
        uniqueCloak.bonusDef = 14f;
        uniqueCloak.isUnique = true;
        uniqueCloak.uniqueEffect = UniqueEffectType.PhoenixRevive;
        uniqueCloak.uniqueEffectName = "Niết Bàn Tái Sinh";
        uniqueCloak.uniqueEffectDescription = "Khi lượng máu tụt về 0, tự động hồi sinh với 50% HP (Thời gian hồi: 10 phút).";
        AssetDatabase.CreateAsset(uniqueCloak, $"{SoPath}/Items/Equip_Unique_PhoenixCloak.asset");

        // 2. Tạo Classes
        CharacterClassData clsArcher = ScriptableObject.CreateInstance<CharacterClassData>();
        clsArcher.className = "Xạ Thủ (Ranger)";
        clsArcher.branch = ClassBranch.Archer;
        clsArcher.tier = 1;
        clsArcher.requiredLevelForPromotion = 30;
        clsArcher.baseAtk = 24f;
        clsArcher.baseDef = 6f;
        clsArcher.baseHp = 110f;
        clsArcher.baseSpeed = 5.2f;
        clsArcher.description = "Chuyên gia xạ kích tầm xa, tốc độ di chuyển cao. Đạt cấp 30 để tiến cấp Class Thần Xạ!";
        clsArcher.characterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_archer.png");
        AssetDatabase.CreateAsset(clsArcher, $"{SoPath}/Classes/Class_Archer.asset");

        CharacterClassData clsWarrior = ScriptableObject.CreateInstance<CharacterClassData>();
        clsWarrior.className = "Chiến Binh (Warrior)";
        clsWarrior.branch = ClassBranch.Warrior;
        clsWarrior.tier = 1;
        clsWarrior.requiredLevelForPromotion = 30;
        clsWarrior.baseAtk = 22f;
        clsWarrior.baseDef = 12f;
        clsWarrior.baseHp = 160f;
        clsWarrior.baseSpeed = 4.3f;
        clsWarrior.description = "Chống chịu bền bỉ, cận chiến áp đảo với khiên chắn và cự kiếm. Đạt cấp 30 để tiến cấp Class Cận Chiến Cấp Cao!";
        clsWarrior.characterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_warrior.png");
        AssetDatabase.CreateAsset(clsWarrior, $"{SoPath}/Classes/Class_Warrior.asset");

        CharacterClassData clsMage = ScriptableObject.CreateInstance<CharacterClassData>();
        clsMage.className = "Pháp Sư (Mage)";
        clsMage.branch = ClassBranch.Mage;
        clsMage.tier = 1;
        clsMage.requiredLevelForPromotion = 30;
        clsMage.baseAtk = 28f;
        clsMage.baseDef = 4f;
        clsMage.baseHp = 95f;
        clsMage.baseSpeed = 4.4f;
        clsMage.description = "Phép thuật uy lực, điều khiển các nguyên tố và khống chế diện rộng. Đạt cấp 30 để tiến cấp Class Đại Pháp Sư!";
        clsMage.characterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_mage.png");
        AssetDatabase.CreateAsset(clsMage, $"{SoPath}/Classes/Class_Mage.asset");

        CharacterClassData clsNovice = ScriptableObject.CreateInstance<CharacterClassData>();
        clsNovice.className = "Tân Thủ (Novice)";
        clsNovice.branch = ClassBranch.Novice;
        clsNovice.tier = 0;
        clsNovice.requiredLevelForPromotion = 15;
        clsNovice.baseAtk = 15f;
        clsNovice.baseDef = 5f;
        clsNovice.baseHp = 100f;
        clsNovice.baseSpeed = 4.5f;
        clsNovice.description = "Lớp nhân vật khởi đầu cho mọi nhà mạo hiểm. Đạt cấp 15 để chọn 1 trong 3 trường phái và mở khóa Thu Phục Thú Cưng!";
        clsNovice.characterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player_novice.png");
        clsNovice.promotionOptions.Add(clsArcher);
        clsNovice.promotionOptions.Add(clsWarrior);
        clsNovice.promotionOptions.Add(clsMage);
        AssetDatabase.CreateAsset(clsNovice, $"{SoPath}/Classes/Class_Novice.asset");

        // 3. Tạo Quái Vật & Boss
        EnemyData enemySlime = ScriptableObject.CreateInstance<EnemyData>();
        enemySlime.enemyId = "enemy_slime";
        enemySlime.enemyName = "Slime Rừng";
        enemySlime.level = 2;
        enemySlime.maxHp = 45f;
        enemySlime.attackPower = 6f;
        enemySlime.defense = 2f;
        enemySlime.expReward = 20;
        enemySlime.goldReward = 8;
        enemySlime.element = ElementType.Earth;
        enemySlime.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/enemy_slime.png");
        enemySlime.dropTable.Add(new DropEntry { item = foodNormal, dropChancePercent = 35f, minQuantity = 1, maxQuantity = 2 });
        enemySlime.dropTable.Add(new DropEntry { item = captureScroll, dropChancePercent = 20f, minQuantity = 1, maxQuantity = 1 });
        AssetDatabase.CreateAsset(enemySlime, $"{SoPath}/Enemies/Enemy_Slime.asset");

        EnemyData enemyWolf = ScriptableObject.CreateInstance<EnemyData>();
        enemyWolf.enemyId = "enemy_wolf";
        enemyWolf.enemyName = "Sói Hoang Dã";
        enemyWolf.level = 6;
        enemyWolf.maxHp = 85f;
        enemyWolf.attackPower = 12f;
        enemyWolf.defense = 4f;
        enemyWolf.moveSpeed = 3.5f;
        enemyWolf.expReward = 50;
        enemyWolf.goldReward = 22;
        enemyWolf.element = ElementType.Wind;
        enemyWolf.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/enemy_wolf.png");
        enemyWolf.dropTable.Add(new DropEntry { item = foodCombo, dropChancePercent = 25f, minQuantity = 1, maxQuantity = 1 });
        AssetDatabase.CreateAsset(enemyWolf, $"{SoPath}/Enemies/Enemy_Wolf.asset");

        EnemyData bossWolfKing = ScriptableObject.CreateInstance<EnemyData>();
        bossWolfKing.enemyId = "boss_wolf_king";
        bossWolfKing.enemyName = "Vua Sói Hắc Ám [Trùm Khu Vực]";
        bossWolfKing.level = 12;
        bossWolfKing.maxHp = 450f;
        bossWolfKing.attackPower = 28f;
        bossWolfKing.defense = 8f;
        bossWolfKing.moveSpeed = 3.8f;
        bossWolfKing.isAreaBoss = true;
        bossWolfKing.expReward = 350;
        bossWolfKing.goldReward = 200;
        bossWolfKing.element = ElementType.Demon;
        bossWolfKing.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/enemy_boss_wolfking.png");
        bossWolfKing.dropTable.Add(new DropEntry { item = uniqueCloak, dropChancePercent = 8.5f, minQuantity = 1, maxQuantity = 1 });
        bossWolfKing.dropTable.Add(new DropEntry { item = foodPureBuff, dropChancePercent = 50f, minQuantity = 1, maxQuantity = 2 });
        AssetDatabase.CreateAsset(bossWolfKing, $"{SoPath}/Enemies/Boss_WolfKing.asset");

        // 4. Tạo Pet
        SkillData petFoxSkill = ScriptableObject.CreateInstance<SkillData>();
        petFoxSkill.skillId = "pet_firefox_flame_bite";
        petFoxSkill.skillName = "Lửa Cắn Hồ Ly";
        petFoxSkill.description = "Đòn đánh lửa mạnh, gây sát thương lớn lên kẻ địch gần nhất.";
        petFoxSkill.element = ElementType.Fire;
        petFoxSkill.damageMultiplier = 2.1f;
        petFoxSkill.cooldownSeconds = 6f;
        petFoxSkill.range = 2.4f;

        PetData petFox = ScriptableObject.CreateInstance<PetData>();
        petFox.petId = "pet_firefox";
        petFox.petName = "Hồ Ly Lửa";
        petFox.element = ElementType.Fire;
        petFox.role = PetRole.DPS;
        petFox.maxHp = 90f;
        petFox.attackPower = 15f;
        petFox.defense = 5f;
        petFox.petSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pet_firefox.png");
        petFox.activeSkill = petFoxSkill;
        AssetDatabase.CreateAsset(petFox, $"{SoPath}/Pets/Pet_FireFox.asset");

        PetData petBear = ScriptableObject.CreateInstance<PetData>();
        petBear.petId = "pet_ironbear";
        petBear.petName = "Gấu Sắt";
        petBear.element = ElementType.Earth;
        petBear.role = PetRole.Tanker;
        petBear.maxHp = 125f;
        petBear.attackPower = 12f;
        petBear.defense = 8f;
        petBear.petSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pet_waterslime.png");
        AssetDatabase.CreateAsset(petBear, $"{SoPath}/Pets/Pet_IronBear.asset");

        PetData petMoth = ScriptableObject.CreateInstance<PetData>();
        petMoth.petId = "pet_moonmoth";
        petMoth.petName = "Bướm Ánh Trăng";
        petMoth.element = ElementType.Wind;
        petMoth.role = PetRole.Support;
        petMoth.maxHp = 80f;
        petMoth.attackPower = 10f;
        petMoth.defense = 4f;
        petMoth.petSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pet_windfairy.png");
        AssetDatabase.CreateAsset(petMoth, $"{SoPath}/Pets/Pet_MoonMoth.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=green>[GameSceneBuilder] Đã khởi tạo hoàn tất toàn bộ ScriptableObjects Database!</color>");
    }

    [MenuItem("Tools/ActionRPG/2. Setup Main Game Scene")]
    public static void SetupMainGameScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0f, 0f, -10f);
            mainCam.orthographic = true;
            mainCam.orthographicSize = 6.5f;
            mainCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            CameraFollow2D camFollow = mainCam.gameObject.AddComponent<CameraFollow2D>();
        }

        // 1b. Tạo Làng Tân Thủ Pixel Art (Tilemap + Buildings + NPCs + Portal)
        CreateVillageBackground();

        // 2. GameManager & Systems
        GameObject managersObj = new GameObject("GameSystems");
        managersObj.AddComponent<GameManager>();
        managersObj.AddComponent<Inventory>();
        managersObj.AddComponent<PetCaptureSystem>();
        managersObj.AddComponent<BlacksmithSystem>();
        managersObj.AddComponent<AlchemySystem>();
        managersObj.AddComponent<AdventurerGuild>();
        managersObj.AddComponent<FloatingDamageText>();
        managersObj.AddComponent<ActionRPG.Save.SaveManager>();

        // 3. Player
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer < 0) playerLayer = 0;

        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.layer = playerLayer;
        playerObj.transform.position = new Vector3(0f, 0f, 0f); // Spawn tại trung tâm làng tân thủ
        playerObj.transform.localScale = Vector3.one;

        SpriteRenderer playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sortingOrder = 5;

        CircleCollider2D playerCol = playerObj.AddComponent<CircleCollider2D>();
        playerCol.radius = 0.35f;

        Rigidbody2D playerRb = playerObj.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 0f;
        playerRb.freezeRotation = true;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        playerObj.AddComponent<Hurtbox2D>();
        CharacterStats pStats = playerObj.AddComponent<CharacterStats>();
        HungerSystem pHunger = playerObj.AddComponent<HungerSystem>();
        ClassSystem pClass = playerObj.AddComponent<ClassSystem>();
        PlayerController pCtrl = playerObj.AddComponent<PlayerController>();
        SpriteAnimator2D pAnim = playerObj.AddComponent<SpriteAnimator2D>();
        Animator pUnityAnim = playerObj.AddComponent<Animator>();
        RuntimeAnimatorController animCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/GameArt/Player/Animations/Player.controller");
        if (animCtrl != null)
        {
            pUnityAnim.runtimeAnimatorController = animCtrl;
        }

        // Load animation frames from sprite sheet
        SetupCharacterAnimation(pAnim, "Assets/Sprites/player_novice.png");

        // Nạp Class Novice vào Player
        CharacterClassData noviceClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>($"{SoPath}/Classes/Class_Novice.asset");
        if (noviceClass != null) pClass.currentClass = noviceClass;

        if (mainCam != null)
        {
            mainCam.GetComponent<CameraFollow2D>().target = playerObj.transform;
        }

        // 4. Đồng Hành: Đầu game nhân vật KHÔNG THỂ có pet.
        // Chỉ khi đạt Cấp 15 và hoàn tất chuyển đổi Class cấp cao hơn thì mới xuất hiện ô thu phục pet và Tiệm Thú Cưng bắt đầu bán Giấy Thu Phục.

        // 5. Cửa Hàng & NPC Tại Làng Tân Thủ
        CreateTownNpc("NPC_Blacksmith", "Rèn Sắt - Thợ Rèn", ShopType.Blacksmith, new Vector3(3f, 2f, 0));
        CreateTownNpc("NPC_Restaurant", "Quán Ăn Thỏ Rừng", ShopType.Restaurant, new Vector3(3f, -2f, 0));
        CreateTownNpc("NPC_AdventurerGuild", "Hiệp Hội Mạo Hiểm Giả", ShopType.AdventurerGuild, new Vector3(-3f, 2f, 0));
        CreateTownNpc("NPC_PetShop", "Trang Trại Thú Cưng", ShopType.PetSanctuary, new Vector3(-3f, -2f, 0));

        // 6. Quái Vật & Bãi Farm (Spawner)
        GameObject spawnerZone1 = new GameObject("Zone1_BeginnerSpawner");
        spawnerZone1.transform.position = new Vector3(12f, 2f, 0f);
        EnemySpawner s1 = spawnerZone1.AddComponent<EnemySpawner>();
        s1.zoneName = "Rừng Tân Thủ (Lv 1-5)";
        EnemyData slimeData = AssetDatabase.LoadAssetAtPath<EnemyData>($"{SoPath}/Enemies/Enemy_Slime.asset");
        if (slimeData != null) s1.spawnableEnemies.Add(slimeData);

        GameObject spawnerZone2 = new GameObject("Zone2_WildernessSpawner");
        spawnerZone2.transform.position = new Vector3(28f, 2f, 0f);
        EnemySpawner s2 = spawnerZone2.AddComponent<EnemySpawner>();
        s2.zoneName = "Vùng Hoang Dã (Lv 6-15) & Boss Vua Sói";
        EnemyData wolfData = AssetDatabase.LoadAssetAtPath<EnemyData>($"{SoPath}/Enemies/Enemy_Wolf.asset");
        EnemyData bossData = AssetDatabase.LoadAssetAtPath<EnemyData>($"{SoPath}/Enemies/Boss_WolfKing.asset");
        if (wolfData != null) s2.spawnableEnemies.Add(wolfData);
        if (bossData != null) s2.spawnableEnemies.Add(bossData);

        // 6b. Cổng dịch chuyển đến bãi săn quái
        CreatePortalToHuntingGrounds();

        // 7. Giao Diện Canvas HUD & UIManager
        SetupUI(playerObj);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/MainGame.unity");
        Debug.Log("<color=green>[GameSceneBuilder] ĐÃ THIẾT LẬP THÀNH CÔNG SCENE MAIN GAME: Assets/Scenes/MainGame.unity!</color>");
    }

    private static void CreateTownNpc(string goName, string npcTitle, ShopType type, Vector3 position)
    {
        int npcLayer = LayerMask.NameToLayer("NPC");
        if (npcLayer < 0) npcLayer = 0;

        GameObject npcObj = new GameObject(goName);
        npcObj.transform.position = position;
        npcObj.layer = npcLayer;

        SpriteRenderer sr = npcObj.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.9f, 0.8f, 0.5f);
        sr.sortingOrder = 3;

        BoxCollider2D col = npcObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);

        ShopNPC shop = npcObj.AddComponent<ShopNPC>();
        shop.npcName = npcTitle;
        shop.shopType = type;
        shop.greetingDialogue = $"Xin chào mạo hiểm giả! Đây là {npcTitle}. Ngươi cần trợ giúp gì?";
    }

    private static void SetupUI(GameObject player)
    {
        // 1. Root Canvas
        GameObject canvasObj = new GameObject("GameUIRoot", typeof(RectTransform));
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Event System
        GameObject eventSys = new GameObject("EventSystem");
        eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSys.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 3. Gameplay HUD
        GameObject hudObj = new GameObject("GameplayHUD", typeof(RectTransform));
        hudObj.transform.SetParent(canvasObj.transform, false);
        ActionRPG.UI.Gameplay.GameplayHUD gameplayHUD = hudObj.AddComponent<ActionRPG.UI.Gameplay.GameplayHUD>();
        gameplayHUD.EnsureUIHierarchy();

        // 4. Các hệ thống UI cốt lõi khác: Class Demo Switcher [F1], Pause Menu [ESC], Character Panel [U], Class Selection, Inventory [I]
        canvasObj.AddComponent<ClassDemoSwitcherUI>();
        canvasObj.AddComponent<PauseMenuUI>();
        canvasObj.AddComponent<ActionRPG.UI.Character.CharacterUI>();
        canvasObj.AddComponent<ClassSelectionUI>();
        canvasObj.AddComponent<InventoryUI>();
        canvasObj.AddComponent<ClassChangeUI>();
    }

    private static void SetupCharacterAnimation(SpriteAnimator2D animator, string spriteSheetPath)
    {
        // First check if high quality pixel character sprites exist
        var pixelIdle = AssetDatabase.LoadAllAssetsAtPath("Assets/GameArt/Player/Sprites/Player_Idle_4F.png");
        var pixelWalk = AssetDatabase.LoadAllAssetsAtPath("Assets/GameArt/Player/Sprites/Player_Walk_6F.png");
        var idles = new List<Sprite>();
        var walks = new List<Sprite>();
        foreach (var a in pixelIdle) if (a is Sprite s) idles.Add(s);
        foreach (var a in pixelWalk) if (a is Sprite s) walks.Add(s);

        if (idles.Count > 0)
        {
            idles.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            walks.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            animator.idleFrames = idles.ToArray();
            animator.walkFrames = walks.ToArray();
            var spr = animator.GetComponent<SpriteRenderer>();
            if (spr != null && idles.Count > 0) spr.sprite = idles[0];
            return;
        }

        // Load all sliced sprites from the sprite sheet
        var allSprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
        var sprites = new List<Sprite>();
        foreach (var asset in allSprites)
        {
            if (asset is Sprite s) sprites.Add(s);
        }

        if (sprites.Count == 0) return;

        // Sort by name to ensure correct order (player_novice_0, player_novice_1, etc.)
        sprites.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

        // Character sprite sheets have 12 frames: 4 idle + 4 walk + 4 attack
        int framesPerAnim = 4;
        if (sprites.Count >= 12)
        {
            animator.idleFrames = sprites.GetRange(0, framesPerAnim).ToArray();
            animator.walkFrames = sprites.GetRange(framesPerAnim, framesPerAnim).ToArray();
            animator.attackFrames = sprites.GetRange(framesPerAnim * 2, framesPerAnim).ToArray();
            
            // Set initial sprite to first idle frame
            var sr = animator.GetComponent<SpriteRenderer>();
            if (sr != null && animator.idleFrames.Length > 0)
            {
                sr.sprite = animator.idleFrames[0];
            }
        }
    }

    private static void CreateVillageBackground()
    {
        // Load village background sprite
        Sprite villageBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/lang tan thu.png");
        if (villageBg == null)
        {
            Debug.LogWarning("[GameSceneBuilder] Không tìm thấy sprite làng tân thủ tại Assets/Sprites/lang tan thu.png");
            return;
        }

        GameObject bgObj = new GameObject("Village_Background");
        bgObj.transform.position = new Vector3(0f, 0f, 10f); // Behind everything
        
        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = villageBg;
        bgSr.sortingOrder = -10; // Render behind all game objects
        bgSr.drawMode = SpriteDrawMode.Simple;
        
        // Scale background to cover camera view (orthographicSize = 6.5, aspect ~16:9)
        // Camera height = 13 units, width = ~23 units
        float cameraHeight = 13f;
        float cameraWidth = cameraHeight * 16f / 9f;
        
        float spriteWidth = villageBg.bounds.size.x;
        float spriteHeight = villageBg.bounds.size.y;
        
        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;
        float scale = Mathf.Max(scaleX, scaleY) * 1.2f; // Slightly larger to ensure coverage
        
        bgObj.transform.localScale = new Vector3(scale, scale, 1f);
        
        Debug.Log($"[GameSceneBuilder] Đã tạo background làng tân thủ (scale: {scale:F2})");
    }

    private static void CreatePortalToHuntingGrounds()
    {
        // Create portal at village edge to teleport to hunting zones
        GameObject portalObj = new GameObject("Portal_To_HuntingGrounds");
        portalObj.transform.position = new Vector3(8f, 0f, 0f); // Right side of village
        
        // Visual - use a simple sprite or create a circle
        SpriteRenderer portalSr = portalObj.AddComponent<SpriteRenderer>();
        portalSr.color = new Color(0.2f, 0.6f, 1f, 0.7f); // Blue portal color
        portalSr.sortingOrder = 2;
        portalSr.drawMode = SpriteDrawMode.Simple;
        
        // Create a simple circle sprite for portal
        // Using a default circle or create procedurally
        CircleCollider2D portalCol = portalObj.AddComponent<CircleCollider2D>();
        portalCol.radius = 1.5f;
        portalCol.isTrigger = true;
        
        // Add portal trigger script
        PortalTrigger portalTrigger = portalObj.AddComponent<PortalTrigger>();
        portalTrigger.destinationPosition = new Vector3(12f, 2f, 0f); // Zone1 beginner spawner position
        portalTrigger.portalName = "Cổng Dịch Chuyển - Rừng Tân Thủ";
        
        Debug.Log("[GameSceneBuilder] Đã tạo cổng dịch chuyển đến bãi săn quái");
    }
}
#endif
