#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using ActionRPG.Classes;
using ActionRPG.Combat;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Bộ cơ sở dữ liệu và Generator tự động cho toàn bộ 14 Class và hệ thống Kỹ Năng của Lumina.
    /// Khớp 100% với tài liệu đặc tả KI trong 'lumina_full_classes_and_mechanics_spec.md'.
    /// Menu: Tools -> Lumina -> Generate All Classes & Skills Database
    /// </summary>
    public static class LuminaClassAssetDatabase
    {
        private const string ClassesDir = "Assets/ScriptableObjects/Classes";
        private const string SkillsDir = "Assets/ScriptableObjects/Skills";

        [MenuItem("Tools/Lumina/Generate All Classes & Skills Database")]
        public static void GenerateAll()
        {
            EnsureDirectories();

            // 1. Tạo kho SkillData
            var skills = GenerateAllSkills();

            // 2. Tạo kho CharacterClassData (14 Classes)
            var classMap = GenerateAllClasses(skills);

            // 3. Liên kết nhánh chuyển nghề (Promotion Trees)
            LinkPromotionTrees(classMap);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LUMINA DATABASE] Đã khởi tạo và đồng bộ thành công toàn bộ 14 Class, 42 Ấn Nội Tại và 48 Kỹ Năng!</b></color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(ClassesDir)) Directory.CreateDirectory(ClassesDir);
            if (!Directory.Exists(SkillsDir)) Directory.CreateDirectory(SkillsDir);
        }

        private static Dictionary<string, SkillData> GenerateAllSkills()
        {
            var dict = new Dictionary<string, SkillData>();

            void CreateSkill(string id, string name, string desc, float mana, float cd, float mult, SkillTargetType type, ElementType elem)
            {
                string path = $"{SkillsDir}/Skill_{id}.asset";
                SkillData skill = AssetDatabase.LoadAssetAtPath<SkillData>(path);
                if (skill == null)
                {
                    skill = ScriptableObject.CreateInstance<SkillData>();
                    AssetDatabase.CreateAsset(skill, path);
                }

                skill.skillId = id;
                skill.skillName = name;
                skill.description = desc;
                skill.manaCost = mana;
                skill.cooldownSeconds = cd;
                skill.damageMultiplier = mult;
                skill.targetType = type;
                skill.element = elem;
                skill.range = (type == SkillTargetType.DirectMelee) ? 2.0f : 7.0f;
                skill.radius = (type == SkillTargetType.AreaOfEffect) ? 3.5f : 1.5f;

                Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/HUD/Skills/icon_skill_{id}.png");
                if (spr != null) skill.icon = spr;

                // Tự động liên kết VFX Prefab
                string[] parts = id.Split('_');
                for (int p = 0; p < parts.Length; p++)
                {
                    if (parts[p].Length > 0) parts[p] = char.ToUpper(parts[p][0]) + parts[p].Substring(1);
                }
                string capId = string.Join("_", parts);

                GameObject vfx = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/VFX/VFX_{capId}.prefab");
                if (vfx == null && id == "mage_fire_orb")
                {
                    vfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/FireOrb_Projectile.prefab");
                }

                if (vfx != null)
                {
                    if (type == SkillTargetType.Projectile) skill.projectilePrefab = vfx;
                    else skill.hitVfxPrefab = vfx;
                }
                else
                {
                    if (type == SkillTargetType.Projectile)
                        skill.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/Arrow_Projectile.prefab");
                    else if (type == SkillTargetType.DirectMelee)
                        skill.hitVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/SlashArc_VFX.prefab");
                }

                EditorUtility.SetDirty(skill);
                dict[id] = skill;
            }

            // === NOVICE (5 Kỹ Năng Tân Thủ Cấp 1 Chuẩn) ===
            CreateSkill("novice_fan_slash", "Đoạn Phong Trảm", "Chém hình quạt 90 độ cự ly rộng gây 140% sát thương.", 10f, 3.0f, 1.4f, SkillTargetType.DirectMelee, ElementType.Wind);
            CreateSkill("novice_flurry_slash", "Loạn Vũ Liên Trảm", "Kích hoạt chém liên tục 6 nhịp trong 2 giây.", 25f, 7.0f, 2.7f, SkillTargetType.DirectMelee, ElementType.None);
            CreateSkill("novice_aegis_barrier", "Kim Cang Giáp", "Tăng 15 điểm giáp và tạo 50 Máu Ảo hấp thụ sát thương trong 5s.", 20f, 14.0f, 0f, SkillTargetType.SelfBuff, ElementType.Earth);
            CreateSkill("novice_quad_dash", "Tứ Bộ Du Long", "Lướt tới 4 đoạn ngắn liên tiếp né đòn với tốc độ cao.", 15f, 10.0f, 0f, SkillTargetType.SelfBuff, ElementType.Wind);
            CreateSkill("novice_holy_heal", "Ánh Sáng Phục Hồi", "Kêu gọi thánh quang phục hồi tức thì 60 HP.", 35f, 22.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Angel);

            // === 1. ĐẤU SĨ / CHIẾN BINH (WARRIOR / BRAWLER) - 5 KỸ NĂNG CHUẨN Q, W, E, R, T ===
            CreateSkill("warrior_cleave", "Trảm Kích (Cleave)", "Vung vũ khí theo hình bán nguyệt phía trước, gây sát thương vật lý lên toàn bộ mục tiêu trúng đòn.", 12f, 2.5f, 1.8f, SkillTargetType.DirectMelee, ElementType.None);
            CreateSkill("warrior_vanguard_charge", "Xung Phong (Vanguard Charge)", "Lao thẳng về hướng chỉ định một đoạn ngắn; nếu chạm kẻ địch đầu tiên sẽ gây choáng nhẹ (0.75s) và đẩy lùi nhẹ mục tiêu.", 16f, 4.5f, 1.5f, SkillTargetType.DirectMelee, ElementType.None);
            CreateSkill("warrior_iron_bastion", "Thiết Giáp (Iron Bastion)", "Đưa vũ khí/khiên lên đỡ, tạo một lớp giáp ảo hấp thụ sát thương trong 3 giây và giảm 20% sát thương nhận vào trong thời gian này.", 18f, 7.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Earth);
            CreateSkill("warrior_tremor_slam", "Địa Chấn (Tremor Slam)", "Đập mạnh vũ khí xuống mặt đất, tạo vết nứt hình nón gây sát thương vật lý và làm chậm 40% tốc độ di chuyển của đối thủ trong 2 giây.", 22f, 6.0f, 2.2f, SkillTargetType.AreaOfEffect, ElementType.Earth);
            CreateSkill("warrior_frenzied_strike", "Cuồng Nộ Kích (Frenzied Strike)", "Tích lực trong thoáng chốc rồi giáng một đòn chí mạng đơn mục tiêu; lượng sát thương tăng thêm dựa trên lượng máu đã mất của bản thân.", 26f, 9.0f, 3.2f, SkillTargetType.DirectMelee, ElementType.Fire);

            // === 2. PHÁP SƯ (MAGE / ELEMENTALIST) - 5 KỸ NĂNG CHUẨN Q, W, E, R, T ===
            CreateSkill("mage_fire_orb", "Hỏa Cầu (Fire Orb)", "Phóng một quả cầu lửa theo đường thẳng; phát nổ khi chạm mục tiêu đầu tiên, gây sát thương phép diện rộng xung quanh.", 15f, 2.5f, 2.0f, SkillTargetType.Projectile, ElementType.Fire);
            CreateSkill("mage_frost_spikes", "Băng Phong Thược (Frost Spikes)", "Triệu hồi gai băng nhô lên tại vùng chỉ định sau 0.5 giây trễ, gây sát thương phép và trói chân kẻ địch trúng phải trong 1.5 giây.", 20f, 4.5f, 2.2f, SkillTargetType.AreaOfEffect, ElementType.Water);
            CreateSkill("mage_blink", "Thiểm Dịch (Blink / Arcane Shift)", "Biến ảnh tức thời về hướng con trỏ chuột một khoảng ngắn, đồng thời để lại ảo ảnh gây chậm nhẹ kẻ địch xung quanh vị trí cũ.", 16f, 5.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Wind);
            CreateSkill("mage_chain_lightning", "Lôi Điện Tỏa (Chain Lightning)", "Phóng một tia sét giật vào mục tiêu chỉ định, sau đó nảy sang tối đa 3 kẻ địch lân cận, sát thương giảm dần 15% sau mỗi lần nảy.", 26f, 7.5f, 2.5f, SkillTargetType.AreaOfEffect, ElementType.Electric);
            CreateSkill("mage_mana_shield", "Nguyên Tố Hộ Thể (Mana Shield)", "Kích hoạt tấm chắn ma pháp hấp thụ sát thương chuyển hóa thành năng lượng (Mana) tương đương 30% giá trị sát thương nhận vào.", 22f, 10.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.None);

            // === 3. XẠ THỦ (ARCHER / MARKSMAN) - 5 KỸ NĂNG CHUẨN Q, W, E, R, T ===
            CreateSkill("archer_piercing_shot", "Xuyên Vân Tiễn (Piercing Shot)", "Kéo căng cung/súng bắn một phát đạn bay xuyên qua mọi kẻ địch trên một đường thẳng, gây sát thương vật lý (sát thương giảm nhẹ qua mỗi mục tiêu).", 14f, 2.8f, 2.0f, SkillTargetType.Projectile, ElementType.Wind);
            CreateSkill("archer_volley", "Mưa Tên (Volley / Barrage)", "Bắn một chùm tên/đạn hình nón về phía trước, gây sát thương và làm chậm đối thủ 30% trong 1.5 giây để giữ cự ly.", 18f, 4.5f, 1.6f, SkillTargetType.Projectile, ElementType.Wind);
            CreateSkill("archer_tumble", "Nhào Lộn Thoát Thân (Tumble)", "Lộn nhanh về hướng chỉ định, đồng thời cường hóa tầm đánh và tốc độ của đòn đánh thường tiếp theo trong vòng 3 giây.", 12f, 3.5f, 1.0f, SkillTargetType.SelfBuff, ElementType.Wind);
            CreateSkill("archer_snare_trap", "Bẫy Dây (Snare Trap)", "Đặt một bẫy tàng hình tại vị trí chỉ định; khi kẻ địch giẫm phải sẽ kích nổ gây sát thương, làm lộ diện và trói chân mục tiêu trong 1.25 giây.", 20f, 6.5f, 1.8f, SkillTargetType.AreaOfEffect, ElementType.Earth);
            CreateSkill("archer_deadeye", "Ngắm Chuẩn Dứt Điểm (Deadeye)", "Đứng yên khóa mục tiêu đơn lẻ trong 1 giây để bắn phát bắn uy lực cực đại, sát thương gia tăng tỉ lệ thuận với lượng máu đã mất của mục tiêu.", 32f, 10.0f, 3.8f, SkillTargetType.Projectile, ElementType.None);

            // === CÁC KỸ NĂNG TIẾP DIỄN KHÁC (BACKWARD COMPATIBILITY) ===
            CreateSkill("warrior_shield_bash", "Đập Khiên Choáng", "Đập mạnh khiên gây choáng và sát thương.", 15f, 4.0f, 1.5f, SkillTargetType.DirectMelee, ElementType.None);
            CreateSkill("warrior_iron_skin", "Kim Cang Hộ Thể", "Tăng 50% phòng ngự và kháng đòn trong 5s.", 18f, 10.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.None);
            CreateSkill("ranger_triple_shot", "Tam Tiễn Tề Phát", "Bắn liền 3 mũi tên liên tiếp về phía trước.", 14f, 2.5f, 1.6f, SkillTargetType.Projectile, ElementType.None);
            CreateSkill("ranger_evasive_shot", "Bắn Giật Lùi", "Bắn một phát tên đẩy lùi kẻ địch đồng thời lùi lại.", 12f, 3.5f, 1.4f, SkillTargetType.Projectile, ElementType.Wind);
            CreateSkill("ranger_hawk_eye", "Ưng Nhãn Thần Tiễn", "Ngắm bắn phát tên chuẩn xác tầm cực xa.", 20f, 8.0f, 2.6f, SkillTargetType.Projectile, ElementType.None);
            CreateSkill("mage_arcane_blast", "Bí Thuật Pháo", "Bắn quả cầu ma pháp gây sát thương phép.", 14f, 2.0f, 1.9f, SkillTargetType.Projectile, ElementType.Water);
            CreateSkill("mage_frost_nova", "Băng Bộc Diện Rộng", "Nổ băng đóng băng và làm chậm toàn bộ quái quanh thân.", 20f, 4.0f, 1.6f, SkillTargetType.AreaOfEffect, ElementType.Water);

            // === FREE MELEE ===
            CreateSkill("melee_heavy_slam", "Địa Chấn Nện", "Đập đất tạo sóng chấn động đẩy lùi quái.", 18f, 3.0f, 2.2f, SkillTargetType.AreaOfEffect, ElementType.Earth);
            CreateSkill("melee_armor_break", "Búa Tạ Phá Giáp", "Bổ mạnh phá vỡ giáp phòng thủ của kẻ địch.", 22f, 4.5f, 2.5f, SkillTargetType.DirectMelee, ElementType.None);
            CreateSkill("melee_ultimate_crush", "Khai Sơn Bạt Địa (Tuyệt Kỹ)", "Tuyệt kỹ Pet biến hình giáng đòn hủy diệt.", 40f, 10.0f, 4.2f, SkillTargetType.AreaOfEffect, ElementType.Earth);

            // === ELEMENTAL ARCHER ===
            CreateSkill("arch_fire_arrow", "Hỏa Bộc Tiễn", "Bắn tên lửa thiêu đốt và nổ lan 2.5m.", 15f, 2.0f, 2.0f, SkillTargetType.Projectile, ElementType.Fire);
            CreateSkill("arch_water_arrow", "Băng Hàn Tỏa Tức", "Bắn tên băng làm chậm 35% tốc độ của mục tiêu.", 15f, 2.0f, 1.8f, SkillTargetType.Projectile, ElementType.Water);
            CreateSkill("arch_wind_arrow", "Cuồng Phong Xuyên Phá", "Mũi tên xé gió xuyên thấu và đẩy lùi mục tiêu.", 18f, 3.0f, 2.2f, SkillTargetType.Projectile, ElementType.Wind);
            CreateSkill("arch_terra_arrow", "Cự Thạch Bạo Kích", "Mũi tên đất bỏ qua 40% giáp của kẻ địch.", 22f, 4.0f, 2.6f, SkillTargetType.Projectile, ElementType.Earth);
            CreateSkill("arch_elemental_storm", "Bão Tên Nguyên Tố", "Mưa tên đa nguyên tố càn quét diện rộng.", 45f, 10.0f, 3.8f, SkillTargetType.AreaOfEffect, ElementType.Fire);

            // === SPIRIT ARCHER ===
            CreateSkill("spirit_summon_single", "Triệu Hồi Linh Thể", "Tiêu 1 Charge gọi Linh Thể tự truy đuổi 9s.", 12f, 1.5f, 1.7f, SkillTargetType.Projectile, ElementType.Wind);
            CreateSkill("spirit_fusion", "Hợp Thể Thần Linh (Fusion x3)", "Tiêu 3 Charge tạo Linh Thể khổng lồ x3 dmg.", 30f, 6.0f, 4.5f, SkillTargetType.Projectile, ElementType.Wind);
            CreateSkill("spirit_soul_recall", "Thu Hồi Tinh Phách", "Thu hồi mọi Linh Thể để sạc lại Charge.", 10f, 4.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.None);
            CreateSkill("spirit_radiant_volley", "Vạn Tiễn Linh Thể", "Phóng loạt đạn linh thể bay vòng quanh.", 38f, 9.0f, 3.4f, SkillTargetType.AreaOfEffect, ElementType.Angel);

            // === ENGINEER GUNNER ===
            CreateSkill("eng_heavy_shot", "Phát Bắn Nén Nhiệt", "Bắn phát đạn nhiệt cực mạnh tăng Heat.", 14f, 1.5f, 1.9f, SkillTargetType.Projectile, ElementType.Fire);
            CreateSkill("eng_deploy_gatling", "Dựng Trụ Liên Thanh", "Triển khai trụ súng máy xả đạn vật lý liên tục.", 20f, 5.0f, 1.5f, SkillTargetType.AreaOfEffect, ElementType.None);
            CreateSkill("eng_deploy_laser", "Dựng Trụ Laser Xuyên Phá", "Triển khai trụ laser bắn tia xuyên giáp.", 25f, 7.0f, 2.4f, SkillTargetType.Projectile, ElementType.Fire);
            CreateSkill("eng_deploy_emp", "Dựng Trụ Điện Từ EMP", "Triển khai trụ EMP phóng xung điện làm chậm và choáng.", 25f, 8.0f, 1.6f, SkillTargetType.AreaOfEffect, ElementType.Electric);
            CreateSkill("eng_heat_purge", "Bung Van Xả Áp Suất", "Xả luồng khí nóng đẩy lùi quái 3m và hạ nhiệt.", 10f, 4.0f, 2.0f, SkillTargetType.AreaOfEffect, ElementType.Fire);

            // === SHADOW GUNNER ===
            CreateSkill("shadow_stealth", "Ẩn Nấp Bóng Tối (Stealth)", "Tàng hình trong 6s, kẻ địch mất dấu.", 20f, 14.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Poison);
            CreateSkill("shadow_ambush_shot", "Phát Bắn Phục Kích (Ambush)", "Phát bắn phá tàng hình chí mạng x3 sát thương.", 25f, 4.0f, 3.2f, SkillTargetType.Projectile, ElementType.None);
            CreateSkill("shadow_backstab_barrage", "Mưa Đạn Sau Lưng", "Xả đạn góc sau lưng mục tiêu +35% sát thương.", 28f, 6.0f, 2.8f, SkillTargetType.Projectile, ElementType.Poison);
            CreateSkill("shadow_smoke_screen", "Màn Khói Vong Hồn", "Thả khói mù giảm độ chính xác và làm chậm quái.", 18f, 8.0f, 1.4f, SkillTargetType.AreaOfEffect, ElementType.Poison);

            // === MAGE ===
            CreateSkill("mage_arcane_blast", "Bí Thuật Pháo", "Bắn quả cầu ma pháp gây sát thương phép.", 14f, 2.0f, 1.9f, SkillTargetType.Projectile, ElementType.Water);
            CreateSkill("mage_frost_nova", "Băng Bộc Diện Rộng", "Nổ băng đóng băng và làm chậm toàn bộ quái quanh thân.", 20f, 4.0f, 1.6f, SkillTargetType.AreaOfEffect, ElementType.Water);
            CreateSkill("mage_mana_shield", "Khiên Hộ Thuật", "Tạo lá chắn năng lượng hấp thụ sát thương.", 22f, 10.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.None);

            // === ELEMENTAL ARCHMAGE ===
            CreateSkill("elem_fire_orb", "Hỏa Diễm Tinh Thể (Nạp Hỏa)", "Nạp nguyên tố Hỏa vào hàng đợi 2 ô.", 12f, 1.0f, 1.6f, SkillTargetType.Projectile, ElementType.Fire);
            CreateSkill("elem_water_pulse", "Băng Lãnh Thủy Ba (Nạp Thủy)", "Nạp nguyên tố Thủy vào hàng đợi 2 ô.", 12f, 1.0f, 1.6f, SkillTargetType.Projectile, ElementType.Water);
            CreateSkill("elem_wind_gust", "Phong Lôi Toàn Phong (Nạp Phong)", "Nạp nguyên tố Phong vào hàng đợi 2 ô.", 12f, 1.0f, 1.6f, SkillTargetType.Projectile, ElementType.Wind);
            CreateSkill("elem_shift", "Chuyển Đổi Tinh Đẩu (Shift)", "Đảo vị trí hoặc nhân bản nguyên tố trong hàng đợi.", 8f, 2.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.None);
            CreateSkill("elem_cataclysm", "Thiên Tai Diệt Thế (Cataclysm)", "Thiên Tai bùng nổ diện rộng sau 3 combo hợp thành.", 50f, 12.0f, 5.0f, SkillTargetType.AreaOfEffect, ElementType.Fire);

            // === SUMMONER ===
            CreateSkill("sum_pet1_strike", "Lệnh: Mãnh Trảo Pet 1", "Chỉ định Pet 1 lao tới cào xé mục tiêu.", 14f, 2.5f, 1.8f, SkillTargetType.DirectMelee, ElementType.None);
            CreateSkill("sum_pet1_skill", "Linh Thuẫn Hộ Mệnh Pet 1", "Pet 1 tạo khiên chắn bảo hộ chủ nhân.", 20f, 6.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Earth);
            CreateSkill("sum_pet2_strike", "Lệnh: Lôi Kích Pet 2", "Chỉ định Pet 2 phóng luồng sét nguyên tố.", 16f, 3.0f, 2.0f, SkillTargetType.Projectile, ElementType.Electric);
            CreateSkill("sum_dual_resonance", "Cộng Hưởng Lưỡng Thú", "Kích nổ phản ứng nguyên tố hiệp lực từ cả 2 Pet.", 30f, 8.0f, 3.2f, SkillTargetType.AreaOfEffect, ElementType.Earth);

            // === CHRONO MAGE ===
            CreateSkill("chrono_bolt", "Tia Thời Gian", "Bắn tia năng lượng thời gian làm chậm kẻ địch.", 12f, 1.8f, 1.7f, SkillTargetType.Projectile, ElementType.Electric);
            CreateSkill("chrono_slow_field", "Trường Giãn Thời Gian", "Tạo vùng làm chậm 50% tốc độ di chuyển và ra chiêu.", 20f, 6.0f, 1.4f, SkillTargetType.AreaOfEffect, ElementType.Electric);
            CreateSkill("chrono_stasis", "Ngưng Đọng Thời Không (Stasis)", "Khóa cứng mục tiêu và nén sát thương gánh chịu.", 25f, 8.0f, 1.5f, SkillTargetType.Projectile, ElementType.Electric);
            CreateSkill("chrono_rewind", "Nghịch Chuyển Không Gian (Reverse)", "Lùi vị trí nhân vật về tọa độ 3 giây trước đó.", 15f, 10.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.None);
            CreateSkill("chrono_rupture", "Bộc Nổ Tuyến Tính", "Xả nổ toàn bộ sát thương nén lên mục tiêu.", 35f, 9.0f, 3.5f, SkillTargetType.AreaOfEffect, ElementType.Electric);

            // === HOLY/DARK PRIEST ===
            CreateSkill("priest_light_bolt", "Thánh Kích / Ma Kích", "Bắn tia sáng hồi phục/gây sát thương theo Stance.", 12f, 2.0f, 1.8f, SkillTargetType.Projectile, ElementType.Angel);
            CreateSkill("priest_bless_curse", "Cầu Phúc / Nguyền Ấn", "Hồi máu theo nhịp hoặc nguyền rủa giảm thủ của quái.", 18f, 5.0f, 1.2f, SkillTargetType.AreaOfEffect, ElementType.Angel);
            CreateSkill("priest_shield_mark", "Quang Thuẫn / Hắc Thuẫn", "Lá chắn ánh sáng bảo vệ hoặc khắc ấn tăng sát thương nhận vào.", 20f, 7.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Angel);
            CreateSkill("priest_stance_toggle", "Hoán Đổi Thánh / Ma (Phím X)", "Chuyển đổi Stance Thiên Thần ↔ Ác Quỷ.", 5f, 1.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.None);
            CreateSkill("priest_holy_zone", "Thánh Vực / Ma Vực", "Vùng hào quang thanh tẩy trị liệu hoặc đày đọa kẻ thù.", 35f, 10.0f, 3.0f, SkillTargetType.AreaOfEffect, ElementType.Angel);

            // === ALCHEMIST ===
            CreateSkill("alch_throw_flask", "Ném Bình Hỗn Hợp", "Ném bình độc gây sát thương duy trì diện rộng.", 14f, 2.0f, 1.7f, SkillTargetType.Projectile, ElementType.Poison);
            CreateSkill("alch_place_cauldron", "Đặt Chum Ma Pháp (Brewing)", "Đặt chum xuống sàn để ủ nguyên liệu.", 25f, 6.0f, 1.0f, SkillTargetType.AreaOfEffect, ElementType.Poison);
            CreateSkill("alch_catalyst_ignite", "Kích Nổ Dược Lý", "Kích phát phản ứng trong chum nổ tung gây sát thương lớn.", 20f, 4.0f, 2.6f, SkillTargetType.AreaOfEffect, ElementType.Poison);
            CreateSkill("alch_healing_mist", "Sương Mù Trị Liệu", "Thả khói dược liệu hồi máu liên tục cho đồng minh.", 25f, 8.0f, 1.2f, SkillTargetType.SelfBuff, ElementType.Water);
            CreateSkill("alch_pure_elixir", "Tiên Dược Hoàn Nguyên", "Thuốc đại bổ tăng toàn bộ chỉ số và hoàn trả nguyên liệu.", 40f, 12.0f, 3.6f, SkillTargetType.AreaOfEffect, ElementType.None);

            return dict;
        }

        private static Dictionary<string, CharacterClassData> GenerateAllClasses(Dictionary<string, SkillData> skills)
        {
            var dict = new Dictionary<string, CharacterClassData>();

            CharacterClassData SetupClass(
                string assetName, string name, ClassBranch branch, ClassRole role, int tier, int reqLvl,
                SpecialMechanic mech, ElementType elem, int diff, float hp, float mp, float atk, float def, float spd, float crit,
                string desc, string mechDesc,
                string s1Name, string s1Desc, string s2Name, string s2Desc, string s3Name, string s3Desc,
                params string[] skillIds)
            {
                string path = $"{ClassesDir}/{assetName}.asset";
                CharacterClassData c = AssetDatabase.LoadAssetAtPath<CharacterClassData>(path);
                if (c == null)
                {
                    c = ScriptableObject.CreateInstance<CharacterClassData>();
                    AssetDatabase.CreateAsset(c, path);
                }

                c.className = name;
                c.branch = branch;
                c.role = role;
                c.tier = tier;
                c.requiredLevelForPromotion = reqLvl;
                c.specialMechanic = mech;
                c.defaultElement = elem;
                c.difficulty = diff;

                c.baseHp = hp;
                c.baseMp = mp;
                c.baseAtk = atk;
                c.baseDef = def;
                c.baseSpeed = spd;
                c.baseCritRate = crit;

                c.description = desc;
                c.mechanicDescription = mechDesc;

                c.passiveSeal1Name = s1Name;
                c.passiveSeal1Desc = s1Desc;
                c.passiveSeal2Name = s2Name;
                c.passiveSeal2Desc = s2Desc;
                c.passiveSeal3Name = s3Name;
                c.passiveSeal3Desc = s3Desc;

                c.unlockedSkills.Clear();
                foreach (var sid in skillIds)
                {
                    if (skills.TryGetValue(sid, out var s))
                    {
                        c.unlockedSkills.Add(s);
                    }
                }

                EditorUtility.SetDirty(c);
                dict[assetName] = c;
                return c;
            }

            // 1. Tân Thủ (5 Kỹ Năng)
            SetupClass("Class_Novice", "Tân Thủ (Novice)", ClassBranch.Novice, ClassRole.DPS, 0, 10,
                SpecialMechanic.None, ElementType.None, 1, 100, 50, 15, 5, 4.5f, 0.05f,
                "Lớp nhân vật khởi đầu cho mọi hiệp sĩ Lumina.", "Làm quen với hệ thống di chuyển, chiến đấu cơ bản và tương tác thế giới.",
                "Tập Sự Cương Thiền", "Giảm 10% sát thương nhận vào khi HP tụt xuống dưới 50%.",
                "Khát Khao Tiến Hóa", "Tăng +20% lượng EXP nhận được khi tiêu diệt quái vật dã ngoại.",
                "Ý Chí Bất Diệt", "Tự động hồi phục 2% HP tối đa mỗi 3 giây khi ở ngoài giao tranh.",
                "novice_fan_slash", "novice_flurry_slash", "novice_aegis_barrier", "novice_quad_dash", "novice_holy_heal");

            // 2. Chiến Binh (Đấu Sĩ / Brawler - 6 Kỹ Năng 1..6)
            SetupClass("Class_Warrior", "Đấu Sĩ (Warrior / Brawler)", ClassBranch.Warrior, ClassRole.Tank, 1, 30,
                SpecialMechanic.None, ElementType.None, 2, 140, 40, 20, 10, 4.5f, 0.06f,
                "Bậc thầy cận chiến kiểm soát nhịp độ, gia tăng áp lực và chống chịu khi đối đầu.", "Cận chiến, khống chế, tạo giáp ảo và giáng đòn dứt điểm bạo kích khi thấp máu.",
                "Chiến Thể Bất Khuất", "Giáp phòng thủ tăng thêm +25% khi lượng máu dưới 40%.",
                "Nộ Khí Xung Thiên", "Mỗi đòn đánh trúng tích 5% tốc độ tấn công (tối đa 25%).",
                "Phản Chấn Thần Lực", "Khi chủ động giơ khiên chặn đòn thành công, phản lại 30% sát thương cho kẻ tấn công.",
                "warrior_cleave", "warrior_vanguard_charge", "warrior_iron_bastion", "warrior_tremor_slam", "warrior_frenzied_strike", "melee_ultimate_crush");

            // 3. Cận Chiến Tự Do
            SetupClass("Class_FreeMelee", "Cận Chiến Tự Do (Free Melee)", ClassBranch.Warrior, ClassRole.SustainDPS, 2, 50,
                SpecialMechanic.PetWeapon, ElementType.None, 4, 160, 50, 26, 9, 5.0f, 0.08f,
                "Lớp cận chiến độc nhất vô nhị. Pet biến hình thành 11 loại vũ khí theo 4 nhóm loài.", "Đổi Pet biến đổi vũ khí tức thì (CD 5s), phủ ngấn nguyên tố Pet lên toàn bộ chiêu thức.",
                "Binh Khí Đồng Thể (Weapon Metamorphosis)", "Khi bấm đổi Pet biến vũ khí (CD 5s), đòn đánh hoặc kỹ năng đầu tiên trong 3s tăng +25% sát thương.",
                "Phủ Ngấn Nguyên Tố (Elemental Edge)", "Toàn bộ chiêu thức vũ khí tự chuyển hóa thành hệ nguyên tố của Pet, áp hiệu ứng lõi và hưởng hệ số x2.0 khắc chế.",
                "Kế Thừa Linh Tính (Familiar Synergy)", "Kế thừa 100% chỉ số từ Hidden Passive của Pet. Pet phẩm chất cao mở khóa hiệu ứng nổ uy lực cho Tuyệt Kỹ (Slot 5).",
                "melee_heavy_slam", "melee_armor_break", "melee_ultimate_crush");

            // 4. Xạ Thủ (Archer / Marksman - 6 Kỹ Năng 1..6)
            SetupClass("Class_Archer", "Xạ Thủ (Archer / Marksman)", ClassBranch.Archer, ClassRole.DPS, 1, 30,
                SpecialMechanic.None, ElementType.None, 2, 95, 60, 22, 4, 5.0f, 0.10f,
                "Thiện xạ tầm xa duy trì khoảng cách (kiting), sát thương vật lý liên tục và dứt điểm uy lực.", "Tầm xa cơ động, rải mưa tên giữ cự ly, bẫy trói chân và ngắm bắn tử thần.",
                "Ưng Nhãn Chuẩn Xác", "Tầm bắn tăng thêm 25%, nhận +10% bạo kích khi khoảng cách tới mục tiêu > 4m.",
                "Bộ Pháp Khinh Linh", "Tăng +15% tốc độ di chuyển trong 2 giây sau mỗi lần lướt né đòn (Dash).",
                "Mũi Tên Xuyên Thấu", "Phát bắn thường có 30% xác suất xuyên qua mục tiêu đầu tiên trúng kẻ địch phía sau.",
                "archer_piercing_shot", "archer_volley", "archer_tumble", "archer_snare_trap", "archer_deadeye", "arch_elemental_storm");

            // 5. Xạ Thủ Nguyên Tố
            SetupClass("Class_ElementalArcher", "Xạ Thủ Nguyên Tố (Elemental Archer)", ClassBranch.Archer, ClassRole.DPS, 2, 50,
                SpecialMechanic.ElementalQuiver, ElementType.Fire, 3, 110, 80, 28, 5, 4.6f, 0.12f,
                "Cung thủ thuần thục 4 loại mũi tên nguyên tố: Hỏa, Thủy, Phong, Thổ.", "Cố định tốc bắn 1.0/s. Chuyển đổi toàn bộ 1% AtkSpd -> 1.5% Sát thương nguyên tố. Chuỗi 3 phát cùng hệ phát 4 chắc chắn bạo kích.",
                "Chuẩn Xác Tuyệt Đối", "Khóa tốc bắn 1.0/s, đổi tốc đánh sang % sát thương nguyên tố; bắn 3 phát cùng hệ thì phát thứ 4 chắc chắn bạo kích.",
                "Khắc Chế Điểm Yếu", "Bắn trúng hệ bị khắc chế tăng x2.0 sát thương và nhân đôi thời gian hiệu lực khống chế.",
                "Dẫn Tiễn Hiệp Đồng", "Mũi tên gắn ấn; đòn đánh Pet vào mục tiêu mang ấn tự kích nổ phản ứng nguyên tố dây chuyền.",
                "arch_fire_arrow", "arch_water_arrow", "arch_wind_arrow", "arch_terra_arrow", "arch_elemental_storm");

            // 6. Xạ Thủ Tinh Linh
            SetupClass("Class_SpiritArcher", "Xạ Thủ Tinh Linh (Spirit Archer)", ClassBranch.Archer, ClassRole.BurstDPS, 2, 50,
                SpecialMechanic.SpiritCharge, ElementType.Wind, 3, 105, 85, 30, 4, 5.2f, 0.14f,
                "Khai thác sức mạnh Tinh Linh từ Pet đồng hành để phóng đạn truy đuổi.", "Quản lý 3 Charge Linh Thể (hồi 4s/charge). Bắn thường tự tìm quái. Giữ nút kích hoạt Hợp Thể Fusion x3 sát thương.",
                "Tuần Hoàn Tinh Phách", "Quái chết hồi 50% thời gian nạp Charge; Kích hoạt Fusion tăng 20% MoveSpeed và miễn nhiễm khống chế trong 5s.",
                "Đồng Điệu Linh Tố", "Tăng +25% hiệu số hiệu ứng hệ nguyên tố; đòn đánh Linh Thể tự kích hoạt phản ứng nguyên tố.",
                "Kế Thừa Linh Thú", "Thừa hưởng 100% Hidden Passive của Pet; đổi Pet trong combat không làm mất hệ của các Linh Thể đang bay.",
                "spirit_summon_single", "spirit_fusion", "spirit_soul_recall", "spirit_radiant_volley");

            // 7. Cơ Khí Sư
            SetupClass("Class_EngineerGunner", "Cơ Khí Sư (Engineer Gunner)", ClassBranch.Archer, ClassRole.Tactician, 2, 50,
                SpecialMechanic.EngineerTurrets, ElementType.Electric, 4, 125, 70, 27, 8, 4.5f, 0.08f,
                "Súng đại bác nhiệt lượng và công nghệ triển khai tối đa 3 trụ súng tự động.", "Quản lý thanh Nhiệt Lượng (Heat). Vùng nhiệt cao +20% dmg. Overheat kẹt súng 3.5s bung van đẩy lùi 3m. Pet bảo trì di động.",
                "Xung Áp Quá Nhiệt", "Vùng nhiệt cao (70-99%) tăng +20% sát thương; Overheat bung van khí nóng đẩy lùi 3m và nhận khiên 15% HP.",
                "Trạm Cơ Động Hiệp Đồng", "Pet tự động di chuyển sửa chữa trụ thấp máu nhất và tăng +25% tốc độ bắn cho trụ.",
                "Thợ Rèn Chế Tác", "Rèn súng/giáp theo 3 cấp chất lượng, tháo rã phế liệu lấy linh kiện, tăng 20% máu tối đa cho trụ.",
                "eng_heavy_shot", "eng_deploy_gatling", "eng_deploy_laser", "eng_deploy_emp", "eng_heat_purge");

            // 8. Sát Thủ Bóng Đêm
            SetupClass("Class_ShadowGunner", "Sát Thủ Bóng Đêm (Shadow Gunner)", ClassBranch.Archer, ClassRole.BurstDPS, 2, 50,
                SpecialMechanic.StealthCritical, ElementType.Poison, 4, 100, 75, 34, 4, 5.4f, 0.20f,
                "Sát thủ chuyên phục kích từ bóng tối kết hợp hỏa lực súng lục giảm thanh.", "Tàng hình chủ động 6s (Stealth). Đòn mở đầu bạo kích x200-300%. Bắn sau lưng +35% dmg. Pet tạo Decoy khiêu khích.",
                "Phục Kích Tử Thần (Ambush)", "Phát bắn đầu tiên phá tàng hình chắc chắn bạo kích với hệ số x200% - x300% sát thương cực hạn.",
                "Bóng Ma Thế Thân (Decoy)", "Khi tàng hình, Pet tạo phân thân Decoy khiêu khích toàn bộ quái vật quay đầu trong 4s, mở toang góc lưng.",
                "Lưỡi Đao Sau Lưng (Backstab)", "Bắn vào góc 180° sau lưng mục tiêu được tăng trực tiếp +35% sát thương và +25% bạo kích.",
                "shadow_stealth", "shadow_ambush_shot", "shadow_backstab_barrage", "shadow_smoke_screen");

            // 9. Pháp Sư (Mage / Elementalist - 6 Kỹ Năng 1..6)
            SetupClass("Class_Mage", "Pháp Sư (Mage / Elementalist)", ClassBranch.Mage, ClassRole.Controller, 1, 30,
                SpecialMechanic.None, ElementType.Fire, 2, 90, 100, 24, 3, 4.3f, 0.08f,
                "Bậc thầy tầm xa dồn sát thương nguyên tố, cấu rỉa và khống chế khu vực.", "Hỏa cầu bộc phá, cọc băng trói chân, thiểm dịch biến ảo, lôi điện tỏa và khiên mana.",
                "Mạch Chảy Bí Thuật", "Giảm 15% tiêu hao năng lượng Mana của tất cả kỹ năng phép.",
                "Khiên Ma Pháp Tự Động", "Khi nhận sát thương chí mạng, tự kích hoạt khiên hấp thụ 50 sát thương (CD 30s).",
                "Cộng Hưởng Nguyên Tố Cơ Bản", "Tăng 15% sát thương phép thuật lên quái vật đang chịu hiệu ứng bất lợi.",
                "mage_fire_orb", "mage_frost_spikes", "mage_blink", "mage_chain_lightning", "mage_mana_shield", "mage_arcane_blast");

            // 10. Pháp Sư Nguyên Tố
            SetupClass("Class_ElementalMage", "Pháp Sư Nguyên Tố (Elemental Archmage)", ClassBranch.Mage, ClassRole.BurstDPS, 2, 50,
                SpecialMechanic.ElementalQueue, ElementType.Fire, 5, 95, 140, 32, 4, 4.4f, 0.12f,
                "Tối thượng pháp sư điều khiển Hàng Đợi 2 Ô Nguyên Tố và Kỹ Năng Thiên Tai.", "Hàng đợi 2 ô: Ô 1 Chủ Thể + Ô 2 Xúc Tác tạo 64 chiêu thức hợp thành. Tích 3 combo liên tiếp kích hoạt Thiên Tai toàn màn hình.",
                "Lưỡng Nghi Hợp Thành", "Đơn hệ tăng +25% dmg và +25% AoE. Đa hệ kéo dài debuff +1.5s, đánh trúng hệ bị khắc chế tăng x2 dmg và +15% crit rate nguyên tố.",
                "Mạch Chuỗi Thiên Tai", "Bộ đếm combo tích 3 tầng mở Thiên Tai AoE toàn màn hình. Sau Thiên Tai hoàn trả 25% mana chuỗi chiêu và tăng 20% MoveSpeed trong 3s.",
                "Linh Thú Xúc Tác", "Đòn đánh Pet có 25% tỷ lệ tự động nạp hạt nguyên tố vào hàng đợi; Pet đánh quái đang dính ấn tự kích nổ reaction dây chuyền.",
                "elem_fire_orb", "elem_water_pulse", "elem_wind_gust", "elem_shift", "elem_cataclysm");

            // 11. Pháp Sư Triệu Hồi
            SetupClass("Class_Summoner", "Pháp Sư Triệu Hồi (Summoner)", ClassBranch.Mage, ClassRole.Tactician, 2, 50,
                SpecialMechanic.DualPetSummon, ElementType.Earth, 4, 115, 110, 24, 7, 4.5f, 0.06f,
                "Cơ chế Song Hành (Dual Summon): Điều khiển đồng thời 2 Pet ra trận.", "Ánh xạ 6 ô chiêu HUD từ 2 Pet. Quản lý HP Pet, trạng thái Nguy Kịch (Downed) và thay Pet Dự Bị. Cộng Hưởng Lưỡng Thú.",
                "Cộng Hưởng Lưỡng Thú", "Khi 2 Pet cùng tấn công 1 mục tiêu trong 3s, tự kích nổ Elemental Reaction và tăng +20% sát thương phản ứng.",
                "Phân Bổ Chiến Thuật", "Pet cận chiến giảm -15% sát thương gánh chịu; Pet tầm xa tăng +15% tầm đánh và tốc độ chiêu thức.",
                "Tiếp Ứng Khẩn Cấp", "Đổi Pet nguy kịch tạo khiên 20% HP và tăng 15% MoveSpeed trong 4s; sóng xung kích đẩy lùi 3m mang hệ Pet mới; giảm 20% thời gian dưỡng thương.",
                "sum_pet1_strike", "sum_pet1_skill", "sum_pet2_strike", "sum_dual_resonance");

            // 12. Pháp Sư Thời Không
            SetupClass("Class_ChronoMage", "Pháp Sư Thời Không (Chrono Mage)", ClassBranch.Mage, ClassRole.Controller, 2, 50,
                SpecialMechanic.TimeControl, ElementType.Electric, 5, 100, 130, 29, 5, 4.7f, 0.10f,
                "Thao túng không-thời gian: Làm chậm, Ngưng đọng nén sát thương, Nghịch chuyển quá khứ.", "Slow giảm tốc. Stasis đóng băng nén sát thương bùng nổ khi kết thúc. Reverse đưa vị trí lùi về 3 giây trước.",
                "Sụp Đổ Tuyến Tính (Temporal Collapse)", "Mục tiêu chịu Slow/Stasis tích tụ Dấu Ấn; khi kết thúc bùng nổ thêm 15% tổng sát thương đã gánh chịu.",
                "Nghịch Đảo Nhân Quả (Causality Reversal)", "Khi kích hoạt Reverse, nhận Lệch Pha Không Gian trong 3s (+20% MoveSpeed, hoàn lại 15% mana).",
                "Linh Thú Hiệp Đồng Thời Gian", "Đòn đánh Pet kéo dài Slow thêm +1.5s (Stasis thêm +0.5s) và giảm 0.5s cooldown kỹ năng khống chế của người chơi.",
                "chrono_bolt", "chrono_slow_field", "chrono_stasis", "chrono_rewind", "chrono_rupture");

            // 13. Mục Sư Thánh - Ma
            SetupClass("Class_HolyDarkPriest", "Mục Sư Thánh – Ma (Holy/Dark Priest)", ClassBranch.Mage, ClassRole.Support, 2, 50,
                SpecialMechanic.HolyDarkPriest, ElementType.Angel, 4, 120, 120, 26, 8, 4.6f, 0.08f,
                "Lưỡng cực ánh sáng và bóng tối: Angel Form hồi máu bảo hộ ↔ Demon Form nguyền rủa hút máu.", "Bấm phím [X] chuyển đổi Stance tức thì hoán đổi toàn bộ 5 ô kỹ năng trên HUD giữa 2 bộ loadout riêng biệt.",
                "Song Diện Thần Thánh", "Stance switch giữa Angel Form (Hồi máu, Bảo vệ) và Demon Form (Nguyền rủa, Hút máu), hoán đổi tức thì toàn bộ 5 ô kỹ năng.",
                "Cân Bằng Âm Dương", "Dùng chiêu Angel tăng 15% sát thương cho chiêu Demon kế tiếp; dùng chiêu Demon tăng 25% lượng hồi phục cho chiêu Angel kế tiếp.",
                "Hiệp Đồng Linh Thần", "Pet kế thừa hào quang: Angel tăng giáp party +20%, Demon tăng hút máu +10% sát thương gây ra.",
                "priest_light_bolt", "priest_bless_curse", "priest_shield_mark", "priest_stance_toggle", "priest_holy_zone");

            // 14. Luyện Dược Sư Chum
            SetupClass("Class_Alchemist", "Luyện Dược Sư Chum (Cauldron Alchemist)", ClassBranch.Mage, ClassRole.Support, 2, 50,
                SpecialMechanic.CauldronAlchemy, ElementType.Poison, 4, 115, 115, 25, 7, 4.5f, 0.08f,
                "Kỹ nghệ chế tạo dược phẩm chiến trận bằng Chum Ma Pháp và cơ chế hoàn nguyên.", "Đặt Chum -> ném nguyên liệu -> delay ủ -> kích phát hiệu ứng -> hoàn nguyên 5/10/15%. Bị ngắt chiêu hoàn 100% nguyên liệu.",
                "Quy Hoàn Dược Lý", "Hoàn nguyên 5%/10%/15% nguyên liệu theo level; hoàn 100% nguyên liệu khi bị gián đoạn/khống chế trong lúc đặt Chum.",
                "Xúc Tác Linh Thú", "Đồng hóa Pet Synergy theo hệ (Hỏa +10% Reaction, Thủy +15% Heal, Thảo mộc +10% Yield).",
                "Thang Điểm Tinh Khiết", "Chất lượng Cao tăng +30% hiệu số hồi phục/damage và mở khóa hiệu ứng phụ nguyên tố.",
                "alch_throw_flask", "alch_place_cauldron", "alch_catalyst_ignite", "alch_healing_mist", "alch_pure_elixir");

            return dict;
        }

        private static void LinkPromotionTrees(Dictionary<string, CharacterClassData> classMap)
        {
            void Link(string from, params string[] to)
            {
                if (!classMap.TryGetValue(from, out var source)) return;
                source.promotionOptions.Clear();
                foreach (var t in to)
                {
                    if (classMap.TryGetValue(t, out var target))
                    {
                        source.promotionOptions.Add(target);
                    }
                }
                EditorUtility.SetDirty(source);
            }

            // Novice -> 3 Nhánh cơ bản
            Link("Class_Novice", "Class_Warrior", "Class_Archer", "Class_Mage");

            // Warrior -> Cận Chiến Tự Do
            Link("Class_Warrior", "Class_FreeMelee");

            // Archer -> 4 Chuyên phái
            Link("Class_Archer", "Class_ElementalArcher", "Class_SpiritArcher", "Class_EngineerGunner", "Class_ShadowGunner");

            // Mage -> 5 Chuyên phái
            Link("Class_Mage", "Class_ElementalMage", "Class_Summoner", "Class_ChronoMage", "Class_HolyDarkPriest", "Class_Alchemist");
        }
    }
}
#endif
