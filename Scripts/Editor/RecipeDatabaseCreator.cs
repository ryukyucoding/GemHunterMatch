using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Match3
{
    /// <summary>
    /// RecipeDatabase 创建工具 - 用于在编辑器中一键创建并初始化 RecipeDatabase
    /// </summary>
    public class RecipeDatabaseCreator : Editor
    {
        [MenuItem("Match3/创建并初始化 RecipeDatabase")]
        public static void CreateRecipeDatabase()
        {
            // 检查是否已存在
            string path = "Assets/Resources/RecipeDatabase.asset";
            RecipeDatabase existingDatabase = AssetDatabase.LoadAssetAtPath<RecipeDatabase>(path);
            
            if (existingDatabase != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "RecipeDatabase 已存在",
                    "在 Resources 文件夹中已经存在 RecipeDatabase.asset，是否要覆盖它？",
                    "覆盖",
                    "取消"
                );
                
                if (!overwrite)
                {
                    Debug.Log("取消创建 RecipeDatabase");
                    return;
                }
                
                // 删除旧的
                AssetDatabase.DeleteAsset(path);
            }
            
            // 创建新的 RecipeDatabase
            RecipeDatabase database = ScriptableObject.CreateInstance<RecipeDatabase>();
            
            // 确保 Resources 文件夹存在
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            // 添加预设配方
            AddDefaultRecipes(database);
            
            // 保存到 Assets
            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 选中新创建的 asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = database;
            
            Debug.Log($"✅ 成功创建 RecipeDatabase！路径：{path}，配方数量：{database.recipes.Count}");
            EditorUtility.DisplayDialog(
                "创建成功",
                $"RecipeDatabase 已成功创建！\n路径：{path}\n配方数量：{database.recipes.Count}",
                "确定"
            );
        }
        
        private static void AddDefaultRecipes(RecipeDatabase database)
        {
            database.recipes = new List<Recipe>();
            
            // Recipe 1: Classic Hamburger
            var recipe1 = new Recipe();
            recipe1.InitializeRecipe("Classic Hamburger", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Lettuce, 3 },
                { FoodType.Steak, 3 }
            }, 12, 0);
            database.recipes.Add(recipe1);
            
            // Recipe 2: Cheese Hamburger
            var recipe2 = new Recipe();
            recipe2.InitializeRecipe("Cheese Hamburger", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Cheese, 3 },
                { FoodType.Egg, 6 }
            }, 15, 0);
            database.recipes.Add(recipe2);
            
            // Recipe 3: Veggie Sandwich
            var recipe3 = new Recipe();
            recipe3.InitializeRecipe("Veggie Sandwich", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Lettuce, 6 },
                { FoodType.Tomato, 6 }
            }, 20, 0);
            database.recipes.Add(recipe3);
            
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
            database.recipes.Add(recipe4);
            
            // Recipe 5: Steak Platter
            var recipe5 = new Recipe();
            recipe5.InitializeRecipe("Steak Platter", null, new Dictionary<FoodType, int>
            {
                { FoodType.Steak, 9 },
                { FoodType.Lettuce, 3 }
            }, 20, 0);
            database.recipes.Add(recipe5);
            
            // Recipe 6: Cheese Paradise
            var recipe6 = new Recipe();
            recipe6.InitializeRecipe("Cheese Paradise", null, new Dictionary<FoodType, int>
            {
                { FoodType.Egg, 12 },
                { FoodType.Bread, 3 },
                { FoodType.Tomato, 3 }
            }, 25, 0);
            database.recipes.Add(recipe6);
            
            // Recipe 7: Garden Fresh
            var recipe7 = new Recipe();
            recipe7.InitializeRecipe("Garden Fresh", null, new Dictionary<FoodType, int>
            {
                { FoodType.Tomato, 9 },
                { FoodType.Lettuce, 9 }
            }, 30, 0);
            database.recipes.Add(recipe7);
            
            // Recipe 8: Cheese & Egg Combo
            var recipe8 = new Recipe();
            recipe8.InitializeRecipe("Cheese & Egg Combo", null, new Dictionary<FoodType, int>
            {
                { FoodType.Egg, 9 },
                { FoodType.Cheese, 9 }
            }, 30, 0);
            database.recipes.Add(recipe8);
            
            // Recipe 9: Power Breakfast
            var recipe9 = new Recipe();
            recipe9.InitializeRecipe("Power Breakfast", null, new Dictionary<FoodType, int>
            {
                { FoodType.Cheese, 9 },
                { FoodType.Steak, 3 },
                { FoodType.Bread, 3 }
            }, 23, 0);
            database.recipes.Add(recipe9);
            
            // Recipe 10: Protein Pack
            var recipe10 = new Recipe();
            recipe10.InitializeRecipe("Protein Pack", null, new Dictionary<FoodType, int>
            {
                { FoodType.Cheese, 6 },
                { FoodType.Steak, 6 }
            }, 20, 0);
            database.recipes.Add(recipe10);
            
            // Recipe 11: Quick Snack
            var recipe11 = new Recipe();
            recipe11.InitializeRecipe("Quick Snack", null, new Dictionary<FoodType, int>
            {
                { FoodType.Bread, 6 },
                { FoodType.Tomato, 3 }
            }, 12, 0);
            database.recipes.Add(recipe11);
            
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
            database.recipes.Add(recipe12);
            
            Debug.Log($"已添加 {database.recipes.Count} 个预设配方到 RecipeDatabase");
        }
    }
}

