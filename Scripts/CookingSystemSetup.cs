using UnityEngine;
using Match3;

namespace Match3
{
    /// <summary>
    /// 料理系統設定腳本 - 在場景中放置此腳本來快速設置料理系統
    /// </summary>
    [DefaultExecutionOrder(-10000)]  // 在 UIHandler(-9000) 之前執行，確保 OrderManager 先被創建
    public class CookingSystemSetup : MonoBehaviour
    {
        [Header("==== 料理系統快速設置 ====")]
        [Space(10)]

        [Header("🎯 執行設置")]
        [Tooltip("勾選後會在場景啟動時自動初始化料理系統")]
        public bool autoInitializeOnStart = true;

        [Space(10)]
        [Header("📋 必要設置")]
        [Tooltip("(已廢棄) 現在使用 RecipeLibrary 直接從代碼讀取配方")]
        public RecipeDatabase recipeDatabase;

        [Space(10)]
        [Header("🍞 食材圖示")]
        public Sprite breadSprite;
        public Sprite cheeseSprite;
        public Sprite eggSprite;
        public Sprite lettuceSprite;
        public Sprite steakSprite;
        public Sprite tomatoSprite;

        [Space(10)]
        [Header("⚙️ UI 設定")]
        [Tooltip("訂單面板位置")]
        public Vector2 orderPanelPosition = new Vector2(60, 100);
        [Tooltip("訂單面板大小")]
        public Vector2 orderPanelSize = new Vector2(1600, 800);

        [Tooltip("餐盤面板位置（相對右下角）")]
        public Vector2 platePanelPosition = new Vector2(-200, 0);  // 往左100px，高度設為0由CSS控制
        [Tooltip("餐盤面板大小")]
        public Vector2 platePanelSize = new Vector2(300, 300);  // 基礎大小，實際會放大5倍
        [Tooltip("餐盤面板背景圖片（可選）")]
        public Sprite platePanelBackground;
        [Tooltip("廚師圖片（可選）")]
        public Sprite chefSprite;

        [Space(10)]
        [Header("🎬 動畫設定")]
        [Tooltip("滑動動畫持續時間")]
        public float slideAnimationDuration = 1.0f;
        [Tooltip("餐盤滑動距離")]
        public float plateSlideDistance = 400f;

        [Space(10)]
        [Header("🔧 調試設定")]
        public bool enableDebugLog = true;

        private void Start()
        {
            if (autoInitializeOnStart)
            {
                SetupCookingSystem();
            }
        }

        /// <summary>
        /// 設置料理系統
        /// </summary>
        [ContextMenu("🚀 設置料理系統")]
        public void SetupCookingSystem()
        {
            DebugLog("開始設置料理系統...");

            // Step 1: 確保有 CookingSystemInitializer
            EnsureCookingSystemInitializer();

            // Step 2: 設置 OrderManager
            SetupOrderManager();

            // Step 3: 初始化料理 UI
            InitializeCookingUI();

            DebugLog("✅ 料理系統設置完成！");
        }

        /// <summary>
        /// 確保有 CookingSystemInitializer
        /// </summary>
        private void EnsureCookingSystemInitializer()
        {
            if (CookingSystemInitializer.Instance == null)
            {
                GameObject initializerObj = new GameObject("CookingSystemInitializer");
                var initializer = initializerObj.AddComponent<CookingSystemInitializer>();

                // 設定圖示路徑
                initializer.enableDebugLog = enableDebugLog;
                initializer.fallbackBreadSprite = breadSprite;
                initializer.fallbackCheeseSprite = cheeseSprite;
                initializer.fallbackEggSprite = eggSprite;
                initializer.fallbackLettuceSprite = lettuceSprite;
                initializer.fallbackSteakSprite = steakSprite;
                initializer.fallbackTomatoSprite = tomatoSprite;
                initializer.platePanelBackground = platePanelBackground;

                DebugLog("✓ 創建了 CookingSystemInitializer");
            }
            else
            {
                // 更新已存在的 Initializer 的餐盤背景設定
                if (platePanelBackground != null)
                {
                    CookingSystemInitializer.Instance.platePanelBackground = platePanelBackground;
                }
                DebugLog("✓ CookingSystemInitializer 已存在");
            }
        }

