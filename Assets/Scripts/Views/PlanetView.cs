using UnityEngine;

public class PlanetView : MonoBehaviour
{
    public PlanetData.Planet planet;

    public void SetPosition(Vector3 pos)
    {
        transform.localPosition = pos;
    }
}
