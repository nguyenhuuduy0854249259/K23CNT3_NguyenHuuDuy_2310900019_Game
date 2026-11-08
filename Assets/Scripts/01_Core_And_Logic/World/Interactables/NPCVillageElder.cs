using UnityEngine;
using ActionRPG.Core;

namespace ActionRPG.Core.World.Interactables
{
    /// <summary>
    /// NPC Trưởng Làng Tân Thủ: Đứng chào đón người chơi tại cổng làng, hướng dẫn lộ trình di chuyển
    /// theo Đại Lộ Huyết Mạch ra Thảo Nguyên và Rừng Sâu.
    /// </summary>
    public class NPCVillageElder : MonoBehaviour
    {
        public string npcName = "Trưởng Làng Eldrin";
        public float interactDistance = 2.5f;

        [TextArea(2, 4)]
        public string greetingMessage = "Chào dũng sĩ! Hãy men theo Đại Lộ Huyết Mạch để tiến vào Thảo Nguyên Xanh. Coi chừng lũ Chuột Chũi và Đại Cổ Thụ nơi Rừng Sâu!";

        private Transform playerTransform;
        private bool isPlayerNearby = false;

        private void Update()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
                return;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            bool nearby = dist <= interactDistance;

            if (nearby != isPlayerNearby)
            {
                isPlayerNearby = nearby;
                if (isPlayerNearby)
                {
                    EventBus.OnShowUIMessage?.Invoke($"[F] Trò chuyện với {npcName}");
                }
            }

            if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
            {
                EventBus.OnShowUIMessage?.Invoke($"💬 {npcName}: \"{greetingMessage}\"");
                EventBus.OnPlaySound?.Invoke("item");
            }
        }
    }
}
