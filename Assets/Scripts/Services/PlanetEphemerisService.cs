using System;
using UnityEngine;

public class PlanetEphemerisService : IPlanetEphemerisService
{
    public Vector3 GetPlanetPosition(PlanetData.Planet planet, DateTime date)
    {
        return PlanetData.GetPlanetPosition(planet, date);
    }
}
