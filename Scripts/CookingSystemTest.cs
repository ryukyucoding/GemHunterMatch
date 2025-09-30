using UnityEngine;
using Match3;

namespace Match3
{
    /// <summary>
    /// 測試料理系統整合的簡單腳本
    /// </summary>
    public class CookingSystemTest : MonoBehaviour
    {
        [Header("測試設定")]
        public RecipeDatabase testRecipeDatabase;

        private void Start()
        {
            // 等待系統初始化
            Invoke(nameof(RunTests), 1.0f);
        }

        private void RunTests()
        {
            Debug.Log("=== 料理系統整合測試開始 ===");

            // 測試 1: 檢查 OrderManager 是否存在並初始化
            if (OrderManager.Instance == null)
            {
                Debug.LogError("測試失敗：OrderManager.Instance 為 null");
                return;
            }
            Debug.Log("✓ OrderManager 實例存在");

            // 測試 2: 檢查 RecipeDatabase 是否設定
            if (OrderManager.Instance.recipeDatabase == null)
            {
                Debug.LogWarning("警告：OrderManager 的 recipeDatabase 未設定");
                if (testRecipeDatabase != null)
                {
                    OrderManager.Instance.recipeDatabase = testRecipeDatabase;
                    Debug.Log("✓ 已設定測試用 RecipeDatabase");
                }
            }
            else
            {
                Debug.Log("✓ RecipeDatabase 已設定");
            }

            // 測試 3: 檢查是否有活躍訂單
            var activeOrders = OrderManager.Instance.GetActiveOrders();
            Debug.Log($"✓ 當前活躍訂單數量：{activeOrders.Count}");

            // 測試 4: 模擬食材收集
            Debug.Log("開始測試食材收集...");
            OrderManager.Instance.CollectIngredient(FoodType.Bread, 1);
            OrderManager.Instance.CollectIngredient(FoodType.Cheese, 1);
            OrderManager.Instance.CollectIngredient(FoodType.Egg, 1);
            Debug.Log("✓ 食材收集測試完成");

            Debug.Log("=== 料理系統整合測試結束 ===");
        }

        [ContextMenu("手動測試食材收集")]
        public void ManualTestIngredientCollection()
        {
            if (OrderManager.Instance == null)
            {
                Debug.LogError("OrderManager.Instance 不存在");
                return;
            }

            // 收集各種食材
            Debug.Log("手動收集食材測試：");
            OrderManager.Instance.CollectIngredient(FoodType.Bread, 2);
            OrderManager.Instance.CollectIngredient(FoodType.Cheese, 1);
            OrderManager.Instance.CollectIngredient(FoodType.Egg, 1);
            OrderManager.Instance.CollectIngredient(FoodType.Lettuce, 1);
            OrderManager.Instance.CollectIngredient(FoodType.Steak, 1);
            OrderManager.Instance.CollectIngredient(FoodType.Tomato, 1);
        }

        [ContextMenu("顯示訂單狀態")]
        public void ShowOrderStatus()
        {
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.ShowOrderStatus();
            }
        }
    }
}