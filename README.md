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

---

---

## v1.1 — Architecture

### Objectif de cette étape

Structurer l'application avec une architecture claire avant d'ajouter les interactions VR :

- séparation des responsabilités (Model / Service / Controller / View)
- dépendances explicites via un Bootstrapper
- aucune dépendance implicite (`FindObjectOfType`, `GameObject.Find` interdits)

---

### Architecture mise en place

```
Bootstrapper
   ↓
Models / Config
   ↓
Services
   ↓
Controllers
   ↓
Views (Unity)
```

| Couche     | Fichier                     | Rôle                                                     |
| ---------- | --------------------------- | -------------------------------------------------------- |
| Model      | `TimeModel.cs`              | État de la simulation (date, vitesse, play/pause)        |
| Config     | `SolarSystemConfig.cs`      | Paramètres éditables (échelle, orbites)                  |
| Service    | `PlanetEphemerisService.cs` | Calcul des positions via `PlanetData`                    |
| Controller | `PlanetSystemController.cs` | Coordination model → service → vues                      |
| View       | `PlanetView.cs`             | Positionnement des GameObjects planètes                  |
| Bootstrap  | `AppBootstrapper.cs`        | Composition root — création et injection des dépendances |

---

### Fonctionnalités implémentées

- **TimeModel** : gestion de la date simulée avec événement `OnTimeChanged`
- **SolarSystemConfig** : ScriptableObject pour les paramètres d'échelle
- **IPlanetEphemerisService** : interface d'abstraction pour le calcul orbital
- **PlanetEphemerisService** : implémentation wrappant `PlanetData.GetPlanetPosition()`
- **PlanetSystemController** : écoute `OnTimeChanged` et met à jour toutes les vues
- **PlanetView** : applique la position calculée sur le GameObject parent (Empty)
- **AppBootstrapper** : instancie et connecte tous les objets au `Start()`
- Structure de scène :
  ```
  App               ← AppBootstrapper
  SolarSystem
   ├── Sun
   ├── Mercury
   ├── Venus
   ├── Earth
   ├── Mars
   ├── Jupiter
   ├── Saturn
   ├── Uranus
   └── Neptune
  ```
  Chaque planète : Empty avec `PlanetView` + Asset Enfant pour le visuel et taille 0.25 pour que ce soit lisible.

---

### Debug story — Correction du mapping des coordonnées

**Problème rencontré :** les planètes n'apparaissaient pas dans le bon plan à l'exécution.

**Diagnostic :** la fonction `PlanetData.GetPlanetPosition()` retourne des coordonnées en repère écliptique (X, Y, Z) alors qu'Unity utilise un repère main-gauche avec Y comme axe vertical.

**Fix appliqué** dans `PlanetEphemerisService.cs` : inversion des coordonnées Y et Z pour mapper correctement le plan orbital sur le plan XZ d'Unity :

```csharp
Vector3 raw = PlanetData.GetPlanetPosition(planet, date);
return new Vector3(raw.x, raw.z, raw.y);
```

**Problème secondaire :** les positions de Vénus et de la Terre semblaient inversées dans la scène — résolu en vérifiant l'assignation des enum `PlanetData.Planet` dans chaque `PlanetView` depuis l'inspecteur Unity.

---

### Capture — Système solaire positionné

![Architecture - Système solaire](images%20rapport/Capture%20v1.1.png)

> Vue de la scène avec les planètes positionnées à `DateTime.Now` via le pipeline Model → Service → Controller → View.

---

### Tag Git

```
v1.1-architecture
```

```bash
git tag -a v1.1-architecture -m "Architecture - models, services, controllers, views"
git push origin main
git push origin --tags
```
