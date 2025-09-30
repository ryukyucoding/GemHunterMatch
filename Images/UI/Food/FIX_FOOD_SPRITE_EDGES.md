# 🍞 修复食材图片边缘显示问题

## 🔍 问题描述
食材图标的左上角或右上角会出现一点点吐司（面包）的残留像素。

## 🎯 原因分析

### 1. Sprite 模式问题
你的食材图片使用的是 **Multiple Sprite 模式**，每张图片包含子图：
- `bread.png` → `bread_0`
- `steak.png` → `steak_0`
- 等等...

### 2. 可能的原因
- ✗ 在 Inspector 中选择了错误的子 sprite
- ✗ Sprite 的裁剪区域（Rect）包含了透明边缘
- ✗ 图片文件本身有残留像素
- ✗ 压缩设置导致的边缘混色

## ✅ 解决方法

### 方法 1：在 Unity Inspector 中重新选择 Sprite（推荐）

1. **找到 CookingSystemSetup**：
   - 在 Hierarchy 窗口中找到包含 `CookingSystemSetup` 脚本的对象
   - 或在 Scene 中找到 cooking system 相关的 GameObject

2. **检查每个食材 Sprite 字段**：
   ```
   CookingSystemSetup Inspector:
   ├─ Bread Sprite
   ├─ Cheese Sprite
   ├─ Egg Sprite
   ├─ Lettuce Sprite
   ├─ Steak Sprite
   └─ Tomato Sprite
   ```

3. **确认选择正确的子 Sprite**：
   - 点击每个 Sprite 字段旁边的圆点图标
   - 在弹出的窗口中，确保选择的是正确的子图（如 `bread_0`）
   - **重要**：如果看到多个选项，选择**没有边缘残留**的那个

4. **如果仍有问题，尝试使用原始图片**：
   - 将 Sprite 字段留空
   - 让系统自动加载（如果配置了 Resources 路径）

### 方法 2：修改 Sprite 的裁剪区域

1. **选择食材图片**：
   - 在 Project 窗口中选择 `Images/UI/Food/bread.png`

2. **打开 Sprite Editor**：
   - 在 Inspector 中点击 **Sprite Editor** 按钮

3. **调整裁剪区域**：
   - 查看蓝色边框（Sprite 的边界）
   - 确保边框**紧贴**实际的图像内容，不包含透明边缘
   - 拖动边框角落来调整大小

4. **应用更改**：
   - 点击 Sprite Editor 顶部的 **Apply** 按钮
   - 对所有有问题的食材图片重复此操作

### 方法 3：重新导入图片（如果图片文件有问题）

1. **在 Project 窗口选择食材图片**

2. **修改导入设置**：
   ```
   Inspector > Texture Import Settings:
   
   Sprite Mode: Single (改为 Single 模式)
   Pixels Per Unit: 100
   Mesh Type: Tight
   Extrude Edges: 0
   Pivot: Center
   
   Advanced:
   ├─ Read/Write Enabled: ✓ (勾选)
   ├─ Generate Mip Maps: ✗ (不勾选)
   └─ Alpha Is Transparency: ✓ (勾选)
   
   Compression:
   ├─ Format: RGBA 32 bit (无压缩)
   └─ Compression Quality: None
   ```

3. **点击 Apply** 重新导入

4. **对所有食材图片重复此操作**

### 方法 4：使用图片编辑软件清理边缘

如果问题仍然存在，可能需要在外部图片编辑软件中处理：

1. **在 Photoshop/GIMP 中打开图片**

2. **检查透明边缘**：
   - 使用 Magic Wand 工具选择透明区域
   - 检查是否有残留的半透明像素

3. **清理边缘**：
   - 使用 Eraser 工具清除边缘的半透明像素
   - 或使用 Layer > Matting > Remove White Matte（Photoshop）

4. **重新导出**：
   - 保存为 PNG 格式
   - 确保背景完全透明
   - 替换 Unity 中的原始文件

## 🔧 已应用的代码修复

我已经修改了 `CookingUIManager.cs`：

**之前**：
```csharp
icon.style.backgroundImage = new StyleBackground(foodSprites[foodType]);
icon.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Contain));
```

**修改后**：
```csharp
icon.style.backgroundImage = new StyleBackground(foodSprites[foodType]);
icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;  // 更好的缩放模式
icon.style.unityBackgroundImageTintColor = Color.white;      // 确保颜色正确
```

## 📋 快速检查清单

- [ ] 检查 CookingSystemSetup Inspector 中的 Sprite 字段是否选择正确
- [ ] 确认每个 Sprite 字段指向正确的子图（如 `bread_0`）
- [ ] 在 Sprite Editor 中检查裁剪区域是否紧贴图像
- [ ] 确认图片导入设置中 `Alpha Is Transparency` 已勾选
- [ ] 如果使用压缩，尝试改为无压缩（RGBA 32 bit）
- [ ] 检查原始图片文件是否有边缘像素问题

## ⚙️ 推荐的最终配置

### Texture Import Settings（每个食材图片）：
```yaml
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 100
Mesh Type: Tight
Extrude Edges: 0
Pivot: Center
Max Size: 2048
Compression: None (或 RGBA 32 bit)
Alpha Is Transparency: ✓
Read/Write Enabled: ✓（如果需要）
```

## 🎯 最快的解决方案

1. 选择所有食材图片（`Images/UI/Food/` 中的所有 PNG）
2. 在 Inspector 中修改：
   - Sprite Mode: **Single**（而不是 Multiple）
   - Compression: **None**
3. 点击 **Apply**
4. 在 CookingSystemSetup 中重新分配 Sprite 字段

这样可以避免子 sprite 的选择问题，直接使用整张图片。
