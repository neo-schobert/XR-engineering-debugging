using UnityEngine;

public class PlanetView : MonoBehaviour
{
    public PlanetData.Planet planet;
    public Material planetMaterial;

    public void SetPosition(Vector3 pos)
    {
        transform.localPosition = pos;
    }
    public Material GetPlanetMaterial()
    {
        return planetMaterial;
    }
}
