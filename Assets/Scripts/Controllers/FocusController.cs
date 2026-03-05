using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FocusController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject focusPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI periodText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI dateText;
    public GameObject closeButton;

    [Header("Planet Preview")]
    public Renderer planetPreview;
    public GameObject planetPreviewSphere;
    public float previewRotationSpeed = 30f;

    [Header("Camera / Panel Position")]
    public Transform cameraTransform;
    public float panelDistance = 1.5f;
    public float panelHeightOffset = 1.6f;
    public float followSpeed = 5f;

    PlanetEphemerisService ephemeris;
    TimeModel timeModel;
    PlanetView focusedPlanet;
    CanvasGroup canvasGroup;
    bool isOpen = false;
    bool snapNextFrame = false;

    // -------------------------------------------------------
    // Init
    // -------------------------------------------------------
    public void Init(PlanetEphemerisService ephemerisService, TimeModel timeModelRef)
    {
        ephemeris = ephemerisService;
        timeModel = timeModelRef;

        canvasGroup = focusPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("[FOCUS] CanvasGroup manquant sur focusPanel !");
            return;
        }

        HidePanel();

        PlanetSelectable.OnPlanetSelected += OnPlanetSelected;
        Debug.Log("[FOCUS] Init OK - cameraTransform = " + cameraTransform);
    }

    void OnDisable()
    {
        PlanetSelectable.OnPlanetSelected -= OnPlanetSelected;
    }

    // -------------------------------------------------------
    // Show / Hide via CanvasGroup
    // -------------------------------------------------------
    void ShowPanel()
    {
        canvasGroup.alpha          = 1f;
        canvasGroup.interactable   = true;
        canvasGroup.blocksRaycasts = true;
        planetPreviewSphere.SetActive(true);
        isOpen = true;
    }

    void HidePanel()
    {
        canvasGroup.alpha          = 0f;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;
        planetPreviewSphere.SetActive(false);
        isOpen = false;
        closeButton.SetActive(false);
    }

    // -------------------------------------------------------
    // Update
    // -------------------------------------------------------
    void Update()
    {
        if (isOpen && planetPreview != null)
            planetPreview.transform.Rotate(Vector3.up, previewRotationSpeed * Time.deltaTime);

        if (!isOpen || cameraTransform == null)
            return;

        if (snapNextFrame)
        {
            snapNextFrame = false;
            PlacePanel();
            return;
        }

        Vector3 targetPos = GetTargetPosition();
        Quaternion targetRot = GetTargetRotation(targetPos);

        focusPanel.transform.position = Vector3.Lerp(
            focusPanel.transform.position, targetPos, Time.deltaTime * followSpeed);
        focusPanel.transform.rotation = Quaternion.Slerp(
            focusPanel.transform.rotation, targetRot, Time.deltaTime * followSpeed);
    }

    // -------------------------------------------------------
    // Positionnement
    // -------------------------------------------------------
    Vector3 GetTargetPosition()
    {
        Vector3 flatForward = new Vector3(
            cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;

        if (flatForward == Vector3.zero)
            flatForward = Vector3.forward;

        Vector3 cameraFlat = new Vector3(
            cameraTransform.position.x, 0f, cameraTransform.position.z);

        return cameraFlat + flatForward * panelDistance + Vector3.up * panelHeightOffset;
    }

    Quaternion GetTargetRotation(Vector3 targetPos)
    {
        Vector3 dir = targetPos - cameraTransform.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return Quaternion.identity;
        return Quaternion.LookRotation(dir);
    }

    void PlacePanel()
    {
        Vector3 targetPos = GetTargetPosition();
        focusPanel.transform.position = targetPos;
        focusPanel.transform.rotation = GetTargetRotation(targetPos);
        Debug.Log("[FOCUS] Panel snapped to " + targetPos);
    }

    // -------------------------------------------------------
    // Sélection d'une planète
    // -------------------------------------------------------
    void OnPlanetSelected(PlanetView planetView)
    {
        if (focusedPlanet == planetView)
        {
            Unfocus();
            return;
        }

        focusedPlanet = planetView;

        PlanetInfoModel info = ephemeris.GetInfo(planetView.planet);
        Debug.Log("[FOCUS] Selected : " + info.Name);

        nameText.text     = info.Name;
        periodText.text   = $"Période orbitale : {info.OrbitalPeriodDays:F0} jours";
        distanceText.text = $"Distance au soleil : {info.DistanceAU:F2} UA";
        dateText.text     = $"Date simulée : {timeModel.CurrentTime:dd/MM/yyyy}";
        closeButton.SetActive(true);

        // Récupère le material via PlanetView et l'applique à la sphère plate
        if (planetPreview != null)
        {
            Material mat = planetView.GetPlanetMaterial();
            if (mat != null)
            {
                planetPreview.material = mat;
                Debug.Log("[FOCUS] Material appliqué : " + mat.name);
            }
            else
            {
                Debug.LogWarning("[FOCUS] GetPlanetMaterial() a retourné null pour " + info.Name);
            }
        }

        ShowPanel();
        snapNextFrame = true;
    }

    // -------------------------------------------------------
    // Fermeture
    // -------------------------------------------------------
    public void Unfocus()
    {
        Debug.Log("[FOCUS] Unfocus called");

        focusedPlanet = null;
        snapNextFrame = false;
        HidePanel();
    }
}