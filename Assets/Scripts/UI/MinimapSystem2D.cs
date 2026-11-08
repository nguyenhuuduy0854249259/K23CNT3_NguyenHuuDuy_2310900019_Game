using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Characters;

namespace ActionRPG.UI
{
    /// <summary>
    /// Hệ Thống Bản Đồ Nhỏ (Minimap 2D) Góc Trên Bên Phải của Canvas (Kích thước 170x170 px).
    /// Tự động sinh RenderTexture bằng code và điều khiển Minimap_Camera chiếu Orthographic theo sát Player.
    /// Có chấm tròn Xanh Sáng ở giữa biểu thị nhân vật chính, nút [+] và [-] phóng to/thu nhỏ tầm nhìn bản đồ.
    /// </summary>
    public class MinimapSystem2D : MonoBehaviour
    {
        public static MinimapSystem2D Instance { get; private set; }

        [Header("Minimap UI References")]
        public RectTransform minimapFrame;
        public RawImage minimapRawImage;
        public Image playerMarkerDot;
        public TMP_Text locationTitleText;
        public Button zoomInButton;
        public Button zoomOutButton;

        [Header("Minimap Camera Settings")]
        public Camera minimapCamera;
        public RenderTexture minimapRenderTexture;
        public float currentZoom = 14f;
        public float minZoom = 8f;
        public float maxZoom = 28f;
        public float zoomStep = 3f;

        [Header("Player Target")]
        public Transform playerTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            EnsureUI(transform.parent != null ? transform.parent : transform);
            InitMinimapCamera();
        }

        private void Start()
        {
            FindPlayerTarget();
        }

        private void LateUpdate()
        {
            if (playerTransform == null)
            {
                FindPlayerTarget();
            }

            if (playerTransform != null && minimapCamera != null)
            {
                Vector3 targetPos = playerTransform.position;
                minimapCamera.transform.position = new Vector3(targetPos.x, targetPos.y, -30f);
            }
        }

        private void OnDestroy()
        {
            if (minimapRenderTexture != null)
            {
                if (minimapCamera != null) minimapCamera.targetTexture = null;
                minimapRenderTexture.Release();
                Destroy(minimapRenderTexture);
                minimapRenderTexture = null;
            }

            if (minimapCamera != null && minimapCamera.gameObject != null)
            {
                Destroy(minimapCamera.gameObject);
                minimapCamera = null;
            }
        }

        public void FindPlayerTarget()
        {
            if (CharacterStats.PlayerInstance != null)
            {
                playerTransform = CharacterStats.PlayerInstance.transform;
                return;
            }

            var playerCtrl = FindObjectOfType<PlayerController>();
            if (playerCtrl != null)
            {
                playerTransform = playerCtrl.transform;
                return;
            }

            var pObj = GameObject.FindWithTag("Player");
            if (pObj != null)
            {
                playerTransform = pObj.transform;
            }
        }

