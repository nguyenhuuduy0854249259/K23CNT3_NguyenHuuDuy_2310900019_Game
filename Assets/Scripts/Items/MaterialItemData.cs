using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Items
{
    public enum MaterialType
    {
        Herb,           // Thảo dược
        Ore,            // Kim loại/Quặng
        MonsterDrop,    // Dịch/Bộ phận quái vật
        Fabric,         // Vải vóc, dây thừng
        Catalyst        // Đá nguyên tố, chất xúc tác
    }

    /// <summary>
    /// Dữ liệu Nguyên liệu chế tạo độc lập.
    /// Dùng cho hệ thống Luyện Dược Sư và Thợ Rèn.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMaterial", menuName = "ActionRPG/Material Item")]
    public class MaterialItemData : ItemData
    {
        [Header("Thông Số Nguyên Liệu")]
        public MaterialType materialType = MaterialType.MonsterDrop;
        public ElementType elementalAffinity = ElementType.None;

        private void OnEnable()
        {
            itemType = ItemType.Material;
            isStackable = true;
            maxStack = 999; // Nguyên liệu có thể stack nhiều hơn (lên tới 999)
        }
    }
}
