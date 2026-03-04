using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SolarSystemHandle : XRGrabInteractable
{
    public Transform targetParent;
    public ScaleController scaleController;

    private Vector3 localPositionOffset;
    private Quaternion localRotationOffset;

    // Two-hand scale
    private float initialHandDistance;
    private float initialScale;
    private bool isTwoHanded = false;

    protected override void Awake()
    {
        base.Awake();
        movementType = MovementType.Instantaneous;
        trackScale = false;
        trackPosition = false;
        transform.localScale = targetParent.localScale/2;
    }


    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Debug.Log("[XR] Table grabbed");

        if (interactorsSelecting.Count == 2)
        {
            // Deux mains → mode scale
            StartTwoHandScale();
        }
        else if (interactorsSelecting.Count == 1)
        {
            // Une main → mode move
            if (targetParent == null) return;
            Transform interactorTransform = args.interactorObject.GetAttachTransform(this);
            localPositionOffset = interactorTransform.InverseTransformPoint(targetParent.position);
            localRotationOffset = Quaternion.Inverse(interactorTransform.rotation) * targetParent.rotation;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        Debug.Log("[XR] Table released");
        isTwoHanded = false;

        // Si il reste une main, reprend le mode move
        if (interactorsSelecting.Count == 1)
        {
            Transform interactorTransform = interactorsSelecting[0].GetAttachTransform(this);
            localPositionOffset = interactorTransform.InverseTransformPoint(targetParent.position);
            localRotationOffset = Quaternion.Inverse(interactorTransform.rotation) * targetParent.rotation;
        }
    }

    void StartTwoHandScale()
    {
        isTwoHanded = true;
        var hand1 = interactorsSelecting[0].GetAttachTransform(this).position;
        var hand2 = interactorsSelecting[1].GetAttachTransform(this).position;
        initialHandDistance = Vector3.Distance(hand1, hand2);
        initialScale = scaleController.currentScale;
        Debug.Log("[INPUT] Two-hand scale started");
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
    
        if (!isSelected || targetParent == null) return;
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) return;
    
        if (interactorsSelecting.Count == 2)
        {
            // Mode scale uniquement
            if (!isTwoHanded) StartTwoHandScale();
    
            var hand1 = interactorsSelecting[0].GetAttachTransform(this).position;
            var hand2 = interactorsSelecting[1].GetAttachTransform(this).position;
            float currentDistance = Vector3.Distance(hand1, hand2);
            float ratio = currentDistance / initialHandDistance;
            scaleController.SetScale(initialScale * ratio);

        }
        else if (interactorsSelecting.Count == 1)
        {
            // Mode move uniquement si une seule main
            isTwoHanded = false;
            Transform interactorTransform = firstInteractorSelecting.GetAttachTransform(this);
            targetParent.position = interactorTransform.TransformPoint(localPositionOffset);
            targetParent.rotation = interactorTransform.rotation * localRotationOffset;
        }
        transform.localScale = targetParent.localScale/2;
        transform.localPosition = targetParent.localPosition;
    }
}