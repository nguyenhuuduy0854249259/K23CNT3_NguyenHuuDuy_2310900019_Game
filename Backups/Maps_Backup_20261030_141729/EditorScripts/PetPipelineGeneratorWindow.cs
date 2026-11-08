using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using ActionRPG.Core;
using ActionRPG.Combat;
using ActionRPG.Classes.Mechanics;
using ActionRPG.Pets;
using ActionRPG.Visuals;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ tự động hóa toàn diện Pipeline Thú Cưng (Pet Sprite Animation & Prefab Pipeline):
    /// - Kiểm tra & chuẩn hóa TextureImporter / Slicing Metadata (giữ nguyên GUID & sub-sprite internalID nếu đã cắt chuẩn).
    /// - Tự động sinh 8 Animation Clips (Idle 4 hướng + Move 4 hướng) sắp xếp đúng thứ tự số tự nhiên (_0.._8).
    /// - Tự động sinh AnimatorController với 2 Blend Tree 2D Simple Directional (Idle & Move) nhớ hướng nhìn cuối cùng.
    /// - Tự động sinh Prefab hoàn chỉnh (SpriteRenderer, Animator, Rigidbody2D, CircleCollider2D IsTrigger, PetController, PetAI, YSortOrder2D).
    /// - Tự động sinh và liên kết ScriptableObject PetData (ID, Tên hiển thị, Icon từ <TenPet>.png, Sprite mặc định và Prefab).
    /// - Hỗ trợ Idempotent 100%: chạy lại nhiều lần cập nhật tại chỗ, không tạo file rác hay làm gãy tham chiếu.
    /// </summary>
    public class PetPipelineGeneratorWindow : EditorWindow
    {
        public const string PetSpritesRoot = "Assets/Sprites/pet";
        public const string PetAnimationsRoot = "Assets/Animations/Pets";
        public const string PetPrefabsRoot = "Assets/Prefabs/Pets";
        public const string PetScriptableObjectsRoot = "Assets/ScriptableObjects/Pets";
        private const string AutoRunSentinelPath = "Temp/PetPipelineAutoRun_v2.done";
        private const string VerificationReportPath = "Logs/PetPipelineVerificationReport.txt";

        [Serializable]
        public class PetPresetConfig
        {
            public string folderName;
            public string petId;
            public string displayName;
            public string speciesName;
            public PetType petType = PetType.Spirit;
            public ElementType element = ElementType.Fire;
            public PetRole role = PetRole.DPS;
            public PetBodyShape bodyShape = PetBodyShape.SmallFast;
            public float moveFps = 8f;
            public int sheetDownIndex = 1;
            public int sheetUpIndex = 2;
            public int sheetLeftIndex = 3;
            public int sheetRightIndex = 4;
            public int defaultColumnsIfUnsliced = 9;
            public float maxHp = 110f;
            public float attackPower = 16f;
            public float defense = 6f;
            public float moveSpeed = 5.2f;
        }

        private static readonly Dictionary<string, PetPresetConfig> BuiltInPresets = new Dictionary<string, PetPresetConfig>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "CaDuoiHuKhong", new PetPresetConfig
                {
                    folderName = "CaDuoiHuKhong",
                    petId = "pet_caduoi_hukhong",
                    displayName = "Cá Đuối Hư Không",
                    speciesName = "Hư Không Linh Ngư",
                    petType = PetType.Flying,
                    element = ElementType.Demon,
                    role = PetRole.Support,
                    bodyShape = PetBodyShape.Flying,
                    moveFps = 8f,
                    maxHp = 110f,
                    attackPower = 18f,
                    defense = 6f,
                    moveSpeed = 5.6f
                }
            },
            {
                "DaMinhTucDang", new PetPresetConfig
                {
                    folderName = "DaMinhTucDang",
                    petId = "pet_daminh_tucdang",
                    displayName = "Dạ Minh Túc Đằng",
                    speciesName = "Tinh Thể Mộc Linh",
                    petType = PetType.Elemental,
                    element = ElementType.Angel,
                    role = PetRole.Support,
                    bodyShape = PetBodyShape.LongSlender,
                    moveFps = 8f,
                    maxHp = 100f,
                    attackPower = 15f,
                    defense = 7f,
                    moveSpeed = 5.2f
                }
            },
            {
                "HuDocNamNhan", new PetPresetConfig
                {
                    folderName = "HuDocNamNhan",
                    petId = "pet_hudoc_namnhan",
                    displayName = "Hủ Độc Nấm Nhãn",
                    speciesName = "Độc Nhãn Linh Khuẩn",
                    petType = PetType.Spirit,
                    element = ElementType.Poison,
                    role = PetRole.Debuffer,
                    bodyShape = PetBodyShape.SmallFast,
                    moveFps = 6f,
                    maxHp = 105f,
                    attackPower = 17f,
                    defense = 8f,
                    moveSpeed = 4.8f
                }
            },
            {
                "NgaiThaoVuCau", new PetPresetConfig
                {
                    folderName = "NgaiThaoVuCau",
                    petId = "pet_ngaithao_vucau",
                    displayName = "Ngải Thảo Vu Cẩu",
                    speciesName = "Vu Thảo Linh Khuyển",
                    petType = PetType.Spirit,
                    element = ElementType.Demon,
                    role = PetRole.DPS,
                    bodyShape = PetBodyShape.SmallFast,
                    moveFps = 8f,
                    maxHp = 115f,
                    attackPower = 20f,
                    defense = 7f,
                    moveSpeed = 5.5f
                }
            },
            {
                "ThucKimNeBa", new PetPresetConfig
                {
                    folderName = "ThucKimNeBa",
                    petId = "pet_thuckim_neba",
                    displayName = "Thực Kim Nê Ba",
                    speciesName = "Hoàng Kim Nê Linh",
                    petType = PetType.Liquid,
                    element = ElementType.Earth,
                    role = PetRole.Tanker,
                    bodyShape = PetBodyShape.HeavyLarge,
                    moveFps = 7f,
                    maxHp = 150f,
                    attackPower = 14f,
                    defense = 12f,
                    moveSpeed = 4.5f
                }
            }
        };

        // Trạng thái UI của EditorWindow
        private string selectedFolderPath = "Assets/Sprites/pet/CaDuoiHuKhong";
        private PetPresetConfig currentConfig = new PetPresetConfig();
        private Vector2 scrollPos;
        private string statusMessage = "Sẵn sàng xử lý tài nguyên Pet.";

        [InitializeOnLoadMethod]
        private static void AutoBuildOnLoadIfNeeded()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return;

                bool missingAnyPetPrefab = !File.Exists("Assets/Prefabs/Pets/Pet_CaDuoiHuKhong.prefab") ||
                                           !File.Exists("Assets/Prefabs/Pets/Pet_DaMinhTucDang.prefab") ||
                                           !File.Exists("Assets/Prefabs/Pets/Pet_HuDocNamNhan.prefab") ||
                                           !File.Exists("Assets/Prefabs/Pets/Pet_NgaiThaoVuCau.prefab") ||
                                           !File.Exists("Assets/Prefabs/Pets/Pet_ThucKimNeBa.prefab");

                if (missingAnyPetPrefab || !File.Exists(AutoRunSentinelPath))
                {
                    try
                    {
                        BuildAllPetsInFolder();
                        VerifyAllPetsPipeline();
                        Directory.CreateDirectory(Path.GetDirectoryName(AutoRunSentinelPath));
                        File.WriteAllText(AutoRunSentinelPath, DateTime.Now.ToString("O"));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[PetPipelineGeneratorWindow] Lỗi khi tự động khởi tạo Pet Pipeline: {ex}");
                    }
                }
            };
        }

        [MenuItem("Tools/ActionRPG/Pet Pipeline Generator Window", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<PetPipelineGeneratorWindow>("Pet Pipeline Generator");
            window.minSize = new Vector2(520, 620);
            window.LoadPresetForFolder(window.selectedFolderPath);
            window.Show();
        }

        public static void RunBatchPipeline()
        {
            Debug.Log("[PetPipeline] Starting RunBatchPipeline...");
            BuildAllPetsInFolder();
            bool ok = VerifyAllPetsPipeline();
            ActionRPG.Tests.Sprint1PetConsistencyTest.RunAllPetTests();
            AssetDatabase.SaveAssets();
            if (!ok)
            {
                throw new Exception("PetPipeline Verification failed! Check Logs/PetPipelineVerificationReport.txt");
            }
            Debug.Log("[PetPipeline] RunBatchPipeline completed with 0 errors!");
        }

        [MenuItem("Tools/ActionRPG/Build All 5 Pets Pipeline", priority = 101)]
        public static void BuildAllPetsInFolder()
        {
            EnsureDirectories();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (!Directory.Exists(PetSpritesRoot))
            {
                Debug.LogError($"[PetPipeline] Không tìm thấy thư mục gốc: {PetSpritesRoot}");
                return;
            }

            string[] subDirs = Directory.GetDirectories(PetSpritesRoot);
            int successCount = 0;

            foreach (string rawDir in subDirs)
            {
                string folderName = Path.GetFileName(rawDir);
                if (string.IsNullOrEmpty(folderName) || folderName.StartsWith(".")) continue;

                PetPresetConfig cfg = GetOrCreatePreset(folderName);
                if (GeneratePetAssets(cfg, out string summary))
                {
                    successCount++;
                    Debug.Log($"<color=green>[PetPipeline] ✔ Hoàn tất Pet '{cfg.displayName}' ({cfg.folderName}): {summary}</color>");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"<color=cyan>[PetPipeline] 🎉 Đã tạo/cập nhật thành công {successCount}/{subDirs.Length} Pet trong '{PetSpritesRoot}'!</color>");
        }

        [MenuItem("Tools/ActionRPG/Verify All 5 Pets Pipeline", priority = 102)]
        public static bool VerifyAllPetsPipeline()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== PET PIPELINE VERIFICATION REPORT ===");
            report.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            string[] expectedFolders = { "CaDuoiHuKhong", "DaMinhTucDang", "HuDocNamNhan", "NgaiThaoVuCau", "ThucKimNeBa" };
            int totalErrors = 0;

            foreach (string folder in expectedFolders)
            {
                report.AppendLine($"\n--- Kiểm tra Pet: {folder} ---");

                // 1. Kiểm tra Icon
                string iconPath = $"{PetSpritesRoot}/{folder}/{folder}.png";
                Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (iconSprite == null)
                {
                    report.AppendLine($"[ERROR] Thiếu Icon Sprite tại {iconPath}");
                    totalErrors++;
                }
                else
                {
                    report.AppendLine($"[OK] Icon Sprite: {iconPath} ({iconSprite.rect.width}x{iconSprite.rect.height})");
                }

                // 2. Kiểm tra 4 sheet hướng
                for (int i = 1; i <= 4; i++)
                {
                    string sheetPath = $"{PetSpritesRoot}/{folder}/{folder}{i}.png";
                    Sprite[] frames = LoadSortedSpritesFromSheet(sheetPath);
                    if (frames == null || frames.Length == 0)
                    {
                        report.AppendLine($"[ERROR] Sheet {sheetPath} không có sub-sprites!");
                        totalErrors++;
                    }
                    else
                    {
                        report.AppendLine($"[OK] Sheet {i}: {sheetPath} -> {frames.Length} frames ({frames[0].name} .. {frames[frames.Length - 1].name})");
                    }
                }

                // 3. Kiểm tra 8 Animation Clips
                string[] clipNames = { "Idle_Down", "Idle_Up", "Idle_Left", "Idle_Right", "Move_Down", "Move_Up", "Move_Left", "Move_Right" };
                foreach (string cName in clipNames)
                {
                    string clipPath = $"{PetAnimationsRoot}/{folder}/{cName}.anim";
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (clip == null)
                    {
                        report.AppendLine($"[ERROR] Thiếu AnimationClip: {clipPath}");
                        totalErrors++;
                        continue;
                    }

                    var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                    if (bindings.Length == 0)
                    {
                        report.AppendLine($"[ERROR] AnimationClip rỗng (không có curve binding): {clipPath}");
                        totalErrors++;
                        continue;
                    }

                    var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]);
                    bool hasNullSprite = keyframes.Any(k => k.value == null);
                    if (hasNullSprite || keyframes.Length == 0)
                    {
                        report.AppendLine($"[ERROR] AnimationClip có Missing Sprite: {clipPath}");
                        totalErrors++;
                    }
                    else
                    {
                        report.AppendLine($"[OK] Clip {cName}: {keyframes.Length} keyframes, Loop={AnimationUtility.GetAnimationClipSettings(clip).loopTime}, FPS={clip.frameRate}");
                    }
                }

                // 4. Kiểm tra AnimatorController
                string ctrlPath = $"{PetAnimationsRoot}/{folder}/{folder}_Controller.controller";
                AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
                if (ctrl == null)
                {
                    report.AppendLine($"[ERROR] Thiếu AnimatorController: {ctrlPath}");
                    totalErrors++;
                }
                else
                {
                    report.AppendLine($"[OK] AnimatorController: {ctrlPath} (Params={ctrl.parameters.Length}, States={ctrl.layers[0].stateMachine.states.Length})");
                }

                // 5. Kiểm tra Prefab
                string prefabPath = $"{PetPrefabsRoot}/Pet_{folder}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    report.AppendLine($"[ERROR] Thiếu Prefab: {prefabPath}");
                    totalErrors++;
                }
                else
                {
                    int missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
                    var sr = prefab.GetComponent<SpriteRenderer>();
                    var anim = prefab.GetComponent<Animator>();
                    var rb = prefab.GetComponent<Rigidbody2D>();
                    var col = prefab.GetComponent<Collider2D>();
                    var petCtrl = prefab.GetComponent<PetController>();
                    var petAi = prefab.GetComponent<PetAI>();

                    if (missingScripts > 0 || sr == null || sr.sprite == null || anim == null || anim.runtimeAnimatorController == null ||
                        rb == null || col == null || !col.isTrigger || petCtrl == null || petAi == null)
                    {
                        report.AppendLine($"[ERROR] Prefab {prefabPath} thiếu component hoặc có Missing Reference! (missingScripts={missingScripts})");
                        totalErrors++;
                    }
                    else
                    {
                        report.AppendLine($"[OK] Prefab: {prefabPath} (Sprite={sr.sprite.name}, Controller={anim.runtimeAnimatorController.name}, Collider.isTrigger={col.isTrigger}, MissingScripts=0)");
                    }
                }

                // 6. Kiểm tra ScriptableObject PetData
                string soPath = $"{PetScriptableObjectsRoot}/Pet_{folder}.asset";
                PetData petData = AssetDatabase.LoadAssetAtPath<PetData>(soPath);
                if (petData == null || petData.icon == null || petData.petSprite == null || petData.petPrefab == null)
                {
                    report.AppendLine($"[ERROR] ScriptableObject {soPath} thiếu dữ liệu liên kết!");
                    totalErrors++;
                }
                else
                {
                    report.AppendLine($"[OK] PetData SO: {soPath} (ID={petData.petId}, Name={petData.petName}, Icon={petData.icon.name}, Prefab={petData.petPrefab.name})");
                }
            }

            report.AppendLine($"\n=== TỔNG KẾT: {totalErrors} LỖI ===");
            Directory.CreateDirectory(Path.GetDirectoryName(VerificationReportPath));
            File.WriteAllText(VerificationReportPath, report.ToString(), Encoding.UTF8);

            if (totalErrors == 0)
                Debug.Log($"<color=green>[PetPipeline Verify] ✔ TẤT CẢ 5 PET ĐẠT CHUẨN 100% (0 lỗi)! Xem chi tiết tại {VerificationReportPath}</color>");
            else
                Debug.LogError($"[PetPipeline Verify] ❌ Phát hiện {totalErrors} lỗi! Xem chi tiết tại {VerificationReportPath}");

            return totalErrors == 0;
        }

        public static PetPresetConfig GetOrCreatePreset(string folderName)
        {
            if (BuiltInPresets.TryGetValue(folderName, out PetPresetConfig preset))
            {
                return JsonUtility.FromJson<PetPresetConfig>(JsonUtility.ToJson(preset));
            }

            return new PetPresetConfig
            {
                folderName = folderName,
                petId = "pet_" + folderName.ToLowerInvariant(),
                displayName = folderName,
                speciesName = "Linh Thú Đồng Hành",
                petType = PetType.Spirit,
                element = ElementType.Fire,
                role = PetRole.DPS,
                bodyShape = PetBodyShape.SmallFast,
                moveFps = 8f,
                sheetDownIndex = 1,
                sheetUpIndex = 2,
                sheetLeftIndex = 3,
                sheetRightIndex = 4,
                defaultColumnsIfUnsliced = 9,
                maxHp = 110f,
                attackPower = 16f,
                defense = 6f,
                moveSpeed = 5.2f
            };
        }

        private void LoadPresetForFolder(string folderAssetPath)
        {
            if (string.IsNullOrEmpty(folderAssetPath)) return;
            string folderName = Path.GetFileName(folderAssetPath.TrimEnd('/', '\\'));
            currentConfig = GetOrCreatePreset(folderName);
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("PET SPRITE ANIMATION & PREFAB PIPELINE", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Công cụ tự động hóa tạo Animation 4 hướng, Animator Controller (BlendTree 2D), Prefab và PetData ScriptableObject từ thư mục Assets/Sprites/pet/<TenPet>.\n" +
                "Đảm bảo tính Idempotent: bảo toàn GUID/internalID của các sheet đã cắt và cập nhật asset tại chỗ.",
                MessageType.Info);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("1. Chọn Thư Mục Pet", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Thư mục Pet", selectedFolderPath);
            if (GUILayout.Button("Chọn...", GUILayout.Width(80)))
            {
                string abs = EditorUtility.OpenFolderPanel("Chọn thư mục Pet trong Assets/Sprites/pet", PetSpritesRoot, "");
                if (!string.IsNullOrEmpty(abs))
                {
                    if (abs.StartsWith(Application.dataPath))
                    {
                        selectedFolderPath = "Assets" + abs.Substring(Application.dataPath.Length).Replace('\\', '/');
                        LoadPresetForFolder(selectedFolderPath);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Thư mục không hợp lệ", "Vui lòng chọn thư mục nằm trong Assets của dự án!", "OK");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // Nút chọn nhanh các thư mục có sẵn trong Assets/Sprites/pet
            if (Directory.Exists(PetSpritesRoot))
            {
                string[] dirs = Directory.GetDirectories(PetSpritesRoot);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Chọn nhanh:");
                foreach (string d in dirs)
                {
                    string fName = Path.GetFileName(d);
                    if (GUILayout.Button(fName, EditorStyles.miniButton))
                    {
                        selectedFolderPath = $"{PetSpritesRoot}/{fName}";
                        LoadPresetForFolder(selectedFolderPath);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("2. Cấu Hình Thông Số & Ánh Xạ Hướng", EditorStyles.boldLabel);
            currentConfig.folderName = EditorGUILayout.TextField("Tên Thư Mục (PascalCase)", currentConfig.folderName);
            currentConfig.petId = EditorGUILayout.TextField("Pet ID", currentConfig.petId);
            currentConfig.displayName = EditorGUILayout.TextField("Tên Hiển Thị (Tiếng Việt)", currentConfig.displayName);
            currentConfig.speciesName = EditorGUILayout.TextField("Chủng Loài", currentConfig.speciesName);
            currentConfig.petType = (PetType)EditorGUILayout.EnumPopup("Hệ Sinh Học (PetType)", currentConfig.petType);
            currentConfig.element = (ElementType)EditorGUILayout.EnumPopup("Nguyên Tố (Element)", currentConfig.element);
            currentConfig.role = (PetRole)EditorGUILayout.EnumPopup("Vai Trò (Role)", currentConfig.role);
            currentConfig.moveFps = EditorGUILayout.Slider("Tốc Độ Hoạt Ảnh (FPS)", currentConfig.moveFps, 4f, 16f);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Ánh Xạ File Spritesheet (<TenPet>1..4.png):", EditorStyles.miniBoldLabel);
            currentConfig.sheetDownIndex = EditorGUILayout.IntSlider("Sheet Hướng Down (Front)", currentConfig.sheetDownIndex, 1, 4);
            currentConfig.sheetUpIndex = EditorGUILayout.IntSlider("Sheet Hướng Up (Back)", currentConfig.sheetUpIndex, 1, 4);
            currentConfig.sheetLeftIndex = EditorGUILayout.IntSlider("Sheet Hướng Left", currentConfig.sheetLeftIndex, 1, 4);
            currentConfig.sheetRightIndex = EditorGUILayout.IntSlider("Sheet Hướng Right", currentConfig.sheetRightIndex, 1, 4);
            currentConfig.defaultColumnsIfUnsliced = EditorGUILayout.IntSlider("Số Cột Mặc Định (Nếu Chưa Slice)", currentConfig.defaultColumnsIfUnsliced, 1, 16);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("3. Thực Thi Pipeline", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
            if (GUILayout.Button($"⚡ Sinh/Cập Nhật Pet Đang Chọn ({currentConfig.folderName})", GUILayout.Height(34)))
            {
                EnsureDirectories();
                if (GeneratePetAssets(currentConfig, out string summary))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    statusMessage = $"✔ Thành công: {summary}";
                }
                else
                {
                    statusMessage = $"❌ Lỗi: {summary}";
                }
            }

            GUI.backgroundColor = new Color(0.25f, 0.65f, 0.95f);
            if (GUILayout.Button("🚀 Sinh/Cập Nhật Toàn Bộ 5 Pet Trong Assets/Sprites/pet", GUILayout.Height(34)))
            {
                BuildAllPetsInFolder();
                statusMessage = "✔ Đã hoàn tất xử lý toàn bộ Pet trong Assets/Sprites/pet!";
            }

            GUI.backgroundColor = new Color(0.95f, 0.75f, 0.25f);
            if (GUILayout.Button("🔍 Kiểm Thử & Xuất Báo Cáo (Verify All 5 Pets)", GUILayout.Height(30)))
            {
                bool ok = VerifyAllPetsPipeline();
                statusMessage = ok ? "✔ Kiểm thử thành công 100% (0 lỗi)!" : "❌ Có lỗi kiểm thử, xem Console!";
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(statusMessage, MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(PetAnimationsRoot);
            Directory.CreateDirectory(PetPrefabsRoot);
            Directory.CreateDirectory(PetScriptableObjectsRoot);
        }

        /// <summary>
        /// Thực thi toàn bộ quy trình sinh asset cho 1 Pet cụ thể (Idempotent).
        /// </summary>
        public static bool GeneratePetAssets(PetPresetConfig cfg, out string summary)
        {
            summary = string.Empty;
            string petSpriteFolder = $"{PetSpritesRoot}/{cfg.folderName}";
            if (!Directory.Exists(petSpriteFolder))
            {
                summary = $"Không tìm thấy thư mục '{petSpriteFolder}'";
                Debug.LogError($"[PetPipeline] {summary}");
                return false;
            }

            // 1. Chuẩn hóa & nạp Icon gốc (<TenPet>.png - Single Sprite)
            string iconPath = $"{petSpriteFolder}/{cfg.folderName}.png";
            EnsureIconImporterSettings(iconPath);
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            // 2. Chuẩn hóa & nạp 4 bộ frames từ <TenPet>1..4.png (Multiple Sprites, bảo toàn GUID & Slicing nếu đã cắt)
            string downSheetPath = $"{petSpriteFolder}/{cfg.folderName}{cfg.sheetDownIndex}.png";
            string upSheetPath = $"{petSpriteFolder}/{cfg.folderName}{cfg.sheetUpIndex}.png";
            string leftSheetPath = $"{petSpriteFolder}/{cfg.folderName}{cfg.sheetLeftIndex}.png";
            string rightSheetPath = $"{petSpriteFolder}/{cfg.folderName}{cfg.sheetRightIndex}.png";

            Sprite[] downFrames = EnsureAndLoadSheetFrames(downSheetPath, cfg.defaultColumnsIfUnsliced);
            Sprite[] upFrames = EnsureAndLoadSheetFrames(upSheetPath, cfg.defaultColumnsIfUnsliced);
            Sprite[] leftFrames = EnsureAndLoadSheetFrames(leftSheetPath, cfg.defaultColumnsIfUnsliced);
            Sprite[] rightFrames = EnsureAndLoadSheetFrames(rightSheetPath, cfg.defaultColumnsIfUnsliced);

            if (downFrames.Length == 0 || upFrames.Length == 0 || leftFrames.Length == 0 || rightFrames.Length == 0)
            {
                summary = $"Thiếu sub-sprites ở một trong các file {cfg.folderName}1..4.png (Down={downFrames.Length}, Up={upFrames.Length}, Left={leftFrames.Length}, Right={rightFrames.Length})";
                Debug.LogError($"[PetPipeline] {summary}");
                return false;
            }

            // 3. Tạo thư mục Animation của Pet và sinh 8 Animation Clips
            string animDir = $"{PetAnimationsRoot}/{cfg.folderName}";
            Directory.CreateDirectory(animDir);

            AnimationClip idleDown = CreateOrUpdateSpriteClip($"{animDir}/Idle_Down.anim", new[] { downFrames[0] }, cfg.moveFps, true);
            AnimationClip idleUp = CreateOrUpdateSpriteClip($"{animDir}/Idle_Up.anim", new[] { upFrames[0] }, cfg.moveFps, true);
            AnimationClip idleLeft = CreateOrUpdateSpriteClip($"{animDir}/Idle_Left.anim", new[] { leftFrames[0] }, cfg.moveFps, true);
            AnimationClip idleRight = CreateOrUpdateSpriteClip($"{animDir}/Idle_Right.anim", new[] { rightFrames[0] }, cfg.moveFps, true);

            AnimationClip moveDown = CreateOrUpdateSpriteClip($"{animDir}/Move_Down.anim", downFrames, cfg.moveFps, true);
            AnimationClip moveUp = CreateOrUpdateSpriteClip($"{animDir}/Move_Up.anim", upFrames, cfg.moveFps, true);
            AnimationClip moveLeft = CreateOrUpdateSpriteClip($"{animDir}/Move_Left.anim", leftFrames, cfg.moveFps, true);
            AnimationClip moveRight = CreateOrUpdateSpriteClip($"{animDir}/Move_Right.anim", rightFrames, cfg.moveFps, true);

            // 4. Tạo hoặc cập nhật AnimatorController với 2D Simple Directional Blend Trees
            string controllerPath = $"{animDir}/{cfg.folderName}_Controller.controller";
            AnimatorController controller = CreateOrUpdateAnimatorController(
                controllerPath,
                idleDown, idleUp, idleLeft, idleRight,
                moveDown, moveUp, moveLeft, moveRight);

            // 5. Tạo hoặc cập nhật ScriptableObject PetData trước để gắn vào Prefab
            string soPath = $"{PetScriptableObjectsRoot}/Pet_{cfg.folderName}.asset";
            PetData petData = CreateOrUpdatePetDataSO(soPath, cfg, iconSprite, downFrames[0], null);

            // 6. Tạo hoặc cập nhật Prefab hoàn chỉnh tại Assets/Prefabs/Pets/Pet_<TenPet>.prefab
            string prefabPath = $"{PetPrefabsRoot}/Pet_{cfg.folderName}.prefab";
            GameObject prefabAsset = CreateOrUpdatePetPrefab(prefabPath, cfg, downFrames[0], controller, petData);

            // 7. Liên kết ngược Prefab vào PetData ScriptableObject
            petData.petPrefab = prefabAsset;
            EditorUtility.SetDirty(petData);

            summary = $"Frames(D/U/L/R)={downFrames.Length}/{upFrames.Length}/{leftFrames.Length}/{rightFrames.Length}, FPS={cfg.moveFps}, Prefab={prefabPath}, SO={soPath}";
            return true;
        }

        /// <summary>
        /// Đảm bảo file <TenPet>.png là Single Sprite, FilterMode = Point, Uncompressed.
        /// </summary>
        private static void EnsureIconImporterSettings(string assetPath)
        {
            if (!File.Exists(assetPath)) return;
            TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti == null) return;

            bool dirty = false;
            if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; dirty = true; }
            if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; dirty = true; }
            if (ti.filterMode != FilterMode.Point) { ti.filterMode = FilterMode.Point; dirty = true; }
            if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
            if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; dirty = true; }

            if (dirty)
            {
                ti.SaveAndReimport();
            }
        }

        /// <summary>
        /// Kiểm tra spritesheet hướng (<TenPet>1..4.png):
        /// - Nếu đã được slice chuẩn (Multiple Sprites): GIỮ NGUYÊN GUID và sub-sprites hiện có, chỉ đảm bảo Point + Uncompressed.
        /// - Nếu chưa slice (khi thêm Pet thứ 6, 7... trong tương lai): Tự động chia lưới đều theo defaultColumns.
        /// - Trả về mảng Sprite đã sắp xếp theo đúng số thứ tự tự nhiên (_0, _1, ..., _8, _9, _10).
        /// </summary>
        public static Sprite[] EnsureAndLoadSheetFrames(string sheetAssetPath, int defaultColumns = 9)
        {
            if (!File.Exists(sheetAssetPath)) return Array.Empty<Sprite>();

            TextureImporter ti = AssetImporter.GetAtPath(sheetAssetPath) as TextureImporter;
            if (ti != null)
            {
                bool settingsChanged = false;
                if (ti.filterMode != FilterMode.Point)
                {
                    ti.filterMode = FilterMode.Point;
                    settingsChanged = true;
                }
                if (ti.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    ti.textureCompression = TextureImporterCompression.Uncompressed;
                    settingsChanged = true;
                }
                if (!ti.alphaIsTransparency)
                {
                    ti.alphaIsTransparency = true;
                    settingsChanged = true;
                }

                Sprite[] existingSprites = LoadSortedSpritesFromSheet(sheetAssetPath);

                // Chỉ tự động slice nếu file chưa ở chế độ Multiple hoặc chưa có sub-sprites (dành cho Pet mới thứ 6, 7...)
                if (ti.spriteImportMode != SpriteImportMode.Multiple || existingSprites.Length <= 1)
                {
                    AutoSliceUniformHorizontalStrip(sheetAssetPath, ti, Mathf.Max(1, defaultColumns));
                    settingsChanged = false; // AutoSlice đã tự SaveAndReimport
                }

                if (settingsChanged)
                {
                    ti.SaveAndReimport();
                }
            }

            return LoadSortedSpritesFromSheet(sheetAssetPath);
        }

        /// <summary>
        /// Tải toàn bộ Sprite con từ 1 sheet và sắp xếp theo đúng chỉ số tự nhiên (_0, _1, _2, ..., _9, _10).
        /// Tuyệt đối tránh lỗi sắp xếp chuỗi chữ cái (_0, _1, _10, _2).
        /// </summary>
        public static Sprite[] LoadSortedSpritesFromSheet(string sheetAssetPath)
        {
            UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(sheetAssetPath);
            if (allAssets == null || allAssets.Length == 0) return Array.Empty<Sprite>();

            List<Sprite> sprites = new List<Sprite>();
            foreach (UnityEngine.Object obj in allAssets)
            {
                if (obj is Sprite s)
                {
                    sprites.Add(s);
                }
            }

            sprites.Sort((a, b) =>
            {
                int idxA = ExtractTrailingNumber(a.name);
                int idxB = ExtractTrailingNumber(b.name);
                if (idxA != idxB && idxA >= 0 && idxB >= 0)
                    return idxA.CompareTo(idxB);

                int xCmp = a.rect.x.CompareTo(b.rect.x);
                if (xCmp != 0) return xCmp;
                return string.Compare(a.name, b.name, StringComparison.Ordinal);
            });

            return sprites.ToArray();
        }

        private static int ExtractTrailingNumber(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return -1;
            Match m = Regex.Match(spriteName, @"_(\d+)$");
            if (m.Success && int.TryParse(m.Groups[1].Value, out int index))
            {
                return index;
            }
            return -1;
        }

        /// <summary>
        /// Tự động chia lưới ngang đồng nhất cho Pet mới chưa được slice.
        /// </summary>
        private static void AutoSliceUniformHorizontalStrip(string sheetAssetPath, TextureImporter ti, int columns)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetAssetPath);
            if (tex == null)
            {
                ti.SaveAndReimport();
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetAssetPath);
            }
            if (tex == null) return;

            int width = tex.width;
            int height = tex.height;
            string baseName = Path.GetFileNameWithoutExtension(sheetAssetPath);

            SerializedObject so = new SerializedObject(ti);
            SerializedProperty spriteSheetProp = so.FindProperty("m_SpriteSheet.m_Sprites");
            if (spriteSheetProp != null)
            {
                spriteSheetProp.ClearArray();
                for (int i = 0; i < columns; i++)
                {
                    int x0 = Mathf.RoundToInt((float)i * width / columns);
                    int x1 = Mathf.RoundToInt((float)(i + 1) * width / columns);
                    int w = Mathf.Max(1, x1 - x0);

                    spriteSheetProp.InsertArrayElementAtIndex(i);
                    SerializedProperty elem = spriteSheetProp.GetArrayElementAtIndex(i);
                    elem.FindPropertyRelative("m_Name").stringValue = $"{baseName}_{i}";
                    elem.FindPropertyRelative("m_Rect").rectValue = new Rect(x0, 0, w, height);
                    elem.FindPropertyRelative("m_Alignment").intValue = (int)SpriteAlignment.Center;
                    elem.FindPropertyRelative("m_Pivot").vector2Value = new Vector2(0.5f, 0.5f);
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            ti.SaveAndReimport();
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật tại chỗ (Idempotent) AnimationClip cho SpriteRenderer.m_Sprite.
        /// </summary>
        private static AnimationClip CreateOrUpdateSpriteClip(string clipAssetPath, Sprite[] frames, float fps, bool loop)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            bool isNew = false;
            if (clip == null)
            {
                clip = new AnimationClip
                {
                    name = Path.GetFileNameWithoutExtension(clipAssetPath)
                };
                isNew = true;
            }

            clip.frameRate = Mathf.Max(1f, fps);

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes;
            if (frames.Length == 1)
            {
                // Giữ ổn định frame Idle ở đầu và cuối chu kỳ
                keyframes = new ObjectReferenceKeyframe[2];
                keyframes[0] = new ObjectReferenceKeyframe { time = 0f, value = frames[0] };
                keyframes[1] = new ObjectReferenceKeyframe { time = 1f / clip.frameRate, value = frames[0] };
            }
            else
            {
                keyframes = new ObjectReferenceKeyframe[frames.Length];
                for (int i = 0; i < frames.Length; i++)
                {
                    keyframes[i] = new ObjectReferenceKeyframe
                    {
                        time = i / clip.frameRate,
                        value = frames[i]
                    };
                }
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            if (isNew)
            {
                AssetDatabase.CreateAsset(clip, clipAssetPath);
            }
            else
            {
                EditorUtility.SetDirty(clip);
            }

            return clip;
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật tại chỗ AnimatorController sử dụng 3 tham số:
        /// MoveX (Float), MoveY (Float), IsMoving (Bool)
        /// cùng 2 Blend Tree 2D Simple Directional (Idle & Move) giữ nguyên hướng nhìn cuối khi dừng lại.
        /// </summary>
        private static AnimatorController CreateOrUpdateAnimatorController(
            string controllerPath,
            AnimationClip idleDown, AnimationClip idleUp, AnimationClip idleLeft, AnimationClip idleRight,
            AnimationClip moveDown, AnimationClip moveUp, AnimationClip moveLeft, AnimationClip moveRight)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Dọn sạch các sub-assets BlendTree/State cũ bên trong controller để tránh trùng lặp khi chạy lại (Idempotent)
            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(controllerPath);
            foreach (UnityEngine.Object sub in subAssets)
            {
                if (sub is BlendTree)
                {
                    UnityEngine.Object.DestroyImmediate(sub, true);
                }
            }

            // Đảm bảo đúng 3 tham số điều khiển: MoveX, MoveY, IsMoving
            EnsureControllerParameter(controller, "MoveX", AnimatorControllerParameterType.Float, 0f, false);
            EnsureControllerParameter(controller, "MoveY", AnimatorControllerParameterType.Float, -1f, false);
            EnsureControllerParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool, 0f, false);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            // Xóa các state cũ trong StateMachine trước khi thiết lập lại chuẩn
            var existingStates = sm.states.ToArray();
            foreach (var st in existingStates)
            {
                sm.RemoveState(st.state);
            }

            // 1. Tạo BlendTree Idle (4 hướng)
            AnimatorState idleState = controller.CreateBlendTreeInController("Idle", out BlendTree idleTree, 0);
            idleTree.name = "Idle_BlendTree";
            idleTree.blendType = BlendTreeType.SimpleDirectional2D;
            idleTree.blendParameter = "MoveX";
            idleTree.blendParameterY = "MoveY";
            idleTree.AddChild(idleDown, new Vector2(0f, -1f));
            idleTree.AddChild(idleUp, new Vector2(0f, 1f));
            idleTree.AddChild(idleLeft, new Vector2(-1f, 0f));
            idleTree.AddChild(idleRight, new Vector2(1f, 0f));

            // 2. Tạo BlendTree Move (4 hướng)
            AnimatorState moveState = controller.CreateBlendTreeInController("Move", out BlendTree moveTree, 0);
            moveTree.name = "Move_BlendTree";
            moveTree.blendType = BlendTreeType.SimpleDirectional2D;
            moveTree.blendParameter = "MoveX";
            moveTree.blendParameterY = "MoveY";
            moveTree.AddChild(moveDown, new Vector2(0f, -1f));
            moveTree.AddChild(moveUp, new Vector2(0f, 1f));
            moveTree.AddChild(moveLeft, new Vector2(-1f, 0f));
            moveTree.AddChild(moveRight, new Vector2(1f, 0f));

            sm.defaultState = idleState;

            // Chuyển trạng thái Idle <-> Move tức thì (không có độ trễ Exit Time)
            AnimatorStateTransition toMove = idleState.AddTransition(moveState);
            toMove.hasExitTime = false;
            toMove.duration = 0f;
            toMove.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            AnimatorStateTransition toIdle = moveState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0f;
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            EditorUtility.SetDirty(idleTree);
            EditorUtility.SetDirty(moveTree);
            EditorUtility.SetDirty(sm);
            EditorUtility.SetDirty(controller);

            return controller;
        }

        private static void EnsureControllerParameter(AnimatorController controller, string paramName, AnimatorControllerParameterType type, float defaultFloat, bool defaultBool)
        {
            foreach (var p in controller.parameters)
            {
                if (p.name == paramName) return;
            }

            AnimatorControllerParameter newParam = new AnimatorControllerParameter
            {
                name = paramName,
                type = type,
                defaultFloat = defaultFloat,
                defaultBool = defaultBool
            };
            controller.AddParameter(newParam);
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật tại chỗ ScriptableObject PetData.
        /// </summary>
        private static PetData CreateOrUpdatePetDataSO(string soPath, PetPresetConfig cfg, Sprite iconSprite, Sprite defaultDownSprite, GameObject prefabRef)
        {
            PetData data = AssetDatabase.LoadAssetAtPath<PetData>(soPath);
            bool isNew = false;
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<PetData>();
                isNew = true;
            }

            data.petId = cfg.petId;
            data.petName = cfg.displayName;
            data.speciesName = cfg.speciesName;
            data.petType = cfg.petType;
            data.element = cfg.element;
            data.role = cfg.role;
            data.bodyShape = cfg.bodyShape;
            data.icon = iconSprite != null ? iconSprite : defaultDownSprite;
            data.petSprite = defaultDownSprite;
            data.maxHp = cfg.maxHp;
            data.attackPower = cfg.attackPower;
            data.defense = cfg.defense;
            data.moveSpeed = cfg.moveSpeed;

            if (prefabRef != null)
            {
                data.petPrefab = prefabRef;
            }

            if (isNew)
            {
                AssetDatabase.CreateAsset(data, soPath);
            }
            else
            {
                EditorUtility.SetDirty(data);
            }

            return data;
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật tại chỗ Prefab Pet hoàn chỉnh tại Assets/Prefabs/Pets/Pet_<TenPet>.prefab.
        /// </summary>
        private static GameObject CreateOrUpdatePetPrefab(
            string prefabPath,
            PetPresetConfig cfg,
            Sprite defaultDownSprite,
            AnimatorController controller,
            PetData petData)
        {
            GameObject root = new GameObject($"Pet_{cfg.folderName}");

            try
            {
                // 1. SpriteRenderer: Sprite mặc định hướng Down, Color = White, Sorting Layer & Order đồng bộ Player/NPC
                SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
                sr.sprite = defaultDownSprite;
                sr.color = Color.white;
                sr.sortingLayerID = 0;
                sr.sortingOrder = 10;

                string urpLitMatPath = AssetDatabase.GUIDToAssetPath("a97c105638bdf8b4a8650670310a4cd3");
                if (!string.IsNullOrEmpty(urpLitMatPath))
                {
                    Material litMat = AssetDatabase.LoadAssetAtPath<Material>(urpLitMatPath);
                    if (litMat != null) sr.sharedMaterial = litMat;
                }

                // 2. Animator: Gán đúng AnimatorController 4 hướng của Pet
                Animator animator = root.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                // 3. Rigidbody2D: Dynamic với Gravity Scale = 0 và Freeze Rotation Z (tương thích linearVelocity của PetAI)
                Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                // 4. CircleCollider2D: Is Trigger = true để Pet đi theo không đẩy hoặc chặn đường Player
                CircleCollider2D col = root.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.35f;
                col.offset = new Vector2(0f, -0.1f);

                // 5. PetController & PetAI: Tái sử dụng hệ thống điều khiển Pet của dự án
                PetController petController = root.AddComponent<PetController>();
                petController.petData = petData;
                petController.minFollowDistance = 1.5f;
                petController.maxFollowDistance = 2.5f;
                petController.teleportDistance = 12.0f;

                PetAI petAI = root.AddComponent<PetAI>();
                petAI.minFollowDistance = 1.5f;
                petAI.maxFollowDistance = 2.5f;
                petAI.teleportDistance = 12.0f;

                // 6. YSortOrder2D: Đồng bộ chiều sâu Y-Sorting với Player và NPC
                YSortOrder2D ySort = root.AddComponent<YSortOrder2D>();
                ySort.footOffsetY = -0.25f;
                ySort.baseSortingOffset = 5000;
                ySort.sortFactor = 32f;
                ySort.isStatic = false;

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out bool success);
                if (!success || savedPrefab == null)
                {
                    Debug.LogError($"[PetPipeline] Không thể lưu Prefab tại {prefabPath}");
                }
                return savedPrefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
