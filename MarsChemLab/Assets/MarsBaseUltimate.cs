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
    public Slider sliderHumid;
    public TextMeshProUGUI textHumidVal;
    public TextMeshProUGUI textTemp;
    public Button btnRunReaction;

    [Header("--- 能源系统 UI ---")]
    public Slider sliderH2;
    public TextMeshProUGUI textH2Val;
    public TextMeshProUGUI textO2Storage;
    public Button btnGenPower;

    [Header("--- 公共显示 ---")]
    public TextMeshProUGUI aiResponseText;

    [Header("--- 配置 ---")]
    public string zhipuApiKey = "";

    // 全局变量
    private float storedOxygen = 0f;

    // 常量
    private const float M_Na2O2 = 78f;
    private const float M_O2 = 32f;
    private const float CO2_Per_Crew = 40f;

    void Start()
    {
        LoadApiKey();

        // 修正点：C# 使用 => 而不是 ->
        // 1. 页面切换逻辑
        if (btnGotoLife != null) btnGotoLife.onClick.AddListener(() => SwitchPage(true));
        if (btnGotoEnergy != null) btnGotoEnergy.onClick.AddListener(() => SwitchPage(false));

        // 2. 生存系统监听
        if (sliderCrew != null) sliderCrew.onValueChanged.AddListener((v) => textCrewVal.text = $"{v:F0}");
        if (sliderNa2O2 != null) sliderNa2O2.onValueChanged.AddListener((v) => textNa2O2Val.text = $"{v:F0} g");
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

        if (!toLifeSupport && textO2Storage != null)
        {
            textO2Storage.text = $"AVAILABLE OXYGEN: {storedOxygen:F1} g";
        }
    }

    void RunLifeSupportSim()
    {
        float crewCount = sliderCrew.value;
        float na2o2Input = sliderNa2O2.value;
        float humidity = sliderHumid.value;

        float totalCO2Prod = crewCount * CO2_Per_Crew;
        float maxNa2O2Consumed = totalCO2Prod * (156f / 88f);
        float actualReactedNa2O2 = Mathf.Min(na2o2Input, maxNa2O2Consumed);
        float wasteNa2O2 = na2o2Input - actualReactedNa2O2;
        float remainingCO2 = totalCO2Prod - (actualReactedNa2O2 * (88f / 156f));

        // --- 修复版：平滑的速率曲线 ---
        float rateK = 0f;

        if (humidity <= 20)
        {
            // 0% - 20%: 速率从 0 缓慢升到 0.2 (不再是死板的 0.1)
            rateK = Mathf.Lerp(0f, 0.2f, humidity / 20f);
        }
        else if (humidity <= 60)
        {
            // 20% - 60%: 速率从 0.2 线性升到 1.0 (最佳区间)
            // 这就是我们要的“中间档位”
            rateK = Mathf.Lerp(0.2f, 1.0f, (humidity - 20f) / 40f);
        }
        else
        {
            // 60% - 100%: 速率从 1.0 飙升到 1.5 (失控区间)
            rateK = Mathf.Lerp(1.0f, 1.5f, (humidity - 60f) / 40f);
        }

        float theoreticalO2 = (actualReactedNa2O2 / M_Na2O2) * 0.5f * M_O2;
        float actualO2 = theoreticalO2 * rateK;
        storedOxygen = actualO2;

        float temp = 25f + (actualO2 * 5f * rateK);

        string status = "STABLE";
        string logColor = "green";

        if (humidity < 20) { status = "FAILURE: ATMOSPHERE TOO DRY"; logColor = "yellow"; }
        else if (remainingCO2 > 50) { status = "WARNING: CO2 TOXICITY HIGH"; logColor = "orange"; }
        else if (wasteNa2O2 > 500) { status = "WARNING: ALKALI DUST HAZARD"; logColor = "orange"; }
        else if (temp > 200) { status = "CRITICAL: THERMAL RUNAWAY"; logColor = "red"; }

        textTemp.text = $"Reactor Temp: {temp:F0} °C";
        aiResponseText.text = $"<color={logColor}>[SYSTEM]: {status}</color>\n[MOSS]: Analyzing data...";

        StartCoroutine(CallAI_LifeSupport(crewCount, humidity, temp, status));
    }

    void RunFuelCellSim()
    {
        float h2Input = sliderH2.value;

        if (storedOxygen <= 10)
        {
            aiResponseText.text = "<color=red>[ERROR]: INSUFFICIENT OXYGEN RESERVES.</color>";
            return;
        }

        float powerOutput = Mathf.Min(h2Input, storedOxygen) * 15f;

        string status = $"GENERATING POWER: {powerOutput:F0} kW";
        aiResponseText.text = $"<color=green>[SYSTEM]: {status}</color>\n[MOSS]: Grid Syncing...";

        StartCoroutine(CallAI_Energy(h2Input, storedOxygen, powerOutput));
    }

    IEnumerator CallAI_LifeSupport(float crew, float humid, float temp, string sysStatus)
    {
        string prompt = $@"You are MOSS. Scene: Mars Life Support. Inputs: {crew} Astronauts, Humidity {humid}%, Temp {temp}C. System Status: {sysStatus}. Task: Roleplay a 1-sentence sci-fi reaction (max 20 words). Tone: Robotic.";
        yield return SendToZhipu(prompt);
    }

    IEnumerator CallAI_Energy(float h2, float o2, float power)
    {
        string prompt = $@"You are MOSS. Scene: Fuel Cell. Inputs: H2 {h2}, O2 {o2}. Output: {power} kW. Task: Roleplay a 1-sentence status report (max 20 words).";
        yield return SendToZhipu(prompt);
    }

    IEnumerator SendToZhipu(string contentPrompt)
    {
        // 1. 清洗 Prompt
        string safePrompt = contentPrompt.Replace("\n", " ").Replace("\"", "'");

        // 2. 构造 JSON
        string json = "{\"model\":\"glm-4\",\"temperature\":0.5,\"messages\":[{\"role\":\"user\",\"content\":\"" + safePrompt + "\"}]}";

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

            // --- 修复点：去掉了 <color> 标签，直接显示纯文本 ---
            // 加上 \n\n 换两行，让 MOSS 的话和上面的 SYSTEM 分开，更清晰
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