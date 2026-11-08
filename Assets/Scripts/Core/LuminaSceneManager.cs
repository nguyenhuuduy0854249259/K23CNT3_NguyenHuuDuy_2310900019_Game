using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ActionRPG.Core
{
    /// <summary>
    /// Hệ thống Quản Lý & Chuyển Đổi Scene / Map của Dự Án Lumina (Lumina Scene Manager):
    /// - Quản lý nạp scene mượt mà: MainGame (Map 1 & Map 2), Map3 (Map độc lập mới), Forest_Demo_Map.
    /// - Hỗ trợ bảo toàn vị trí SpawnPoint cho Player khi di chuyển qua Portal giữa các Scene.
    /// - Tích hợp cơ chế Fade-in / Fade-out và EventBus thông báo chuyển map.
    /// </summary>
    public class LuminaSceneManager : MonoBehaviour
    {
        public const string SceneMainGame = "MainGame";
        public const string SceneMap3 = "Map3";
        public const string SceneForestDemo = "Forest_Demo_Map";

        private static LuminaSceneManager _instance;
        public static LuminaSceneManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var existing = FindFirstObjectByType<LuminaSceneManager>();
                    if (existing != null)
                    {
                        _instance = existing;
                    }
                    else
                    {
                        GameObject go = new GameObject("LuminaSceneManager");
                        _instance = go.AddComponent<LuminaSceneManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public static Vector3? PendingSpawnPosition { get; set; } = null;
        public static bool IsTransitioning { get; private set; } = false;

        public static event Action<string> OnSceneTransitionStarted;
        public static event Action<string> OnSceneTransitionCompleted;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoadedInternal;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoadedInternal;
            }
        }

        /// <summary>
        /// Nạp Scene bất kỳ kèm tọa độ xuất hiện mong muốn của người chơi.
        /// </summary>
        public void LoadScene(string sceneName, Vector3? targetSpawnPosition = null)
        {
            if (IsTransitioning) return;
            StartCoroutine(TransitionRoutine(sceneName, targetSpawnPosition));
        }

        /// <summary>
        /// Nạp nhanh Map 3.
        /// </summary>
        public void LoadMap3(Vector3? targetSpawnPosition = null)
        {
            LoadScene(SceneMap3, targetSpawnPosition);
        }

        /// <summary>
        /// Quay về MainGame (Map 1 & Map 2).
        /// </summary>
        public void LoadMainGame(Vector3? targetSpawnPosition = null)
        {
            LoadScene(SceneMainGame, targetSpawnPosition);
        }

        private IEnumerator TransitionRoutine(string sceneName, Vector3? targetSpawnPosition)
        {
            IsTransitioning = true;
            PendingSpawnPosition = targetSpawnPosition;

            Debug.Log($"<color=cyan><b>[LuminaSceneManager] Bắt đầu chuyển Scene sang: {sceneName}</b></color>");
            OnSceneTransitionStarted?.Invoke(sceneName);

            // Chờ 0.15s cho UI fade nếu có
            yield return new WaitForSecondsRealtime(0.15f);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (asyncLoad != null)
            {
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            IsTransitioning = false;
        }

        private void OnSceneLoadedInternal(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"<color=green><b>[LuminaSceneManager] ✅ Đã nạp hoàn tất Scene: {scene.name}</b></color>");

            if (PendingSpawnPosition.HasValue)
            {
                Vector3 spawnPos = PendingSpawnPosition.Value;
                PendingSpawnPosition = null;

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = spawnPos;
                    Debug.Log($"[LuminaSceneManager] Đã đặt vị trí người chơi tại: {spawnPos}");
                }
            }

            OnSceneTransitionCompleted?.Invoke(scene.name);
        }
    }
}

