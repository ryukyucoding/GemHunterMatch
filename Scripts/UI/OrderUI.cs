using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Match3;

namespace Match3
{
    /// <summary>
    /// 訂單 UI 系統 - 負責顯示左側的訂單面板
    /// </summary>
    public class OrderUI : MonoBehaviour
    {
        [Header("UI 元件")]
        public GameObject orderPanel;                    // 訂單面板
        public TextMeshProUGUI dishNameText;            // 料理名稱文字
        public Image dishIconImage;                     // 料理圖示
        public Transform ingredientContainer;           // 食材進度容器
        public GameObject ingredientProgressPrefab;     // 食材進度項目預製體

        [Header("設定")]
        public bool enableDebugLog = true;              // 是否啟用調試日誌

        // 當前顯示的訂單
        private OrderManager.ActiveOrder currentOrder;
        private List<IngredientProgressItem> ingredientItems = new List<IngredientProgressItem>();

        /// <summary>
        /// 食材進度項目類別
        /// </summary>
        [System.Serializable]
        public class IngredientProgressItem
        {
            public FoodType foodType;
            public GameObject itemObject;
            public Image iconImage;
            public TextMeshProUGUI progressText;
            public Image progressBar;
        }

        private void Start()
        {
            // 訂閱 OrderManager 事件
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.OnNewOrderStarted.AddListener(ShowNewOrder);
                OrderManager.Instance.OnIngredientCollected.AddListener(OnIngredientCollected);
                OrderManager.Instance.OnOrderCompleted.AddListener(OnOrderCompleted);
                OrderManager.Instance.OnOrderExpired.AddListener(OnOrderExpired);
            }

            // 初始化 UI
            ClearOrder();
        }

