using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class MarsBaseUltimate : MonoBehaviour
{
    [Header("--- 页面控制 ---")]
    public GameObject pageLifeSupport;
    public GameObject pageEnergy;
    public Button btnGotoLife;
    public Button btnGotoEnergy;

    [Header("--- 生存系统 UI ---")]
    public Slider sliderCrew;
    public TextMeshProUGUI textCrewVal;

    public Slider sliderNa2O2;
    public TextMeshProUGUI textNa2O2Val;

    // 新增：氧气分流/曝气滑块
    public Slider sliderAeration;
    public TextMeshProUGUI textAerationVal;

    public Slider sliderHumid;
    public TextMeshProUGUI textHumidVal;

    public TextMeshProUGUI textTemp;
    public Button btnRunReaction;

    [Header("--- 能源系统 UI ---")]
    public Slider sliderH2;
    public TextMeshProUGUI textH2Val;
    public TextMeshProUGUI textO2Storage;
    // 新增：水资源统计显示
    public TextMeshProUGUI textWaterStats;
    public Button btnGenPower;

    [Header("--- 公共显示 ---")]
    public TextMeshProUGUI aiResponseText;

    [Header("--- 配置 ---")]
    public string zhipuApiKey = "";

    // --- 全局数据 ---
    private float storedOxygen = 0f; // 储存的氧气
    private float recycledWaterRate = 0f; // 循环水效率 (0-1)

    // --- 化学常量 ---
    private const float M_Na2O2 = 78f;
    private const float M_O2 = 32f;
    private const float CO2_Per_Crew = 40f;
    private const float Breathing_Per_Crew = 5f; // 假设每人每小时需要5g氧气呼吸

    void Start()
    {
        LoadApiKey();

        // 1. 页面切换
        if (btnGotoLife != null) btnGotoLife.onClick.AddListener(() => SwitchPage(true));
        if (btnGotoEnergy != null) btnGotoEnergy.onClick.AddListener(() => SwitchPage(false));

        // 2. 生存系统监听
        if (sliderCrew != null) sliderCrew.onValueChanged.AddListener((v) => textCrewVal.text = $"{v:F0}");
        if (sliderNa2O2 != null) sliderNa2O2.onValueChanged.AddListener((v) => textNa2O2Val.text = $"{v:F0} g");
        // 新增监听
        if (sliderAeration != null) sliderAeration.onValueChanged.AddListener((v) => textAerationVal.text = $"{v:F0} %");
        if (sliderHumid != null) sliderHumid.onValueChanged.AddListener((v) => textHumidVal.text = $"{v:F0} %");
        if (btnRunReaction != null) btnRunReaction.onClick.AddListener(RunLifeSupportSim);

        // 3. 能源系统监听
        if (sliderH2 != null) sliderH2.onValueChanged.AddListener((v) => textH2Val.text = $"{v:F0} L/min");
        if (btnGenPower != null) btnGenPower.onClick.AddListener(RunFuelCellSim);

        SwitchPage(true);
    }

    void SwitchPage(bool toLifeSupport)
    {
        if (pageLifeSupport != null) pageLifeSupport.SetActive(toLifeSupport);
        if (pageEnergy != null) pageEnergy.SetActive(!toLifeSupport);

        if (!toLifeSupport)
        {
            if (textO2Storage != null)
                textO2Storage.text = $"AVAILABLE OXYGEN: {storedOxygen:F1} g";

            // --- 修改点：切换过来时，立刻计算并显示“存量水” ---
            if (textWaterStats != null)
            {
                // 此时还没发电，所以 SYNTHESIZED 是 0
                // 但是 RECYCLED 是上一关带过来的，可以直接算
                float currentRecycled = sliderCrew.value * 500f * recycledWaterRate;

                textWaterStats.text = $"RECYCLED WATER: {currentRecycled:F0} mL (Rate: {recycledWaterRate * 100:F0}%)\n" +
                                      "SYNTHESIZED WATER: 0.0 mL (Waiting for Power...)";
            }
        }
    }

    // --- 核心逻辑 1: 生存系统 (含污水处理耦合) ---
    void RunLifeSupportSim()
    {
        float crewCount = sliderCrew.value;
        float na2o2Input = sliderNa2O2.value;
        float humidity = sliderHumid.value;
        float aerationPercent = sliderAeration.value / 100f; // 0.0 - 1.0

        // 1. 基础化学计算 (产氧)
        float totalCO2Prod = crewCount * CO2_Per_Crew;
        float maxNa2O2Consumed = totalCO2Prod * (156f / 88f);
        float actualReactedNa2O2 = Mathf.Min(na2o2Input, maxNa2O2Consumed);
        float wasteNa2O2 = na2o2Input - actualReactedNa2O2;

        // 速率计算 (湿度影响)
        float rateK = 0f;
        if (humidity <= 20) rateK = Mathf.Lerp(0f, 0.2f, humidity / 20f);
        else if (humidity <= 60) rateK = Mathf.Lerp(0.2f, 1.0f, (humidity - 20f) / 40f);
        else rateK = Mathf.Lerp(1.0f, 1.5f, (humidity - 60f) / 40f);

        float totalO2Produced = (actualReactedNa2O2 / M_Na2O2) * 0.5f * M_O2 * rateK;
        float temp = 25f + (totalO2Produced * 3f * rateK);

        // 2. 氧气分配逻辑 (关键耦合点!)
        float o2ForAeration = totalO2Produced * aerationPercent; // 分给污水处理的
        float o2ForBreathing = totalO2Produced - o2ForAeration;  // 留给呼吸的

        // 存入全局变量 (只有呼吸剩下的才能带到下一关发电，或者全部带走看你怎么定，这里假设全部存储)
        storedOxygen = totalO2Produced;

        // 3. 状态判定
        string status = "STABLE";
        string logColor = "green";
        float survivalProb = 98.5f; // 初始概率

        float requiredBreathingO2 = crewCount * Breathing_Per_Crew;
        // 简单的污水净化阈值：每人需要至少 10g 氧气来曝气才算干净
        float requiredAerationO2 = crewCount * 2f;

        // 判定逻辑链
        if (humidity < 10)
        {
            status = "FAILURE: ATMOSPHERE TOO DRY";
            logColor = "yellow";
            survivalProb = 0f;
        }
        else if (temp > 200)
        {
            status = "CRITICAL: THERMAL RUNAWAY";
            logColor = "red";
            survivalProb = 15f;
        }
        else if (wasteNa2O2 > 500)
        {
            status = "WARNING: ALKALI DUST HAZARD";
            logColor = "orange";
            survivalProb = 60f;
        }
        else if (o2ForBreathing < requiredBreathingO2)
        {
            // 氧气都拿去曝气了，人没气吸了
            status = "DANGER: ASPHYXIATION RISK (O2 LOW)";
            logColor = "red";
            survivalProb = 40f;
        }
        else if (o2ForAeration < requiredAerationO2)
        {
            // 氧气全吸了，没给污水处理
            status = "WARNING: WATER CONTAMINATION HIGH";
            logColor = "orange";
            recycledWaterRate = 0.2f; // 净化失败，回收率低
            survivalProb = 75f;
        }
        else
        {
            // 完美状态
            status = "STABLE";
            recycledWaterRate = 0.95f; // 净化成功，回收率高
        }

        textTemp.text = $"Reactor Temp: {temp:F0} °C";

        // 构建 AI 回复
        aiResponseText.text = $"<color={logColor}>[SYSTEM]: {status}</color>\n[MOSS]: Analyzing Life Support Data...";
        StartCoroutine(CallAI_LifeSupport(crewCount, humidity, temp, status, survivalProb));
    }

    // --- 核心逻辑 2: 能源系统 (含水合成) ---
    void RunFuelCellSim()
    {
        float h2Input = sliderH2.value;

        if (storedOxygen <= 5)
        {
            aiResponseText.text = "<color=red>[ERROR]: INSUFFICIENT OXYGEN RESERVES.</color>";
            textWaterStats.text = "WATER PRODUCTION HALTED";
            return;
        }

        // 1. 发电计算
        float o2Consumed = Mathf.Min(h2Input, storedOxygen);
        float powerOutput = o2Consumed * 15f;

        // 2. 产水计算 (2H2 + O2 -> 2H2O, 质量比 32:36 -> 1:1.125)
        float synthesizedWater = o2Consumed * 1.125f; // 化学合成水 (增量)

        // 3. 循环水计算 (基于人数和之前的净化效率)
        // 假设每人携带 500ml 水循环使用
        float recycledWaterTotal = sliderCrew.value * 500f * recycledWaterRate;

        // 4. 更新水资源面板 (富文本)
        textWaterStats.text = $"RECYCLED WATER: {recycledWaterTotal:F0} mL (Rate: {recycledWaterRate * 100:F0}%)\n" +
                              $"SYNTHESIZED WATER: {synthesizedWater:F1} mL";

        // 5. 状态判定
        string statusStr = "";
        string colorTag = "";

        if (powerOutput > 1000f)
        {
            statusStr = $"WARNING: VOLTAGE OVERLOAD ({powerOutput:F0} kW)";
            colorTag = "red";
        }
        else
        {
            statusStr = $"GENERATING POWER: {powerOutput:F0} kW";
            colorTag = "green";
        }

        aiResponseText.text = $"<color={colorTag}>[SYSTEM]: {statusStr}</color>\n[MOSS]: Syncing with Water Reclamation...";

        StartCoroutine(CallAI_Energy(h2Input, storedOxygen, powerOutput, recycledWaterTotal + synthesizedWater));
    }

    // --- AI 接口 (针对文档优化 Prompt) ---
    IEnumerator CallAI_LifeSupport(float crew, float humid, float temp, string sysStatus, float prob)
    {
        string prompt = $@"
You are MOSS. Scene: Mars Life Support. 
Status: {sysStatus}.
Survival Probability: {prob}%.

Task: Roleplay a status report (max 25 words).
- If STABLE: Must state 'Survival Probability: 99%'.
- If WATER CONTAMINATION: Warn about dirty water recycling.
- If ASPHYXIATION: Warn about low oxygen for breathing.
- Tone: Cold, Scientific.
";
        yield return SendToZhipu(prompt);
    }

    IEnumerator CallAI_Energy(float h2, float o2, float power, float totalWater)
    {
        string prompt = $@"
You are MOSS. Scene: Fuel Cell & Water System.
Power: {power} kW. 
Total Water Reserves: {totalWater} mL.

Task: Roleplay a status report (max 25 words).
- If Power > 1000: Warn VOLTAGE OVERLOAD.
- Else: Confirm Power Grid and Clean Water supply secured. Mention 'Water Cycle Optimized'.
";
        yield return SendToZhipu(prompt);
    }

    // --- 通用工具 ---
    IEnumerator SendToZhipu(string contentPrompt)
    {
        string safePrompt = contentPrompt.Replace("\n", " ").Replace("\"", "'");
        string json = "{\"model\":\"glm-4\",\"temperature\":0.6,\"messages\":[{\"role\":\"user\",\"content\":\"" + safePrompt + "\"}]}";

        UnityWebRequest request = new UnityWebRequest("https://open.bigmodel.cn/api/paas/v4/chat/completions", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + zhipuApiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string content = ExtractContent(request.downloadHandler.text);
            aiResponseText.text += $"\n\n[MOSS]: {content}";
        }
    }

    void LoadApiKey()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "api_key.txt");
        if (System.IO.File.Exists(path)) zhipuApiKey = System.IO.File.ReadAllText(path).Trim();
    }

    string ExtractContent(string json)
    {
        try
        {
            int contentIndex = json.IndexOf("\"content\":\"");
            if (contentIndex == -1) return "Offline.";
            int start = contentIndex + 11;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start).Replace("\\n", "\n").Replace("\\\"", "\"");
        }
        catch { return "Error."; }
    }
}