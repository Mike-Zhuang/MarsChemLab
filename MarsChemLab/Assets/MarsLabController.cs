using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class MarsLabController : MonoBehaviour
{
    [Header("UI 绑定")]
    public TMP_InputField inputField;
    public Slider massSlider;
    public TextMeshProUGUI valueDisplay;
    public TextMeshProUGUI aiResponseText;
    public Button submitButton;
    public TextMeshProUGUI tempDisplay; // 温度显示

    [Header("配置")]
    public string zhipuApiKey = ""; // 如果本地加载失败，可以在这里手动填入备用

    // --- 硬核化学常量 ---
    private const float M_Na2O2 = 78f;
    private const float M_O2 = 32f;
    private const float ConsumptionRate = 50f;

    // 热力学数据
    private const float EnthalpyPerMoleO2 = 487f;
    private const float ReactorHeatCapacity = 100f;
    private const float BaseTemp = 25f;

    void Start()
    {
        LoadApiKey();
        if (massSlider != null)
        {
            massSlider.onValueChanged.AddListener(OnSliderChange);
            OnSliderChange(massSlider.value);
        }
        submitButton.onClick.AddListener(OnSubmit);
    }

    void LoadApiKey()
    {
        // 尝试读取本地 Key，如果没有读到，就会用 Inspector 里填的默认值
        string path = System.IO.Path.Combine(Application.dataPath, "api_key.txt");
        if (System.IO.File.Exists(path))
        {
            zhipuApiKey = System.IO.File.ReadAllText(path).Trim();
        }
    }

    void OnSliderChange(float val)
    {
        // 实时计算预览
        float molesNa2O2 = val / M_Na2O2;
        float molesO2 = molesNa2O2 * 0.5f;
        float heatGenerated = molesO2 * EnthalpyPerMoleO2;
        float finalTemp = BaseTemp + (heatGenerated / ReactorHeatCapacity);

        if (valueDisplay != null)
            valueDisplay.text = $"Na2O2 Input: {val:F0} g";

        if (tempDisplay != null)
        {
            tempDisplay.text = $"Reactor Temp: {finalTemp:F0} °C";
            if (finalTemp > 300) tempDisplay.color = Color.red;
            else if (finalTemp > 100) tempDisplay.color = Color.yellow;
            else tempDisplay.color = Color.cyan;
        }
    }

    public void OnSubmit()
    {
        float mass = massSlider.value;
        float molesNa2O2 = mass / M_Na2O2;
        float molesO2 = molesNa2O2 * 0.5f;
        float massO2 = molesO2 * M_O2;
        float survivalHours = massO2 / ConsumptionRate;
        float heatKJ = molesO2 * EnthalpyPerMoleO2;
        float finalTemp = BaseTemp + (heatKJ / ReactorHeatCapacity);

        // 先显示本地计算结果（让老师看到没有 AI 也能用）
        string status = "";
        bool isOverheat = finalTemp > 300f;

        if (isOverheat) status = "<color=red>[CRITICAL WARNING: THERMAL RUNAWAY]</color>";
        else status = "<color=green>[THERMAL STABLE]</color>";

        string localResult = $"[SYSTEM ANALYSIS]\n" +
                             $"O2 Yield: {massO2:F1} g\n" +
                             $"Survival: {survivalHours:F1} Hours\n" +
                             $"Reactor Temp: {finalTemp:F0} °C\n" +
                             $"{status}";

        aiResponseText.text = localResult + "\n\n<color=yellow>Connecting to MOSS AI...</color>";

        StartCoroutine(CallZhipuAI(massO2, survivalHours, finalTemp));
    }

    IEnumerator CallZhipuAI(float o2, float hours, float temp)
    {
        // 1. 定义 Prompt (这里用换行是为了代码好看)
        string rawPrompt = $@"
You are MOSS, the safety AI of the Mars Base. 
Current System Status: 
- Oxygen Yield: {o2:F1} g
- Survival Time: {hours:F1} hours
- Reactor Temp: {temp:F0} Celsius (Critical Limit: 300 C)

Your Task: Output a system log based strictly on the following logic. Do not add any other words.

Logic Chain:
1. IF Temp > 300: Output 'CRITICAL ALERT: REACTOR OVERHEAT. EVACUATE IMMEDIATELY.'
2. IF Temp <= 300 AND Hours < 5: Output 'WARNING: INSUFFICIENT OXYGEN. INCREASE DOSAGE.'
3. IF Temp <= 300 AND Hours >= 5: Output 'SYSTEM STABLE. LIFE SUPPORT ONLINE.'

Output Format: Just the sentence from the logic chain.
";

        // 2. ⭐关键修复⭐：把 Prompt 清洗成 JSON 能接受的单行格式
        string safePrompt = rawPrompt.Replace("\n", "\\n").Replace("\r", "").Replace("\"", "\\\"");

        // 3. 构造 JSON
        string json = "{" +
            "\"model\": \"glm-4\"," +
            "\"temperature\": 0.1," +
            "\"messages\": [" +
                "{\"role\": \"user\", \"content\": \"" + safePrompt + "\"}" +
            "]" +
        "}";

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
            aiResponseText.text = $"<color=cyan>[MOSS LOG]</color>\n{content}";
        }
        else
        {
            // 如果还是报错，打印详细信息
            aiResponseText.text = "Error: " + request.error + "\n" + request.downloadHandler.text;
        }
    }

    string ExtractContent(string json)
    {
        try
        {
            int contentIndex = json.IndexOf("\"content\":\"");
            if (contentIndex == -1) return "System Offline.";
            int start = contentIndex + 11;
            int end = json.IndexOf("\"", start);
            string result = json.Substring(start, end - start);
            return result.Replace("\\n", "\n").Replace("\\\"", "\"");
        }
        catch { return "Data Parsing Error."; }
    }
}