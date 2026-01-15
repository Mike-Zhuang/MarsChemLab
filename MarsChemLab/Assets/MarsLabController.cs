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

    [Header("配置")]
    // Key 将从本地文件加载
    public string zhipuApiKey = "";

    // 化学常量
    private const float M_Na2O2 = 78f;
    private const float M_O2 = 32f;
    private const float ConsumptionRate = 50f;

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
        // 从 Assets/api_key.txt 读取 API Key
        string path = System.IO.Path.Combine(Application.dataPath, "api_key.txt");
        if (System.IO.File.Exists(path))
        {
            zhipuApiKey = System.IO.File.ReadAllText(path).Trim();
        }
        else
        {
            Debug.LogError("API Key file not found at: " + path);
            if (aiResponseText != null) aiResponseText.text = "Error: API Key missing.";
        }
    }

    void OnSliderChange(float val)
    {
        if (valueDisplay != null)
            valueDisplay.text = $"Na2O2 Input: {val:F0} g";
    }

    public void OnSubmit()
    {
        float mass = massSlider.value;
        float molesNa2O2 = mass / M_Na2O2;
        float molesO2 = molesNa2O2 * 0.5f;
        float massO2 = molesO2 * M_O2;
        float survivalHours = massO2 / ConsumptionRate;

        // 本地计算结果
        string localResult = $"[SYSTEM CALCULATION]\nO2 Produced: {massO2:F1}g\nEst. Survival: {survivalHours:F1} Hours";
        aiResponseText.text = localResult + "\n\n<color=yellow>Connecting to MOSS AI...</color>";

        // 呼叫 AI
        StartCoroutine(CallZhipuAI(massO2, survivalHours));
    }

    IEnumerator CallZhipuAI(float o2, float hours)
    {
        // 这里的 Prompt 设定了 AI 扮演 MOSS
        string prompt = $"You are MOSS, the AI of a Mars Base. " +
                        $"A student engineer added chemical reactants producing {o2:F1}g of Oxygen, " +
                        $"allowing survival for {hours:F1} hours. " +
                        $"If hours < 5, warn them urgently in English (USE CAPS). " +
                        $"If hours > 10, congratulate them calmly in English. " +
                        $"Keep it extremely short (under 20 words).";

        // 构造 JSON
        string json = "{" +
            "\"model\": \"glm-4\"," +
            "\"messages\": [" +
                "{\"role\": \"user\", \"content\": \"" + prompt + "\"}" +
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
            string response = request.downloadHandler.text;
            string content = ExtractContent(response);
            aiResponseText.text = $"[MOSS LOG]\n{content}";
        }
        else
        {
            aiResponseText.text = "Error: " + request.error;
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
            // 简单处理转义字符
            string result = json.Substring(start, end - start);
            return result.Replace("\\n", "\n").Replace("\\\"", "\"");
        }
        catch
        {
            return "Data Error.";
        }
    }
}