        private void OnDestroy()
        {
            // 取消訂閱事件
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.OnNewOrderStarted.RemoveListener(ShowNewOrder);
                OrderManager.Instance.OnIngredientCollected.RemoveListener(OnIngredientCollected);
                OrderManager.Instance.OnOrderCompleted.RemoveListener(OnOrderCompleted);
                OrderManager.Instance.OnOrderExpired.RemoveListener(OnOrderExpired);
            }
        }

        /// <summary>
        /// 顯示新訂單
        /// </summary>
        /// <param name="recipe">配方</param>
        public void ShowNewOrder(Recipe recipe)
        {
            if (recipe == null)
            {
                DebugLog("ShowNewOrder: 配方為 null");
                return;
            }

            DebugLog($"顯示新訂單: {recipe.dishName}");

            // 獲取當前訂單
            var activeOrders = OrderManager.Instance.GetActiveOrders();
            currentOrder = activeOrders.Find(order => order.recipe.dishName == recipe.dishName);

            if (currentOrder == null)
            {
                DebugLog("無法找到對應的活躍訂單");
                return;
            }

            // 更新基本資訊
            dishNameText.text = recipe.dishName;
            dishIconImage.sprite = recipe.dishIcon;

            // 創建食材進度項目
            CreateIngredientItems(recipe);

            // 顯示面板
            orderPanel.SetActive(true);

            // 更新顯示
            UpdateOrderDisplay();
        }

        /// <summary>
        /// 創建食材進度項目
        /// </summary>
        /// <param name="recipe">配方</param>
        private void CreateIngredientItems(Recipe recipe)
        {
            // 清除現有項目
            ClearIngredientItems();

            foreach (var ingredient in recipe.RequiredIngredients)
            {
                if (ingredientProgressPrefab == null)
                {
                    DebugLog("警告：ingredientProgressPrefab 未設定，將創建簡單文字項目");
                    CreateSimpleTextItem(ingredient.Key, ingredient.Value);
                    continue;
                }

                // 創建進度項目
                GameObject itemObj = Instantiate(ingredientProgressPrefab, ingredientContainer);

                var progressItem = new IngredientProgressItem
                {
                    foodType = ingredient.Key,
                    itemObject = itemObj
                };

                // 查找子元件
                progressItem.iconImage = itemObj.GetComponentInChildren<Image>();
                var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                progressItem.progressText = texts.Length > 0 ? texts[0] : null;

                var images = itemObj.GetComponentsInChildren<Image>();
                progressItem.progressBar = images.Length > 1 ? images[1] : null;

                // 設定食材圖示（如果有的話）
                if (progressItem.iconImage != null)
                {
                    // 這裡可以設定食材對應的圖示
                    // progressItem.iconImage.sprite = GetFoodTypeSprite(ingredient.Key);
                }

                ingredientItems.Add(progressItem);
            }
        }

        /// <summary>
        /// 創建簡單文字項目（當沒有預製體時的備案）
        /// </summary>
        private void CreateSimpleTextItem(FoodType foodType, int required)
        {
            GameObject textObj = new GameObject($"Ingredient_{foodType}");
            textObj.transform.SetParent(ingredientContainer);

            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = $"{foodType}: 0/{required}";
            textComponent.fontSize = 16;
            textComponent.color = Color.white;

            var progressItem = new IngredientProgressItem
            {
                foodType = foodType,
                itemObject = textObj,
                progressText = textComponent
            };

            ingredientItems.Add(progressItem);
        }

        /// <summary>
        /// 更新訂單顯示
        /// </summary>
        public void UpdateOrderDisplay()
        {
            if (currentOrder == null) return;

            foreach (var item in ingredientItems)
            {
                int collected = currentOrder.collectedIngredients.ContainsKey(item.foodType)
                    ? currentOrder.collectedIngredients[item.foodType] : 0;
                int required = currentOrder.recipe.RequiredIngredients[item.foodType];

                // 更新文字
                if (item.progressText != null)
                {
                    item.progressText.text = $"{item.foodType}: {collected}/{required}";

                    // 改變顏色表示完成狀態
                    item.progressText.color = collected >= required ? Color.green : Color.white;
                }

                // 更新進度條
                if (item.progressBar != null)
                {
                    float progress = required > 0 ? (float)collected / required : 0f;
                    item.progressBar.fillAmount = progress;
                    item.progressBar.color = collected >= required ? Color.green : Color.yellow;
                }
            }
        }

        /// <summary>
        /// 清除訂單顯示
        /// </summary>
        public void ClearOrder()
        {
            orderPanel.SetActive(false);
            dishNameText.text = "";
            dishIconImage.sprite = null;
            currentOrder = null;
            ClearIngredientItems();
        }

        /// <summary>
        /// 清除食材項目
        /// </summary>
        private void ClearIngredientItems()
        {
            foreach (var item in ingredientItems)
            {
                if (item.itemObject != null)
                {
                    Destroy(item.itemObject);
                }
            }
            ingredientItems.Clear();
        }

        /// <summary>
        /// 當食材被收集時的回調
        /// </summary>
        private void OnIngredientCollected(FoodType foodType, int amount)
        {
            DebugLog($"食材收集: {foodType} +{amount}");
            UpdateOrderDisplay();
        }

        /// <summary>
        /// 當訂單完成時的回調
        /// </summary>
        private void OnOrderCompleted(Recipe recipe, int orderID)
        {
            DebugLog($"訂單完成: {recipe.dishName}");

            // 播放完成動畫或效果
            StartCoroutine(PlayOrderCompleteEffect());
        }

        /// <summary>
        /// 當訂單過期時的回調
        /// </summary>
        private void OnOrderExpired(Recipe recipe)
        {
            DebugLog($"訂單過期: {recipe.dishName}");
            ClearOrder();
        }

        /// <summary>
        /// 播放訂單完成效果
        /// </summary>
        private System.Collections.IEnumerator PlayOrderCompleteEffect()
        {
            // 簡單的閃爍效果
            for (int i = 0; i < 3; i++)
            {
                orderPanel.GetComponent<CanvasGroup>().alpha = 0.5f;
                yield return new WaitForSeconds(0.2f);
                orderPanel.GetComponent<CanvasGroup>().alpha = 1.0f;
                yield return new WaitForSeconds(0.2f);
            }

            // 延遲後清除
            yield return new WaitForSeconds(1.0f);
            ClearOrder();
        }

        /// <summary>
        /// 調試日誌
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[OrderUI] {message}");
            }
        }

        /// <summary>
        /// 手動更新顯示（用於調試）
        /// </summary>
        [ContextMenu("手動更新顯示")]
        public void ManualUpdateDisplay()
        {
            if (OrderManager.Instance != null)
            {
                var activeOrders = OrderManager.Instance.GetActiveOrders();
                if (activeOrders.Count > 0)
                {
                    ShowNewOrder(activeOrders[0].recipe);
                }
            }
        }
    }
}