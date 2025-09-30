using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Match3;

namespace Match3
{
    /// <summary>
    /// 餐盤 UI 系統 - 負責右側的餐盤顯示
    /// </summary>
    public class PlateUI : MonoBehaviour
    {
        public static PlateUI Instance { get; private set; }

        [Header("UI 元件")]
        public GameObject platePanel;                   // 餐盤面板
        public Transform plateContainer;                // 餐盤容器
        public Transform ingredientIconContainer;       // 食材圖示容器
        public GameObject plateBackground;              // 餐盤背景

        [Header("動畫設定")]
        public float slideOutDuration = 1.0f;          // 滑出動畫持續時間
        public float slideOutDistance = 500f;          // 滑出距離
        public AnimationCurve slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 滑出曲線

        [Header("食材圖示設定")]
        public Vector2 iconSize = new Vector2(50, 50);  // 圖示大小
        public float iconSpacing = 60f;                 // 圖示間距
        public int maxIconsPerRow = 3;                  // 每行最大圖示數

        [Header("設定")]
        public bool enableDebugLog = true;              // 是否啟用調試日誌

        // 當前餐盤狀態
        private List<IngredientIcon> ingredientIcons = new List<IngredientIcon>();
        private bool isAnimating = false;

        /// <summary>
        /// 食材圖示類別
        /// </summary>
        [System.Serializable]
        public class IngredientIcon
        {
            public FoodType foodType;
            public GameObject iconObject;
            public Image iconImage;
            public int count;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 訂閱 OrderManager 事件
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.OnIngredientCollected.AddListener(OnIngredientCollected);
                OrderManager.Instance.OnOrderCompleted.AddListener(OnOrderCompleted);
                OrderManager.Instance.OnNewOrderStarted.AddListener(OnNewOrderStarted);
            }

