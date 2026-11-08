#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Trình tạo Map 2 Thuần Thủ Tục (Procedural Tilemap &amp; Prefab Generator) - BẢN SỬA LỖI v2
    /// - TUYỆT ĐỐI KHÔNG dùng ảnh nền. Chỉ SetTile + Instantiate prefab/sprite 16x16 có sẵn trong dự án.
    /// - Nền: cỏ xanh rừng đậm (exterior_157 của map_doi_thuong, tô tối theo nhiễu Perlin tạo vân) - phủ 100%.
    /// - Vách đá ngang lượn sóng giữa map (y = H/2) chia Cao Nguyên / Vùng Thấp, cầu thang đá nhúng vào vách
    ///   đúng điểm đường mòn trung tâm đi qua.
    /// - 4 ngôi nhà trên cao nguyên, rào gỗ bao quanh, đường đá cuội dẫn thẳng tới cầu thang.
    /// - Núi quặng đá xám 2 tầng (Tile_MountainRock tô bóng) có quặng trên đỉnh - KHÔNG dùng gạch đỏ.
    /// - Rừng rậm mật độ cao lấp kín mọi khoảng cỏ trống, chừa 2 ô đệm quanh đường &amp; nhà.
    /// </summary>
    [InitializeOnLoad]
    public static class Map2ProceduralGenerator
    {
        private const int W = 60;
        private const int H = 60;
        private const int Seed = 20261031;

        private const string MainScenePath = "Assets/Scenes/MainGame.unity";
        private const string TriggerPath = "Temp/DoGenerateMap2.trigger";

        private const string DT = "Assets/Sprites/map_doi_thuong/";
        private const string PAL = "Assets/Prefabs/Map3/DoiThuong_Palette/";
        private const string FP = "Assets/Prefabs/ForestPrototype/";
        private const string CliffSheet = DT + "PNG/multi_tiered_cliffs_and_nature.png";
        private const string DetailSheet = DT + "PNG/ground_grass_details.png";
        private const string TileMountainPath = "Assets/Tilemaps/ForestDemo/Tile_MountainRock.asset";

        // Đường vách đá chính
        private const int CliffBaseTop = 31;          // mép trên của vách (đơn vị local)
        private const float StairsCenterX = 30f;      // đường mòn trung tâm cắt qua vách tại đây

        // ---- trạng thái chạy ----
        private static System.Random rng;
        private static Transform root;
        private static Tilemap ground;
        private static bool[,] isPath;
        private static bool[,] blocked;   // cấm thực vật (đường + đệm, nhà, núi, hố...)
        private static bool[,] occ;       // đã có vật thể
        private static int[] cliffTopAt;  // mép trên vách theo từng cột x
        private static TileBase tGrass, tDirt;
        private static TileBase[] tStone;
        private static Material spriteMat;

        static Map2ProceduralGenerator()
        {
            EditorApplication.update -= CheckTrigger;
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (!File.Exists(TriggerPath)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            try { File.Delete(TriggerPath); } catch { }
            try
            {
                Debug.Log("<color=cyan><b>[Map2ProceduralGenerator] 🚀 Phát hiện trigger - bắt đầu tạo Map 2...</b></color>");
                GenerateProceduralMap2();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Map2ProceduralGenerator] Lỗi: {ex}");
            }
        }

        // =====================================================================================
        [MenuItem("Tools/Map 2/🔨 1. Tự Động Xây Dựng Map 2 Bằng Tilemap & Prefabs", priority = 10)]
        public static void GenerateProceduralMap2()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != MainScenePath)
                activeScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            GameObject map2Root = GameObject.Find("Map_Zone_2");
            if (map2Root == null) { Debug.LogError("[Map2ProceduralGenerator] Không tìm thấy 'Map_Zone_2'!"); return; }
            root = map2Root.transform;
            rng = new System.Random(Seed);
            isPath = new bool[W, H];
            blocked = new bool[W, H];
            occ = new bool[W, H];
            cliffTopAt = new int[W];

            // ---------- 1. TẢI TÀI NGUYÊN ----------
            tGrass = LoadTile("exterior_157");
            tDirt = LoadTile("exterior_231");
            tStone = new[] { LoadTile("exterior_106"), LoadTile("exterior_107"), LoadTile("exterior_108"), LoadTile("exterior_109"),
                             LoadTile("exterior_138"), LoadTile("exterior_139"), LoadTile("exterior_140"), LoadTile("exterior_141") };
            TileBase tMountain = AssetDatabase.LoadAssetAtPath<TileBase>(TileMountainPath);
            TileBase tFenceH1 = LoadTile("exterior_16"), tFenceH2 = LoadTile("exterior_17"), tFenceV = LoadTile("exterior_5");

            if (tGrass == null || tDirt == null || tMountain == null || Array.Exists(tStone, t => t == null))
            {
                Debug.LogError("[Map2ProceduralGenerator] Thiếu tile cơ bản (exterior_157/231/106.., Tile_MountainRock). Dừng.");
                return;
            }

            Sprite[] cliffH = { LoadSub(CliffSheet, "cliff_horiz_1"), LoadSub(CliffSheet, "cliff_horiz_2"), LoadSub(CliffSheet, "cliff_horiz_3") };
            Sprite cliffV = LoadSub(CliffSheet, "cliff_vert_1");
            Sprite stairs = LoadSub(CliffSheet, "stairs_left_4step");

            GameObject P(string n) => AssetDatabase.LoadAssetAtPath<GameObject>(PAL + n + ".prefab");
            GameObject F(string n) => AssetDatabase.LoadAssetAtPath<GameObject>(FP + n + ".prefab");

            GameObject[] bigTrees = { P("01_Cay_Soi_Co_Thu_Lon"), P("04_Cay_Tao_Do_Lon"), P("07_Cay_Tan_Tron_Lon") };
            GameObject[] midTrees = { P("02_Cay_Soi_Vua"), P("05_Cay_Tao_Do_Vua"), P("08_Cay_Tan_Tron_Vua"), P("10_Cay_Thong_Xanh_Lon") };
            GameObject[] smallTrees = { P("03_Cay_Soi_Nho"), P("06_Cay_Tao_Do_Nho"), P("09_Cay_Tan_Tron_Nho"), P("11_Cay_Thong_Xanh_Nho") };
            GameObject[] bushes = { P("23_Bui_Cay_Xanh_Lon"), P("24_Bui_Cay_Xanh_Tron"), P("25_Bui_Duong_Xi"), P("30_Tham_Co_Xanh_Tron") };
            GameObject[] forestProps = { P("21_Goc_Cay_Co_Thu"), P("22_Than_Go_Muc_Reu"), P("26_Nam_Do_Khong_Lo"), P("27_Cum_3_Nam_Do"),
                                         P("19_Tang_Da_Nho_Tren_Co"), P("20_Da_Xep_Chong_Xam"), P("18_Tang_Da_Vua_Tren_Co") };
            GameObject house = P("12_Ngoi_Nha_Mai_Do");
            GameObject well = P("13_Gieng_Nuoc_Da"), scarecrow = P("14_Bu_Nhin_Bi_Ngo"), crates = P("15_Thung_Go_Doi"), barrel = P("16_Thung_Ruou_Go");
            GameObject rockBig = P("17_Tang_Da_Lon_Tren_Co"), rockMid = P("18_Tang_Da_Vua_Tren_Co"), rockStack = P("20_Da_Xep_Chong_Xam");

            GameObject boss = F("Boss_Treant_Ancient"), treant = F("Enemy_Treant"), mole = F("Enemy_Mole");
            GameObject ore = F("GatheringNode_Ore"), copper = F("GatheringNode_CopperOre"), crystal = F("GatheringNode_CrystalOre");
            GameObject deer = F("Animal_Deer_Stag"), rabbitB = F("Animal_Rabbit_Brown"), rabbitW = F("Animal_Rabbit_White");

            if (house != null && house.GetComponent<SpriteRenderer>() != null)
                spriteMat = house.GetComponent<SpriteRenderer>().sharedMaterial;

            // ---------- 2. DỌN SẠCH (giữ Map2_Ground, Map2_Walls, Map2_Spawners_Pets) ----------
            var toRemove = new List<GameObject>();
            for (int i = 0; i < root.childCount; i++)
            {
                string n = root.GetChild(i).name;
                if (n == "Map2_Ground" || n == "Map2_Walls" || n == "Map2_Spawners_Pets") continue;
                toRemove.Add(root.GetChild(i).gameObject);
            }
            foreach (var go in toRemove) UnityEngine.Object.DestroyImmediate(go);

            // ---------- 3. NỀN CỎ XANH RỪNG ĐẬM 100% ----------
            ground = GetOrCreateTilemap("Map2_Ground", 0);
            ground.ClearAllTiles();
            ground.color = Color.white;
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                    PaintGrass(x, y);

            // ---------- 4. ĐƯỜNG MÒN & ĐƯỜNG ĐÁ ----------
            // 4.1 Đấu trường Boss (tròn) trên cao nguyên
            Vector2 arena = new Vector2(30f, 51f);
            FillDisk(arena, 5.5f, false);
            // 4.2 Đường mòn trung tâm: Đấu trường -> cầu thang
            CarvePath(new[] { new Vector2(30, 46), new Vector2(29, 42), new Vector2(30.5f, 38.5f), new Vector2(30, 35), new Vector2(30, 31) }, 1.4f, false);
            // 4.3 Nhánh sang núi quặng giữa-phải
            CarvePath(new[] { new Vector2(30.5f, 38.5f), new Vector2(37, 38), new Vector2(44, 36.5f), new Vector2(50, 35.5f) }, 1.1f, false);
            // 4.4 Làng: đường đá cuội
            RectPath(2, 33, 29, 34, true);   // phố dưới -> nối thẳng tới cầu thang
            RectPath(2, 45, 29, 46, true);   // phố giữa -> nối vào đường trung tâm
            RectPath(11, 33, 12, 46, true);  // trục dọc giữa 2 dãy nhà
            Vector2[] houseCenters = { new Vector2(6.5f, 51.5f), new Vector2(17.5f, 51.5f), new Vector2(6.5f, 40f), new Vector2(17.5f, 40f) };
            RectPath(3, 47, 10, 47, true); RectPath(14, 47, 21, 47, true);   // sân trước dãy trên
            RectPath(3, 35, 10, 35, true); RectPath(14, 35, 21, 35, true);   // sân trước dãy dưới
            // 4.5 Vùng thấp: cầu thang -> vòng xoay lối vào
            int stairsBottom = CliffBaseTop - 5;
            CarvePath(new[] { new Vector2(30, stairsBottom + 0.5f), new Vector2(28.5f, 22), new Vector2(31, 18), new Vector2(30, 14.5f) }, 1.4f, false);
            Vector2 round = new Vector2(30f, 9.5f);
            FillRing(round, 1.8f, 4.6f);
            CarvePath(new[] { new Vector2(30, 5), new Vector2(30, -1) }, 1.4f, false);
            // 4.6 Đường mòn đất hữu cơ xuyên rừng
            CarvePath(new[] { new Vector2(34, 9), new Vector2(39, 7.5f), new Vector2(44, 5) }, 1.0f, false);   // -> hố trũng
            CarvePath(new[] { new Vector2(26, 10), new Vector2(19, 11), new Vector2(12, 13) }, 1.0f, false);   // -> trảng cỏ Tây

            // 4.7 Hố đất trũng góc dưới-phải
            for (int x = 45; x < W; x++)
                for (int y = 0; y <= 14; y++)
                {
                    PaintTile(x, y, tDirt, new Color(0.72f, 0.70f, 0.68f));
                    blocked[x, y] = true;
                }

            // ---------- 5. NÚI QUẶNG ĐÁ XÁM 2 TẦNG (Map2_Walls) ----------
            Tilemap walls = GetOrCreateTilemap("Map2_Walls", 5);
            walls.ClearAllTiles();
            walls.color = Color.white;
            Vector2 mC1 = new Vector2(50f, 42f); float rx1 = 7f, ry1 = 6f;
            Vector2 mC2 = new Vector2(50f, 44f); float rx2 = 4.2f, ry2 = 3.4f;
            bool In1(int x, int y) => InEllipse(x, y, mC1, rx1, ry1);
            bool In2(int x, int y) => InEllipse(x, y, mC2, rx2, ry2);
            for (int x = 40; x < W; x++)
                for (int y = 33; y < 52; y++)
                {
                    if (!In1(x, y)) continue;
                    var p = new Vector3Int(x, y, 0);
                    walls.SetTile(p, tMountain);
                    walls.SetTileFlags(p, TileFlags.None);
                    Color c;
                    if (In2(x, y))
                        c = (!In2(x, y - 1) || !In2(x, y - 2)) ? Gray(0.68f) : Gray(1.0f);   // tầng 2: vách / đỉnh
                    else
                        c = (!In1(x, y - 1) || !In1(x, y - 2)) ? Gray(0.52f) : Gray(0.82f);  // tầng 1: vách / mặt bằng
                    if ((x == 49 || x == 50) && (!In1(x, y - 1) || !In1(x, y - 2))) c = new Color(0.10f, 0.10f, 0.13f); // cửa hầm mỏ
                    walls.SetColor(p, c);
                }
            EnsureTilemapCollider(walls);

            // ---------- 6. HÀNG RÀO GỖ (Tilemap riêng có collider) ----------
            Tilemap fences = GetOrCreateTilemap("Map2_Fences", 90);
            fences.ClearAllTiles();
            if (tFenceH1 != null && tFenceV != null)
            {
                void FH(int x0, int x1, int y) { for (int x = x0; x <= x1; x++) fences.SetTile(new Vector3Int(x, y, 0), (x % 2 == 0 || tFenceH2 == null) ? tFenceH1 : tFenceH2); }
                void FV(int x, int y0, int y1) { for (int y = y0; y <= y1; y++) fences.SetTile(new Vector3Int(x, y, 0), tFenceV); }
                // Rào bao quanh làng (cổng tại 2 con phố ở cạnh phải)
                FH(1, 22, 57);
                FV(1, 33, 56);
                FV(22, 36, 44); FV(22, 48, 56);
                // Rào viền trên & phải Đấu trường Boss
                FH(23, 37, 57);
                FV(37, 47, 56);
                // Rào dọc lối vào phía dưới
                foreach (int y in new[] { 16, 19, 22 }) { FH(24, 25, y); FH(34, 35, y); BlockRect(23, y - 1, 26, y + 1); BlockRect(33, y - 1, 36, y + 1); }
                EnsureTilemapCollider(fences);
            }

            // ---------- 7. VÁCH ĐÁ NGANG LƯỢN SÓNG + CẦU THANG ----------
            Transform cliffRoot = NewContainer("Map2_Cliff_And_Stairs");
            float[] leftCenters = { 26, 22, 18, 14, 10, 6, 2 };
            int[] leftWave = { 0, -1, -1, 0, -1, 0, 0 };
            float[] rightCenters = { 34, 38, 42, 46, 50, 54, 58 };
            int[] rightWave = { 0, 1, 1, 0, -1, 0, 1 };
            for (int x = 0; x < W; x++) cliffTopAt[x] = CliffBaseTop;
            int seg = 0;
            void PlaceCliff(float cx, int top)
            {
                Sprite s = (cliffH[seg % 3] != null) ? cliffH[seg % 3] : cliffH[0];
                seg++;
                if (s == null) return;
                var go = SpawnSprite(s, cliffRoot, new Vector2(cx, top - 1.75f), $"CliffWall_{seg:00}", 0, false);
                AlignTop(go, top);
                AddBoxFromBounds(go, 0.7f, -0.12f);
                ApplyYSort(go);
                for (int x = Mathf.FloorToInt(cx - 2); x < Mathf.CeilToInt(cx + 2); x++) if (x >= 0 && x < W) cliffTopAt[x] = top;
            }
            for (int i = 0; i < leftCenters.Length; i++) PlaceCliff(leftCenters[i], CliffBaseTop + leftWave[i]);
            for (int i = 0; i < rightCenters.Length; i++) PlaceCliff(rightCenters[i], CliffBaseTop + rightWave[i]);
            if (stairs != null)
            {
                var st = SpawnSprite(stairs, cliffRoot, new Vector2(StairsCenterX, CliffBaseTop - 2.5f), "Stone_Stairs_Center", 0, false);
                AlignTop(st, CliffBaseTop);
                ApplyYSort(st);
            }
            for (int x = 0; x < W; x++)
                for (int y = cliffTopAt[x] - 5; y <= cliffTopAt[x] + 1; y++)
                    if (y >= 0 && y < H) blocked[x, y] = true;
            BlockRect(26, stairsBottom - 2, 34, CliffBaseTop + 2);

            // Vách bao hố trũng (bờ Bắc + bờ Tây, chừa lối xuống ở phía Tây-Nam)
            foreach (float cx in new[] { 46f, 50f, 54f, 58f })
            {
                Sprite s = (cliffH[seg % 3] != null) ? cliffH[seg % 3] : cliffH[0];
                var go = SpawnSprite(s, cliffRoot, new Vector2(cx, 13f), $"PitRim_N_{seg++:00}", 0, false);
                if (go == null) continue;
                AlignTop(go, 15f); AddBoxFromBounds(go, 0.7f, -0.12f); ApplyYSort(go);
            }
            if (cliffV != null)
            {
                var go = SpawnSprite(cliffV, cliffRoot, new Vector2(44.5f, 10f), "PitRim_W", 0, false);
                AlignTop(go, 13f); AddBoxFromBounds(go, 0.9f, 0f); ApplyYSort(go);
            }
            BlockRect(42, 0, W - 1, 16);

            // ---------- 8. LÀNG 4 NHÀ TRÊN CAO NGUYÊN ----------
            Transform village = NewContainer("Map2_Env_Village");
            for (int i = 0; i < houseCenters.Length; i++)
            {
                var h = SpawnPrefabAt(house, village, houseCenters[i], $"Village_House_{i + 1}");
                if (h != null && (i % 2 == 1)) foreach (var sr in h.GetComponentsInChildren<SpriteRenderer>()) sr.flipX = true;
            }
            SpawnPrefabTrunk(well, village, new Vector2(12f, 50.5f), "Village_Well");
            SpawnPrefabTrunk(scarecrow, village, new Vector2(25f, 40f), "Village_Scarecrow");
            SpawnPrefabTrunk(crates, village, new Vector2(24.3f, 36.3f), "Village_Crates");
            SpawnPrefabTrunk(barrel, village, new Vector2(26f, 36.2f), "Village_Barrel");
            BlockRect(0, 32, 23, 57);
            BlockRect(23, 35, 27, 44);

            // ---------- 9. ĐÁ & QUẶNG KHU NÚI, TINH THỂ RỪNG ĐÔNG-BẮC ----------
            Transform nodes = NewContainer("Map2_Env_Ores_And_Rocks");
            SpawnPrefabTrunk(ore, nodes, new Vector2(48f, 45.5f), "Ore_1");
            SpawnPrefabTrunk(copper, nodes, new Vector2(52f, 45.5f), "Ore_Copper_1");
            SpawnPrefabTrunk(crystal, nodes, new Vector2(50f, 46.6f), "Ore_Crystal_1");
            SpawnPrefabTrunk(ore, nodes, new Vector2(50.5f, 43.6f), "Ore_2");
            SpawnPrefabTrunk(copper, nodes, new Vector2(45f, 40.5f), "Ore_Copper_2");
            SpawnPrefabTrunk(rockBig, nodes, new Vector2(43.5f, 35.6f), "Mountain_Rock_1");
            SpawnPrefabTrunk(rockMid, nodes, new Vector2(56.5f, 36.2f), "Mountain_Rock_2");
            SpawnPrefabTrunk(rockStack, nodes, new Vector2(46.5f, 35.3f), "Mountain_Rock_3");
            SpawnPrefabTrunk(crates, nodes, new Vector2(53.2f, 35.2f), "Mine_Crates");
            SpawnPrefabTrunk(rockMid, nodes, new Vector2(25.5f, 54.5f), "Arena_Rock_W");
            SpawnPrefabTrunk(rockMid, nodes, new Vector2(34.5f, 54.5f), "Arena_Rock_E");
            SpawnPrefabTrunk(rockStack, nodes, new Vector2(48f, 7.5f), "Pit_Rock_1");
            SpawnPrefabTrunk(rockMid, nodes, new Vector2(55f, 10.5f), "Pit_Rock_2");
            BlockEllipse(mC1, rx1 + 1.5f, ry1 + 1.5f);
            BlockDisk(arena, 7.5f);
            BlockDisk(round, 5.5f);

            Vector2[] crystals = { new Vector2(41, 55), new Vector2(44, 57.5f), new Vector2(50, 56.5f), new Vector2(54, 53), new Vector2(57, 57.5f), new Vector2(47.5f, 51.5f) };
            for (int i = 0; i < crystals.Length; i++)
            {
                SpawnPrefabTrunk(crystal, nodes, crystals[i], $"Crystal_Node_{i + 1}");
                BlockDisk(crystals[i], 1.3f);
            }

            // ---------- 10. ĐỆM 2 Ô QUANH ĐƯỜNG + TRẢNG CỎ ----------
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                    if (isPath[x, y])
                        for (int dx = -2; dx <= 2; dx++)
                            for (int dy = -2; dy <= 2; dy++)
                                if (dx * dx + dy * dy <= 6) Block(x + dx, y + dy);

            var clearings = new Dictionary<string, Vector3>
            {
                { "A", new Vector3(10, 14, 3f) }, { "B", new Vector3(18, 21, 2.5f) }, { "C", new Vector3(40, 21, 2.5f) },
                { "D", new Vector3(50, 22, 2.5f) }, { "E", new Vector3(16, 4, 2.5f) }, { "F", new Vector3(40, 13, 2f) },
                { "G", new Vector3(47, 54, 2.5f) }, { "H", new Vector3(37.5f, 43, 2f) }, { "I", new Vector3(37, 34, 2f) },
                { "J", new Vector3(6, 24, 2f) }
            };
            foreach (var c in clearings.Values) BlockDisk(new Vector2(c.x, c.y), c.z);

            // ---------- 11. RỪNG RẬM MẬT ĐỘ CAO ----------
            Transform forest = NewContainer("Map2_Env_DenseForest");
            int treeCount = 0, propCount = 0;
            const float treeStep = 2.3f;
            for (float gy = -0.5f; gy < H + 0.5f; gy += treeStep)
                for (float gx = -0.5f; gx < W + 0.5f; gx += treeStep)
                {
                    Vector2 p = new Vector2(gx + R(0.2f, treeStep - 0.2f), gy + R(0.2f, treeStep - 0.2f));
                    if (!CanPlace(p, 1.0f)) continue;
                    double roll = rng.NextDouble();
                    GameObject pf = roll < 0.40 ? Pick(bigTrees) : roll < 0.78 ? Pick(midTrees) : Pick(smallTrees);
                    if (pf == null) continue;
                    var t = SpawnPrefabTrunk(pf, forest, p, $"Tree_{++treeCount:000}");
                    if (t != null && rng.NextDouble() < 0.5) foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>()) sr.flipX = true;
                    MarkOcc(p, 1.2f);
                }
            // Bụi cây, gốc cây, khúc gỗ đổ, nấm, đá - lấp khe hở
            for (float gy = 0f; gy < H; gy += 1.6f)
                for (float gx = 0f; gx < W; gx += 1.6f)
                {
                    Vector2 p = new Vector2(gx + R(0.1f, 1.5f), gy + R(0.1f, 1.5f));
                    if (!CanPlace(p, 0.7f)) continue;
                    if (rng.NextDouble() > 0.8) continue;
                    GameObject pf = rng.NextDouble() < 0.62 ? Pick(bushes) : Pick(forestProps);
                    if (pf == null) continue;
                    var b = SpawnPrefabTrunk(pf, forest, p, $"Foliage_{++propCount:000}");
                    if (b != null && rng.NextDouble() < 0.5) foreach (var sr in b.GetComponentsInChildren<SpriteRenderer>()) sr.flipX = true;
                    MarkOcc(p, 0.8f);
                }

            // ---------- 12. CHI TIẾT MẶT ĐẤT: cụm cỏ / sỏi (không collider) ----------
            Transform details = NewContainer("Map2_Env_GroundDetails");
            var tufts = new List<Sprite>(); var pebbles = new List<Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(DetailSheet))
                if (o is Sprite s) { if (s.rect.center.y < s.texture.height * 0.5f) tufts.Add(s); else pebbles.Add(s); }
            int detailCount = 0;
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                {
                    if (walls.HasTile(new Vector3Int(x, y, 0))) continue;
                    bool onPath = isPath[x, y];
                    var list = onPath ? pebbles : tufts;
                    if (list.Count == 0) continue;
                    if (rng.NextDouble() > (onPath ? 0.10 : 0.22)) continue;
                    var d = SpawnSprite(list[rng.Next(list.Count)], details, new Vector2(x + R(0.2f, 0.8f), y + R(0.2f, 0.8f)), $"Detail_{++detailCount}", 2, false);
                    if (d != null) d.GetComponent<SpriteRenderer>().flipX = rng.NextDouble() < 0.5;
                }

            // ---------- 13. BOSS, QUÁI, ĐỘNG VẬT ----------
            Transform monsters = NewContainer("Map2_Spawners_Monsters");
            Vector2 C(string k, float ox = 0, float oy = 0) => new Vector2(clearings[k].x + ox, clearings[k].y + oy);
            SpawnPrefabAt(boss, monsters, new Vector2(30f, 51.5f), "Boss_Treant_Ancient");
            string[] treantAt = { "B", "C", "D", "A", "G", "H" };
            for (int i = 0; i < treantAt.Length; i++) SpawnPrefabAt(treant, monsters, C(treantAt[i]), $"Enemy_Treant_{i + 1}");
            Vector2[] moleAt = { new Vector2(48.5f, 4.5f), new Vector2(52.5f, 8f), new Vector2(56.5f, 4f), C("E"), C("F"), C("I"), C("J"), C("A", 2f, -1.5f) };
            for (int i = 0; i < moleAt.Length; i++) SpawnPrefabAt(mole, monsters, moleAt[i], $"Enemy_Mole_{i + 1}");

            Transform animals = NewContainer("Map2_Env_Critters");
            SpawnPrefabAt(deer, animals, C("E", 1.2f, 1f), "Deer_1");
            SpawnPrefabAt(deer, animals, C("J", -0.8f, 0.6f), "Deer_2");
            SpawnPrefabAt(rabbitB, animals, C("F", 1f, 0.6f), "Rabbit_1");
            SpawnPrefabAt(rabbitW, animals, C("D", 1.5f, -1f), "Rabbit_2");
            SpawnPrefabAt(rabbitB, animals, C("B", 1.5f, 1f), "Rabbit_3");
            SpawnPrefabAt(rabbitW, animals, C("H", -1f, -0.8f), "Rabbit_4");

            // ---------- 14. BẢO TỒN 5 LINH THÚ ----------
            Transform pets = root.Find("Map2_Spawners_Pets");
            if (pets != null)
            {
                Vector2[] petAt = { C("A", -1.5f, 1f), C("E", -1.5f, -1f), C("J", 1f, -1f), C("I", -1f, 0.5f), C("C", -1.5f, -1f) };
                for (int i = 0; i < pets.childCount && i < petAt.Length; i++)
                    pets.GetChild(i).position = root.position + new Vector3(petAt[i].x, petAt[i].y, 0f);
            }

            // ---------- 15. RANH GIỚI VÔ HÌNH + ÁNH SÁNG ----------
            var boundary = new GameObject("Map2_Boundary");
            boundary.transform.SetParent(root, false);
            var edge = boundary.AddComponent<EdgeCollider2D>();
            edge.points = new[] { new Vector2(28, 0), new Vector2(0, 0), new Vector2(0, H), new Vector2(W, H), new Vector2(W, 0), new Vector2(32, 0) };

            var lightGo = new GameObject("Map2_GlobalLight2D");
            lightGo.transform.SetParent(root, false);
            lightGo.transform.localPosition = new Vector3(30f, 30f, 0f);
            var l2d = lightGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Global;
            l2d.color = Color.white;
            l2d.intensity = 1.0f;

            ground.RefreshAllTiles();
            walls.RefreshAllTiles();
            fences.RefreshAllTiles();

            // ---------- 16. LƯU & FOCUS ----------
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Selection.activeGameObject = map2Root;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(root.position + new Vector3(30f, 30f, 0f), Quaternion.identity, 34f);
                SceneView.lastActiveSceneView.Repaint();
            }
            Debug.Log($"<color=green><b>[Map2ProceduralGenerator] 🎉 HOÀN THÀNH MAP 2 v2: {treeCount} cây, {propCount} bụi/đạo cụ, {detailCount} chi tiết nền, vách đá 2 tầng + cầu thang, 4 nhà, núi đá xám 2 tầng.</b></color>");
        }

        // =====================================================================================
        // TIỆN ÍCH
        // =====================================================================================
        private static TileBase LoadTile(string name) => AssetDatabase.LoadAssetAtPath<TileBase>(DT + name + ".asset");

        private static Sprite LoadSub(string sheet, string name)
        {
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(sheet))
                if (o is Sprite s && s.name == name) return s;
            Debug.LogWarning($"[Map2ProceduralGenerator] Không tìm thấy sprite '{name}' trong {sheet}");
            return null;
        }

        private static float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
        private static T Pick<T>(T[] arr) where T : class => arr[rng.Next(arr.Length)];
        private static Color Gray(float k) => new Color(k, k, k, 1f);

        private static Tilemap GetOrCreateTilemap(string name, int order)
        {
            Transform t = root.Find(name);
            GameObject go;
            if (t != null)
            {
                go = t.gameObject;
            }
            else
            {
                go = new GameObject(name);
                go.transform.SetParent(root, false);
                go.transform.localPosition = Vector3.zero;
            }
            var tm = go.GetComponent<Tilemap>();
            if (tm == null) tm = go.AddComponent<Tilemap>();
            var tr = go.GetComponent<TilemapRenderer>();
            if (tr == null) tr = go.AddComponent<TilemapRenderer>();
            tr.sortingOrder = order;
            return tm;
        }

        private static void EnsureTilemapCollider(Tilemap tm)
        {
            var rb = tm.GetComponent<Rigidbody2D>();
            if (rb == null) rb = tm.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var comp = tm.GetComponent<CompositeCollider2D>();
            if (comp == null) comp = tm.gameObject.AddComponent<CompositeCollider2D>();
            comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
            var col = tm.GetComponent<TilemapCollider2D>();
            if (col == null) col = tm.gameObject.AddComponent<TilemapCollider2D>();
            col.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        private static Transform NewContainer(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }

        // ---- Tô tile ----
        private static void PaintGrass(int x, int y)
        {
            float n1 = Mathf.PerlinNoise(x * 0.11f + 13.7f, y * 0.11f + 7.3f);
            float n2 = Mathf.PerlinNoise(x * 0.37f + 3.1f, y * 0.37f + 91.4f);
            float k = Mathf.Lerp(0.78f, 0.93f, n1 * 0.7f + n2 * 0.3f);
            if (y < CliffBaseTop - 3) k *= 0.95f; // nền rừng vùng thấp tối hơn
            PaintTile(x, y, tGrass, new Color(k * 0.96f, k, k * 0.93f, 1f));
        }

        private static void PaintTile(int x, int y, TileBase tile, Color c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            var p = new Vector3Int(x, y, 0);
            ground.SetTile(p, tile);
            ground.SetTileFlags(p, TileFlags.None);
            ground.SetColor(p, c);
        }

        private static void PaintPath(int x, int y, bool stone)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            PaintTile(x, y, stone ? tStone[(x * 7 + y * 13) % tStone.Length] : tDirt, Color.white);
            isPath[x, y] = true;
        }

        private static void CarvePath(Vector2[] pts, float r, bool stone)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                float len = Vector2.Distance(pts[i], pts[i + 1]);
                int steps = Mathf.Max(1, Mathf.CeilToInt(len * 4f));
                for (int s = 0; s <= steps; s++)
                {
                    Vector2 p = Vector2.Lerp(pts[i], pts[i + 1], (float)s / steps);
                    for (int x = Mathf.FloorToInt(p.x - r - 1); x <= Mathf.CeilToInt(p.x + r); x++)
                        for (int y = Mathf.FloorToInt(p.y - r - 1); y <= Mathf.CeilToInt(p.y + r); y++)
                            if (Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), p) <= r) PaintPath(x, y, stone);
                }
            }
        }

        private static void RectPath(int x0, int y0, int x1, int y1, bool stone)
        {
            for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) PaintPath(x, y, stone);
        }

        private static void FillDisk(Vector2 c, float r, bool stone)
        {
            for (int x = Mathf.FloorToInt(c.x - r - 1); x <= Mathf.CeilToInt(c.x + r); x++)
                for (int y = Mathf.FloorToInt(c.y - r - 1); y <= Mathf.CeilToInt(c.y + r); y++)
                    if (Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c) <= r) PaintPath(x, y, stone);
        }

        private static void FillRing(Vector2 c, float rIn, float rOut)
        {
            for (int x = Mathf.FloorToInt(c.x - rOut - 1); x <= Mathf.CeilToInt(c.x + rOut); x++)
                for (int y = Mathf.FloorToInt(c.y - rOut - 1); y <= Mathf.CeilToInt(c.y + rOut); y++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                    if (d <= rOut && d >= rIn) PaintPath(x, y, false);
                }
        }

        private static bool InEllipse(int x, int y, Vector2 c, float rx, float ry)
        {
            float dx = (x + 0.5f - c.x) / rx, dy = (y + 0.5f - c.y) / ry;
            return dx * dx + dy * dy <= 1f;
        }

        // ---- Mặt nạ ----
        private static void Block(int x, int y) { if (x >= 0 && y >= 0 && x < W && y < H) blocked[x, y] = true; }
        private static void BlockRect(int x0, int y0, int x1, int y1) { for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) Block(x, y); }
        private static void BlockDisk(Vector2 c, float r)
        {
            for (int x = Mathf.FloorToInt(c.x - r - 1); x <= Mathf.CeilToInt(c.x + r); x++)
                for (int y = Mathf.FloorToInt(c.y - r - 1); y <= Mathf.CeilToInt(c.y + r); y++)
                    if (Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c) <= r) Block(x, y);
        }
        private static void BlockEllipse(Vector2 c, float rx, float ry)
        {
            for (int x = Mathf.FloorToInt(c.x - rx - 1); x <= Mathf.CeilToInt(c.x + rx); x++)
                for (int y = Mathf.FloorToInt(c.y - ry - 1); y <= Mathf.CeilToInt(c.y + ry); y++)
                    if (InEllipse(x, y, c, rx, ry)) Block(x, y);
        }

        private static bool CanPlace(Vector2 p, float r)
        {
            if (p.x < 0.3f || p.y < 0.3f || p.x > W - 0.3f || p.y > H - 0.3f) return false;
            for (int x = Mathf.FloorToInt(p.x - r - 1); x <= Mathf.CeilToInt(p.x + r); x++)
                for (int y = Mathf.FloorToInt(p.y - r - 1); y <= Mathf.CeilToInt(p.y + r); y++)
                {
                    if (x < 0 || y < 0 || x >= W || y >= H) continue;
                    if (Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), p) > r) continue;
                    if (blocked[x, y] || occ[x, y]) return false;
                }
            return true;
        }

        private static void MarkOcc(Vector2 p, float r)
        {
            for (int x = Mathf.FloorToInt(p.x - r - 1); x <= Mathf.CeilToInt(p.x + r); x++)
                for (int y = Mathf.FloorToInt(p.y - r - 1); y <= Mathf.CeilToInt(p.y + r); y++)
                    if (x >= 0 && y >= 0 && x < W && y < H && Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), p) <= r) occ[x, y] = true;
        }

        // ---- Spawn & sắp xếp hiển thị theo trục Y ----
        private static bool TryBounds(GameObject go, out Bounds b)
        {
            b = default; bool any = false;
            foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr.sprite == null) continue;
                if (!any) { b = sr.bounds; any = true; } else b.Encapsulate(sr.bounds);
            }
            return any;
        }

        private static void ApplyYSort(GameObject go)
        {
            if (go == null || !TryBounds(go, out Bounds b)) return;
            float localMinY = b.min.y - root.position.y;
            int baseOrder = 100 + Mathf.RoundToInt((62f - localMinY) * 10f);
            var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
            int rootOrder = srs[0].sortingOrder;
            foreach (var sr in srs) sr.sortingOrder = baseOrder + (sr.sortingOrder - rootOrder);
        }

        private static void AlignBottom(GameObject go, float localY)
        {
            if (!TryBounds(go, out Bounds b)) return;
            go.transform.position += new Vector3(0f, root.position.y + localY - b.min.y, 0f);
        }

        private static void AlignTop(GameObject go, float localY)
        {
            if (go == null || !TryBounds(go, out Bounds b)) return;
            go.transform.position += new Vector3(0f, root.position.y + localY - b.max.y, 0f);
        }

        private static void AddBoxFromBounds(GameObject go, float heightFrac, float yShiftFrac)
        {
            if (go == null || !TryBounds(go, out Bounds b)) return;
            var box = go.GetComponent<BoxCollider2D>();
            if (box == null) box = go.AddComponent<BoxCollider2D>();
            Vector3 lc = go.transform.InverseTransformPoint(b.center);
            box.size = new Vector2(b.size.x, b.size.y * heightFrac);
            box.offset = new Vector2(lc.x, lc.y + b.size.y * yShiftFrac);
        }

        private static GameObject SpawnSprite(Sprite s, Transform parent, Vector2 localPos, string name, int order, bool ysort)
        {
            if (s == null) return null;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            go.layer = 11;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s;
            if (spriteMat != null) sr.sharedMaterial = spriteMat;
            sr.sortingOrder = order;
            if (ysort) ApplyYSort(go);
            return go;
        }

        /// <summary>Đặt prefab sao cho chân (đáy sprite) nằm tại điểm trunk.</summary>
        private static GameObject SpawnPrefabTrunk(GameObject prefab, Transform parent, Vector2 trunk, string name)
        {
            if (prefab == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            go.transform.localPosition = new Vector3(trunk.x, trunk.y, 0f);
            AlignBottom(go, trunk.y - 0.3f);
            ApplyYSort(go);
            return go;
        }

        /// <summary>Đặt prefab theo tâm (pivot) - dùng cho nhà, quái, động vật.</summary>
        private static GameObject SpawnPrefabAt(GameObject prefab, Transform parent, Vector2 pos, string name)
        {
            if (prefab == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            ApplyYSort(go);
            return go;
        }
    }
}
#endif
