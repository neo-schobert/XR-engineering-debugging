using UnityEngine;
using System;

public class AppBootstrapper : MonoBehaviour
{
    public SolarSystemConfig config;

    public PlanetView[] planets;

    TimeModel timeModel;
    PlanetSystemController controller;
    public TimeController timeController;

    public OrbitRenderer[] orbitRenderers; // ← ajouter

    void Start()
    {
        Debug.Log("[BOOT] Initializing application");

        timeModel = new TimeModel();

        var ephemeris = new PlanetEphemerisService();

        controller = new PlanetSystemController(
            timeModel,
            ephemeris,
            planets
        );
        
        // Initialiser les OrbitRenderers

        foreach (var orbit in orbitRenderers)
        {
            orbit.Init(ephemeris);
            timeModel.OnTimeChanged += orbit.UpdateOrbit;
        }



        timeController.Init(timeModel);

    }
}