        /// <summary>
        /// 設置 OrderManager
        /// </summary>
        private void SetupOrderManager()
        {
            // 注意：現在使用 RecipeLibrary 系統，不再需要 recipeDatabase
            DebugLog("SetupOrderManager - 使用 RecipeLibrary 系統（配方直接從代碼載入）");

            // 檢查是否已有 OrderManager
            if (OrderManager.Instance == null)
            {
                // 創建 OrderManager
                GameObject orderManagerObj = new GameObject("OrderManager");
                var orderManager = orderManagerObj.AddComponent<OrderManager>();

                // recipeDatabase 欄位已廢棄，不再需要設置
                // OrderManager 會自動使用 RecipeLibrary.GetAllRecipes()

                orderManager.enableDebugLog = enableDebugLog;

                DebugLog("✓ 創建了 OrderManager（使用 RecipeLibrary）");
            }
            else
            {
                DebugLog("✓ OrderManager 已存在（使用 RecipeLibrary）");

                // 如果沒有活躍訂單，立即創建一個
                if (OrderManager.Instance.GetActiveOrders().Count == 0)
                {
                    DebugLog("OrderManager 沒有活躍訂單，立即創建新訂單");
                    OrderManager.Instance.StartNewOrder();
                }
            }
        }

        /// <summary>
        /// 初始化料理 UI
        /// </summary>
        private void InitializeCookingUI()
        {
            if (CookingSystemInitializer.Instance != null)
            {
                CookingSystemInitializer.Instance.InitializeCookingSystem();

                // 如果 CookingUIManager 創建成功，應用設定
                if (CookingUIManager.Instance != null)
                {
                    ApplyUISettings(CookingUIManager.Instance);
                }
            }
        }

        /// <summary>
        /// 應用 UI 設定
        /// </summary>
        private void ApplyUISettings(CookingUIManager cookingUIManager)
        {
            // 設定位置和大小
            cookingUIManager.orderPanelPosition = orderPanelPosition;
            cookingUIManager.orderPanelSize = orderPanelSize;
            cookingUIManager.platePanelPosition = platePanelPosition;
            cookingUIManager.platePanelSize = platePanelSize;

            // 設定動畫參數
            cookingUIManager.slideAnimationDuration = slideAnimationDuration;
            cookingUIManager.plateSlideDistance = plateSlideDistance;

            // 設定圖示（如果 Initializer 沒有載入成功的話）
            if (cookingUIManager.breadSprite == null) cookingUIManager.breadSprite = breadSprite;
            if (cookingUIManager.cheeseSprite == null) cookingUIManager.cheeseSprite = cheeseSprite;
            if (cookingUIManager.eggSprite == null) cookingUIManager.eggSprite = eggSprite;
            if (cookingUIManager.lettuceSprite == null) cookingUIManager.lettuceSprite = lettuceSprite;
            if (cookingUIManager.steakSprite == null) cookingUIManager.steakSprite = steakSprite;
            if (cookingUIManager.tomatoSprite == null) cookingUIManager.tomatoSprite = tomatoSprite;

            // 設定廚師圖片
            if (cookingUIManager.chefSprite == null) cookingUIManager.chefSprite = chefSprite;

            // 餐盤背景已經由 CookingSystemInitializer 設置，這裡不需要重複設置

            DebugLog("✓ 應用了 UI 設定");
        }

