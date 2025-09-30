using UnityEngine;
using UnityEngine.UIElements;
using Match3;

namespace Match3
{
    /// <summary>
    /// 料理系統初始化器 - 自動設置 CookingUIManager 和相關資源
    /// </summary>
    [DefaultExecutionOrder(-5000)] // 在大部分系統之後，但在 UI 系統之前執行
    public class CookingSystemInitializer : MonoBehaviour
    {
        [Header("食材圖示資源路徑")]
        public string breadSpritePath = "UI/Food/bread_0";
        public string cheeseSpritePath = "UI/Food/cheese_0";
        public string eggSpritePath = "UI/Food/egg_0";
        public string lettuceSpritePath = "UI/Food/lettuce_0";
        public string steakSpritePath = "UI/Food/steak_0";
        public string tomatoSpritePath = "UI/Food/tomato_0";

        [Header("替代食材圖示 (如果資源路徑不存在)")]
        public Sprite fallbackBreadSprite;
        public Sprite fallbackCheeseSprite;
        public Sprite fallbackEggSprite;
        public Sprite fallbackLettuceSprite;
        public Sprite fallbackSteakSprite;
        public Sprite fallbackTomatoSprite;

        [Header("設定")]
        public bool enableDebugLog = true;
        public bool initializeOnStart = true;

