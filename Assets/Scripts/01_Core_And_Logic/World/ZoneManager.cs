using System;
using UnityEngine;
using ActionRPG.Core;

namespace ActionRPG.Core.World
{
    public enum GameZone
    {
        Village_Oakhaven = 0,    // Làng Tân Thủ Oakhaven (Khu An Toàn)
        Grassland_Lv1_10 = 1,    // Thảo Nguyên Xanh (Cấp 1 - 10)
        Highway_Road = 2,        // Đại Lộ Liên Vùng
        DeepForest_Lv10_20 = 3,  // Rừng Rậm Hắc Ám (Cấp 10 - 20)
        BossArena = 4            // Sào Huyệt Boss Hắc Diệu Ma Viên
    }

    /// <summary>
    /// Bộ quản lý phân vùng thế giới (World Zone Manager) chuẩn Sprint 11.
    /// Giám sát phân vùng hiện tại của người chơi và phát tín hiệu OnZoneChanged.
    /// </summary>
    public class ZoneManager : MonoBehaviour
    {
        public static ZoneManager Instance { get; private set; }

        [Header("Phân Vùng Hiện Tại")]
        [SerializeField] private GameZone currentZone = GameZone.Village_Oakhaven;
        public GameZone CurrentZone => currentZone;

        public static event Action<GameZone> OnZoneChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Thông báo vùng khởi đầu
            NotifyZone(currentZone, false);
        }

        public void ChangeZone(GameZone newZone)
        {
            if (currentZone == newZone) return;

            currentZone = newZone;
            NotifyZone(newZone, true);
        }

        private void NotifyZone(GameZone zone, bool showPopup)
        {
            OnZoneChanged?.Invoke(zone);

            string displayName = GetZoneDisplayName(zone);
            bool isSafe = IsSafeZone(zone);

            Debug.Log($"<color=yellow>[ZoneManager] Người chơi bước vào: {displayName} (An toàn: {isSafe})</color>");

            if (showPopup)
            {
                string prefix = isSafe ? "🛡️ [KHU AN TOÀN]" : "⚔️ [KHU DÃ NGOẠI]";
                EventBus.OnShowUIMessage?.Invoke($"{prefix} {displayName}");
            }
        }

        public static string GetZoneDisplayName(GameZone zone)
        {
            switch (zone)
            {
                case GameZone.Village_Oakhaven:
                    return "Làng Tân Thủ Oakhaven";
                case GameZone.Grassland_Lv1_10:
                    return "Thảo Nguyên Xanh (Lv 1 - 10)";
                case GameZone.Highway_Road:
                    return "Đại Lộ Mạo Hiểm";
                case GameZone.DeepForest_Lv10_20:
                    return "Rừng Rậm Hắc Ám (Lv 10 - 20)";
                case GameZone.BossArena:
                    return "Sào Huyệt Hắc Diệu Ma Viên";
                default:
                    return zone.ToString();
            }
        }

        public static bool IsSafeZone(GameZone zone)
        {
            return zone == GameZone.Village_Oakhaven;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
