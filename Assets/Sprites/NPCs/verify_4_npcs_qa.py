"""
Fast Comprehensive QA Verification Script for Part 2/3: 4 NPCs
"""

import os
import re
from PIL import Image

def verify():
    # 1. Build GUID index in one pass
    guid_to_path = {}
    print("Indexing Assets meta files...")
    for root, _, files in os.walk("Assets"):
        for file in files:
            if file.endswith(".meta"):
                mp = os.path.join(root, file)
                try:
                    with open(mp, "r", encoding="utf-8") as mf:
                        for line in mf:
                            if "guid:" in line:
                                m = re.search(r"guid:\s*([a-f0-9]{32})", line)
                                if m:
                                    guid_to_path[m.group(1)] = mp[:-5] # remove .meta
                                break
                except Exception:
                    pass

    print(f"Indexed {len(guid_to_path)} asset GUIDs.")

    report = []
    has_error = False

    npcs = [
        ("Boris", "Assets/Prefabs/ForestPrototype/NPC_Blacksmith_Barnaby.prefab", ["Prop_Hammer"]),
        ("Elena", "Assets/Prefabs/ForestPrototype/NPC_Alchemist_Elena.prefab", ["Prop_HerbBag"]),
        ("Carlos", "Assets/Prefabs/ForestPrototype/NPC_GuildMaster_Carlos.prefab", ["Prop_CrimsonCape", "Prop_KnightSword"]),
        ("Lina", "Assets/Prefabs/ForestPrototype/NPC_Merchant_Lina.prefab", ["Prop_Basket", "Prop_CoinPouch"]),
    ]

    for name, prefab_path, prop_names in npcs:
        report.append(f"\n=== NPC: {name} ===")
        if not os.path.exists(prefab_path):
            report.append(f"  [ERROR] Prefab not found: {prefab_path}")
            has_error = True
            continue
        report.append(f"  [OK] Prefab exists: {prefab_path}")

        with open(prefab_path, "r", encoding="utf-8") as f:
            prefab_text = f.read()

        for p in prop_names:
            if f"m_Name: {p}" in prefab_text:
                report.append(f"  [OK] Child Prop GameObject found: {p}")
            else:
                report.append(f"  [ERROR] Missing Child Prop GameObject: {p}")
                has_error = True

        guids = re.findall(r"guid:\s*([a-f0-9]{32})", prefab_text)
        checked_sprites = 0
        for guid in set(guids):
            asset_path = guid_to_path.get(guid)
            if not asset_path:
                continue
            if asset_path.endswith(".png"):
                checked_sprites += 1
                if not os.path.exists(asset_path):
                    report.append(f"  [ERROR] Missing PNG: {asset_path}")
                    has_error = True
                else:
                    with Image.open(asset_path) as im:
                        if im.size != (32, 32):
                            report.append(f"  [ERROR] Invalid size {im.size} for {asset_path}")
                            has_error = True
                    meta_path = asset_path + ".meta"
                    with open(meta_path, "r", encoding="utf-8") as mf:
                        mc = mf.read()
                        if "spritePixelsToUnits: 16" not in mc:
                            report.append(f"  [ERROR] PPU is not 16 in {meta_path}")
                            has_error = True
                        if "filterMode: 0" not in mc:
                            report.append(f"  [ERROR] FilterMode is not Point (0) in {meta_path}")
                            has_error = True

        report.append(f"  [OK] Verified {checked_sprites} referenced sprites (32x32, PPU=16, FilterMode=Point)")

    print("\n".join(report))
    if has_error:
        print("\nQA RESULT: FAILED with errors.")
        exit(1)
    else:
        print("\nQA RESULT: 100% PASSED!")

if __name__ == "__main__":
    verify()
