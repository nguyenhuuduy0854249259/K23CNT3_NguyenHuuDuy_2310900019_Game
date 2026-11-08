# Prompt sửa lỗi cho Antigravity — Dự án Unity RPG (`d:\Unity\prj1\gam`)

> [!IMPORTANT]
> Mỗi lần chạy, dán **Quy tắc chung** cộng với **một** prompt giai đoạn vào một cuộc hội thoại mới. Làm xong giai đoạn nào thì mở Unity, kiểm tra hết lỗi compile và test thử rồi mới chạy giai đoạn tiếp theo.

---

## 0. Quy tắc chung (dán ở đầu MỌI prompt)

```text
Bạn đang sửa lỗi cho dự án Unity 2D RPG tại d:\Unity\prj1\gam (code trong Assets/Scripts, server Node.js trong server/src).

QUY TẮC BẮT BUỘC:
1. XÁC MINH TRƯỚC KHI SỬA: Danh sách lỗi dưới đây lấy từ một bản review tự động, có thể có mục SAI hoặc đã lỗi thời.
   Với từng mục, đọc code thật trước. Nếu lỗi không có trong code thì KHÔNG sửa. Ghi mục đó là "Không đúng thực tế" và nói rõ lý do.
2. Không đổi tên, không xóa public field, public method hay class đang được dùng ở chỗ khác. Trước khi đổi signature,
   grep toàn bộ Assets/Scripts để tìm mọi chỗ gọi và cập nhật hết các chỗ đó.
3. Không đổi tên hay xóa field có [SerializeField] hoặc public field của MonoBehaviour/ScriptableObject, vì làm vậy sẽ mất giá trị
   đã gán trong Inspector. Nếu bắt buộc phải đổi tên thì dùng [FormerlySerializedAs].
4. Không sửa file .unity, .prefab, .asset, .meta.
5. Giữ nguyên comment và docstring hiện có. Comment mới viết bằng tiếng Việt, ngắn gọn.
6. Sửa tối thiểu, đúng phạm vi. Không refactor lớn nếu prompt không yêu cầu.
7. Code phải compile được với phiên bản Unity của project (xem ProjectSettings/ProjectVersion.txt).
   Dùng API đúng với phiên bản đó, ví dụ Rigidbody2D.linearVelocity hay velocity.
8. Khi xong, báo cáo theo bảng: | Mục | Trạng thái (Đã sửa / Không đúng thực tế / Bỏ qua) | File:dòng | Mô tả thay đổi |
   Liệt kê thêm những gì tôi cần test thủ công trong Unity.
```

---

## Giai đoạn 1 — Inventory & Kinh tế (ưu tiên cao nhất)

```text
[Dán Quy tắc chung ở trên]

NHIỆM VỤ: Sửa lỗi Inventory và giao dịch.

1. Assets/Scripts/Items/Inventory.cs — AddItem():
   Lỗi nghi ngờ: khi item stack được lấp đầy slot cũ nhưng hết slot trống cho phần dư, item bị thêm một phần rồi mới return false.
   Cách sửa: tính trước xem có đủ chỗ cho TOÀN BỘ số lượng không. Không đủ thì không thêm gì và return false (thao tác nguyên tử).
   Thêm method public bool CanAddItem(ItemData item, int amount).

2. Inventory.cs — RemoveItem():
   Lỗi nghi ngờ: dùng .Find() nên chỉ kiểm tra slot đầu tiên.
   Cách sửa: tính tổng số lượng trên mọi slot. Đủ thì trừ dần qua nhiều slot và xóa slot rỗng, không đủ thì return false
   và không thay đổi gì. Thêm public int GetItemCount(ItemData item) nếu chưa có.

3. Assets/Scripts/Crafting/BlacksmithSystem.cs:
   a) EnhanceEquipment() sửa trực tiếp field của ScriptableObject EquipmentData (vd: bonusAtk += 4f), làm hỏng asset gốc.
      Cách sửa: không ghi vào SO. Thêm dữ liệu runtime cho cấp cường hóa, ví dụ class [Serializable] EquipmentInstance
      { EquipmentData data; int enhanceLevel; } hoặc Dictionary lưu bonus theo item trong Inventory.
      Sau đó cập nhật CharacterStats để cộng bonus runtime đó. Chọn cách ít xáo trộn nhất và giải thích lý do chọn.
   b) Tiền bị trừ trước khi thêm đồ. Đổi thứ tự thành: kiểm tra đủ tiền → kiểm tra CanAddItem → trừ tiền → thêm item.

4. Assets/Scripts/Town/ShopNPC.cs: áp dụng cùng thứ tự giao dịch như mục 3b.

5. Assets/Scripts/Crafting/AlchemySystem.cs: kiểm tra đủ TẤT CẢ nguyên liệu và đủ chỗ cho thành phẩm trước,
   rồi mới trừ nguyên liệu.

6. Assets/Scripts/Items/ConsumableItemData.cs: field cooldownTime nghi ngờ không được dùng.
   Kiểm tra xem cooldown có đang được xử lý ở nơi khác không (GameplayHUD có HpPotionCooldownTimer/MpPotionCooldownTimer static).
   Nếu chưa có cơ chế nào thì thêm cooldown dùng chung cho từng loại consumable.
   Nếu timer static thì reset nó khi load scene hoặc khi respawn.

7. Assets/Scripts/Items/Loot/LootDropper.cs: thêm field [SerializeField] int maxDropsPerKill (0 = không giới hạn) để giới hạn số món rơi.
```

