using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ActionRPG.Pets;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ đặt 5 Linh Thú (CaDuoiHuKhong, HuDocNamNhan, DaMinhTucDang, ThucKimNeBa, NgaiThaoVuCau)
    /// vào Map thứ 2 (Zone_02_Emerald_Steppe và Map_Zone_2) trong cả Forest_Demo_Map.unity và MainGame.unity.
    /// </summary>
    public static class PlacePetsInMap2
    {
        private const string DemoScenePath = "Assets/Scenes/Forest_Demo_Map.unity";
        private const string MainGameScenePath = "Assets/Scenes/MainGame.unity";
        private const string AutoPlaceSentinel = "Temp/PlacePetsInMap2_v1.done";

        [InitializeOnLoadMethod]
        private static void AutoPlaceOnLoadIfNeeded()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return;

                if (!File.Exists(AutoPlaceSentinel))
                {
                    try
                    {
                        PlacePetsInAllMap2Scenes();
                        Directory.CreateDirectory(Path.GetDirectoryName(AutoPlaceSentinel));
                        File.WriteAllText(AutoPlaceSentinel, DateTime.Now.ToString("O"));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[PlacePetsInMap2] Lỗi khi tự động đặt Pet vào Map 2: {ex}");
                    }
                }
            };
        }

        public struct PetPlacementInfo
        {
            public string prefabName;
            public string instanceName;
            public Vector3 steppePosition;   // Tọa độ trên Zone_02_Emerald_Steppe (Thảo Nguyên Xanh)
            public Vector3 mapZone2Position; // Tọa độ trên Map_Zone_2 (Map 2 phụ tách biệt)
            public string description;
        }

        public static readonly PetPlacementInfo[] PetsToPlace = new PetPlacementInfo[]
        {
            new PetPlacementInfo
            {
                prefabName = "Pet_CaDuoiHuKhong",
                instanceName = "WildPet_CaDuoiHuKhong",
                steppePosition = new Vector3(8.0f, -14.0f, 0f),
                mapZone2Position = new Vector3(-82.0f, 92.0f, 0f),
                description = "Cá Đuối Hư Không - Lượn sóng ven Hồ Thảo Nguyên SteppePond"
            },
            new PetPlacementInfo
            {
                prefabName = "Pet_DaMinhTucDang",
                instanceName = "WildPet_DaMinhTucDang",
                steppePosition = new Vector3(14.5f, 13.0f, 0f),
                mapZone2Position = new Vector3(-62.0f, 96.0f, 0f),
                description = "Dạ Minh Túc Đằng - Đậu cạnh Vườn Dược Thảo Ánh Trăng"
            },
            new PetPlacementInfo
            {
                prefabName = "Pet_HuDocNamNhan",
                instanceName = "WildPet_HuDocNamNhan",
                steppePosition = new Vector3(2.5f, -8.0f, 0f),
                mapZone2Position = new Vector3(-78.0f, 85.0f, 0f),
                description = "Hủ Độc Nấm Nhãn - Râm mát ven Vũng Nước Tây Bắc"
            },
            new PetPlacementInfo
            {
                prefabName = "Pet_NgaiThaoVuCau",
                instanceName = "WildPet_NgaiThaoVuCau",
                steppePosition = new Vector3(6.0f, 4.0f, 0f),
                mapZone2Position = new Vector3(-66.0f, 88.0f, 0f),
                description = "Ngải Thảo Vu Cẩu - Chạy nhảy giữa Thảo Nguyên Khoáng Đạt"
            },
            new PetPlacementInfo
            {
                prefabName = "Pet_ThucKimNeBa",
                instanceName = "WildPet_ThucKimNeBa",
                steppePosition = new Vector3(11.0f, -5.5f, 0f),
                mapZone2Position = new Vector3(-72.0f, 90.0f, 0f),
                description = "Thực Kim Nê Ba - Trườn bò cạnh Bãi Quặng Đồng Ven Bờ"
            }
        };

        [MenuItem("Tools/ActionRPG/Place All 5 Pets in Map 2", priority = 105)]
        public static void PlacePetsInAllMap2Scenes()
        {
            Debug.Log("<color=cyan>🚀 Bắt đầu đặt 5 Linh Thú vào Bản Đồ Thứ 2...</color>");

            // 1. Đặt vào Forest_Demo_Map.unity (Zone_02_Emerald_Steppe)
            if (File.Exists(DemoScenePath))
            {
                PlaceInScene(DemoScenePath, isMainGame: false);
            }

            // 2. Đặt vào MainGame.unity (Zone_02_Emerald_Steppe và Map_Zone_2)
            if (File.Exists(MainGameScenePath))
            {
                PlaceInScene(MainGameScenePath, isMainGame: true);
            }

            // Mở lại Scene làm việc chính MainGame.unity (chứa đầy đủ cả Map 1 và Map 2)
            if (File.Exists(MainGameScenePath) && !Application.isBatchMode)
            {
                EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>🎉 Đã hoàn tất đặt 5 Linh Thú vào Map thứ 2 trên toàn bộ Scene dự án!</color>");
        }

        public static void PlaceInScene(string scenePath, bool isMainGame)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[PlacePetsInMap2] Không thể mở scene {scenePath}");
                return;
            }

            int placedCount = 0;

            // 1. Đặt vào Zone_02_Emerald_Steppe (Khu vực 2 của Bản đồ Rừng / Thảo Nguyên)
            GameObject zone2Go = GameObject.Find("Zone_02_Emerald_Steppe");
            if (zone2Go != null)
            {
                Transform petContainer = zone2Go.transform.Find("Pets_Zone2");
                if (petContainer == null)
                {
                    GameObject pObj = new GameObject("Pets_Zone2");
                    pObj.transform.SetParent(zone2Go.transform, false);
                    petContainer = pObj.transform;
                }

                foreach (var info in PetsToPlace)
                {
                    // Xóa pet cũ trùng tên nếu có để idempotent
                    Transform oldPet = petContainer.Find(info.instanceName);
                    if (oldPet != null) UnityEngine.Object.DestroyImmediate(oldPet.gameObject);

                    GameObject go = SpawnAndConfigureWildPet(info.prefabName, info.instanceName, info.steppePosition, petContainer);
                    if (go != null) placedCount++;
                }
            }

            // 2. Nếu là MainGame.unity và có Map_Zone_2 (Map 2 tách biệt)
            if (isMainGame)
            {
                GameObject map2Root = GameObject.Find("Map_Zone_2");
                if (map2Root != null)
                {
                    Transform petContainer = map2Root.transform.Find("Map2_Spawners_Pets");
                    if (petContainer == null)
                    {
                        GameObject pObj = new GameObject("Map2_Spawners_Pets");
                        pObj.transform.SetParent(map2Root.transform, false);
                        petContainer = pObj.transform;
                    }

                    foreach (var info in PetsToPlace)
                    {
                        Transform oldPet = petContainer.Find("Map2_" + info.instanceName);
                        if (oldPet != null) UnityEngine.Object.DestroyImmediate(oldPet.gameObject);

                        GameObject go = SpawnAndConfigureWildPet(info.prefabName, "Map2_" + info.instanceName, info.mapZone2Position, petContainer);
                        if (go != null) placedCount++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"<color=green>[PlacePetsInMap2] Đã lưu {placedCount} Linh Thú vào Scene: {scenePath}</color>");
        }

        private static GameObject SpawnAndConfigureWildPet(string prefabName, string instanceName, Vector3 pos, Transform parent)
        {
            string prefabPath = $"Assets/Prefabs/Pets/{prefabName}.prefab";
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError($"[PlacePetsInMap2] Không tìm thấy Prefab tại {prefabPath}");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            instance.name = instanceName;
            instance.transform.position = pos;
            if (parent != null) instance.transform.SetParent(parent, true);

            // Cấu hình Hoang Dã: không tự bám đuôi người chơi từ xa
            PetController ctrl = instance.GetComponent<PetController>();
            if (ctrl != null) ctrl.isWild = true;

            PetAI ai = instance.GetComponent<PetAI>();
            if (ai != null) ai.isWild = true;

            // Gắn WildPetInteractable để người chơi có thể lại gần tương tác / thu nhận
            WildPetInteractable interactable = instance.GetComponent<WildPetInteractable>();
            if (interactable == null) interactable = instance.AddComponent<WildPetInteractable>();

            EditorUtility.SetDirty(instance);
            return instance;
        }

        public static void BatchExecute()
        {
            PlacePetsInAllMap2Scenes();
        }
    }
}
