using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SizeComparisonTest
{
    public static void CheckSizes()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);
        GameObject player = GameObject.Find("Player_Novice_Tier1");
        GameObject boris = GameObject.Find("NPC_ThoRen_Boris");
        GameObject eldrin = GameObject.Find("NPC_TruongLang_Eldrin");

        if (player != null)
        {
            var sr = player.GetComponent<SpriteRenderer>();
            Debug.Log($"[SIZE TEST] PLAYER: localScale={player.transform.localScale}, SR.bounds.size={sr.bounds.size}, Sprite.rect={sr.sprite.rect}, Sprite.PPU={sr.sprite.pixelsPerUnit}");
        }
        else
        {
            Debug.Log("[SIZE TEST] PLAYER NOT FOUND");
        }

        if (boris != null)
        {
            var sr = boris.GetComponent<SpriteRenderer>();
            Debug.Log($"[SIZE TEST] BORIS: localScale={boris.transform.localScale}, SR.bounds.size={sr.bounds.size}, Sprite.rect={sr.sprite.rect}, Sprite.PPU={sr.sprite.pixelsPerUnit}");
        }

        if (eldrin != null)
        {
            var sr = eldrin.GetComponent<SpriteRenderer>();
            Debug.Log($"[SIZE TEST] ELDRIN: localScale={eldrin.transform.localScale}, SR.bounds.size={sr.bounds.size}, Sprite.rect={sr.sprite.rect}, Sprite.PPU={sr.sprite.pixelsPerUnit}");
        }
    }
}
