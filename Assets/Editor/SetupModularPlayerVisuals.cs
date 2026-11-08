using UnityEngine;
using UnityEditor;
using ActionRPG.Presentation.Views;

namespace ActionRPG.Editor
{
    public class SetupModularPlayerVisuals
    {
        [MenuItem("Tools/Lumina/Setup Modular Visual Layers")]
        public static void Setup()
        {
            GameObject player = Selection.activeGameObject;
            if (player == null)
            {
                Debug.LogWarning("Vui lòng chọn Player GameObject trên Hierarchy trước khi chạy Setup.");
                return;
            }

            // Đảm bảo có PlayerPresenter và CharacterVisualView
            var presenter = player.GetComponent<ActionRPG.Core.Bridges.PlayerPresenter>();
            var charVisual = player.GetComponent<CharacterVisualView>();

            if (charVisual == null)
            {
                charVisual = player.AddComponent<CharacterVisualView>();
            }

            // Tạo Body Container
            GameObject bodyObj = FindOrCreateChild(player, "Body");
            
            // Xếp lớp từ dưới lên trên
            var bootsRenderer = CreateLayer(bodyObj, "Boots_Layer", 10);
            var armorRenderer = CreateLayer(bodyObj, "Armor_Layer", 11);
            var glovesRenderer = CreateLayer(bodyObj, "Gloves_Layer", 12);
            var helmetRenderer = CreateLayer(bodyObj, "Helmet_Layer", 13);
            
            // Gắn PlayerEquipmentVisualView
            var equipView = player.GetComponent<PlayerEquipmentVisualView>();
            if (equipView == null)
                equipView = player.AddComponent<PlayerEquipmentVisualView>();

            equipView.bootsRenderer = bootsRenderer;
            equipView.armorRenderer = armorRenderer;
            equipView.glovesRenderer = glovesRenderer;
            equipView.helmetRenderer = helmetRenderer;

            // Tạo Weapon Holder
            GameObject weaponHolderObj = FindOrCreateChild(bodyObj, "Weapon_Holder");
            weaponHolderObj.transform.localPosition = Vector3.zero;
            
            // Renderer của Vũ khí
            GameObject weaponRendererObj = FindOrCreateChild(weaponHolderObj, "Weapon_Renderer");
            weaponRendererObj.transform.localPosition = Vector3.zero;
            var weaponRenderer = weaponRendererObj.GetComponent<SpriteRenderer>();
            if (weaponRenderer == null)
            {
                weaponRenderer = weaponRendererObj.AddComponent<SpriteRenderer>();
            }
            weaponRenderer.sortingOrder = 14;

            // Gắn WeaponSocketView
            var weaponSocket = player.GetComponent<WeaponSocketView>();
            if (weaponSocket == null)
                weaponSocket = player.AddComponent<WeaponSocketView>();
            
            weaponSocket.weaponHolder = weaponHolderObj.transform;
            weaponSocket.weaponRenderer = weaponRenderer;

            // Gán vào Presenter
            if (presenter != null)
            {
                SerializedObject so = new SerializedObject(presenter);
                so.Update();
                so.FindProperty("equipmentVisualView").objectReferenceValue = equipView;
                so.FindProperty("weaponSocketView").objectReferenceValue = weaponSocket;
                so.FindProperty("visualView").objectReferenceValue = charVisual;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(player);
            Debug.Log("✅ Cài đặt Modular Visual Layers thành công trên Player!");
        }

        private static GameObject FindOrCreateChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            if (child != null) return child.gameObject;

            GameObject newChild = new GameObject(childName);
            newChild.transform.SetParent(parent.transform);
            newChild.transform.localPosition = Vector3.zero;
            newChild.transform.localRotation = Quaternion.identity;
            newChild.transform.localScale = Vector3.one;
            return newChild;
        }

        private static SpriteRenderer CreateLayer(GameObject parent, string layerName, int order)
        {
            GameObject layerObj = FindOrCreateChild(parent, layerName);
            SpriteRenderer sr = layerObj.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = layerObj.AddComponent<SpriteRenderer>();
            }
            sr.sortingOrder = order;
            return sr;
        }
    }
}
