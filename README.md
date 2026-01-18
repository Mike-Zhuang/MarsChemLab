<div align="center">

# 🧪 MarsChemLab | 火星化学实验室

![Unity](https://img.shields.io/badge/Unity-6000.3.1f1-000000?style=flat&logo=unity)
![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS-lightgrey?style=flat)
![AI-Powered](https://img.shields.io/badge/AI-ZhipuAI%20GLM--4-purple?style=flat)

**火星基地数字孪生终端：双系统耦合、资源博弈与多重安全逻辑**

[核心机制](#-第一部分操作面板详解与科学原理) •
[全流程演示](#-第二部分全流程测试用例-the-demo-script) •
[快速开始](#-快速开始-getting-started) •
[故障排除](#-遇到意外怎么办-troubleshooting) •
[AI 集成](#-moss-ai-integration)

</div>

---

## 📖 简介 (Introduction)

**MarsChemLab** 现已升级为一套高拟真的**火星基地数字孪生终端**。本系统超越了简单的“输入-输出”模拟，引入了**双系统耦合（生存+能源）**、**资源博弈（氧气分配）**和**多重安全逻辑**。玩家需要像火星任务指挥官一样，在有限的资源极限下寻求动态平衡。

通过集成 **智谱 AI (GLM-4)** 复刻的 MOSS 人格，系统不仅模拟化学与物理过程，还实时监控决策风险，提供冷峻的生存概率评估。

---

## 🔬 第一部分：操作面板详解与科学原理

### 🅰️ 生存系统页 (Page_LifeSupport)

*核心任务：在有限的条件下，平衡“呼吸”与“水循环”的需求。*

#### 1. Crew Size (宇航员人数)
* **作用：** 设置游戏的“难度系数”。
* **原理：** 决定了 **$CO_2$ 产生量**（作为污染源）和 **最低呼吸氧气需求**。
* **代码逻辑：** `Total_CO2 = Crew * 40g`。

#### 2. Na2O2 Input (过氧化钠投入量)
* **作用：** 核心化学反应物。
* **原理：** **限制性反应物 (Limiting Reagent)**。投入量必须根据 $CO_2$ 量计算。
    * 太少 $\rightarrow$ $CO_2$ 吸收不完 $\rightarrow$ 中毒。
    * 太多 $\rightarrow$ 造成浪费 $\rightarrow$ **粉尘危害 (Alkali Dust Hazard)**。
* **方程式：** $$ 2Na_2O_2 + 2CO_2 \xrightarrow{H_2O} 2Na_2CO_3 + O_2 $$

#### 3. O2 Aeration Ratio (氧气曝气/分流比) —— ✨ 新核心功能
* **作用：** **资源分配策略**。决定把多少氧气分给污水处理系统。
* **原理：** 污水处理需要好氧细菌或直接氧化，必须通入氧气（曝气）。
    * 分流 0% $\rightarrow$ 污水没法处理 $\rightarrow$ **水污染报警**。
    * 分流 100% $\rightarrow$ 宇航员没气吸 $\rightarrow$ **窒息报警**。
* **教学点：** 演示资源稀缺时的博弈论。

#### 4. Catalyst Humidity (催化剂湿度)
* **作用：** 控制 **反应速率 (Kinetics)**。
* **原理：** 固气反应需要水蒸气作为介质/催化剂。
    * 湿度 < 20% $\rightarrow$ 反应几乎停止。
    * 湿度 > 60% $\rightarrow$ 反应过快，放热失控 $\rightarrow$ **爆炸**。
* **代码逻辑：** 使用 `Mathf.Lerp` 线性插值计算反应效率 `Rate_K`。

#### 5. Reactor Temp (反应堆温度)
* **作用：** 系统的安全指标。
* **原理：** 该反应是放热反应。温度 = 基础室温 + (产氧量 × 反应速率 × 放热系数)。

---

### 🅱️ 能源系统页 (Page_FuelCell)

*核心任务：利用化学能发电，并合成增量水源。*

#### 1. H2 Fuel Flow (氢气流速)
* **作用：** 决定发电功率的上限。
* **原理：** 燃料电池反应 $$ 2H_2 + O_2 \rightarrow 2H_2O $$

#### 2. Available Oxygen (可用氧气)
* **作用：** 显示从上一关（生存页）带过来的“氧气存折”。
* **原理：** **木桶效应**。如果氧气很少，哪怕氢气拉满，也发不了多少电。

#### 3. Water Stats (水资源面板) —— ✨ 新核心功能
* **Recycled Water (循环水)：**
    * **来源：** 生存页。
    * **算法：** `Crew * 500mL * 净化效率`。
    * **注意：** 如果你在生存页不仅氧气够，而且没报水污染，这里的回收率就是 95%，否则很低。
* **Synthesized Water (合成水)：**
    * **来源：** 燃料电池。
    * **算法：** `消耗的氧气 * 1.125` (根据摩尔质量 32:36 计算)。

---

## 🎬 第二部分：全流程测试用例 (The Demo Script)

请严格按照这 **6 个步骤** 进行演示，能覆盖所有代码分支，逻辑严密，效果炸裂。

#### 🧪 测试 1：功能验证 —— 反应条件（太干）
*目的是展示：有药也没用，必须有环境条件。*
* **Page:** LIFE SUPPORT
* **设置：**
    * Crew: **4**
    * Na2O2: **600g** (正常)
    * Aeration: **40%** (正常)
    * Humidity: **0%** (拉到最左)
* **点击 INITIATE**
* **结果：**
    * Temp: **~25°C** (没反应)
    * Status: <span style="color:orange">FAILURE: ATMOSPHERE TOO DRY</span>
* **话术：** “大家看，虽然原料充足，但没有水汽作为催化媒介，反应无法启动。”

#### 🔥 测试 2：功能验证 —— 热失控（爆炸）
*目的是展示：视觉冲击和安全性教育。*
* **Page:** LIFE SUPPORT
* **设置：**
    * Crew: **10** (拉满)
    * Na2O2: **1000g** (加量)
    * Aeration: **40%**
    * Humidity: **100%** (拉满，全速反应)
* **点击 INITIATE**
* **结果：**
    * Temp: **> 200°C** (甚至更高)
    * Status: <span style="color:red">CRITICAL: THERMAL RUNAWAY</span>
    * MOSS: 尖叫报警。
* **话术：** “在高负荷下，如果我们不控制反应速率，瞬间放热会导致反应堆熔毁。”

#### ⚠️ 测试 3：功能验证 —— 水污染（分流不足）
*目的是展示：好氧曝气对水处理的重要性。*
* **Page:** LIFE SUPPORT
* **设置：**
    * Crew: **4**
    * Na2O2: **600g**
    * Humidity: **50%** (调回正常)
    * Aeration: **0%** (一点氧气都不给水处理)
* **点击 INITIATE**
* **结果：**
    * Status: <span style="color:orange">WARNING: WATER CONTAMINATION HIGH</span>
    * MOSS: 警告水循环系统污染。
* **话术：** “如果我们只顾呼吸，不分流氧气去处理污水（Aeration 0%），虽然没窒息，但基地的水循环系统因为缺氧曝气而崩溃了。”

#### ☠️ 测试 4：功能验证 —— 窒息（分流过多）
*目的是展示：资源分配的博弈。*
* **Page:** LIFE SUPPORT
* **设置：**
    * Aeration: **100%** (全拿去洗澡了)
    * 其他不变
* **点击 INITIATE**
* **结果：**
    * Status: <span style="color:red">DANGER: ASPHYXIATION RISK</span>
* **话术：** “反之，如果氧气全被占用于工业用途，宇航员将面临窒息风险。我们需要找到平衡点。”

#### 🟢 测试 5：完美通关 —— 建立稳态
*目的是展示：科学计算后的最佳状态，为下一关做准备。*
* **Page:** LIFE SUPPORT
* **设置：**
    * Aeration: **30% - 40%** (黄金区间)
    * 其他不变 (Crew 4, Na2O2 600, Humid 50)
* **点击 INITIATE**
* **结果：**
    * Status: <span style="color:green">STABLE</span>
    * MOSS: **"Survival Probability: 99%"**
* **注意：** 此时你产生了几十克氧气，这也是下一关的门票。

#### 🔋 测试 6：终极融合 —— 发电与增量产水
*目的是展示：跨学科融合和双轨制水保障。*
* **操作：** 保持测试 5 的 STABLE 状态，点击顶部 **FUEL CELL** 按钮。
* **观察 1 (进入页面瞬间)：**
    * `RECYCLED WATER`: **1900 mL** (这是刚才测试 5 净化成功的存量)
    * `SYNTHESIZED WATER`: **0 mL** (还没发电)
    * *话术：“看，这是我们刚才通过化学手段净化的循环水。”*
* **操作 2：**
    * H2 Flow: 拉到 **50**
    * 点击 **GENERATE POWER**
* **观察 2 (结果)：**
    * `SYNTHESIZED WATER`: 变成了 **~50-100 mL** (取决于你的氧气量)
    * Status: <span style="color:green">GENERATING POWER: xxx kW</span>
    * *话术：“启动燃料电池后，我们不仅获得了电力，还通过电化学反应额外合成了纯净水，补充了循环损耗。”*

---

## 🚀 快速开始 (Getting Started)

### 环境要求
- **Unity 6 (6000.3.1f1)** 或更高版本
- Git LFS (推荐)

### 安装配置
1. **克隆仓库**: `git clone https://github.com/Mike-Zhuang/MarsChemLab.git`
2. **导入项目**: 使用 Unity Hub 打开目录。
3. **🔑 配置 API Key (必须)**:
   - 本项目依赖 ZhipuAI 提供 MOSS 的智能对话。
   - 在 `MarsChemLab/Assets/` 下创建 `api_key.txt`。
   - 粘贴你的 Key (不含空格换行)。

### 🏗️ 构建指南 (Build)

如果你想生成独立的 executable 运行：
1. 在 Unity 中打开 **File > Build Settings**。
2. 将 **Scenes/SampleScene** 添加到 "Scenes In Build"。
3. 选择目标平台 (Windows/macOS)。
4. 点击 **Build** 并选择输出目录（推荐 `Mars_Windows_Build/`）。

> **注意**：本项目已包含最新的 Windows 构建版本，可直接在 `Mars_Windows_Build/` 目录下运行体验。

---

## 💡 遇到“意外”怎么办？(Troubleshooting)

1. **为什么一直是红色报警？**
   - 检查是不是人数 (Crew) 太多了？人数越多，对氧气需求越大，如果不加大药量，怎么调都是错。建议演示时用 **Crew = 4** 最稳。

2. **为什么 Recycled Water 是 0？**
   - 说明你在生存页最后一次点击 `INITIATE` 时，状态是 **WATER CONTAMINATION** (水污染)。你必须在生存页看到 **STABLE** 后，再切过来，水才会被净化成功。

---

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