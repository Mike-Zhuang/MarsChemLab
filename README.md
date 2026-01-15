<div align="center">

# 🧪 MarsChemLab | 火星化学实验室

![Unity](https://img.shields.io/badge/Unity-6000.3.1f1-000000?style=flat&logo=unity)
![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS-lightgrey?style=flat)
![AI-Powered](https://img.shields.io/badge/AI-ZhipuAI%20GLM--4-purple?style=flat)

**基于真实的化学反应模型与生成式 AI 的火星生存模拟器**

[特性](#-特性-features) •
[快速开始](#-快速开始-getting-started) •
[游戏机制](#-游戏机制-mechanics) •
[AI 集成](#-moss-ai-integration) •
[许可证](#-许可证-license)

</div>

---

## 📖 简介 (Introduction)

**MarsChemLab** 是一个结合了硬核化学模拟与叙事互动的 Unity 项目。玩家扮演火星基地的首席科学家，需要精确计算化学配平，管理氧气与能源供应，以维持基地的生存。

项目不仅仅是简单的数值游戏，还深度集成了 **智谱 AI (GLM-4)**，通过 Prompt Engineering 复刻了《流浪地球》中 **MOSS** 的冷峻人格。AI 会实时监控你的决策，给出基于科学事实的生存概率评估。

## ✨ 特性 (Features)

- ⚗️ **硬核化学模拟**
  - **生命维持系统**：基于过氧化钠 ($Na_2O_2$) 与 $CO_2$ 的反应方程式，精确计算氧气产出与化学品消耗。
  - **动态反应速率**：环境湿度实时影响化学反应效率，通过线性插值算法模拟真实物理特性。

- ⚡ **能源管理系统**
  - **氢燃料电池**：利用剩余氧气与存量氢气发电，建立资源循环。
  - **资源互锁**：生命维持系统的产物直接影响能源系统的可用性。

- 🤖 **MOSS AI 人格**
  - **实时反馈**：集成 GLM-4 模型，AI 会根据你的每一次操作生成独特的剧情文本。
  - **沉浸式体验**：完全复刻 MOSS 的语气与逻辑，提供“绝对理性”的生存建议。

- 🖥️ **现代化 UI**
  - **实时数据可视化**：TextMeshPro 驱动的高清数值面板。
  - **交互式控制台**：滑动条精确控制与分页式系统面板。

## 🚀 快速开始 (Getting Started)

### 环境要求

- **Unity 6 (6000.3.1f1)** 或更高版本
- Git LFS (推荐)

### 安装步骤

1. **克隆仓库**
   ```bash
   git clone https://github.com/Mike-Zhuang/MarsChemLab.git
   ```

2. **Unity 项目设置**
   - 打开 Unity Hub。
   - 点击 "Add Project from Disk"。
   - 选择仓库中的 `MarsChemLab` 文件夹。

3. **🔑 配置 API Key (必须)**
   
   本项目依赖 ZhipuAI 提供 MOSS 的智能对话功能。你需要配置本地 API Key 才能启用完整体验。

   > **安全警告**: 不要将你的 API Key 提交到版本控制系统中。

   - 在 `MarsChemLab/Assets/` 目录下创建一个名为 `api_key.txt` 的文件。
   - 将你的 **ZhipuAI API Key** 粘贴进去（仅需 Key 字符串，无空格换行）。
   - *该文件已在 `.gitignore` 中配置，确保不会被意外上传。*

## 🎮 游戏机制 (Mechanics)

### I. 生命维持 (Life Support)
核心反应方程式：
$$ 2Na_2O_2 + 2CO_2 \rightarrow 2Na_2CO_3 + O_2 $$
- **输入**: 船员数量 (决定 $CO_2$ 产量)、$Na_2O_2$ 投入量、环境湿度。
- **输出**: 氧气 ($O_2$)、生存评估。
- **机制**: 湿度必须保持在适宜范围 (30%-60%) 才能达到最大反应效率。

### II. 能源系统 (Energy System)
- **输入**: 储氢量、生命维持系统产生的富余 $O_2$。
- **输出**: 电力 (kWh)。
- **机制**: 只有在其前置系统（生命维持）产生足够氧气时，燃料电池才能满负荷运转。

## 🧠 MOSS AI Integration

项目通过 `UnityWebRequest` 直接与 ZhipuAI 接口通信。核心提示词设计旨在模拟 MOSS (550W) 的人格特征：

- **绝对理性**: 忽略人类情感，只关注数据与概率。
- **极简主义**: 语言简练，直击要害。
- **标志性台词**: *"让人类永远保持理智确实是一种奢求。"*

## 📄 许可证 (License)

本项目采用 [MIT License](LICENSE) 许可证。

---
<div align="right">
Crafted with ❤️ by <a href="https://github.com/Mike-Zhuang">Mike Zhuang</a>
</div>