---

## Giai đoạn 2 — Class Mechanics

```text
[Dán Quy tắc chung ở trên]

NHIỆM VỤ: Sửa bug logic trong Assets/Scripts/Classes/Mechanics/. Xác minh từng mục trước khi sửa.

1. ShadowGunnerMechanics.cs: ExitStealth() nghi ngờ không reset isAmbushReady nên buff x2.5 bị giữ mãi.
   → Reset cờ khi thoát tàng hình (hoặc sau một khoảng ân hạn ngắn có thể cấu hình), và tiêu thụ cờ sau phát bắn đầu tiên.
2. EngineerGunnerMechanics.cs: activeTurrets chỉ Add, không Remove, nên đặt đủ giới hạn thì kẹt vĩnh viễn.
   → Xóa trụ khỏi list khi trụ bị phá hoặc hết thời gian. Trước mỗi lần đặt, dọn phần tử null bằng RemoveAll(t => t == null).
   PetPerformMaintenance: chỉ sửa trụ khi pet nằm trong bán kính cấu hình được.
3. ElementalQueueMechanics.cs:
   a) isCataclysmReady không được reset sau khi cast → reset về false.
   b) ElementalShift() xử lý trường hợp queue có 2 phần tử nhưng không bao giờ chạy tới được, vì PushElement tự cast khi đủ 2.
      → Đọc mô tả thiết kế trong file/comment rồi chọn một hướng sửa hợp lý, giải thích lựa chọn. Không tự bịa thêm gameplay mới.
4. ElementalArcherMechanics.cs: mô tả ghi "3 phát cùng hệ thì phát thứ 4 chắc chắn bạo kích", nhưng code bạo kích ngay ở phát 3.
   → Sửa cho khớp mô tả.
5. PetWeaponMechanics.cs: buff Metamorphosis "+25% trong 3s" bị tắt ngay sau đòn đầu tiên. → Đổi sang buff theo thời gian (timer 3s).
   Thay FindObjectOfType<PetController>() bằng reference đã cache, lấy pet của chính owner qua PlayerPetRoster nếu có.
6. ChronoMageMechanics.cs: stasisAccumulatedDamage không dọn key khi enemy chết. → Dọn khi target bị destroy hoặc chết
   (dùng event chết của EnemyStats nếu có, nếu không thì quét key null hoặc đã chết khi stasis kết thúc).
7. AlchemistCauldronMechanics.cs: InterruptBrewing() log "hoàn trả 100%" nhưng không trả item. → Trả nguyên liệu vào Inventory.
   Gọi GetRefundRate() trong CompleteBrewing nếu đúng thiết kế. Sửa phép chia int trong CalculateCompositeQuality (dùng float rồi làm tròn).
8. SpiritArcherMechanics.cs: TryTriggerSpiritFusion dùng currentCharges -= MaxCharges thay vì gán = 0.
   SoulRecall: nếu CharacterStats có cơ chế khiên thì cấp khiên. Nếu chưa có thì ghi TODO rõ ràng và báo lại, không tự tạo hệ thống khiên lớn.
9. SummonerDualPetMechanics.cs: quick swap gán currentHp = maxHp nên đổi pet là được hồi máu miễn phí.
   → Lưu HP riêng cho từng slot và giữ nguyên HP khi swap.
10. Assets/Scripts/Characters/ClassSystem.cs: parryWindowTimer giảm xuống âm không giới hạn → clamp ở 0.
```

---

## Giai đoạn 3 — Combat & Enemy

