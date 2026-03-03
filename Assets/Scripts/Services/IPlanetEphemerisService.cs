using System;
using UnityEngine;

public interface IPlanetEphemerisService
{
    Vector3 GetPlanetPosition(PlanetData.Planet planet, DateTime date);
}
