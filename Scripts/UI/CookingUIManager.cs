using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Match3;

namespace Match3
{
    /// <summary>
    /// 料理 UI 管理器 - 使用 UI Toolkit C# API 動態創建和管理料理相關 UI
    /// </summary>
    public class CookingUIManager : MonoBehaviour
    {
        [Header("UI 設定")]
        public UIDocument uiDocument;                           // UI Document 引用
        public bool enableDebugLog = true;                      // 是否啟用調試日誌

        [Header("訂單面板設定")]
        public Vector2 orderPanelSize = new Vector2(300, 800);  // 訂單面板大小
        public Vector2 orderPanelPosition = new Vector2(100, 100); // 訂單面板位置

        [Header("餐盤面板設定")]
        public Vector2 platePanelSize = new Vector2(250, 200);  // 餐盤面板大小
        public Vector2 platePanelPosition = new Vector2(-300, -150); // 餐盤面板位置（相對右下角）
        public Sprite platePanelBackground;                     // 餐盤面板背景圖片

        [Header("動畫設定")]
        public float slideAnimationDuration = 1.0f;            // 滑動動畫持續時間
        public float plateSlideDistance = 400f;                // 餐盤滑動距離

        [Header("食材圖示")]
        public Sprite breadSprite;                              // 麵包圖示
        public Sprite cheeseSprite;                             // 起司圖示
        public Sprite eggSprite;                                // 雞蛋圖示
        public Sprite lettuceSprite;                            // 生菜圖示
        public Sprite steakSprite;                              // 牛排圖示
        public Sprite tomatoSprite;                             // 番茄圖示

        // UI 元素引用
        private VisualElement rootElement;
        private VisualElement orderPanel;
        private VisualElement platePanel;
        private Label dishNameLabel;
        private Label remainingOrdersLabel;  // 剩餘訂單數標籤
        private VisualElement ingredientContainer;
        private VisualElement plateIconContainer;

        // 內部狀態
        private List<IngredientProgressElement> ingredientElements = new List<IngredientProgressElement>();
        private List<VisualElement> plateIcons = new List<VisualElement>();
        private bool isAnimating = false;
        private Dictionary<FoodType, Sprite> foodSprites;

        /// <summary>
        /// 食材進度元素類別
        /// </summary>
        private class IngredientProgressElement
        {
            public FoodType foodType;
            public VisualElement container;
            public Label progressLabel;
            public VisualElement progressBar;
            public VisualElement progressFill;
        }

        // Singleton 模式
        public static CookingUIManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticData()
        {
            Instance = null;
        }

        void Awake()
        {
            // 設置 Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                DebugLog("已存在另一個 CookingUIManager Instance，銷毀此物件");
                Destroy(this);
                return;
            }

            // 等待 UIDocument 被創建
            StartCoroutine(WaitForUIDocument());
        }
        
        IEnumerator WaitForUIDocument()
        {
            DebugLog("等待 UIDocument...");
            yield return new WaitForSeconds(0.5f);
            uiDocument = FindFirstObjectByType<UIDocument>();
            DebugLog($"找到 UIDocument: {(uiDocument != null ? "✅" : "❌")}");

            if (uiDocument != null)
            {
                InitializeUI();
                DebugLog($"InitializeUI 完成，orderPanel: {(orderPanel != null ? "✅" : "❌")}, platePanel: {(platePanel != null ? "✅" : "❌")}");
                
                // UI 初始化完成後，立即訂閱事件
                SubscribeToOrderManagerEvents();
                DebugLog("UI 初始化完成後已訂閱 OrderManager 事件");
            }
            else
            {
                DebugLog("❌ 錯誤：找不到 UIDocument，無法初始化 UI");
            }

            // UI 初始化完成後，再顯示初始訂單
            yield return ShowInitialOrderDelayed();
        }

        private void Start()
        {
            // 訂閱事件會在 WaitForUIDocument 協程中完成，確保 UI 初始化後才訂閱
            // 這樣可以避免在 UI 未準備好時收到事件
        }
        
        private void OnEnable()
        {
            // 每次啟用時重新訂閱（確保場景重載後事件不丟失）
            if (OrderManager.Instance != null)
            {
                SubscribeToOrderManagerEvents();

                // 如果 UI 已經初始化，重新顯示訂單和餐盤
                if (orderPanel != null && platePanel != null)
                {
                    StartCoroutine(RefreshUIOnEnable());
                }
            }
        }

        /// <summary>
        /// OnEnable 時刷新 UI 顯示
        /// </summary>
        private IEnumerator RefreshUIOnEnable()
        {
            // 等待幾幀確保 OrderManager 已更新
            yield return null;
            yield return null;

            RefreshCurrentOrderDisplay();
            RefreshPlate();

            DebugLog("OnEnable: 已刷新訂單和餐盤顯示");
        }
        
