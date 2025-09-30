using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Match3;

namespace Match3
{
    /// <summary>
    /// 食材飛行動畫系統 - 負責食材從棋盤飛到餐盤的動畫
    /// </summary>
    public class IngredientFlyAnimation : MonoBehaviour
    {
        [Header("動畫設定")]
        public float flyDuration = 1.0f;                    // 飛行持續時間
        public AnimationCurve flyCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 飛行曲線
        public float arcHeight = 100f;                      // 弧形高度
        public Vector2 iconSize = new Vector2(50, 50);      // 飛行圖示大小

        [Header("效果設定")]
        public bool enableTrailEffect = true;               // 是否啟用拖尾效果
        public float fadeOutDuration = 0.3f;               // 淡出持續時間
        public float scaleUpFactor = 1.2f;                 // 起始放大倍數

        [Header("UI 設定")]
        public Canvas flyAnimationCanvas;                   // 用於動畫的畫布
        public GameObject flyIconPrefab;                    // 飛行圖示預製體

        [Header("設定")]
        public bool enableDebugLog = true;                  // 是否啟用調試日誌

        // 靜態實例（方便全域調用）
        public static IngredientFlyAnimation Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticData()
        {
            Instance = null;
        }

        // 動畫管理
        private int activeAnimations = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 確保有動畫畫布
            if (flyAnimationCanvas == null)
            {
                flyAnimationCanvas = FindFirstObjectByType<Canvas>();
                if (flyAnimationCanvas == null)
                {
                    DebugLog("警告：找不到 Canvas，將創建新的 Canvas");
                    CreateAnimationCanvas();
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 創建動畫畫布
        /// </summary>
        private void CreateAnimationCanvas()
        {
            GameObject canvasObj = new GameObject("FlyAnimationCanvas");
            flyAnimationCanvas = canvasObj.AddComponent<Canvas>();
            flyAnimationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            flyAnimationCanvas.sortingOrder = 1000; // 確保在最上層

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 播放飛行動畫
        /// </summary>
        /// <param name="startPos">起始位置（世界座標）</param>
        /// <param name="endPos">結束位置（世界座標）</param>
        /// <param name="icon">飛行圖示</param>
        /// <param name="onComplete">完成回調</param>
        public static void PlayFlyAnimation(Vector3 startPos, Vector3 endPos, Sprite icon, Action onComplete = null)
        {
            if (Instance == null)
            {
                Debug.LogError("IngredientFlyAnimation Instance 不存在！");
                onComplete?.Invoke();
                return;
            }

            Instance.StartFlyAnimation(startPos, endPos, icon, onComplete);
        }

        /// <summary>
        /// 開始飛行動畫
        /// </summary>
        private void StartFlyAnimation(Vector3 startWorldPos, Vector3 endWorldPos, Sprite icon, Action onComplete)
        {
            DebugLog($"開始飛行動畫: {startWorldPos} -> {endWorldPos}");

            // 轉換為螢幕座標
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                DebugLog("錯誤：找不到攝影機");
                onComplete?.Invoke();
                return;
            }

            Vector3 startScreenPos = camera.WorldToScreenPoint(startWorldPos);
            Vector3 endScreenPos = camera.WorldToScreenPoint(endWorldPos);

            // 創建飛行圖示
            GameObject flyIcon = CreateFlyIcon(icon, startScreenPos);
            if (flyIcon == null)
            {
                onComplete?.Invoke();
                return;
            }

            // 開始動畫
            StartCoroutine(FlyAnimationCoroutine(flyIcon, startScreenPos, endScreenPos, onComplete));
        }

        /// <summary>
        /// 創建飛行圖示
        /// </summary>
        private GameObject CreateFlyIcon(Sprite icon, Vector3 startScreenPos)
        {
            GameObject flyIcon;

            if (flyIconPrefab != null)
            {
                flyIcon = Instantiate(flyIconPrefab, flyAnimationCanvas.transform);
            }
            else
            {
                // 創建簡單的飛行圖示
                flyIcon = new GameObject("FlyIcon");
                flyIcon.transform.SetParent(flyAnimationCanvas.transform);

                Image imageComponent = flyIcon.AddComponent<Image>();
                imageComponent.sprite = icon;
                imageComponent.preserveAspect = true;
            }

            // 設定圖示
            RectTransform rectTransform = flyIcon.GetComponent<RectTransform>();
            rectTransform.sizeDelta = iconSize;
            rectTransform.position = startScreenPos;
            rectTransform.localScale = Vector3.one * scaleUpFactor;

            return flyIcon;
        }

        /// <summary>
        /// 飛行動畫協程
        /// </summary>
        private IEnumerator FlyAnimationCoroutine(GameObject flyIcon, Vector3 startPos, Vector3 endPos, Action onComplete)
        {
            activeAnimations++;

            RectTransform rectTransform = flyIcon.GetComponent<RectTransform>();
            Image imageComponent = flyIcon.GetComponent<Image>();

            float elapsedTime = 0f;
            Vector3 startScale = rectTransform.localScale;
            Vector3 endScale = Vector3.one;

            // 主飛行動畫
            while (elapsedTime < flyDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / flyDuration;
                float curveValue = flyCurve.Evaluate(progress);

                // 計算弧形路徑
                Vector3 currentPos = CalculateArcPosition(startPos, endPos, curveValue);
                rectTransform.position = currentPos;

                // 縮放動畫
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, progress);

                // 旋轉效果（可選）
                rectTransform.rotation = Quaternion.Euler(0, 0, progress * 360f);

                yield return null;
            }

            // 確保到達終點
            rectTransform.position = endPos;
            rectTransform.localScale = endScale;

            // 到達動畫（放大縮小效果）
            yield return StartCoroutine(PlayArrivalEffect(rectTransform, imageComponent));

            // 清理
            Destroy(flyIcon);
            activeAnimations--;

            // 調用完成回調
            onComplete?.Invoke();

            DebugLog("飛行動畫完成");
        }

        /// <summary>
        /// 計算弧形位置
        /// </summary>
        private Vector3 CalculateArcPosition(Vector3 startPos, Vector3 endPos, float progress)
        {
            Vector3 midPoint = Vector3.Lerp(startPos, endPos, progress);

            // 添加弧形高度
            float arcOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
            midPoint.y += arcOffset;

            return midPoint;
        }

        /// <summary>
        /// 播放到達效果
        /// </summary>
        private IEnumerator PlayArrivalEffect(RectTransform rectTransform, Image imageComponent)
        {
            float effectDuration = 0.2f;
            float elapsedTime = 0f;

            Vector3 originalScale = rectTransform.localScale;
            Vector3 maxScale = originalScale * 1.3f;

            // 放大效果
            while (elapsedTime < effectDuration * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (effectDuration * 0.5f);
                rectTransform.localScale = Vector3.Lerp(originalScale, maxScale, progress);
                yield return null;
            }

            elapsedTime = 0f;

            // 縮小並淡出
            while (elapsedTime < effectDuration * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (effectDuration * 0.5f);

                rectTransform.localScale = Vector3.Lerp(maxScale, Vector3.zero, progress);

                if (imageComponent != null)
                {
                    Color color = imageComponent.color;
                    color.a = Mathf.Lerp(1f, 0f, progress);
                    imageComponent.color = color;
                }

                yield return null;
            }
        }

        /// <summary>
        /// 調試日誌
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[IngredientFlyAnimation] {message}");
            }
        }

        /// <summary>
        /// 獲取當前活躍動畫數量
        /// </summary>
        public int GetActiveAnimationCount()
        {
            return activeAnimations;
        }

        /// <summary>
        /// 等待所有動畫完成
        /// </summary>
        public IEnumerator WaitForAllAnimationsComplete()
        {
            while (activeAnimations > 0)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 手動測試飛行動畫（用於調試）
        /// </summary>
        [ContextMenu("測試飛行動畫")]
        public void TestFlyAnimation()
        {
            Vector3 startPos = new Vector3(-200, 0, 0);
            Vector3 endPos = new Vector3(200, 0, 0);

            // 創建測試圖示
            Sprite testSprite = Resources.Load<Sprite>("TestIcon");
            if (testSprite == null)
            {
                // 創建簡單的白色方塊作為測試
                Texture2D texture = Texture2D.whiteTexture;
                testSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            }

            PlayFlyAnimation(startPos, endPos, testSprite, () => {
                DebugLog("測試動畫完成");
            });
        }

        /// <summary>
        /// 靜態方法：檢查實例是否存在
        /// </summary>
        public static bool IsInstanceAvailable()
        {
            return Instance != null;
        }
    }
}