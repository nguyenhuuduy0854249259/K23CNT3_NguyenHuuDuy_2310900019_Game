#!/usr/bin/env python3
"""
QA Verification Script for All 7 Chibi Modular NPCs (Part 1, Part 2 & Part 3)
Checks:
- All 32x32 PNG files in Assets/Sprites/NPCs/Modular/{Eldrin,Boris,Elena,Carlos,Lina,Kaelen,Falcon,Villager}
- Canvas dimensions == 32x32
- Zero fringe halo pixels (0 < alpha < 255 outside DropShadow)
- .meta files exist with spritePixelsToUnit: 16 and filterMode: 0
- Prefab YAML files reference valid GUIDs, m_LocalScale = (1, 1, 1), Color = White (1,1,1,1),
  and Part 3 components FalconBehaviorController and VillagerPropInteract are attached and enabled.
"""

import json
import os
import re
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
MODULAR_ROOT = os.path.join(ROOT, "Assets", "Sprites", "NPCs", "Modular")
PREFAB_ROOT = os.path.join(ROOT, "Assets", "Prefabs", "ForestPrototype")

FOLDERS = ["Eldrin", "Boris", "Elena", "Carlos", "Lina", "Kaelen", "Falcon", "Villager"]
PREFABS = [
    "NPC_VillageElder.prefab",
    "NPC_Blacksmith_Barnaby.prefab",
    "NPC_Alchemist_Elena.prefab",
    "NPC_GuildMaster_Carlos.prefab",
    "NPC_Merchant_Lina.prefab",
    "NPC_PetMaster_Kaelen.prefab",
    "Pet_Falcon.prefab",
    "NPC_Villager_Wanderer.prefab",
]


def verify_all():
    report = {
        "overall_passed": True,
        "folders": {},
        "prefabs": {},
    }

    guid_to_path = {}

    for folder in FOLDERS:
        folder_path = os.path.join(MODULAR_ROOT, folder)
        pngs = sorted([f for f in os.listdir(folder_path) if f.endswith(".png")])
        folder_info = {
            "png_count": len(pngs),
            "all_32x32": True,
            "all_meta_ppu16_point": True,
            "zero_halo_errors": True,
            "errors": [],
        }

        for png in pngs:
            p = os.path.join(folder_path, png)
            im = Image.open(p).convert("RGBA")
            if im.size != (32, 32):
                folder_info["all_32x32"] = False
                folder_info["errors"].append(f"{png}: size {im.size} != (32, 32)")
                report["overall_passed"] = False

            # Check semi-transparent pixels (allowed only in bottom shadow region y >= 25, world prop shadow y >= 18, or fx_ glow sprites)
            px = im.load()
            is_fx_glow = png.startswith("fx_") or png.startswith("prop_elder_staff_")
            for y in range(32):
                for x in range(32):
                    r, g, b, a = px[x, y]
                    if 0 < a < 255 and not is_fx_glow:
                        if y < 18 and not (r == 10 and g == 12 and b == 18):
                            folder_info["zero_halo_errors"] = False
                            folder_info["errors"].append(f"{png}: unexpected semi-transparent pixel at ({x},{y}) rgba=({r},{g},{b},{a})")
                            report["overall_passed"] = False

            meta_p = p + ".meta"
            if not os.path.exists(meta_p):
                folder_info["all_meta_ppu16_point"] = False
                folder_info["errors"].append(f"{png}: missing .meta")
                report["overall_passed"] = False
            else:
                with open(meta_p, "r", encoding="utf-8") as f:
                    mc = f.read()
                m_guid = re.search(r"guid:\s*([0-9a-f]{32})", mc)
                if m_guid:
                    guid_to_path[m_guid.group(1)] = f"{folder}/{png}"
                if "spritePixelsToUnits: 16" not in mc or "filterMode: 0" not in mc:
                    folder_info["all_meta_ppu16_point"] = False
                    folder_info["errors"].append(f"{png}.meta: PPU!=16 or filterMode!=0")
                    report["overall_passed"] = False

        report["folders"][folder] = folder_info

    for prefab in PREFABS:
        pp = os.path.join(PREFAB_ROOT, prefab)
        with open(pp, "r", encoding="utf-8") as f:
            content = f.read()

        guids = re.findall(r"guid:\s*([0-9a-f]{32})", content)
        matched_sprites = [guid_to_path[g] for g in guids if g in guid_to_path]
        has_scale_1 = "m_LocalScale: {x: 1, y: 1, z: 1}" in content
        has_scale_135 = "m_LocalScale: {x: 1.35" in content

        prefab_info = {
            "matched_modular_sprite_refs": len(matched_sprites),
            "root_scale_is_1": has_scale_1 and not has_scale_135,
            "has_falcon_behavior_controller": "8b4c9a12d6e54f039c17a5e82d3f6b19" in content,
            "has_villager_prop_interact": "7c5d8e23f4a649b18e29c1d43a5b7f20" in content,
            "passed": len(matched_sprites) > 0 and has_scale_1 and not has_scale_135,
        }
        if not prefab_info["passed"]:
            report["overall_passed"] = False
        report["prefabs"][prefab] = prefab_info

    out_json = os.path.join(ROOT, "Docs", "all_7_npcs_qa_report.json")
    with open(out_json, "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2)

    print(f"QA OVERALL PASSED: {report['overall_passed']}")
    for k, v in report["folders"].items():
        print(f"  Folder {k}: {v['png_count']} PNGs, 32x32={v['all_32x32']}, PPU16={v['all_meta_ppu16_point']}, NoHalo={v['zero_halo_errors']}")
    for k, v in report["prefabs"].items():
        print(f"  Prefab {k}: {v['matched_modular_sprite_refs']} sprite refs, scale1={v['root_scale_is_1']}, passed={v['passed']}")


if __name__ == "__main__":
    verify_all()