        /// <summary>
        /// Khởi tạo RenderTexture và Camera chiếu từ trên cao xuống theo sát Player
        /// </summary>
        public void InitMinimapCamera()
        {
            if (minimapRenderTexture == null)
            {
                minimapRenderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                minimapRenderTexture.name = "Minimap_RenderTexture";
                minimapRenderTexture.filterMode = FilterMode.Point;
                minimapRenderTexture.Create();
            }

            if (minimapRawImage != null)
            {
                minimapRawImage.texture = minimapRenderTexture;
            }

            if (minimapCamera == null)
            {
                GameObject camObj = new GameObject("Minimap_Camera");
                minimapCamera = camObj.AddComponent<Camera>();
                minimapCamera.orthographic = true;
                minimapCamera.orthographicSize = currentZoom;
                minimapCamera.clearFlags = CameraClearFlags.SolidColor;
                minimapCamera.backgroundColor = new Color(0.12f, 0.17f, 0.14f, 1f); // Nền màu cỏ râm dịu mắt
                minimapCamera.nearClipPlane = 0.1f;
                minimapCamera.farClipPlane = 100f;
                minimapCamera.depth = -10;
                
                // Loại trừ UI Layer (Layer 5) để không tự soi lại chính Canvas UI
                minimapCamera.cullingMask = ~(1 << 5);

                minimapCamera.targetTexture = minimapRenderTexture;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(camObj);
                }
            }
        }

        public void ZoomIn()
        {
            currentZoom = Mathf.Clamp(currentZoom - zoomStep, minZoom, maxZoom);
            if (minimapCamera != null)
            {
                minimapCamera.orthographicSize = currentZoom;
            }
        }

        public void ZoomOut()
        {
            currentZoom = Mathf.Clamp(currentZoom + zoomStep, minZoom, maxZoom);
            if (minimapCamera != null)
            {
                minimapCamera.orthographicSize = currentZoom;
            }
        }

        /// <summary>
        /// Tự động sinh giao diện Procedural UI cho Minimap neo ở góc trên bên phải (Top-Right)
        /// Kích thước chuẩn 170x170 px kèm tiêu đề nhỏ bên dưới: "Làng Oakhaven"
        /// </summary>
        public static MinimapSystem2D EnsureUI(Transform parent)
        {
            if (parent == null) return Instance;

            Transform existing = (parent.name == "MinimapSystem2D_Frame") ? parent : parent.Find("MinimapSystem2D_Frame");
            if (existing != null)
            {
                var comp = existing.GetComponent<MinimapSystem2D>();
                if (comp != null)
                {
                    Instance = comp;
                    var existingRt = existing.GetComponent<RectTransform>();
                    if (existingRt != null)
                    {
                        existingRt.anchorMin = new Vector2(1f, 1f);
                        existingRt.anchorMax = new Vector2(1f, 1f);
                        existingRt.pivot = new Vector2(1f, 1f);
                        existingRt.anchoredPosition = new Vector2(-16f, -16f);
                        existingRt.sizeDelta = new Vector2(170f, 170f);
                    }
                    FontManager.SanitizeUIHierarchy(existing);
                    return comp;
                }
            }

            // 1. Khung Minimap neo ở Góc Trên Bên Phải (Top-Right), cách lề trên và lề phải đúng 16px
            GameObject frameGo = new GameObject("MinimapSystem2D_Frame", typeof(RectTransform));
            frameGo.transform.SetParent(parent, false);
            var frameRt = frameGo.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(1f, 1f);
            frameRt.anchorMax = new Vector2(1f, 1f);
            frameRt.pivot = new Vector2(1f, 1f);
            frameRt.anchoredPosition = new Vector2(-16f, -16f);
            frameRt.sizeDelta = new Vector2(170f, 170f);

            var frameBg = frameGo.AddComponent<Image>();
            frameBg.color = new Color(0.06f, 0.08f, 0.11f, 0.98f); // Nền tối huyền bí

            // Viền vàng kim pixel art sắc nét
            var frameOutline = frameGo.AddComponent<Outline>();
            frameOutline.effectColor = new Color(0.85f, 0.70f, 0.25f, 1f);
            frameOutline.effectDistance = new Vector2(2f, -2f);

            var compInstance = frameGo.AddComponent<MinimapSystem2D>();
            compInstance.minimapFrame = frameRt;
            Instance = compInstance;

            // 2. RawImage hiển thị RenderTexture bên trong khung
            RectTransform rawRt = CreateRect("Minimap_RawImage", frameRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rawRt.offsetMin = new Vector2(3f, 3f);
            rawRt.offsetMax = new Vector2(-3f, -3f);
            var rawImg = rawRt.gameObject.AddComponent<RawImage>();
            rawImg.color = Color.white;
            compInstance.minimapRawImage = rawImg;

            // 3. Chấm tròn màu Xanh Sáng ở chính giữa Minimap (Vị trí Nhân vật chính)
            RectTransform dotRt = CreateRect("Player_Marker_Dot", rawRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 10f));
            var dotImg = dotRt.gameObject.AddComponent<Image>();
            dotImg.color = new Color(0.2f, 1f, 0.45f, 1f); // Xanh neon sáng rực

            var dotOutline = dotRt.gameObject.AddComponent<Outline>();
            dotOutline.effectColor = new Color(1f, 1f, 1f, 0.95f); // Viền trắng nổi bật
            dotOutline.effectDistance = new Vector2(1f, -1f);
            compInstance.playerMarkerDot = dotImg;

            // 4. Nút Phóng to [+] và Thu nhỏ [-] ở cạnh trên bên phải Minimap
            RectTransform zoomBtnGroup = CreateRect("Zoom_Controls", frameRt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-6f, -6f), new Vector2(50f, 24f));
            
            // Nút [-]
            RectTransform zoomOutRt = CreateRect("Btn_ZoomOut", zoomBtnGroup, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(22f, 22f));
            var zoomOutBg = zoomOutRt.gameObject.AddComponent<Image>();
            zoomOutBg.color = new Color(0.12f, 0.15f, 0.2f, 0.9f);
            var zoomOutOutline = zoomOutRt.gameObject.AddComponent<Outline>();
            zoomOutOutline.effectColor = new Color(0.7f, 0.6f, 0.25f, 0.8f);
            zoomOutOutline.effectDistance = new Vector2(1f, -1f);
            var zoomOutBtn = zoomOutRt.gameObject.AddComponent<Button>();
            var zoomOutTxt = CreateText("Txt", zoomOutRt, "-", 15f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            zoomOutTxt.rectTransform.anchorMin = Vector2.zero;
            zoomOutTxt.rectTransform.anchorMax = Vector2.one;
            zoomOutTxt.rectTransform.offsetMin = zoomOutTxt.rectTransform.offsetMax = Vector2.zero;
            zoomOutBtn.onClick.AddListener(() => compInstance.ZoomOut());
            compInstance.zoomOutButton = zoomOutBtn;

            // Nút [+]
            RectTransform zoomInRt = CreateRect("Btn_ZoomIn", zoomBtnGroup, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(22f, 22f));
            var zoomInBg = zoomInRt.gameObject.AddComponent<Image>();
            zoomInBg.color = new Color(0.12f, 0.15f, 0.2f, 0.9f);
            var zoomInOutline = zoomInRt.gameObject.AddComponent<Outline>();
            zoomInOutline.effectColor = new Color(0.7f, 0.6f, 0.25f, 0.8f);
            zoomInOutline.effectDistance = new Vector2(1f, -1f);
            var zoomInBtn = zoomInRt.gameObject.AddComponent<Button>();
            var zoomInTxt = CreateText("Txt", zoomInRt, "+", 15f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            zoomInTxt.rectTransform.anchorMin = Vector2.zero;
            zoomInTxt.rectTransform.anchorMax = Vector2.one;
            zoomInTxt.rectTransform.offsetMin = zoomInTxt.rectTransform.offsetMax = Vector2.zero;
            zoomInBtn.onClick.AddListener(() => compInstance.ZoomIn());
            compInstance.zoomInButton = zoomInBtn;

            // 5. Khung Tiêu Đề bên dưới Minimap: "Làng Oakhaven"
            RectTransform titleBoxRt = CreateRect("Title_Box", frameRt, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(0f, 22f));
            var titleBg = titleBoxRt.gameObject.AddComponent<Image>();
            titleBg.color = new Color(0.08f, 0.10f, 0.14f, 0.92f);
            var titleOutline = titleBoxRt.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0.6f, 0.5f, 0.2f, 0.7f);
            titleOutline.effectDistance = new Vector2(1f, -1f);

            var titleTxt = CreateText("Txt_Location", titleBoxRt, "Làng Oakhaven", 11f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.92f, 0.75f));
            titleTxt.rectTransform.anchorMin = Vector2.zero;
            titleTxt.rectTransform.anchorMax = Vector2.one;
            titleTxt.rectTransform.offsetMin = titleTxt.rectTransform.offsetMax = Vector2.zero;
            compInstance.locationTitleText = titleTxt;

            compInstance.InitMinimapCamera();
            compInstance.FindPlayerTarget();

            return compInstance;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private static TMP_Text CreateText(string name, Transform parent, string content, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            FontManager.ApplyFont(tmp);
            return tmp;
        }
    }
}
