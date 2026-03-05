using UnityEngine;

public class AppBootstrapper : MonoBehaviour
{
    [Header("Config")]
    public SolarSystemConfig config;

    [Header("Planètes")]
    public PlanetView[] planets;

    [Header("Orbites")]
    public OrbitRenderer[] orbitRenderers;

    [Header("Controllers (scène)")]
    public TimeController   timeController;
    public ScaleController  scaleController;
    public FocusController  focusController;
    public TimeUIController timeUIController;
    public WorldSpaceUI     worldSpaceUI;
    public DebugOverlayView     debugOverlay;      // NOUVEAU

    [Header("Références scène")]
    public Transform solarSystemRoot;
    public Transform xrOrigin;
    public Transform xrCamera;

    TimeModel              timeModel;
    PlanetSystemController planetSystemController;
    PlanetEphemerisService ephemeris;

    void Start()
    {
        Debug.Log("[BOOT] Initializing application");

        // --- Models & Services ---
        timeModel = new TimeModel();
        timeModel.SetScale(config.initialTimeScale);

        ephemeris = new PlanetEphemerisService();

        // --- Planet system ---
        planetSystemController = new PlanetSystemController(timeModel, ephemeris, planets);

        // --- Orbit renderers ---
        foreach (var orbit in orbitRenderers)
        {
            orbit.Init(ephemeris);
            timeModel.OnTimeChanged += orbit.UpdateOrbit;
            orbit.SetVisible(config.showOrbits);
        }

        // --- Scale ---
        scaleController.Init(solarSystemRoot, config);

        // --- Focus / info panel ---
        focusController.Init(ephemeris, timeModel);

        // --- Billboard ---
        worldSpaceUI.Init(xrCamera);

        // --- UI panel ---
        timeUIController.Init(timeModel, orbitRenderers, scaleController, xrOrigin, config);

        // --- Debug overlay ---
        debugOverlay.Init(timeModel, config);

        // --- Time ---
        timeController.Init(timeModel);

        Debug.Log("[BOOT] Application ready");
    }
}