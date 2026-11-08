using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using ActionRPG.Core;
using ActionRPG.Visuals;
using ActionRPG.Characters;
using ActionRPG.Enemies;
using ActionRPG.Combat;
using ActionRPG.Environment;
using ActionRPG.Core.World;
using ActionRPG.Core.World.Interactables;
using ActionRPG.Presentation.Views;
using ActionRPG.Core.Bridges;
using ActionRPG.Items;
using ActionRPG.Town;

/// <summary>
/// Tool kiến thiết lại toàn bộ Bản Đồ Thế Giới Lumina (Grand World V3 - Serene Village, CodeArt2D, Free-Roaming NPCs & Steppe Wildlife):
/// 1. Xóa các hình ảnh phẳng cũ của Bụi Thảo Mộc Xanh, Mỏ Quặng Đồng Thô, Mỏ Quặng Sắt Thô; thay bằng bộ sprite PixelSpec (codeart2d) + Khoáng thạch chuẩn tỷ lệ.
/// 2. Thiết lập đầy đủ Cửa Hàng & Công Trình Kiến Trúc (Làng Oakhaven): Tiệm Rèn Boris (Lò rèn động 8 frame + Nhà rèn), Dược Quán Elena, Hội Thám Hiểm Carlos, Tiệm Tạp Hóa, Khu Bảo Tồn Thú Cưng Kaelen và Nhà Dân.
/// 3. Các NPC trong làng di chuyển tự do (NPCWanderController2D + Hoạt ảnh 4 hướng): Trưởng Làng Eldrin, Thợ Rèn Boris, Dược Sĩ Elena, Hội Trưởng Carlos, Quản Thú Kaelen và Cư Dân Oakhaven.
/// 4. Thảo Nguyên Xanh (Emerald Steppe): Đưa các loài động vật có sẵn (Hươu Ciervo, Thỏ Nâu/Trắng Conejo, Sói Xám/Trắng Lobo) và quái vật (Slime, Chuột Chũi, Heo Rừng Đất, Cua Đá, Búp Cổ Thụ) vào hoạt động tự nhiên.
/// 5. Rải các bụi cỏ mini, hoa cỏ thảo nguyên, bụi thảo dược, mỏ quặng đồng/sắt/tinh thể và các tảng đá (Rocks pack) đúng tỷ lệ môi trường.
/// 6. Làm lại Tường Thành & Cổng Làng (nhỏ gọn, sắc nét, có trụ cổng mái ngói và rào gỗ đá).
/// 7. Dòng suối uốn lượn, cầu gỗ bắc ngang và Hồ Nước khu rừng / thảo nguyên sử dụng mẫu nước Serene Village + sóng nước động (water_waves_16x16).
/// </summary>
public static class ForestMapGenerator
{
    const string SpriteDir = "Assets/Sprites/ForestPack";
    const string VWDir = "Assets/Sprites/VillageAndWorld";
    const string GatherDir = "Assets/Sprites/Gathering";
    const string AnimalsDir = "Assets/Sprites/Animals";
    const string NPCsDir = "Assets/Sprites/NPCs";
    const string TileSpriteDir = "Assets/Sprites/ForestPack/Tiles";
    const string TileAssetDir = "Assets/Tilemaps/ForestDemo";
    const string PrefabDir = "Assets/Prefabs/ForestPrototype";
    const string AudioPath = "Assets/Audio/ForestPack/ancient_path.ogg";
    const string DemoScenePath = "Assets/Scenes/Forest_Demo_Map.unity";
    const string MainGameScenePath = "Assets/Scenes/MainGame.unity";

    // Kích thước toàn bộ Thế giới (160 ô ngang x 64 ô dọc: X [-65..95], Y [-32..32])
    const int MapMinX = -65, MapMaxX = 95;
    const int MapMinY = -32, MapMaxY = 32;

    // Tọa độ Làng Tân Thủ Oakhaven (44 x 36 ô)
    const int VillageMinX = -62, VillageMaxX = -18;
    const int VillageMinY = -18, VillageMaxY = 18;
    const int VillageGateYMin = -2, VillageGateYMax = 2; // Cổng làng bên phải mở tại X = -18

    // Con đường chạy dọc xuống phía Nam
    const int SouthRoadXMin = -13, SouthRoadXMax = -10;
    const int SouthRoadYMin = -28, SouthRoadYMax = 1;

    // 4 Vùng Boss trong Rừng Sâu (X: 25 -> 95)
    const int ForestBorderX = 25;
    const int VolcanoMinX = 30, VolcanoMaxX = 56, VolcanoMinY = -28, VolcanoMaxY = -8;
    const int LakeMinX = 64, LakeMaxX = 90, LakeMinY = -28, LakeMaxY = -6;
    const int TreeMinX = 30, TreeMaxX = 56, TreeMinY = 8, TreeMaxY = 28;
    const int PeakMinX = 64, PeakMaxX = 90, PeakMinY = 8, PeakMaxY = 28;

    const int OrderGround = 0, OrderObstacle = 10, OrderProp = 5000;

    static readonly Vector2Int DirtCell = new Vector2Int(1, 0);
    static readonly Vector2Int PathCell = new Vector2Int(7, 12);
    static readonly Vector2Int GrassCell = new Vector2Int(59, 8);
    static readonly Vector2Int WallCell = new Vector2Int(38, 3);

    [MenuItem("Tools/Lumina/Generate Forest Demo Map")]
    public static void GenerateDemoMap()
    {
        GenerateWorldToScene(DemoScenePath);
    }