            // 初始化餐盤
            Clear();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // 取消訂閱事件
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.OnIngredientCollected.RemoveListener(OnIngredientCollected);
                OrderManager.Instance.OnOrderCompleted.RemoveListener(OnOrderCompleted);
                OrderManager.Instance.OnNewOrderStarted.RemoveListener(OnNewOrderStarted);
            }
        }

        /// <summary>
        /// 添加食材圖示
        /// </summary>
        /// <param name="foodType">食材類型</param>
        /// <param name="icon">圖示</param>
        public void AddIngredientIcon(FoodType foodType, Sprite icon)
        {
            if (isAnimating)
            {
                DebugLog("正在播放動畫，無法添加食材圖示");
                return;
            }

            DebugLog($"添加食材圖示: {foodType}");

            // 檢查是否已存在相同食材
            var existingIcon = ingredientIcons.Find(item => item.foodType == foodType);
            if (existingIcon != null)
            {
                // 增加數量（如果需要顯示數量的話）
                existingIcon.count++;
                UpdateIconDisplay(existingIcon);
                return;
            }

            // 創建新的食材圖示
            GameObject iconObj = CreateIngredientIconObject(foodType, icon);
            if (iconObj == null) return;

            var ingredientIcon = new IngredientIcon
            {
                foodType = foodType,
                iconObject = iconObj,
                iconImage = iconObj.GetComponent<Image>(),
                count = 1
            };

            ingredientIcons.Add(ingredientIcon);
            ArrangeIcons();

            // 播放添加動畫
            StartCoroutine(PlayAddIconAnimation(iconObj));
        }

        /// <summary>
        /// 創建食材圖示物件
        /// </summary>
        private GameObject CreateIngredientIconObject(FoodType foodType, Sprite icon)
        {
            if (ingredientIconContainer == null)
            {
                DebugLog("錯誤：ingredientIconContainer 未設定");
                return null;
            }

            GameObject iconObj = new GameObject($"Icon_{foodType}");
            iconObj.transform.SetParent(ingredientIconContainer);

            // 添加 Image 元件
            Image imageComponent = iconObj.AddComponent<Image>();
            imageComponent.sprite = icon;
            imageComponent.preserveAspect = true;

            // 設定大小
            RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = iconSize;
            rectTransform.localScale = Vector3.one;

            return iconObj;
        }

        /// <summary>
        /// 更新圖示顯示
        /// </summary>
        private void UpdateIconDisplay(IngredientIcon ingredientIcon)
        {
            // 這裡可以添加數量顯示邏輯
            // 例如：在圖示上添加數字標籤
        }

        /// <summary>
        /// 排列圖示
        /// </summary>
        private void ArrangeIcons()
        {
            for (int i = 0; i < ingredientIcons.Count; i++)
            {
                int row = i / maxIconsPerRow;
                int col = i % maxIconsPerRow;

                Vector3 position = new Vector3(
                    col * iconSpacing,
                    -row * iconSpacing,
                    0
                );

                ingredientIcons[i].iconObject.transform.localPosition = position;
            }
        }

        /// <summary>
        /// 播放完成動畫
        /// </summary>
        public void PlayCompleteAnimation()
        {
            if (isAnimating) return;

            DebugLog("播放餐盤完成動畫");
            StartCoroutine(PlayCompleteAnimationCoroutine());
        }

        /// <summary>
        /// 完成動畫協程
        /// </summary>
        private IEnumerator PlayCompleteAnimationCoroutine()
        {
            isAnimating = true;

            Vector3 originalPosition = plateContainer.localPosition;
            Vector3 targetPosition = originalPosition + Vector3.right * slideOutDistance;

            float elapsedTime = 0f;

            // 滑出動畫
            while (elapsedTime < slideOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / slideOutDuration;
                float curveValue = slideOutCurve.Evaluate(progress);

                plateContainer.localPosition = Vector3.Lerp(originalPosition, targetPosition, curveValue);

                yield return null;
            }

            plateContainer.localPosition = targetPosition;

            // 等待一段時間
            yield return new WaitForSeconds(0.5f);

            // 重置位置並清除
            plateContainer.localPosition = originalPosition;
            Clear();

            isAnimating = false;
        }

        /// <summary>
        /// 播放添加圖示動畫
        /// </summary>
        private IEnumerator PlayAddIconAnimation(GameObject iconObj)
        {
            Vector3 originalScale = iconObj.transform.localScale;
            iconObj.transform.localScale = Vector3.zero;

            float duration = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float scale = Mathf.Lerp(0f, 1f, progress);

                iconObj.transform.localScale = originalScale * scale;

                yield return null;
            }

            iconObj.transform.localScale = originalScale;
        }

        /// <summary>
        /// 清除餐盤
        /// </summary>
        public void Clear()
        {
            DebugLog("清除餐盤");

            // 銷毀所有食材圖示
            foreach (var icon in ingredientIcons)
            {
                if (icon.iconObject != null)
                {
                    Destroy(icon.iconObject);
                }
            }

            ingredientIcons.Clear();
            isAnimating = false;
        }

        /// <summary>
        /// 當食材被收集時的回調
        /// </summary>
        private void OnIngredientCollected(FoodType foodType, int amount)
        {
            // 這裡可以根據需要添加食材圖示
            // 實際的圖示添加會在飛行動畫完成後觸發
            DebugLog($"餐盤收到食材收集事件: {foodType} +{amount}");
        }

        /// <summary>
        /// 當訂單完成時的回調
        /// </summary>
        private void OnOrderCompleted(Recipe recipe, int orderID)
        {
            DebugLog($"訂單完成，播放餐盤動畫: {recipe.dishName}");
            PlayCompleteAnimation();
        }

        /// <summary>
        /// 當新訂單開始時的回調
        /// </summary>
        private void OnNewOrderStarted(Recipe recipe)
        {
            DebugLog($"新訂單開始，清除餐盤: {recipe.dishName}");
            if (!isAnimating)
            {
                Clear();
            }
        }

        /// <summary>
        /// 調試日誌
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[PlateUI] {message}");
            }
        }

        /// <summary>
        /// 手動測試添加圖示（用於調試）
        /// </summary>
        [ContextMenu("測試添加圖示")]
        public void TestAddIcon()
        {
            // 創建測試圖示
            Sprite testSprite = Resources.Load<Sprite>("TestIcon");
            AddIngredientIcon(FoodType.Bread, testSprite);
        }

        /// <summary>
        /// 手動測試完成動畫（用於調試）
        /// </summary>
        [ContextMenu("測試完成動畫")]
        public void TestCompleteAnimation()
        {
            PlayCompleteAnimation();
        }

        /// <summary>
        /// 獲取當前餐盤上的食材數量
        /// </summary>
        public int GetIngredientCount(FoodType foodType)
        {
            var icon = ingredientIcons.Find(item => item.foodType == foodType);
            return icon?.count ?? 0;
        }

        /// <summary>
        /// 檢查餐盤是否為空
        /// </summary>
        public bool IsEmpty()
        {
            return ingredientIcons.Count == 0;
        }
    }
}