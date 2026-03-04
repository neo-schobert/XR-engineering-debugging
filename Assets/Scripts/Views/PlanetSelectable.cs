using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System;

public class PlanetSelectable : XRSimpleInteractable
{
    public PlanetView planetView;
    public static event Action<PlanetView> OnPlanetSelected;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Debug.Log("[XR] Planet selected: " + planetView.planet);
        OnPlanetSelected?.Invoke(planetView);
    }
}