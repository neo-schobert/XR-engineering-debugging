using UnityEngine;

[CreateAssetMenu(menuName = "XR/Solar System Config")]
public class SolarSystemConfig : ScriptableObject
{
    public float distanceScale = 0.000001f;
    public float planetSizeScale = 0.01f;
    public bool showOrbits = true;
}