        /// <summary>
        /// 驗證設置
        /// </summary>
        [ContextMenu("🔍 驗證料理系統設置")]
        public void ValidateSetup()
        {
            DebugLog("=== 料理系統設置驗證 ===");

            // 檢查必要組件
            bool hasOrderManager = OrderManager.Instance != null;
            bool hasCookingUI = CookingUIManager.Instance != null;
            bool hasRecipeDB = recipeDatabase != null;

            DebugLog($"OrderManager: {(hasOrderManager ? "✅" : "❌")}");
            DebugLog($"CookingUIManager: {(hasCookingUI ? "✅" : "❌")}");
            DebugLog($"RecipeDatabase: {(hasRecipeDB ? "✅" : "❌")}");

            // 檢查圖示
            int spriteCount = 0;
            if (breadSprite != null) spriteCount++;
            if (cheeseSprite != null) spriteCount++;
            if (eggSprite != null) spriteCount++;
            if (lettuceSprite != null) spriteCount++;
            if (steakSprite != null) spriteCount++;
            if (tomatoSprite != null) spriteCount++;

            DebugLog($"食材圖示: {spriteCount}/6 ({(spriteCount == 6 ? "✅" : spriteCount > 0 ? "⚠️" : "❌")})");

            // 檢查 RecipeDatabase 內容
            if (hasRecipeDB)
            {
                int recipeCount = recipeDatabase.recipes.Count;
                DebugLog($"配方數量: {recipeCount} ({(recipeCount > 0 ? "✅" : "❌")})");
            }

            // 整體狀態
            bool isFullySetup = hasOrderManager && hasCookingUI && hasRecipeDB && spriteCount >= 3;
            DebugLog($"整體狀態: {(isFullySetup ? "✅ 完全設置" : "⚠️ 需要調整")}");

            DebugLog("=== 驗證完成 ===");
        }

        /// <summary>
        /// 重置料理系統
        /// </summary>
        [ContextMenu("🔄 重置料理系統")]
        public void ResetCookingSystem()
        {
            DebugLog("重置料理系統...");

            // 清理 CookingUIManager
            if (CookingUIManager.Instance != null)
            {
                if (CookingUIManager.Instance.gameObject.GetComponent<CookingUIManager>() != null)
                {
                    DestroyImmediate(CookingUIManager.Instance.gameObject.GetComponent<CookingUIManager>());
                }
            }

            // 清理 OrderManager
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.ClearAllOrders();
            }

            DebugLog("✅ 料理系統已重置");
        }

        /// <summary>
        /// 測試料理系統
        /// </summary>
        [ContextMenu("🎮 測試料理系統")]
        public void TestCookingSystem()
        {
            DebugLog("開始測試料理系統...");

            if (OrderManager.Instance == null)
            {
                DebugLog("❌ OrderManager 不存在，無法測試");
                return;
            }

            if (CookingUIManager.Instance == null)
            {
                DebugLog("❌ CookingUIManager 不存在，無法測試");
                return;
            }

            // 測試創建訂單
            OrderManager.Instance.StartNewOrder();

            // 測試收集食材
            OrderManager.Instance.CollectIngredient(FoodType.Bread, 1);
            OrderManager.Instance.CollectIngredient(FoodType.Cheese, 1);

            DebugLog("✅ 料理系統測試完成");
        }

        /// <summary>
        /// 調試日誌
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[CookingSystemSetup] {message}");
            }
        }

        /// <summary>
        /// 在 Inspector 中顯示幫助信息
        /// </summary>
        private void OnValidate()
        {
            // 確保面板大小合理
            if (orderPanelSize.x < 200) orderPanelSize.x = 200;
            if (orderPanelSize.y < 300) orderPanelSize.y = 300;
            if (platePanelSize.x < 150) platePanelSize.x = 150;
            if (platePanelSize.y < 150) platePanelSize.y = 150;

            // 確保動畫時間合理
            if (slideAnimationDuration < 0.1f) slideAnimationDuration = 0.1f;
            if (plateSlideDistance < 100f) plateSlideDistance = 100f;
        }
    }
}