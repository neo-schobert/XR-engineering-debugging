using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TimeUIController : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] TextMeshProUGUI dateLabel;
    [SerializeField] Slider          dateSlider;
    [SerializeField] Slider          speedSlider;
    [SerializeField] TextMeshProUGUI speedLabel;
    [SerializeField] Button          playPauseButton;
    [SerializeField] TextMeshProUGUI playPauseLabel;
    [SerializeField] Toggle          orbitToggle;
    [SerializeField] Button          resetViewButton;
    [SerializeField] Button          resetScaleButton;

    TimeModel        timeModel;
    OrbitRenderer[]  orbitRenderers;
    ScaleController  scaleController;
    Transform        xrOrigin;
    SolarSystemConfig config;

    bool suppressCallbacks = false;

    // -------------------------------------------------------------------------

    public void Init(
        TimeModel        model,
        OrbitRenderer[]  orbits,
        ScaleController  scale,
        Transform        origin,
        SolarSystemConfig cfg)
    {
        timeModel       = model;
        orbitRenderers  = orbits;
        scaleController = scale;
        xrOrigin        = origin;
        config          = cfg;

        SetupListeners();
        timeModel.OnTimeChanged += OnTimeChanged;
        RefreshAll(timeModel.CurrentTime);

        Debug.Log("[UI] TimeUIController initialized");
    }

    // -------------------------------------------------------------------------

    void SetupListeners()
    {
        playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        resetViewButton.onClick.AddListener(OnResetView);
        resetScaleButton.onClick.AddListener(OnResetScale);
        orbitToggle.onValueChanged.AddListener(OnOrbitToggled);
        orbitToggle.isOn = config.showOrbits;

        dateSlider.minValue = 0f;
        dateSlider.maxValue = 1f;
        dateSlider.onValueChanged.AddListener(OnDateSliderChanged);

        speedSlider.minValue = config.speedMin;
        speedSlider.maxValue = config.speedMax;
        speedSlider.value    = config.initialTimeScale;
        speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
    }

    void OnPlayPauseClicked()
    {
        if (timeModel.IsPlaying) timeModel.Pause();
        else                     timeModel.Play();

        playPauseLabel.text = timeModel.IsPlaying ? "⏸" : "▶";
        Debug.Log("[UI] " + (timeModel.IsPlaying ? "Playing" : "Paused"));
    }

    void OnDateSliderChanged(float t)
    {
        if (suppressCallbacks) return;

        DateTime minDate  = new DateTime(config.yearMin, 1, 1);
        DateTime maxDate  = new DateTime(config.yearMax, 1, 1);
        double   total    = (maxDate - minDate).TotalDays;
        DateTime picked   = minDate.AddDays(t * total);

        timeModel.SetTime(picked);
        Debug.Log($"[UI] Date scrubbed to {picked:yyyy-MM-dd}");
    }

    void OnSpeedSliderChanged(float value)
    {
        timeModel.SetScale(value);
        speedLabel.text = $"x{value:F0}";
        Debug.Log($"[UI] Speed set to x{value:F0}");
    }

    void OnOrbitToggled(bool visible)
    {
        foreach (var orbit in orbitRenderers)
            orbit.SetVisible(visible);

        Debug.Log($"[UI] Orbits {(visible ? "shown" : "hidden")}");
    }

    void OnResetView()
    {
        if (xrOrigin == null) return;
        xrOrigin.position = Vector3.zero;
        xrOrigin.rotation = Quaternion.identity;
        Debug.Log("[UI] Viewpoint reset");
    }

    void OnResetScale()
    {
        scaleController?.Reset();
        Debug.Log("[UI] Scale reset");
    }

    // -------------------------------------------------------------------------

    void OnTimeChanged(DateTime t) => RefreshAll(t);

    void RefreshAll(DateTime t)
    {
        dateLabel.text = t.ToString("dd MMM yyyy");

        suppressCallbacks = true;
        DateTime minDate  = new DateTime(config.yearMin, 1, 1);
        DateTime maxDate  = new DateTime(config.yearMax, 1, 1);
        double   total    = (maxDate - minDate).TotalDays;
        double   elapsed  = (t - minDate).TotalDays;
        dateSlider.value  = Mathf.Clamp01((float)(elapsed / total));
        suppressCallbacks = false;

        playPauseLabel.text = timeModel.IsPlaying ? "⏸" : "▶";
    }

    void OnDestroy()
    {
        if (timeModel != null)
            timeModel.OnTimeChanged -= OnTimeChanged;
    }
}