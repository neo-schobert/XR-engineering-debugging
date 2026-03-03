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

---

---

## v1.2 — Événements + Trajectoires

### Objectif de cette étape

Mettre en place le flux complet de la simulation temporelle et afficher les trajectoires orbitales en temps réel :

- le temps avance via un `TimeController`
- chaque changement déclenche un événement `OnTimeChanged`
- les planètes et les trajectoires se mettent à jour en réaction

---

### Flux de la simulation

```
                TimeController (Update)
                       ↓
                  TimeModel.SetTime()
                       ↓
                  OnTimeChanged (event)
                       ↙               ↘
        PlanetSystemController          OrbitRenderer[]
                ↓
        PlanetView.SetPosition()
```

À chaque tick de simulation, `TimeController` fait avancer la date simulée et appelle `TimeModel.SetTime()`. Cela déclenche l'événement `OnTimeChanged`, auquel sont abonnés le `PlanetSystemController` (qui repositionne les planètes) et chaque `OrbitRenderer` (qui met à jour sa trajectoire). Aucune logique métier ne tourne dans `Update()` côté planètes — tout passe par les événements.

---

### Fonctionnalités implémentées

- **TimeController** : fait avancer `DateTime` à chaque frame selon `secondsPerDay`, appelle `TimeModel.SetTime()`
- **OrbitRenderer** : trajectoire de prévision future par planète, via rolling buffer
  - `UpdateOrbit()` : à chaque `OnTimeChanged`, écrase le point le plus vieux avec le nouveau point le plus loin dans le futur
  - `SetVisible()` : active/désactive la trajectoire
- **AppBootstrapper** mis à jour : initialise les `OrbitRenderer` et les abonne à `OnTimeChanged`

---

### Debug story — Offset des planètes et trajectoires incorrectes

**Problème rencontré :** les trajectoires affichées ne correspondaient pas aux orbites réelles. Les cercles apparaissaient décentrés et sans rapport avec la position des planètes dans la scène.

**Diagnostic :** un offset de position s'était glissé dans le calcul des points d'orbite. Les positions calculées par `OrbitRenderer` n'étaient pas cohérentes avec celles calculées par `PlanetSystemController` — les deux utilisaient le même service mais les résultats divergeaient visuellement, ce qui a mis en cause le script de trajectoire.

**Fix appliqué :** correction du calcul dans `UpdateOrbit()` pour que les points soient bien calculés en world space, cohérent avec les positions des planètes. Un paramètre `yOffset` a été exposé dans l'Inspector pour permettre un ajustement fin si besoin :

```csharp
points[i] = ephemeris.GetPlanetPosition(planet, futureTime) + new Vector3(0, yOffset, 0);
```

---

### Capture — Trajectoires orbitales

![Événements + Trajectoires](images%20rapport/Capture%20v1.2.png)

> Les planètes se déplacent en temps réel et les trajectoires de prévision suivent correctement les orbites.

---

### Tag Git

```
v1.2-events
```

```bash
git tag -a v1.2-events -m "Events + OrbitRenderer - simulation temporelle et trajectoires"
git push origin main
git push origin --tags
```
