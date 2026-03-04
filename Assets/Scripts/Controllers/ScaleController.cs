using UnityEngine;

public class ScaleController : MonoBehaviour
{
    public Transform target; // SolarSystemRoot
    public float minScale = 0.1f;
    public float maxScale = 5f;
    public float currentScale = 1f;

    public void SetScale(float value)
    {
        Debug.Log("[INPUT] Scale requested: " + value);

        float clamped = Mathf.Clamp(value, minScale, maxScale);

        if (clamped != value)
            Debug.LogWarning("[WARN] Scale clamped to " + clamped);

        currentScale = clamped;
        target.localScale = Vector3.one * currentScale;

        Debug.Log("[XR] Scale applied: " + currentScale);
    }

    public void ScaleUp() => SetScale(currentScale * 1.2f);
    public void ScaleDown() => SetScale(currentScale / 1.2f);
}