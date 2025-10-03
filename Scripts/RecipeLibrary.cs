using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 配方库 - 直接在代码中定义所有配方，不依赖 ScriptableObject
    /// </summary>
    public static class RecipeLibrary
    {
        private static List<Recipe> _recipes = null;

        /// <summary>
        /// 获取所有配方
        /// </summary>
        public static List<Recipe> GetAllRecipes()
        {
            if (_recipes == null)
            {
                InitializeRecipes();
            }
            return _recipes;
        }

        /// <summary>
        /// 获取随机配方
        /// </summary>
        public static Recipe GetRandomRecipe()
        {
            var recipes = GetAllRecipes();
            if (recipes.Count == 0) return null;
            return recipes[Random.Range(0, recipes.Count)];
        }

        /// <summary>
        /// 根据名称获取配方
        /// </summary>
        public static Recipe GetRecipeByName(string name)
        {
            var recipes = GetAllRecipes();
            return recipes.Find(r => r.dishName == name);
        }

        /// <summary>
        /// 初始化所有配方
        /// </summary>
        private static void InitializeRecipes()
        {
            _recipes = new List<Recipe>();

            // Recipe 1: Classic Hamburger
            var recipe1 = new Recipe();
            recipe1.InitializeRecipe("Classic Hamburger", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Lettuce, 3 },
                { FoodType.Steak, 3 }
            }, 12, 0);
            _recipes.Add(recipe1);

            // Recipe 2: Cheese Hamburger
            var recipe2 = new Recipe();
            recipe2.InitializeRecipe("Cheese Hamburger", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Cheese, 3 },
                { FoodType.Egg, 6 }
            }, 15, 0);
            _recipes.Add(recipe2);

            // Recipe 3: Veggie Sandwich
            var recipe3 = new Recipe();
            recipe3.InitializeRecipe("Veggie Sandwich", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Lettuce, 6 },
                { FoodType.Tomato, 6 }
            }, 20, 0);
            _recipes.Add(recipe3);

            // Recipe 4: Super Hamburger
            var recipe4 = new Recipe();
            recipe4.InitializeRecipe("Super Hamburger", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Steak, 6 },
                { FoodType.Tomato, 3 },
                { FoodType.Egg, 6 },
                { FoodType.Lettuce, 3 },
                { FoodType.Cheese, 3 }
            }, 30, 0);
            _recipes.Add(recipe4);

            // Recipe 5: Steak Platter
            var recipe5 = new Recipe();
            recipe5.InitializeRecipe("Steak Platter", null, new Dictionary<FoodType, int>
            {
                { FoodType.Steak, 9 },
                { FoodType.Lettuce, 3 }
            }, 20, 0);
            _recipes.Add(recipe5);

            // Recipe 6: Cheese Paradise
            var recipe6 = new Recipe();
            recipe6.InitializeRecipe("Cheese Paradise", null, new Dictionary<FoodType, int>
            {
                { FoodType.Egg, 12 },
                { FoodType.Bread, 3 },
                { FoodType.Tomato, 3 }
            }, 25, 0);
            _recipes.Add(recipe6);

            // Recipe 7: Garden Fresh
            var recipe7 = new Recipe();
            recipe7.InitializeRecipe("Garden Fresh", null, new Dictionary<FoodType, int>
            {
                { FoodType.Tomato, 9 },
                { FoodType.Lettuce, 9 }
            }, 30, 0);
            _recipes.Add(recipe7);

            // Recipe 8: Cheese & Egg Combo
            var recipe8 = new Recipe();
            recipe8.InitializeRecipe("Cheese & Egg Combo", null, new Dictionary<FoodType, int>
            {
                { FoodType.Egg, 9 },
                { FoodType.Cheese, 9 }
            }, 30, 0);
            _recipes.Add(recipe8);

            // Recipe 9: Power Breakfast
            var recipe9 = new Recipe();
            recipe9.InitializeRecipe("Power Breakfast", null, new Dictionary<FoodType, int>
            {
                { FoodType.Cheese, 9 },
                { FoodType.Steak, 3 },
                { FoodType.Bread, 3 }
            }, 23, 0);
            _recipes.Add(recipe9);

            // Recipe 10: Protein Pack
            var recipe10 = new Recipe();
            recipe10.InitializeRecipe("Protein Pack", null, new Dictionary<FoodType, int>
            {
                { FoodType.Cheese, 6 },
                { FoodType.Steak, 6 }
            }, 20, 0);
            _recipes.Add(recipe10);

            // Recipe 11: Quick Snack
            var recipe11 = new Recipe();
            recipe11.InitializeRecipe("Quick Snack", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Tomato, 3 }
            }, 12, 0);
            _recipes.Add(recipe11);

            // Recipe 12: Ultimate Feast
            var recipe12 = new Recipe();
            recipe12.InitializeRecipe("Ultimate Feast", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Lettuce, 3 },
                { FoodType.Tomato, 3 },
                { FoodType.Egg, 6 },
                { FoodType.Cheese, 6 },
                { FoodType.Steak, 6 }
            }, 30, 0);
            _recipes.Add(recipe12);

            Debug.Log($"[RecipeLibrary] 已初始化 {_recipes.Count} 个配方");
        }

        /// <summary>
        /// 重置配方库（测试用）
        /// </summary>
        public static void ResetLibrary()
        {
            _recipes = null;
        }
    }
}