```text
[Dán Quy tắc chung ở trên]

NHIỆM VỤ: Sửa logic chiến đấu và AI.

1. Công thức giáp không thống nhất: EnemyStats.cs dùng defense * 0.5f,
   còn 01_Core_And_Logic/Services/DamageCalculationService.cs dùng DefenseMitigationFactor = 0.6f.
   → Gom về một hằng số hoặc công thức duy nhất trong DamageCalculationService và cho EnemyStats gọi lại.
   Báo cho tôi giá trị đã chọn.
2. 01_Core_And_Logic/Domain/Models/PlayerHealthModel.cs: TakeDamage không truyền elementalMultiplier.
   → Truyền hệ số khắc hệ (từ ElementalSynergy hoặc DamageInfo) qua toàn bộ đường đi Hurtbox2D → presenter → model.
3. Combat/TelegraphIndicator.cs: AoE chỉ trúng tag "Player" trong khi comment ghi trúng cả Pet. → Thêm tag "Pet", gây damage qua component máu của pet.
4. Enemies/EnemyController.cs:
   a) EnemyProjectile bay xuyên Pet → gây damage cho Pet rồi hủy đạn.
   b) playerTarget không được reset khi player bị destroy hoặc inactive → kiểm tra null/activeInHierarchy rồi chuyển về Idle/Patrol.
5. Enemies/BossController.cs: Boss vẫn chase hoặc đánh thường trong lúc tung skill có telegraph.
   → Thêm cờ hoặc state Casting để khóa di chuyển và đòn đánh thường cho tới khi coroutine skill kết thúc.
6. Combat/Projectile.cs: nhiều va chạm trong cùng frame có thể vượt pierceCount → thêm cờ isSpent, return sớm khi đã hết lượt xuyên.
7. Enemies/EnemySpawner.cs: kiểm tra vị trí spawn bằng Physics2D.OverlapCircle với layer vật cản, thử lại tối đa N lần.
8. 01_Core_And_Logic/World/Spawners/ZoneEnemySpawner.cs: khi SpawnSingleEnemy() trả về null, pendingRespawnTimers bị thêm timer rác mỗi frame.
   → Chỉ thêm timer khi tổng (active + pending) < max, và log cảnh báo một lần nếu pool rỗng.
9. Combat/Hurtbox2D.cs: chuyển giá trị giảm sát thương khi block (85%) thành field cấu hình được. Không cần refactor kiến trúc.
```

---

## Giai đoạn 4 — Pets

```text
[Dán Quy tắc chung ở trên]

NHIỆM VỤ: Sửa Assets/Scripts/Pets/.

1. PetAI.cs FindBestTarget(): thêm kiểm tra tầm nhìn bằng Physics2D.Linecast với layer vật cản (tường/obstacle).
2. PetAI.cs TeleportToOwner(): kiểm tra điểm đến bằng OverlapCircle, thử vài lần. Nếu đều thất bại thì đặt pet ngay tại vị trí owner.
3. PetAI.cs CalculateAvoidanceSteering: bỏ qua collider của mục tiêu hiện tại và layer Enemy khi tính né vật cản.
4. PetCaptureSystem.cs:
   a) Pet vừa bắt được gán full máu → giữ tỷ lệ HP lúc bắt (hoặc giá trị thiết kế hợp lý). Kiểm tra constructor PetInstance.
   b) Kiểm tra null hoặc đã chết cho enemy trong suốt coroutine capture. Nếu enemy chết giữa chừng thì hủy capture an toàn.
```

---

## Giai đoạn 5 — Save System

```text
[Dán Quy tắc chung ở trên]

NHIỆM VỤ: Hợp nhất hệ thống save. Đây là thay đổi lớn: LÊN KẾ HOẠCH TRƯỚC, trình bày cho tôi duyệt rồi mới sửa.

Bối cảnh: có 2 hệ thống là Core/SaveSystem.cs (cũ, GameManager đang dùng) và Save/SaveManager.cs + SaveData.cs + SaveSecurity.cs (mới, đầy đủ hơn).

1. Liệt kê mọi chỗ gọi SaveSystem và SaveManager (grep). So sánh dữ liệu mà mỗi hệ thống lưu.
2. Đề xuất cách chuyển GameManager sang SaveManager, giữ khả năng đọc file save cũ (migration) nếu file cũ tồn tại.
3. Sau khi tôi duyệt:
   - Ghi file an toàn: ghi ra file .tmp rồi thay thế (File.Replace/Move), có giữ bản backup. Nếu file chính lỗi thì đọc lại từ backup.
   - SaveSecurity: tạo IV ngẫu nhiên cho mỗi lần mã hóa và ghi IV kèm vào đầu dữ liệu. Vẫn đọc được định dạng cũ.
     Thêm saveVersion vào SaveData.
   - SaveData: lưu thêm vị trí slot cho item trong túi.
   - SaveManager load: clamp HP của pet ≤ MaxHp. Tránh gọi Clear() lên collection mà UI đang tham chiếu (cập nhật tại chỗ rồi bắn event).
   - Cache reference thay vì gọi FindObjectOfType nhiều lần mỗi lần save.
4. CharacterStats.Die(): thay hồi sinh ở Vector3.zero bằng điểm hồi sinh (Waystone cuối cùng hoặc spawn point của scene).
   Kiểm tra Waystone.cs và ZoneManager.cs xem đã có dữ liệu này chưa.
```

