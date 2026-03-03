# TP XR — Solar System Workbench

## v1.0 — Boot XR

### Objectif de cette étape

Mettre en place une scène VR minimale et fonctionnelle avec :

- un environnement de base (sol + repère d'axes)
- un cube interactable (grab) pour valider XRI
- la configuration du build debug
- le XR Device Simulator pour tester sans casque

---

### Fonctionnalités implémentées

- **Scène Boot** avec sol (Plane) et repère visuel XYZ (axes colorés Rouge/Vert/Bleu)
- **XR Origin** configuré en mode `Floor` avec Left & Right Hand Controllers
- **XR Interaction Manager** présent dans la scène
- **GrabCube** interactable :
  - `Rigidbody` + `Box Collider` + `XR Grab Interactable`
  - Movement Type : `Velocity Tracking`
- **XR Device Simulator** intégré pour itérer sans casque depuis l'éditeur
- **Build Settings** configurés :
  - ✅ Development Build
  - ✅ Script Debugging
  - ✅ Autoconnect Profiler

---

### Capture — Game View (XR Device Simulator)

![Scène Boot - Game View](images%20rapport/Capture%20v1.0.png)

> Vue de la scène Boot depuis le XR Device Simulator dans Unity.  
> On distingue le sol et le cube grabbable.

---

### Tag Git

```
v1.0-boot
```

```bash
git tag -a v1.0 -m "Boot XR - scène minimale validée"
git push origin main
git push origin --tags
```
