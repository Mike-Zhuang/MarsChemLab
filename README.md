# MarsChemLab 🧪🔴

**MarsChemLab** 是一个基于 Unity 开发的火星生存化学模拟项目。玩家需要计算和管理化学反应以产生足够的氧气，维持火星基地的生存。

## 🚀 项目特性 (Features)

- **氧气生成模拟**: 基于真实的化学常量，模拟过氧化钠 ($Na_2O_2$) 反应生成氧气的过程。
- **AI 智能交互 (MOSS)**: 集成了智谱 AI (GLM-4) 模型，模拟《流浪地球》中的 **MOSS** 人格。AI 会根据你的计算结果，用 MOSS 标志性的语气评估你的存活概率。
- **动态 UI**: 实时调整反应物质量，即时反馈生存时间。

## 🛠️ 安装与配置 (Installation)

1. **克隆仓库**:
   ```bash
   git clone https://github.com/Mike-Zhuang/MarsChemLab.git
   ```

2. **Unity 项目设置**:
   - 使用 Unity Hub 打开里面的 `MarsChemLab` 文件夹作为项目。

3. **🔑 配置 API Key (重要)**:
   本项目需要 ZhipuAI 的 API Key 才能启用 MOSS AI 功能。为了安全起见，Key 必须手动配置且不会被上传。
   
   - 在项目的 `MarsChemLab/Assets/` 目录下创建一个名为 `api_key.txt` 的新文件。
   - 将你的 ZhipuAI API Key 粘贴到该文件中（仅保留 Key 字符串，不要有多余空格或换行）。
   - *注意：该文件已被 `.gitignore` 忽略，确保你的 Key 不会泄露。*

## 🎮 运行指南

1. 运行 Unity 场景。
2. 拖动滑块输入过氧化钠 ($Na_2O_2$) 的质量。
3. 点击 **Submit** 按钮。
4. 查看本地计算结果，等待 MOSS 的最终裁决。

## 📄 许可证 (License)

本项目采用 MIT 许可证 - 详情请参阅 [LICENSE](LICENSE) 文件。
