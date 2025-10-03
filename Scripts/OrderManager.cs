using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Match3
{
    /// <summary>
    /// 訂單管理器 - 管理當前訂單和食材收集進度
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        public static OrderManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticData()
        {
            Instance = null;
        }

        [Header("訂單設定")]
        [Tooltip("(已廢棄) 現在使用 RecipeLibrary 直接從代碼讀取配方")]
        public RecipeDatabase recipeDatabase;                          // 配方資料庫（已廢棄，保留以兼容舊場景）
        public int maxConcurrentOrders = 3;                           // 最多同時進行的訂單數
        public float orderTimeLimit = 120f;                          // 訂單時間限制（秒）

        [Header("調試")]
        public bool enableDebugLog = true;                           // 是否啟用調試日誌

        // 當前活躍的訂單
        private List<ActiveOrder> activeOrders = new List<ActiveOrder>();
        
        // 已完成的訂單數量
        private int completedOrderCount = 0;
        public int CompletedOrderCount => completedOrderCount;

        // 事件系統
        public UnityEvent<FoodType, int> OnIngredientCollected;      // 食材被收集事件
        public UnityEvent<Recipe, int> OnOrderCompleted;             // 訂單完成事件
        public UnityEvent<Recipe> OnNewOrderStarted;                 // 新訂單開始事件
        public UnityEvent<Recipe> OnOrderExpired;                    // 訂單過期事件

        // C# 事件（用於程式訂閱）
        public static event Action<FoodType, int> IngredientCollected;
        public static event Action<Recipe, int> OrderCompleted;
        public static event Action<Recipe> NewOrderStarted;
        public static event Action<Recipe> OrderExpired;

        /// <summary>
        /// 活躍訂單類別
        /// </summary>
        [Serializable]
        public class ActiveOrder
        {
            public Recipe recipe;                                    // 配方
            public Dictionary<FoodType, int> collectedIngredients;   // 已收集的食材
            public float remainingTime;                              // 剩餘時間
            public int orderID;                                      // 訂單ID

            public ActiveOrder(Recipe recipe, int id, float timeLimit)
            {
                this.recipe = recipe;
                this.orderID = id;
                this.remainingTime = timeLimit;
                this.collectedIngredients = new Dictionary<FoodType, int>();

                // 初始化已收集食材為0
                foreach (var ingredient in recipe.RequiredIngredients.Keys)
                {
                    collectedIngredients[ingredient] = 0;
                }
            }

            /// <summary>
            /// 檢查訂單是否完成
            /// </summary>
            public bool IsComplete()
            {
                foreach (var required in recipe.RequiredIngredients)
                {
                    if (!collectedIngredients.ContainsKey(required.Key) ||
                        collectedIngredients[required.Key] < required.Value)
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// 獲取完成進度百分比
            /// </summary>
            public float GetProgressPercentage()
            {
                int totalRequired = recipe.RequiredIngredients.Values.Sum();
                int totalCollected = 0;

                foreach (var required in recipe.RequiredIngredients)
                {
                    int collected = collectedIngredients.ContainsKey(required.Key) ?
                                  collectedIngredients[required.Key] : 0;
                    totalCollected += Mathf.Min(collected, required.Value);
                }

                return totalRequired > 0 ? (float)totalCollected / totalRequired : 0f;
            }
        }

        private void Awake()
        {
            DebugLog("OrderManager Awake 被調用");
            
            if (Instance == null)
            {
                Instance = this;
                DebugLog("OrderManager 設置為 Singleton Instance");
                InitializeOrderManager();
            }
            else
            {
                DebugLog("已存在另一個 OrderManager Instance，銷毀此物件");
                Destroy(gameObject);
            }
        }
        

        private void Start()
        {
            // 使用新的 RecipeLibrary 系統
            var recipes = RecipeLibrary.GetAllRecipes();
            DebugLog($"OrderManager.Start() 被調用，RecipeLibrary 配方數量: {recipes.Count}");

            // 開始第一個訂單
            if (recipes.Count > 0)
            {
                DebugLog($"準備創建第一個訂單，配方數量: {recipes.Count}");
                StartNewOrder();
            }
            else
            {
                DebugLog($"❌ 錯誤：RecipeLibrary 沒有配方，無法創建訂單");
            }
        }

        private void Update()
        {
            // 更新訂單計時器
            UpdateOrderTimers();
        }

        /// <summary>
        /// 初始化訂單管理器
        /// </summary>
        private void InitializeOrderManager()
        {
            activeOrders.Clear();
            completedOrderCount = 0;  // 重置已完成訂單計數

            // 使用新的 RecipeLibrary 系統，不再依賴 RecipeDatabase ScriptableObject
            var recipes = RecipeLibrary.GetAllRecipes();
            DebugLog($"訂單管理器初始化完成，RecipeLibrary 配方數量: {recipes.Count}");
        }

        /// <summary>
        /// 開始新訂單
        /// </summary>
        /// <param name="specificRecipe">指定的配方，null 則隨機選擇</param>
        public void StartNewOrder(Recipe specificRecipe = null)
        {
            if (activeOrders.Count >= maxConcurrentOrders)
            {
                DebugLog("已達到最大訂單數量，無法創建新訂單");
                return;
            }

            // 使用 RecipeLibrary 獲取配方
            Recipe recipe = specificRecipe ?? RecipeLibrary.GetRandomRecipe();
            if (recipe == null)
            {
                Debug.LogError("OrderManager: 無法從 RecipeLibrary 獲取配方!");
                return;
            }

            int orderID = activeOrders.Count > 0 ? activeOrders.Max(o => o.orderID) + 1 : 1;
            var newOrder = new ActiveOrder(recipe, orderID, orderTimeLimit);
            activeOrders.Add(newOrder);

            DebugLog($"開始新訂單: {recipe.dishName} (ID: {orderID})");

            // 觸發事件
            OnNewOrderStarted?.Invoke(recipe);
            NewOrderStarted?.Invoke(recipe);
        }

        /// <summary>
        /// 收集食材
        /// </summary>
        /// <param name="foodType">食材類型</param>
        /// <param name="quantity">數量</param>
        /// <param name="worldPosition">食材在世界座標中的位置（用於飛行動畫）</param>
        public void CollectIngredient(FoodType foodType, int quantity = 1, Vector3 worldPosition = default)
        {
            bool ingredientUsed = false;

            // 對每個活躍訂單嘗試使用食材
            foreach (var order in activeOrders.ToList())
            {
                if (order.recipe.RequiredIngredients.ContainsKey(foodType))
                {
                    int needed = order.recipe.RequiredIngredients[foodType];
                    int currentCollected = order.collectedIngredients[foodType];

                    if (currentCollected < needed)
                    {
                        int canAdd = Mathf.Min(quantity, needed - currentCollected);
                        ingredientUsed = true;

                        DebugLog($"訂單 {order.orderID} 準備收集 {canAdd} 個 {foodType} ({currentCollected + canAdd}/{needed})");

                        // 播放飛行動畫（如果提供了世界位置）
                        if (worldPosition != default && IngredientFlyAnimation.IsInstanceAvailable())
                        {
                            // 獲取餐盤位置（這裡需要根據實際 UI 設置調整）
                            Vector3 platePosition = GetPlateWorldPosition();

                            // 獲取食材圖示
                            Sprite foodSprite = GetFoodTypeSprite(foodType);

                            // 播放飛行動畫，動畫完成後更新進度
                            IngredientFlyAnimation.PlayFlyAnimation(worldPosition, platePosition, foodSprite, () => {
                                // 動畫完成後更新進度
                                order.collectedIngredients[foodType] += canAdd;

                                // 觸發食材收集事件
                                OnIngredientCollected?.Invoke(foodType, canAdd);
                                IngredientCollected?.Invoke(foodType, canAdd);

                                // 通知餐盤 UI 添加圖示
                                if (PlateUI.Instance != null)
                                {
                                    PlateUI.Instance.AddIngredientIcon(foodType, foodSprite);
                                }

                                // 檢查訂單是否完成
                                if (order.IsComplete())
                                {
                                    CompleteOrder(order);
                                }

                                DebugLog($"訂單 {order.orderID} 完成收集 {canAdd} 個 {foodType}");
                            });
                        }
                        else
                        {
                            // 沒有動畫，直接更新進度
                            order.collectedIngredients[foodType] += canAdd;

                            // 觸發食材收集事件
                            OnIngredientCollected?.Invoke(foodType, canAdd);
                            IngredientCollected?.Invoke(foodType, canAdd);

                            // 檢查訂單是否完成
                            if (order.IsComplete())
                            {
                                CompleteOrder(order);
                            }
                        }

                        break; // 每次只對一個訂單使用食材
                    }
                }
            }

            if (!ingredientUsed)
            {
                DebugLog($"收集的 {foodType} 暫時無法使用於任何訂單");
            }
        }

        /// <summary>
        /// 完成訂單
        /// </summary>
        /// <param name="order">要完成的訂單</param>
        private void CompleteOrder(ActiveOrder order)
        {
            DebugLog($"完成訂單: {order.recipe.dishName} (ID: {order.orderID})");

            // 增加完成訂單計數
            completedOrderCount++;

            // 給予時間獎勵
            if (LevelData.Instance != null && LevelData.Instance.UseTimerMode)
            {
                LevelData.Instance.AddTime(order.recipe.timeBonus);
                DebugLog($"獲得時間獎勵: +{order.recipe.timeBonus} 秒");
            }

            // 觸發完成事件
            OnOrderCompleted?.Invoke(order.recipe, order.orderID);
            OrderCompleted?.Invoke(order.recipe, order.orderID);

            // 移除完成的訂單
            activeOrders.Remove(order);

            // 檢查是否達到通關條件
            if (LevelData.Instance != null && completedOrderCount >= LevelData.Instance.RequiredOrderCount)
            {
                DebugLog($"達到訂單目標！已完成 {completedOrderCount}/{LevelData.Instance.RequiredOrderCount} 個訂單");
                
                // 觸發勝利條件
                if (LevelData.Instance.OnAllGoalFinished != null)
                {
                    LevelData.Instance.OnAllGoalFinished.Invoke();
                }
                
                // 停止輸入
                if (GameManager.Instance?.Board != null)
                {
                    GameManager.Instance.Board.ToggleInput(false);
                }
                
                return; // 不再開始新訂單
            }

            // 自動開始新訂單
            StartNewOrder();
        }

        /// <summary>
        /// 更新訂單計時器
        /// </summary>
        private void UpdateOrderTimers()
        {
            for (int i = activeOrders.Count - 1; i >= 0; i--)
            {
                var order = activeOrders[i];
                order.remainingTime -= Time.deltaTime;

                if (order.remainingTime <= 0)
                {
                    DebugLog($"訂單過期: {order.recipe.dishName} (ID: {order.orderID})");

                    // 觸發過期事件
                    OnOrderExpired?.Invoke(order.recipe);
                    OrderExpired?.Invoke(order.recipe);

                    // 移除過期訂單
                    activeOrders.RemoveAt(i);

                    // 可選：開始新訂單來替代過期的訂單
                    StartNewOrder();
                }
            }
        }

        /// <summary>
        /// 檢查是否有任何訂單完成
        /// </summary>
        public bool HasCompletedOrder()
        {
            return activeOrders.Any(order => order.IsComplete());
        }

        /// <summary>
        /// 獲取所有活躍訂單
        /// </summary>
        public List<ActiveOrder> GetActiveOrders()
        {
            return new List<ActiveOrder>(activeOrders);
        }

        /// <summary>
        /// 根據ID獲取訂單
        /// </summary>
        public ActiveOrder GetOrderByID(int orderID)
        {
            return activeOrders.Find(order => order.orderID == orderID);
        }

        /// <summary>
        /// 取消訂單
        /// </summary>
        /// <param name="orderID">要取消的訂單ID</param>
        public void CancelOrder(int orderID)
        {
            var order = GetOrderByID(orderID);
            if (order != null)
            {
                DebugLog($"取消訂單: {order.recipe.dishName} (ID: {orderID})");
                activeOrders.Remove(order);
            }
        }

        /// <summary>
        /// 清除所有訂單
        /// </summary>
        public void ClearAllOrders()
        {
            DebugLog("清除所有訂單");
            activeOrders.Clear();
        }
        
        /// <summary>
        /// 重置訂單系統（重玩關卡時調用）
        /// </summary>
        public void ResetOrderSystem()
        {
            DebugLog("重置訂單系統");
            activeOrders.Clear();
            completedOrderCount = 0;
        }

        /// <summary>
        /// 獲取餐盤世界位置
        /// </summary>
        private Vector3 GetPlateWorldPosition()
        {
            // 這裡需要根據實際 UI 設置來獲取餐盤位置
            // 暫時返回螢幕右側的位置
            Camera camera = Camera.main;
            if (camera == null) camera = FindFirstObjectByType<Camera>();

            if (camera != null)
            {
                // 螢幕右側中央位置
                Vector3 screenPos = new Vector3(Screen.width * 0.8f, Screen.height * 0.5f, 10f);
                return camera.ScreenToWorldPoint(screenPos);
            }

            return Vector3.right * 5f; // 備案位置
        }

        /// <summary>
        /// 獲取食材類型對應的圖示
        /// </summary>
        private Sprite GetFoodTypeSprite(FoodType foodType)
        {
            // 這裡應該根據實際的資源設置來獲取對應的圖示
            // 暫時返回 null，可以後續添加資源映射

            // 示例：可以創建一個食材圖示映射字典
            // return foodTypeSpriteMapping.ContainsKey(foodType) ? foodTypeSpriteMapping[foodType] : null;

            // 暫時使用白色方塊作為佔位符
            if (Texture2D.whiteTexture != null)
            {
                return Sprite.Create(Texture2D.whiteTexture,
                    new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                    Vector2.one * 0.5f);
            }

            return null;
        }

        /// <summary>
        /// 調試日誌
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[OrderManager] {message}");
            }
        }

        /// <summary>
        /// 獲取訂單狀態摘要（調試用）
        /// </summary>
        [ContextMenu("顯示訂單狀態")]
        public void ShowOrderStatus()
        {
            DebugLog($"=== 訂單狀態摘要 (共 {activeOrders.Count} 個) ===");

            foreach (var order in activeOrders)
            {
                string status = $"ID:{order.orderID} | {order.recipe.dishName} | 進度:{order.GetProgressPercentage():P1} | 剩餘時間:{order.remainingTime:F1}s";
                DebugLog(status);

                foreach (var ingredient in order.recipe.RequiredIngredients)
                {
                    int collected = order.collectedIngredients[ingredient.Key];
                    DebugLog($"  • {ingredient.Key}: {collected}/{ingredient.Value}");
                }
            }
        }

        private void OnDestroy()
        {
            DebugLog("OrderManager OnDestroy 被調用");
            
            if (Instance == this)
            {
                // 場景重載時重置訂單計數
                completedOrderCount = 0;
                Instance = null;
                DebugLog("已清除 OrderManager Singleton Instance");
            }
        }
    }
}