        /// <summary>
        /// 延遲顯示初始訂單（確保 OrderManager 已創建訂單）
        /// </summary>
        private IEnumerator ShowInitialOrderDelayed()
        {
            DebugLog("ShowInitialOrderDelayed 開始");

            // 等待更多幀確保 OrderManager.Start() 已執行
            yield return new WaitForSeconds(0.2f);

            DebugLog($"準備刷新訂單，OrderManager.Instance: {(OrderManager.Instance != null ? "✅" : "❌")}");

            if (OrderManager.Instance != null)
            {
                var activeOrders = OrderManager.Instance.GetActiveOrders();
                DebugLog($"活躍訂單數量: {activeOrders.Count}");

                // 如果有訂單，立即顯示
                if (activeOrders.Count > 0)
                {
                    RefreshCurrentOrderDisplay();
                    RefreshPlate();
                }
                else
                {
                    DebugLog("目前沒有訂單，等待 OnNewOrderStarted 事件");
                    // 不做任何事，等待事件觸發
                }
            }

            DebugLog($"ShowInitialOrderDelayed 完成，orderPanel.display: {(orderPanel != null ? orderPanel.style.display.value.ToString() : "null")}, platePanel.display: {(platePanel != null ? platePanel.style.display.value.ToString() : "null")}");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // 取消訂閱事件
            UnsubscribeFromOrderManagerEvents();
        }
        
        /// <summary>
        /// 重置 UI（重玩關卡時調用）
        /// </summary>
        public void ResetUI()
        {
            DebugLog("重置 CookingUI");
            
            // 清空訂單顯示
            ClearOrder();
            
            // 清空餐盤
            ClearPlate();
            
            // 清空食材元素列表
            ingredientElements.Clear();
            
            // 清空食材容器
            if (ingredientContainer != null)
            {
                ingredientContainer.Clear();
            }
            
            // 隱藏面板
            if (orderPanel != null)
            {
                orderPanel.style.display = DisplayStyle.None;
            }
            if (platePanel != null)
            {
                platePanel.style.display = DisplayStyle.None;
            }
            
            DebugLog("CookingUI 重置完成");
        }

        /// <summary>
        /// 初始化食材圖示映射
        /// </summary>
        private void InitializeFoodSprites()
        {
            foodSprites = new Dictionary<FoodType, Sprite>
            {
                { FoodType.Bread, breadSprite },
                { FoodType.Cheese, cheeseSprite },
                { FoodType.Egg, eggSprite },
                { FoodType.Lettuce, lettuceSprite },
                { FoodType.Steak, steakSprite },
                { FoodType.Tomato, tomatoSprite }
            };

            DebugLog("食材圖示映射初始化完成");
        }

        /// <summary>
        /// 初始化 UI
        /// </summary>
        private void InitializeUI()
        {
            DebugLog("初始化料理 UI...");

            // 獲取 UIDocument
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument == null)
            {
                uiDocument = FindFirstObjectByType<UIDocument>();
            }

            if (uiDocument == null)
            {
                DebugLog("錯誤：找不到 UIDocument");
                return;
            }

            // 獲取根元素
            rootElement = uiDocument.rootVisualElement;
            if (rootElement == null)
            {
                DebugLog("錯誤：rootVisualElement 為 null");
                return;
            }

            // 初始化食材圖示映射
            InitializeFoodSprites();

            // 創建 UI 元素
            CreateOrderPanel();
            CreatePlatePanel();

