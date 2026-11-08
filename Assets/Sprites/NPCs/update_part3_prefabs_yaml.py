"""
Update Part 3/3 Prefabs YAML with newly generated Modular Sprites, Props,
FalconBehaviorController, and VillagerPropInteract:
1. NPC_PetMaster_Kaelen.prefab (+ Child Pet_Falcon & PerchPoint)
2. Pet_Falcon.prefab
3. NPC_Villager_Wanderer.prefab (+ Child Prop_StrawHat, Prop_FruitBasket & VillagerPropInteract)
"""

import os
import re

FALCON_BEHAVIOR_GUID = "8b4c9a12d6e54f039c17a5e82d3f6b19"
VILLAGER_INTERACT_GUID = "7c5d8e23f4a649b18e29c1d43a5b7f20"


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


def update_part3_prefabs():
    old_anim_pattern = r"(DirectionalWalkAnimator\n\s*walkFrontFrames:[\s\S]*?idleSide:[^\n]*(?:\n\s*idleLeft:[^\n]*)?)"

    # ==========================================================================
    # 1. KAELEN (NPC_PetMaster_Kaelen.prefab) + Child Pet_Falcon
    # ==========================================================================
    kaelen_path = "Assets/Prefabs/ForestPrototype/NPC_PetMaster_Kaelen.prefab"
    with open(kaelen_path, "r", encoding="utf-8") as f:
        content = f.read()

    k_idle_front = get_guid("Assets/Sprites/NPCs/Modular/Kaelen/npc_kaelen_body_idle_front.png")
    k_idle_back = get_guid("Assets/Sprites/NPCs/Modular/Kaelen/npc_kaelen_body_idle_back.png")
    k_idle_side = get_guid("Assets/Sprites/NPCs/Modular/Kaelen/npc_kaelen_body_idle_side.png")
    k_idle_left = get_guid("Assets/Sprites/NPCs/Modular/Kaelen/npc_kaelen_body_idle_left.png")

    k_walk_front = [get_guid(f"Assets/Sprites/NPCs/Modular/Kaelen/npc_kaelen_body_walk_front_{i}.png") for i in range(1, 5)]
    k_walk_back = [get_guid(f"Assets/Sprites/NPCs/Modular/Kaelen/npc_kaelen_body_walk_back_{i}.png") for i in range(1, 5)]
    k_walk_side = [get_guid(f"Assets/Sprites/NPCs/Modular/Kaelen/npc_kaelen_body_walk_side_{i}.png") for i in range(1, 5)]
    k_walk_left = [get_guid(f"Assets/Sprites/NPCs/Modular/Kaelen/npc_kaelen_body_walk_left_{i}.png") for i in range(1, 5)]

    f_perch_1 = get_guid("Assets/Sprites/NPCs/Modular/Falcon/falcon_idle_perched_1.png")
    f_perch_2 = get_guid("Assets/Sprites/NPCs/Modular/Falcon/falcon_idle_perched_2.png")
    f_fly = [get_guid(f"Assets/Sprites/NPCs/Modular/Falcon/falcon_fly_loop_{i}.png") for i in range(1, 5)]

    new_kaelen_anim = f"""DirectionalWalkAnimator
  walkFrontFrames:
  - {sprite_ref(k_walk_front[0])}
  - {sprite_ref(k_walk_front[1])}
  - {sprite_ref(k_walk_front[2])}
  - {sprite_ref(k_walk_front[3])}
  walkBackFrames:
  - {sprite_ref(k_walk_back[0])}
  - {sprite_ref(k_walk_back[1])}
  - {sprite_ref(k_walk_back[2])}
  - {sprite_ref(k_walk_back[3])}
  walkSideFrames:
  - {sprite_ref(k_walk_side[0])}
  - {sprite_ref(k_walk_side[1])}
  - {sprite_ref(k_walk_side[2])}
  - {sprite_ref(k_walk_side[3])}
  walkLeftFrames:
  - {sprite_ref(k_walk_left[0])}
  - {sprite_ref(k_walk_left[1])}
  - {sprite_ref(k_walk_left[2])}
  - {sprite_ref(k_walk_left[3])}
  idleFront: {sprite_ref(k_idle_front)}
  idleBack: {sprite_ref(k_idle_back)}
  idleSide: {sprite_ref(k_idle_side)}
  idleLeft: {sprite_ref(k_idle_left)}"""

    content = re.sub(old_anim_pattern, new_kaelen_anim, content)

    # Fix Pet_Falcon child localScale to {x: 1, y: 1, z: 1} when parented to Kaelen
    content = re.sub(
        r"(m_GameObject: \{fileID: 2497645711354552198\}[\s\S]*?m_LocalScale: )\{x: [0-9.]+, y: [0-9.]+, z: 1\}",
        r"\g<1>{x: 1, y: 1, z: 1}",
        content,
    )

    # Update perchOffset on NPCModularCharacter2D so Falcon sits on left gauntlet (X = +0.3125 in front)
    content = content.replace(
        "perchOffsetFront: {x: -0.25, y: 0.08}",
        "perchOffsetFront: {x: 0.3125, y: 0.0625}",
    ).replace(
        "perchOffsetBack: {x: 0.25, y: 0.08}",
        "perchOffsetBack: {x: -0.3125, y: 0.0625}",
    ).replace(
        "perchOffsetSide: {x: -0.28, y: 0.06}",
        "perchOffsetSide: {x: 0.25, y: 0.0625}",
    )

    # Add FalconBehaviorController component to Pet_Falcon GameObject if not already present
    if FALCON_BEHAVIOR_GUID not in content:
        content = content.replace(
            "  - component: {fileID: 6912333305807143405}\n  - component: {fileID: 5808892613853724282}",
            "  - component: {fileID: 6912333305807143405}\n  - component: {fileID: 6912333305807143406}\n  - component: {fileID: 5808892613853724282}",
        )
        falcon_behavior_yaml = f"""--- !u!114 &6912333305807143406
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 2497645711354552198}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {FALCON_BEHAVIOR_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.FalconBehaviorController
  kaelenTransform: {{fileID: 1392848017612851322}}
  perchPoint: {{fileID: 7793579537056927887}}
  idlePerchedFrames:
  - {sprite_ref(f_perch_1)}
  - {sprite_ref(f_perch_2)}
  flyLoopFrames:
  - {sprite_ref(f_fly[0])}
  - {sprite_ref(f_fly[1])}
  - {sprite_ref(f_fly[2])}
  - {sprite_ref(f_fly[3])}
  perchedFps: 2.5
  flyFps: 8.5
  orbitRadiusX: 2.2
  orbitRadiusY: 1.45
  flightHeightOffset: 0.55
  angularSpeed: 1.65
  landingSpeed: 3.2
  minPerchDuration: 4
  maxPerchDuration: 6.5
  minOrbitLoops: 1.5
  maxOrbitLoops: 2.5
  currentState: 0
"""
        content = content.rstrip() + "\n" + falcon_behavior_yaml

    with open(kaelen_path, "w", encoding="utf-8") as f:
        f.write(content)
    print("NPC_PetMaster_Kaelen.prefab updated!")

    # ==========================================================================
    # 2. VILLAGER OAKHAVEN (NPC_Villager_Wanderer.prefab)
    # ==========================================================================
    villager_path = "Assets/Prefabs/ForestPrototype/NPC_Villager_Wanderer.prefab"
    with open(villager_path, "r", encoding="utf-8") as f:
        v_content = f.read()

    v_unarmed_idle_front = get_guid("Assets/Sprites/NPCs/Modular/Villager/npc_villager_body_idle_front.png")
    v_unarmed_idle_back = get_guid("Assets/Sprites/NPCs/Modular/Villager/npc_villager_body_idle_back.png")
    v_unarmed_idle_side = get_guid("Assets/Sprites/NPCs/Modular/Villager/npc_villager_body_idle_side.png")
    v_unarmed_idle_left = get_guid("Assets/Sprites/NPCs/Modular/Villager/npc_villager_body_idle_left.png")

    v_unarmed_walk_front = [get_guid(f"Assets/Sprites/NPCs/Modular/Villager/npc_villager_body_walk_front_{i}.png") for i in range(1, 5)]
    v_unarmed_walk_back = [get_guid(f"Assets/Sprites/NPCs/Modular/Villager/npc_villager_body_walk_back_{i}.png") for i in range(1, 5)]
    v_unarmed_walk_side = [get_guid(f"Assets/Sprites/NPCs/Modular/Villager/npc_villager_body_walk_side_{i}.png") for i in range(1, 5)]
    v_unarmed_walk_left = [get_guid(f"Assets/Sprites/NPCs/Modular/Villager/npc_villager_body_walk_left_{i}.png") for i in range(1, 5)]

    v_carry_idle_front = get_guid("Assets/Sprites/NPCs/Modular/Villager/npc_villager_carry_idle_front.png")
    v_carry_idle_back = get_guid("Assets/Sprites/NPCs/Modular/Villager/npc_villager_carry_idle_back.png")
    v_carry_idle_side = get_guid("Assets/Sprites/NPCs/Modular/Villager/npc_villager_carry_idle_side.png")
    v_carry_idle_left = get_guid("Assets/Sprites/NPCs/Modular/Villager/npc_villager_carry_idle_left.png")

    v_carry_walk_front = [get_guid(f"Assets/Sprites/NPCs/Modular/Villager/npc_villager_carry_walk_front_{i}.png") for i in range(1, 5)]
    v_carry_walk_back = [get_guid(f"Assets/Sprites/NPCs/Modular/Villager/npc_villager_carry_walk_back_{i}.png") for i in range(1, 5)]
    v_carry_walk_side = [get_guid(f"Assets/Sprites/NPCs/Modular/Villager/npc_villager_carry_walk_side_{i}.png") for i in range(1, 5)]
    v_carry_walk_left = [get_guid(f"Assets/Sprites/NPCs/Modular/Villager/npc_villager_carry_walk_left_{i}.png") for i in range(1, 5)]

    hat_front = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_hat_front.png")
    hat_back = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_hat_back.png")
    hat_side = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_hat_side.png")
    hat_left = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_hat_left.png")
    hat_world = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_hat_world.png")

    basket_front = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_basket_front.png")
    basket_back = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_basket_back.png")
    basket_side = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_basket_side.png")
    basket_left = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_basket_left.png")
    basket_world = get_guid("Assets/Sprites/NPCs/Modular/Villager/prop_villager_basket_world.png")

    new_villager_anim = f"""DirectionalWalkAnimator
  walkFrontFrames:
  - {sprite_ref(v_carry_walk_front[0])}
  - {sprite_ref(v_carry_walk_front[1])}
  - {sprite_ref(v_carry_walk_front[2])}
  - {sprite_ref(v_carry_walk_front[3])}
  walkBackFrames:
  - {sprite_ref(v_carry_walk_back[0])}
  - {sprite_ref(v_carry_walk_back[1])}
  - {sprite_ref(v_carry_walk_back[2])}
  - {sprite_ref(v_carry_walk_back[3])}
  walkSideFrames:
  - {sprite_ref(v_carry_walk_side[0])}
  - {sprite_ref(v_carry_walk_side[1])}
  - {sprite_ref(v_carry_walk_side[2])}
  - {sprite_ref(v_carry_walk_side[3])}
  walkLeftFrames:
  - {sprite_ref(v_carry_walk_left[0])}
  - {sprite_ref(v_carry_walk_left[1])}
  - {sprite_ref(v_carry_walk_left[2])}
  - {sprite_ref(v_carry_walk_left[3])}
  idleFront: {sprite_ref(v_carry_idle_front)}
  idleBack: {sprite_ref(v_carry_idle_back)}
  idleSide: {sprite_ref(v_carry_idle_side)}
  idleLeft: {sprite_ref(v_carry_idle_left)}"""

    v_content = re.sub(old_anim_pattern, new_villager_anim, v_content)

    # Update Prop_StrawHat and Prop_FruitBasket NPCModularProp blocks
    matches = list(re.finditer(r"MonoBehaviour:\n\s*m_ObjectHideFlags: 0[\s\S]*?Assembly-CSharp::ActionRPG\.Town\.NPCModularProp[\s\S]*?(?=\n---|\Z)", v_content))
    if len(matches) == 2:
        # First is Prop_StrawHat (6125393925006624397)
        hat_block = matches[0].group(0)
        header_hat = hat_block.split("EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp")[0] + "EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.NPCModularProp"
        new_hat_prop = f"""{header_hat}
  spriteFront: {sprite_ref(hat_front)}
  spriteBack: {sprite_ref(hat_back)}
  spriteSide: {sprite_ref(hat_side)}
  spriteLeft: {sprite_ref(hat_left)}
  sortingOffsetFront: 2
  sortingOffsetBack: 2
  sortingOffsetSideRight: 2
  sortingOffsetSideLeft: 2
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

        # Second is Prop_FruitBasket (2537834651225379883)
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
  sortingOffsetSideLeft: 1
  offsetFront: {{x: 0, y: 0}}
  offsetBack: {{x: 0, y: 0}}
  offsetSide: {{x: 0, y: 0}}
  offsetLeft: {{x: 0, y: 0}}
  syncWithWalkStepBob: 1
  hasSecondaryMotion: 1
  motionAmplitudeX: 0.015
  motionAmplitudeY: 0.02
  motionFrequency: 7
  bounceOnlyWhenMoving: 1
  sr: {{fileID: 0}}"""

        v_content = v_content.replace(hat_block, new_hat_prop).replace(basket_block, new_basket_prop)

    # Add VillagerPropInteract component to NPC_Villager_Wanderer root GameObject if not present
    if VILLAGER_INTERACT_GUID not in v_content:
        v_content = v_content.replace(
            "  - component: {fileID: 6172833093988749445}\n  - component: {fileID: 6085279566136645447}",
            "  - component: {fileID: 6172833093988749445}\n  - component: {fileID: 6172833093988749446}\n  - component: {fileID: 6085279566136645447}",
        )
        villager_interact_yaml = f"""--- !u!114 &6172833093988749446
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 7608856207728251541}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {VILLAGER_INTERACT_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::ActionRPG.Town.VillagerPropInteract
  strawHatProp: {{fileID: 6125393925006624397}}
  fruitBasketProp: {{fileID: 2537834651225379883}}
  worldHatSprite: {sprite_ref(hat_world)}
  worldBasketSprite: {sprite_ref(basket_world)}
  carryIdleFront: {sprite_ref(v_carry_idle_front)}
  carryIdleBack: {sprite_ref(v_carry_idle_back)}
  carryIdleSide: {sprite_ref(v_carry_idle_side)}
  carryIdleLeft: {sprite_ref(v_carry_idle_left)}
  carryWalkFrontFrames:
  - {sprite_ref(v_carry_walk_front[0])}
  - {sprite_ref(v_carry_walk_front[1])}
  - {sprite_ref(v_carry_walk_front[2])}
  - {sprite_ref(v_carry_walk_front[3])}
  carryWalkBackFrames:
  - {sprite_ref(v_carry_walk_back[0])}
  - {sprite_ref(v_carry_walk_back[1])}
  - {sprite_ref(v_carry_walk_back[2])}
  - {sprite_ref(v_carry_walk_back[3])}
  carryWalkSideFrames:
  - {sprite_ref(v_carry_walk_side[0])}
  - {sprite_ref(v_carry_walk_side[1])}
  - {sprite_ref(v_carry_walk_side[2])}
  - {sprite_ref(v_carry_walk_side[3])}
  carryWalkLeftFrames:
  - {sprite_ref(v_carry_walk_left[0])}
  - {sprite_ref(v_carry_walk_left[1])}
  - {sprite_ref(v_carry_walk_left[2])}
  - {sprite_ref(v_carry_walk_left[3])}
  unarmedIdleFront: {sprite_ref(v_unarmed_idle_front)}
  unarmedIdleBack: {sprite_ref(v_unarmed_idle_back)}
  unarmedIdleSide: {sprite_ref(v_unarmed_idle_side)}
  unarmedIdleLeft: {sprite_ref(v_unarmed_idle_left)}
  unarmedWalkFrontFrames:
  - {sprite_ref(v_unarmed_walk_front[0])}
  - {sprite_ref(v_unarmed_walk_front[1])}
  - {sprite_ref(v_unarmed_walk_front[2])}
  - {sprite_ref(v_unarmed_walk_front[3])}
  unarmedWalkBackFrames:
  - {sprite_ref(v_unarmed_walk_back[0])}
  - {sprite_ref(v_unarmed_walk_back[1])}
  - {sprite_ref(v_unarmed_walk_back[2])}
  - {sprite_ref(v_unarmed_walk_back[3])}
  unarmedWalkSideFrames:
  - {sprite_ref(v_unarmed_walk_side[0])}
  - {sprite_ref(v_unarmed_walk_side[1])}
  - {sprite_ref(v_unarmed_walk_side[2])}
  - {sprite_ref(v_unarmed_walk_side[3])}
  unarmedWalkLeftFrames:
  - {sprite_ref(v_unarmed_walk_left[0])}
  - {sprite_ref(v_unarmed_walk_left[1])}
  - {sprite_ref(v_unarmed_walk_left[2])}
  - {sprite_ref(v_unarmed_walk_left[3])}
  basketGroundOffset: {{x: 0.32, y: -0.14, z: 0}}
  hatGroundOffset: {{x: -0.32, y: -0.16, z: 0}}
  isWearingHat: 1
  isCarryingBasket: 1
  enableAutonomousRoutine: 1
  routineInterval: 10
"""
        v_content = v_content.rstrip() + "\n" + villager_interact_yaml

    with open(villager_path, "w", encoding="utf-8") as f:
        f.write(v_content)
    print("NPC_Villager_Wanderer.prefab updated!")


if __name__ == "__main__":
    update_part3_prefabs()
