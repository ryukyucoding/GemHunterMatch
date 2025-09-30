using System.Collections;
using UnityEngine;
using Match3;

namespace Match3
{
    /// <summary>
    /// 完整料理 UI 系統測試腳本
    /// </summary>
    public class CookingUISystemTest : MonoBehaviour
    {
        [Header("測試設定")]
        public RecipeDatabase testRecipeDatabase;
        public OrderUI orderUI;
        public PlateUI plateUI;
        public IngredientFlyAnimation flyAnimation;

        [Header("測試參數")]
        public bool runTestsOnStart = true;
        public float testDelay = 2.0f;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunUISystemTests());
            }
        }

        /// <summary>
        /// 執行完整的 UI 系統測試
        /// </summary>
        private IEnumerator RunUISystemTests()
        {
            Debug.Log("=== 料理 UI 系統測試開始 ===");

            yield return new WaitForSeconds(1.0f);

            // 測試 1: 檢查所有系統組件
            yield return StartCoroutine(TestSystemComponents());

            yield return new WaitForSeconds(testDelay);

            // 測試 2: 檢查 OrderManager 整合
            yield return StartCoroutine(TestOrderManagerIntegration());

            yield return new WaitForSeconds(testDelay);

            // 測試 3: 測試飛行動畫
            yield return StartCoroutine(TestFlyAnimation());

            yield return new WaitForSeconds(testDelay);

            // 測試 4: 測試完整流程
            yield return StartCoroutine(TestCompleteFlow());

            Debug.Log("=== 料理 UI 系統測試結束 ===");
        }

        /// <summary>
        /// 測試系統組件
        /// </summary>
        private IEnumerator TestSystemComponents()
        {
            Debug.Log("--- 測試 1: 檢查系統組件 ---");

            // 檢查 OrderManager
            if (OrderManager.Instance == null)
            {
                Debug.LogError("❌ OrderManager.Instance 不存在");
            }
            else
            {
                Debug.Log("✓ OrderManager.Instance 存在");

                if (OrderManager.Instance.recipeDatabase == null && testRecipeDatabase != null)
                {
                    OrderManager.Instance.recipeDatabase = testRecipeDatabase;
                    Debug.Log("✓ 已設定測試用 RecipeDatabase");
                }
            }

            // 檢查 OrderUI
            if (orderUI == null)
            {
                orderUI = FindFirstObjectByType<OrderUI>();
            }

            if (orderUI == null)
            {
                Debug.LogError("❌ OrderUI 不存在");
            }
            else
            {
                Debug.Log("✓ OrderUI 存在");
            }

            // 檢查 PlateUI
            if (plateUI == null)
            {
                plateUI = FindFirstObjectByType<PlateUI>();
            }

            if (plateUI == null && PlateUI.Instance == null)
            {
                Debug.LogError("❌ PlateUI 不存在");
            }
            else
            {
                Debug.Log("✓ PlateUI 存在");
            }

            // 檢查 IngredientFlyAnimation
            if (flyAnimation == null)
            {
                flyAnimation = FindFirstObjectByType<IngredientFlyAnimation>();
            }

            if (flyAnimation == null && !IngredientFlyAnimation.IsInstanceAvailable())
            {
                Debug.LogError("❌ IngredientFlyAnimation 不存在");
            }
            else
            {
                Debug.Log("✓ IngredientFlyAnimation 存在");
            }

            yield return null;
        }

        /// <summary>
        /// 測試 OrderManager 整合
        /// </summary>
        private IEnumerator TestOrderManagerIntegration()
        {
            Debug.Log("--- 測試 2: OrderManager 整合 ---");

            if (OrderManager.Instance == null)
            {
                Debug.LogError("❌ OrderManager 不存在，跳過測試");
                yield break;
            }

            // 檢查訂單數量
            var activeOrders = OrderManager.Instance.GetActiveOrders();
            Debug.Log($"當前活躍訂單數量：{activeOrders.Count}");

            // 如果沒有訂單，嘗試創建一個
            if (activeOrders.Count == 0)
            {
                Debug.Log("嘗試創建新訂單...");
                OrderManager.Instance.StartNewOrder();
                yield return new WaitForSeconds(0.5f);

                activeOrders = OrderManager.Instance.GetActiveOrders();
                Debug.Log($"創建後的訂單數量：{activeOrders.Count}");
            }

            // 如果有訂單，顯示詳細資訊
            if (activeOrders.Count > 0)
            {
                var order = activeOrders[0];
                Debug.Log($"第一個訂單：{order.recipe.dishName}");
                Debug.Log($"所需食材數量：{order.recipe.RequiredIngredients.Count}");

                foreach (var ingredient in order.recipe.RequiredIngredients)
                {
                    int collected = order.collectedIngredients.ContainsKey(ingredient.Key) ?
                        order.collectedIngredients[ingredient.Key] : 0;
                    Debug.Log($"  • {ingredient.Key}: {collected}/{ingredient.Value}");
                }
            }

            yield return null;
        }

        /// <summary>
        /// 測試飛行動畫
        /// </summary>
        private IEnumerator TestFlyAnimation()
        {
            Debug.Log("--- 測試 3: 飛行動畫 ---");

            if (!IngredientFlyAnimation.IsInstanceAvailable())
            {
                Debug.LogError("❌ IngredientFlyAnimation 不可用，跳過測試");
                yield break;
            }

            // 創建測試圖示
            Sprite testSprite = CreateTestSprite();

            // 測試飛行動畫
            Vector3 startPos = new Vector3(-3, 0, 0);
            Vector3 endPos = new Vector3(3, 0, 0);

            Debug.Log("播放測試飛行動畫...");
            bool animationComplete = false;

            IngredientFlyAnimation.PlayFlyAnimation(startPos, endPos, testSprite, () => {
                Debug.Log("✓ 飛行動畫完成");
                animationComplete = true;
            });

            // 等待動畫完成
            float timeout = 5.0f;
            float elapsed = 0f;

            while (!animationComplete && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!animationComplete)
            {
                Debug.LogWarning("⚠️ 飛行動畫可能未完成或超時");
            }
        }

        /// <summary>
        /// 測試完整流程
        /// </summary>
        private IEnumerator TestCompleteFlow()
        {
            Debug.Log("--- 測試 4: 完整流程 ---");

            if (OrderManager.Instance == null)
            {
                Debug.LogError("❌ OrderManager 不存在，跳過完整流程測試");
                yield break;
            }

            Debug.Log("模擬食材收集流程...");

            // 獲取測試位置
            Vector3 testWorldPos = new Vector3(0, 0, 0);

            // 模擬收集不同食材
            var foodTypes = new FoodType[] {
                FoodType.Bread,
                FoodType.Cheese,
                FoodType.Egg,
                FoodType.Lettuce
            };

            foreach (var foodType in foodTypes)
            {
                Debug.Log($"收集食材：{foodType}");
                OrderManager.Instance.CollectIngredient(foodType, 1, testWorldPos);

                // 等待動畫和處理完成
                yield return new WaitForSeconds(1.5f);
            }

            // 顯示最終狀態
            var finalOrders = OrderManager.Instance.GetActiveOrders();
            Debug.Log($"測試後的訂單數量：{finalOrders.Count}");

            if (finalOrders.Count > 0)
            {
                var order = finalOrders[0];
                Debug.Log($"訂單狀態：{order.recipe.dishName}");
                Debug.Log($"完成度：{order.GetProgressPercentage():P1}");
            }
        }

        /// <summary>
        /// 創建測試圖示
        /// </summary>
        private Sprite CreateTestSprite()
        {
            if (Texture2D.whiteTexture != null)
            {
                return Sprite.Create(Texture2D.whiteTexture,
                    new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                    Vector2.one * 0.5f);
            }

            return null;
        }

        /// <summary>
        /// 手動執行測試
        /// </summary>
        [ContextMenu("執行 UI 系統測試")]
        public void ManualRunTests()
        {
            StartCoroutine(RunUISystemTests());
        }

        /// <summary>
        /// 手動測試飛行動畫
        /// </summary>
        [ContextMenu("測試飛行動畫")]
        public void ManualTestFlyAnimation()
        {
            StartCoroutine(TestFlyAnimation());
        }

        /// <summary>
        /// 手動測試食材收集
        /// </summary>
        [ContextMenu("測試食材收集")]
        public void ManualTestIngredientCollection()
        {
            if (OrderManager.Instance != null)
            {
                Vector3 testPos = transform.position;
                OrderManager.Instance.CollectIngredient(FoodType.Bread, 1, testPos);
            }
        }

        /// <summary>
        /// 重置所有系統
        /// </summary>
        [ContextMenu("重置系統")]
        public void ResetSystems()
        {
            Debug.Log("重置料理 UI 系統...");

            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.ClearAllOrders();
            }

            if (plateUI != null)
            {
                plateUI.Clear();
            }

            if (orderUI != null)
            {
                orderUI.ClearOrder();
            }

            Debug.Log("✓ 系統重置完成");
        }
    }
}