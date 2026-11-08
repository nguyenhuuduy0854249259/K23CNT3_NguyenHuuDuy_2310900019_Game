#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Trình tạo Map 1 Thuần Thủ Tục (2D Tilemap Generator) - v4 (Chi tiết cao).
    /// Bố cục Lưới 3 Cột x 2 Hàng (6 Biomes) theo đúng ảnh tham chiếu:
    ///   Hàng trên : Thảo nguyên 3 bậc vách đất | Rừng rậm + cổng tím | Núi đá xám 4 tầng
    ///   Hàng dưới : Làng chữ thập + sông/cầu   | Núi lửa + vịnh vuông | Hồ tròn + đảo + thác
    ///   Mép trái + mép dưới: biển xanh.
    /// Toàn bộ dựng bằng SetTile + Instantiate từ asset có sẵn (KHÔNG dùng ảnh nền).
    /// </summary>
    [InitializeOnLoad]
    public static class Map1ProceduralGenerator
    {
        public const string MainGameScenePath = "Assets/Scenes/MainGame.unity";
        private const string TriggerPath = "Temp/DoGenerateMap1.trigger";

        // Lưới Map 1 (160 x 64 ô)
        public const int MapMinX = -65, MapMaxX = 95;
        public const int MapMinY = -32, MapMaxY = 32;
        private const int W = MapMaxX - MapMinX, H = MapMaxY - MapMinY;

        // Ranh giới phân khu
        public const int TopSplit1 = 12;   // Thảo nguyên | Rừng
        public const int TopSplit2 = 58;   // Rừng | Núi đá
        public const int BotSplit1 = 7;    // Làng/thảo nguyên dưới | Núi lửa
        public const int BotSplit2 = 52;   // Núi lửa | Ốc đảo
        public const int RowSplit = 5;     // Hàng dưới (< 5) | Hàng trên (>= 5)
        public const int OceanTop = -28;   // Biển phía Nam: y <= -28

        // Thư mục asset
        private const string TileDir = "Assets/Tilemaps/ForestDemo/";
        private const string VWDir = "Assets/Sprites/VillageAndWorld/";
        private const string FPDir = "Assets/Sprites/ForestPack/";
        private const string DTDir = "Assets/Sprites/map_doi_thuong/";
        private const string PrefabFP = "Assets/Prefabs/ForestPrototype/";
        private const string PrefabPal = "Assets/Prefabs/Map3/DoiThuong_Palette/";
        private const string CliffSheet = DTDir + "PNG/multi_tiered_cliffs_and_nature.png";
        private const string GrassDetailSheet = DTDir + "PNG/ground_grass_details.png";

        // Bảng màu chuẩn
        static readonly Color CPlainsA = new Color(1.00f, 1.00f, 0.80f);
        static readonly Color CPlainsB = new Color(0.90f, 0.97f, 0.70f);
        static readonly Color CVillage = new Color(1.00f, 1.00f, 1.00f);
        static readonly Color CForestA = new Color(0.58f, 0.70f, 0.56f);
        static readonly Color CForestB = new Color(0.50f, 0.63f, 0.50f);
        static readonly Color CDirt = new Color(1.00f, 0.96f, 0.90f);
        static readonly Color CRedRoad = new Color(1.00f, 0.74f, 0.62f);
        static readonly Color CPlot = new Color(0.78f, 0.62f, 0.52f);
        static readonly Color CSand = new Color(1.00f, 1.00f, 0.86f);
        static readonly Color CCliffBrown = new Color(0.72f, 0.52f, 0.40f);
        static readonly Color CCliffBrownDk = new Color(0.56f, 0.40f, 0.31f);
        static readonly Color CRockGrey = new Color(0.62f, 0.74f, 1.00f);
        static readonly Color CRockGreyB = new Color(0.56f, 0.67f, 0.92f);
        static readonly Color CCliffGrey = new Color(0.44f, 0.52f, 0.72f);
        static readonly Color CCliffGreyDk = new Color(0.36f, 0.43f, 0.60f);
        static readonly Color CVolcRock = new Color(0.40f, 0.32f, 0.32f);
        static readonly Color CVolcRockB = new Color(0.32f, 0.26f, 0.26f);
        static readonly Color CVolcCliff = new Color(0.62f, 0.30f, 0.24f);
        static readonly Color CVolcCliffDk = new Color(0.45f, 0.20f, 0.17f);
        static readonly Color CStoneRing = new Color(0.66f, 0.78f, 1.00f);
        static readonly Color CStairLight = new Color(0.80f, 0.86f, 1.00f);
        static readonly Color CStairDark = new Color(0.55f, 0.62f, 0.80f);
        static readonly Color TintMossCliff = new Color(0.62f, 0.78f, 0.62f);
        static readonly Color CMossFace = new Color(0.50f, 0.62f, 0.56f);
        static readonly Color CMossFaceDk = new Color(0.40f, 0.50f, 0.45f);
        static readonly Color TintGreyCliff = new Color(0.66f, 0.70f, 0.80f);
        static readonly Color TintGreyRock = new Color(0.80f, 0.82f, 0.88f);

        static TileBase tGrass, tDirt, tWater, tLava, tWall;
        static Tilemap gTm, colTm;
        static Sprite[] grassDetails;
        static System.Random rng;
        static bool[,] blocked;
        static bool[,] occ;
        static int spawnCount;
        static readonly Dictionary<string, UnityEngine.Object> cache = new Dictionary<string, UnityEngine.Object>();

        static Map1ProceduralGenerator()
        {
            EditorApplication.update -= CheckTrigger;
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (!File.Exists(TriggerPath)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            try { File.Delete(TriggerPath); } catch { }
            try { GenerateProceduralMap1(); }
            catch (Exception ex) { Debug.LogError($"[Map1ProceduralGenerator] Lỗi: {ex}"); }
        }

        [MenuItem("Tools/Map 1/🔨 1. Tự Động Thiết Kế Lại Map 1 (Theo Đúng Blueprint)", priority = 10)]
        public static void GenerateProceduralMap1()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != MainGameScenePath)
                scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Single);

            Debug.Log("<color=cyan><b>[Map1ProceduralGenerator] 🚀 Dựng Map 1 v4 (6 biomes, chi tiết cao)...</b></color>");
            LoadTiles();
            rng = new System.Random(20261102);
            blocked = new bool[W, H];
            occ = new bool[W, H];
            spawnCount = 0;

            GameObject gridGo = GameObject.Find("Grid_World");
            if (gridGo == null)
            {
                gridGo = new GameObject("Grid_World");
                gridGo.AddComponent<Grid>().cellSize = Vector3.one;
            }
            gridGo.transform.position = Vector3.zero;
            for (int i = gridGo.transform.childCount - 1; i >= 0; i--)
            {
                var c = gridGo.transform.GetChild(i);
                if (c.name != "Tilemap_Ground" && c.name != "Tilemap_Obstacles")
                    UnityEngine.Object.DestroyImmediate(c.gameObject);
            }
            gTm = GetOrCreateTilemap(gridGo.transform, "Tilemap_Ground", 0);
            colTm = GetOrCreateTilemap(gridGo.transform, "Tilemap_Obstacles", 1);
            EnsureTilemapCollider(colTm);
            var colRenderer = colTm.GetComponent<TilemapRenderer>();
            if (colRenderer != null) colRenderer.enabled = false;
            gTm.ClearAllTiles();
            colTm.ClearAllTiles();

            GameObject envGo = GameObject.Find("=== WORLD ENVIRONMENT ===");
            if (envGo == null) envGo = new GameObject("=== WORLD ENVIRONMENT ===");
            envGo.transform.position = Vector3.zero;
            for (int i = envGo.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(envGo.transform.GetChild(i).gameObject);

            Transform zPlains = CreateZone(envGo.transform, "Zone_01_TopLeft_Plains");
            Transform zForest = CreateZone(envGo.transform, "Zone_02_TopCenter_Forest");
            Transform zMount = CreateZone(envGo.transform, "Zone_03_TopRight_Mountains");
            Transform zVillage = CreateZone(envGo.transform, "Zone_04_BottomLeft_Village");
            Transform zVolcano = CreateZone(envGo.transform, "Zone_05_BottomCenter_Volcano");
            Transform zOasis = CreateZone(envGo.transform, "Zone_06_BottomRight_Oasis");
            Transform zSea = CreateZone(envGo.transform, "Zone_07_Ocean_And_Roads");

            PaintBaseGround();
            BuildOceanBorders(zSea);
            BuildRoadNetwork(zSea);

            BuildZone_TopLeft_Plains(zPlains);
            BuildZone_TopCenter_Forest(zForest);
            BuildZone_TopRight_Mountains(zMount);
            BuildZone_BottomLeft_Village(zVillage);
            BuildZone_BottomCenter_Volcano(zVolcano);
            BuildZone_BottomRight_Oasis(zOasis);

            gTm.RefreshAllTiles();
            colTm.RefreshAllTiles();
            var comp = colTm.GetComponent<CompositeCollider2D>();
            if (comp != null) comp.GenerateGeometry();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.pivot = new Vector3(15f, 0f, 0f);
                SceneView.lastActiveSceneView.size = 45f;
                SceneView.lastActiveSceneView.Repaint();
            }
            Debug.Log($"<color=green><b>[Map1ProceduralGenerator] 🎉 HOÀN THÀNH MAP 1 v4 - {spawnCount} đối tượng đã đặt.</b></color>");
        }

        static void LoadTiles()
        {
            tGrass = AssetDatabase.LoadAssetAtPath<TileBase>(DTDir + "exterior_157.asset");
            if (tGrass == null) tGrass = AssetDatabase.LoadAssetAtPath<TileBase>(TileDir + "Tile_Grass.asset");
            tDirt = AssetDatabase.LoadAssetAtPath<TileBase>(DTDir + "exterior_231.asset");
            if (tDirt == null) tDirt = AssetDatabase.LoadAssetAtPath<TileBase>(TileDir + "Tile_Dirt.asset");
            tWater = AssetDatabase.LoadAssetAtPath<TileBase>(TileDir + "Tile_Water.asset");
            tLava = AssetDatabase.LoadAssetAtPath<TileBase>(TileDir + "Tile_Lava.asset");
            tWall = AssetDatabase.LoadAssetAtPath<TileBase>(TileDir + "Tile_Wall.asset");
            var list = new List<Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(GrassDetailSheet))
                if (o is Sprite s) list.Add(s);
            grassDetails = list.ToArray();
        }

        static Sprite VW(string n) => LoadCached<Sprite>(VWDir + n + ".png");
        static Sprite FPs(string n) => LoadCached<Sprite>(FPDir + n + ".png");
        static GameObject Pal(string n) => LoadCached<GameObject>(PrefabPal + n + ".prefab");
        static GameObject FP(string n) => LoadCached<GameObject>(PrefabFP + n + ".prefab");

        static T LoadCached<T>(string path) where T : UnityEngine.Object
        {
            if (cache.TryGetValue(path, out var o) && o != null) return o as T;
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a != null) cache[path] = a;
            return a;
        }

        static Sprite Cliff(string n)
        {
            string key = CliffSheet + "#" + n;
            if (cache.TryGetValue(key, out var o) && o != null) return o as Sprite;
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(CliffSheet))
                if (obj is Sprite s && s.name == n) { cache[key] = s; return s; }
            return null;
        }

        static bool InMap(int x, int y) => x >= MapMinX && x < MapMaxX && y >= MapMinY && y < MapMaxY;

        static void Paint(int x, int y, TileBase t, Color c)
        {
            if (!InMap(x, y) || t == null) return;
            var p = new Vector3Int(x, y, 0);
            gTm.SetTile(p, t);
            gTm.SetTileFlags(p, TileFlags.None);
            gTm.SetColor(p, c);
        }

        static void Block(int x, int y)
        {
            if (!InMap(x, y)) return;
            if (tWall != null) colTm.SetTile(new Vector3Int(x, y, 0), tWall);
            blocked[x - MapMinX, y - MapMinY] = true;
        }

        static void Unblock(int x, int y)
        {
            if (!InMap(x, y)) return;
            colTm.SetTile(new Vector3Int(x, y, 0), null);
        }

        static void Reserve(int x, int y)
        {
            if (InMap(x, y)) blocked[x - MapMinX, y - MapMinY] = true;
        }

        static void ReserveRect(int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) Reserve(x, y);
        }

        static bool IsBlocked(int x, int y) => !InMap(x, y) || blocked[x - MapMinX, y - MapMinY];

        static float Noise(int x, int y, float scale, float seed) => Mathf.PerlinNoise((x + 500) * scale + seed, (y + 500) * scale + seed * 0.7f);

        static void FillNoise(int x0, int y0, int x1, int y1, TileBase t, Color a, Color b, float scale = 0.12f, float seed = 1f)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                {
                    float n = Noise(x, y, scale, seed);
                    float k = Mathf.Clamp01((n - 0.35f) * 2.2f);
                    Paint(x, y, t, Color.Lerp(a, b, k));
                }
        }

        static void FillRect(int x0, int y0, int x1, int y1, TileBase t, Color c)
        {
            for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) Paint(x, y, t, c);
        }

        static void Water(int x, int y)
        {
            Paint(x, y, tWater, Color.white);
            Block(x, y);
        }

        static void Road(Color col, float radius, params Vector2[] pts)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                Vector2 a = pts[i], b = pts[i + 1];
                int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) * 3f));
                for (int s = 0; s <= steps; s++)
                {
                    Vector2 p = Vector2.Lerp(a, b, (float)s / steps);
                    int r = Mathf.CeilToInt(radius + 1.5f);
                    for (int x = Mathf.FloorToInt(p.x) - r; x <= Mathf.CeilToInt(p.x) + r; x++)
                        for (int y = Mathf.FloorToInt(p.y) - r; y <= Mathf.CeilToInt(p.y) + r; y++)
                        {
                            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), p);
                            float rr = radius + (Noise(x, y, 0.35f, 7f) - 0.5f) * 0.7f;
                            if (d <= rr)
                            {
                                if (gTm.GetTile(new Vector3Int(x, y, 0)) == tWater) continue;
                                Paint(x, y, tDirt, col);
                            }
                            if (d <= radius + 1.2f) Reserve(x, y);
                        }
                }
            }
        }

        static void CliffBand(Transform zone, int x0, int x1, int yBottom, int height, Color face, Color faceDark, Color spriteTint, params int[] stairCenters)
        {
            var stairCols = new HashSet<int>();
            foreach (int sc in stairCenters) for (int dx = -1; dx <= 1; dx++) stairCols.Add(sc + dx);

            for (int x = x0; x <= x1; x++)
            {
                for (int y = yBottom; y < yBottom + height; y++)
                {
                    if (stairCols.Contains(x))
                    {
                        Paint(x, y, tDirt, ((y - yBottom) % 2 == 0) ? CStairDark : CStairLight);
                        Reserve(x, y);
                        continue;
                    }
                    bool top = y == yBottom + height - 1;
                    bool stripe = ((x * 7 + y * 3) % 5) == 0 || Noise(x, y, 0.5f, 3f) > 0.62f;
                    Color c = top ? Color.Lerp(face, Color.white, 0.18f) : (stripe ? faceDark : face);
                    Paint(x, y, tDirt, c);
                    Block(x, y);
                }
                int sy = yBottom - 1;
                if (!stairCols.Contains(x) && InMap(x, sy) && gTm.GetTile(new Vector3Int(x, sy, 0)) != tWater)
                {
                    var p = new Vector3Int(x, sy, 0);
                    gTm.SetColor(p, gTm.GetColor(p) * new Color(0.82f, 0.82f, 0.82f, 1f));
                }
            }

            Sprite[] faces = { Cliff("cliff_horiz_1"), Cliff("cliff_horiz_2"), Cliff("cliff_horiz_3") };
            for (int x = x0; x + 3 <= x1 + 1; x += 4)
            {
                bool hitsStairs = false;
                for (int dx = 0; dx < 4; dx++) if (stairCols.Contains(x + dx)) hitsStairs = true;
                if (hitsStairs) continue;
                var s = faces[(x - x0) / 4 % 3];
                var go = Spr(s, zone, new Vector2(x + 2f, yBottom + height * 0.5f), "Cliff_Face", 4, spriteTint, 1f, null, false);
                if (go != null) go.transform.localScale = new Vector3(1f, height / 3.5f, 1f);
            }

            var stairSprite = Cliff("stairs_left_4step");
            foreach (int sc in stairCenters)
            {
                var go = Spr(stairSprite, zone, new Vector2(sc + 0.5f, yBottom + height * 0.5f), "Stone_Stairs", 5, Color.white, 1f, null, false);
                if (go != null) go.transform.localScale = new Vector3(0.75f, height / 5f, 1f);
            }
        }

        static void CliffBlock(Transform zone, int x0, int y0, int x1, int y1, Color face, Color faceDark, Color spriteTint, bool sprites = true)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                {
                    bool stripe = ((x * 3 + y * 5) % 4) == 0 || Noise(x, y, 0.45f, 11f) > 0.6f;
                    Paint(x, y, tDirt, stripe ? faceDark : face);
                    Block(x, y);
                }
            if (!sprites) return;
            var v1 = Cliff("cliff_vert_1"); var v2 = Cliff("cliff_vert_2");
            for (int x = x0; x <= x1; x += 2)
                for (int y = y0; y + 5 <= y1 + 1; y += 6)
                    Spr(((x + y) & 1) == 0 ? v1 : v2, zone, new Vector2(x + 1f, y + 3f), "Cliff_Vertical", 4, spriteTint, 1f, null, false);
        }

        static Transform CreateZone(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static Transform Group(Transform zone, string name)
        {
            var t = zone.Find(name);
            return t != null ? t : CreateZone(zone, name);
        }

        static GameObject Spr(Sprite s, Transform parent, Vector2 pos, string name, int order, Color tint, float scale = 1f, Vector2? box = null, bool count = true)
        {
            if (s == null) return null;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.sortingOrder = order;
            sr.color = tint;
            if (box.HasValue)
            {
                var b = go.AddComponent<BoxCollider2D>();
                b.size = box.Value / scale;
                b.offset = new Vector2(0f, -(s.bounds.size.y * 0.5f) + box.Value.y * 0.5f / scale);
            }
            if (count) spawnCount++;
            return go;
        }

        static GameObject Pf(GameObject prefab, Transform parent, Vector2 pos, string name = null, float scale = 1f)
        {
            if (prefab == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (go == null) return null;
            go.name = name ?? prefab.name;
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            if (!Mathf.Approximately(scale, 1f)) go.transform.localScale = Vector3.Scale(go.transform.localScale, new Vector3(scale, scale, 1f));
            spawnCount++;
            return go;
        }

        static float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
        static bool Chance(float p) => rng.NextDouble() < p;
        static T Pick<T>(params T[] arr) => arr[rng.Next(arr.Length)];

        static bool Free(Vector2 p, float r)
        {
            int ri = Mathf.CeilToInt(r);
            int cx = Mathf.FloorToInt(p.x), cy = Mathf.FloorToInt(p.y);
            for (int x = cx - ri; x <= cx + ri; x++)
                for (int y = cy - ri; y <= cy + ri; y++)
                {
                    if (!InMap(x, y)) return false;
                    if (Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), p) > r + 0.5f) continue;
                    if (blocked[x - MapMinX, y - MapMinY] || occ[x - MapMinX, y - MapMinY]) return false;
                }
            return true;
        }

        static void Occupy(Vector2 p, float r)
        {
            int ri = Mathf.CeilToInt(r);
            int cx = Mathf.FloorToInt(p.x), cy = Mathf.FloorToInt(p.y);
            for (int x = cx - ri; x <= cx + ri; x++)
                for (int y = cy - ri; y <= cy + ri; y++)
                    if (InMap(x, y) && Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), p) <= r + 0.25f)
                        occ[x - MapMinX, y - MapMinY] = true;
        }

        static int Scatter(RectInt area, float spacing, float chance, float r, Func<Vector2, bool> where, Action<Vector2> place)
        {
            int n = 0;
            for (float x = area.xMin + spacing * 0.5f; x < area.xMax; x += spacing)
                for (float y = area.yMin + spacing * 0.5f; y < area.yMax; y += spacing)
                {
                    if (!Chance(chance)) continue;
                    var p = new Vector2(x + R(-0.45f, 0.45f) * spacing, y + R(-0.45f, 0.45f) * spacing);
                    if (where != null && !where(p)) continue;
                    if (!Free(p, r)) continue;
                    Occupy(p, r);
                    place(p);
                    n++;
                }
            return n;
        }

        static int ScatterDecal(RectInt area, float spacing, float chance, Func<Vector2, bool> where, Action<Vector2> place)
        {
            int n = 0;
            for (float x = area.xMin + spacing * 0.5f; x < area.xMax; x += spacing)
                for (float y = area.yMin + spacing * 0.5f; y < area.yMax; y += spacing)
                {
                    if (!Chance(chance)) continue;
                    var p = new Vector2(x + R(-0.5f, 0.5f) * spacing, y + R(-0.5f, 0.5f) * spacing);
                    int cx = Mathf.FloorToInt(p.x), cy = Mathf.FloorToInt(p.y);
                    if (IsBlocked(cx, cy) || occ[cx - MapMinX, cy - MapMinY]) continue;
                    if (where != null && !where(p)) continue;
                    place(p);
                    n++;
                }
            return n;
        }

        static GameObject Place(Func<GameObject> make, Vector2 p, float r)
        {
            Occupy(p, r);
            return make();
        }

        static void GrassDecal(Transform t, Vector2 p, bool flowers)
        {
            Sprite s;
            if (flowers) s = Pick(VW("mini_grass_flower_gold"), VW("mini_grass_flower_pink"), VW("bush_flower_white"), VW("bush_flower_blue"), VW("bush_rose_red"), VW("serene_white_flower_herb"));
            else if (grassDetails != null && grassDetails.Length > 0 && Chance(0.6f)) s = grassDetails[rng.Next(grassDetails.Length)];
            else s = VW("mini_grass_tuft");
            Spr(s, t, p, flowers ? "Flower" : "Grass_Tuft", 2, Color.white);
        }

        static void SmallRock(Transform t, Vector2 p, Color tint)
        {
            var s = Pick(VW("rock_pebble_small"), VW("rock_flat_stepping"), VW("rock_field_medium"), VW("rock_cracked_medium"), VW("rock_mossy_medium"));
            Spr(s, t, p, "Rock_Small", 3, tint);
        }

        static bool InVillageRect(Vector2 p) => p.x >= -59f && p.x <= -15f && p.y >= -26.5f && p.y <= -1.5f;

        // =========================================================================================
        // PHA 1.1: TÔ NỀN ĐẤT (BASE GROUND)
        // =========================================================================================
        private static void PaintBaseGround()
        {
            FillNoise(MapMinX, RowSplit, TopSplit1 - 1, MapMaxY - 1, tGrass, CPlainsA, CPlainsB, 0.10f, 1.2f);
            FillNoise(TopSplit1, RowSplit, TopSplit2 - 1, MapMaxY - 1, tGrass, CForestA, CForestB, 0.14f, 4.5f);
            FillNoise(TopSplit2, RowSplit, MapMaxX - 1, MapMaxY - 1, tDirt, CRockGrey, CRockGreyB, 0.12f, 7.8f);
            FillNoise(MapMinX, MapMinY, BotSplit1 - 1, RowSplit - 1, tGrass, CVillage, CPlainsB, 0.09f, 2.3f);
            FillRect(-58, -25, -16, -2, tGrass, CVillage);
            FillNoise(BotSplit1, MapMinY, BotSplit2 - 1, RowSplit - 1, tDirt, CVolcRock, CVolcRockB, 0.11f, 9.1f);
            FillNoise(BotSplit2, MapMinY, MapMaxX - 1, RowSplit - 1, tGrass, CPlainsA, CPlainsB, 0.10f, 5.6f);
            FillNoise(TopSplit1 - 2, RowSplit, TopSplit1 + 1, MapMaxY - 1, tGrass, CPlainsB, CForestA, 0.2f, 3.1f);
            FillNoise(TopSplit2 - 2, RowSplit, TopSplit2 + 1, MapMaxY - 1, tGrass, CForestA, CRockGrey, 0.2f, 6.2f);
        }

        // =========================================================================================
        // PHA 1.2: BIỂN PHÍA NAM VÀ VỊNH NƯỚC BỜ TÂY
        // =========================================================================================
        private static void BuildOceanBorders(Transform zone)
        {
            for (int x = MapMinX; x < MapMaxX; x++)
                for (int y = MapMinY; y <= OceanTop; y++)
                    Water(x, y);

            for (int x = MapMinX; x < MapMaxX; x++)
                if (gTm.GetTile(new Vector3Int(x, -27, 0)) != tWater)
                    Paint(x, -27, tDirt, CSand);

            for (int x = MapMinX; x <= -61; x++)
                for (int y = MapMinY; y < MapMaxY; y++)
                    Water(x, y);

            int[,] riverCells = {
                { -60, -11 }, { -59, -11 }, { -58, -11 },
                { -60, -12 }, { -59, -12 }, { -58, -12 },
                { -60, -13 }, { -59, -13 }, { -58, -13 },
                { -60, -14 }, { -59, -14 }, { -58, -14 },
                { -60, -15 }, { -59, -15 }, { -58, -15 },
                { -59, -16 }, { -58, -16 }, { -57, -16 }, { -56, -16 },
                { -58, -17 }, { -57, -17 }, { -56, -17 }, { -55, -17 },
                { -57, -18 }, { -56, -18 }, { -55, -18 }, { -54, -18 },
                { -56, -19 }, { -55, -19 }, { -54, -19 }, { -53, -19 },
                { -55, -20 }, { -54, -20 }, { -53, -20 },
                { -54, -21 }, { -53, -21 }, { -52, -21 },
                { -53, -22 }, { -52, -22 },
                { -53, -23 }, { -52, -23 },
                { -53, -24 }, { -52, -24 },
                { -52, -25 }, { -51, -25 },
                { -52, -26 }, { -51, -26 },
                { -51, -27 }, { -50, -27 }
            };
            for (int i = 0; i < riverCells.GetLength(0); i++)
                Water(riverCells[i, 0], riverCells[i, 1]);
        }

        // =========================================================================================
        // PHA 1.3: MẠNG LƯỚI ĐƯỜNG MÒN LIÊN VÙNG
        // =========================================================================================
        private static void BuildRoadNetwork(Transform zone)
        {
            Road(CDirt, 1.4f,
                new Vector2(-16f, -13f),
                new Vector2(-6f, -13f),
                new Vector2(-6f, 3.5f),
                new Vector2(-9.5f, 3.5f)
            );

            Road(CDirt, 1.3f,
                new Vector2(-6f, 3.5f),
                new Vector2(3f, 3.5f),
                new Vector2(10f, 3.5f),
                new Vector2(18f, 3.5f),
                new Vector2(32f, 3.5f),
                new Vector2(32f, 5.5f)
            );

            Road(CDirt, 1.3f,
                new Vector2(32f, 3.5f),
                new Vector2(45f, 3.5f),
                new Vector2(55f, 3.5f),
                new Vector2(68f, 3.5f),
                new Vector2(80f, 3.5f),
                new Vector2(80f, 5.5f)
            );

            Road(CDirt, 1.3f,
                new Vector2(68f, 3.5f),
                new Vector2(68f, -2.5f)
            );

            Road(CDirt, 1.2f,
                new Vector2(-6f, -13f),
                new Vector2(-6f, -24f),
                new Vector2(0f, -26f)
            );
        }

        // =========================================================================================
        // 1. TOP-LEFT: THẢO NGUYÊN CỎ VÀNG + 3 BẬC VÁCH ĐẤT
        // =========================================================================================
        private static void BuildZone_TopLeft_Plains(Transform zone)
        {
            Transform gCliff = Group(zone, "01_Cliffs_And_Stairs");
            Transform gTree = Group(zone, "02_Trees");
            Transform gRock = Group(zone, "03_Rocks_Bushes");
            Transform gDecal = Group(zone, "04_Flowers_Grass");
            Transform gLife = Group(zone, "05_Animals_Enemies_Nodes");

            CliffBand(gCliff, -36, 11, 23, 4, CCliffBrown, CCliffBrownDk, Color.white, -28);
            CliffBand(gCliff, -52, -37, 19, 4, CCliffBrown, CCliffBrownDk, Color.white);
            CliffBand(gCliff, -65, -53, 15, 4, CCliffBrown, CCliffBrownDk, Color.white);

            Road(CDirt, 1.1f, new Vector2(-28f, 28.3f), new Vector2(-29.5f, 30f), new Vector2(-28f, 32.5f));
            Road(CDirt, 1.2f, new Vector2(-28f, 21.3f), new Vector2(-24f, 17f), new Vector2(-18f, 12f), new Vector2(-13f, 7f), new Vector2(-9.5f, 3.5f));
            Spr(VW("signpost_wood_east"), gCliff, new Vector2(-25.8f, 21.2f), "Signpost_Stairs", 9, Color.white);

            RectInt upper = new RectInt(-65, 5, 77, 27);
            RectInt lower = new RectInt(-65, -27, 72, 32);
            Func<Vector2, bool> okUpper = p => p.x < 11f;
            Func<Vector2, bool> okLower = p => p.x < 6.5f && !InVillageRect(p);

            Vector2[] boulders = { new Vector2(-6f, 18f), new Vector2(-5f, 16.5f), new Vector2(-20f, 13f), new Vector2(-50f, 9f), new Vector2(-42f, 26f), new Vector2(3f, 27.5f), new Vector2(-14f, 29f) };
            foreach (var b in boulders)
                if (Free(b, 1.2f)) Place(() => Pf(Pick(Pal("17_Tang_Da_Lon_Tren_Co"), Pal("20_Da_Xep_Chong_Xam")), gRock, b, "Plains_Boulder"), b, 1.2f);

            foreach (var area in new[] { upper, lower })
            {
                var ok = area.Equals(upper) ? okUpper : okLower;
                Scatter(area, 9f, 0.45f, 1f, ok, p => Pf(Pick(FP("GatheringNode_CrimsonHerb"), FP("GatheringNode_Bush"), FP("GatheringNode_MoonHerb")), gLife, p));
                Scatter(area, 11f, 0.3f, 1f, p => ok(p) && Vector2.Distance(p, new Vector2(-38f, -13f)) > 26f,
                    p => Pf(Pick(FP("Enemy_Steppe_Boar"), FP("Enemy_Steppe_Wolf"), FP("Enemy_Mole")), gLife, p));
                Scatter(area, 10f, 0.3f, 0.8f, ok, p => Pf(Pick(FP("Animal_Deer_Stag"), FP("Animal_Rabbit_Brown"), FP("Animal_Rabbit_White")), gLife, p));
                Scatter(area, 14f, 0.35f, 0.5f, ok, p => Spr(Pick(Cliff("critter_squirrel"), Cliff("critter_rabbit_pink"), Cliff("critter_deer_doe")), gLife, p, "Critter", 9, Color.white));

                Scatter(area, 5.5f, 0.36f, 1.3f, ok, p => PlainsTree(gTree, p, true));
                Scatter(area, 4f, 0.30f, 0.9f, ok, p => PlainsTree(gTree, p, false));
                Scatter(area, 4.5f, 0.32f, 0.9f, ok, p => Pf(Pick(Pal("18_Tang_Da_Vua_Tren_Co"), Pal("19_Tang_Da_Nho_Tren_Co"), Pal("17_Tang_Da_Lon_Tren_Co")), gRock, p, "Plains_Rock"));
                Scatter(area, 3.2f, 0.34f, 0.6f, ok, p =>
                {
                    if (Chance(0.5f)) Pf(Pick(Pal("23_Bui_Cay_Xanh_Lon"), Pal("24_Bui_Cay_Xanh_Tron"), Pal("25_Bui_Duong_Xi")), gRock, p, "Plains_Bush");
                    else Spr(Pick(VW("bush_round_neat"), FPs("bush"), FPs("bush-tall"), VW("bush_flower_gold")), gRock, p, "Plains_Bush", 9, Color.white);
                });
                Scatter(area, 3.5f, 0.35f, 0.4f, ok, p => SmallRock(gRock, p, Color.white));
                Scatter(area, 6f, 0.28f, 0.8f, ok, p => Spr(Pick(VW("serene_herb_red_patch"), VW("serene_herb_sprout_patch"), VW("serene_wood_log_pile")), gDecal, p, "Herb_Patch", 3, Color.white));

                ScatterDecal(area, 6.5f, 0.45f, ok, p => Pf(Pal("30_Tham_Co_Xanh_Tron"), gDecal, p, "Grass_Carpet"));
                ScatterDecal(area, 1.5f, 0.30f, ok, p => GrassDecal(gDecal, p, true));
                ScatterDecal(area, 1.3f, 0.42f, ok, p => GrassDecal(gDecal, p, false));
            }
        }

        static void PlainsTree(Transform t, Vector2 p, bool big)
        {
            if (big)
            {
                if (Chance(0.6f)) Pf(Pick(Pal("02_Cay_Soi_Vua"), Pal("05_Cay_Tao_Do_Vua"), Pal("08_Cay_Tan_Tron_Vua"), Pal("07_Cay_Tan_Tron_Lon")), t, p, "Plains_Tree");
                else Spr(Pick(VW("tree_emerald_oak"), VW("tree_bright_lime")), t, p, "Plains_Tree", 10, Color.white, 1f, new Vector2(1f, 0.6f));
            }
            else
            {
                if (Chance(0.6f)) Pf(Pick(Pal("03_Cay_Soi_Nho"), Pal("06_Cay_Tao_Do_Nho"), Pal("09_Cay_Tan_Tron_Nho")), t, p, "Plains_SmallTree");
                else Spr(VW("tree_young_sapling"), t, p, "Plains_Sapling", 10, Color.white, 1f, new Vector2(0.6f, 0.4f));
            }
        }

        // =========================================================================================
        // 2. TOP-CENTER: RỪNG RẬM CỎ XANH ĐẬM + CỔNG TÍM
        // =========================================================================================
        private static void BuildZone_TopCenter_Forest(Transform zone)
        {
            Transform gCliff = Group(zone, "01_Cliffs_Stairs_Shrine");
            Transform gTree = Group(zone, "02_Dense_Trees");
            Transform gUnder = Group(zone, "03_Undergrowth");
            Transform gDecal = Group(zone, "04_Ground_Details");
            Transform gLife = Group(zone, "05_Enemies_Nodes");

            CliffBlock(gCliff, 12, 9, 12, 26, CMossFace, CMossFaceDk, TintMossCliff, false);
            CliffBand(gCliff, 12, 57, 5, 4, CMossFace, CMossFaceDk, TintMossCliff, 32);

            Vector2 shrine = new Vector2(33f, 23.5f);
            for (int x = 28; x <= 38; x++)
                for (int y = 18; y <= 29; y++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), shrine);
                    if (d <= 2.2f) Paint(x, y, tDirt, new Color(0.78f, 0.62f, 0.98f));
                    else if (d <= 3.6f) Paint(x, y, tDirt, ((x + y) & 1) == 0 ? CStoneRing : CRockGreyB);
                    if (d <= 4.2f) Reserve(x, y);
                }
            Spr(Cliff("magic_portal_glowing"), gCliff, shrine, "Forest_Purple_Portal_Glow", 2, Color.white, 0.45f);
            Pf(FP("Waystone_Monument"), gCliff, shrine + new Vector2(0f, 0.3f), "Forest_Purple_Waystone");
            for (int i = 0; i < 10; i++)
            {
                float ang = i * 36f * Mathf.Deg2Rad;
                if (Mathf.Abs(Mathf.Sin(ang)) > 0.85f) continue;
                var sp = shrine + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 3.9f;
                Spr(Pick(VW("rock_field_medium"), VW("rock_cracked_medium")), gCliff, sp, "Shrine_Standing_Stone", 9, TintGreyRock, 1f, new Vector2(1f, 0.6f));
            }

            Color forestPath = new Color(0.86f, 0.80f, 0.72f);
            Road(forestPath, 1.1f, new Vector2(32f, 10.3f), new Vector2(32f, 14f), new Vector2(29f, 17f), new Vector2(29.5f, 19.5f), new Vector2(31.5f, 21.2f));
            Road(forestPath, 1.0f, new Vector2(32f, 14f), new Vector2(36.5f, 16.5f), new Vector2(37f, 20f), new Vector2(35f, 21.5f));
            Road(forestPath, 1.0f, new Vector2(33f, 26.5f), new Vector2(31f, 29f), new Vector2(30f, 32.5f));
            Road(forestPath, 0.9f, new Vector2(29f, 17f), new Vector2(21f, 19f), new Vector2(16f, 24f), new Vector2(13.5f, 28.5f), new Vector2(10f, 29f));
            Road(forestPath, 0.9f, new Vector2(37f, 20f), new Vector2(46f, 18f), new Vector2(53f, 12f), new Vector2(60f, 10f));
            Spr(VW("street_lantern_wood"), gCliff, new Vector2(30.3f, 11f), "Forest_Lantern", 9, Color.white);
            Spr(VW("street_lantern_wood"), gCliff, new Vector2(34.0f, 11f), "Forest_Lantern", 9, Color.white);

            RectInt area = new RectInt(13, 9, 45, 23);
            Func<Vector2, bool> ok = p => p.x > 13.2f && p.x < 57.5f;

            Scatter(area, 9f, 0.5f, 1f, ok, p => Pf(Pick(FP("Enemy_Treant"), FP("Enemy_Treant"), FP("Enemy_Mole"), FP("Enemy_Forest_WhiteWolf")), gLife, p));
            Scatter(area, 8f, 0.55f, 0.8f, ok, p => Pf(Pick(FP("GatheringNode_MoonHerb"), FP("GatheringNode_Bush"), FP("GatheringNode_CrimsonHerb")), gLife, p));
            Pf(FP("Enemy_Mole_Leader"), gLife, new Vector2(46f, 26f));

            Scatter(area, 2.1f, 0.93f, 0.85f, ok, p => ForestTree(gTree, p));
            Scatter(area, 1.6f, 0.55f, 0.4f, ok, p =>
            {
                float r = (float)rng.NextDouble();
                if (r < 0.35f) Pf(Pick(Pal("23_Bui_Cay_Xanh_Lon"), Pal("24_Bui_Cay_Xanh_Tron"), Pal("25_Bui_Duong_Xi")), gUnder, p, "Forest_Bush");
                else if (r < 0.55f) Pf(Pick(Pal("26_Nam_Do_Khong_Lo"), Pal("27_Cum_3_Nam_Do")), gUnder, p, "Forest_Mushroom");
                else if (r < 0.70f) Pf(Pick(Pal("21_Goc_Cay_Co_Thu"), Pal("22_Than_Go_Muc_Reu")), gUnder, p, "Forest_Stump_Log");
                else if (r < 0.85f) Spr(Pick(VW("serene_mushroom_cluster"), FPs("bush-tall"), FPs("trunk")), gUnder, p, "Forest_Undergrowth", 9, Color.white);
                else SmallRock(gUnder, p, new Color(0.75f, 0.85f, 0.75f));
            });
            ScatterDecal(area, 1.2f, 0.55f, ok, p => GrassDecal(gDecal, p, Chance(0.15f)));
        }

        static void ForestTree(Transform t, Vector2 p)
        {
            float r = (float)rng.NextDouble();
            if (r < 0.22f)
                Spr(FPs("tree-dried"), t, p, "Forest_Tree_Dried", 10, new Color(0.88f, 0.78f, 0.68f), 0.42f, new Vector2(0.9f, 0.6f));
            else if (r < 0.55f)
                Pf(Pick(Pal("01_Cay_Soi_Co_Thu_Lon"), Pal("02_Cay_Soi_Vua"), Pal("03_Cay_Soi_Nho"), Pal("07_Cay_Tan_Tron_Lon"), Pal("08_Cay_Tan_Tron_Vua"), Pal("09_Cay_Tan_Tron_Nho")), t, p, "Forest_Tree_Oak");
            else if (r < 0.75f)
                Pf(Pick(Pal("10_Cay_Thong_Xanh_Lon"), Pal("11_Cay_Thong_Xanh_Nho")), t, p, "Forest_Tree_Pine");
            else
                Spr(Pick(VW("tree_deep_teal"), VW("tree_emerald_oak"), VW("tree_forest_pine")), t, p, "Forest_Tree", 10, new Color(0.85f, 0.92f, 0.85f), 1f, new Vector2(1f, 0.6f));
        }

        // =========================================================================================
        // 3. TOP-RIGHT: NÚI ĐÁ XÁM 4 TẦNG
        // =========================================================================================
        private static void BuildZone_TopRight_Mountains(Transform zone)
        {
            Transform gCliff = Group(zone, "01_Tiers_And_Stairs");
            Transform gTree = Group(zone, "02_Pines");
            Transform gRock = Group(zone, "03_Rocks_Ores");
            Transform gLife = Group(zone, "04_Portals_Enemies_Boss");

            CliffBand(gCliff, 58, 94, 5, 3, CCliffGrey, CCliffGreyDk, TintGreyCliff, 80);
            CliffBlock(gCliff, 58, 12, 62, 31, CCliffGrey, CCliffGreyDk, TintGreyCliff);
            CliffBand(gCliff, 63, 94, 12, 3, CCliffGrey, CCliffGreyDk, TintGreyCliff, 72);
            CliffBlock(gCliff, 63, 19, 67, 31, CCliffGrey, CCliffGreyDk, TintGreyCliff);
            CliffBand(gCliff, 68, 94, 19, 3, CCliffGrey, CCliffGreyDk, TintGreyCliff, 84);
            CliffBlock(gCliff, 68, 25, 73, 31, CCliffGrey, CCliffGreyDk, TintGreyCliff);
            CliffBand(gCliff, 74, 94, 25, 3, CCliffGrey, CCliffGreyDk, TintGreyCliff, 78);

            Road(CDirt, 1.0f, new Vector2(80f, 9.3f), new Vector2(76f, 9.7f), new Vector2(72f, 10.1f));
            Road(CDirt, 1.0f, new Vector2(72f, 16.3f), new Vector2(78f, 17f), new Vector2(84f, 17.2f));
            Road(CDirt, 0.9f, new Vector2(84f, 23.3f), new Vector2(81f, 23.4f), new Vector2(78f, 23.3f));
            Road(CDirt, 1.0f, new Vector2(78f, 29.6f), new Vector2(84f, 30f), new Vector2(89f, 30f));

            Spr(Cliff("magic_portal_glowing"), gLife, new Vector2(66f, 16.8f), "Mountain_Purple_Portal_Tier2", 3, Color.white, 0.38f);
            ReserveRect(64, 15, 68, 18);
            Spr(Cliff("magic_portal_glowing"), gLife, new Vector2(91.5f, 29.8f), "Mountain_Purple_Portal_Summit", 3, Color.white, 0.38f);
            ReserveRect(89, 28, 94, 31);
            Pf(FP("Peak_Monument_2"), gLife, new Vector2(75.5f, 30f), "Summit_Monument");
            Pf(FP("Boss_Summit_WolfKing"), gLife, new Vector2(85f, 29.3f));

            RectInt area = new RectInt(58, 8, 37, 24);
            Func<Vector2, bool> ok = p => p.x > 58.5f && p.x < 94.5f;
            Scatter(area, 6f, 0.5f, 0.9f, ok, p =>
            {
                if (Chance(0.55f)) Pf(Pick(FP("GatheringNode_Ore"), FP("GatheringNode_CopperOre"), FP("GatheringNode_CrystalOre")), gRock, p);
                else Spr(Pick(VW("rock_ore_node"), VW("rock_crystal_node"), VW("rock_dark_ore")), gRock, p, "Ore_Node", 9, Color.white, 1f, new Vector2(1f, 0.6f));
            });
            Scatter(area, 9f, 0.45f, 1f, ok, p => Pf(Pick(FP("Enemy_Steppe_Wolf"), FP("Enemy_Forest_WhiteWolf")), gLife, p));
            Scatter(area, 2.6f, 0.55f, 0.9f, ok, p =>
            {
                if (Chance(0.65f)) Pf(Pick(Pal("10_Cay_Thong_Xanh_Lon"), Pal("11_Cay_Thong_Xanh_Nho")), gTree, p, "Mountain_Pine");
                else Spr(VW("tree_forest_pine"), gTree, p, "Mountain_Pine", 10, Color.white, 1f, new Vector2(1f, 0.6f));
            });
            Scatter(area, 3.2f, 0.45f, 0.7f, ok, p =>
            {
                if (Chance(0.4f)) Pf(Pick(Pal("20_Da_Xep_Chong_Xam"), Pal("19_Tang_Da_Nho_Tren_Co")), gRock, p, "Mountain_Rock");
                else Spr(Pick(VW("rock_boulder_large"), VW("rock_cracked_medium"), VW("rock_cliff_large")), gRock, p, "Mountain_Rock", 9, TintGreyRock, 1f, new Vector2(1.2f, 0.7f));
            });
            ScatterDecal(area, 1.8f, 0.35f, ok, p => SmallRock(gRock, p, TintGreyRock));
            ScatterDecal(area, 2.2f, 0.25f, ok, p => GrassDecal(gRock, p, false));
        }

        // =========================================================================================
        // 4. BOTTOM-LEFT: NGÔI LÀNG TRÊN NỀN CỎ XANH NHẠT
        // =========================================================================================
        private static void BuildZone_BottomLeft_Village(Transform zone)
        {
            Transform gFence = Group(zone, "01_Fences_And_Gates");
            Transform gBridges = Group(zone, "02_Bridges");
            Transform gHouses = Group(zone, "03_Houses_And_Plots");
            Transform gProps = Group(zone, "04_Village_Props");
            Transform gNPCs = Group(zone, "05_NPCs_And_Monuments");
            Transform gDecal = Group(zone, "06_Garden_And_Flowers");

            Vector2 center = new Vector2(-38f, -13f);

            Road(CRedRoad, 2.0f, new Vector2(-58f, -13f), new Vector2(-16f, -13f));
            Road(CRedRoad, 2.0f, new Vector2(-38f, -2f), new Vector2(-38f, -25f));

            for (int x = -41; x <= -35; x++)
            {
                for (int y = -16; y <= -10; y++)
                {
                    Paint(x, y, tDirt, CRedRoad);
                    Reserve(x, y);
                }
            }

            Spr(VW("street_lantern_wood"), gProps, new Vector2(-41f, -10f), "Village_Lantern_NW", 9, Color.white);
            Spr(VW("street_lantern_wood"), gProps, new Vector2(-35f, -10f), "Village_Lantern_NE", 9, Color.white);
            Spr(VW("street_lantern_wood"), gProps, new Vector2(-41f, -16f), "Village_Lantern_SW", 9, Color.white);
            Spr(VW("street_lantern_wood"), gProps, new Vector2(-35f, -16f), "Village_Lantern_SE", 9, Color.white);

            Pf(FP("Prop_Animated_Campfire"), gProps, center, "Village_Campfire");

            var bH = VW("bridge_wood_horizontal");
            Spr(bH, gBridges, new Vector2(-60f, -13f), "Bridge_Wood_West_1", 8, Color.white, 1f, null, false);
            Spr(bH, gBridges, new Vector2(-58f, -13f), "Bridge_Wood_West_2", 8, Color.white, 1f, null, false);
            for (int bx = -61; bx <= -57; bx++)
            {
                Unblock(bx, -13);
                Unblock(bx, -14);
                Reserve(bx, -13);
                Reserve(bx, -14);
            }

            var fH = VW("fence_wood_horizontal");
            for (float x = -57.5f; x <= -16.5f; x += 3f)
            {
                if (Mathf.Abs(x - (-38f)) < 2.5f) continue;
                Spr(fH, gFence, new Vector2(x, -2f), "Fence_North", 6, Color.white, 1f, new Vector2(3f, 0.6f));
                Reserve(Mathf.RoundToInt(x), -2);
            }
            for (float x = -57.5f; x <= -16.5f; x += 3f)
            {
                if (Mathf.Abs(x - (-38f)) < 2.5f) continue;
                Spr(fH, gFence, new Vector2(x, -25f), "Fence_South", 6, Color.white, 1f, new Vector2(3f, 0.6f));
                Reserve(Mathf.RoundToInt(x), -25);
            }
            for (float y = -24f; y <= -3f; y += 2f)
            {
                if (Mathf.Abs(y - (-13f)) < 2f) continue;
                Spr(fH, gFence, new Vector2(-16f, y), "Fence_East", 6, Color.white, 1f, new Vector2(0.6f, 2f));
                Reserve(-16, Mathf.RoundToInt(y));
            }
            for (float y = -24f; y <= -3f; y += 2f)
            {
                if (Mathf.Abs(y - (-13f)) < 2f) continue;
                if (gTm.GetTile(new Vector3Int(-58, Mathf.RoundToInt(y), 0)) == tWater) continue;
                Spr(fH, gFence, new Vector2(-58f, y), "Fence_West", 6, Color.white, 1f, new Vector2(0.6f, 2f));
                Reserve(-58, Mathf.RoundToInt(y));
            }

            Spr(VW("village_gate_pillar"), gFence, new Vector2(-40f, -2f), "Gate_North_L", 7, Color.white);
            Spr(VW("village_gate_pillar"), gFence, new Vector2(-36f, -2f), "Gate_North_R", 7, Color.white);
            Spr(VW("village_gate_pillar"), gFence, new Vector2(-16f, -11.5f), "Gate_East_T", 7, Color.white);
            Spr(VW("village_gate_pillar"), gFence, new Vector2(-16f, -14.5f), "Gate_East_B", 7, Color.white);
            Spr(VW("village_gate_pillar"), gFence, new Vector2(-57f, -11.5f), "Gate_West_T", 7, Color.white);
            Spr(VW("village_gate_pillar"), gFence, new Vector2(-57f, -14.5f), "Gate_West_B", 7, Color.white);

            BuildVillageQuarter(gHouses, gProps, gDecal,
                plotMin: new Vector2Int(-54, -10), plotMax: new Vector2Int(-44, -4),
                housePositions: new[] { new Vector2(-51f, -6f), new Vector2(-47f, -6f) },
                houseSprites: new[] { VW("house_red_cottage"), VW("house_red_shop") },
                isRed: true
            );

            BuildVillageQuarter(gHouses, gProps, gDecal,
                plotMin: new Vector2Int(-32, -10), plotMax: new Vector2Int(-22, -4),
                housePositions: new[] { new Vector2(-29f, -6f), new Vector2(-25f, -6f) },
                houseSprites: new[] { VW("house_blue_cottage"), VW("house_blue_guild_hall") },
                isRed: false
            );

            BuildVillageQuarter(gHouses, gProps, gDecal,
                plotMin: new Vector2Int(-54, -22), plotMax: new Vector2Int(-44, -16),
                housePositions: new[] { new Vector2(-51f, -19f), new Vector2(-47f, -19f) },
                houseSprites: new[] { VW("house_green_cottage"), VW("house_blue_shop") },
                isRed: false
            );

            BuildVillageQuarter(gHouses, gProps, gDecal,
                plotMin: new Vector2Int(-32, -22), plotMax: new Vector2Int(-22, -16),
                housePositions: new[] { new Vector2(-29f, -19f), new Vector2(-25f, -19f) },
                houseSprites: new[] { VW("house_red_villa"), VW("house_red_cottage") },
                isRed: true
            );

            Spr(Cliff("magic_portal_glowing"), gNPCs, new Vector2(-54.5f, -10f), "Village_Portal_Glow", 2, Color.white, 0.35f);
            Pf(FP("Waystone_Monument"), gNPCs, new Vector2(-54.5f, -9.8f), "Village_Purple_Monument");
            ReserveRect(-56, -11, -53, -9);

            Pf(Pal("13_Gieng_Nuoc_Da"), gProps, new Vector2(-35f, -9f), "Stone_Well");
            ReserveRect(-36, -10, -34, -8);

            Pf(Pal("14_Bu_Nhin_Bi_Ngo"), gProps, new Vector2(-46f, -10.5f), "Pumpkin_Scarecrow");
            Pf(Pal("15_Thung_Go_Doi"), gProps, new Vector2(-33f, -7f), "Wood_Crates");
            Pf(Pal("16_Thung_Ruou_Go"), gProps, new Vector2(-45f, -7f), "Wine_Barrels");
            Pf(FP("WorldProp_FruitBasket"), gProps, new Vector2(-37f, -14.5f));
            Pf(FP("WorldProp_StrawHat"), gProps, new Vector2(-36.2f, -14.2f));

            Spr(VW("flowerbox_red"), gProps, new Vector2(-51f, -7.5f), "Flowerbox_1", 9, Color.white);
            Spr(VW("flowerbox_blue_gold"), gProps, new Vector2(-29f, -7.5f), "Flowerbox_2", 9, Color.white);
            Spr(VW("planter_long_flowers"), gProps, new Vector2(-25f, -7.5f), "Planter_1", 9, Color.white);

            Pf(FP("NPC_VillageElder"), gNPCs, new Vector2(-36f, -12f), "NPC_Elder");
            Pf(FP("NPC_Merchant_Lina"), gNPCs, new Vector2(-40f, -12f), "NPC_Merchant");
            Pf(FP("NPC_Alchemist_Elena"), gNPCs, new Vector2(-49f, -8f), "NPC_Alchemist");
            Pf(FP("NPC_Blacksmith_Barnaby"), gNPCs, new Vector2(-27f, -8f), "NPC_Blacksmith");
            Pf(FP("NPC_GuildMaster_Carlos"), gNPCs, new Vector2(-31f, -15f), "NPC_Carlos");
            Pf(FP("NPC_Villager_Wanderer"), gNPCs, new Vector2(-41f, -15f), "NPC_Wanderer");

            Spr(VW("signpost_wood_town"), gProps, new Vector2(-36.5f, -3f), "Town_Signpost", 9, Color.white);

            Pf(Pal("30_Tham_Co_Xanh_Tron"), gDecal, new Vector2(-38f, -7f), "Village_Carpet_N");
            Pf(Pal("30_Tham_Co_Xanh_Tron"), gDecal, new Vector2(-38f, -20f), "Village_Carpet_S");

            for (float fx = -56f; fx <= -18f; fx += 2.2f)
            {
                for (float fy = -24f; fy <= -3f; fy += 2.2f)
                {
                    if (IsBlocked(Mathf.FloorToInt(fx), Mathf.FloorToInt(fy))) continue;
                    if (Chance(0.4f)) GrassDecal(gDecal, new Vector2(fx, fy), Chance(0.45f));
                }
            }
        }

        private static void BuildVillageQuarter(Transform gHouses, Transform gProps, Transform gDecal,
            Vector2Int plotMin, Vector2Int plotMax, Vector2[] housePositions, Sprite[] houseSprites, bool isRed)
        {
            for (int x = plotMin.x; x <= plotMax.x; x++)
                for (int y = plotMin.y; y <= plotMax.y; y++)
                    Paint(x, y, tDirt, CPlot);

            var fH = VW("fence_wood_horizontal");
            for (float x = plotMin.x + 0.5f; x <= plotMax.x - 0.5f; x += 2.5f)
            {
                Spr(fH, gProps, new Vector2(x, plotMin.y + 0.2f), "Yard_Fence_B", 6, Color.white, 0.8f, new Vector2(2f, 0.4f));
                Reserve(Mathf.RoundToInt(x), plotMin.y);
            }

            for (int i = 0; i < housePositions.Length && i < houseSprites.Length; i++)
            {
                var hp = housePositions[i];
                var hs = houseSprites[i];
                Spr(hs, gHouses, hp, hs.name, 10, Color.white, 1f, new Vector2(2.6f, 1.8f));
                ReserveRect(Mathf.FloorToInt(hp.x - 1.5f), Mathf.FloorToInt(hp.y - 1.2f), Mathf.CeilToInt(hp.x + 1.5f), Mathf.CeilToInt(hp.y + 0.8f));
            }
        }

        // =========================================================================================
        // 5. BOTTOM-CENTER: MIỆNG NÚI LỬA DUNG NHAM & VỊNH NƯỚC VUÔNG
        // =========================================================================================
        private static void BuildZone_BottomCenter_Volcano(Transform zone)
        {
            Transform gCliff = Group(zone, "01_Volcano_Cliffs_And_Stairs");
            Transform gCrater = Group(zone, "02_Crater_And_Lava");
            Transform gBay = Group(zone, "03_Square_Water_Bay");
            Transform gRocks = Group(zone, "04_Dark_Rocks_And_Ores");
            Transform gLife = Group(zone, "05_Boss_And_Enemies");

            for (int x = 7; x <= 14; x++)
                for (int y = -28; y <= -17; y++)
                    Water(x, y);

            for (int y = -28; y <= -17; y++) Paint(6, y, tDirt, CSand);
            for (int x = 7; x <= 15; x++) Paint(x, -16, tDirt, CVolcRock);
            for (int y = -28; y <= -17; y++) Paint(15, y, tDirt, CVolcRock);

            Vector2 vCenter = new Vector2(29.5f, -9.5f);

            for (int x = 18; x <= 41; x++)
            {
                for (int y = -20; y <= 2; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), vCenter);
                    if (dist <= 11.8f)
                    {
                        Paint(x, y, tDirt, (dist > 9.5f) ? CVolcCliff : CVolcRockB);
                        if (dist > 9.2f) Block(x, y);
                    }
                }
            }

            for (int x = 21; x <= 38; x++)
            {
                for (int y = -18; y <= 0; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), vCenter);
                    if (dist <= 8.5f) Paint(x, y, tDirt, CRockGreyB);
                }
            }

            for (int x = 23; x <= 36; x++)
            {
                for (int y = -16; y <= -3; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), vCenter);
                    if (dist <= 6.8f && dist > 3.0f)
                    {
                        Paint(x, y, tLava, Color.white);
                        Block(x, y);
                    }
                }
            }

            for (int x = 27; x <= 32; x++)
            {
                for (int y = -12; y <= -7; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), vCenter);
                    if (dist <= 3.0f)
                    {
                        Paint(x, y, tDirt, new Color(0.25f, 0.18f, 0.18f));
                        Block(x, y);
                    }
                }
            }

            Pf(FP("Boss_Volcano_DemonApe"), gLife, vCenter, "Boss_DemonApe");
            ReserveRect(27, -12, 32, -7);

            CliffBand(gCliff, 20, 39, -17, 3, CVolcCliff, CVolcCliffDk, new Color(0.7f, 0.45f, 0.4f), 29);
            CliffBand(gCliff, 18, 41, -24, 3, CVolcCliff, CVolcCliffDk, new Color(0.7f, 0.45f, 0.4f), 29);

            Road(CDirt, 1.4f,
                new Vector2(29.5f, -14.5f),
                new Vector2(29.5f, -17.5f),
                new Vector2(29.5f, -24.5f),
                new Vector2(29.5f, -27.5f)
            );

            RectInt volcArea = new RectInt(16, -26, 34, 28);
            Scatter(volcArea, 7f, 0.45f, 1f, p => p.x > 16.5f && p.x < 50f && Vector2.Distance(p, vCenter) > 7f,
                p => Pf(Pick(FP("GatheringNode_Ore"), FP("GatheringNode_CopperOre")), gRocks, p));
            Scatter(volcArea, 8f, 0.4f, 1f, p => p.x > 16.5f && p.x < 50f && Vector2.Distance(p, vCenter) > 8f,
                p => Pf(Pick(FP("Enemy_Mole"), FP("Enemy_Mole_Leader")), gLife, p));

            Scatter(volcArea, 4f, 0.4f, 0.8f, p => p.x > 16.5f && p.x < 50f && Vector2.Distance(p, vCenter) > 7f,
                p => Spr(Pick(VW("rock_boulder_large"), VW("rock_cliff_large"), VW("rock_dark_ore")), gRocks, p, "Volcano_Rock", 9, new Color(0.55f, 0.45f, 0.45f), 1f, new Vector2(1.2f, 0.7f)));
        }

        // =========================================================================================
        // 6. BOTTOM-RIGHT: ỐC ĐẢO & HỒ NƯỚC TRÒN
        // =========================================================================================
        private static void BuildZone_BottomRight_Oasis(Transform zone)
        {
            Transform gCliff = Group(zone, "01_North_Cliff_And_Stairs");
            Transform gLake = Group(zone, "02_Lake_And_Sand");
            Transform gIsland = Group(zone, "03_Island_And_Bridges");
            Transform gWaterfall = Group(zone, "04_South_Waterfall");
            Transform gTrees = Group(zone, "05_Oasis_Trees_And_Plants");
            Transform gLife = Group(zone, "06_Boss_And_Water_Slimes");

            CliffBand(gCliff, 54, 94, 1, 3, CCliffBrown, CCliffBrownDk, Color.white, 68);

            Vector2 lakeCenter = new Vector2(73.5f, -14.5f);
            float rSand = 13.5f;
            float rWater = 10.5f;
            float rIsland = 4.0f;

            for (int x = 58; x <= 89; x++)
            {
                for (int y = -28; y <= 0; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), lakeCenter);
                    if (dist <= rIsland) Paint(x, y, tGrass, CVillage);
                    else if (dist <= rWater) Water(x, y);
                    else if (dist <= rSand) Paint(x, y, tDirt, CSand);
                }
            }

            Pf(Pal("01_Cay_Soi_Co_Thu_Lon"), gIsland, lakeCenter + new Vector2(0f, 0.8f), "Island_Ancient_Oak");
            Pf(Pal("18_Tang_Da_Vua_Tren_Co"), gIsland, lakeCenter + new Vector2(1.5f, -1.2f), "Island_Rock");
            Spr(VW("bush_flower_gold"), gIsland, lakeCenter + new Vector2(-1.5f, -0.8f), "Island_Flower_1", 9, Color.white);
            Spr(VW("mini_grass_flower_pink"), gIsland, lakeCenter + new Vector2(0.5f, -1.8f), "Island_Flower_2", 9, Color.white);
            ReserveRect(71, -16, 76, -13);

            var bH = VW("bridge_wood_horizontal");
            Spr(bH, gIsland, new Vector2(65f, -14.5f), "Island_Bridge_West_1", 8, Color.white, 1f, null, false);
            Spr(bH, gIsland, new Vector2(68f, -14.5f), "Island_Bridge_West_2", 8, Color.white, 1f, null, false);
            for (int bx = 63; bx <= 69; bx++)
            {
                Unblock(bx, -14);
                Unblock(bx, -15);
                Reserve(bx, -14);
                Reserve(bx, -15);
            }

            Spr(bH, gIsland, new Vector2(73.5f, -18.5f), "Island_Pier_South", 8, Color.white, 0.85f, null, false);

            for (int y = -28; y <= -24; y++)
                for (int x = 71; x <= 76; x++)
                    Water(x, y);

            CliffBlock(gWaterfall, 68, -27, 70, -24, CCliffBrown, CCliffBrownDk, Color.white, false);
            CliffBlock(gWaterfall, 77, -27, 79, -24, CCliffBrown, CCliffBrownDk, Color.white, false);

            Pf(FP("Waterfall"), gWaterfall, new Vector2(73.5f, -26f), "South_Ocean_Waterfall");

            Pf(FP("Prop_Animated_WaterWaves"), gLake, new Vector2(68f, -10f), "Lake_Waves_1");
            Pf(FP("Prop_Animated_WaterWaves"), gLake, new Vector2(78f, -11f), "Lake_Waves_2");
            Pf(FP("Prop_Animated_WaterWaves"), gLake, new Vector2(78f, -18f), "Lake_Waves_3");

            Pf(FP("Boss_Lake_Leviathan"), gLife, new Vector2(73.5f, -8.5f), "Boss_Leviathan");
            Pf(FP("Enemy_Water_Slime"), gLife, new Vector2(66f, -11f));
            Pf(FP("Enemy_Water_Slime"), gLife, new Vector2(81f, -14f));
            Pf(FP("Enemy_Water_Slime"), gLife, new Vector2(73f, -21f));

            RectInt oasisArea = new RectInt(53, -27, 41, 28);
            Func<Vector2, bool> okOasis = p => Vector2.Distance(p, lakeCenter) > rWater + 1f;

            Scatter(oasisArea, 4.5f, 0.38f, 1.2f, okOasis,
                p => Pf(Pick(Pal("04_Cay_Tao_Do_Lon"), Pal("05_Cay_Tao_Do_Vua"), Pal("07_Cay_Tan_Tron_Lon")), gTrees, p, "Oasis_Tree"));
            Scatter(oasisArea, 3.5f, 0.35f, 0.6f, okOasis,
                p => Spr(Pick(VW("bush_flower_gold"), VW("bush_flower_blue"), VW("bush_flower_white")), gTrees, p, "Oasis_Bush", 9, Color.white));
            Scatter(oasisArea, 7f, 0.4f, 0.8f, okOasis,
                p => Pf(Pick(FP("GatheringNode_Bush"), FP("GatheringNode_MoonHerb")), gTrees, p));
            ScatterDecal(oasisArea, 1.4f, 0.35f, okOasis,
                p => GrassDecal(gTrees, p, Chance(0.5f)));
        }

        private static Tilemap GetOrCreateTilemap(Transform parent, string name, int order)
        {
            Transform t = parent.Find(name);
            GameObject go;
            if (t != null) go = t.gameObject;
            else
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
            }
            var tm = go.GetComponent<Tilemap>();
            if (tm == null) tm = go.AddComponent<Tilemap>();
            var tr = go.GetComponent<TilemapRenderer>();
            if (tr == null) tr = go.AddComponent<TilemapRenderer>();
            tr.sortingOrder = order;
            tr.enabled = true;
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
    }
}
#endif
