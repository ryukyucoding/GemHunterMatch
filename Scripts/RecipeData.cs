using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 食材類型枚舉
    /// </summary>
    public enum FoodType
    {
        Bread = 0,
        Cheese = 1,
        Egg = 2,
        Lettuce = 3,
        Steak = 4,
        Tomato = 5
    }

    /// <summary>
    /// 料理配方類別
    /// </summary>
    [Serializable]
    public class Recipe
    {
        [Header("料理基本資訊")]
        public string dishName;                                          // 料理名稱
        public Sprite dishIcon;                                         // 料理圖示

        [Header("所需食材")]
        [SerializeField] private List<IngredientRequirement> ingredients; // 所需食材列表

        [Header("獎勵")]
        public int timeBonus = 10;                                      // 完成後獎勵的時間（秒）
        public int scoreBonus = 100;                                   // 額外分數獎勵

        /// <summary>
        /// 食材需求的序列化類別（用於在 Inspector 中編輯）
        /// </summary>
        [Serializable]
        public class IngredientRequirement
        {
            public FoodType foodType;
            public int quantity;
        }

        /// <summary>
        /// 獲取所需食材的字典（方便程式使用）
        /// </summary>
        public Dictionary<FoodType, int> RequiredIngredients
        {
            get
            {
                var dict = new Dictionary<FoodType, int>();
                if (ingredients != null)
                {
                    foreach (var ingredient in ingredients)
                    {
                        dict[ingredient.foodType] = ingredient.quantity;
                    }
                }
                return dict;
            }
        }

        /// <summary>
        /// 初始化配方（用於程式動態創建）
        /// </summary>
        public void InitializeRecipe(string name, Sprite icon, Dictionary<FoodType, int> requiredIngredients, int timeReward = 10, int scoreReward = 100)
        {
            dishName = name;
            dishIcon = icon;
            timeBonus = timeReward;
            scoreBonus = scoreReward;

            ingredients = new List<IngredientRequirement>();
            foreach (var kvp in requiredIngredients)
            {
                ingredients.Add(new IngredientRequirement
                {
                    foodType = kvp.Key,
                    quantity = kvp.Value
                });
            }
        }

        /// <summary>
        /// 獲取配方描述文字
        /// </summary>
        public string GetRecipeDescription()
        {
            var description = $"{dishName}:\n";
            foreach (var ingredient in ingredients)
            {
                description += $"• {ingredient.foodType}: {ingredient.quantity}\n";
            }
            description += $"時間獎勵: +{timeBonus}秒";
            return description;
        }
    }

    /// <summary>
    /// 料理資料庫 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Match3/Recipe Database")]
    public class RecipeDatabase : ScriptableObject
    {
        [Header("料理配方清單")]
        public List<Recipe> recipes = new List<Recipe>();

        /// <summary>
        /// 根據名稱查找配方
        /// </summary>
        public Recipe GetRecipeByName(string dishName)
        {
            return recipes.Find(recipe => recipe.dishName.Equals(dishName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 獲取隨機配方
        /// </summary>
        public Recipe GetRandomRecipe()
        {
            if (recipes.Count == 0) return null;
            int randomIndex = UnityEngine.Random.Range(0, recipes.Count);
            return recipes[randomIndex];
        }

        /// <summary>
        /// 根據難度獲取配方（根據所需食材總數判斷難度）
        /// </summary>
        public Recipe GetRecipeByDifficulty(int minIngredients, int maxIngredients)
        {
            var filteredRecipes = recipes.FindAll(recipe =>
            {
                int totalIngredients = 0;
                foreach (var ingredient in recipe.RequiredIngredients.Values)
                {
                    totalIngredients += ingredient;
                }
                return totalIngredients >= minIngredients && totalIngredients <= maxIngredients;
            });

            if (filteredRecipes.Count == 0) return GetRandomRecipe();

            int randomIndex = UnityEngine.Random.Range(0, filteredRecipes.Count);
            return filteredRecipes[randomIndex];
        }

        /// <summary>
        /// 獲取包含特定食材的配方
        /// </summary>
        public List<Recipe> GetRecipesWithIngredient(FoodType foodType)
        {
            return recipes.FindAll(recipe => recipe.RequiredIngredients.ContainsKey(foodType));
        }

        /// <summary>
        /// 驗證資料庫完整性
        /// </summary>
        [ContextMenu("驗證配方資料庫")]
        public void ValidateDatabase()
        {
            Debug.Log($"配方資料庫包含 {recipes.Count} 個配方:");

            foreach (var recipe in recipes)
            {
                if (string.IsNullOrEmpty(recipe.dishName))
                {
                    Debug.LogWarning("發現未命名的配方!");
                    continue;
                }

                if (recipe.dishIcon == null)
                {
                    Debug.LogWarning($"配方 '{recipe.dishName}' 缺少圖示!");
                }

                if (recipe.RequiredIngredients.Count == 0)
                {
                    Debug.LogWarning($"配方 '{recipe.dishName}' 沒有設定所需食材!");
                }

                Debug.Log($"✓ {recipe.dishName}: {recipe.RequiredIngredients.Count} 種食材, +{recipe.timeBonus}秒");
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// 在編輯器中添加預設配方（開發用）
        /// </summary>
        [ContextMenu("添加預設配方")]
        public void AddDefaultRecipes()
        {
            recipes.Clear();

            // === 簡單料理 (2-3種食材) ===

            // 吐司
            var toast = new Recipe();
            toast.InitializeRecipe("奶油吐司", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 }
            }, 8, 80);
            recipes.Add(toast);

            // 煎蛋
            var friedEgg = new Recipe();
            friedEgg.InitializeRecipe("煎蛋", null, new Dictionary<FoodType, int>
            {
                { FoodType.Egg, 2 }
            }, 8, 80);
            recipes.Add(friedEgg);

            // 起司吐司
            var cheeseToast = new Recipe();
            cheeseToast.InitializeRecipe("起司吐司", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 1 },
                { FoodType.Cheese, 1 }
            }, 10, 100);
            recipes.Add(cheeseToast);

            // 蛋吐司
            var eggToast = new Recipe();
            eggToast.InitializeRecipe("蛋吐司", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 1 },
                { FoodType.Egg, 1 }
            }, 10, 100);
            recipes.Add(eggToast);

            // === 中等料理 (3-4種食材) ===

            // 起司蛋堡
            var cheeseBurger = new Recipe();
            cheeseBurger.InitializeRecipe("起司蛋堡", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 },
                { FoodType.Egg, 1 },
                { FoodType.Cheese, 1 }
            }, 12, 120);
            recipes.Add(cheeseBurger);

            // 經典漢堡
            var burger = new Recipe();
            burger.InitializeRecipe("經典漢堡", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 },
                { FoodType.Steak, 1 },
                { FoodType.Lettuce, 1 }
            }, 15, 150);
            recipes.Add(burger);

            // 蔬菜三明治
            var veggieSandwich = new Recipe();
            veggieSandwich.InitializeRecipe("蔬菜三明治", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 },
                { FoodType.Lettuce, 2 },
                { FoodType.Tomato, 1 }
            }, 12, 120);
            recipes.Add(veggieSandwich);

            // 番茄蛋三明治
            var tomatoEggSandwich = new Recipe();
            tomatoEggSandwich.InitializeRecipe("番茄蛋三明治", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 },
                { FoodType.Egg, 1 },
                { FoodType.Tomato, 1 }
            }, 12, 120);
            recipes.Add(tomatoEggSandwich);

            // 起司牛排堡
            var cheeseSteakBurger = new Recipe();
            cheeseSteakBurger.InitializeRecipe("起司牛排堡", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 },
                { FoodType.Steak, 1 },
                { FoodType.Cheese, 1 }
            }, 15, 150);
            recipes.Add(cheeseSteakBurger);

            // === 複雜料理 (4-5種食材) ===

            // 豪華漢堡
            var deluxeBurger = new Recipe();
            deluxeBurger.InitializeRecipe("豪華漢堡", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 },
                { FoodType.Steak, 1 },
                { FoodType.Cheese, 1 },
                { FoodType.Lettuce, 1 },
                { FoodType.Tomato, 1 }
            }, 20, 200);
            recipes.Add(deluxeBurger);

            // 蔬菜總匯
            var veggiePlatter = new Recipe();
            veggiePlatter.InitializeRecipe("蔬菜總匯", null, new Dictionary<FoodType, int>
            {
                { FoodType.Lettuce, 2 },
                { FoodType.Tomato, 2 },
                { FoodType.Cheese, 1 }
            }, 15, 150);
            recipes.Add(veggiePlatter);

            // 早餐拼盤
            var breakfastCombo = new Recipe();
            breakfastCombo.InitializeRecipe("早餐拼盤", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 1 },
                { FoodType.Egg, 2 },
                { FoodType.Cheese, 1 },
                { FoodType.Tomato, 1 }
            }, 18, 180);
            recipes.Add(breakfastCombo);

            // 終極漢堡
            var ultimateBurger = new Recipe();
            ultimateBurger.InitializeRecipe("終極漢堡", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 3 },
                { FoodType.Steak, 2 },
                { FoodType.Cheese, 1 },
                { FoodType.Lettuce, 1 },
                { FoodType.Tomato, 1 },
                { FoodType.Egg, 1 }
            }, 25, 250);
            recipes.Add(ultimateBurger);

            // === 特色料理 ===

            // 雙層起司堡
            var doubleCheeseburger = new Recipe();
            doubleCheeseburger.InitializeRecipe("雙層起司堡", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 },
                { FoodType.Steak, 1 },
                { FoodType.Cheese, 2 }
            }, 16, 160);
            recipes.Add(doubleCheeseburger);

            // 健康沙拉堡
            var healthyBurger = new Recipe();
            healthyBurger.InitializeRecipe("健康沙拉堡", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 2 },
                { FoodType.Lettuce, 2 },
                { FoodType.Tomato, 2 }
            }, 14, 140);
            recipes.Add(healthyBurger);

            Debug.Log($"已添加 {recipes.Count} 個預設配方!");
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
}