    [MenuItem("Tools/Lumina/Generate World Map to MainGame")]
    public static void GenerateMainGameMap()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "⚠️ CẢNH BÁO NGUY HIỂM: GHI ĐÈ MAIN GAME",
            "Bạn đang yêu cầu dựng lại toàn bộ Map trong MainGame.unity từ đầu bằng thuật toán tự động.\n\n" +
            "HÀNH ĐỘNG NÀY SẼ XÓA SẠCH VÀ GHI ĐÈ toàn bộ công trình, cầu gỗ, suối nước, cây cối và Map 2 mà bạn đã chỉnh sửa thủ công!\n\n" +
            "Bạn có THỰC SỰ muốn xóa sạch để tạo lại từ đầu không?",
            "VẪN GHI ĐÈ (XÓA HẾT ĐỂ TẠO LẠI)",
            "HỦY BỎ (GIỮ NGUYÊN MAP HIỆN TẠI)");

        if (!confirm)
        {
            Debug.Log("<color=yellow>[ForestMapGenerator] Đã hủy thao tác ghi đè Scene. Bản đồ hiện tại của bạn được bảo vệ an toàn 100%.</color>");
            return;
        }

        GenerateWorldToScene(MainGameScenePath);
    }

    public static void GenerateBothMapsBatch()
    {
        Lumina.EditorTools.SereneVillageAndRocksImporter.ConfigureAll();
        GenerateWorldToScene(DemoScenePath, false);
        GenerateWorldToScene(MainGameScenePath, false);
    }

    public static void GenerateWorldToScene(string targetScenePath, bool promptSave = true)
    {
        if (promptSave && !Application.isBatchMode)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        }

        EnsureFolder("Assets/Tilemaps");
        EnsureFolder(TileAssetDir);
        EnsureFolder(TileSpriteDir);
        EnsureFolder(PrefabDir);
        EnsureFolder("Assets/Scenes");

        // 1. Cấu hình toàn bộ Sprite (ForestPack, SereneVillage, Rocks, VillageAndWorld, Gathering, Animals, NPCs)
        ConfigureSprites();
        Lumina.EditorTools.SereneVillageAndRocksImporter.ConfigureAll();

        Tile dirtTile = CreateTileFromTileset("Tile_Dirt", DirtCell, Tile.ColliderType.None);
        Tile pathTile = CreateTileFromTileset("Tile_Path", PathCell, Tile.ColliderType.None);
        Tile grassTile = CreateTileFromTileset("Tile_Grass", GrassCell, Tile.ColliderType.None);
        Tile wallTile = CreateTileFromTileset("Tile_Wall", WallCell, Tile.ColliderType.Grid);

        // Sử dụng mẫu nước Serene Village và tường thành nhỏ sắc nét (codeart2d)
        Tile waterTile = CreateTileFromCustomSprite("Tile_Water", VWDir + "/tile_serene_water_center.png", GenerateWaterTexture, Tile.ColliderType.None);
        Tile lavaTile = CreatePixelArtTile("Tile_Lava", GenerateLavaTexture, Tile.ColliderType.None);
        Tile mountainRockTile = CreatePixelArtTile("Tile_MountainRock", GenerateMountainRockTexture, Tile.ColliderType.None);
        Tile deepGrassTile = CreatePixelArtTile("Tile_DeepGrass", GenerateDeepGrassTexture, Tile.ColliderType.None);
        Tile castleWallTile = CreateTileFromCustomSprite("Tile_CastleWall", VWDir + "/wall_stone_timber.png", GenerateCastleWallTexture, Tile.ColliderType.Grid);

        // 2. Tạo / Cập nhật toàn bộ Prefab (Buildings, NPCs, Animals, Monsters, Gathering Nodes, Bosses)
        BuildPrefabs();

        // 3. Khởi tạo Scene mới
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 4. Tạo Grid và các Lớp Tilemap
        GameObject gridGo = new GameObject("Grid_World");
        Grid grid = gridGo.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        Tilemap groundTm = CreateTilemap(gridGo.transform, "Tilemap_Ground", OrderGround);
        Tilemap obstacleTm = CreateTilemap(gridGo.transform, "Tilemap_Obstacles", OrderObstacle);

        var rb = obstacleTm.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var tc = obstacleTm.gameObject.AddComponent<TilemapCollider2D>();
        tc.compositeOperation = Collider2D.CompositeOperation.None;

        // Vẽ nền địa hình (có dòng suối chảy từ Đại Hồ qua Thảo Nguyên) và tường thành sắc nét
        PaintGround(groundTm, dirtTile, grassTile, pathTile, waterTile, lavaTile, mountainRockTile, deepGrassTile);
        PaintWalls(obstacleTm, wallTile, castleWallTile);

        // 5. Cấu trúc Phân Khu Thế Giới
        Transform worldRoot = new GameObject("=== WORLD ENVIRONMENT ===").transform;
        Transform zone1 = new GameObject("Zone_01_Village_Oakhaven").transform;
        zone1.SetParent(worldRoot);
        Transform zone2 = new GameObject("Zone_02_Emerald_Steppe").transform;
        zone2.SetParent(worldRoot);
        Transform zone3 = new GameObject("Zone_03_Deep_Ancient_Forest").transform;
        zone3.SetParent(worldRoot);

        // ===================================================================
        // 🏰 KHU 1: LÀNG TÂN THỦ OAKHAVEN (KIẾN TRÚC SERENE VILLAGE, TƯỜNG THÀNH SẮC NÉT & NPC DI CHUYỂN TỰ DO)
        // ===================================================================
        GameObject safeZoneObj = new GameObject("SafeZone_VillageTrigger");
        safeZoneObj.transform.SetParent(zone1);
        safeZoneObj.transform.position = new Vector3((VillageMinX + VillageMaxX) * 0.5f, (VillageMinY + VillageMaxY) * 0.5f, 0);
        var safeBox = safeZoneObj.AddComponent<BoxCollider2D>();
        safeBox.isTrigger = true;
        safeBox.size = new Vector2(VillageMaxX - VillageMinX + 1, VillageMaxY - VillageMinY + 1);
        safeZoneObj.AddComponent<SafeZoneTrigger>();

        // Thiết lập hệ thống tường thành vật lý kiên cố (không thể đi qua, chỉ có thể ra vào bằng Cổng Làng)
        BuildVillageFortressWallColliders(zone1);

        // Điểm Spawn của người chơi tại Quảng Trường Làng
        Vector3 playerSpawnPos = new Vector3(-37f, 0f, 0f);
        GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.SetParent(zone1);
        spawnPoint.transform.position = playerSpawnPos;

        // Trụ đá Waystone linh thiêng & Lửa trại ấm áp tại quảng trường trung tâm
        SpawnPrefab("Waystone_Monument", new Vector3(-40f, 0.5f, 0f), zone1);
        SpawnPrefab("Prop_Animated_Campfire", new Vector3(-38.5f, -2.0f, 0f), zone1).name = "Plaza_Campfire";

        // Cây Cổ Thụ & Bồn Hoa Quảng Trường Làng
        PlaceVWProp("Village_Plaza_EmeraldOak", "tree_emerald_oak", new Vector3(-43.8f, 2.8f, 0), zone1, new Vector3(1.25f, 1.25f, 1f), new Vector2(1.0f, 0.5f), -1.2f);
        PlaceVWProp("Village_Plaza_Planter1", "planter_long_flowers", new Vector3(-42.5f, 3.8f, 0), zone1, Vector3.one, new Vector2(1.6f, 0.5f), -0.2f);
        PlaceVWProp("Village_Plaza_Planter2", "planter_long_gold", new Vector3(-35.5f, 3.8f, 0), zone1, Vector3.one, new Vector2(1.6f, 0.5f), -0.2f);
        PlaceVWProp("Village_Plaza_Lantern1", "street_lantern_wood", new Vector3(-34.5f, 2.2f, 0), zone1, Vector3.one, new Vector2(0.4f, 0.3f), -0.7f);
        PlaceVWProp("Village_Plaza_Lantern2", "street_lantern_wood", new Vector3(-34.5f, -2.2f, 0), zone1, Vector3.one, new Vector2(0.4f, 0.3f), -0.7f);

        // CỔNG LÀNG OAKHAVEN NHỎ GỌN & SẮC NÉT (East Gate tại X = -18)
        PlaceGateStructure(zone1);

        // -------------------------------------------------------------------
        // 🏠 HỆ THỐNG CỬA HÀNG & CÔNG TRÌNH KIẾN TRÚC TRONG LÀNG OAKHAVEN
        // -------------------------------------------------------------------
        // 1. Tiệm Rèn Vũ Khí Boris / Barnaby (Tây Bắc Làng: -49, 9.5)
        SpawnPrefab("Building_Blacksmith_Forge", new Vector3(-50.5f, 10.5f, 0), zone1).name = "Building_Blacksmith_Forge";
        SpawnPrefab("Building_Blacksmith_House", new Vector3(-45.2f, 10.8f, 0), zone1).name = "Building_Blacksmith_Workshop";
        PlaceVWProp("Blacksmith_Sign", "signpost_wood_town", new Vector3(-47.5f, 7.6f, 0), zone1, Vector3.one, new Vector2(0.5f, 0.35f), -0.3f);
        PlaceVWProp("Blacksmith_LogPile", "serene_wood_log_pile", new Vector3(-53.2f, 8.2f, 0), zone1, Vector3.one, new Vector2(0.7f, 0.4f), -0.2f);
        PlaceVWProp("Blacksmith_OreSample", "rock_ore_node", new Vector3(-52.2f, 7.8f, 0), zone1, new Vector3(0.85f, 0.85f, 1f), new Vector2(0.7f, 0.4f), -0.2f);

        // 2. Dược Quán Luyện Dược Elena (Tây Nam Làng: -49, -8.5)
        SpawnPrefab("Building_Apothecary_Elena", new Vector3(-49.0f, -7.8f, 0), zone1).name = "Building_Apothecary_Elena";
        SpawnPrefab("Building_Herbalist_Cottage", new Vector3(-54.5f, -8.2f, 0), zone1).name = "Building_Herbalist_Cottage";
        PlaceVWProp("Apothecary_Sign", "signpost_wood_town", new Vector3(-46.5f, -10.2f, 0), zone1, Vector3.one, new Vector2(0.5f, 0.35f), -0.3f);
        PlaceVWProp("Apothecary_Flowerbox1", "flowerbox_blue_gold", new Vector3(-51.2f, -10.2f, 0), zone1, Vector3.one, new Vector2(1.4f, 0.4f), -0.2f);
        PlaceVWProp("Apothecary_Flowerbox2", "flowerbox_red", new Vector3(-47.2f, -10.2f, 0), zone1, Vector3.one, new Vector2(0.8f, 0.4f), -0.2f);
        PlaceVWProp("Apothecary_HerbPatch1", "serene_herb_red_patch", new Vector3(-52.5f, -11.5f, 0), zone1, Vector3.one, Vector2.zero, -0.2f, false);
        PlaceVWProp("Apothecary_HerbPatch2", "serene_herb_sprout_patch", new Vector3(-49.5f, -11.5f, 0), zone1, Vector3.one, Vector2.zero, -0.2f, false);

        // 3. Hiệp Hội Mạo Hiểm Giả Carlos (Đông Bắc Làng: -27, 9.5)
        SpawnPrefab("Building_HunterGuild_Carlos", new Vector3(-27.0f, 10.5f, 0), zone1).name = "Building_HunterGuild_Carlos";
        SpawnPrefab("Building_Guild_Barracks", new Vector3(-32.5f, 10.8f, 0), zone1).name = "Building_Guild_Barracks";
        PlaceVWProp("Guild_Sign", "signpost_wood_east", new Vector3(-24.5f, 7.8f, 0), zone1, Vector3.one, new Vector2(0.5f, 0.35f), -0.3f);
        PlaceVWProp("Guild_Mailbox", "mailbox_red", new Vector3(-29.5f, 7.8f, 0), zone1, Vector3.one, new Vector2(0.5f, 0.4f), -0.6f);
        PlaceVWProp("Guild_Fence", "fence_wood_horizontal", new Vector3(-23.0f, 8.2f, 0), zone1, Vector3.one, new Vector2(2.6f, 0.4f), -0.2f);

        // 4. Cửa Tiệm Tạp Hóa & Quán Trọ Oakhaven (Đông Nam Làng: -27, -8.5)
        SpawnPrefab("Building_GeneralStore", new Vector3(-27.0f, -7.8f, 0), zone1).name = "Building_GeneralStore";
        SpawnPrefab("Building_Village_Tavern", new Vector3(-32.8f, -8.0f, 0), zone1).name = "Building_Village_Tavern";
        PlaceVWProp("Store_Sign", "signpost_wood_town", new Vector3(-24.5f, -10.2f, 0), zone1, Vector3.one, new Vector2(0.5f, 0.35f), -0.3f);
        PlaceVWProp("Store_Planter", "planter_long_gold", new Vector3(-29.2f, -10.2f, 0), zone1, Vector3.one, new Vector2(1.6f, 0.4f), -0.2f);

        // 5. Khu Bảo Tồn Thú Cưng Kaelen & Nhà Dân Oakhaven
        SpawnPrefab("Building_PetSanctuary", new Vector3(-39.0f, 11.2f, 0), zone1).name = "Building_PetSanctuary_Kaelen";
        PlaceVWProp("PetSanctuary_Fence1", "fence_wood_gate_row", new Vector3(-39.0f, 8.5f, 0), zone1, Vector3.one, new Vector2(2.6f, 0.35f), -0.2f);
        SpawnPrefab("Animal_Rabbit_White", new Vector3(-37.5f, 9.2f, 0), zone1).name = "Sanctuary_Pet_Rabbit";

        SpawnPrefab("Building_Village_Residence", new Vector3(-39.0f, -11.0f, 0), zone1).name = "Building_Elder_Residence";
        PlaceVWProp("ElderHouse_Flowerbox", "flowerbox_blue_gold", new Vector3(-39.0f, -13.2f, 0), zone1, Vector3.one, new Vector2(1.5f, 0.4f), -0.2f);

        // -------------------------------------------------------------------
        // 🚶 CÁC NHÂN VẬT NPC DI CHUYỂN TỰ DO BÊN TRONG LÀNG OAKHAVEN
        // -------------------------------------------------------------------
        SpawnPrefab("NPC_VillageElder", new Vector3(-21.2f, -0.8f, 0), zone1).name = "NPC_TruongLang_Eldrin";
        SpawnPrefab("NPC_Blacksmith_Barnaby", new Vector3(-47.8f, 7.2f, 0), zone1).name = "NPC_ThoRen_Boris";
        SpawnPrefab("NPC_Alchemist_Elena", new Vector3(-48.5f, -10.8f, 0), zone1).name = "NPC_DuocSi_Elena";
        SpawnPrefab("NPC_GuildMaster_Carlos", new Vector3(-26.8f, 7.2f, 0), zone1).name = "NPC_HoiTruong_Carlos";
        var kaelenGo = SpawnPrefab("NPC_PetMaster_Kaelen", new Vector3(-39.0f, 7.5f, 0), zone1);
        kaelenGo.name = "NPC_QuanThu_Kaelen";
        var falconGo = SpawnPrefab("Pet_Falcon", new Vector3(-39.25f, 7.58f, 0), zone1);
        falconGo.name = "Pet_Falcon_Companion";
        var fCtrl = falconGo.GetComponent<FalconPetController2D>();
        if (fCtrl != null)
        {
            fCtrl.kaelenTransform = kaelenGo.transform;
            var perch = kaelenGo.transform.Find("Socket_FalconPerch");
            if (perch != null) fCtrl.perchPoint = perch;
        }
        SpawnPrefab("NPC_Merchant_Lina", new Vector3(-27.2f, -10.8f, 0), zone1).name = "NPC_ThuongNhan_Lina";
        SpawnPrefab("NPC_Villager_Wanderer", new Vector3(-36.5f, 1.5f, 0), zone1).name = "NPC_CuDan_Oakhaven_01";
        SpawnPrefab("NPC_Villager_Wanderer", new Vector3(-42.0f, -3.5f, 0), zone1).name = "NPC_CuDan_Oakhaven_02";

        // Cây xanh Serene Village, bụi hoa & cỏ mini trong Làng
        PlaceSereneTreeCluster("Village_SereneGrove_NW", new Vector3(-58.0f, 14.5f, 0), zone1, "tree_emerald_oak", "tree_young_sapling");
        PlaceSereneTreeCluster("Village_SereneGrove_SW", new Vector3(-58.0f, -14.5f, 0), zone1, "tree_forest_pine", "tree_young_sapling");
        PlaceSereneTreeCluster("Village_SereneGrove_NE", new Vector3(-21.5f, 14.5f, 0), zone1, "tree_bright_lime", "tree_young_sapling");
        PlaceSereneTreeCluster("Village_SereneGrove_SE", new Vector3(-21.5f, -14.5f, 0), zone1, "tree_emerald_oak", "tree_young_sapling");

        ScatterMiniGrassAndFlowers(zone1, new Rect(-58f, -16f, 38f, 32f), 28, "VillageGrass");

        // ===================================================================
        // 🌾 KHU 2: CON ĐƯỜNG HUYẾT MẠCH & THẢO NGUYÊN XANH (EMERALD STEPPE)
        // ===================================================================
        PlaceVWProp("Signpost_SouthRoad", "signpost_wood_east", new Vector3(-9.5f, -1.2f, 0), zone2, Vector3.one, new Vector2(0.5f, 0.35f), -0.3f);
        PlaceVWProp("Highway_Lantern_1", "street_lantern_wood", new Vector3(-13.8f, -0.5f, 0), zone2, Vector3.one, new Vector2(0.4f, 0.3f), -0.7f);
        PlaceVWProp("Highway_Lantern_2", "street_lantern_wood", new Vector3(-9.2f, -8.0f, 0), zone2, Vector3.one, new Vector2(0.4f, 0.3f), -0.7f);

        // Cầu gỗ bắc qua dòng suối nối Hồ Thảo Nguyên và Đại Hồ Rừng Sâu
        PlaceVWProp("Steppe_Stream_Bridge_West", "bridge_wood_horizontal", new Vector3(16.0f, -14.0f, 0), zone2, new Vector3(1.2f, 1.1f, 1f), Vector2.zero, -0.5f, false);
        PlaceVWProp("Steppe_Stream_Bridge_East", "bridge_wood_horizontal", new Vector3(23.5f, -14.8f, 0), zone2, new Vector3(1.2f, 1.1f, 1f), Vector2.zero, -0.5f, false);

        // -------------------------------------------------------------------
        // 💧 HỒ NƯỚC & SUỐI MƠ THẢO NGUYÊN (CÓ SÓNG NƯỚC ĐỘNG & SUỐI THU HOẠCH)
        // -------------------------------------------------------------------
        SpawnWaterWavesInPond(zone2, new Vector3(10.0f, -13.0f, 0), 4, 2.8f, "SteppePond_Wave");
        SpawnWaterWavesInPond(zone2, new Vector3(3.5f, -9.0f, 0), 2, 1.2f, "SteppePuddleNW_Wave");
        SpawnWaterWavesInPond(zone2, new Vector3(19.5f, -14.5f, 0), 3, 2.2f, "SteppeStream_Wave");

        // Cảnh quan ven hồ & suối: Đá rêu phong (mossy_rock), đá cuội nhỏ, hoa ven hồ
        PlacePondShoreCluster(zone2, new Vector3(5.2f, -8.5f, 0), "SteppePond_Shore_NW");
        PlacePondShoreCluster(zone2, new Vector3(15.0f, -10.8f, 0), "SteppePond_Shore_NE");
        PlacePondShoreCluster(zone2, new Vector3(6.2f, -17.2f, 0), "SteppePond_Shore_SW");
        PlacePondShoreCluster(zone2, new Vector3(14.2f, -17.8f, 0), "SteppePond_Shore_SE");
        PlaceVWProp("SteppePond_EmeraldTree", "tree_emerald_oak", new Vector3(16.8f, -9.2f, 0), zone2, new Vector3(1.3f, 1.3f, 1f), new Vector2(1.0f, 0.5f), -1.2f);

        // -------------------------------------------------------------------
        // 🦌 ĐỘNG VẬT CÓ SẴN & QUÁI VẬT TRÊN THẢO NGUYÊN XANH (ANIMALS & MONSTERS)
        // -------------------------------------------------------------------
        // 1. Động vật hoang dã có sẵn (Hươu Ciervo, Thỏ Conejo, Sói Lobo) di chuyển tự nhiên trên thảo nguyên
        SpawnPrefab("Animal_Deer_Stag", new Vector3(0.5f, 9.5f, 0), zone2).name = "Steppe_Deer_01";
        SpawnPrefab("Animal_Deer_Stag", new Vector3(13.5f, 8.0f, 0), zone2).name = "Steppe_Deer_02";
        SpawnPrefab("Animal_Deer_Stag", new Vector3(6.5f, -6.5f, 0), zone2).name = "Steppe_Deer_Pond";

        SpawnPrefab("Animal_Rabbit_Brown", new Vector3(-6.0f, 5.5f, 0), zone2).name = "Steppe_Rabbit_Brown_01";
        SpawnPrefab("Animal_Rabbit_Brown", new Vector3(4.5f, 2.0f, 0), zone2).name = "Steppe_Rabbit_Brown_02";
        SpawnPrefab("Animal_Rabbit_White", new Vector3(-3.5f, -3.5f, 0), zone2).name = "Steppe_Rabbit_White_01";
        SpawnPrefab("Animal_Rabbit_White", new Vector3(11.5f, -4.5f, 0), zone2).name = "Steppe_Rabbit_White_02";

        // 2. Quái vật Thảo Nguyên (Thủy Slime, Chuột Chũi, Heo Rừng Đất, Cua Đá Suối, Sói Xám Thảo Nguyên, Búp Cổ Thụ)
        SpawnPrefab("Enemy_Water_Slime", new Vector3(8.5f, -12.5f, 0), zone2).name = "WaterSlime_Steppe_01";
        SpawnPrefab("Enemy_Water_Slime", new Vector3(12.2f, -13.8f, 0), zone2).name = "WaterSlime_Steppe_02";
        SpawnPrefab("Enemy_Water_Slime", new Vector3(4.0f, -8.8f, 0), zone2).name = "WaterSlime_Puddle_01";

        SpawnPrefab("Enemy_Steppe_Crab", new Vector3(6.8f, -15.8f, 0), zone2).name = "Enemy_Steppe_Crab_01";
        SpawnPrefab("Enemy_Steppe_Crab", new Vector3(14.8f, -15.2f, 0), zone2).name = "Enemy_Steppe_Crab_02";

        SpawnPrefab("Enemy_Mole", new Vector3(-4.0f, 3.0f, 0), zone2).name = "Enemy_Mole_01";
        SpawnPrefab("Enemy_Mole", new Vector3(2.0f, 7.0f, 0), zone2).name = "Enemy_Mole_02";
        SpawnPrefab("Enemy_Mole", new Vector3(8.0f, 5.0f, 0), zone2).name = "Enemy_Mole_03";
        SpawnPrefab("Enemy_Mole", new Vector3(-3.0f, -8.0f, 0), zone2).name = "Enemy_Mole_04";

        SpawnPrefab("Enemy_Steppe_Boar", new Vector3(13.5f, 2.5f, 0), zone2).name = "Enemy_Steppe_Boar_01";
        SpawnPrefab("Enemy_Steppe_Boar", new Vector3(5.5f, -21.0f, 0), zone2).name = "Enemy_Steppe_Boar_02";

        SpawnPrefab("Enemy_Steppe_Wolf", new Vector3(18.0f, 7.5f, 0), zone2).name = "Enemy_Steppe_Wolf_01";
        SpawnPrefab("Enemy_Steppe_Wolf", new Vector3(17.5f, -4.5f, 0), zone2).name = "Enemy_Steppe_Wolf_02";

        SpawnPrefab("Pet_Treant_Sprout", new Vector3(19.5f, 12.0f, 0), zone2).name = "Enemy_Steppe_Sprout_01";

        // 🐾 5 LINH THÚ HOANG DÃ XUẤT HIỆN TRÊN BẢN ĐỒ THỨ 2 (ZONE 2: THẢO NGUYÊN XANH)
        SpawnWildPetInZone("Pet_CaDuoiHuKhong", new Vector3(8.0f, -14.0f, 0), zone2, "WildPet_CaDuoiHuKhong");
        SpawnWildPetInZone("Pet_DaMinhTucDang", new Vector3(14.5f, 13.0f, 0), zone2, "WildPet_DaMinhTucDang");
        SpawnWildPetInZone("Pet_HuDocNamNhan", new Vector3(2.5f, -8.0f, 0), zone2, "WildPet_HuDocNamNhan");
        SpawnWildPetInZone("Pet_NgaiThaoVuCau", new Vector3(6.0f, 4.0f, 0), zone2, "WildPet_NgaiThaoVuCau");
        SpawnWildPetInZone("Pet_ThucKimNeBa", new Vector3(11.0f, -5.5f, 0), zone2, "WildPet_ThucKimNeBa");

        // Thủ Lĩnh Chuột Chũi Alpha (Lv 5) trấn giữ chặng cuối thảo nguyên
        SpawnPrefab("Enemy_Mole_Leader", new Vector3(21.0f, 1.5f, 0), zone2).name = "Enemy_Mole_Alpha_Leader";

        // -------------------------------------------------------------------
        // 🌿 ĐIỂM THU THẬP TÀI NGUYÊN (GATHERING NODES: THẢO DƯỢC & KHOÁNG SẢN) TRÊN THẢO NGUYÊN
        // -------------------------------------------------------------------
        // Bụi Thảo Mộc Xanh & Bụi Dược Thảo Tinh Khiết (vẽ bằng codeart2d)
        SpawnPrefab("GatheringNode_Bush", new Vector3(-1.5f, 5.2f, 0), zone2).name = "GatheringNode_Herb_01";
        SpawnPrefab("GatheringNode_Bush", new Vector3(11.5f, 8.5f, 0), zone2).name = "GatheringNode_Herb_02";
        SpawnPrefab("GatheringNode_Bush", new Vector3(-2.5f, -14.5f, 0), zone2).name = "GatheringNode_Herb_03";
        SpawnPrefab("GatheringNode_Bush", new Vector3(8.5f, -7.2f, 0), zone2).name = "GatheringNode_Herb_04";
        SpawnPrefab("GatheringNode_MoonHerb", new Vector3(15.5f, 14.2f, 0), zone2).name = "GatheringNode_MoonHerb_01";
        SpawnPrefab("GatheringNode_CrimsonHerb", new Vector3(2.5f, -18.5f, 0), zone2).name = "GatheringNode_CrimsonHerb_01";

        // Mỏ Quặng Đồng Thô, Mỏ Quặng Sắt Thô & Mỏ Đá Tinh Thể (kết hợp codeart2d & Rocks pack)
        SpawnPrefab("GatheringNode_CopperOre", new Vector3(6.8f, -5.2f, 0), zone2).name = "GatheringNode_Copper_01";
        SpawnPrefab("GatheringNode_CopperOre", new Vector3(-5.5f, -12.0f, 0), zone2).name = "GatheringNode_Copper_02";
        SpawnPrefab("GatheringNode_CopperOre", new Vector3(4.5f, 14.5f, 0), zone2).name = "GatheringNode_Copper_03";
        SpawnPrefab("GatheringNode_Ore", new Vector3(19.0f, -6.8f, 0), zone2).name = "GatheringNode_Iron_01";
        SpawnPrefab("GatheringNode_Ore", new Vector3(20.5f, 9.5f, 0), zone2).name = "GatheringNode_Iron_02";
        SpawnPrefab("GatheringNode_CrystalOre", new Vector3(16.2f, -19.5f, 0), zone2).name = "GatheringNode_Crystal_01";

        // Cụm Cây Serene Village, Đá tảng môi trường & Bụi cỏ mini rải khắp Thảo Nguyên
        PlaceSereneTreeCluster("Steppe_SereneGrove_NW", new Vector3(3.0f, 13.0f, 0), zone2, "tree_emerald_oak", "tree_bright_lime");
        PlaceSereneTreeCluster("Steppe_SereneGrove_NE", new Vector3(16.0f, 12.5f, 0), zone2, "tree_forest_pine", "tree_young_sapling");
        PlaceSereneTreeCluster("Steppe_SereneGrove_SW", new Vector3(-3.5f, -22.0f, 0), zone2, "tree_bright_lime", "tree_emerald_oak");
        PlaceVWProp("Steppe_Tree_Center", "tree_emerald_oak", new Vector3(9.0f, 3.2f, 0), zone2, new Vector3(1.2f, 1.2f, 1f), new Vector2(0.9f, 0.45f), -1.1f);

        // Rải các mẫu đá môi trường từ bộ Rocks (Boulder, Mossy Rock, Cracked Rock, Flat Stone)
        PlaceVWProp("Steppe_MossyRock_1", "rock_mossy_medium", new Vector3(1.2f, 11.2f, 0), zone2, Vector3.one, new Vector2(1.1f, 0.55f), -0.3f);
        PlaceVWProp("Steppe_MossyRock_2", "rock_mossy_medium", new Vector3(13.2f, -8.5f, 0), zone2, Vector3.one, new Vector2(1.1f, 0.55f), -0.3f);
        PlaceVWProp("Steppe_Boulder_1", "rock_boulder_large", new Vector3(21.5f, -10.5f, 0), zone2, Vector3.one, new Vector2(1.4f, 0.7f), -0.4f);
        PlaceVWProp("Steppe_CrackedRock_1", "rock_cracked_medium", new Vector3(-6.2f, -16.5f, 0), zone2, Vector3.one, new Vector2(1.0f, 0.5f), -0.3f);
        PlaceVWProp("Steppe_FlatStone_1", "rock_flat_stepping", new Vector3(0.5f, -1.5f, 0), zone2, Vector3.one, Vector2.zero, -0.2f, false);
        PlaceVWProp("Steppe_FlatStone_2", "rock_flat_stepping", new Vector3(10.5f, 0.8f, 0), zone2, Vector3.one, Vector2.zero, -0.2f, false);

        ScatterMiniGrassAndFlowers(zone2, new Rect(-8f, -26f, 31f, 52f), 55, "SteppeGrass");

        // ===================================================================
        // 🌲 KHU 3: ĐẠI RỪNG RẬM CẤP CAO & 4 VÙNG BOSS (X: 25 đến 95)
        // ===================================================================
        for (int y = -28; y <= 28; y += 3)
        {
            if (y >= -4 && y <= 0) continue; // Chừa Cổng vào Rừng Sâu
            if (y >= -17 && y <= -13) continue; // Chừa lòng suối chảy từ Đại Hồ sang Thảo Nguyên
            float xOffset = (Mathf.Abs(y) % 2 == 0) ? 0f : 1.2f;
            string treeSpr = (Mathf.Abs(y) % 2 == 0) ? "tree_deep_teal" : "tree_forest_pine";
            PlaceVWProp("Border_ForestTree_" + y, treeSpr, new Vector3(ForestBorderX + xOffset, y, 0), zone3, new Vector3(1.25f, 1.25f, 1f), new Vector2(1.0f, 0.5f), -1.2f);
            if (y % 6 == 0)
            {
                PlaceVWProp("Border_Rock_" + y, "rock_mossy_medium", new Vector3(ForestBorderX + xOffset - 1.2f, y - 1f, 0), zone3, new Vector3(0.85f, 0.85f, 1f), new Vector2(0.9f, 0.45f), -0.25f);
            }
        }

        // Cổng vào Rừng Sâu
        PlaceVWProp("ForestGate_NorthSentinel", "tree_deep_teal", new Vector3(ForestBorderX, 2.0f, 0), zone3, new Vector3(1.5f, 1.5f, 1f), new Vector2(1.2f, 0.6f), -1.4f);
        PlaceVWProp("ForestGate_SouthSentinel", "tree_deep_teal", new Vector3(ForestBorderX, -5.5f, 0), zone3, new Vector3(1.5f, 1.5f, 1f), new Vector2(1.2f, 0.6f), -1.4f);
        PlaceVWProp("Signpost_ForestDanger", "signpost_wood_east", new Vector3(ForestBorderX - 1.8f, 0.5f, 0), zone3, Vector3.one, new Vector2(0.5f, 0.35f), -0.3f);

        // Động vật & Quái Rừng Sâu (Sói Trắng, Hươu Tinh Khôi, Búp Cổ Thụ)
        SpawnPrefab("Pet_Treant_Sprout", new Vector3(29f, 2f, 0), zone3).name = "Forest_TreantSprout_01";
        SpawnPrefab("Enemy_Forest_WhiteWolf", new Vector3(33f, 4.5f, 0), zone3).name = "Forest_WhiteWolf_01";
        SpawnPrefab("Animal_Deer_Stag", new Vector3(36f, -3.5f, 0), zone3).name = "Forest_PureDeer_01";
        SpawnPrefab("GatheringNode_MoonHerb", new Vector3(31f, -6.5f, 0), zone3).name = "Forest_MoonHerb_01";
        SpawnPrefab("GatheringNode_CrystalOre", new Vector3(58f, -3.0f, 0), zone3).name = "Forest_CrystalOre_01";

        // -------------------------------------------------------------------
        // 🔥 VÙNG BOSS 1: NÚI LỬA DUNG NHAM (CALDERA OF FLAMES - Tây Nam Rừng)
        // -------------------------------------------------------------------
        Transform subVolcano = new GameObject("SubZone_01_Volcano").transform;
        subVolcano.SetParent(zone3);
        PlaceProp("Sign_Volcano", "sign", new Vector3(VolcanoMinX + 1f, VolcanoMaxY + 1f, 0), subVolcano, Vector3.one, new Vector2(0.6f, 0.4f), -0.4f);
        SpawnPrefab("Boss_Volcano_DemonApe", new Vector3((VolcanoMinX + VolcanoMaxX) * 0.5f, (VolcanoMinY + VolcanoMaxY) * 0.5f, 0), subVolcano).name = "Boss_Volcano_DemonApe";
        PlaceProp("Volcano_Monument_1", "rock-monument", new Vector3(VolcanoMinX + 3f, VolcanoMinY + 4f, 0), subVolcano, Vector3.one, new Vector2(2f, 0.8f), -1.2f);
        PlaceProp("Volcano_Monument_2", "rock-monument", new Vector3(VolcanoMaxX - 3f, VolcanoMaxY - 4f, 0), subVolcano, Vector3.one, new Vector2(2f, 0.8f), -1.2f);
        PlaceVWProp("Volcano_DarkRock_1", "rock_dark_ore", new Vector3(VolcanoMinX + 6f, VolcanoMinY + 5f, 0), subVolcano, Vector3.one, new Vector2(0.9f, 0.5f), -0.25f);
        PlaceVWProp("Volcano_DarkRock_2", "rock_cliff_large", new Vector3(VolcanoMaxX - 6f, VolcanoMaxY - 5f, 0), subVolcano, Vector3.one, new Vector2(1.4f, 0.7f), -0.35f);
        SpawnPrefab("GatheringNode_Ore", new Vector3(VolcanoMinX + 4f, VolcanoMaxY - 3f, 0), subVolcano).name = "Volcano_IronVein_01";

        // -------------------------------------------------------------------
        // 💧 VÙNG BOSS 2: ĐẠI HỒ NƯỚC SÂU KHU RỪNG (GREAT AZURE LAKE - Đông Nam Rừng)
        // Vẽ lại hồ nước khu rừng với thác đổ, cầu gỗ, sóng nước động, đá tinh thể & cây Serene Village
        // -------------------------------------------------------------------
        Transform subLake = new GameObject("SubZone_02_GreatLake").transform;
        subLake.SetParent(zone3);
        PlaceVWProp("Sign_GreatLake", "signpost_wood_east", new Vector3(LakeMinX + 1f, LakeMaxY + 1f, 0), subLake, Vector3.one, new Vector2(0.5f, 0.35f), -0.3f);

        // 2 Thác nước cổ linh đổ vào Đại Hồ từ vách đá phía Bắc
        SpawnPrefab("Waterfall", new Vector3(LakeMaxX - 6f, LakeMaxY - 0.5f, 0), subLake).name = "GreatLake_Waterfall_East";
        SpawnPrefab("Waterfall", new Vector3(LakeMinX + 8f, LakeMaxY - 0.5f, 0), subLake).name = "GreatLake_Waterfall_West";

        // Cầu gỗ bắc qua cửa suối phía Tây Đại Hồ & Bến gỗ ngắm cảnh phía Bắc
        PlaceVWProp("GreatLake_Bridge_West", "bridge_wood_horizontal", new Vector3(LakeMinX + 1.5f, -15.5f, 0), subLake, new Vector3(1.3f, 1.2f, 1f), Vector2.zero, -0.5f, false);
        PlaceVWProp("GreatLake_Bridge_NorthDock", "bridge_wood_vertical", new Vector3((LakeMinX + LakeMaxX) * 0.5f, LakeMaxY - 2.2f, 0), subLake, new Vector3(1.1f, 1.1f, 1f), Vector2.zero, -0.5f, false);

        // Sóng nước lăn tăn động (water_waves_16x16) trên mặt Đại Hồ khu rừng
        SpawnWaterWavesInPond(subLake, new Vector3((LakeMinX + LakeMaxX) * 0.5f, (LakeMinY + LakeMaxY) * 0.5f, 0), 8, 6.5f, "GreatLake_Wave");

        // Boss 2: Thủy Tinh Linh Vương (Lv 22 - Hệ Thủy)
        SpawnPrefab("Boss_Lake_Leviathan", new Vector3((LakeMinX + LakeMaxX) * 0.5f, (LakeMinY + LakeMaxY) * 0.5f, 0), subLake).name = "Boss_Lake_Leviathan";

        // Cảnh quan bờ Đại Hồ: Cây cổ thụ soi bóng, đá rêu phong, mỏ tinh thể lam ngọc và thảo dược ánh trăng
        PlacePondShoreCluster(subLake, new Vector3(LakeMinX + 3.5f, LakeMinY + 4.5f, 0), "GreatLake_Shore_SW");
        PlacePondShoreCluster(subLake, new Vector3(LakeMaxX - 4.5f, LakeMinY + 5.0f, 0), "GreatLake_Shore_SE");
        PlacePondShoreCluster(subLake, new Vector3(LakeMinX + 4.5f, LakeMaxY - 3.5f, 0), "GreatLake_Shore_NW");
        PlacePondShoreCluster(subLake, new Vector3(LakeMaxX - 4.0f, LakeMaxY - 4.0f, 0), "GreatLake_Shore_NE");

        PlaceVWProp("GreatLake_EmeraldTree_1", "tree_deep_teal", new Vector3(LakeMinX + 2.5f, LakeMaxY - 2.0f, 0), subLake, new Vector3(1.35f, 1.35f, 1f), new Vector2(1.0f, 0.5f), -1.2f);
        PlaceVWProp("GreatLake_EmeraldTree_2", "tree_emerald_oak", new Vector3(LakeMaxX - 2.5f, LakeMinY + 3.0f, 0), subLake, new Vector3(1.35f, 1.35f, 1f), new Vector2(1.0f, 0.5f), -1.2f);
        SpawnPrefab("GatheringNode_CrystalOre", new Vector3(LakeMinX + 2.8f, LakeMinY + 7.5f, 0), subLake).name = "GreatLake_Crystal_01";
        SpawnPrefab("GatheringNode_MoonHerb", new Vector3(LakeMaxX - 3.2f, LakeMinY + 8.5f, 0), subLake).name = "GreatLake_MoonHerb_01";

        // -------------------------------------------------------------------
        // 🌳 VÙNG BOSS 3: CÂY CỔ THỤ KHỔNG LỒ (SACRED TITAN TREE - Tây Bắc Rừng)
        // -------------------------------------------------------------------
        Transform subTree = new GameObject("SubZone_03_AncientTree").transform;
        subTree.SetParent(zone3);
        PlaceProp("Sign_AncientTree", "sign", new Vector3(TreeMinX + 1f, TreeMinY - 1f, 0), subTree, Vector3.one, new Vector2(0.6f, 0.4f), -0.4f);
        SpawnPrefab("Boss_Treant_Ancient", new Vector3((TreeMinX + TreeMaxX) * 0.5f, (TreeMinY + TreeMaxY) * 0.5f, 0), subTree).name = "Boss_Treant_Ancient";
        PlaceVWProp("AncientDomain_Giant_1", "tree_emerald_oak", new Vector3(TreeMinX + 4f, TreeMaxY - 4f, 0), subTree, new Vector3(1.6f, 1.6f, 1f), new Vector2(1.2f, 0.6f), -1.4f);
        PlaceVWProp("AncientDomain_Giant_2", "tree_deep_teal", new Vector3(TreeMaxX - 4f, TreeMaxY - 4f, 0), subTree, new Vector3(1.55f, 1.55f, 1f), new Vector2(1.2f, 0.6f), -1.4f);
        PlaceVWProp("AncientDomain_MossyRock", "rock_mossy_medium", new Vector3((TreeMinX + TreeMaxX) * 0.5f + 6.5f, (TreeMinY + TreeMaxY) * 0.5f - 3f, 0), subTree, Vector3.one, new Vector2(1.1f, 0.5f), -0.3f);
        SpawnPrefab("GatheringNode_Bush", new Vector3(TreeMinX + 5.5f, TreeMinY + 3.5f, 0), subTree).name = "AncientDomain_Herb_01";
        SpawnPrefab("GatheringNode_MoonHerb", new Vector3(TreeMaxX - 5.5f, TreeMinY + 3.5f, 0), subTree).name = "AncientDomain_MoonHerb_01";

        // -------------------------------------------------------------------
        // ⚡ VÙNG BOSS 4: ĐỈNH NÚI CAO MÂY GIÓ (STORM SUMMIT PEAK - Đông Bắc Rừng)
        // -------------------------------------------------------------------
        Transform subPeak = new GameObject("SubZone_04_MountainPeak").transform;
        subPeak.SetParent(zone3);
        PlaceProp("Sign_StormPeak", "sign", new Vector3(PeakMinX + 1f, PeakMinY - 1f, 0), subPeak, Vector3.one, new Vector2(0.6f, 0.4f), -0.4f);
        SpawnPrefab("Boss_Summit_WolfKing", new Vector3((PeakMinX + PeakMaxX) * 0.5f, (PeakMinY + PeakMaxY) * 0.5f, 0), subPeak).name = "Boss_Summit_WolfKing";
        SpawnPrefab("Enemy_Forest_WhiteWolf", new Vector3(PeakMinX + 6f, PeakMinY + 6f, 0), subPeak).name = "Peak_Guard_WhiteWolf_01";
        SpawnPrefab("Enemy_Forest_WhiteWolf", new Vector3(PeakMaxX - 6f, PeakMinY + 6f, 0), subPeak).name = "Peak_Guard_WhiteWolf_02";
        PlaceProp("Peak_Monument_1", "rock-monument", new Vector3(PeakMinX + 3.5f, PeakMaxY - 3.5f, 0), subPeak, Vector3.one, new Vector2(2f, 0.8f), -1.2f);
        PlaceProp("Peak_Monument_2", "rock-monument", new Vector3(PeakMaxX - 3.5f, PeakMaxY - 3.5f, 0), subPeak, Vector3.one, new Vector2(2f, 0.8f), -1.2f);
        PlaceVWProp("Peak_CliffRock_1", "rock_cliff_large", new Vector3(PeakMinX + 7f, PeakMinY + 3.5f, 0), subPeak, Vector3.one, new Vector2(1.4f, 0.7f), -0.35f);
        PlaceVWProp("Peak_CrystalNode", "rock_crystal_node", new Vector3(PeakMaxX - 7f, PeakMinY + 4.0f, 0), subPeak, Vector3.one, new Vector2(1.0f, 0.5f), -0.25f);

        ScatterMiniGrassAndFlowers(zone3, new Rect(27f, -28f, 64f, 56f), 40, "ForestGrass");

        // ===================================================================
        // 6. PLAYER, CAMERA VÀ NHẠC NỀN
        // ===================================================================
        GameObject player = SpawnPrefab("Player_Novice_Tier1", playerSpawnPos, null);
        if (player != null)
        {
            player.tag = "Player";
            player.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.CharacterScale = 1.5f;
        }

        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.08f, 0.12f);
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(playerSpawnPos.x, playerSpawnPos.y, -10f);

        var follow = camGo.AddComponent<CameraFollow2D>();
        if (player != null) follow.target = player.transform;
        follow.smoothSpeed = 0.15f;
        follow.useBounds = true;
        float halfH = cam.orthographicSize;
        float halfW = halfH * 16f / 9f;
        follow.minBounds = new Vector2(MapMinX + halfW, MapMinY + halfH);
        follow.maxBounds = new Vector2(MapMaxX - halfW, MapMaxY - halfH);

        GameObject bgm = new GameObject("BGM");
        AudioSource src = bgm.AddComponent<AudioSource>();
        src.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        src.loop = true;
        src.playOnAwake = true;
        src.volume = 0.5f;

        EditorSceneManager.SaveScene(scene, targetScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"<color=green>🎉 [ForestMapGenerator] ĐÃ KIẾN THIẾT THÀNH CÔNG BẢN ĐỒ THẾ GIỚI LUMINA V3 TẠI: {targetScenePath}!</color>");
    }

    // =======================================================================
    // CỔNG LÀNG & HÀNG RÀO KIẾN TRÚC NHỎ GỌN SẮC NÉT
    // =======================================================================
    static void PlaceGateStructure(Transform zone1)
    {
        GameObject gateRoot = new GameObject("Village_EastGate_Architecture");
        gateRoot.transform.SetParent(zone1);
        gateRoot.transform.position = new Vector3(VillageMaxX, 0f, 0f);

        // 2 Trụ cổng mái ngói đỏ nhỏ gọn sắc nét (vẽ bằng codeart2d 16x24 px @ PPU 16 = rộng 1m, cao 1.5m)
        PlaceVWProp("GatePillar_North", "village_gate_pillar", new Vector3(VillageMaxX, VillageGateYMax + 0.65f, 0), gateRoot.transform, new Vector3(1.1f, 1.1f, 1f), new Vector2(0.8f, 0.6f), -0.45f);
        PlaceVWProp("GatePillar_South", "village_gate_pillar", new Vector3(VillageMaxX, VillageGateYMin - 0.65f, 0), gateRoot.transform, new Vector3(1.1f, 1.1f, 1f), new Vector2(0.8f, 0.6f), -0.45f);

        // Đèn lồng gỗ & biển báo hai bên cổng làng
        PlaceVWProp("GateLantern_North", "street_lantern_wood", new Vector3(VillageMaxX - 1.1f, VillageGateYMax + 0.5f, 0), gateRoot.transform, Vector3.one, new Vector2(0.4f, 0.3f), -0.7f);
        PlaceVWProp("GateLantern_South", "street_lantern_wood", new Vector3(VillageMaxX - 1.1f, VillageGateYMin - 0.5f, 0), gateRoot.transform, Vector3.one, new Vector2(0.4f, 0.3f), -0.7f);
        PlaceVWProp("Signpost_VillageExit", "signpost_wood_east", new Vector3(VillageMaxX - 1.5f, VillageGateYMax + 1.3f, 0), gateRoot.transform, Vector3.one, new Vector2(0.5f, 0.35f), -0.3f);

        // Khóm hoa hồng & đá cuội trang trí chân cổng
        PlaceVWProp("GateBush_North", "bush_rose_red", new Vector3(VillageMaxX - 0.8f, VillageGateYMax + 1.5f, 0), gateRoot.transform, Vector3.one, new Vector2(0.6f, 0.4f), -0.2f);
        PlaceVWProp("GateBush_South", "bush_flower_gold", new Vector3(VillageMaxX - 0.8f, VillageGateYMin - 1.5f, 0), gateRoot.transform, Vector3.one, new Vector2(0.6f, 0.4f), -0.2f);
    }

    // =======================================================================
    // TẠO & CẬP NHẬT TOÀN BỘ PREFAB (BUILDINGS, NPCS, ANIMALS, MONSTERS, NODES)
    // =======================================================================
    static void BuildPrefabs()
    {
        // 1. Player_Novice_Tier1 (Scale lớn hơn 1.5 lần hiện tại)
        CreateOrUpdatePrefab("Player_Novice_Tier1", go => {
            go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.GetOrAddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Horizontal;
            col.size = new Vector2(0.5f, 0.32f);
            col.offset = new Vector2(0f, -0.4f);

            go.GetOrAddComponent<CharacterVisualView>();
            go.GetOrAddComponent<WeaponSocketView>();
            go.GetOrAddComponent<PlayerPresenter>();
            go.GetOrAddComponent<CharacterStats>();
            go.GetOrAddComponent<ClassSystem>();
            var pc = go.GetOrAddComponent<PlayerController>();
            pc.CharacterScale = 1.5f;
            go.GetOrAddComponent<LuminaFourDirectionCharacterAnimator>();

            var anim = go.GetOrAddComponent<DirectionalWalkAnimator>();
            anim.walkFrontFrames = LoadSpriteFrames("hero-walk-front", 6);
            anim.walkBackFrames = LoadSpriteFrames("hero-walk-back", 6);
            anim.walkSideFrames = LoadSpriteFrames("hero-walk-side", 6);
            anim.idleFront = LoadSprite(SpriteDir + "/hero-idle-front.png");
            anim.idleBack = LoadSprite(SpriteDir + "/hero-idle-back.png");
            anim.idleSide = LoadSprite(SpriteDir + "/hero-idle-side.png");
            anim.fps = 8f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.4f;
            ySort.isStatic = false;

            sr.sprite = anim.idleFront;
        });

        // ===================================================================
        // NHÓM PREFAB NPC DI CHUYỂN TỰ DO TRONG LÀNG OAKHAVEN (SPRITE RIÊNG BIỆT, TỶ LỆ ĐỒNG ĐỀU)
        // ===================================================================
        // 2a. NPC_VillageElder (Trưởng Làng Eldrin)
        CreateOrUpdatePrefab("NPC_VillageElder", go => {
            SetupFreeRoamingVillageNPC(
                go,
                "eldrin",
                "Trưởng Làng Eldrin",
                "Chào dũng sĩ! Làng Oakhaven đã mở cửa Tiệm Rèn Boris, Dược Quán Elena và Hội Thám Hiểm Carlos. Hãy chuẩn bị kỹ trước khi ra Thảo Nguyên Xanh!",
                1.4f,
                2.8f,
                1.05f,
                null);
        });

        // 2b. NPC_Blacksmith_Barnaby (Thợ Rèn Boris / Barnaby)
        CreateOrUpdatePrefab("NPC_Blacksmith_Barnaby", go => {
            SetupFreeRoamingVillageNPC(
                go,
                "boris",
                "Thợ Rèn Boris",
                "Lò rèn Oakhaven luôn đỏ lửa! Mang Quặng Đồng và Quặng Sắt ngoài Thảo Nguyên về đây, ta sẽ rèn cho ngươi vũ khí sắc bén nhất!",
                1.5f,
                2.6f,
                1.1f,
                ShopType.Blacksmith);
        });

        // 2c. NPC_Alchemist_Elena (Dược Sĩ Elena)
        CreateOrUpdatePrefab("NPC_Alchemist_Elena", go => {
            SetupFreeRoamingVillageNPC(
                go,
                "elena",
                "Dược Sĩ Elena",
                "Hương thảo mộc quanh làng thật trong lành! Nếu thu thập được Thảo Mộc Xanh hay Cỏ Ánh Trăng ngoài suối, hãy ghé Dược Quán của ta nhé!",
                1.45f,
                2.8f,
                1.15f,
                ShopType.AlchemyLab);
        });

        // 2d. NPC_GuildMaster_Carlos (Hội Trưởng Carlos)
        CreateOrUpdatePrefab("NPC_GuildMaster_Carlos", go => {
            SetupFreeRoamingVillageNPC(
                go,
                "carlos",
                "Hội Trưởng Carlos",
                "Hiệp Hội Mạo Hiểm Giả luôn chào đón những chiến binh quả cảm! Hãy tiêu diệt lũ Chuột Chũi và Heo Rừng Đất để nâng cấp danh hiệu!",
                1.5f,
                2.8f,
                1.2f,
                ShopType.AdventurerGuild);
        });

        // 2e. NPC_PetMaster_Kaelen (Quản Thú Kaelen)
        CreateOrUpdatePrefab("NPC_PetMaster_Kaelen", go => {
            SetupFreeRoamingVillageNPC(
                go,
                "kaelen",
                "Quản Thú Kaelen",
                "Muông thú trên Thảo Nguyên Xanh đều có linh tính. Khi đạt cấp độ yêu cầu, hãy dùng Giấy Thu Phục để kết giao bạn đồng hành nhé!",
                1.45f,
                2.4f,
                1.1f,
                ShopType.PetSanctuary);
        });

        // 2f. NPC_Merchant_Lina (Thương Nhân Tạp Hóa Lina)
        CreateOrUpdatePrefab("NPC_Merchant_Lina", go => {
            SetupFreeRoamingVillageNPC(
                go,
                "lina",
                "Thương Nhân Lina",
                "Tiệm Tạp Hóa Oakhaven có đủ lương khô, bánh mì và vật tư dã ngoại cho chuyến đi săn dài ngày của bạn!",
                1.4f,
                2.6f,
                1.15f,
                ShopType.Restaurant);
        });

        // 2g. NPC_Villager_Wanderer (Cư Dân Oakhaven đi dạo)
        CreateOrUpdatePrefab("NPC_Villager_Wanderer", go => {
            SetupFreeRoamingVillageNPC(
                go,
                "villager",
                "Cư Dân Oakhaven",
                "Từ ngày các ngôi nhà mới và cây cầu gỗ qua suối được dựng lên, làng Oakhaven trở nên nhộn nhịp hẳn!",
                1.42f,
                4.2f,
                1.25f,
                null);
        });

        // 2h. Pet_Falcon (Chim Ưng Đồng Hành của Quản Thú Kaelen)
        CreateOrUpdatePrefab("Pet_Falcon", go => {
            go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_idle_perched_1.png");
            sr.color = Color.white;

            var ctrl = go.GetOrAddComponent<FalconPetController2D>();
            ctrl.idlePerchedFrames = new Sprite[] {
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_idle_perched_1.png"),
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_idle_perched_2.png")
            };
            ctrl.flyLoopFrames = new Sprite[] {
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_1.png"),
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_2.png"),
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_3.png"),
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_4.png")
            };
            ctrl.flightRadius = 1.8f;
            ctrl.flightSpeed = 2.2f;
            ctrl.minPerchTime = 4.0f;
            ctrl.maxPerchTime = 7.0f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.1f;
            ySort.isStatic = false;
        });

        // 2i. WorldProp_StrawHat (Mũ Rơm Thế Giới Khi Dân Làng Tháo Ra)
        CreateOrUpdatePrefab("WorldProp_StrawHat", go => {
            go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Sprites/NPCs/Modular/Villager/prop_villager_hat_world.png");
            sr.color = Color.white;
            var col = go.GetOrAddComponent<BoxCollider2D>();
            col.size = new Vector2(0.25f, 0.16f);
            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.1f;
            ySort.isStatic = true;
            ySort.baseSortingOffset = 5000;
        });

        // 2j. WorldProp_FruitBasket (Giỏ Trái Cây Thế Giới Khi Dân Làng Đặt Xuống Đất)
        CreateOrUpdatePrefab("WorldProp_FruitBasket", go => {
            go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Sprites/NPCs/Modular/Villager/prop_villager_basket_world.png");
            sr.color = Color.white;
            var col = go.GetOrAddComponent<BoxCollider2D>();
            col.size = new Vector2(0.25f, 0.18f);
            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.1f;
            ySort.isStatic = true;
            ySort.baseSortingOffset = 5000;
        });

        // ===================================================================
        // NHÓM PREFAB CÔNG TRÌNH KIẾN TRÚC (LÀNG OAKHAVEN - SERENE VILLAGE)
        // ===================================================================
        // 15. Building_Blacksmith_Forge (Lò Rèn Hoạt Ảnh Khói & Lửa Than Hồng 8 Frame)
        CreateOrUpdatePrefab("Building_Blacksmith_Forge", go => {
            go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = OrderProp;

            var anim = go.GetOrAddComponent<SpriteFrameAnimator>();
            Sprite[] forgeFrames = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                forgeFrames[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Buildings/ForgeFrames/blacksmith_forge_{i}.png");
            }
            anim.frames = forgeFrames;
            anim.framesPerSecond = 6f;
            if (forgeFrames[0] != null) sr.sprite = forgeFrames[0];

            var col = go.GetOrAddComponent<BoxCollider2D>();
            col.size = new Vector2(4.2f, 2.0f);
            col.offset = new Vector2(-0.1f, -1.6f);

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -2.0f;
            ySort.baseSortingOffset = 5000;
            ySort.isStatic = true;
        });

        CreateBuildingPrefab("Building_Blacksmith_House", VWDir + "/house_red_tall_chimney.png", new Vector2(2.8f, 1.6f), -1.1f);
        CreateBuildingPrefab("Building_Apothecary_Elena", VWDir + "/house_green_apothecary_hall.png", new Vector2(3.8f, 1.7f), -1.1f);
        CreateBuildingPrefab("Building_Herbalist_Cottage", VWDir + "/house_green_cottage.png", new Vector2(2.8f, 1.5f), -1.1f);
        CreateBuildingPrefab("Building_HunterGuild_Carlos", VWDir + "/house_blue_guild_hall.png", new Vector2(3.8f, 1.7f), -1.1f);
        CreateBuildingPrefab("Building_Guild_Barracks", VWDir + "/house_blue_cottage.png", new Vector2(2.8f, 1.5f), -1.1f);
        CreateBuildingPrefab("Building_GeneralStore", VWDir + "/house_red_general_store.png", new Vector2(3.8f, 1.7f), -1.1f);
        CreateBuildingPrefab("Building_Village_Tavern", VWDir + "/house_red_shop.png", new Vector2(2.8f, 1.5f), -1.1f);
        CreateBuildingPrefab("Building_PetSanctuary", VWDir + "/house_green_shop.png", new Vector2(2.8f, 1.5f), -1.1f);
        CreateBuildingPrefab("Building_Village_Residence", VWDir + "/house_blue_shop.png", new Vector2(2.8f, 1.5f), -1.1f);

        // Lửa trại động (Campfire 4 frames) & Sóng nước động (Water Waves 4 frames)
        CreateOrUpdatePrefab("Prop_Animated_Campfire", go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = OrderProp;
            var anim = go.GetOrAddComponent<SpriteFrameAnimator>();
            List<Sprite> frames = new List<Sprite>();
            for (int i = 0; i < 4; i++)
            {
                var s = LoadSprite($"{VWDir}/campfire_frame_{i}.png");
                if (s != null) frames.Add(s);
            }
            anim.frames = frames.ToArray();
            anim.framesPerSecond = 6f;
            if (frames.Count > 0) sr.sprite = frames[0];

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.35f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.3f;
            ySort.baseSortingOffset = 5000;
            ySort.isStatic = true;
        });

        CreateOrUpdatePrefab("Prop_Animated_WaterWaves", go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = OrderGround + 2;
            var anim = go.GetOrAddComponent<SpriteFrameAnimator>();
            List<Sprite> frames = new List<Sprite>();
            for (int i = 0; i < 4; i++)
            {
                var s = LoadSprite($"{VWDir}/water_wave_frame_{i}.png");
                if (s != null) frames.Add(s);
            }
            anim.frames = frames.ToArray();
            anim.framesPerSecond = 4.5f;
            if (frames.Count > 0) sr.sprite = frames[0];
        });

        // ===================================================================
        // NHÓM PREFAB ĐỘNG VẬT HOANG DÃ & QUÁI VẬT THẢO NGUYÊN XANH
        // ===================================================================
        // Hươu Tinh Khôi / Hươu Sao Thảo Nguyên (Ciervo - 4 hướng từ gói pet_animals)
        CreateOrUpdatePrefab("Animal_Deer_Stag", go => {
            SetupAnimalCreaturePrefab(go, "deer", "Hươu Sao Thảo Nguyên", 5, 110f, 1.4f, 4.5f, false);
        });

        // Thỏ Nâu Thảo Nguyên (Conejo)
        CreateOrUpdatePrefab("Animal_Rabbit_Brown", go => {
            SetupAnimalCreaturePrefab(go, "rabbit_brown", "Thỏ Nâu Thảo Nguyên", 1, 45f, 1.55f, 3.8f, false);
        });

        // Thỏ Trắng Tinh Anh (Conejo_blanco)
        CreateOrUpdatePrefab("Animal_Rabbit_White", go => {
            SetupAnimalCreaturePrefab(go, "rabbit_white", "Thỏ Trắng Đồng Cỏ", 2, 50f, 1.55f, 3.8f, false);
        });

        // Sói Xám Thảo Nguyên (Lobo - Quái chủ động tuần tra)
        CreateOrUpdatePrefab("Enemy_Steppe_Wolf", go => {
            SetupAnimalCreaturePrefab(go, "wolf_grey", "Sói Xám Thảo Nguyên", 5, 135f, 2.6f, 4.0f, true);
        });

        // Sói Trắng Rừng Sâu (Lobo_blanco - Quái tinh anh)
        CreateOrUpdatePrefab("Enemy_Forest_WhiteWolf", go => {
            SetupAnimalCreaturePrefab(go, "wolf_white", "Bạch Lang Rừng Sâu", 12, 260f, 3.0f, 4.5f, true);
        });

        // Heo Rừng Đất (Steppe Boar - codeart2d PixelSpec + Enemy_Steppe_Boar.asset)
        CreateOrUpdatePrefab("Enemy_Steppe_Boar", go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.sprite = LoadSprite(AnimalsDir + "/steppe_boar_earth.png");

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.38f;
            col.offset = new Vector2(0f, -0.15f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 6;
            stats.enemyName = "Heo Rừng Đất";
            stats.maxHp = 120f;
            stats.currentHp = 120f;

            var ctrl = go.GetOrAddComponent<EnemyController>();
            ctrl.enemyName = "Heo Rừng Đất";
            ctrl.level = 6;
            ctrl.element = ElementType.Earth;
            ctrl.maxHp = 120f;
            ctrl.currentHp = 120f;
            ctrl.moveSpeed = 2.6f;
            ctrl.attackPower = 15f;
            ctrl.patrolRadius = 3.5f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.2f;
            ySort.isStatic = false;
        });

        // Cua Đá Suối Mơ (Steppe Rock Crab - codeart2d PixelSpec + enemy_steppe_rock_crab.asset)
        CreateOrUpdatePrefab("Enemy_Steppe_Crab", go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.sprite = LoadSprite(AnimalsDir + "/steppe_crab.png");

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.34f;
            col.offset = new Vector2(0f, -0.12f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 4;
            stats.enemyName = "Cua Đá Suối Mơ";
            stats.maxHp = 95f;
            stats.currentHp = 95f;

            var ctrl = go.GetOrAddComponent<EnemyController>();
            ctrl.enemyName = "Cua Đá Suối Mơ";
            ctrl.level = 4;
            ctrl.element = ElementType.Water;
            ctrl.maxHp = 95f;
            ctrl.currentHp = 95f;
            ctrl.moveSpeed = 2.0f;
            ctrl.attackPower = 12f;
            ctrl.patrolRadius = 2.8f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.15f;
            ySort.isStatic = false;
        });

        // 3. Enemy_Mole (Chuột Chũi Thường Lv 2)
        CreateOrUpdatePrefab("Enemy_Mole", go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.offset = new Vector2(0f, -0.2f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 2;
            stats.enemyName = "Chuột Chũi";
            stats.maxHp = 60f;
            stats.currentHp = 60f;

            var ctrl = go.GetOrAddComponent<EnemyController>();
            ctrl.enemyName = "Chuột Chũi";
            ctrl.level = 2;
            ctrl.maxHp = 60f;
            ctrl.currentHp = 60f;
            ctrl.moveSpeed = 2.4f;
            ctrl.patrolRadius = 3f;

            var anim = go.GetOrAddComponent<DirectionalWalkAnimator>();
            anim.walkFrontFrames = LoadSpriteFrames("mole-walk-front", 4);
            anim.walkBackFrames = LoadSpriteFrames("mole-walk-back", 4);
            anim.walkSideFrames = LoadSpriteFrames("mole-walk-side", 4);
            anim.idleFront = LoadSprite(SpriteDir + "/mole-idle-front.png");
            anim.idleBack = LoadSprite(SpriteDir + "/mole-idle-back.png");
            anim.idleSide = LoadSprite(SpriteDir + "/mole-idle-side.png");
            anim.fps = 7f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.2f;
            ySort.isStatic = false;

            sr.sprite = anim.idleFront;
        });

        // 4. Enemy_Mole_Leader (Chuột Chũi Đầu Đàn Lv 5)
        CreateOrUpdatePrefab("Enemy_Mole_Leader", go => {
            go.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 11;
            sr.color = new Color(1f, 0.78f, 0.78f);

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.offset = new Vector2(0f, -0.22f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 5;
            stats.enemyName = "Chuột Chũi Đầu Đàn";
            stats.maxHp = 220f;
            stats.currentHp = 220f;

            var ctrl = go.GetOrAddComponent<EnemyController>();
            ctrl.enemyName = "Chuột Chũi Đầu Đàn";
            ctrl.level = 5;
            ctrl.maxHp = 220f;
            ctrl.currentHp = 220f;
            ctrl.moveSpeed = 2.7f;
            ctrl.attackPower = 18f;
            ctrl.patrolRadius = 3.5f;

            var anim = go.GetOrAddComponent<DirectionalWalkAnimator>();
            anim.walkFrontFrames = LoadSpriteFrames("mole-walk-front", 4);
            anim.walkBackFrames = LoadSpriteFrames("mole-walk-back", 4);
            anim.walkSideFrames = LoadSpriteFrames("mole-walk-side", 4);
            anim.idleFront = LoadSprite(SpriteDir + "/mole-idle-front.png");
            anim.idleBack = LoadSprite(SpriteDir + "/mole-idle-back.png");
            anim.idleSide = LoadSprite(SpriteDir + "/mole-idle-side.png");
            anim.fps = 8f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.22f;
            ySort.isStatic = false;

            sr.sprite = anim.idleFront;
        });

        // 5. Enemy_Water_Slime (Quái hệ Thủy tân thủ ở Đầm Nước)
        CreateOrUpdatePrefab("Enemy_Water_Slime", go => {
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.sprite = LoadSprite("Assets/Sprites/pet_waterslime.png");
            if (sr.sprite == null) sr.sprite = LoadSprite("Assets/Sprites/enemy_slime.png");
            sr.color = new Color(0.35f, 0.85f, 1f);

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.offset = new Vector2(0f, -0.1f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 3;
            stats.enemyName = "Thủy Slime";
            stats.maxHp = 75f;
            stats.currentHp = 75f;

            var ctrl = go.GetOrAddComponent<EnemyController>();
            ctrl.enemyName = "Thủy Slime";
            ctrl.level = 3;
            ctrl.element = ElementType.Water;
            ctrl.maxHp = 75f;
            ctrl.currentHp = 75f;
            ctrl.moveSpeed = 2.0f;
            ctrl.attackPower = 9f;
            ctrl.patrolRadius = 3f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.1f;
            ySort.isStatic = false;
        });

        // 6. Boss 1: Hắc Diệu Ma Viên (Lv 20 - Hệ Hỏa / Núi Lửa)
        CreateOrUpdatePrefab("Boss_Volcano_DemonApe", go => {
            go.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 15;
            sr.sprite = LoadSprite("Assets/Sprites/enemy_boss_wolfking.png");
            if (sr.sprite == null) sr.sprite = LoadSprite(SpriteDir + "/treant-idle-front.png");
            sr.color = new Color(1f, 0.35f, 0.2f);

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.65f;
            col.offset = new Vector2(0f, -0.3f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 20;
            stats.enemyName = "Hắc Diệu Ma Viên";
            stats.maxHp = 650f;
            stats.currentHp = 650f;

            var boss = go.GetOrAddComponent<BossController>();
            boss.enemyName = "Hắc Diệu Ma Viên";
            boss.level = 20;
            boss.element = ElementType.Fire;
            boss.maxHp = 650f;
            boss.currentHp = 650f;
            boss.moveSpeed = 3.2f;
            boss.attackPower = 28f;
            boss.attackRange = 2.0f;
            boss.isWorldBoss = true;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.3f;
            ySort.isStatic = false;
        });

        // 7. Boss 2: Thủy Quái Đại Hồ (Lv 22 - Hệ Thủy / Đại Hồ)
        CreateOrUpdatePrefab("Boss_Lake_Leviathan", go => {
            go.transform.localScale = new Vector3(2.0f, 2.0f, 1f);

            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 15;
            sr.sprite = LoadSprite("Assets/Sprites/pet_waterslime.png");
            if (sr.sprite == null) sr.sprite = LoadSprite("Assets/Sprites/enemy_slime.png");
            sr.color = new Color(0.2f, 0.85f, 1f);

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.7f;
            col.offset = new Vector2(0f, -0.2f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 22;
            stats.enemyName = "Thủy Tinh Linh Vương";
            stats.maxHp = 700f;
            stats.currentHp = 700f;

            var boss = go.GetOrAddComponent<BossController>();
            boss.enemyName = "Thủy Tinh Linh Vương";
            boss.level = 22;
            boss.element = ElementType.Water;
            boss.maxHp = 700f;
            boss.currentHp = 700f;
            boss.moveSpeed = 2.6f;
            boss.attackPower = 26f;
            boss.attackRange = 2.2f;
            boss.isWorldBoss = true;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.2f;
            ySort.isStatic = false;
        });

        // 8. Boss 3: Đại Cổ Thụ Tinh Linh (Lv 22 - Hệ Mộc / Cổ Thụ Khổng Lồ)
        CreateOrUpdatePrefab("Boss_Treant_Ancient", go => {
            go.transform.localScale = new Vector3(2.0f, 2.0f, 1f);

            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 15;
            sr.sprite = LoadSprite(SpriteDir + "/treant-idle-front.png");
            sr.color = new Color(0.75f, 1f, 0.75f);

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.8f;
            col.offset = new Vector2(0f, -0.5f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 22;
            stats.enemyName = "Đại Cổ Thụ Tinh Linh";
            stats.maxHp = 750f;
            stats.currentHp = 750f;

            var boss = go.GetOrAddComponent<BossController>();
            boss.enemyName = "Đại Cổ Thụ Tinh Linh";
            boss.level = 22;
            boss.element = ElementType.Earth;
            boss.maxHp = 750f;
            boss.currentHp = 750f;
            boss.moveSpeed = 2.0f;
            boss.attackPower = 32f;
            boss.attackRange = 2.5f;
            boss.isWorldBoss = true;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.5f;
            ySort.isStatic = false;
        });

        // 9. Boss 4: Vua Sói Đầu Đàn Đỉnh Núi (Lv 25 - Hệ Phong / Đỉnh Núi Cao)
        CreateOrUpdatePrefab("Boss_Summit_WolfKing", go => {
            go.transform.localScale = new Vector3(1.7f, 1.7f, 1f);

            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 15;
            sr.sprite = LoadSprite("Assets/Sprites/enemy_boss_wolfking.png");
            sr.color = new Color(0.92f, 0.95f, 1f);

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.65f;
            col.offset = new Vector2(0f, -0.3f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 25;
            stats.enemyName = "Vua Sói Đầu Đàn Đỉnh Núi";
            stats.maxHp = 800f;
            stats.currentHp = 800f;

            var boss = go.GetOrAddComponent<BossController>();
            boss.enemyName = "Vua Sói Đầu Đàn Đỉnh Núi";
            boss.level = 25;
            boss.element = ElementType.Wind;
            boss.maxHp = 800f;
            boss.currentHp = 800f;
            boss.moveSpeed = 4.0f;
            boss.attackPower = 35f;
            boss.attackRange = 2.2f;
            boss.isWorldBoss = true;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.3f;
            ySort.isStatic = false;
        });

        // 10. Pet_Treant_Sprout (Tinh Linh Cây Con Lv 8 - có đủ animation 4 hướng)
        CreateOrUpdatePrefab("Pet_Treant_Sprout", go => {
            go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.color = new Color(0.85f, 1f, 0.85f);

            var rb = go.GetOrAddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.offset = new Vector2(0f, -0.25f);

            go.GetOrAddComponent<Hurtbox2D>();

            var stats = go.GetOrAddComponent<EnemyStats>();
            stats.level = 8;
            stats.enemyName = "Búp Cổ Thụ";
            stats.maxHp = 130f;
            stats.currentHp = 130f;

            var ctrl = go.GetOrAddComponent<EnemyController>();
            ctrl.enemyName = "Búp Cổ Thụ";
            ctrl.level = 8;
            ctrl.maxHp = 130f;
            ctrl.currentHp = 130f;
            ctrl.moveSpeed = 2.3f;
            ctrl.patrolRadius = 3f;

            var anim = go.GetOrAddComponent<DirectionalWalkAnimator>();
            anim.walkFrontFrames = LoadSpriteFrames("treant-walk-front", 4);
            anim.walkBackFrames = LoadSpriteFrames("treant-walk-back", 4);
            anim.walkSideFrames = LoadSpriteFrames("treant-walk-side", 4);
            anim.idleFront = LoadSprite(SpriteDir + "/treant-idle-front.png");
            anim.idleBack = LoadSprite(SpriteDir + "/treant-idle-back.png");
            anim.idleSide = LoadSprite(SpriteDir + "/treant-idle-side.png");
            anim.fps = 7f;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.25f;
            ySort.isStatic = false;

            sr.sprite = anim.idleFront;
        });

        // ===================================================================
        // NHÓM PREFAB ĐIỂM THU THẬP TÀI NGUYÊN MỚI (VẼ BẰNG CODEART2D PIXELSPEC + ROCKS)
        // ===================================================================
        // 11a. GatheringNode_Bush (Bụi Thảo Mộc Xanh Oakhaven - codeart2d herb_bush_emerald)
        CreateGatheringNodePrefab(
            "GatheringNode_Bush",
            GatherDir + "/herb_bush_emerald.png",
            GatheringNodeType.HerbBush,
            "Bụi Thảo Mộc Xanh",
            "Assets/Resources/Items/material_herb_leaf.asset",
            1, 3);

        // 11b. GatheringNode_MoonHerb (Bụi Dược Thảo Ánh Trăng - codeart2d herb_bush_moonlight)
        CreateGatheringNodePrefab(
            "GatheringNode_MoonHerb",
            GatherDir + "/herb_bush_moonlight.png",
            GatheringNodeType.HerbBush,
            "Bụi Cỏ Ánh Trăng",
            "Assets/ScriptableObjects/Items/mat_herb_fiber.asset",
            1, 3);

        // 11c. GatheringNode_CrimsonHerb (Bụi Huyết Thảo - codeart2d herb_bush_crimson)
        CreateGatheringNodePrefab(
            "GatheringNode_CrimsonHerb",
            GatherDir + "/herb_bush_crimson.png",
            GatheringNodeType.HerbBush,
            "Bụi Huyết Thảo",
            "Assets/Resources/Items/material_herb_leaf.asset",
            2, 4);

        // 12a. GatheringNode_CopperOre (Mạch Quặng Đồng Thô - codeart2d ore_vein_copper)
        CreateGatheringNodePrefab(
            "GatheringNode_CopperOre",
            GatherDir + "/ore_vein_copper.png",
            GatheringNodeType.OreVein,
            "Mạch Quặng Đồng Thô",
            "Assets/Resources/Items/material_copper_ore.asset",
            1, 2);

        // 12b. GatheringNode_Ore (Mạch Quặng Sắt Thô - codeart2d ore_vein_iron)
        CreateGatheringNodePrefab(
            "GatheringNode_Ore",
            GatherDir + "/ore_vein_iron.png",
            GatheringNodeType.OreVein,
            "Mạch Quặng Sắt Thô",
            "Assets/Resources/Items/material_iron_ore.asset",
            1, 2);

        // 12c. GatheringNode_CrystalOre (Mỏ Tinh Thể Lam Ngọc - rock_crystal_node / ore_vein_crystal)
        CreateGatheringNodePrefab(
            "GatheringNode_CrystalOre",
            VWDir + "/rock_crystal_node.png",
            GatheringNodeType.OreVein,
            "Mỏ Tinh Thể Lam Ngọc",
            "Assets/ScriptableObjects/Items/mat_stone_powder.asset",
            1, 3);

        // 13. Waystone_Monument
        CreateOrUpdatePrefab("Waystone_Monument", go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 20;
            sr.sprite = LoadSprite(SpriteDir + "/rock-monument.png");

            var col = go.GetOrAddComponent<BoxCollider2D>();
            col.size = new Vector2(2.2f, 0.8f);
            col.offset = new Vector2(0f, -1.2f);

            var waystone = go.GetOrAddComponent<Waystone>();
            waystone.waystoneId = "waystone_village";
            waystone.waystoneName = "Trụ Đá Làng Tân Thủ";

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -1.2f;
            ySort.isStatic = true;
        });

        // 14. Waterfall (Thác Nước Động 3 frame)
        CreateOrUpdatePrefab("Waterfall", go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = OrderProp;

            var anim = go.GetOrAddComponent<SpriteFrameAnimator>();
            anim.frames = new Sprite[] {
                LoadSprite(SpriteDir + "/waterfall-1.png"),
                LoadSprite(SpriteDir + "/waterfall-2.png"),
                LoadSprite(SpriteDir + "/waterfall-3.png")
            };
            anim.framesPerSecond = 5f;
            sr.sprite = anim.frames[0];

            var col = go.GetOrAddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 4.5f);
            col.offset = new Vector2(0f, 0f);

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -2.2f;
            ySort.isStatic = true;
        });
    }

    static void SetupFreeRoamingVillageNPC(
        GameObject go,
        string npcKey,
        string npcTitle,
        string dialogue,
        float visualScale,
        float wanderRadius,
        float moveSpeed,
        ShopType? optionalShopType)
    {
        go.transform.localScale = new Vector3(0.75f, 0.75f, 1f); // Thu nhỏ NPC đồng bộ kích thước chuẩn với Class nhân vật (1.03 unit = 1 tile)

        var sr = go.GetOrAddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.color = Color.white; // Màu sắc nguyên bản của Sprite Pixel Art riêng biệt

        var rb = go.GetOrAddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = go.GetOrAddComponent<BoxCollider2D>();
        col.size = new Vector2(0.48f, 0.3f);
        col.offset = new Vector2(0f, -0.35f);

        // Tương tác hội thoại bằng phím [F]
        var elderInteract = go.GetOrAddComponent<NPCVillageElder>();
        elderInteract.npcName = npcTitle;
        elderInteract.greetingMessage = dialogue;
        elderInteract.interactDistance = 2.4f;

        // Nếu là chủ cửa tiệm thì gắn thêm ShopNPC
        if (optionalShopType.HasValue)
        {
            var shop = go.GetOrAddComponent<ShopNPC>();
            shop.npcName = npcTitle;
            shop.shopType = optionalShopType.Value;
            shop.greetingDialogue = dialogue;
        }

        // Hoạt ảnh bước đi 4 hướng với sprite thân nhân vật bóc tách (Modular Body)
        var anim = go.GetOrAddComponent<DirectionalWalkAnimator>();
        anim.idleFront = LoadModularIdleSprite(npcKey, "front");
        anim.idleBack = LoadModularIdleSprite(npcKey, "back");
        anim.idleSide = LoadModularIdleSprite(npcKey, "side");
        anim.idleLeft = LoadModularIdleSprite(npcKey, "left");
        anim.walkFrontFrames = LoadModularNpcFrames(npcKey, "front", 4);
        anim.walkBackFrames = LoadModularNpcFrames(npcKey, "back", 4);
        anim.walkSideFrames = LoadModularNpcFrames(npcKey, "side", 4);
        anim.walkLeftFrames = LoadModularNpcFrames(npcKey, "left", 4);
        anim.fps = 6f;
        sr.sprite = anim.idleFront;

        // Di chuyển tự do quanh vị trí nhà/cửa tiệm
        var wander = go.GetOrAddComponent<NPCWanderController2D>();
        wander.wanderRadius = wanderRadius;
        wander.moveSpeed = moveSpeed;
        wander.pauseWhenPlayerNearby = true;
        wander.playerStopDistance = 2.2f;

        var ySort = go.GetOrAddComponent<YSortOrder2D>();
        ySort.footOffsetY = -0.35f;
        ySort.isStatic = false;

        // Xóa các prop / socket cũ nếu có để tránh nhân bản khi tạo lại
        for (int i = go.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = go.transform.GetChild(i);
            if (child.name.StartsWith("Prop_") || child.name.StartsWith("Socket_") || child.name.StartsWith("Modular_"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        string folder = GetModularFolder(npcKey);
        string key = npcKey.ToLower();

        // Thiết lập bóc tách chi tiết từng NPC (Modular Props & Sockets)
        if (key == "eldrin")
        {
            // Trưởng Làng Eldrin: Quyền trượng gỗ sồi ngọc lam (Prop_ElderStaff) luôn trên tay phải trong cả 4 hướng
            GameObject staffObj = new GameObject("Prop_ElderStaff");
            staffObj.transform.SetParent(go.transform, false);
            staffObj.transform.localPosition = Vector3.zero;
            var staffSr = staffObj.AddComponent<SpriteRenderer>();
            staffSr.color = Color.white;
            var staffProp = staffObj.AddComponent<NPCModularProp>();
            staffProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_staff_front.png");
            staffProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_staff_back.png");
            staffProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_staff_side.png");
            staffProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_staff_left.png");
            staffProp.offsetFront = Vector2.zero;
            staffProp.offsetBack = Vector2.zero;
            staffProp.offsetSide = Vector2.zero;
            staffProp.offsetLeft = Vector2.zero;
            staffProp.syncWithWalkStepBob = true;
            staffProp.sortingOffsetFront = 1;
            staffProp.sortingOffsetBack = -1;
            staffProp.sortingOffsetSideRight = 1;
            staffProp.sortingOffsetSideLeft = -1;

            // Hiệu ứng phát sáng của viên ngọc lam trên đỉnh trượng (Glow / Pulse) đồng bộ lưới 32x32
            GameObject glowObj = new GameObject("Prop_OrbGlow");
            glowObj.transform.SetParent(staffObj.transform, false);
            glowObj.transform.localPosition = Vector3.zero;
            var glowSr = glowObj.AddComponent<SpriteRenderer>();
            glowSr.sprite = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_orb_glow_front.png");
            glowSr.color = Color.white;
            glowSr.sortingOrder = 12;
            var orbGlow = glowObj.AddComponent<EldrinStaffOrbGlow>();
            orbGlow.glowFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_orb_glow_front.png");
            orbGlow.glowBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_orb_glow_back.png");
            orbGlow.glowSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_orb_glow_side.png");
            orbGlow.glowLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_eldrin_orb_glow_left.png");
        }
        else if (key == "boris")
        {
            // Thợ Rèn Boris: Búa rèn thép tôi trên tay trái trong mọi trạng thái (Prop_Hammer), vung theo nhịp bước đi
            GameObject hammerObj = new GameObject("Prop_Hammer");
            hammerObj.transform.SetParent(go.transform, false);
            hammerObj.transform.localPosition = Vector3.zero;
            var hammerSr = hammerObj.AddComponent<SpriteRenderer>();
            hammerSr.color = Color.white;
            var hammerProp = hammerObj.AddComponent<NPCModularProp>();
            hammerProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_boris_hammer_front.png");
            hammerProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_boris_hammer_back.png");
            hammerProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_boris_hammer_side.png");
            hammerProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_boris_hammer_left.png");
            hammerProp.offsetFront = Vector2.zero;
            hammerProp.offsetBack = Vector2.zero;
            hammerProp.offsetSide = Vector2.zero;
            hammerProp.offsetLeft = Vector2.zero;
            hammerProp.syncWithWalkStepBob = true;
            hammerProp.sortingOffsetFront = 1;
            hammerProp.sortingOffsetBack = -1;
            hammerProp.sortingOffsetSideRight = -1;
            hammerProp.sortingOffsetSideLeft = 1;
            hammerProp.hasSecondaryMotion = false;
        }
        else if (key == "elena")
        {
            // Dược Sĩ Elena: Túi da đeo chéo và lọ độc dược bên hông phải (Prop_HerbBag), nảy nhẹ 1px (secondary bounce) theo bước chân
            GameObject pouchObj = new GameObject("Prop_HerbBag");
            pouchObj.transform.SetParent(go.transform, false);
            pouchObj.transform.localPosition = Vector3.zero;
            var pouchSr = pouchObj.AddComponent<SpriteRenderer>();
            pouchSr.color = Color.white;
            var pouchProp = pouchObj.AddComponent<NPCModularProp>();
            pouchProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_elena_pouch_front.png");
            pouchProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_elena_pouch_back.png");
            pouchProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_elena_pouch_side.png");
            pouchProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_elena_pouch_left.png");
            pouchProp.offsetFront = Vector2.zero;
            pouchProp.offsetBack = Vector2.zero;
            pouchProp.offsetSide = Vector2.zero;
            pouchProp.offsetLeft = Vector2.zero;
            pouchProp.syncWithWalkStepBob = true;
            pouchProp.sortingOffsetFront = 1;
            pouchProp.sortingOffsetBack = -1;
            pouchProp.sortingOffsetSideRight = 1;
            pouchProp.sortingOffsetSideLeft = -1;
            pouchProp.hasSecondaryMotion = true;
            pouchProp.motionAmplitudeX = 0.015f;
            pouchProp.motionAmplitudeY = 0.04f;
            pouchProp.motionFrequency = 7.0f;
            pouchProp.bounceOnlyWhenMoving = true;
        }
        else if (key == "carlos")
        {
            // Hội Trưởng Carlos: Áo choàng đỏ bay nhẹ phía sau (Prop_CrimsonCape) + Thanh kiếm hiệp sĩ chuôi mạ vàng bên hông trái (Prop_KnightSword)
            GameObject capeObj = new GameObject("Prop_CrimsonCape");
            capeObj.transform.SetParent(go.transform, false);
            capeObj.transform.localPosition = Vector3.zero;
            var capeSr = capeObj.AddComponent<SpriteRenderer>();
            capeSr.color = Color.white;
            var capeProp = capeObj.AddComponent<NPCModularProp>();
            capeProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_carlos_cape_front.png");
            capeProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_carlos_cape_back.png");
            capeProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_carlos_cape_side.png");
            capeProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_carlos_cape_left.png");
            capeProp.offsetFront = Vector2.zero;
            capeProp.offsetBack = Vector2.zero;
            capeProp.offsetSide = Vector2.zero;
            capeProp.offsetLeft = Vector2.zero;
            capeProp.syncWithWalkStepBob = true;
            capeProp.sortingOffsetFront = -1; // Áo choàng sau lưng khi nhìn xuống
            capeProp.sortingOffsetBack = 1;   // Áo choàng che lưng khi nhìn lên
            capeProp.sortingOffsetSideRight = -1;
            capeProp.sortingOffsetSideLeft = -1;
            capeProp.hasSecondaryMotion = true;
            capeProp.motionAmplitudeX = 0.025f;
            capeProp.motionAmplitudeY = 0.015f;
            capeProp.motionFrequency = 5.0f;
            capeProp.bounceOnlyWhenMoving = true;

            GameObject swordObj = new GameObject("Prop_KnightSword");
            swordObj.transform.SetParent(go.transform, false);
            swordObj.transform.localPosition = Vector3.zero;
            var swordSr = swordObj.AddComponent<SpriteRenderer>();
            swordSr.color = Color.white;
            var swordProp = swordObj.AddComponent<NPCModularProp>();
            swordProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_carlos_sword_front.png");
            swordProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_carlos_sword_back.png");
            swordProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_carlos_sword_side.png");
            swordProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_carlos_sword_left.png");
            swordProp.offsetFront = Vector2.zero;
            swordProp.offsetBack = Vector2.zero;
            swordProp.offsetSide = Vector2.zero;
            swordProp.offsetLeft = Vector2.zero;
            swordProp.syncWithWalkStepBob = true;
            swordProp.sortingOffsetFront = 1;
            swordProp.sortingOffsetBack = -1;
            swordProp.sortingOffsetSideRight = -1;
            swordProp.sortingOffsetSideLeft = 1;
            swordProp.hasSecondaryMotion = false;
        }
        else if (key == "kaelen")
        {
            // Quản Thú Kaelen: Túi thức ăn bên hông (Prop_FeedPouch) + Điểm neo PerchPoint + Thực thể Chim Ưng (Pet_Falcon)
            GameObject pouchObj = new GameObject("Prop_FeedPouch");
            pouchObj.transform.SetParent(go.transform, false);
            pouchObj.transform.localPosition = Vector3.zero;
            var pouchSr = pouchObj.AddComponent<SpriteRenderer>();
            pouchSr.color = Color.white;
            var pouchProp = pouchObj.AddComponent<NPCModularProp>();
            pouchProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_kaelen_pouch_front.png");
            pouchProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_kaelen_pouch_back.png");
            pouchProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_kaelen_pouch_side.png");
            pouchProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_kaelen_pouch_left.png");
            pouchProp.offsetFront = Vector2.zero;
            pouchProp.offsetBack = Vector2.zero;
            pouchProp.offsetSide = Vector2.zero;
            pouchProp.offsetLeft = Vector2.zero;
            pouchProp.syncWithWalkStepBob = true;
            pouchProp.sortingOffsetFront = 1;
            pouchProp.sortingOffsetBack = -1;
            pouchProp.sortingOffsetSideRight = 1;
            pouchProp.sortingOffsetSideLeft = -1;
            pouchProp.hasSecondaryMotion = false;

            GameObject perchObj = new GameObject("PerchPoint");
            perchObj.transform.SetParent(go.transform, false);
            perchObj.transform.localPosition = new Vector3(0.3125f, 0.0625f, 0f);

            GameObject falconObj = new GameObject("Pet_Falcon");
            falconObj.transform.SetParent(go.transform, false);
            falconObj.transform.localPosition = perchObj.transform.localPosition;
            falconObj.transform.localScale = Vector3.one;
            var falconSr = falconObj.AddComponent<SpriteRenderer>();
            falconSr.sprite = LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_idle_perched_1.png");
            falconSr.color = Color.white;

            var falconBehavior = falconObj.AddComponent<FalconBehaviorController>();
            falconBehavior.kaelenTransform = go.transform;
            falconBehavior.perchPoint = perchObj.transform;
            falconBehavior.idlePerchedFrames = new Sprite[] {
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_idle_perched_1.png"),
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_idle_perched_2.png")
            };
            falconBehavior.flyLoopFrames = new Sprite[] {
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_1.png"),
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_2.png"),
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_3.png"),
                LoadSprite("Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_4.png")
            };
        }
        else if (key == "lina")
        {
            // Thương Nhân Lina: Giỏ hàng mây đan đung đưa (Prop_Basket) + Túi tiền vàng & sổ ghi chép ở thắt lưng (Prop_CoinPouch)
            GameObject basketObj = new GameObject("Prop_Basket");
            basketObj.transform.SetParent(go.transform, false);
            basketObj.transform.localPosition = Vector3.zero;
            var basketSr = basketObj.AddComponent<SpriteRenderer>();
            basketSr.color = Color.white;
            var basketProp = basketObj.AddComponent<NPCModularProp>();
            basketProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_lina_basket_front.png");
            basketProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_lina_basket_back.png");
            basketProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_lina_basket_side.png");
            basketProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_lina_basket_left.png");
            basketProp.offsetFront = Vector2.zero;
            basketProp.offsetBack = Vector2.zero;
            basketProp.offsetSide = Vector2.zero;
            basketProp.offsetLeft = Vector2.zero;
            basketProp.syncWithWalkStepBob = true;
            basketProp.sortingOffsetFront = 1;
            basketProp.sortingOffsetBack = -1;
            basketProp.sortingOffsetSideRight = 1;
            basketProp.sortingOffsetSideLeft = -1;
            basketProp.hasSecondaryMotion = true;
            basketProp.motionAmplitudeX = 0.03f;
            basketProp.motionAmplitudeY = 0.02f;
            basketProp.motionFrequency = 7.0f;
            basketProp.bounceOnlyWhenMoving = true;

            GameObject pouchObj = new GameObject("Prop_CoinPouch");
            pouchObj.transform.SetParent(go.transform, false);
            pouchObj.transform.localPosition = Vector3.zero;
            var pouchSr = pouchObj.AddComponent<SpriteRenderer>();
            pouchSr.color = Color.white;
            var pouchProp = pouchObj.AddComponent<NPCModularProp>();
            pouchProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_lina_pouch_front.png");
            pouchProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_lina_pouch_back.png");
            pouchProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_lina_pouch_side.png");
            pouchProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_lina_pouch_left.png");
            pouchProp.offsetFront = Vector2.zero;
            pouchProp.offsetBack = Vector2.zero;
            pouchProp.offsetSide = Vector2.zero;
            pouchProp.offsetLeft = Vector2.zero;
            pouchProp.syncWithWalkStepBob = true;
            pouchProp.sortingOffsetFront = 1;
            pouchProp.sortingOffsetBack = -1;
            pouchProp.sortingOffsetSideRight = -1;
            pouchProp.sortingOffsetSideLeft = 1;
            pouchProp.hasSecondaryMotion = false;
        }
        else if (key == "villager")
        {
            // Cư Dân Oakhaven: Mũ rơm (Prop_StrawHat) + Giỏ hoa quả (Prop_FruitBasket) + Thân đầy đủ tóc nâu
            GameObject hatObj = new GameObject("Prop_StrawHat");
            hatObj.transform.SetParent(go.transform, false);
            hatObj.transform.localPosition = Vector3.zero;
            var hatSr = hatObj.AddComponent<SpriteRenderer>();
            hatSr.color = Color.white;
            var hatProp = hatObj.AddComponent<NPCModularProp>();
            hatProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_hat_front.png");
            hatProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_hat_back.png");
            hatProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_hat_side.png");
            hatProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_hat_left.png");
            hatProp.offsetFront = Vector2.zero;
            hatProp.offsetBack = Vector2.zero;
            hatProp.offsetSide = Vector2.zero;
            hatProp.offsetLeft = Vector2.zero;
            hatProp.syncWithWalkStepBob = true;
            hatProp.sortingOffsetFront = 2;
            hatProp.sortingOffsetBack = 2;
            hatProp.sortingOffsetSideRight = 2;
            hatProp.sortingOffsetSideLeft = 2;
            hatProp.hasSecondaryMotion = false;

            GameObject basketObj = new GameObject("Prop_FruitBasket");
            basketObj.transform.SetParent(go.transform, false);
            basketObj.transform.localPosition = Vector3.zero;
            var basketSr = basketObj.AddComponent<SpriteRenderer>();
            basketSr.color = Color.white;
            var basketProp = basketObj.AddComponent<NPCModularProp>();
            basketProp.spriteFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_basket_front.png");
            basketProp.spriteBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_basket_back.png");
            basketProp.spriteSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_basket_side.png");
            basketProp.spriteLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_basket_left.png");
            basketProp.offsetFront = Vector2.zero;
            basketProp.offsetBack = Vector2.zero;
            basketProp.offsetSide = Vector2.zero;
            basketProp.offsetLeft = Vector2.zero;
            basketProp.syncWithWalkStepBob = true;
            basketProp.sortingOffsetFront = 1;
            basketProp.sortingOffsetBack = -1;
            basketProp.sortingOffsetSideRight = 1;
            basketProp.sortingOffsetSideLeft = 1;
            basketProp.hasSecondaryMotion = true;
            basketProp.motionAmplitudeX = 0.015f;
            basketProp.motionAmplitudeY = 0.02f;
            basketProp.motionFrequency = 7.0f;
            basketProp.bounceOnlyWhenMoving = true;

            var propInteract = go.GetOrAddComponent<VillagerPropInteract>();
            propInteract.strawHatProp = hatProp;
            propInteract.fruitBasketProp = basketProp;
            propInteract.worldHatSprite = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_hat_world.png");
            propInteract.worldBasketSprite = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/prop_villager_basket_world.png");

            propInteract.unarmedIdleFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_villager_body_idle_front.png");
            propInteract.unarmedIdleBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_villager_body_idle_back.png");
            propInteract.unarmedIdleSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_villager_body_idle_side.png");
            propInteract.unarmedIdleLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_villager_body_idle_left.png");

            propInteract.carryIdleFront = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_villager_carry_idle_front.png");
            propInteract.carryIdleBack = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_villager_carry_idle_back.png");
            propInteract.carryIdleSide = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_villager_carry_idle_side.png");
            propInteract.carryIdleLeft = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_villager_carry_idle_left.png");

            var propHandler = go.GetOrAddComponent<VillagerPropHandler>();
            propHandler.hatProp = hatProp;
            propHandler.basketProp = basketProp;
            propHandler.worldHatSprite = propInteract.worldHatSprite;
            propHandler.worldBasketSprite = propInteract.worldBasketSprite;
            propHandler.enableAutonomousRoutine = false;
        }

        // Gắn bộ điều phối đa tầng Modular Character
        var modularCharacter = go.GetOrAddComponent<NPCModularCharacter2D>();
        modularCharacter.RefreshProps();
        if (key == "kaelen")
        {
            var perch = go.transform.Find("PerchPoint");
            if (perch == null) perch = go.transform.Find("Socket_FalconPerch");
            if (perch != null)
            {
                modularCharacter.perchSocket = perch;
                modularCharacter.perchOffsetFront = new Vector2(0.3125f, 0.0625f);
                modularCharacter.perchOffsetBack = new Vector2(-0.3125f, 0.0625f);
                modularCharacter.perchOffsetSide = new Vector2(0.25f, 0.0625f);
            }
        }
    }

    static void SetupAnimalCreaturePrefab(
        GameObject go,
        string animalPrefix,
        string displayName,
        int level,
        float maxHp,
        float moveSpeed,
        float wanderRadius,
        bool isHostileEnemy)
    {
        go.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) go.layer = enemyLayer;

        var sr = go.GetOrAddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        var rb = go.GetOrAddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = go.GetOrAddComponent<CircleCollider2D>();
        col.radius = 0.32f;
        col.offset = new Vector2(0f, -0.15f);

        var anim = go.GetOrAddComponent<DirectionalWalkAnimator>();
        anim.walkFrontFrames = LoadNumberedFrames($"{AnimalsDir}/{animalPrefix}_walk_front", 4);
        anim.walkBackFrames = LoadNumberedFrames($"{AnimalsDir}/{animalPrefix}_walk_back", 4);
        anim.walkSideFrames = LoadNumberedFrames($"{AnimalsDir}/{animalPrefix}_walk_side", 4);
        anim.idleFront = LoadSprite($"{AnimalsDir}/{animalPrefix}_idle_front_0.png");
        anim.idleBack = LoadSprite($"{AnimalsDir}/{animalPrefix}_idle_back_0.png");
        anim.idleSide = LoadSprite($"{AnimalsDir}/{animalPrefix}_idle_side_0.png");
        anim.fps = 7.5f;
        if (anim.idleFront != null) sr.sprite = anim.idleFront;

        go.GetOrAddComponent<Hurtbox2D>();

        var stats = go.GetOrAddComponent<EnemyStats>();
        stats.level = level;
        stats.enemyName = displayName;
        stats.maxHp = maxHp;
        stats.currentHp = maxHp;
        stats.expReward = 20 + level * 5;
        stats.goldReward = 5 + level * 3;

        var hideItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Resources/Items/material_animal_hide.asset");
        if (hideItem != null)
        {
            stats.dropTable = new List<DropEntry>
            {
                new DropEntry { item = hideItem, dropChancePercent = 85f, minQuantity = 1, maxQuantity = 2 }
            };
        }

        if (isHostileEnemy)
        {
            var ctrl = go.GetOrAddComponent<EnemyController>();
            ctrl.enemyName = displayName;
            ctrl.level = level;
            ctrl.maxHp = maxHp;
            ctrl.currentHp = maxHp;
            ctrl.moveSpeed = moveSpeed;
            ctrl.attackPower = 8f + level * 2.2f;
            ctrl.patrolRadius = wanderRadius;
        }
        else
        {
            // Động vật hiền lành (Hươu, Thỏ) đi dạo tự do trên Thảo Nguyên
            var wander = go.GetOrAddComponent<ActionRPG.Town.NPCWanderController2D>();
            wander.wanderRadius = wanderRadius;
            wander.moveSpeed = moveSpeed;
            wander.pauseWhenPlayerNearby = false;
        }

        go.GetOrAddComponent<ActionRPG.UI.EnemyOverheadUI>();

        var ySort = go.GetOrAddComponent<YSortOrder2D>();
        ySort.footOffsetY = -0.2f;
        ySort.isStatic = false;
    }

    static void CreateBuildingPrefab(string prefabName, string spritePath, Vector2 footColliderSize, float footOffsetY)
    {
        CreateOrUpdatePrefab(prefabName, go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = OrderProp;
            sr.sprite = LoadSprite(spritePath);

            var col = go.GetOrAddComponent<BoxCollider2D>();
            col.size = footColliderSize;
            col.offset = new Vector2(0f, footOffsetY);

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = footOffsetY - 0.3f;
            ySort.baseSortingOffset = 5000;
            ySort.isStatic = true;
        });
    }

    static void CreateGatheringNodePrefab(
        string prefabName,
        string spritePath,
        GatheringNodeType nodeType,
        string displayName,
        string itemAssetPath,
        int minQty,
        int maxQty)
    {
        CreateOrUpdatePrefab(prefabName, go => {
            var sr = go.GetOrAddComponent<SpriteRenderer>();
            sr.sortingOrder = 8;
            var sp = LoadSprite(spritePath);
            sr.sprite = sp != null ? sp : LoadSprite(SpriteDir + "/bush.png");

            var col = go.GetOrAddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.85f;

            var node = go.GetOrAddComponent<GatheringNode>();
            node.nodeType = nodeType;
            node.nodeName = displayName;
            node.harvestedItem = AssetDatabase.LoadAssetAtPath<ItemData>(itemAssetPath);
            node.minQuantity = minQty;
            node.maxQuantity = maxQty;

            var ySort = go.GetOrAddComponent<YSortOrder2D>();
            ySort.footOffsetY = -0.35f;
            ySort.isStatic = true;
        });
    }

    // =======================================================================
    // VẼ TILEMAP: HỒ NƯỚC, DÒNG SUỐI UỐN LƯỢN & ĐỊA HÌNH HỮU CƠ
    // =======================================================================
    static void PaintGround(
        Tilemap ground,
        Tile dirt,
        Tile grass,
        Tile path,
        Tile water,
        Tile lava,
        Tile mountainRock,
        Tile deepGrass)
    {
        for (int x = MapMinX; x < MapMaxX; x++)
        {
            for (int y = MapMinY; y < MapMaxY; y++)
            {
                Tile targetTile = grass;
                Color tint = Color.white;

                // Dòng suối uốn lượn chảy từ Đại Hồ (x=66, y=-15) xuyên qua Rừng và đổ vào Ao Thảo Nguyên (x=10, y=-13)
                float streamCenterY = -14.2f + 1.4f * Mathf.Sin(x * 0.16f);
                bool isStreamChannel = (x >= 10 && x <= 68) && Mathf.Abs(y - streamCenterY) <= 1.15f;
                bool isStreamBank = (x >= 10 && x <= 68) && Mathf.Abs(y - streamCenterY) <= 2.1f;
                // Chừa lối cầu gỗ bắc ngang qua suối
                bool isBridgeCrossing = (x >= 15 && x <= 17) || (x >= 23 && x <= 24) || (x >= 64 && x <= 66);

                // 1. LÀNG TÂN THỦ OAKHAVEN (X: -62 đến -18, Y: -18 đến 18)
                if (x >= VillageMinX && x <= VillageMaxX && y >= VillageMinY && y <= VillageMaxY)
                {
                    // Quảng trường đá trung tâm làng (-44..-34, -4..4)
                    if (x >= -44 && x <= -34 && y >= -4 && y <= 4)
                    {
                        targetTile = path;
                    }
                    // Đường chính Đông - Tây nối tới Cổng Làng (-56..-18, -2..2)
                    else if (x >= -56 && x <= VillageMaxX && y >= VillageGateYMin && y <= VillageGateYMax)
                    {
                        targetTile = path;
                    }
                    // Đường nhánh Bắc - Nam dẫn vào các cửa tiệm (Boris, Elena, Carlos, Lina, Kaelen)
                    else if ((x >= -49 && x <= -47 && y >= -11 && y <= 8) ||
                             (x >= -28 && x <= -26 && y >= -11 && y <= 8) ||
                             (x >= -40 && x <= -38 && y >= -11 && y <= 9))
                    {
                        targetTile = path;
                    }
                    // Lề đất quanh khu cửa tiệm
                    else if ((x >= -54 && x <= -44 && (Mathf.Abs(y) >= 6 && Mathf.Abs(y) <= 12)) ||
                             (x >= -34 && x <= -23 && (Mathf.Abs(y) >= 6 && Mathf.Abs(y) <= 12)))
                    {
                        targetTile = dirt;
                    }
                    else
                    {
                        targetTile = grass;
                    }
                }
                // 2. CON ĐƯỜNG HUYẾT MẠCH DỌC XUỐNG PHÍA NAM
                else if (
                    (x >= VillageMaxX && x <= SouthRoadXMax && y >= VillageGateYMin && y <= VillageGateYMax) ||
                    (x >= SouthRoadXMin && x <= SouthRoadXMax && y >= SouthRoadYMin && y <= SouthRoadYMax)
                )
                {
                    targetTile = path;
                }
                else if (
                    (x == SouthRoadXMin - 1 || x == SouthRoadXMax + 1) &&
                    (y >= SouthRoadYMin - 1 && y <= SouthRoadYMax + 1)
                )
                {
                    targetTile = ((x + y * 3) % 2 == 0) ? dirt : grass;
                }
                // 3. THẢO NGUYÊN XANH & AO HỒ SUỐI MƠ (X: -9 đến 25)
                else if (x < ForestBorderX)
                {
                    float dx = x - 10.0f;
                    float dy = y - (-13.0f);
                    float distPond = (dx * dx) / (5.8f * 5.8f) + (dy * dy) / (4.3f * 4.3f) + 0.12f * Mathf.Sin(dx * 1.3f) * Mathf.Cos(dy * 1.5f);

                    float dx1 = x - 3.5f;
                    float dy1 = y - (-9.0f);
                    float distPuddle1 = (dx1 * dx1) / (2.5f * 2.5f) + (dy1 * dy1) / (1.6f * 1.6f);

                    if ((distPond <= 1.0f || distPuddle1 <= 1.0f || isStreamChannel) && !isBridgeCrossing)
                    {
                        targetTile = water;
                    }
                    else if (isBridgeCrossing && isStreamChannel)
                    {
                        targetTile = path;
                    }
                    else if (distPond <= 1.35f || distPuddle1 <= 1.42f || isStreamBank)
                    {
                        targetTile = dirt;
                    }
                    else
                    {
                        targetTile = grass;
                    }
                }
                // 4. ĐẠI RỪNG RẬM & 4 VÙNG BOSS (X >= 25)
                else
                {
                    float vlDx = x - 43.0f;
                    float vlDy = y - (-18.0f);
                    float distVolcano = (vlDx * vlDx) / (10.5f * 10.5f) + (vlDy * vlDy) / (7.2f * 7.2f) + 0.10f * Mathf.Sin(vlDx * 0.8f);

                    // ĐẠI HỒ NƯỚC SÂU KHU RỪNG: Mở rộng tự nhiên có vịnh nhỏ & suối nối
                    float lkDx = x - 77.0f;
                    float lkDy = y - (-17.0f);
                    float distLake = (lkDx * lkDx) / (12.2f * 12.2f) + (lkDy * lkDy) / (9.2f * 9.2f)
                                     + 0.14f * Mathf.Sin(lkDx * 0.55f) * Mathf.Cos(lkDy * 0.65f);

                    // Hòn đảo nhỏ giữa Đại Hồ (tâm 77, -17, bán kính ~2.2m) để đặt Boss hoặc kho báu
                    float islandDist = (lkDx * lkDx) / (2.5f * 2.5f) + (lkDy * lkDy) / (2.0f * 2.0f);

                    if (x >= PeakMinX && x <= PeakMaxX && y >= PeakMinY && y <= PeakMaxY)
                    {
                        targetTile = mountainRock;
                    }
                    else if (distLake <= 1.0f && islandDist > 1.0f)
                    {
                        targetTile = water;
                    }
                    else if (distLake <= 1.0f && islandDist <= 1.0f)
                    {
                        targetTile = dirt;
                    }
                    else if (distLake <= 1.32f && x >= LakeMinX - 1 && x <= LakeMaxX + 1 && y >= LakeMinY - 1 && y <= LakeMaxY + 1)
                    {
                        targetTile = dirt;
                    }
                    else if (isStreamChannel && !isBridgeCrossing && distVolcano > 1.25f)
                    {
                        targetTile = water;
                    }
                    else if (isBridgeCrossing && isStreamChannel)
                    {
                        targetTile = path;
                    }
                    else if (distVolcano <= 1.0f)
                    {
                        targetTile = lava;
                    }
                    else if (distVolcano <= 1.32f && x >= VolcanoMinX - 1 && x <= VolcanoMaxX + 1 && y >= VolcanoMinY - 1 && y <= VolcanoMaxY + 1)
                    {
                        targetTile = mountainRock;
                    }
                    else
                    {
                        targetTile = deepGrass;
                        tint = new Color(0.78f, 0.92f, 0.78f);
                    }
                }

                Vector3Int pos = new Vector3Int(x, y, 0);
                ground.SetTile(pos, targetTile);
                ground.SetTileFlags(pos, TileFlags.None);
                ground.SetColor(pos, tint);
            }
        }
    }

    static void PaintWalls(Tilemap obstacleMap, Tile borderWall, Tile castleWall)
    {
        for (int x = MapMinX; x < MapMaxX; x++)
        {
            obstacleMap.SetTile(new Vector3Int(x, MapMinY, 0), borderWall);
            obstacleMap.SetTile(new Vector3Int(x, MapMaxY - 1, 0), borderWall);
        }
        for (int y = MapMinY; y < MapMaxY; y++)
        {
            obstacleMap.SetTile(new Vector3Int(MapMinX, y, 0), borderWall);
            obstacleMap.SetTile(new Vector3Int(MapMaxX - 1, y, 0), borderWall);
        }

        // Tường Thành Làng Oakhaven nhỏ gọn sắc nét (wall_stone_timber 16x16)
        for (int x = VillageMinX; x <= VillageMaxX; x++)
        {
            obstacleMap.SetTile(new Vector3Int(x, VillageMaxY, 0), castleWall);
            obstacleMap.SetTile(new Vector3Int(x, VillageMinY, 0), castleWall);
        }
        for (int y = VillageMinY; y <= VillageMaxY; y++)
        {
            obstacleMap.SetTile(new Vector3Int(VillageMinX, y, 0), castleWall);
        }
        for (int y = VillageMinY; y <= VillageMaxY; y++)
        {
            if (y >= VillageGateYMin && y <= VillageGateYMax) continue; // Cổng Làng mở
            obstacleMap.SetTile(new Vector3Int(VillageMaxX, y, 0), castleWall);
        }
    }

    static Tilemap CreateTilemap(Transform parent, string name, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        Tilemap tm = go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.AddComponent<TilemapRenderer>();
        tr.sortingLayerName = "Default";
        tr.sortingOrder = order;
        return tm;
    }

    // =======================================================================
    // HELPERS: ĐẶT PROP SERENE VILLAGE, MINI GRASS, SÓNG NƯỚC & CỤM CẢNH QUAN
    // =======================================================================
    static GameObject PlaceVWProp(
        string name,
        string vwSpriteName,
        Vector3 pos,
        Transform parent,
        Vector3 scale,
        Vector2 colliderSize,
        float colliderOffsetY,
        bool hasCollider = true,
        bool flipX = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(VWDir + "/" + vwSpriteName + ".png");
        sr.flipX = flipX;

        if (hasCollider && colliderSize.sqrMagnitude > 0.001f)
        {
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = colliderSize;
            col.offset = new Vector2(0f, colliderOffsetY);
        }

        var ySort = go.AddComponent<YSortOrder2D>();
        ySort.footOffsetY = colliderOffsetY;
        ySort.baseSortingOffset = 5000;
        ySort.isStatic = true;

        return go;
    }

    static void PlaceSereneTreeCluster(string clusterName, Vector3 basePos, Transform parent, string mainTree, string subTree)
    {
        GameObject cluster = new GameObject(clusterName);
        cluster.transform.SetParent(parent);
        cluster.transform.position = basePos;

        PlaceVWProp(clusterName + "_MainTree", mainTree, basePos, cluster.transform, new Vector3(1.3f, 1.3f, 1f), new Vector2(1.0f, 0.5f), -1.2f);
        PlaceVWProp(clusterName + "_SubTree", subTree, basePos + new Vector3(1.9f, -0.7f, 0), cluster.transform, new Vector3(1.0f, 1.0f, 1f), new Vector2(0.8f, 0.4f), -0.9f, true, true);
        PlaceVWProp(clusterName + "_FlowerBush", "bush_flower_blue", basePos + new Vector3(-1.2f, -1.4f, 0), cluster.transform, Vector3.one, new Vector2(0.6f, 0.35f), -0.2f);
        PlaceVWProp(clusterName + "_SmallRock", "rock_pebble_small", basePos + new Vector3(0.4f, -1.6f, 0), cluster.transform, Vector3.one, Vector2.zero, -0.15f, false);
    }

    static void ScatterMiniGrassAndFlowers(Transform parent, Rect area, int count, string prefix)
    {
        GameObject folder = new GameObject(prefix + "_MiniGrassAndFlowers");
        folder.transform.SetParent(parent);

        string[] grassTypes = new[] {
            "mini_grass_tuft",
            "mini_grass_tuft",
            "mini_grass_flower_gold",
            "mini_grass_flower_pink",
            "serene_white_flower_herb"
        };

        for (int i = 0; i < count; i++)
        {
            // Dùng hàm băm tất định để vị trí cỏ mini tự nhiên và ổn định
            float hx = Mathf.Sin((i + 1) * 12.9898f) * 43758.5453f;
            float hy = Mathf.Cos((i + 1) * 78.233f) * 23421.631f;
            float rx = hx - Mathf.Floor(hx);
            float ry = hy - Mathf.Floor(hy);

            float px = area.xMin + rx * area.width;
            float py = area.yMin + ry * area.height;

            string sprName = grassTypes[i % grassTypes.Length];
            GameObject g = new GameObject($"{prefix}_{i:00}");
            g.transform.SetParent(folder.transform);
            g.transform.position = new Vector3(px, py, 0f);

            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite($"{VWDir}/{sprName}.png");
            sr.sortingOrder = OrderGround + 1;
            sr.flipX = (i % 2 == 1);
        }
    }

    static void SpawnWaterWavesInPond(Transform parent, Vector3 center, int count, float radius, string prefix)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / Mathf.Max(1, count);
            float r = (i % 2 == 0) ? radius * 0.45f : radius * 0.75f;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r * 0.7f, 0f);
            SpawnPrefab("Prop_Animated_WaterWaves", pos, parent).name = $"{prefix}_{i:00}";
        }
    }

    static GameObject PlaceProp(
        string name,
        string spriteName,
        Vector3 pos,
        Transform parent,
        Vector3 scale,
        Vector2 colliderSize,
        float colliderOffsetY,
        bool hasCollider = true,
        bool flipX = false,
        Color? tint = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(SpriteDir + "/" + spriteName + ".png");
        sr.flipX = flipX;
        if (tint.HasValue) sr.color = tint.Value;

        if (hasCollider)
        {
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = colliderSize;
            col.offset = new Vector2(0f, colliderOffsetY);
        }

        var ySort = go.AddComponent<YSortOrder2D>();
        ySort.footOffsetY = colliderOffsetY;
        ySort.baseSortingOffset = 5000;
        ySort.isStatic = true;

        return go;
    }

    static void PlacePondShoreCluster(Transform parent, Vector3 pos, string namePrefix)
    {
        PlaceVWProp(namePrefix + "_MossyStone", "rock_mossy_medium", pos, parent, new Vector3(0.8f, 0.8f, 1f), new Vector2(0.8f, 0.4f), -0.2f);
        PlaceVWProp(namePrefix + "_Pebble", "rock_pebble_small", pos + new Vector3(0.8f, -0.35f, 0), parent, Vector3.one, Vector2.zero, -0.15f, false, true);
        PlaceProp(namePrefix + "_Reed", "bush-tall", pos + new Vector3(-0.75f, 0.45f, 0), parent, new Vector3(0.75f, 0.75f, 1f), new Vector2(0.5f, 0.3f), -0.15f);
        PlaceVWProp(namePrefix + "_Flower", "bush_flower_white", pos + new Vector3(0.55f, 0.55f, 0), parent, Vector3.one, Vector2.zero, -0.15f, false);
    }

    static GameObject SpawnPrefab(string prefabName, Vector3 pos, Transform parent)
    {
        string p = PrefabDir + "/" + prefabName + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
        if (prefab == null)
        {
            Debug.LogWarning("[ForestMapGenerator] Thiếu prefab: " + p);
            return new GameObject(prefabName + "_MISSING");
        }
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = pos;
        if (parent != null) go.transform.SetParent(parent, true);
        return go;
    }

    static GameObject SpawnWildPetInZone(string petPrefabName, Vector3 pos, Transform parent, string instanceName)
    {
        string p = "Assets/Prefabs/Pets/" + petPrefabName + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
        if (prefab == null)
        {
            Debug.LogWarning("[ForestMapGenerator] Thiếu pet prefab: " + p);
            return null;
        }
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = instanceName;
        go.transform.position = pos;
        if (parent != null) go.transform.SetParent(parent, true);

        var ctrl = go.GetComponent<ActionRPG.Pets.PetController>();
        if (ctrl != null) ctrl.isWild = true;
        var ai = go.GetComponent<ActionRPG.Pets.PetAI>();
        if (ai != null) ai.isWild = true;

        var interactable = go.GetComponent<ActionRPG.Pets.WildPetInteractable>();
        if (interactable == null) interactable = go.AddComponent<ActionRPG.Pets.WildPetInteractable>();

        return go;
    }

    static void CreateOrUpdatePrefab(string prefabName, System.Action<GameObject> configure)
    {
        string path = PrefabDir + "/" + prefabName + ".prefab";
        GameObject go = new GameObject(prefabName);
        try
        {
            configure(go);
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    static Sprite[] LoadSpriteFrames(string prefix, int count)
    {
        List<Sprite> list = new List<Sprite>();
        for (int i = 1; i <= count; i++)
        {
            Sprite s = LoadSprite($"{SpriteDir}/{prefix}-{i}.png");
            if (s != null) list.Add(s);
        }
        return list.ToArray();
    }

    static Sprite[] LoadNumberedFrames(string prefixPath, int count)
    {
        List<Sprite> list = new List<Sprite>();
        for (int i = 0; i < count; i++)
        {
            Sprite s = LoadSprite($"{prefixPath}_{i}.png");
            if (s != null) list.Add(s);
        }
        return list.ToArray();
    }

    static Sprite[] LoadNpcFrames(string npcKey, string dir, int count)
    {
        List<Sprite> list = new List<Sprite>();
        for (int i = 1; i <= count; i++)
        {
            Sprite s = LoadSprite($"Assets/Sprites/NPCs/npc_{npcKey}_walk_{dir}_{i}.png");
            if (s != null) list.Add(s);
        }
        return list.ToArray();
    }

    static string GetModularFolder(string npcKey)
    {
        switch (npcKey.ToLower())
        {
            case "eldrin": return "Eldrin";
            case "boris": return "Boris";
            case "elena": return "Elena";
            case "carlos": return "Carlos";
            case "kaelen": return "Kaelen";
            case "lina": return "Lina";
            case "villager": return "Villager";
            default: return npcKey;
        }
    }

    static Sprite[] LoadModularNpcFrames(string npcKey, string dir, int count)
    {
        string folder = GetModularFolder(npcKey);
        List<Sprite> list = new List<Sprite>();
        for (int i = 1; i <= count; i++)
        {
            Sprite s = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_{npcKey}_body_walk_{dir}_{i}.png");
            if (s == null)
            {
                s = LoadSprite($"Assets/Sprites/NPCs/npc_{npcKey}_walk_{dir}_{i}.png");
            }
            if (s != null) list.Add(s);
        }
        return list.ToArray();
    }

    static Sprite LoadModularIdleSprite(string npcKey, string dir)
    {
        string folder = GetModularFolder(npcKey);
        Sprite s = LoadSprite($"Assets/Sprites/NPCs/Modular/{folder}/npc_{npcKey}_body_idle_{dir}.png");
        if (s == null)
        {
            s = LoadSprite($"Assets/Sprites/NPCs/npc_{npcKey}_idle_{dir}.png");
        }
        return s;
    }

    // =======================================================================
    // TƯỜNG THÀNH VẬT LÝ KIÊN CỐ (BLOCKING BARRIERS - CHỈ VÀO QUA CỔNG LÀNG)
    // =======================================================================
    static void BuildVillageFortressWallColliders(Transform parent)
    {
        GameObject fortressRoot = new GameObject("Village_Fortress_Physical_Barriers");
        fortressRoot.transform.SetParent(parent);

        // 1. Tường Bắc (North Wall Barrier)
        CreateBarrierBox(fortressRoot.transform, "Barrier_Wall_North",
            new Vector3((VillageMinX + VillageMaxX) * 0.5f + 0.5f, VillageMaxY + 0.5f, 0f),
            new Vector2(VillageMaxX - VillageMinX + 1.6f, 1.4f));

        // 2. Tường Nam (South Wall Barrier)
        CreateBarrierBox(fortressRoot.transform, "Barrier_Wall_South",
            new Vector3((VillageMinX + VillageMaxX) * 0.5f + 0.5f, VillageMinY - 0.5f, 0f),
            new Vector2(VillageMaxX - VillageMinX + 1.6f, 1.4f));

        // 3. Tường Tây (West Wall Barrier)
        CreateBarrierBox(fortressRoot.transform, "Barrier_Wall_West",
            new Vector3(VillageMinX - 0.5f, (VillageMinY + VillageMaxY) * 0.5f + 0.5f, 0f),
            new Vector2(1.4f, VillageMaxY - VillageMinY + 1.6f));

        // 4. Tường Đông - Cánh Bắc (East Wall North Wing): Từ đỉnh tường đến cột cổng Bắc (Y = 2.8 .. 18.5)
        float northSectionHeight = (VillageMaxY + 0.5f) - (VillageGateYMax + 0.8f);
        float northSectionCenterY = (VillageMaxY + 0.5f + VillageGateYMax + 0.8f) * 0.5f;
        CreateBarrierBox(fortressRoot.transform, "Barrier_Wall_East_NorthSection",
            new Vector3(VillageMaxX + 0.5f, northSectionCenterY, 0f),
            new Vector2(1.4f, northSectionHeight));

        // 5. Tường Đông - Cánh Nam (East Wall South Wing): Từ cột cổng Nam xuống chân tường (Y = -18.5 .. -2.8)
        float southSectionHeight = (VillageGateYMin - 0.8f) - (VillageMinY - 0.5f);
        float southSectionCenterY = (VillageMinY - 0.5f + VillageGateYMin - 0.8f) * 0.5f;
        CreateBarrierBox(fortressRoot.transform, "Barrier_Wall_East_SouthSection",
            new Vector3(VillageMaxX + 0.5f, southSectionCenterY, 0f),
            new Vector2(1.4f, southSectionHeight));

        // LƯU Ý QUAN TRỌNG:
        // Khoảng trống Cổng Làng từ Y = -2.8 đến Y = +2.8 tại X = VillageMaxX (-18)
        // là LỐI ĐI MỞ HOÀN TOÀN, người chơi và NPC chỉ có thể đi qua đây để vào/ra Làng Oakhaven!
    }

    static void CreateBarrierBox(Transform parent, string name, Vector3 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var box = go.AddComponent<BoxCollider2D>();
        box.size = size;
        box.offset = Vector2.zero;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
    }

    static Sprite LoadSprite(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets != null)
        {
            foreach (Object o in assets)
            {
                if (o is Sprite s) return s;
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Tile CreateTileFromCustomSprite(string name, string customPngPath, System.Func<Texture2D> fallbackGenerator, Tile.ColliderType collider)
    {
        string pngPath = TileSpriteDir + "/" + name + ".png";
        if (File.Exists(customPngPath))
        {
            File.Copy(customPngPath, pngPath, true);
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);
            EnsurePropImport(pngPath);
        }
        else
        {
            return CreatePixelArtTile(name, fallbackGenerator, collider);
        }

        string assetPath = TileAssetDir + "/" + name + ".asset";
        Tile asset = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        asset.sprite = LoadSprite(pngPath);
        asset.colliderType = collider;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    static Tile CreateTileFromTileset(string name, Vector2Int cell, Tile.ColliderType collider)
    {
        string pngPath = TileSpriteDir + "/" + name + ".png";
        if (!File.Exists(pngPath))
        {
            Texture2D src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            src.LoadImage(File.ReadAllBytes(SpriteDir + "/tileset.png"));
            int px = cell.x * 16;
            int py = src.height - (cell.y + 1) * 16;
            Texture2D tile = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tile.SetPixels(src.GetPixels(px, py, 16, 16));
            tile.Apply();
            File.WriteAllBytes(pngPath, tile.EncodeToPNG());
            Object.DestroyImmediate(src);
            Object.DestroyImmediate(tile);
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);
        }
        EnsurePropImport(pngPath);

        string assetPath = TileAssetDir + "/" + name + ".asset";
        Tile asset = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        asset.sprite = LoadSprite(pngPath);
        asset.colliderType = collider;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    static Tile CreatePixelArtTile(string name, System.Func<Texture2D> textureGenerator, Tile.ColliderType collider)
    {
        string pngPath = TileSpriteDir + "/" + name + ".png";
        Texture2D tex = textureGenerator();
        File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);
        EnsurePropImport(pngPath);

        string assetPath = TileAssetDir + "/" + name + ".asset";
        Tile asset = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        asset.sprite = LoadSprite(pngPath);
        asset.colliderType = collider;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    static Texture2D GenerateWaterTexture()
    {
        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Color baseBlue = new Color(0.18f, 0.56f, 0.86f);
        Color deepBlue = new Color(0.14f, 0.45f, 0.76f);
        Color lightCyan = new Color(0.48f, 0.82f, 0.98f);
        Color crestWhite = new Color(0.85f, 0.96f, 1.0f);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Color c = baseBlue;
                if (y == 3 || y == 9 || y == 14)
                {
                    if ((x + y * 2) % 6 < 4) c = deepBlue;
                }
                else if (y == 4 || y == 10 || y == 15)
                {
                    if ((x + y * 2) % 6 < 3) c = lightCyan;
                    if ((x + y * 2) % 6 == 1) c = crestWhite;
                }
                tex.SetPixel(x, y, c);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateLavaTexture()
    {
        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Color magmaRed = new Color(0.85f, 0.22f, 0.08f);
        Color lavaOrange = new Color(1.0f, 0.55f, 0.10f);
        Color coreYellow = new Color(1.0f, 0.88f, 0.25f);
        Color crustBlack = new Color(0.22f, 0.10f, 0.08f);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Color c = magmaRed;
                if ((x * 2 + y * 3) % 7 == 0) c = lavaOrange;
                else if ((x + y) % 9 == 0) c = coreYellow;
                else if ((x * 3 + y * 2) % 11 == 0 || (x == 4 && y == 4) || (x == 12 && y == 11)) c = crustBlack;
                tex.SetPixel(x, y, c);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateMountainRockTexture()
    {
        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Color rockMid = new Color(0.44f, 0.46f, 0.52f);
        Color rockShadow = new Color(0.28f, 0.30f, 0.35f);
        Color rockLight = new Color(0.62f, 0.65f, 0.72f);
        Color crack = new Color(0.20f, 0.21f, 0.25f);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Color c = rockMid;
                if ((x + y) % 8 == 0) c = rockShadow;
                else if ((x + y + 1) % 8 == 0) c = rockLight;
                if ((x * 4 + y * 5) % 13 == 0) c = crack;
                tex.SetPixel(x, y, c);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateDeepGrassTexture()
    {
        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Color mossBase = new Color(0.14f, 0.35f, 0.18f);
        Color mossDark = new Color(0.09f, 0.24f, 0.12f);
        Color mossLight = new Color(0.22f, 0.48f, 0.26f);
        Color forestFlower = new Color(0.85f, 0.75f, 0.40f);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Color c = mossBase;
                if ((x * 2 + y) % 5 == 0) c = mossDark;
                else if ((x + y * 3) % 6 == 0) c = mossLight;
                if ((x == 3 && y == 7) || (x == 11 && y == 13)) c = forestFlower;
                tex.SetPixel(x, y, c);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateCastleWallTexture()
    {
        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Color stoneFace = new Color(0.48f, 0.50f, 0.54f);
        Color stoneLight = new Color(0.65f, 0.67f, 0.72f);
        Color stoneDark = new Color(0.35f, 0.36f, 0.39f);
        Color mortar = new Color(0.22f, 0.23f, 0.25f);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Color c = stoneFace;
                if (y == 0 || y == 7 || y == 8 || y == 15) c = mortar;
                else if (y < 8 && (x == 0 || x == 15)) c = mortar;
                else if (y >= 8 && (x == 7 || x == 8)) c = mortar;
                else if (y == 6 || y == 14) c = stoneLight;
                else if (y == 1 || y == 9) c = stoneDark;
                tex.SetPixel(x, y, c);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    static void ConfigureSprites()
    {
        string[] dirs = new[] { SpriteDir, VWDir, GatherDir, AnimalsDir, NPCsDir };
        foreach (string dir in dirs)
        {
            if (!Directory.Exists(dir)) continue;
            string[] allPngs = Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories);
            foreach (string file in allPngs)
            {
                EnsurePropImport(file.Replace("\\", "/"));
            }
        }
    }

    static void EnsurePropImport(string assetPath)
    {
        TextureImporter imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (imp == null) return;
        bool changed = false;
        if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; changed = true; }
        if (imp.spriteImportMode == SpriteImportMode.None) { imp.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (imp.spritePixelsPerUnit != 16) { imp.spritePixelsPerUnit = 16; changed = true; }
        if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; changed = true; }
        if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (changed) imp.SaveAndReimport();
    }

    static void EnsureFolder(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
    }
}

