using System;
using UnityEngine;

public class PlanetSystemController
{
    TimeModel timeModel;
    IPlanetEphemerisService ephemeris;

    PlanetView[] planets;

    public PlanetSystemController(
        TimeModel timeModel,
        IPlanetEphemerisService ephemeris,
        PlanetView[] planets)
    {
        this.timeModel = timeModel;
        this.ephemeris = ephemeris;
        this.planets = planets;

        timeModel.OnTimeChanged += UpdatePlanets;
    }

    void UpdatePlanets(DateTime time)
    {
        Debug.Log("[TIME] Updating planets " + time);

        foreach (var planet in planets)
        {
            Vector3 pos =
                ephemeris.GetPlanetPosition(planet.planet, time);

            planet.SetPosition(pos);
        }
    }
}