            DebugLog("料理 UI 初始化完成");
        }

        /// <summary>
        /// 創建訂單面板
        /// </summary>
        private void CreateOrderPanel()
        {
            // 創建主面板
            orderPanel = new VisualElement();
            orderPanel.name = "CookingOrderPanel";
            orderPanel.AddToClassList("cooking-ui-panel");  // 添加 CSS 類以設定 z-index

            // 設定響應式樣式 - 參考 MainUI 的 top-panel 設計
            orderPanel.style.display = DisplayStyle.Flex;  // 確保 Flex 布局
            orderPanel.style.position = Position.Absolute;
            orderPanel.style.width = 1400;  // 寬度變成4倍（400 * 4）
            orderPanel.style.height = new StyleLength(new Length(70, LengthUnit.Percent)); // 進一步增加高度以容納6個食材（從60%增加到75%）
            orderPanel.style.left = 100;  // 向右移動
            orderPanel.style.top = new StyleLength(new Length(50, LengthUnit.Percent));    // 再向上移動一些（從45%改為40%）
            orderPanel.style.translate = new StyleTranslate(new Translate(0, new Length(-50, LengthUnit.Percent))); // 垂直置中
            
            // 使用透明背景配合邊框營造玻璃感
            orderPanel.style.backgroundColor = new Color(0.95f, 0.92f, 0.85f, 0.95f); // 淺米色背景
            orderPanel.style.borderTopWidth = 4;
            orderPanel.style.borderBottomWidth = 4;
            orderPanel.style.borderLeftWidth = 4;
            orderPanel.style.borderRightWidth = 4;
            orderPanel.style.borderTopColor = new Color(0.85f, 0.7f, 0.5f, 1f);  // 金棕色邊框
            orderPanel.style.borderBottomColor = new Color(0.85f, 0.7f, 0.5f, 1f);
            orderPanel.style.borderLeftColor = new Color(0.85f, 0.7f, 0.5f, 1f);
            orderPanel.style.borderRightColor = new Color(0.85f, 0.7f, 0.5f, 1f);
            orderPanel.style.borderTopLeftRadius = 20;    // 增大圓角
            orderPanel.style.borderTopRightRadius = 20;
            orderPanel.style.borderBottomLeftRadius = 20;
            orderPanel.style.borderBottomRightRadius = 20;
            orderPanel.style.paddingTop = 10;   // 減小內距以節省空間
            orderPanel.style.paddingBottom = 10;
            orderPanel.style.paddingLeft = 20;
            orderPanel.style.paddingRight = 20;

            // 創建料理名稱標籤 - 參考 MainUI 的大字體設計
            dishNameLabel = new Label("等待訂單...");
            dishNameLabel.name = "DishNameLabel";
            dishNameLabel.style.fontSize = 120;  // 縮小字體以節省空間
            dishNameLabel.style.color = new Color(0.69f, 0.46f, 0.24f, 1f); // 棕色文字
            dishNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            dishNameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            dishNameLabel.style.marginBottom = 10;  // 減小底部間距
            dishNameLabel.style.paddingTop = 10;    // 減小上方內距
            dishNameLabel.style.paddingBottom = 10; // 減小下方內距
            // 添加文字陰影效果（模擬 MainUI 的 text-shadow）
            
            // 創建分隔線
            var separator = new VisualElement();
            separator.style.height = 8;  // 縮小分隔線高度以節省空間
            separator.style.backgroundColor = new Color(0.85f, 0.7f, 0.5f, 0.8f); // 金棕色分隔線
            separator.style.marginBottom = 10;  // 減小底部間距
            separator.style.marginLeft = 15;    // 增加左右邊距
            separator.style.marginRight = 15;
            separator.style.borderTopLeftRadius = 2;
            separator.style.borderTopRightRadius = 2;
            separator.style.borderBottomLeftRadius = 2;
            separator.style.borderBottomRightRadius = 2;

            // 創建食材容器 - 添加滾動功能以支持多個食材
            var scrollView = new ScrollView();
            scrollView.name = "IngredientScrollView";
            scrollView.style.flexGrow = 1;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            
            ingredientContainer = new VisualElement();
            ingredientContainer.name = "IngredientContainer";
            ingredientContainer.style.flexGrow = 1;
            
            scrollView.Add(ingredientContainer);

            // 組裝面板
            orderPanel.Add(dishNameLabel);
            orderPanel.Add(separator);
            orderPanel.Add(scrollView);

            // 添加到根元素（確保在 Cover 之前）
            var coverElement = rootElement.Q<VisualElement>("Cover");
            if (coverElement != null)
            {
                // 在 Cover 之前插入
                int coverIndex = rootElement.IndexOf(coverElement);
                rootElement.Insert(coverIndex, orderPanel);
            }
            else
            {
                // 如果找不到 Cover，直接添加
                rootElement.Add(orderPanel);
            }

            // 初始時隱藏
            orderPanel.style.display = DisplayStyle.None;

            DebugLog("訂單面板創建完成");
        }

        /// <summary>
        /// 創建餐盤面板
        /// </summary>
        private void CreatePlatePanel()
        {
            // 創建主面板
            platePanel = new VisualElement();
            platePanel.name = "CookingPlatePanel";
            platePanel.AddToClassList("cooking-ui-panel");  // 添加 CSS 類以設定 z-index

            // 設定樣式 - 使用餐盤 sprite 作為背景
            platePanel.style.position = Position.Absolute;
            platePanel.style.right = -platePanelPosition.x - 250;  // 往左移動80px
            platePanel.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent)); // 高度在整個畫面的1/2
            platePanel.style.translate = new StyleTranslate(new Translate(0, new Length(50, LengthUnit.Percent))); // 垂直置中
            platePanel.style.width = platePanelSize.x * 6;   // 放大5倍
            platePanel.style.height = platePanelSize.y * 6;  // 放大5倍
            
            // 嘗試載入餐盤背景圖片
            Sprite plateSprite = platePanelBackground;
            if (plateSprite == null)
            {
                // 嘗試從 Resources 載入餐盤圖片（注意路徑需要在 Resources 資料夾下）
                // 如果圖片不在 Resources 資料夾，需要在 Inspector 中手動設定
                plateSprite = Resources.Load<Sprite>("UI/Food/plate");
                
                if (plateSprite == null)
                {
                    DebugLog("⚠️ 未找到餐盤圖片，請在 CookingSystemSetup 中設定 platePanelBackground");
                }
            }
            
            if (plateSprite != null)
            {
                platePanel.style.backgroundImage = new StyleBackground(plateSprite);
                platePanel.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
            
            // 透明背景，無邊框
            platePanel.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            platePanel.style.borderTopWidth = 0;
            platePanel.style.borderBottomWidth = 0;
            platePanel.style.borderLeftWidth = 0;
            platePanel.style.borderRightWidth = 0;
            platePanel.style.paddingTop = 30;
            platePanel.style.paddingBottom = 30;
            platePanel.style.paddingLeft = 30;
            platePanel.style.paddingRight = 30;

            // 創建圖示容器 - 直接放在餐盤上，每3個換一行
            plateIconContainer = new VisualElement();
            plateIconContainer.name = "PlateIconContainer";
            plateIconContainer.style.position = Position.Absolute;  // 使用絕對定位
            plateIconContainer.style.flexDirection = FlexDirection.Row;
            plateIconContainer.style.flexWrap = Wrap.Wrap;
            plateIconContainer.style.justifyContent = Justify.Center;
            plateIconContainer.style.alignItems = Align.Center;
            plateIconContainer.style.alignContent = Align.Center;
            plateIconContainer.style.width = 850;  // 固定寬度以容納3個圖標
            plateIconContainer.style.height = Length.Percent(100);
            plateIconContainer.style.left = Length.Percent(50);  // 水平置中
            plateIconContainer.style.top = Length.Percent(50);   // 垂直置中
            plateIconContainer.style.translate = new StyleTranslate(new Translate(Length.Percent(-50), Length.Percent(-50)));  // 偏移以真正居中

            // 組裝面板 - 只添加圖示容器，不需要標題
            platePanel.Add(plateIconContainer);

            // 添加到根元素（確保在 Cover 之前）
            var coverElement = rootElement.Q<VisualElement>("Cover");
            if (coverElement != null)
            {
                // 在 Cover 之前插入
                int coverIndex = rootElement.IndexOf(coverElement);
                rootElement.Insert(coverIndex, platePanel);
            }
            else
            {
                // 如果找不到 Cover，直接添加
                rootElement.Add(platePanel);
            }

            // 初始時隱藏
            platePanel.style.display = DisplayStyle.None;

            // 創建剩餘訂單數標籤 - 獨立於餐盤，類似剩餘時間
            remainingOrdersLabel = new Label();
            remainingOrdersLabel.name = "RemainingOrdersLabel";
            remainingOrdersLabel.style.position = Position.Absolute;
            remainingOrdersLabel.style.right = -platePanelPosition.x - 70;  // 再往左移動 (從 -100 改為 +50)
            remainingOrdersLabel.style.bottom = new StyleLength(new Length(15, LengthUnit.Percent)); // 螢幕底部往上15%
            remainingOrdersLabel.style.translate = new StyleTranslate(new Translate(0, 0));
            remainingOrdersLabel.style.fontSize = 120;  // 再放大字體 (從 150 改為 180)
            remainingOrdersLabel.style.color = new Color(0.69f, 0.46f, 0.24f, 1f);  // 棕色文字
            remainingOrdersLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            remainingOrdersLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            remainingOrdersLabel.style.whiteSpace = WhiteSpace.NoWrap;

            // 添加剩餘訂單數標籤到根元素
            if (coverElement != null)
            {
                int coverIndex = rootElement.IndexOf(coverElement);
                rootElement.Insert(coverIndex, remainingOrdersLabel);
            }
            else
            {
                rootElement.Add(remainingOrdersLabel);
            }

            // 初始化剩餘訂單數顯示
            UpdateRemainingOrdersDisplay();

            DebugLog("餐盤面板創建完成");
        }

        /// <summary>
        /// 更新餐盤背景圖片（在 ApplyUISettings 後調用）
        /// </summary>
        public void UpdatePlatePanelBackground()
        {
            if (platePanel == null)
            {
                DebugLog("⚠️ 餐盤面板尚未創建，無法更新背景");
                return;
            }

            if (platePanelBackground != null)
            {
                platePanel.style.backgroundImage = new StyleBackground(platePanelBackground);
                platePanel.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                platePanel.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
                // 清除邊框（如果之前有設置的話）
                platePanel.style.borderTopWidth = 0;
                platePanel.style.borderBottomWidth = 0;
                platePanel.style.borderLeftWidth = 0;
                platePanel.style.borderRightWidth = 0;
                DebugLog($"✓ 餐盤背景已更新: {platePanelBackground.name}");
            }
            else
            {
                DebugLog("⚠️ platePanelBackground 為 null，無法更新背景");
            }
        }

        /// <summary>
        /// 更新訂單顯示
        /// </summary>
        /// <param name="recipe">配方</param>
        /// <param name="collected">已收集食材</param>
        /// <param name="required">所需食材</param>
        public void UpdateOrderDisplay(Recipe recipe, Dictionary<FoodType, int> collected, Dictionary<FoodType, int> required)
        {
            if (recipe == null || orderPanel == null)
            {
                DebugLog("UpdateOrderDisplay: recipe 或 orderPanel 為 null");
                return;
            }

            DebugLog($"更新訂單顯示: {recipe.dishName}");

            // 更新料理名稱
            dishNameLabel.text = recipe.dishName;

            // 清除現有食材元素
            ClearIngredientElements();

            // 創建新的食材進度元素
            foreach (var requiredItem in required)
            {
                FoodType foodType = requiredItem.Key;
                int requiredAmount = requiredItem.Value;
                int collectedAmount = collected.ContainsKey(foodType) ? collected[foodType] : 0;

                CreateIngredientProgressElement(foodType, collectedAmount, requiredAmount);
            }

            // 顯示訂單面板
            orderPanel.style.display = DisplayStyle.Flex;
            DebugLog($"訂單面板已顯示：{recipe.dishName}");
            
            // 同時顯示餐盤面板（確保重新開始時也顯示）
            if (platePanel != null)
            {
                platePanel.style.display = DisplayStyle.Flex;
                DebugLog("餐盤面板已顯示");
            }
        }

        /// <summary>
        /// 創建食材進度元素
        /// </summary>
        private void CreateIngredientProgressElement(FoodType foodType, int collected, int required)
        {
            // 創建容器
            var container = new VisualElement();
            container.style.display = DisplayStyle.Flex;  // 確保使用 Flex 布局
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginBottom = 15;   // 進一步減小間距以容納更多食材
            container.style.height = 200;        // 減小列高以容納6個食材
            container.style.minHeight = 200;     // 設定最小高度，防止被壓縮
            container.style.flexShrink = 0;      // 防止收縮
            container.style.paddingLeft = 10;   // 增加左右內距
            container.style.paddingRight = 10;
            container.style.backgroundColor = new Color(1f, 1f, 1f, 0.4f); // 半透明白色背景
            container.style.borderTopLeftRadius = 12;
            container.style.borderTopRightRadius = 12;
            container.style.borderBottomLeftRadius = 12;
            container.style.borderBottomRightRadius = 12;

            // 創建食材圖示 - 調整尺寸以配合容器高度
            var icon = new VisualElement();
            icon.style.width = 160;   // 縮小圖示尺寸以配合200px容器高度
            icon.style.height = 160;
            icon.style.borderTopLeftRadius = 16;
            icon.style.borderTopRightRadius = 16;
            icon.style.borderBottomLeftRadius = 16;
            icon.style.borderBottomRightRadius = 16;
            icon.style.marginRight = 30;  // 增加右邊距
            // 移除白色邊框
            icon.style.borderTopWidth = 0;
            icon.style.borderBottomWidth = 0;
            icon.style.borderLeftWidth = 0;
            icon.style.borderRightWidth = 0;
            icon.style.borderTopColor = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            icon.style.borderBottomColor = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            icon.style.borderLeftColor = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            icon.style.borderRightColor = new Color(0.9f, 0.9f, 0.9f, 0.8f);

            // 設定圖示或顏色
            if (foodSprites != null && foodSprites.ContainsKey(foodType) && foodSprites[foodType] != null)
            {
                icon.style.backgroundImage = new StyleBackground(foodSprites[foodType]);
                // 使用 ScaleToFit 並設定背景位置為居中，避免邊緣殘留
                icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                icon.style.unityBackgroundImageTintColor = Color.white;
            }
            else
            {
                // 備案：使用顏色方塊
                icon.style.backgroundColor = GetFoodTypeColor(foodType);
            }

            // 創建進度標籤 - 參考 MainUI 的大字體設計
            // 直接顯示實際數量
            var progressLabel = new Label($"{foodType}: {collected}/{required}");
            progressLabel.style.fontSize = 120;  // 再放大兩倍（60 * 2）
            progressLabel.style.color = collected >= required ? new Color(0.2f, 0.7f, 0.2f, 1f) : new Color(0.25f, 0.16f, 0.41f, 1f); // 深紫色文字
            progressLabel.style.flexGrow = 1;
            progressLabel.style.unityFontStyleAndWeight = FontStyle.Bold;  // 加粗字體

            // 創建進度條背景
            var progressBarBg = new VisualElement();
            progressBarBg.style.width = 300;  // 再放大兩倍（150 * 2）
            progressBarBg.style.height = 40; // 再放大兩倍（20 * 2）
            progressBarBg.style.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
            progressBarBg.style.borderTopLeftRadius = 6;
            progressBarBg.style.borderTopRightRadius = 6;
            progressBarBg.style.borderBottomLeftRadius = 6;
            progressBarBg.style.borderBottomRightRadius = 6;
            progressBarBg.style.marginLeft = 10;
            progressBarBg.style.borderTopWidth = 2;
            progressBarBg.style.borderBottomWidth = 2;
            progressBarBg.style.borderLeftWidth = 2;
            progressBarBg.style.borderRightWidth = 2;
            progressBarBg.style.borderTopColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            progressBarBg.style.borderBottomColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            progressBarBg.style.borderLeftColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            progressBarBg.style.borderRightColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);

            // 創建進度條填充 - 使用實際數量計算進度
            var progressFill = new VisualElement();
            float progress = required > 0 ? (float)collected / required : 0f;
            progressFill.style.width = Length.Percent(progress * 100);
            progressFill.style.height = 40;
            progressFill.style.backgroundColor = collected >= required ? new Color(0.3f, 0.8f, 0.3f, 1f) : new Color(1f, 0.8f, 0.2f, 1f); // 綠色或金黃色
            progressFill.style.borderTopLeftRadius = 4;
            progressFill.style.borderTopRightRadius = 4;
            progressFill.style.borderBottomLeftRadius = 4;
            progressFill.style.borderBottomRightRadius = 4;

            // 組裝
            progressBarBg.Add(progressFill);
            container.Add(icon);
            container.Add(progressLabel);
            container.Add(progressBarBg);

            ingredientContainer.Add(container);
            
            // 強制刷新布局，確保樣式正確應用
            container.MarkDirtyRepaint();

            // 記錄元素
            var element = new IngredientProgressElement
            {
                foodType = foodType,
                container = container,
                progressLabel = progressLabel,
                progressBar = progressBarBg,
                progressFill = progressFill
            };

            ingredientElements.Add(element);
        }

        /// <summary>
        /// 獲取食材類型對應的顏色
        /// </summary>
        private Color GetFoodTypeColor(FoodType foodType)
        {
            return foodType switch
            {
                FoodType.Bread => new Color(1.0f, 0.8f, 0.4f), // 淺橙色
                FoodType.Cheese => new Color(1.0f, 1.0f, 0.4f), // 淺黃色
                FoodType.Egg => new Color(1.0f, 0.9f, 0.7f), // 蛋黃色
                FoodType.Lettuce => new Color(0.4f, 0.8f, 0.4f), // 綠色
                FoodType.Steak => new Color(0.8f, 0.4f, 0.4f), // 紅色
                FoodType.Tomato => new Color(1.0f, 0.4f, 0.4f), // 番茄紅
                _ => Color.white
            };
        }

        /// <summary>
        /// 添加食材到餐盤
        /// </summary>
        /// <param name="foodType">食材類型</param>
        public void AddIngredientToPlate(FoodType foodType)
        {
            AddIngredientToPlate(foodType, null);
        }

        /// <summary>
        /// 添加食材到餐盤
        /// </summary>
        /// <param name="foodType">食材類型</param>
        /// <param name="customIcon">自定義圖示（可選）</param>
        public void AddIngredientToPlate(FoodType foodType, Sprite customIcon)
        {
            if (plateIconContainer == null)
            {
                DebugLog("AddIngredientToPlate: plateIconContainer 為 null");
                return;
            }

            DebugLog($"添加食材到餐盤: {foodType}");

            // 創建圖示元素 - 配合放大的餐盤增大尺寸，無邊框
            var iconElement = new VisualElement();
            iconElement.style.width = 200;   // 增大圖示尺寸以配合放大的餐盤
            iconElement.style.height = 200;
            iconElement.style.marginLeft = 35;  // 左右邊距35px，確保3個一排並居中
            iconElement.style.marginRight = 35;
            iconElement.style.marginTop = 15;
            iconElement.style.marginBottom = 15;
            iconElement.style.borderTopLeftRadius = 20;
            iconElement.style.borderTopRightRadius = 20;
            iconElement.style.borderBottomLeftRadius = 20;
            iconElement.style.borderBottomRightRadius = 20;
            // 移除白色邊框
            iconElement.style.borderTopWidth = 0;
            iconElement.style.borderBottomWidth = 0;
            iconElement.style.borderLeftWidth = 0;
            iconElement.style.borderRightWidth = 0;

            // 選擇圖示來源：優先使用自定義圖示，其次使用映射圖示，最後使用顏色
            Sprite spriteToUse = customIcon;
            if (spriteToUse == null && foodSprites != null && foodSprites.ContainsKey(foodType))
            {
                spriteToUse = foodSprites[foodType];
            }

            // 設定圖示或顏色
            if (spriteToUse != null)
            {
                iconElement.style.backgroundImage = new StyleBackground(spriteToUse);
                // 使用 ScaleToFit 並設定背景位置為居中，避免邊緣殘留
                iconElement.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                iconElement.style.unityBackgroundImageTintColor = Color.white;
            }
            else
            {
                // 備案：使用食材對應的顏色
                iconElement.style.backgroundColor = GetFoodTypeColor(foodType);
            }

            plateIconContainer.Add(iconElement);
            plateIcons.Add(iconElement);

            // 顯示餐盤
            platePanel.style.display = DisplayStyle.Flex;

            // 不再播放添加動畫（因為現在是批量渲染）
            // StartCoroutine(PlayAddIconAnimation(iconElement));
        }

        /// <summary>
        /// 播放餐盤滑動動畫
        /// </summary>
        public void PlayPlateSlideAnimation()
        {
            if (isAnimating || platePanel == null)
            {
                DebugLog("PlayPlateSlideAnimation: 正在動畫中或 platePanel 為 null");
                return;
            }

            DebugLog("播放餐盤滑動動畫");
            StartCoroutine(PlateSlideAnimationCoroutine());
        }

        /// <summary>
        /// 餐盤滑動動畫協程 - 往右滑出並淡出
        /// </summary>
        private IEnumerator PlateSlideAnimationCoroutine()
        {
            isAnimating = true;

            float startRight = -platePanelPosition.x - 150;  // 當前位置
            float endRight = startRight - plateSlideDistance * 2;  // 往右滑動更遠（減少right值）

            float elapsedTime = 0f;
            float totalDuration = slideAnimationDuration * 1.5f;  // 增加動畫時長，讓它更慢

            // 滑出動畫（往右）+ 淡出效果
            while (elapsedTime < totalDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / totalDuration;
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

                // 滑動位置
                float currentRight = Mathf.Lerp(startRight, endRight, easedProgress);
                platePanel.style.right = currentRight;

                // 淡出效果（從不透明到透明）
                float opacity = Mathf.Lerp(1f, 0f, easedProgress);
                platePanel.style.opacity = opacity;

                yield return null;
            }

            // 等待一下
            yield return new WaitForSeconds(0.3f);

            // 重置位置和透明度，並清空
            platePanel.style.right = startRight;
            platePanel.style.opacity = 1f;
            ClearPlate();

            isAnimating = false;
        }

        /// <summary>
        /// 播放添加圖示動畫
        /// </summary>
        private IEnumerator PlayAddIconAnimation(VisualElement iconElement)
        {
            // 起始縮放為 0
            iconElement.style.scale = new StyleScale(Vector2.zero);

            float duration = 0.3f;
            float elapsedTime = 0f;

            // 縮放動畫
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float scale = Mathf.Lerp(0f, 1f, progress);

                iconElement.style.scale = new StyleScale(Vector2.one * scale);

                yield return null;
            }

            iconElement.style.scale = new StyleScale(Vector2.one);
        }

        /// <summary>
        /// 清空餐盤
        /// </summary>
        public void ClearPlate()
        {
            if (plateIconContainer != null)
            {
                plateIconContainer.Clear();
                plateIcons.Clear();
                platePanel.style.display = DisplayStyle.None;
                DebugLog("餐盤已清空");
            }
        }

        /// <summary>
        /// 清空訂單
        /// </summary>
        public void ClearOrder()
        {
            if (orderPanel != null)
            {
                dishNameLabel.text = "無訂單";
                ClearIngredientElements();
                orderPanel.style.display = DisplayStyle.None;
                DebugLog("訂單已清空");
            }
        }

        /// <summary>
        /// 清除食材元素
        /// </summary>
        private void ClearIngredientElements()
        {
            ingredientContainer?.Clear();
            ingredientElements.Clear();
        }

        /// <summary>
        /// 訂閱 OrderManager 事件
        /// </summary>
        private void SubscribeToOrderManagerEvents()
        {
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.OnNewOrderStarted.AddListener(OnNewOrderStarted);
                OrderManager.Instance.OnIngredientCollected.AddListener(OnIngredientCollected);
                OrderManager.Instance.OnOrderCompleted.AddListener(OnOrderCompleted);
                OrderManager.Instance.OnOrderExpired.AddListener(OnOrderExpired);
                DebugLog("已訂閱 OrderManager 事件");
            }
        }

        /// <summary>
        /// 取消訂閱 OrderManager 事件
        /// </summary>
        private void UnsubscribeFromOrderManagerEvents()
        {
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.OnNewOrderStarted.RemoveListener(OnNewOrderStarted);
                OrderManager.Instance.OnIngredientCollected.RemoveListener(OnIngredientCollected);
                OrderManager.Instance.OnOrderCompleted.RemoveListener(OnOrderCompleted);
                OrderManager.Instance.OnOrderExpired.RemoveListener(OnOrderExpired);
            }
        }

        /// <summary>
        /// 新訂單開始事件處理
        /// </summary>
        private void OnNewOrderStarted(Recipe recipe)
        {
            DebugLog($"收到新訂單事件: {recipe.dishName}");

            var activeOrders = OrderManager.Instance.GetActiveOrders();
            var order = activeOrders.Find(o => o.recipe.dishName == recipe.dishName);

            if (order != null)
            {
                UpdateOrderDisplay(recipe, order.collectedIngredients, recipe.RequiredIngredients);
                UpdateRemainingOrdersDisplay();  // 更新剩餘訂單數
            }
        }

        /// <summary>
        /// 食材收集事件處理
        /// </summary>
        private void OnIngredientCollected(FoodType foodType, int amount)
        {
            DebugLog($"收到食材收集事件: {foodType} +{amount}");

            // 更新當前訂單顯示
            RefreshCurrentOrderDisplay();

            // 重新渲染餐盤（根據當前訂單需求和已收集數量）
            RefreshPlate();
        }

        /// <summary>
        /// 訂單完成事件處理
        /// </summary>
        private void OnOrderCompleted(Recipe recipe, int _)
        {
            DebugLog($"收到訂單完成事件: {recipe.dishName}");

            // 播放餐盤動畫
            PlayPlateSlideAnimation();

            // 更新剩餘訂單數
            UpdateRemainingOrdersDisplay();

            // 延遲顯示下一個訂單（而不是清空）
            StartCoroutine(DelayedShowNextOrder());
        }

        /// <summary>
        /// 訂單過期事件處理
        /// </summary>
        private void OnOrderExpired(Recipe recipe)
        {
            DebugLog($"收到訂單過期事件: {recipe.dishName}");
            ClearOrder();
            ClearPlate();
        }

        /// <summary>
        /// 延遲清空訂單
        /// </summary>
        private IEnumerator DelayedClearOrder()
        {
            yield return new WaitForSeconds(slideAnimationDuration + 1.0f);
            ClearOrder();
        }
        
        /// <summary>
        /// 延遲顯示下一個訂單
        /// </summary>
        private IEnumerator DelayedShowNextOrder()
        {
            yield return new WaitForSeconds(slideAnimationDuration + 1.0f);
            
            // 檢查是否還有新訂單
            if (OrderManager.Instance != null)
            {
                var activeOrders = OrderManager.Instance.GetActiveOrders();
                if (activeOrders.Count > 0)
                {
                    // 還有訂單，顯示下一個
                    var nextOrder = activeOrders[0];
                    DebugLog($"顯示下一個訂單: {nextOrder.recipe.dishName}");
                    UpdateOrderDisplay(nextOrder.recipe, nextOrder.collectedIngredients, nextOrder.recipe.RequiredIngredients);
                    
                    // 重新渲染餐盤（新訂單從頭開始）
                    RefreshPlate();
                }
                else
                {
                    // 沒有訂單了，清空顯示
                    DebugLog("所有訂單已完成");
                    ClearOrder();
                    ClearPlate();
                }
            }
        }

        /// <summary>
        /// 重新渲染餐盤（根據當前訂單需求顯示食材）
        /// </summary>
        private void RefreshPlate()
        {
            if (OrderManager.Instance == null || plateIconContainer == null)
                return;

            // 清空當前餐盤
            ClearPlate();

            var activeOrders = OrderManager.Instance.GetActiveOrders();
            if (activeOrders.Count == 0)
                return;

            var order = activeOrders[0];
            var recipe = order.recipe;

            // 只顯示當前訂單需要的食材
            foreach (var requiredIngredient in recipe.RequiredIngredients)
            {
                FoodType foodType = requiredIngredient.Key;
                int requiredAmount = requiredIngredient.Value;

                // 獲取已收集數量
                int collectedAmount = order.collectedIngredients.ContainsKey(foodType) 
                    ? order.collectedIngredients[foodType] 
                    : 0;

                // 計算應該顯示的圖標數量 = 已收集數量 / 3（向下取整）
                int iconsToShow = collectedAmount / 3;

                // 添加對應數量的圖標到餐盤
                for (int i = 0; i < iconsToShow; i++)
                {
                    AddIngredientToPlate(foodType);
                }

                DebugLog($"餐盤顯示 {foodType}: {iconsToShow} 個圖標 (已收集 {collectedAmount}/需求 {requiredAmount})");
            }

            // 始終顯示餐盤（即使沒有食材圖標，也要顯示空餐盤）
            platePanel.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// 刷新當前訂單顯示
        /// </summary>
        private void RefreshCurrentOrderDisplay()
        {
            DebugLog($"RefreshCurrentOrderDisplay 被調用，orderPanel: {(orderPanel != null ? "存在" : "null")}");

            if (OrderManager.Instance != null)
            {
                var activeOrders = OrderManager.Instance.GetActiveOrders();
                DebugLog($"取得活躍訂單數量: {activeOrders.Count}");

                if (activeOrders.Count > 0)
                {
                    var order = activeOrders[0];
                    DebugLog($"刷新訂單顯示: {order.recipe.dishName}");
                    UpdateOrderDisplay(order.recipe, order.collectedIngredients, order.recipe.RequiredIngredients);

                    // 確保訂單面板顯示
                    if (orderPanel != null)
                    {
                        orderPanel.style.display = DisplayStyle.Flex;
                        DebugLog($"訂單面板已設置為 Flex，當前 display: {orderPanel.style.display.value}");
                    }
                    
                    // 確保餐盤面板也顯示（即使是空的）
                    if (platePanel != null)
                    {
                        platePanel.style.display = DisplayStyle.Flex;
                        DebugLog("餐盤面板已顯示");
                    }
                }
                else
                {
                    DebugLog("沒有活躍訂單可顯示");
                }
            }
            else
            {
                DebugLog("OrderManager.Instance 為 null，無法刷新訂單");
            }
        }

        /// <summary>
        /// 更新剩餘訂單數顯示
        /// </summary>
        private void UpdateRemainingOrdersDisplay()
        {
            if (remainingOrdersLabel == null)
                return;

            // 清空現有內容
            remainingOrdersLabel.Clear();

            // 如果還沒初始化完成，先顯示預設文字
            if (OrderManager.Instance == null || LevelData.Instance == null)
            {
                remainingOrdersLabel.text = "Loading...";
                remainingOrdersLabel.style.display = DisplayStyle.Flex;
                return;
            }

            int completed = OrderManager.Instance.CompletedOrderCount;
            int required = LevelData.Instance.RequiredOrderCount;
            int remaining = required - completed;

            if (remaining > 0)
            {
                string dishText = remaining == 1 ? "dish" : "dishes";

                // 使用 rich text 格式讓數字部分更大且為黃色
                remainingOrdersLabel.enableRichText = true;
                remainingOrdersLabel.text = $"<size=180><color=#F25A2D>{remaining}</color></size>  more {dishText} to go!!";
                remainingOrdersLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                remainingOrdersLabel.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// 調試日誌
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[CookingUIManager] {message}");
            }
        }

        /// <summary>
        /// 手動測試方法
        /// </summary>
        [ContextMenu("測試訂單顯示")]
        public void TestOrderDisplay()
        {
            if (OrderManager.Instance != null)
            {
                var activeOrders = OrderManager.Instance.GetActiveOrders();
                if (activeOrders.Count > 0)
                {
                    var order = activeOrders[0];
                    UpdateOrderDisplay(order.recipe, order.collectedIngredients, order.recipe.RequiredIngredients);
                }
            }
        }

        [ContextMenu("測試餐盤動畫")]
        public void TestPlateAnimation()
        {
            AddIngredientToPlate(FoodType.Bread);
            AddIngredientToPlate(FoodType.Cheese);
            AddIngredientToPlate(FoodType.Egg);
            PlayPlateSlideAnimation();
        }
    }
}