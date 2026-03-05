using UnityEngine;

[CreateAssetMenu(menuName = "XR/Solar System Config")]
public class SolarSystemConfig : ScriptableObject
{
    [Header("Echelle")]
    public float distanceScale   = 0.000001f;
    public float planetSizeScale = 0.01f;
    public float minScale        = 0.1f;
    public float maxScale        = 5f;

    [Header("Simulation")]
    public float initialTimeScale = 1f;
    public float speedMin         = 1f;
    public float speedMax         = 365f;

    [Header("Date")]
    public int yearMin = 2000;
    public int yearMax = 2100;

    [Header("Orbites")]
    public bool showOrbits = true;

    [Header("Debug")]
    public bool showDebug = true;
}