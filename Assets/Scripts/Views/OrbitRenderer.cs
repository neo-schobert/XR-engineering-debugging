using UnityEngine;
using System;

[RequireComponent(typeof(LineRenderer))]
public class OrbitRenderer : MonoBehaviour
{
    public PlanetData.Planet planet;
    public int visiblePoints = 180;
    public int daysPerPoint = 2;

    LineRenderer lineRenderer;
    IPlanetEphemerisService ephemeris;
    Vector3[] orbitPoints;

    public void Init(IPlanetEphemerisService ephemerisService)
    {
        ephemeris = ephemerisService;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = false;
        lineRenderer.useWorldSpace = false; // ← local space !
        orbitPoints = new Vector3[visiblePoints];
        lineRenderer.positionCount = visiblePoints;
    }

public void UpdateOrbit(DateTime currentTime)
{
    for (int i = 0; i < visiblePoints; i++)
    {
        DateTime t = currentTime.AddDays(i * daysPerPoint);
        Vector3 pos = ephemeris.GetPlanetPosition(planet, t);

        // Soustrait la position locale de la planète pour centrer sur le soleil
        orbitPoints[i] = pos - transform.localPosition;
    }

    lineRenderer.SetPositions(orbitPoints);
}
    public void SetVisible(bool visible)
    {
        lineRenderer.enabled = visible;
    }
}