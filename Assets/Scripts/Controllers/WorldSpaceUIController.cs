using UnityEngine;
using UnityEngine.InputSystem;

public class WorldSpaceUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] GameObject panel;

    [Header("Input")]
    [SerializeField] InputActionReference toggleAction;

    [Header("Camera")]
    [SerializeField] Transform cameraTransform;

    [Header("Positioning")]
    [SerializeField] float distance     = 1.5f;
    [SerializeField] float heightOffset = -0.2f;

    [Header("Smoothing")]
    [SerializeField] float followSpeed = 3f;
    [SerializeField] float rotateSpeed = 5f;

    bool isVisible = false;

    // -------------------------------------------------------------------------

    public void Init(Transform cam)
    {
        cameraTransform = cam;
        panel.SetActive(false);
        isVisible = false;
        Debug.Log("[UI] WorldSpaceUI initialized — hidden by default");
    }

    void OnEnable()
    {
        toggleAction.action.Enable();
        toggleAction.action.performed += OnToggle;
    }

    void OnDisable()
    {
        toggleAction.action.performed -= OnToggle;
        toggleAction.action.Disable(); // ← manquant
    }

    void OnToggle(InputAction.CallbackContext ctx) => Toggle();

    public void Toggle()
    {
        isVisible = !isVisible;
        panel.SetActive(isVisible);
        if (isVisible) SnapToCamera();
        Debug.Log("[UI] Panel " + (isVisible ? "opened" : "closed"));
    }

    // -------------------------------------------------------------------------

    void LateUpdate()
    {
        if (!isVisible || cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPos = cameraTransform.position
                          + forward * distance
                          + Vector3.up * heightOffset;

        panel.transform.position = Vector3.Lerp(
            panel.transform.position,
            targetPos,
            Time.deltaTime * followSpeed);

        Vector3 lookDir = panel.transform.position - cameraTransform.position;
        lookDir.y = 0f;

        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            panel.transform.rotation = Quaternion.Slerp(
                panel.transform.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed);
        }
    }

    void SnapToCamera()
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        panel.transform.position = cameraTransform.position
                                 + forward * distance
                                 + Vector3.up * heightOffset;

        Vector3 lookDir = panel.transform.position - cameraTransform.position;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
            panel.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}