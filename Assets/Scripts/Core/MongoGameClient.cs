using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ActionRPG.Core
{
    /// <summary>
    /// Client kết nối trực tiếp từ Unity tới REST API Node.js & MongoDB Server.
    /// Hỗ trợ lưu trữ tiến trình đám mây (Cloud Save) và tải nội dung game mở (Open Game Content).
    /// </summary>
    public class MongoGameClient : MonoBehaviour
    {
        public static MongoGameClient Instance { get; private set; }

        [Header("Cấu Hình Kết Nối API MongoDB")]
        [SerializeField] private string baseUrl = "http://localhost:3000";
        [SerializeField] private float timeoutSeconds = 8f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái hoạt động của Backend & MongoDB (Health Check)
        /// </summary>
        public void CheckServerHealth(Action<bool, string> callback)
        {
            StartCoroutine(GetCoroutine($"{baseUrl}/health", (success, text) =>
            {
                callback?.Invoke(success, text);
            }));
        }

        /// <summary>
        /// Lưu hồ sơ nhân vật lên MongoDB thông qua API
        /// </summary>
        public void SavePlayerToDatabase(PlayerSaveData data, Action<bool, string> onComplete)
        {
            if (data == null)
            {
                onComplete?.Invoke(false, "PlayerSaveData rỗng.");
                return;
            }

            string playerId = string.IsNullOrEmpty(data.playerId) ? "player_001" : data.playerId;
            string url = $"{baseUrl}/api/players/{playerId}/save";
            string jsonBody = JsonUtility.ToJson(data);

            StartCoroutine(PutJsonCoroutine(url, jsonBody, (success, responseText) =>
            {
                if (success)
                {
                    Debug.Log($"[MongoGameClient] Đã lưu thành công cho {playerId} lên MongoDB!");
                    onComplete?.Invoke(true, responseText);
                }
                else
                {
                    Debug.LogError($"[MongoGameClient] Lưu dữ liệu thất bại: {responseText}");
                    onComplete?.Invoke(false, responseText);
                }
            }));
        }

        /// <summary>
        /// Tải dữ liệu lưu của người chơi từ MongoDB về game
        /// </summary>
        public void LoadPlayerFromDatabase(string playerId, Action<PlayerSaveData, string> onComplete)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = "player_001";
            }

            string url = $"{baseUrl}/api/players/{playerId}/save";
            StartCoroutine(GetCoroutine(url, (success, responseText) =>
            {
                if (success)
                {
                    try
                    {
                        PlayerSaveData loadedData = JsonUtility.FromJson<PlayerSaveData>(responseText);
                        Debug.Log($"[MongoGameClient] Tải thành công dữ liệu MongoDB cho {playerId}!");
                        onComplete?.Invoke(loadedData, null);
                    }
                    catch (Exception ex)
                    {
                        onComplete?.Invoke(null, $"Parse JSON thất bại: {ex.Message}");
                    }
                }
                else
                {
                    onComplete?.Invoke(null, responseText);
                }
            }));
        }

        /// <summary>
        /// Tải danh sách nội dung game mở (classes, skills, items, pets, quests,...) từ MongoDB
        /// </summary>
        public void FetchGameContent(string contentType, Action<bool, string> onComplete)
        {
            string url = $"{baseUrl}/api/content/{contentType}";
            StartCoroutine(GetCoroutine(url, (success, responseText) =>
            {
                onComplete?.Invoke(success, responseText);
            }));
        }

        private IEnumerator GetCoroutine(string url, Action<bool, string> callback)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.timeout = Mathf.RoundToInt(timeoutSeconds);
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, req.downloadHandler.text);
                }
                else
                {
                    callback?.Invoke(false, req.error ?? "Yêu cầu thất bại");
                }
            }
        }

        private IEnumerator PutJsonCoroutine(string url, string jsonBody, Action<bool, string> callback)
        {
            using (UnityWebRequest req = new UnityWebRequest(url, "PUT"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = Mathf.RoundToInt(timeoutSeconds);

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, req.downloadHandler.text);
                }
                else
                {
                    callback?.Invoke(false, req.error ?? "Lỗi PUT JSON");
                }
            }
        }
    }
}
