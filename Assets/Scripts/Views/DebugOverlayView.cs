using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DebugOverlayView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI overlayText;

    [Header("Parametres")]
    [SerializeField] float updateInterval = 0.2f;

    TimeModel     timeModel;
    Queue<string> logLines       = new Queue<string>();
    string        lastAction     = "-";
    List<string>  activeWarnings = new List<string>();

    const int MAX_LOG_LINES = 4;

    float timer      = 0f;
    float deltaAccum = 0f;
    int   frameCount = 0;
    float fps        = 0f;

    bool isVisible = false;

    // -------------------------------------------------------------------------

    public void Init(TimeModel model, SolarSystemConfig config)
    {
        timeModel = model;
        SetVisible(config.showDebug);
        Debug.Log("[PERF] DebugOverlayView initialized");
    }

    public void Toggle()
    {
        SetVisible(!isVisible);
        Debug.Log("[UI] Debug overlay " + (isVisible ? "shown" : "hidden"));
    }

    void SetVisible(bool visible)
    {
        isVisible = visible;
        overlayText.gameObject.SetActive(visible);
    }

    // -------------------------------------------------------------------------

    void OnEnable()  => Application.logMessageReceived += HandleLog;
    void OnDisable() => Application.logMessageReceived -= HandleLog;

    // -------------------------------------------------------------------------

    void Update()
    {
        if (!isVisible) return;

        deltaAccum += Time.deltaTime;
        frameCount++;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            fps        = frameCount / deltaAccum;
            deltaAccum = 0f;
            frameCount = 0;
            timer      = 0f;
            RefreshOverlay();
        }
    }

    // -------------------------------------------------------------------------

    void HandleLog(string message, string stackTrace, LogType type)
    {
        string clean = CleanString(message);

        if (clean.StartsWith("[INPUT]") || clean.StartsWith("[XR]"))
            lastAction = clean;

        if (type == LogType.Warning)
        {
            string warn = "[WARN] " + clean;
            if (!activeWarnings.Contains(warn))
            {
                activeWarnings.Add(warn);
                if (activeWarnings.Count > 3)
                    activeWarnings.RemoveAt(0);
            }
        }

        bool relevant = type == LogType.Error
                     || clean.StartsWith("[TIME]")
                     || clean.StartsWith("[INPUT]")
                     || clean.StartsWith("[XR]")
                     || clean.StartsWith("[PERF]")
                     || clean.StartsWith("[UI]")
                     || clean.StartsWith("[BOOT]");

        if (!relevant) return;

        string colored;
        if (type == LogType.Error)
            colored = "<color=red>" + clean + "</color>";
        else if (type == LogType.Warning)
            colored = "<color=yellow>" + clean + "</color>";
        else
            colored = clean;

        logLines.Enqueue(colored);
        while (logLines.Count > MAX_LOG_LINES)
            logLines.Dequeue();
    }

    string CleanString(string input)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char c in input)
        {
            if (c >= 32 && c <= 126)
                sb.Append(c);
        }
        return sb.ToString();
    }

    // -------------------------------------------------------------------------

    void RefreshOverlay()
    {
        if (overlayText == null) return;

        var sb = new System.Text.StringBuilder();

        float frameMs   = fps > 0 ? 1000f / fps : 0f;
        string fpsColor = fps >= 60 ? "green" : fps >= 30 ? "yellow" : "red";
        sb.AppendLine("<color=" + fpsColor + ">" + fps.ToString("F0") + " FPS  " + frameMs.ToString("F1") + " ms</color>");

        if (timeModel != null)
        {
            string state = timeModel.IsPlaying ? "<color=green>PLAY</color>" : "<color=yellow>PAUSE</color>";
            sb.AppendLine(state + "  " + timeModel.CurrentTime.ToString("dd MMM yyyy") + "  x" + timeModel.TimeScale.ToString("F0"));
        }

        sb.AppendLine("<color=cyan>" + lastAction + "</color>");

        foreach (var w in activeWarnings)
            sb.AppendLine("<color=yellow>" + w + "</color>");

        if (logLines.Count > 0)
        {
            sb.AppendLine("---");
            foreach (var line in logLines)
                sb.AppendLine(line);
        }

        overlayText.text = sb.ToString();
    }
}