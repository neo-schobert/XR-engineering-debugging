using System;
using UnityEngine;

public class PlanetEphemerisService : IPlanetEphemerisService
{
    public Vector3 GetPlanetPosition(PlanetData.Planet planet, DateTime date)
    {
        return PlanetData.GetPlanetPosition(planet, date);
    }

    public PlanetInfoModel GetInfo(PlanetData.Planet planet)
    {
        DateTime epoch = new DateTime(2000, 1, 1);

        // Distance moyenne sur 12 échantillons à 30 jours d'intervalle
        float totalDistance = 0f;
        int samples = 12;
        for (int i = 0; i < samples; i++)
        {
            Vector3 pos = GetPlanetPosition(planet, epoch.AddDays(i * 30));
            totalDistance += new Vector3(pos.x, 0, pos.z).magnitude;
        }
        float distanceAU = totalDistance / samples;

        // 3ème loi de Kepler : T (jours) = a^(3/2) * 365.25
        float periodDays = Mathf.Pow(distanceAU, 1.5f) * 365.25f;

        Debug.Log($"[INFO] {planet} — distance: {distanceAU:F2} AU, période: {periodDays:F0} jours");

        return new PlanetInfoModel
        {
            Name = planet.ToString(),
            OrbitalPeriodDays = periodDays,
            DistanceAU = distanceAU
        };
    }
}