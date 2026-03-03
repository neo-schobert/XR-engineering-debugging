using System;
using UnityEngine;

public class PlanetEphemerisService : IPlanetEphemerisService
{
    public Vector3 GetPlanetPosition(PlanetData.Planet planet, DateTime date)
    {
        Vector3 pos = PlanetData.GetPlanetPosition(planet, date);

        return new Vector3(pos.x, pos.z, pos.y);
    }
}
