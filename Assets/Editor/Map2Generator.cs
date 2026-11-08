using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ Unity EditorWindow tự động tạo và dựng toàn bộ khu vực "Map_Zone_2"
    /// (Rừng rậm, Ngôi làng nhỏ, Đường mòn uốn lượn và Sào huyệt Quái vật / Boss).
    /// Tuân thủ tuyệt đối quy tắc cô lập trong "Map_Zone_2", không bao giờ can thiệp sang Map 1.
    /// </summary>
    public class Map2Generator : EditorWindow
    {
        [Header("1. Kích Thước & Tọa Độ Cục Bộ (Trong Map_Zone_2)")]
        public int width = 60;
        public int height = 60;
        public Vector2Int startPos = Vector2Int.zero;

        [Header("2. Tham Chiếu Object Gốc & Tilemap Của Riêng Map 2")]
        public Transform mapZone2Parent;
        public Tilemap map2Ground;
        public Tilemap map2Walls;

        [Header("3. Tham Chiếu TileBase")]
        public TileBase villageGrassTile;
        public TileBase deepForestTile;
        public TileBase dirtTile;
        public TileBase pathTile;
        public TileBase wallTile;

        [Header("4. Tham Chiếu Prefabs / GameObjects Mẫu")]
        public List<GameObject> treePrefabs = new List<GameObject>();
        public List<GameObject> housePrefabs = new List<GameObject>();
        public GameObject fencePrefab;
        public GameObject bossMonumentPrefab;
        public List<GameObject> monsterPrefabs = new List<GameObject>();
        public GameObject bossPrefab;

        [Header("5. Tham Số Mật Độ")]
        [Range(0.05f, 0.85f)]
        public float treeDensity = 0.35f;
        [Range(1, 30)]
        public int monsterPackCount = 8;

        private SerializedObject serializedWindow;
        private SerializedProperty propWidth;
        private SerializedProperty propHeight;
        private SerializedProperty propStartPos;
        private SerializedProperty propMapZone2Parent;
        private SerializedProperty propMap2Ground;
        private SerializedProperty propMap2Walls;
        private SerializedProperty propVillageGrassTile;
        private SerializedProperty propDeepForestTile;
        private SerializedProperty propDirtTile;
        private SerializedProperty propPathTile;
        private SerializedProperty propWallTile;
        private SerializedProperty propTreePrefabs;
        private SerializedProperty propHousePrefabs;
        private SerializedProperty propFencePrefab;
        private SerializedProperty propBossMonumentPrefab;
        private SerializedProperty propMonsterPrefabs;
        private SerializedProperty propBossPrefab;
        private SerializedProperty propTreeDensity;
        private SerializedProperty propMonsterPackCount;

        private Vector2 scrollPosition;

        private const string Map2RootName = "Map_Zone_2";
        private const string Map2GroundName = "Map2_Ground";
        private const string Map2WallsName = "Map2_Walls";
        private const string ContainerVillageName = "Env_Village";
        private const string ContainerForestName = "Env_Forest_Trees";
        private const string ContainerMonstersName = "Spawners_Monsters";
        private const string AltContainerVillageName = "Map2_Env_Village";
        private const string AltContainerForestName = "Map2_Env_Forest";
        private const string AltContainerMonstersName = "Map2_Spawners_Monsters";
        private const string TileAssetFolder = "Assets/Tilemaps/ForestDemo";
        private const string PrefabFolder = "Assets/Prefabs/ForestPrototype";
        private const string VWDir = "Assets/Sprites/VillageAndWorld";
        private const string ForestSpriteDir = "Assets/Sprites/ForestPack";

        [MenuItem("Tools/Map Generator/Generate Map 2")]
        public static void ShowWindow()
        {
            Map2Generator window = GetWindow<Map2Generator>("Map 2 Generator");
            window.minSize = new Vector2(460f, 650f);
            window.AutoBindMap2ReferencesOnly();
            window.Show();
        }

        private void OnEnable()
        {
            InitSerializedProperties();
            AutoBindMap2ReferencesOnly();
        }

        private void InitSerializedProperties()
        {
            serializedWindow = new SerializedObject(this);
            propWidth = serializedWindow.FindProperty("width");
            propHeight = serializedWindow.FindProperty("height");
            propStartPos = serializedWindow.FindProperty("startPos");
            propMapZone2Parent = serializedWindow.FindProperty("mapZone2Parent");
            propMap2Ground = serializedWindow.FindProperty("map2Ground");
            propMap2Walls = serializedWindow.FindProperty("map2Walls");
            propVillageGrassTile = serializedWindow.FindProperty("villageGrassTile");
            propDeepForestTile = serializedWindow.FindProperty("deepForestTile");
            propDirtTile = serializedWindow.FindProperty("dirtTile");
            propPathTile = serializedWindow.FindProperty("pathTile");
            propWallTile = serializedWindow.FindProperty("wallTile");
            propTreePrefabs = serializedWindow.FindProperty("treePrefabs");
            propHousePrefabs = serializedWindow.FindProperty("housePrefabs");
            propFencePrefab = serializedWindow.FindProperty("fencePrefab");
            propBossMonumentPrefab = serializedWindow.FindProperty("bossMonumentPrefab");
            propMonsterPrefabs = serializedWindow.FindProperty("monsterPrefabs");
            propBossPrefab = serializedWindow.FindProperty("bossPrefab");
            propTreeDensity = serializedWindow.FindProperty("treeDensity");
            propMonsterPackCount = serializedWindow.FindProperty("monsterPackCount");
        }

        /// <summary>
        /// Chỉ tìm kiếm "Map_Zone_2" và các con bên trong "Map_Zone_2",
        /// tuyệt đối không tìm kiếm hay động chạm tới bất kỳ đối tượng nào của Map 1.
        /// </summary>
        private void AutoBindMap2ReferencesOnly()
        {
            if (mapZone2Parent == null)
            {
                GameObject foundZone2 = GameObject.Find(Map2RootName);
                if (foundZone2 != null && foundZone2.name == Map2RootName)
                {
                    mapZone2Parent = foundZone2.transform;
                }
            }

            if (mapZone2Parent != null)
            {
                // Chỉ dùng mapZone2Parent.Find để lấy đúng Tilemap con của Map_Zone_2
                if (map2Ground == null || !IsStrictChildOfMap2(map2Ground.transform))
                {
                    Transform groundTrans = mapZone2Parent.Find(Map2GroundName);
                    map2Ground = groundTrans != null ? groundTrans.GetComponent<Tilemap>() : null;
                }

                if (map2Walls == null || !IsStrictChildOfMap2(map2Walls.transform))
                {
                    Transform wallsTrans = mapZone2Parent.Find(Map2WallsName);
                    map2Walls = wallsTrans != null ? wallsTrans.GetComponent<Tilemap>() : null;
                }
            }

            // Tự động nạp các Tile Asset từ Project (AssetDatabase)
            if (villageGrassTile == null)
            {
                villageGrassTile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TileAssetFolder}/Tile_Grass.asset");
            }
            if (deepForestTile == null)
            {
                deepForestTile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TileAssetFolder}/Tile_DeepForest.asset");
                if (deepForestTile == null)
                {
                    deepForestTile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TileAssetFolder}/Tile_DeepGrass.asset");
                }
            }
            if (dirtTile == null)
            {
                dirtTile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TileAssetFolder}/Tile_Dirt.asset");
            }
            if (pathTile == null)
            {
                pathTile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TileAssetFolder}/Tile_Path.asset");
            }
            if (wallTile == null)
            {
                wallTile = AssetDatabase.LoadAssetAtPath<TileBase>($"{TileAssetFolder}/Tile_Wall.asset");
            }

            // Đảm bảo các Prefab mẫu cho Cây, Hàng rào, Đài tưởng niệm tồn tại trong Project
            EnsureDefaultPropPrefabsExistInProject();

            // Tự động nạp Tree Prefabs
            if (treePrefabs == null)
            {
                treePrefabs = new List<GameObject>();
            }
            treePrefabs.RemoveAll(item => item == null);
            if (treePrefabs.Count == 0)
            {
                TryAddProjectPrefab(treePrefabs, $"{PrefabFolder}/Border_ForestTree_20.prefab");
                TryAddProjectPrefab(treePrefabs, $"{PrefabFolder}/Border_ForestTree_23.prefab");
                TryAddProjectPrefab(treePrefabs, $"{PrefabFolder}/Tree_EmeraldOak.prefab");
            }

            // Tự động nạp House Prefabs
            if (housePrefabs == null)
            {
                housePrefabs = new List<GameObject>();
            }
            housePrefabs.RemoveAll(item => item == null);
            if (housePrefabs.Count == 0)
            {
                TryAddProjectPrefab(housePrefabs, $"{PrefabFolder}/Building_Village_Residence.prefab");
                TryAddProjectPrefab(housePrefabs, $"{PrefabFolder}/Building_Herbalist_Cottage.prefab");
                TryAddProjectPrefab(housePrefabs, $"{PrefabFolder}/Building_Village_Tavern.prefab");
            }

            // Tự động nạp Fence Prefab
            if (fencePrefab == null)
            {
                fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/PetSanctuary_Fence1.prefab");
            }

            // Tự động nạp Boss Monument Prefab
            if (bossMonumentPrefab == null)
            {
                bossMonumentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/Peak_Monument_2.prefab");
                if (bossMonumentPrefab == null)
                {
                    bossMonumentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/Waystone_Monument.prefab");
                }
            }

            // Tự động nạp Monster Prefabs
            if (monsterPrefabs == null)
            {
                monsterPrefabs = new List<GameObject>();
            }
            monsterPrefabs.RemoveAll(item => item == null);
            if (monsterPrefabs.Count == 0)
            {
                TryAddProjectPrefab(monsterPrefabs, $"{PrefabFolder}/Enemy_Forest_WhiteWolf.prefab");
                TryAddProjectPrefab(monsterPrefabs, $"{PrefabFolder}/Enemy_Mole.prefab");
                TryAddProjectPrefab(monsterPrefabs, $"{PrefabFolder}/Enemy_Steppe_Boar.prefab");
                TryAddProjectPrefab(monsterPrefabs, $"{PrefabFolder}/Enemy_Treant.prefab");
                TryAddProjectPrefab(monsterPrefabs, $"{PrefabFolder}/Enemy_Water_Slime.prefab");
            }

            // Tự động nạp Boss Prefab
            if (bossPrefab == null)
            {
                bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/Boss_Treant_Ancient.prefab");
                if (bossPrefab == null)
                {
                    bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/Enemy_Treant_Boss.prefab");
                }
            }

            if (serializedWindow != null)
            {
                serializedWindow.Update();
            }
        }

        /// <summary>
        /// Tạo sẵn các Prefab Asset trong thư mục Assets/Prefabs/ForestPrototype (nếu chưa có)
        /// từ các Sprite có sẵn trong Project để danh sách Tree, Fence, Monument luôn đầy đủ 100%,
        /// hoàn toàn không cần tìm kiếm hay chạm vào bất kỳ object nào của Map 1.
        /// </summary>
        private static void EnsureDefaultPropPrefabsExistInProject()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "ForestPrototype");
            }

            CreateSpritePropPrefabIfMissing(
                $"{PrefabFolder}/Border_ForestTree_20.prefab",
                "Border_ForestTree_20",
                $"{VWDir}/tree_deep_teal.png",
                new Vector2(1.0f, 0.5f),
                new Vector2(0f, -1.2f),
                5);

            CreateSpritePropPrefabIfMissing(
                $"{PrefabFolder}/Border_ForestTree_23.prefab",
                "Border_ForestTree_23",
                $"{VWDir}/tree_forest_pine.png",
                new Vector2(1.0f, 0.5f),
                new Vector2(0f, -1.2f),
                5);

            CreateSpritePropPrefabIfMissing(
                $"{PrefabFolder}/Tree_EmeraldOak.prefab",
                "Tree_EmeraldOak",
                $"{VWDir}/tree_emerald_oak.png",
                new Vector2(1.0f, 0.5f),
                new Vector2(0f, -1.2f),
                5);

            string fenceSpritePath = File.Exists($"{VWDir}/fence_wood_horizontal.png")
                ? $"{VWDir}/fence_wood_horizontal.png"
                : $"{VWDir}/fence_wood_gate_row.png";

            CreateSpritePropPrefabIfMissing(
                $"{PrefabFolder}/PetSanctuary_Fence1.prefab",
                "PetSanctuary_Fence1",
                fenceSpritePath,
                new Vector2(1.1f, 0.35f),
                new Vector2(0f, -0.2f),
                4);

            CreateSpritePropPrefabIfMissing(
                $"{PrefabFolder}/Peak_Monument_2.prefab",
                "Peak_Monument_2",
                $"{ForestSpriteDir}/rock-monument.png",
                new Vector2(2.0f, 0.8f),
                new Vector2(0f, -1.2f),
                5);
        }

        private static void CreateSpritePropPrefabIfMissing(
            string prefabAssetPath,
            string prefabName,
            string spriteAssetPath,
            Vector2 colliderSize,
            Vector2 colliderOffset,
            int sortingOrder)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath) != null)
            {
                return;
            }

            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetPath);
            if (sp == null)
            {
                return;
            }

            GameObject tempObj = new GameObject(prefabName);
            SpriteRenderer sr = tempObj.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = sortingOrder;

            BoxCollider2D col = tempObj.AddComponent<BoxCollider2D>();
            col.size = colliderSize;
            col.offset = colliderOffset;

            PrefabUtility.SaveAsPrefabAsset(tempObj, prefabAssetPath);
            DestroyImmediate(tempObj);
        }

        private static void TryAddProjectPrefab(List<GameObject> list, string assetPath)
        {
            if (list == null) return;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null && !list.Contains(prefab))
            {
                list.Add(prefab);
            }
        }

        /// <summary>
        /// Kiểm tra một Transform có thực sự nằm bên trong "Map_Zone_2" hay không.
        /// Ngăn chặn tuyệt đối việc kéo nhầm Tilemap của Map 1 vào ô tham chiếu.
        /// </summary>
        private bool IsStrictChildOfMap2(Transform target)
        {
            if (target == null || mapZone2Parent == null) return false;
            return target.parent == mapZone2Parent || target.IsChildOf(mapZone2Parent);
        }

        private void OnGUI()
        {
            if (serializedWindow == null)
            {
                InitSerializedProperties();
            }

            serializedWindow.Update();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(8f);
            EditorGUILayout.LabelField("MAP 2 GENERATOR - KHU VỰC MAP_ZONE_2", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "QUY TẮC AN TOÀN:\n" +
                "• Mọi thao tác vẽ Tile và sinh GameObject chỉ diễn ra bên trong 'Map_Zone_2'.\n" +
                "• Tuyệt đối không tìm kiếm hay can thiệp vào bất kỳ đối tượng nào của Map 1.",
                MessageType.Info);

            // 1. Kích thước & Tọa độ cục bộ
            GUILayout.Space(6f);
            EditorGUILayout.LabelField("1. Kích Thước & Tọa Độ Cục Bộ", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propWidth, new GUIContent("Width (Chiều rộng)"));
            EditorGUILayout.PropertyField(propHeight, new GUIContent("Height (Chiều cao)"));
            EditorGUILayout.PropertyField(propStartPos, new GUIContent("Start Pos (Tọa độ gốc)"));

            if (width < 25) width = 25;
            if (height < 35) height = 35;

            // 2. Tham chiếu Object gốc & Tilemap của riêng Map 2
            GUILayout.Space(8f);
            EditorGUILayout.LabelField("2. Tham Chiếu Gốc & Tilemap (Chỉ Thuộc Map_Zone_2)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propMapZone2Parent, new GUIContent("Map_Zone_2 Parent"));

            if (mapZone2Parent == null)
            {
                EditorGUILayout.HelpBox(
                    "Chưa tìm thấy GameObject 'Map_Zone_2' trong Scene! Nhấn nút bên dưới hoặc bấm 'Generate Map 2' để tự động tạo mới 'Map_Zone_2' tách biệt ở tọa độ (-100, 80, 0).",
                    MessageType.Warning);

                if (GUILayout.Button("Tìm Hoặc Tạo Mới 'Map_Zone_2' Tại (-100, 80, 0)", GUILayout.Height(26f)))
                {
                    EnsureMapZone2RootAndTilemaps();
                }
            }

            EditorGUILayout.PropertyField(propMap2Ground, new GUIContent("Map2_Ground Tilemap"));
            EditorGUILayout.PropertyField(propMap2Walls, new GUIContent("Map2_Walls Tilemap"));

            // Kiểm tra bảo vệ: nếu người dùng kéo nhầm Tilemap ngoài Map_Zone_2 thì từ chối ngay
            if (map2Ground != null && (mapZone2Parent == null || !IsStrictChildOfMap2(map2Ground.transform) || map2Ground.name != Map2GroundName))
            {
                EditorGUILayout.HelpBox("CẢNH BÁO: map2Ground phải là '" + Map2GroundName + "' nằm bên trong 'Map_Zone_2'! Đã gỡ tham chiếu không hợp lệ.", MessageType.Error);
                map2Ground = null;
            }
            if (map2Walls != null && (mapZone2Parent == null || !IsStrictChildOfMap2(map2Walls.transform) || map2Walls.name != Map2WallsName))
            {
                EditorGUILayout.HelpBox("CẢNH BÁO: map2Walls phải là '" + Map2WallsName + "' nằm bên trong 'Map_Zone_2'! Đã gỡ tham chiếu không hợp lệ.", MessageType.Error);
                map2Walls = null;
            }

            // 3. Tham chiếu TileBase
            GUILayout.Space(8f);
            EditorGUILayout.LabelField("3. Tham Chiếu Tile Assets", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propVillageGrassTile, new GUIContent("Village Grass Tile (Tile_Grass)"));
            EditorGUILayout.PropertyField(propDeepForestTile, new GUIContent("Deep Forest Tile (Tile_DeepForest)"));
            EditorGUILayout.PropertyField(propDirtTile, new GUIContent("Dirt Tile (Tile_Dirt)"));
            EditorGUILayout.PropertyField(propPathTile, new GUIContent("Path Tile (Tile_Path)"));
            EditorGUILayout.PropertyField(propWallTile, new GUIContent("Wall Tile (Tile_Wall)"));

            // 4. Tham chiếu Prefabs / GameObjects mẫu
            GUILayout.Space(8f);
            EditorGUILayout.LabelField("4. Tham Chiếu Prefabs / GameObjects Mẫu", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propTreePrefabs, new GUIContent("Tree Prefabs (Cây rừng)"), true);
            EditorGUILayout.PropertyField(propHousePrefabs, new GUIContent("House Prefabs (Nhà làng)"), true);
            EditorGUILayout.PropertyField(propFencePrefab, new GUIContent("Fence Prefab (Hàng rào)"));
            EditorGUILayout.PropertyField(propBossMonumentPrefab, new GUIContent("Boss Monument Prefab (Đài tưởng niệm)"));
            EditorGUILayout.PropertyField(propMonsterPrefabs, new GUIContent("Monster Prefabs (Quái thường)"), true);
            EditorGUILayout.PropertyField(propBossPrefab, new GUIContent("Boss Prefab (Quái Boss/Elite)"));

            // 5. Tham số mật độ
            GUILayout.Space(8f);
            EditorGUILayout.LabelField("5. Tham Số Mật Độ Rừng & Quái", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propTreeDensity, new GUIContent("Tree Density (Mật độ cây)"));
            EditorGUILayout.PropertyField(propMonsterPackCount, new GUIContent("Monster Pack Count (Số cụm quái)"));

            serializedWindow.ApplyModifiedProperties();

            // 6. Các nút điều khiển
            GUILayout.Space(16f);
            GUI.backgroundColor = new Color(0.22f, 0.78f, 0.38f);
            if (GUILayout.Button("DỰNG MAP 2 HOÀN CHỈNH (GENERATE MAP 2)", GUILayout.Height(40f)))
            {
                GenerateMap2();
            }

            GUILayout.Space(6f);
            GUI.backgroundColor = new Color(0.9f, 0.32f, 0.28f);
            if (GUILayout.Button("XÓA SẠCH MAP 2 (CLEAR MAP 2)", GUILayout.Height(30f)))
            {
                if (EditorUtility.DisplayDialog(
                    "Xác Nhận Clear Map 2",
                    "Bạn có chắc chắn muốn xóa toàn bộ Tile trên Map2_Ground, Map2_Walls và các object con trong Map2_Env_Village, Map2_Env_Forest, Map2_Spawners_Monsters?\n(Map 1 được bảo vệ an toàn 100%).",
                    "Xóa Map 2",
                    "Hủy"))
                {
                    ClearMap2();
                }
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(12f);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Đảm bảo GameObject gốc "Map_Zone_2" và 2 Tilemap "Map2_Ground", "Map2_Walls" tồn tại hợp lệ bên trong "Map_Zone_2".
        /// </summary>
        private bool EnsureMapZone2RootAndTilemaps()
        {
            if (mapZone2Parent == null)
            {
                GameObject existingZone2 = GameObject.Find(Map2RootName);
                if (existingZone2 != null && existingZone2.name == Map2RootName)
                {
                    mapZone2Parent = existingZone2.transform;
                }
                else
                {
                    GameObject newZone2 = new GameObject(Map2RootName);
                    newZone2.transform.position = new Vector3(-100f, 80f, 0f);
                    newZone2.AddComponent<Grid>();
                    Undo.RegisterCreatedObjectUndo(newZone2, "Create Map_Zone_2");
                    mapZone2Parent = newZone2.transform;
                }
            }

            if (mapZone2Parent.GetComponent<Grid>() == null)
            {
                Undo.AddComponent<Grid>(mapZone2Parent.gameObject);
            }

            // Chỉ tìm hoặc tạo "Map2_Ground" bên trong mapZone2Parent
            if (map2Ground == null || !IsStrictChildOfMap2(map2Ground.transform) || map2Ground.name != Map2GroundName)
            {
                Transform groundTrans = mapZone2Parent.Find(Map2GroundName);
                if (groundTrans == null)
                {
                    GameObject groundGo = new GameObject(Map2GroundName);
                    groundGo.transform.SetParent(mapZone2Parent, false);
                    groundGo.transform.localPosition = Vector3.zero;
                    map2Ground = groundGo.AddComponent<Tilemap>();
                    TilemapRenderer groundRenderer = groundGo.AddComponent<TilemapRenderer>();
                    groundRenderer.sortingOrder = 0;
                    Undo.RegisterCreatedObjectUndo(groundGo, "Create Map2_Ground");
                }
                else
                {
                    map2Ground = groundTrans.GetComponent<Tilemap>();
                    if (map2Ground == null)
                    {
                        map2Ground = Undo.AddComponent<Tilemap>(groundTrans.gameObject);
                    }
                    TilemapRenderer groundRenderer = groundTrans.GetComponent<TilemapRenderer>();
                    if (groundRenderer == null)
                    {
                        groundRenderer = Undo.AddComponent<TilemapRenderer>(groundTrans.gameObject);
                    }
                    groundRenderer.sortingOrder = 0;
                }
            }

            // Chỉ tìm hoặc tạo "Map2_Walls" bên trong mapZone2Parent
            if (map2Walls == null || !IsStrictChildOfMap2(map2Walls.transform) || map2Walls.name != Map2WallsName)
            {
                Transform wallsTrans = mapZone2Parent.Find(Map2WallsName);
                if (wallsTrans == null)
                {
                    GameObject wallsGo = new GameObject(Map2WallsName);
                    wallsGo.transform.SetParent(mapZone2Parent, false);
                    wallsGo.transform.localPosition = Vector3.zero;
                    map2Walls = wallsGo.AddComponent<Tilemap>();
                    TilemapRenderer wallsRenderer = wallsGo.AddComponent<TilemapRenderer>();
                    wallsRenderer.sortingOrder = 2;
                    ConfigureMap2WallColliders(wallsGo);
                    Undo.RegisterCreatedObjectUndo(wallsGo, "Create Map2_Walls");
                }
                else
                {
                    map2Walls = wallsTrans.GetComponent<Tilemap>();
                    if (map2Walls == null)
                    {
                        map2Walls = Undo.AddComponent<Tilemap>(wallsTrans.gameObject);
                    }
                    TilemapRenderer wallsRenderer = wallsTrans.GetComponent<TilemapRenderer>();
                    if (wallsRenderer == null)
                    {
                        wallsRenderer = Undo.AddComponent<TilemapRenderer>(wallsTrans.gameObject);
                    }
                    wallsRenderer.sortingOrder = 2;
                    ConfigureMap2WallColliders(wallsTrans.gameObject);
                }
            }
            else
            {
                TilemapRenderer wallsRenderer = map2Walls.GetComponent<TilemapRenderer>();
                if (wallsRenderer != null)
                {
                    wallsRenderer.sortingOrder = 2;
                }
                ConfigureMap2WallColliders(map2Walls.gameObject);
            }

            if (serializedWindow != null)
            {
                serializedWindow.Update();
            }

            return mapZone2Parent != null && map2Ground != null && map2Walls != null;
        }

        private static void ConfigureMap2WallColliders(GameObject wallsGo)
        {
            Rigidbody2D rb = wallsGo.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = wallsGo.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Static;

            CompositeCollider2D composite = wallsGo.GetComponent<CompositeCollider2D>();
            if (composite == null)
            {
                composite = wallsGo.AddComponent<CompositeCollider2D>();
            }
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

            TilemapCollider2D tilemapCollider = wallsGo.GetComponent<TilemapCollider2D>();
            if (tilemapCollider == null)
            {
                tilemapCollider = wallsGo.AddComponent<TilemapCollider2D>();
            }
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
#pragma warning disable 0618
            tilemapCollider.usedByComposite = true;
#pragma warning restore 0618
        }

        /// <summary>
        /// Tìm hoặc tạo mới Container con trực tiếp bên trong "Map_Zone_2" tại localPosition = Vector3.zero.
        /// Hỗ trợ cả tên mới ("Env_Village") và tên cũ ("Map2_Env_Village").
        /// </summary>
        private Transform GetOrCreateMap2Container(string primaryName, string altName = null)
        {
            Transform container = mapZone2Parent.Find(primaryName);
            if (container == null && !string.IsNullOrEmpty(altName))
            {
                container = mapZone2Parent.Find(altName);
            }

            if (container == null)
            {
                GameObject containerObj = new GameObject(primaryName);
                containerObj.transform.SetParent(mapZone2Parent, false);
                containerObj.transform.localPosition = Vector3.zero;
                Undo.RegisterCreatedObjectUndo(containerObj, "Create " + primaryName);
                container = containerObj.transform;
            }
            else
            {
                container.localPosition = Vector3.zero;
            }
            return container;
        }

        /// <summary>
        /// Thuật toán chính dựng toàn bộ Map 2 bên trong "Map_Zone_2".
        /// </summary>
        public void GenerateMap2()
        {
            AutoBindMap2ReferencesOnly();

            if (!EnsureMapZone2RootAndTilemaps())
            {
                return;
            }

            // Kiểm tra bảo vệ lần cuối trước khi thực hiện
            if (!IsStrictChildOfMap2(map2Ground.transform) || !IsStrictChildOfMap2(map2Walls.transform))
            {
                EditorUtility.DisplayDialog(
                    "Lỗi Bảo Vệ Map 1",
                    "Tilemap được gán không nằm trong 'Map_Zone_2'! Đã chặn thao tác để bảo vệ Map 1.",
                    "OK");
                return;
            }

            TileBase effectiveGrassTile = villageGrassTile != null ? villageGrassTile : deepForestTile;
            TileBase effectiveForestTile = deepForestTile != null ? deepForestTile : villageGrassTile;
            TileBase effectiveDirtTile = dirtTile != null ? dirtTile : effectiveGrassTile;
            TileBase effectivePathTile = pathTile != null ? pathTile : effectiveDirtTile;

            if (effectiveGrassTile == null || wallTile == null)
            {
                EditorUtility.DisplayDialog(
                    "Thiếu TileBase",
                    "Vui lòng gán ít nhất Village Grass Tile (Tile_Grass) và Wall Tile (Tile_Wall) trước khi dựng Map 2!",
                    "OK");
                return;
            }

            // Dọn sạch dữ liệu cũ của riêng Map 2 trước khi vẽ mới
            ClearMap2Internal();

            // =========================================================================
            // BƯỚC 1: TẠO CONTAINER PHÂN CẤP BÊN TRONG MAP_ZONE_2
            // =========================================================================
            Transform envVillage = GetOrCreateMap2Container(ContainerVillageName, AltContainerVillageName);
            Transform envForest = GetOrCreateMap2Container(ContainerForestName, AltContainerForestName);
            Transform spawnersMonsters = GetOrCreateMap2Container(ContainerMonstersName, AltContainerMonstersName);

            // =========================================================================
            // BƯỚC 2: VẼ TILEMAP LÊN MAP2_GROUND VÀ MAP2_WALLS
            // =========================================================================
            Undo.RecordObject(map2Ground, "Paint Map2_Ground");
            Undo.RecordObject(map2Walls, "Paint Map2_Walls");

            Vector2 villageCenterLocal = new Vector2(width / 2f, 10f);
            float villageRadius = 9f;

            Vector2 bossCenterLocal = new Vector2(width / 2f, height - 12f);
            float bossRadius = 8f;

            int gateCenterX = width / 2;

            // Lưu tập hợp các ô cục bộ (x, y) là đường mòn hoặc tường để tránh đặt cây/nhà/quái đè lên
            HashSet<Vector2Int> roadAndDirtCells = new HashSet<Vector2Int>();
            HashSet<Vector2Int> mainPathCells = new HashSet<Vector2Int>();
            HashSet<Vector2Int> wallCells = new HashSet<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3Int cellPos = new Vector3Int(startPos.x + x, startPos.y + y, 0);
                    Vector2 currentLocal = new Vector2(x, y);

                    // 2.1. Kiểm tra vùng Làng, Sào Huyệt Boss và Rừng để lát nền Map2_Ground
                    float distToVillage = Vector2.Distance(currentLocal, villageCenterLocal);
                    float distToBoss = Vector2.Distance(currentLocal, bossCenterLocal);

                    if (distToVillage <= villageRadius)
                    {
                        map2Ground.SetTile(cellPos, effectiveGrassTile);
                    }
                    else if (distToBoss <= bossRadius)
                    {
                        map2Ground.SetTile(cellPos, effectiveDirtTile);
                    }
                    else
                    {
                        map2Ground.SetTile(cellPos, effectiveForestTile);
                    }

                    // 2.2. Dựng Tường Bao ở 4 cạnh, chừa trống 3 ô ở chính giữa cạnh dưới (y == 0, x từ width/2 - 1 đến width/2 + 1)
                    bool isBorder = (x == 0 || x == width - 1 || y == 0 || y == height - 1);
                    if (isBorder)
                    {
                        bool isBottomEntrance = (y == 0 && x >= gateCenterX - 1 && x <= gateCenterX + 1);
                        if (!isBottomEntrance)
                        {
                            map2Walls.SetTile(cellPos, wallTile);
                            wallCells.Add(new Vector2Int(x, y));
                        }
                        else
                        {
                            map2Walls.SetTile(cellPos, null);
                        }
                    }
                }
            }

            // 2.3. Vẽ Đường Mòn (pathTile) độ rộng 3 ô, viền dirtTile hai bên:
            // Nối từ cổng dưới (width / 2, 0) đi qua làng tới (width / 2, 19),
            // sau đó uốn lượn theo hàm Mathf.Sin từ (width / 2, 19) lên Sào Huyệt (width / 2, height - 12).
            int bossTargetY = Mathf.Max(20, height - 12);
            for (int y = 0; y <= bossTargetY; y++)
            {
                int centerRoadX;
                if (y <= 19)
                {
                    centerRoadX = gateCenterX;
                }
                else
                {
                    float t = (float)(y - 19) / Mathf.Max(1, bossTargetY - 19);
                    float waveOffset = Mathf.Sin(t * Mathf.PI * 2f) * 6f;
                    centerRoadX = Mathf.RoundToInt(gateCenterX + waveOffset);
                }

                // Viền dirtTile hai bên (dx = -2 và dx = +2)
                for (int dx = -2; dx <= 2; dx++)
                {
                    int rx = Mathf.Clamp(centerRoadX + dx, 1, width - 2);
                    Vector3Int roadCellPos = new Vector3Int(startPos.x + rx, startPos.y + y, 0);
                    Vector2Int localCoord = new Vector2Int(rx, y);

                    if (dx >= -1 && dx <= 1)
                    {
                        map2Ground.SetTile(roadCellPos, effectivePathTile);
                        mainPathCells.Add(localCoord);
                        roadAndDirtCells.Add(localCoord);
                    }
                    else
                    {
                        if (!mainPathCells.Contains(localCoord))
                        {
                            map2Ground.SetTile(roadCellPos, effectiveDirtTile);
                            roadAndDirtCells.Add(localCoord);
                        }
                    }
                }
            }

            // Đảm bảo 3 ô cổng tại y = 0 luôn được lát pathTile
            for (int gx = gateCenterX - 1; gx <= gateCenterX + 1; gx++)
            {
                Vector3Int gateCell = new Vector3Int(startPos.x + gx, startPos.y, 0);
                map2Ground.SetTile(gateCell, effectivePathTile);
                mainPathCells.Add(new Vector2Int(gx, 0));
                roadAndDirtCells.Add(new Vector2Int(gx, 0));
            }

            // =========================================================================
            // BƯỚC 3: ĐẶT CÔNG TRÌNH LÀNG VÀO "Map2_Env_Village"
            // =========================================================================
            // 3.1. Đặt fencePrefab bao quanh chu vi Làng (bán kính 9), chừa lối đi phía Nam và Bắc
            if (fencePrefab != null)
            {
                List<Vector2> placedFencePositions = new List<Vector2>();
                int sampleSteps = 36;
                for (int i = 0; i < sampleSteps; i++)
                {
                    float angleRad = (i * 360f / sampleSteps) * Mathf.Deg2Rad;
                    int fx = Mathf.RoundToInt(villageCenterLocal.x + Mathf.Cos(angleRad) * villageRadius);
                    int fy = Mathf.RoundToInt(villageCenterLocal.y + Mathf.Sin(angleRad) * villageRadius);

                    if (fx <= 1 || fx >= width - 2 || fy <= 1 || fy >= height - 2)
                    {
                        continue;
                    }

                    // Chừa lối đi phía Nam và phía Bắc
                    if (Mathf.Abs(fx - gateCenterX) <= 2)
                    {
                        continue;
                    }

                    Vector2Int fenceCoord = new Vector2Int(fx, fy);
                    if (roadAndDirtCells.Contains(fenceCoord))
                    {
                        continue;
                    }

                    bool tooClose = false;
                    for (int p = 0; p < placedFencePositions.Count; p++)
                    {
                        if (Vector2.Distance(placedFencePositions[p], fenceCoord) < 1.8f)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose)
                    {
                        continue;
                    }

                    placedFencePositions.Add(fenceCoord);
                    Vector3Int fenceCellPos = new Vector3Int(startPos.x + fx, startPos.y + fy, 0);
                    Vector3 worldPos = map2Ground.GetCellCenterWorld(fenceCellPos);

                    GameObject fenceInstance = InstantiateElementInMap2(
                        fencePrefab,
                        worldPos,
                        envVillage,
                        $"Map2_VillageFence_{fx}_{fy}");

                    if (fenceInstance != null)
                    {
                        SetSortingOrderSafe(fenceInstance, 4);
                    }
                }
            }

            // 3.2. Đặt 2-3 ngôi nhà từ housePrefabs ở hai bên trái/phải trong làng (cách đường chính ít nhất 3 ô)
            List<GameObject> validHouses = GetValidPrefabs(housePrefabs);
            if (validHouses.Count > 0)
            {
                Vector2Int[] houseLocalOffsets = new Vector2Int[]
                {
                    new Vector2Int(gateCenterX - 5, 13), // Bên trái làng (cách đường chính 5 ô >= 3 ô)
                    new Vector2Int(gateCenterX + 5, 13), // Bên phải làng (cách đường chính 5 ô >= 3 ô)
                    new Vector2Int(gateCenterX - 5, 7)   // Bên trái phía dưới làng (cách đường chính 5 ô >= 3 ô)
                };

                for (int i = 0; i < houseLocalOffsets.Length; i++)
                {
                    Vector2Int hCoord = houseLocalOffsets[i];
                    if (hCoord.x <= 1 || hCoord.x >= width - 2 || hCoord.y <= 1 || hCoord.y >= height - 2)
                    {
                        continue;
                    }

                    GameObject houseAsset = validHouses[i % validHouses.Count];
                    Vector3Int houseCellPos = new Vector3Int(startPos.x + hCoord.x, startPos.y + hCoord.y, 0);
                    Vector3 houseWorldPos = map2Ground.GetCellCenterWorld(houseCellPos);

                    GameObject houseInstance = InstantiateElementInMap2(
                        houseAsset,
                        houseWorldPos,
                        envVillage,
                        $"Map2_VillageHouse_{i + 1}");

                    if (houseInstance != null)
                    {
                        SetSortingOrderSafe(houseInstance, 5);
                    }
                }
            }

            // =========================================================================
            // BƯỚC 4: TRỒNG RỪNG RẬM VÀO "Map2_Env_Forest"
            // =========================================================================
            List<GameObject> validTrees = GetValidPrefabs(treePrefabs);
            HashSet<Vector2Int> occupiedByTrees = new HashSet<Vector2Int>();

            if (validTrees.Count > 0)
            {
                for (int x = 2; x < width - 2; x += 2)
                {
                    for (int y = 2; y < height - 2; y += 2)
                    {
                        Vector2Int coord = new Vector2Int(x, y);
                        if (wallCells.Contains(coord) || roadAndDirtCells.Contains(coord))
                        {
                            continue;
                        }

                        Vector2 currentLocal = new Vector2(x, y);
                        float distToVillage = Vector2.Distance(currentLocal, villageCenterLocal);
                        float distToBoss = Vector2.Distance(currentLocal, bossCenterLocal);

                        // Ngoài bán kính Làng (R <= 10) và ngoài Sào Huyệt (R <= 9)
                        if (distToVillage <= 10f || distToBoss <= 9f)
                        {
                            continue;
                        }

                        // Không đè sát lên đường mòn
                        if (GetMinDistanceToCells(coord, roadAndDirtCells) < 1.8f)
                        {
                            continue;
                        }

                        float noiseValue = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) + Random.Range(-0.1f, 0.1f);
                        if (noiseValue > (1f - treeDensity))
                        {
                            GameObject randomTreePrefab = validTrees[Random.Range(0, validTrees.Count)];
                            Vector3Int treeCellPos = new Vector3Int(startPos.x + x, startPos.y + y, 0);
                            Vector3 treeWorldPos = map2Ground.GetCellCenterWorld(treeCellPos);

                            GameObject treeInstance = InstantiateElementInMap2(
                                randomTreePrefab,
                                treeWorldPos,
                                envForest,
                                $"Map2_Tree_{x}_{y}");

                            if (treeInstance != null)
                            {
                                float randScale = Random.Range(0.9f, 1.15f);
                                treeInstance.transform.localScale = new Vector3(randScale, randScale, 1f);

                                SpriteRenderer sr = treeInstance.GetComponent<SpriteRenderer>();
                                if (sr == null)
                                {
                                    sr = treeInstance.GetComponentInChildren<SpriteRenderer>();
                                }
                                if (sr != null)
                                {
                                    sr.flipX = Random.value > 0.5f;
                                    sr.sortingOrder = 5;
                                }

                                occupiedByTrees.Add(coord);
                            }
                        }
                    }
                }
            }

            // =========================================================================
            // BƯỚC 5: RẢI QUÁI & BOSS VÀO "Map2_Spawners_Monsters"
            // =========================================================================
            // 5.1. Tại tâm Sào Huyệt (width / 2, height - 12): Đặt bossMonumentPrefab và bossPrefab
            Vector3Int bossCenterCell = new Vector3Int(
                startPos.x + Mathf.RoundToInt(bossCenterLocal.x),
                startPos.y + Mathf.RoundToInt(bossCenterLocal.y),
                0);

            if (bossMonumentPrefab != null)
            {
                Vector3Int monumentCell = new Vector3Int(bossCenterCell.x, bossCenterCell.y + 3, 0);
                Vector3 monumentWorldPos = map2Ground.GetCellCenterWorld(monumentCell);
                GameObject monumentInstance = InstantiateElementInMap2(
                    bossMonumentPrefab,
                    monumentWorldPos,
                    spawnersMonsters,
                    "Map2_BossMonument");

                if (monumentInstance != null)
                {
                    SetSortingOrderSafe(monumentInstance, 5);
                }
            }

            if (bossPrefab != null)
            {
                Vector3 bossWorldPos = map2Ground.GetCellCenterWorld(bossCenterCell);
                GameObject bossInstance = InstantiateElementInMap2(
                    bossPrefab,
                    bossWorldPos,
                    spawnersMonsters,
                    "Map2_Boss_Elite");

                if (bossInstance != null)
                {
                    SetSortingOrderSafe(bossInstance, 6);
                }
            }

            // 5.2. Rải monsterPackCount cụm quái (2-4 con/cụm) từ monsterPrefabs tại các khoảng trống trong rừng
            List<GameObject> validMonsters = GetValidPrefabs(monsterPrefabs);
            if (validMonsters.Count > 0)
            {
                List<Vector2Int> candidatePackCenters = new List<Vector2Int>();
                for (int x = 3; x < width - 3; x++)
                {
                    for (int y = 20; y < height - 14; y++)
                    {
                        Vector2Int coord = new Vector2Int(x, y);
                        if (wallCells.Contains(coord) || roadAndDirtCells.Contains(coord) || occupiedByTrees.Contains(coord))
                        {
                            continue;
                        }

                        Vector2 currentLocal = new Vector2(x, y);
                        if (Vector2.Distance(currentLocal, villageCenterLocal) <= 11f ||
                            Vector2.Distance(currentLocal, bossCenterLocal) <= 9.5f)
                        {
                            continue;
                        }

                        float distToPath = GetMinDistanceToCells(coord, mainPathCells);
                        if (distToPath >= 3f && distToPath <= 7f)
                        {
                            candidatePackCenters.Add(coord);
                        }
                    }
                }

                for (int i = 0; i < candidatePackCenters.Count; i++)
                {
                    int swapIdx = Random.Range(i, candidatePackCenters.Count);
                    Vector2Int temp = candidatePackCenters[i];
                    candidatePackCenters[i] = candidatePackCenters[swapIdx];
                    candidatePackCenters[swapIdx] = temp;
                }

                List<Vector2Int> chosenPackCenters = new List<Vector2Int>();
                int packsCreated = 0;

                for (int i = 0; i < candidatePackCenters.Count && packsCreated < monsterPackCount; i++)
                {
                    Vector2Int candidate = candidatePackCenters[i];

                    bool tooCloseToOtherPack = false;
                    for (int j = 0; j < chosenPackCenters.Count; j++)
                    {
                        if (Vector2Int.Distance(candidate, chosenPackCenters[j]) < 6f)
                        {
                            tooCloseToOtherPack = true;
                            break;
                        }
                    }
                    if (tooCloseToOtherPack)
                    {
                        continue;
                    }

                    chosenPackCenters.Add(candidate);
                    packsCreated++;

                    int packSize = Random.Range(2, 5); // 2 đến 4 con mỗi cụm
                    for (int m = 0; m < packSize; m++)
                    {
                        Vector2Int offset = new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
                        int mx = Mathf.Clamp(candidate.x + offset.x, 2, width - 3);
                        int my = Mathf.Clamp(candidate.y + offset.y, 2, height - 3);
                        Vector2Int monsterCoord = new Vector2Int(mx, my);

                        if (roadAndDirtCells.Contains(monsterCoord) || wallCells.Contains(monsterCoord))
                        {
                            monsterCoord = candidate;
                        }

                        Vector3Int monsterCellPos = new Vector3Int(startPos.x + monsterCoord.x, startPos.y + monsterCoord.y, 0);
                        Vector3 monsterWorldPos = map2Ground.GetCellCenterWorld(monsterCellPos) + new Vector3(
                            Random.Range(-0.25f, 0.25f),
                            Random.Range(-0.25f, 0.25f),
                            0f);

                        GameObject monsterAsset = validMonsters[Random.Range(0, validMonsters.Count)];
                        GameObject monsterInstance = InstantiateElementInMap2(
                            monsterAsset,
                            monsterWorldPos,
                            spawnersMonsters,
                            $"Map2_Pack{packsCreated}_Monster_{m + 1}");

                        if (monsterInstance != null)
                        {
                            SetSortingOrderSafe(monsterInstance, 5);
                        }
                    }
                }
            }

            // 5.3. Rải 5 Linh Thú hoang dã vào Map_Zone_2
            Transform petContainer = GetOrCreateMap2Container("Map2_Spawners_Pets", "Map2_Pets");
            SpawnWildPetInMap2("Pet_CaDuoiHuKhong", new Vector2Int(12, 14), petContainer, "Map2_WildPet_CaDuoiHuKhong");
            SpawnWildPetInMap2("Pet_DaMinhTucDang", new Vector2Int(45, 18), petContainer, "Map2_WildPet_DaMinhTucDang");
            SpawnWildPetInMap2("Pet_HuDocNamNhan", new Vector2Int(18, 38), petContainer, "Map2_WildPet_HuDocNamNhan");
            SpawnWildPetInMap2("Pet_NgaiThaoVuCau", new Vector2Int(30, 26), petContainer, "Map2_WildPet_NgaiThaoVuCau");
            SpawnWildPetInMap2("Pet_ThucKimNeBa", new Vector2Int(42, 36), petContainer, "Map2_WildPet_ThucKimNeBa");

            map2Ground.RefreshAllTiles();
            map2Walls.RefreshAllTiles();

            EditorUtility.SetDirty(map2Ground);
            EditorUtility.SetDirty(map2Walls);
            EditorUtility.SetDirty(mapZone2Parent.gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"<color=green>✓ [Map2Generator] Đã sinh hoàn chỉnh khu vực '{Map2RootName}' ({width}x{height}) an toàn tuyệt đối!</color>");
        }

        /// <summary>
        /// Chức năng dọn dẹp an toàn ClearMap2():
        /// - CHỈ gọi map2Ground.ClearAllTiles() và map2Walls.ClearAllTiles().
        /// - CHỈ xóa các GameObject con bên trong "Map2_Env_Village", "Map2_Env_Forest" và "Map2_Spawners_Monsters" thuộc "Map_Zone_2".
        /// </summary>
        public void ClearMap2()
        {
            if (mapZone2Parent == null)
            {
                GameObject foundZone2 = GameObject.Find(Map2RootName);
                if (foundZone2 != null && foundZone2.name == Map2RootName)
                {
                    mapZone2Parent = foundZone2.transform;
                }
            }

            if (mapZone2Parent == null)
            {
                Debug.LogWarning("[Map2Generator] Không tìm thấy 'Map_Zone_2' để dọn dẹp.");
                return;
            }

            ClearMap2Internal();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("<color=yellow>[Map2Generator] Đã dọn sạch toàn bộ Tile và Object con trong 'Map_Zone_2' (Map 1 hoàn toàn nguyên vẹn).</color>");
        }

        private void ClearMap2Internal()
        {
            if (mapZone2Parent == null) return;

            if (map2Ground == null || !IsStrictChildOfMap2(map2Ground.transform))
            {
                Transform groundTrans = mapZone2Parent.Find(Map2GroundName);
                if (groundTrans != null) map2Ground = groundTrans.GetComponent<Tilemap>();
            }

            if (map2Walls == null || !IsStrictChildOfMap2(map2Walls.transform))
            {
                Transform wallsTrans = mapZone2Parent.Find(Map2WallsName);
                if (wallsTrans != null) map2Walls = wallsTrans.GetComponent<Tilemap>();
            }

            // 1. Chỉ xóa Tile trên map2Ground và map2Walls thuộc Map_Zone_2
            if (map2Ground != null && IsStrictChildOfMap2(map2Ground.transform))
            {
                Undo.RecordObject(map2Ground, "Clear Map2_Ground");
                map2Ground.ClearAllTiles();
                map2Ground.RefreshAllTiles();
                EditorUtility.SetDirty(map2Ground);
            }

            if (map2Walls != null && IsStrictChildOfMap2(map2Walls.transform))
            {
                Undo.RecordObject(map2Walls, "Clear Map2_Walls");
                map2Walls.ClearAllTiles();
                map2Walls.RefreshAllTiles();
                EditorUtility.SetDirty(map2Walls);
            }

            // 2. Chỉ xóa các GameObject con bên trong các Container của Map_Zone_2
            ClearContainerChildrenSafe(mapZone2Parent.Find(ContainerVillageName));
            ClearContainerChildrenSafe(mapZone2Parent.Find(AltContainerVillageName));
            ClearContainerChildrenSafe(mapZone2Parent.Find(ContainerForestName));
            ClearContainerChildrenSafe(mapZone2Parent.Find(AltContainerForestName));
            ClearContainerChildrenSafe(mapZone2Parent.Find(ContainerMonstersName));
            ClearContainerChildrenSafe(mapZone2Parent.Find(AltContainerMonstersName));
        }

        private void ClearContainerChildrenSafe(Transform container)
        {
            if (container == null || !IsStrictChildOfMap2(container))
            {
                return;
            }

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                GameObject childObj = container.GetChild(i).gameObject;
                Undo.DestroyObjectImmediate(childObj);
            }
        }

        private static GameObject InstantiateElementInMap2(
            GameObject sourcePrefabOrTemplate,
            Vector3 worldPos,
            Transform targetContainer,
            string instanceName)
        {
            if (sourcePrefabOrTemplate == null || targetContainer == null)
            {
                return null;
            }

            GameObject instance = null;
            if (PrefabUtility.IsPartOfPrefabAsset(sourcePrefabOrTemplate))
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefabOrTemplate, targetContainer);
                if (instance != null)
                {
                    instance.transform.position = worldPos;
                }
            }
            else
            {
                instance = Instantiate(sourcePrefabOrTemplate, worldPos, Quaternion.identity, targetContainer);
            }

            if (instance != null)
            {
                instance.name = instanceName;
                instance.SetActive(true);
                Undo.RegisterCreatedObjectUndo(instance, "Spawn " + instanceName);
            }

            return instance;
        }

        private GameObject SpawnWildPetInMap2(string petPrefabName, Vector2Int localCoord, Transform parent, string instanceName)
        {
            string p = "Assets/Prefabs/Pets/" + petPrefabName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (prefab == null) return null;

            Vector3Int cellPos = new Vector3Int(startPos.x + localCoord.x, startPos.y + localCoord.y, 0);
            Vector3 worldPos = map2Ground != null ? map2Ground.GetCellCenterWorld(cellPos) : new Vector3(cellPos.x, cellPos.y, 0);

            GameObject go = InstantiateElementInMap2(prefab, worldPos, parent, instanceName);
            if (go != null)
            {
                var ctrl = go.GetComponent<ActionRPG.Pets.PetController>();
                if (ctrl != null) ctrl.isWild = true;
                var ai = go.GetComponent<ActionRPG.Pets.PetAI>();
                if (ai != null) ai.isWild = true;

                var interactable = go.GetComponent<ActionRPG.Pets.WildPetInteractable>();
                if (interactable == null) interactable = go.AddComponent<ActionRPG.Pets.WildPetInteractable>();

                SetSortingOrderSafe(go, 10);
            }
            return go;
        }

        private static List<GameObject> GetValidPrefabs(List<GameObject> sourceList)
        {
            List<GameObject> valid = new List<GameObject>();
            if (sourceList == null) return valid;

            for (int i = 0; i < sourceList.Count; i++)
            {
                if (sourceList[i] != null)
                {
                    valid.Add(sourceList[i]);
                }
            }
            return valid;
        }

        private static float GetMinDistanceToCells(Vector2Int point, HashSet<Vector2Int> targetCells)
        {
            float minDist = float.MaxValue;
            foreach (Vector2Int cell in targetCells)
            {
                float dist = Vector2Int.Distance(point, cell);
                if (dist < minDist)
                {
                    minDist = dist;
                }
            }
            return minDist;
        }

        private static void SetSortingOrderSafe(GameObject targetObj, int sortingOrder)
        {
            if (targetObj == null) return;
            SpriteRenderer sr = targetObj.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = targetObj.GetComponentInChildren<SpriteRenderer>();
            }
            if (sr != null)
            {
                sr.sortingOrder = sortingOrder;
            }
        }
    }
}