---

## Giai đoạn 6 — Server, UI & Performance

```text
[Dán Quy tắc chung ở trên]

NHIỆM VỤ:

A. Server (server/src):
1. PUT /api/players/:playerId/save đang lưu thẳng { ...req.body } → chỉ chấp nhận các field trong whitelist
   và validate kiểu, khoảng giá trị (dùng zod hoặc validate thủ công).
2. Thêm try/catch hoặc middleware xử lý lỗi cho mọi route async. Thêm express-rate-limit. Cấu hình CORS theo biến môi trường.
3. Thêm xác thực đơn giản: API key hoặc token đọc từ process.env, gửi qua header, và cập nhật Core/MongoGameClient.cs để gửi header đó.
   Trong MongoGameClient đặt request.timeout, và xử lý trường hợp callback nhận data null.
4. Cập nhật package.json nếu thêm dependency, rồi chạy npm install.

B. UI (Assets/Scripts/UI):
1. ClassChangeUI.TryOpenPromotion(): đang tự tạo ClassSelectionUI không có reference → báo lỗi rõ ràng thay vì tạo object rỗng
   (hoặc tìm cả object đang inactive bằng FindObjectOfType(true)).
2. EquipmentUI: đang đăng ký cả Inventory.OnEquipmentChanged lẫn EventBus.OnEquipmentChanged → chỉ giữ một.
3. InventoryUI/EquipmentUI: khi unsubscribe, kiểm tra Inventory.Instance != null.
4. GameplayHUD.UpdateQuickSlotsUI() lặp qua túi đồ mỗi frame → chỉ cập nhật khi inventory thay đổi (event).
5. PetHUDWidget, CharacterStatsUI, SkillsUI, TalentUI: cache reference thay vì gọi FindObjectOfType trong Refresh.
6. BossHealthBarUI: dừng Lerp khi đã gần đạt targetFill.

C. Performance & vặt:
1. CameraFollow2D: Lerp nên phụ thuộc deltaTime (1 - Mathf.Exp(-speed * dt)). Nếu target null thì tìm lại Player.
2. PortalTrigger: chỉ kích hoạt với tag Player, thêm cooldown, dịch chuyển qua Rigidbody2D.position.
3. HungerSystem: hồi máu theo tick (mỗi 1 giây) thay vì mỗi frame.
4. AdventurerGuild: so khớp quest bằng ID/tên chính xác thay vì string.Contains.
5. WorldItem: hủy trạng thái nam châm khi player ra ngoài bán kính tối đa.
6. GatheringNode: không CreateInstance<ItemData>() lặp lại nhiều lần; chỉ tạo một lần rồi cache.

KHÔNG làm trong giai đoạn này: tách PlayerController hoặc GameplayHUD (để dành cho một task refactor riêng).
```

---

## (Tùy chọn) Giai đoạn 7 — Refactor kiến trúc

```text
[Dán Quy tắc chung ở trên]

Phân tích Assets/Scripts/Characters/PlayerController.cs và Assets/Scripts/UI/Gameplay/GameplayHUD.cs.
CHỈ lập kế hoạch tách thành các component nhỏ (ví dụ PlayerMovement, PlayerCombat, PlayerInteraction; các widget HUD riêng),
nói rõ field serialized nào sẽ di chuyển và ảnh hưởng tới prefab/scene ra sao.
Chưa sửa code cho tới khi tôi duyệt kế hoạch.
Đánh giá thêm ClassDemoSwitcherUI.cs: nó có được dùng trong scene không (grep GUID của script trong file .unity/.prefab), rồi đề xuất giữ, chuyển sang Editor tool, hay xóa.
```
