"""
Update 4 NPC Prefabs YAML with newly generated Modular Sprites and Props
Part 2/3: Boris, Elena, Carlos, Lina
"""

import os
import re

def get_guid(rel_path):
    meta_path = rel_path + ".meta"
    if not os.path.exists(meta_path):
        raise FileNotFoundError(f"Missing meta file for {rel_path}")
    with open(meta_path, "r", encoding="utf-8") as f:
        content = f.read()
    m = re.search(r"guid:\s*([a-f0-9]{32})", content)
    if not m:
        raise ValueError(f"Could not find guid in {meta_path}")
    return m.group(1)

def sprite_ref(guid):
    return f"{{fileID: 21300000, guid: {guid}, type: 3}}"

def update_prefabs():
    # -------------------------------------------------------------
    # 1. BORIS (NPC_Blacksmith_Barnaby.prefab)
    # -------------------------------------------------------------
    boris_path = "Assets/Prefabs/ForestPrototype/NPC_Blacksmith_Barnaby.prefab"
    with open(boris_path, "r", encoding="utf-8") as f:
        content = f.read()

    # Rename Prop_SteelHammer -> Prop_Hammer
    content = content.replace("m_Name: Prop_SteelHammer", "m_Name: Prop_Hammer")

    # Guids for Boris body
    boris_idle_front = get_guid("Assets/Sprites/NPCs/Modular/Boris/npc_boris_body_idle_front.png")
    boris_idle_back = get_guid("Assets/Sprites/NPCs/Modular/Boris/npc_boris_body_idle_back.png")
    boris_idle_side = get_guid("Assets/Sprites/NPCs/Modular/Boris/npc_boris_body_idle_side.png")
    boris_idle_left = get_guid("Assets/Sprites/NPCs/Modular/Boris/npc_boris_body_idle_left.png")
    boris_walk_front = [get_guid(f"Assets/Sprites/NPCs/Modular/Boris/npc_boris_body_walk_front_{i}.png") for i in range(1, 5)]
    boris_walk_back = [get_guid(f"Assets/Sprites/NPCs/Modular/Boris/npc_boris_body_walk_back_{i}.png") for i in range(1, 5)]
    boris_walk_side = [get_guid(f"Assets/Sprites/NPCs/Modular/Boris/npc_boris_body_walk_side_{i}.png") for i in range(1, 5)]
    boris_walk_left = [get_guid(f"Assets/Sprites/NPCs/Modular/Boris/npc_boris_body_walk_left_{i}.png") for i in range(1, 5)]

    # Hammer
    hammer_front = get_guid("Assets/Sprites/NPCs/Modular/Boris/prop_boris_hammer_front.png")
    hammer_back = get_guid("Assets/Sprites/NPCs/Modular/Boris/prop_boris_hammer_back.png")
    hammer_side = get_guid("Assets/Sprites/NPCs/Modular/Boris/prop_boris_hammer_side.png")
    hammer_left = get_guid("Assets/Sprites/NPCs/Modular/Boris/prop_boris_hammer_left.png")

    # Update DirectionalWalkAnimator on Boris
    # Replace walkSideFrames block with walkSideFrames + walkLeftFrames
    old_anim_pattern = r"(DirectionalWalkAnimator\n\s*walkFrontFrames:[\s\S]*?idleSide:[^\n]*)"
    new_anim_block = f"""DirectionalWalkAnimator
  walkFrontFrames:
  - {sprite_ref(boris_walk_front[0])}
  - {sprite_ref(boris_walk_front[1])}
  - {sprite_ref(boris_walk_front[2])}
  - {sprite_ref(boris_walk_front[3])}
  walkBackFrames:
  - {sprite_ref(boris_walk_back[0])}
  - {sprite_ref(boris_walk_back[1])}
  - {sprite_ref(boris_walk_back[2])}
  - {sprite_ref(boris_walk_back[3])}
  walkSideFrames:
  - {sprite_ref(boris_walk_side[0])}
  - {sprite_ref(boris_walk_side[1])}
  - {sprite_ref(boris_walk_side[2])}
  - {sprite_ref(boris_walk_side[3])}
  walkLeftFrames:
  - {sprite_ref(boris_walk_left[0])}
  - {sprite_ref(boris_walk_left[1])}
  - {sprite_ref(boris_walk_left[2])}
  - {sprite_ref(boris_walk_left[3])}
  idleFront: {sprite_ref(boris_idle_front)}
  idleBack: {sprite_ref(boris_idle_back)}
  idleSide: {sprite_ref(boris_idle_side)}
  idleLeft: {sprite_ref(boris_idle_left)}"""

    content = re.sub(old_anim_pattern, new_anim_block, content)

    # Update Hammer NPCModularProp
    old_hammer_prop = r"(NPCModularProp\n\s*spriteFront:[\s\S]*?bounceOnlyWhenMoving:[^\n]*)"
    new_hammer_prop = f"""NPCModularProp
  spriteFront: {sprite_ref(hammer_front)}
  spriteBack: {sprite_ref(hammer_back)}
  spriteSide: {sprite_ref(hammer_side)}
  spriteLeft: {sprite_ref(hammer_left)}
  sortingOffsetFront: 1
  sortingOffsetBack: -1
  sortingOffsetSideRight: -1
  sortingOffsetSideLeft: 1
  offsetFront: {{x: 0, y: 0}}
  offsetBack: {{x: 0, y: 0}}
  offsetSide: {{x: 0, y: 0}}
  offsetLeft: {{x: 0, y: 0}}
  syncWithWalkStepBob: 1
  hasSecondaryMotion: 0
  motionAmplitudeX: 0
  motionAmplitudeY: 0
  motionFrequency: 0
  bounceOnlyWhenMoving: 1"""

    content = re.sub(old_hammer_prop, new_hammer_prop, content)
    with open(boris_path, "w", encoding="utf-8") as f:
        f.write(content)
    print("Boris prefab updated!")

    # -------------------------------------------------------------
    # 2. ELENA (NPC_Alchemist_Elena.prefab)
    # -------------------------------------------------------------
    elena_path = "Assets/Prefabs/ForestPrototype/NPC_Alchemist_Elena.prefab"
    with open(elena_path, "r", encoding="utf-8") as f:
        content = f.read()

    # Rename Prop_PotionSatchel -> Prop_HerbBag
    content = content.replace("m_Name: Prop_PotionSatchel", "m_Name: Prop_HerbBag")

    # Guids for Elena body
    elena_idle_front = get_guid("Assets/Sprites/NPCs/Modular/Elena/npc_elena_body_idle_front.png")
    elena_idle_back = get_guid("Assets/Sprites/NPCs/Modular/Elena/npc_elena_body_idle_back.png")
    elena_idle_side = get_guid("Assets/Sprites/NPCs/Modular/Elena/npc_elena_body_idle_side.png")
    elena_idle_left = get_guid("Assets/Sprites/NPCs/Modular/Elena/npc_elena_body_idle_left.png")
    elena_walk_front = [get_guid(f"Assets/Sprites/NPCs/Modular/Elena/npc_elena_body_walk_front_{i}.png") for i in range(1, 5)]
    elena_walk_back = [get_guid(f"Assets/Sprites/NPCs/Modular/Elena/npc_elena_body_walk_back_{i}.png") for i in range(1, 5)]
    elena_walk_side = [get_guid(f"Assets/Sprites/NPCs/Modular/Elena/npc_elena_body_walk_side_{i}.png") for i in range(1, 5)]
    elena_walk_left = [get_guid(f"Assets/Sprites/NPCs/Modular/Elena/npc_elena_body_walk_left_{i}.png") for i in range(1, 5)]

    # Herb Bag
    pouch_front = get_guid("Assets/Sprites/NPCs/Modular/Elena/prop_elena_pouch_front.png")
    pouch_back = get_guid("Assets/Sprites/NPCs/Modular/Elena/prop_elena_pouch_back.png")
    pouch_side = get_guid("Assets/Sprites/NPCs/Modular/Elena/prop_elena_pouch_side.png")
    pouch_left = get_guid("Assets/Sprites/NPCs/Modular/Elena/prop_elena_pouch_left.png")

    new_elena_anim_block = f"""DirectionalWalkAnimator
  walkFrontFrames:
  - {sprite_ref(elena_walk_front[0])}
  - {sprite_ref(elena_walk_front[1])}
  - {sprite_ref(elena_walk_front[2])}
  - {sprite_ref(elena_walk_front[3])}
  walkBackFrames:
  - {sprite_ref(elena_walk_back[0])}
  - {sprite_ref(elena_walk_back[1])}
  - {sprite_ref(elena_walk_back[2])}
  - {sprite_ref(elena_walk_back[3])}
  walkSideFrames:
  - {sprite_ref(elena_walk_side[0])}
  - {sprite_ref(elena_walk_side[1])}
  - {sprite_ref(elena_walk_side[2])}
  - {sprite_ref(elena_walk_side[3])}
  walkLeftFrames:
  - {sprite_ref(elena_walk_left[0])}
  - {sprite_ref(elena_walk_left[1])}
  - {sprite_ref(elena_walk_left[2])}
  - {sprite_ref(elena_walk_left[3])}
  idleFront: {sprite_ref(elena_idle_front)}
  idleBack: {sprite_ref(elena_idle_back)}
  idleSide: {sprite_ref(elena_idle_side)}
  idleLeft: {sprite_ref(elena_idle_left)}"""

    content = re.sub(old_anim_pattern, new_elena_anim_block, content)

    # Update Herb Bag NPCModularProp
    new_herb_prop = f"""NPCModularProp
  spriteFront: {sprite_ref(pouch_front)}
  spriteBack: {sprite_ref(pouch_back)}
  spriteSide: {sprite_ref(pouch_side)}
  spriteLeft: {sprite_ref(pouch_left)}
  sortingOffsetFront: 1
  sortingOffsetBack: -1
  sortingOffsetSideRight: 1
  sortingOffsetSideLeft: -1
  offsetFront: {{x: 0, y: 0}}
  offsetBack: {{x: 0, y: 0}}
  offsetSide: {{x: 0, y: 0}}
  offsetLeft: {{x: 0, y: 0}}
  syncWithWalkStepBob: 1
  hasSecondaryMotion: 1
  motionAmplitudeX: 0.015
  motionAmplitudeY: 0.04
  motionFrequency: 7
  bounceOnlyWhenMoving: 1"""

    content = re.sub(old_hammer_prop, new_herb_prop, content)
    with open(elena_path, "w", encoding="utf-8") as f:
        f.write(content)
    print("Elena prefab updated!")

    # -------------------------------------------------------------
    # 3. CARLOS (NPC_GuildMaster_Carlos.prefab)
    # -------------------------------------------------------------
    carlos_path = "Assets/Prefabs/ForestPrototype/NPC_GuildMaster_Carlos.prefab"
    with open(carlos_path, "r", encoding="utf-8") as f:
        content = f.read()

    # Guids for Carlos body
    carlos_idle_front = get_guid("Assets/Sprites/NPCs/Modular/Carlos/npc_carlos_body_idle_front.png")
    carlos_idle_back = get_guid("Assets/Sprites/NPCs/Modular/Carlos/npc_carlos_body_idle_back.png")
    carlos_idle_side = get_guid("Assets/Sprites/NPCs/Modular/Carlos/npc_carlos_body_idle_side.png")
    carlos_idle_left = get_guid("Assets/Sprites/NPCs/Modular/Carlos/npc_carlos_body_idle_left.png")
    carlos_walk_front = [get_guid(f"Assets/Sprites/NPCs/Modular/Carlos/npc_carlos_body_walk_front_{i}.png") for i in range(1, 5)]
    carlos_walk_back = [get_guid(f"Assets/Sprites/NPCs/Modular/Carlos/npc_carlos_body_walk_back_{i}.png") for i in range(1, 5)]
    carlos_walk_side = [get_guid(f"Assets/Sprites/NPCs/Modular/Carlos/npc_carlos_body_walk_side_{i}.png") for i in range(1, 5)]
    carlos_walk_left = [get_guid(f"Assets/Sprites/NPCs/Modular/Carlos/npc_carlos_body_walk_left_{i}.png") for i in range(1, 5)]

    # Cape
    cape_front = get_guid("Assets/Sprites/NPCs/Modular/Carlos/prop_carlos_cape_front.png")
    cape_back = get_guid("Assets/Sprites/NPCs/Modular/Carlos/prop_carlos_cape_back.png")
    cape_side = get_guid("Assets/Sprites/NPCs/Modular/Carlos/prop_carlos_cape_side.png")
    cape_left = get_guid("Assets/Sprites/NPCs/Modular/Carlos/prop_carlos_cape_left.png")

    # Sword
    sword_front = get_guid("Assets/Sprites/NPCs/Modular/Carlos/prop_carlos_sword_front.png")
    sword_back = get_guid("Assets/Sprites/NPCs/Modular/Carlos/prop_carlos_sword_back.png")
    sword_side = get_guid("Assets/Sprites/NPCs/Modular/Carlos/prop_carlos_sword_side.png")
    sword_left = get_guid("Assets/Sprites/NPCs/Modular/Carlos/prop_carlos_sword_left.png")

    new_carlos_anim_block = f"""DirectionalWalkAnimator
  walkFrontFrames:
  - {sprite_ref(carlos_walk_front[0])}
  - {sprite_ref(carlos_walk_front[1])}
  - {sprite_ref(carlos_walk_front[2])}
  - {sprite_ref(carlos_walk_front[3])}
  walkBackFrames:
  - {sprite_ref(carlos_walk_back[0])}
  - {sprite_ref(carlos_walk_back[1])}
  - {sprite_ref(carlos_walk_back[2])}
  - {sprite_ref(carlos_walk_back[3])}
  walkSideFrames:
  - {sprite_ref(carlos_walk_side[0])}
  - {sprite_ref(carlos_walk_side[1])}
  - {sprite_ref(carlos_walk_side[2])}
  - {sprite_ref(carlos_walk_side[3])}
  walkLeftFrames:
  - {sprite_ref(carlos_walk_left[0])}
  - {sprite_ref(carlos_walk_left[1])}
  - {sprite_ref(carlos_walk_left[2])}
  - {sprite_ref(carlos_walk_left[3])}
  idleFront: {sprite_ref(carlos_idle_front)}
  idleBack: {sprite_ref(carlos_idle_back)}
  idleSide: {sprite_ref(carlos_idle_side)}
  idleLeft: {sprite_ref(carlos_idle_left)}"""

    content = re.sub(old_anim_pattern, new_carlos_anim_block, content)

    # Carlos has 2 NPCModularProps: first is Cape, second is Sword
    # We find all occurrences of NPCModularProp blocks
    matches = list(re.finditer(r"MonoBehaviour:\n\s*m_ObjectHideFlags: 0[\s\S]*?Assembly-CSharp::ActionRPG\.Town\.NPCModularProp[\s\S]*?(?=\n---|\Z)", content))
    if len(matches) == 2:
        # Cape prop
        cape_block = matches[0].group(0)
        # Preserve header lines
        header_cape = cape_block.split("EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp")[0] + "EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp"
        new_cape_prop = f"""{header_cape}
  spriteFront: {sprite_ref(cape_front)}
  spriteBack: {sprite_ref(cape_back)}
  spriteSide: {sprite_ref(cape_side)}
  spriteLeft: {sprite_ref(cape_left)}
  sortingOffsetFront: -1
  sortingOffsetBack: 1
  sortingOffsetSideRight: -1
  sortingOffsetSideLeft: -1
  offsetFront: {{x: 0, y: 0}}
  offsetBack: {{x: 0, y: 0}}
  offsetSide: {{x: 0, y: 0}}
  offsetLeft: {{x: 0, y: 0}}
  syncWithWalkStepBob: 1
  hasSecondaryMotion: 1
  motionAmplitudeX: 0.025
  motionAmplitudeY: 0.015
  motionFrequency: 5
  bounceOnlyWhenMoving: 1
  sr: {{fileID: 0}}"""

        sword_block = matches[1].group(0)
        header_sword = sword_block.split("EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp")[0] + "EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp"
        new_sword_prop = f"""{header_sword}
  spriteFront: {sprite_ref(sword_front)}
  spriteBack: {sprite_ref(sword_back)}
  spriteSide: {sprite_ref(sword_side)}
  spriteLeft: {sprite_ref(sword_left)}
  sortingOffsetFront: 1
  sortingOffsetBack: -1
  sortingOffsetSideRight: -1
  sortingOffsetSideLeft: 1
  offsetFront: {{x: 0, y: 0}}
  offsetBack: {{x: 0, y: 0}}
  offsetSide: {{x: 0, y: 0}}
  offsetLeft: {{x: 0, y: 0}}
  syncWithWalkStepBob: 1
  hasSecondaryMotion: 0
  motionAmplitudeX: 0
  motionAmplitudeY: 0
  motionFrequency: 0
  bounceOnlyWhenMoving: 1
  sr: {{fileID: 0}}"""

        content = content.replace(cape_block, new_cape_prop).replace(sword_block, new_sword_prop)

    with open(carlos_path, "w", encoding="utf-8") as f:
        f.write(content)
    print("Carlos prefab updated!")

    # -------------------------------------------------------------
    # 4. LINA (NPC_Merchant_Lina.prefab)
    # -------------------------------------------------------------
    lina_path = "Assets/Prefabs/ForestPrototype/NPC_Merchant_Lina.prefab"
    with open(lina_path, "r", encoding="utf-8") as f:
        content = f.read()

    # Rename Prop_WickerBasket -> Prop_Basket
    content = content.replace("m_Name: Prop_WickerBasket", "m_Name: Prop_Basket")

    # Guids for Lina body
    lina_idle_front = get_guid("Assets/Sprites/NPCs/Modular/Lina/npc_lina_body_idle_front.png")
    lina_idle_back = get_guid("Assets/Sprites/NPCs/Modular/Lina/npc_lina_body_idle_back.png")
    lina_idle_side = get_guid("Assets/Sprites/NPCs/Modular/Lina/npc_lina_body_idle_side.png")
    lina_idle_left = get_guid("Assets/Sprites/NPCs/Modular/Lina/npc_lina_body_idle_left.png")
    lina_walk_front = [get_guid(f"Assets/Sprites/NPCs/Modular/Lina/npc_lina_body_walk_front_{i}.png") for i in range(1, 5)]
    lina_walk_back = [get_guid(f"Assets/Sprites/NPCs/Modular/Lina/npc_lina_body_walk_back_{i}.png") for i in range(1, 5)]
    lina_walk_side = [get_guid(f"Assets/Sprites/NPCs/Modular/Lina/npc_lina_body_walk_side_{i}.png") for i in range(1, 5)]
    lina_walk_left = [get_guid(f"Assets/Sprites/NPCs/Modular/Lina/npc_lina_body_walk_left_{i}.png") for i in range(1, 5)]

    # Basket
    basket_front = get_guid("Assets/Sprites/NPCs/Modular/Lina/prop_lina_basket_front.png")
    basket_back = get_guid("Assets/Sprites/NPCs/Modular/Lina/prop_lina_basket_back.png")
    basket_side = get_guid("Assets/Sprites/NPCs/Modular/Lina/prop_lina_basket_side.png")
    basket_left = get_guid("Assets/Sprites/NPCs/Modular/Lina/prop_lina_basket_left.png")

    # Coin pouch
    lina_pouch_front = get_guid("Assets/Sprites/NPCs/Modular/Lina/prop_lina_pouch_front.png")
    lina_pouch_back = get_guid("Assets/Sprites/NPCs/Modular/Lina/prop_lina_pouch_back.png")
    lina_pouch_side = get_guid("Assets/Sprites/NPCs/Modular/Lina/prop_lina_pouch_side.png")
    lina_pouch_left = get_guid("Assets/Sprites/NPCs/Modular/Lina/prop_lina_pouch_left.png")

    new_lina_anim_block = f"""DirectionalWalkAnimator
  walkFrontFrames:
  - {sprite_ref(lina_walk_front[0])}
  - {sprite_ref(lina_walk_front[1])}
  - {sprite_ref(lina_walk_front[2])}
  - {sprite_ref(lina_walk_front[3])}
  walkBackFrames:
  - {sprite_ref(lina_walk_back[0])}
  - {sprite_ref(lina_walk_back[1])}
  - {sprite_ref(lina_walk_back[2])}
  - {sprite_ref(lina_walk_back[3])}
  walkSideFrames:
  - {sprite_ref(lina_walk_side[0])}
  - {sprite_ref(lina_walk_side[1])}
  - {sprite_ref(lina_walk_side[2])}
  - {sprite_ref(lina_walk_side[3])}
  walkLeftFrames:
  - {sprite_ref(lina_walk_left[0])}
  - {sprite_ref(lina_walk_left[1])}
  - {sprite_ref(lina_walk_left[2])}
  - {sprite_ref(lina_walk_left[3])}
  idleFront: {sprite_ref(lina_idle_front)}
  idleBack: {sprite_ref(lina_idle_back)}
  idleSide: {sprite_ref(lina_idle_side)}
  idleLeft: {sprite_ref(lina_idle_left)}"""

    content = re.sub(old_anim_pattern, new_lina_anim_block, content)

    # Lina has 2 NPCModularProps: first is CoinPouch, second is Basket
    matches = list(re.finditer(r"MonoBehaviour:\n\s*m_ObjectHideFlags: 0[\s\S]*?Assembly-CSharp::ActionRPG\.Town\.NPCModularProp[\s\S]*?(?=\n---|\Z)", content))
    if len(matches) == 2:
        # First is coin pouch (GameObject 4785178482636729635 Prop_CoinPouch)
        pouch_block = matches[0].group(0)
        header_pouch = pouch_block.split("EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp")[0] + "EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp"
        new_pouch_prop = f"""{header_pouch}
  spriteFront: {sprite_ref(lina_pouch_front)}
  spriteBack: {sprite_ref(lina_pouch_back)}
  spriteSide: {sprite_ref(lina_pouch_side)}
  spriteLeft: {sprite_ref(lina_pouch_left)}
  sortingOffsetFront: 1
  sortingOffsetBack: -1
  sortingOffsetSideRight: -1
  sortingOffsetSideLeft: 1
  offsetFront: {{x: 0, y: 0}}
  offsetBack: {{x: 0, y: 0}}
  offsetSide: {{x: 0, y: 0}}
  offsetLeft: {{x: 0, y: 0}}
  syncWithWalkStepBob: 1
  hasSecondaryMotion: 0
  motionAmplitudeX: 0
  motionAmplitudeY: 0
  motionFrequency: 0
  bounceOnlyWhenMoving: 1
  sr: {{fileID: 0}}"""

        # Second is basket (GameObject 6416232675129424951 Prop_Basket)
        basket_block = matches[1].group(0)
        header_basket = basket_block.split("EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp")[0] + "EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp"
        new_basket_prop = f"""{header_basket}
  spriteFront: {sprite_ref(basket_front)}
  spriteBack: {sprite_ref(basket_back)}
  spriteSide: {sprite_ref(basket_side)}
  spriteLeft: {sprite_ref(basket_left)}
  sortingOffsetFront: 1
  sortingOffsetBack: -1
  sortingOffsetSideRight: 1
  sortingOffsetSideLeft: -1
  offsetFront: {{x: 0, y: 0}}
  offsetBack: {{x: 0, y: 0}}
  offsetSide: {{x: 0, y: 0}}
  offsetLeft: {{x: 0, y: 0}}
  syncWithWalkStepBob: 1
  hasSecondaryMotion: 1
  motionAmplitudeX: 0.03
  motionAmplitudeY: 0.02
  motionFrequency: 7
  bounceOnlyWhenMoving: 1
  sr: {{fileID: 0}}"""

        content = content.replace(pouch_block, new_pouch_prop).replace(basket_block, new_basket_prop)

    with open(lina_path, "w", encoding="utf-8") as f:
        f.write(content)
    print("Lina prefab updated!")

if __name__ == "__main__":
    update_prefabs()