        // Singleton 模式
        public static CookingSystemInitializer Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticData()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeCookingSystem();
            }
        }

        /// <summary>
        /// 初始化料理系統
        /// </summary>
        [ContextMenu("初始化料理系統")]
        public void InitializeCookingSystem()
        {
            DebugLog("開始初始化料理系統...");

            // 查找或創建 UIDocument
            UIDocument uiDocument = FindOrCreateUIDocument();
            if (uiDocument == null)
            {
                DebugLog("錯誤：無法找到或創建 UIDocument");
                return;
            }

            // 添加 CookingUIManager 組件
            CookingUIManager cookingUIManager = SetupCookingUIManager(uiDocument);
            if (cookingUIManager == null)
            {
                DebugLog("錯誤：無法設置 CookingUIManager");
                return;
            }

            // 載入並設定食材圖示
            LoadAndAssignFoodSprites(cookingUIManager);

            DebugLog("料理系統初始化完成！");
        }

        /// <summary>
        /// 查找或創建 UIDocument
        /// </summary>
        private UIDocument FindOrCreateUIDocument()
        {
            // 首先嘗試找到現有的 UIDocument
            UIDocument uiDocument = FindFirstObjectByType<UIDocument>();

            if (uiDocument != null)
            {
                DebugLog($"找到現有的 UIDocument: {uiDocument.name}");
                return uiDocument;
            }

            DebugLog("未找到 UIDocument，嘗試創建新的...");

            // 如果沒有找到，嘗試創建新的
            GameObject uiObject = new GameObject("CookingUI_Document");
            uiDocument = uiObject.AddComponent<UIDocument>();

            // 嘗試載入現有的 UI 資源
            var panelSettings = Resources.Load<PanelSettings>("UI/PanelSettings");
            if (panelSettings != null)
            {
                uiDocument.panelSettings = panelSettings;
                DebugLog("已設定 PanelSettings");
            }

            var visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/MainUI");
            if (visualTreeAsset != null)
            {
                uiDocument.visualTreeAsset = visualTreeAsset;
                DebugLog("已設定 VisualTreeAsset");
            }

            DebugLog($"創建了新的 UIDocument: {uiObject.name}");
            return uiDocument;
        }

        /// <summary>
        /// 設置 CookingUIManager 組件
        /// </summary>
        private CookingUIManager SetupCookingUIManager(UIDocument uiDocument)
        {
            // 檢查是否已經存在 CookingUIManager
            CookingUIManager existingManager = uiDocument.GetComponent<CookingUIManager>();
            if (existingManager != null)
            {
                DebugLog("CookingUIManager 已存在，使用現有的");
                return existingManager;
            }

            // 檢查場景中是否已有其他 CookingUIManager
            if (CookingUIManager.Instance != null)
            {
                DebugLog("場景中已存在 CookingUIManager Instance");
                return CookingUIManager.Instance;
            }

            // 添加新的 CookingUIManager 組件
            CookingUIManager cookingUIManager = uiDocument.gameObject.AddComponent<CookingUIManager>();

            // 設定基本參數
            cookingUIManager.uiDocument = uiDocument;
            cookingUIManager.enableDebugLog = enableDebugLog;

            DebugLog("已添加 CookingUIManager 組件");
            return cookingUIManager;
        }

        /// <summary>
        /// 載入並分配食材圖示
        /// </summary>
        private void LoadAndAssignFoodSprites(CookingUIManager cookingUIManager)
        {
            DebugLog("開始載入食材圖示...");

            // 載入麵包圖示
            cookingUIManager.breadSprite = LoadSpriteWithFallback(breadSpritePath, fallbackBreadSprite, "Bread");

            // 載入起司圖示
            cookingUIManager.cheeseSprite = LoadSpriteWithFallback(cheeseSpritePath, fallbackCheeseSprite, "Cheese");

            // 載入雞蛋圖示
            cookingUIManager.eggSprite = LoadSpriteWithFallback(eggSpritePath, fallbackEggSprite, "Egg");

            // 載入生菜圖示
            cookingUIManager.lettuceSprite = LoadSpriteWithFallback(lettuceSpritePath, fallbackLettuceSprite, "Lettuce");

            // 載入牛排圖示
            cookingUIManager.steakSprite = LoadSpriteWithFallback(steakSpritePath, fallbackSteakSprite, "Steak");

            // 載入番茄圖示
            cookingUIManager.tomatoSprite = LoadSpriteWithFallback(tomatoSpritePath, fallbackTomatoSprite, "Tomato");

            DebugLog("食材圖示載入完成");
        }

        /// <summary>
        /// 載入圖示，如果失敗則使用備用圖示
        /// </summary>
        private Sprite LoadSpriteWithFallback(string resourcePath, Sprite fallbackSprite, string spriteName)
        {
            // 如果已提供備用圖示，優先使用（避免不必要的 Resources 載入）
            if (fallbackSprite != null)
            {
                DebugLog($"✓ 使用已設定的 {spriteName} 圖示");
                return fallbackSprite;
            }

            // 嘗試從 Resources 載入
            Sprite loadedSprite = Resources.Load<Sprite>(resourcePath);

            if (loadedSprite != null)
            {
                DebugLog($"✓ 從 Resources 載入 {spriteName} 圖示: {resourcePath}");
                return loadedSprite;
            }

            DebugLog($"❌ 無法載入 {spriteName} 圖示，路徑: {resourcePath}，且未提供備用圖示");
            return null;
        }

        /// <summary>
        /// 手動重新初始化系統
        /// </summary>
        [ContextMenu("重新初始化料理系統")]
        public void ReinitializeCookingSystem()
        {
            DebugLog("重新初始化料理系統...");

            // 清理現有的 CookingUIManager
            if (CookingUIManager.Instance != null)
            {
                DestroyImmediate(CookingUIManager.Instance.gameObject.GetComponent<CookingUIManager>());
            }

            // 重新初始化
            InitializeCookingSystem();
        }

        /// <summary>
        /// 檢查系統狀態
        /// </summary>
        [ContextMenu("檢查料理系統狀態")]
        public void CheckSystemStatus()
        {
            DebugLog("=== 料理系統狀態檢查 ===");

            // 檢查 UIDocument
            UIDocument uiDoc = FindFirstObjectByType<UIDocument>();
            DebugLog($"UIDocument: {(uiDoc != null ? "✓ 存在" : "❌ 不存在")}");

            // 檢查 CookingUIManager
            DebugLog($"CookingUIManager Instance: {(CookingUIManager.Instance != null ? "✓ 存在" : "❌ 不存在")}");

            // 檢查 OrderManager
            DebugLog($"OrderManager Instance: {(OrderManager.Instance != null ? "✓ 存在" : "❌ 不存在")}");

            // 檢查 RecipeDatabase
            if (OrderManager.Instance != null)
            {
                DebugLog($"RecipeDatabase: {(OrderManager.Instance.recipeDatabase != null ? "✓ 已設定" : "❌ 未設定")}");
            }

            DebugLog("=== 狀態檢查完成 ===");
        }

        /// <summary>
        /// 調試日誌
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[CookingSystemInitializer] {message}");
            }
        }

        /// <summary>
        /// 在應用程式關閉時清理
        /// </summary>
        private void OnApplicationQuit()
        {
            Instance = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}