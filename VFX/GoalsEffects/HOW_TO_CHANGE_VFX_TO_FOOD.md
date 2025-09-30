# 🍕 如何将 Win/Lose VFX 从宝石改为食材

## 📍 问题
Win 和 Lose 特效目前显示的是宝石（使用 `Gem_Atlas.png`），需要改为显示食材。

## ✅ 解决方案

### 方法 1：在 Unity Editor 中直接修改（推荐）

#### 步骤 1：打开 VFX Graph
1. 在 Unity Project 窗口中找到 `VFX/GoalsEffects/VFX_Win.vfx`
2. 双击打开 VFX Graph 编辑器

#### 步骤 2：找到纹理设置
1. 在 VFX Graph 窗口中，寻找包含 "[Gems]" 或类似名称的系统
2. 找到 **Output Particle Quad** 节点
3. 选中该节点，在右侧 Inspector 查看属性

#### 步骤 3：替换纹理
在 Inspector 中找到纹理相关的字段：
- **Main Texture** 或 **Particle Texture**
- 当前应该指向 `Gem_Atlas`

替换选项：

**选项 A - 使用单一食材（最简单）**
```
选择任一食材图片：
- Images/UI/Food/bread.png
- Images/UI/Food/steak.png
- Images/UI/Food/cheese.png
- Images/UI/Food/egg.png
- Images/UI/Food/lettuce.png
- Images/UI/Food/tomato.png
```

**选项 B - 使用食材图集**
```
使用新创建的 Sprite Atlas：
- Settings/Sprite Atlases/SpriteAtlas_Food
```

**选项 C - 创建食材图集纹理（需要图片编辑）**
```
1. 使用图片编辑软件（如 Photoshop、GIMP）
2. 将所有食材图片合并到一张大图中（类似 Gem_Atlas.png 的布局）
3. 保存为 Food_Atlas.png
4. 在 VFX 中使用这个新图集
5. 配置 SubUV 参数以显示不同的食材
```

#### 步骤 4：调整 SubUV 设置（如使用图集）
如果使用包含多个食材的图集：
1. 在 Output Particle 节点中启用 **Use SubUV**
2. 设置 **SubUV Size**（例如：6x1 表示 6 种食材排成一行）
3. 启用 **SubUV Random** 以随机显示不同食材

#### 步骤 5：保存和编译
1. 点击 VFX Graph 编辑器顶部的 **Save** 按钮
2. 点击 **Compile** 按钮（或等待自动编译）

#### 步骤 6：重复 Lose Effect
对 `VFX/GoalsEffects/VFX_Loose.vfx` 执行相同操作

### 方法 2：创建食材图集纹理（手动方式）

如果你想要完全模仿宝石图集的效果：

1. **准备食材图片**：
   - bread.png
   - steak.png
   - cheese.png
   - egg.png
   - lettuce.png
   - tomato.png

2. **合并成图集**：
   - 使用 Photoshop、GIMP 或在线工具
   - 将 6 张图片排列在一张图上（例如 6x1 网格）
   - 确保每个食材占据相同大小的区域
   - 保存为 `Food_Atlas.png`

3. **导入 Unity**：
   ```
   将 Food_Atlas.png 放到：
   Assets/GemHunterMatch/Images/UI/Food/Food_Atlas.png
   ```

4. **在 VFX 中使用**：
   - 在 VFX Graph 中将纹理指向 Food_Atlas.png
   - 配置 SubUV: 6x1 (6 列 1 行)

## 🎨 推荐配置

### 最简单方式（只显示一种食材）
```
VFX Graph > Output Particle > Main Texture = bread.png
```

### 最真实方式（显示多种随机食材）
```
1. 创建 Food_Atlas.png（6 种食材排成一行）
2. VFX Graph 设置：
   - Main Texture = Food_Atlas.png
   - Use SubUV = ✓
   - SubUV Size = 6, 1
   - SubUV Random = ✓
```

## 📁 相关文件

- **VFX 文件**：
  - `VFX/GoalsEffects/VFX_Win.vfx`
  - `VFX/GoalsEffects/VFX_Loose.vfx`

- **食材图片**：
  - `Images/UI/Food/*.png`

- **现有宝石图集（参考）**：
  - `Images/UI/Gems/Gem_Atlas.png`

- **新创建的食材 Sprite Atlas**：
  - `Settings/Sprite Atlases/SpriteAtlas_Food.spriteatlasv2`

## ⚠️ 注意事项

1. **备份原始 VFX**：在修改前建议复制一份原始文件
2. **SubUV 坐标**：如果使用图集，确保 SubUV 设置正确匹配图集布局
3. **纹理大小**：食材图片可能需要调整到相似的大小以获得最佳效果
4. **性能**：使用 Sprite Atlas 比单独的纹理性能更好

## 🔧 故障排除

### 问题：VFX 不显示或显示黑色
- 检查纹理是否正确导入
- 确保纹理的 Texture Type 设置为 **Default** 或 **Sprite**
- 检查纹理的 Read/Write Enabled 是否启用

### 问题：显示的食材被拉伸或变形
- 检查 SubUV Size 设置是否匹配图集布局
- 确保所有食材图片大小一致

### 问题：只显示第一个食材
- 启用 **SubUV Random** 选项
- 检查 SubUV Size 设置是否正确
