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

---

---

## v1.3 — Interactions VR : Grab / Échelle / Sélection

### Objectif de cette étape

Transformer la simulation en objet manipulable en VR :

- déplacer le système solaire dans l'espace (grab)
- modifier son échelle à deux mains (pinch / spread)
- sélectionner une planète pour afficher ses informations

---

### Flux des interactions

```
XR Interactable (Handle)
         ↓
  Script d'interaction
         ↓
     Controller
         ↓
       Views
```

Aucune interaction ne manipule directement les transforms des planètes. Tout passe par un contrôleur dédié.

---

### Structure de la scène

```
SolarSystemRoot        ← point de manipulation global
 ├── SolarSystem
 │    ├── Sun
 │    ├── Mercury
 │    ├── Venus
 │    ├── Earth
 │    ├── Mars
 │    ├── Jupiter
 │    ├── Saturn
 │    ├── Uranus
 │    └── Neptune
 └── Handle            ← poignée de grab + contrôle d'échelle
```

`SolarSystemRoot` est l'objet racine sur lequel sont appliquées toutes les transformations (position, rotation, échelle). Les planètes en sont enfants, ce qui évite de les modifier individuellement.

---

### Fonctionnalités implémentées

#### 1 — Grab

- Un objet `Handle` porte un `XR Grab Interactable` (Rigidbody kinematic + Collider)
- Saisir le handle avec **une manette** déplace l'ensemble de `SolarSystemRoot`
- Logs émis :
  ```
  [XR] Table grabbed
  [XR] Table released
  ```

#### 2 — Contrôle de l'échelle à deux mains

- L'utilisateur grab le `Handle` avec **les deux manettes simultanément**
- **Écarter** les manettes → zoom in (agrandissement)
- **Rapprocher** les manettes → zoom out (réduction)
- L'échelle est centralisée dans un composant `ScaleController` :
  - expose une méthode `SetScale(float value)`
  - applique la transformation sur `SolarSystemRoot`
  - clamp l'échelle dans des limites définies pour éviter les valeurs aberrantes
- La distance de référence (`initialHandDistance`) et l'échelle de référence (`initialScale`) sont capturées au moment où le second grab est détecté. L'échelle courante est ensuite calculée par ratio :

```csharp
float ratio = currentHandDistance / initialHandDistance;
scaleController.SetScale(initialScale * ratio);
```

- Logs émis :
  ```
  [INPUT] Scale requested
  [XR] Scale applied
  [WARN] Scale clamped
  ```

#### 3 — Sélection de planète et panneau d'informations

- Chaque planète porte un script `PlanetSelectable` :
  - détectable par un `XR Ray Interactor`
  - signale la sélection sans contenir de logique UI
- Un `FocusController` reçoit l'événement et :
  - enregistre la planète active
  - active un panneau d'informations (`InfoPanel`)
- Le panneau affiche :
  - nom de la planète
  - distance au Soleil
  - période orbitale
  - date simulée courante
  - la planette elle-même en rotation
- Un **bouton Fermer** permet de fermer le panneau
- Le panneau **suit le joueur** — canvas en world space repositionné chaque frame devant le regard

---

### Flux de sélection

```
Ray Interactor
      ↓
PlanetSelectable
      ↓
FocusController
      ↓
InfoPanel (UI)
```

---

### Capture — Interactions VR

![Interactions VR - Grab / Échelle / Sélection](images%20rapport/Capture%20v1.3.png)

> Le système solaire est manipulable en VR. Le panneau d'information s'ouvre à la sélection d'une planète et suit le joueur.

---

### Tag Git

```
v1.3-vr-interactions
```

```bash
git tag -a v1.3-vr-interactions -m "VR interactions - grab, scale, planet selection + info panel"
git push origin main
git push origin --tags
```

---

---

## v1.4 — UI world-space + instrumentation

### Objectif de cette étape

Rendre l'application pilotable et déboguable directement dans le casque :

- un panneau de contrôle world-space accessible via le bouton B
- un overlay debug toujours visible affichant les métriques clés (désactivable dans le panneau de contrôle)
- tous les événements critiques loggés avec des tags structurés

---

### Flux UI

```
Bouton B (right controller)
          ↓
    WorldSpaceUI.Toggle()
          ↓
    Canvas world-space
          ↓
    TimeUIController
          ↓
       TimeModel
```

Le panneau suit le joueur via un billboard smoothé. `WorldSpaceUI` tourne sur le GameObject `App` (toujours actif) pour pouvoir écouter le bouton même quand le Canvas est désactivé.

---

### A — Panneau de contrôle world-space

- **Canvas world-space** interactif avec XR Interaction Toolkit
- Activé / désactivé avec le **bouton B** du right controller (toggle)
- À l'ouverture, le panneau se snap instantanément devant le joueur, puis suit en douceur (lerp/slerp)
- Contenu du panneau :

| Élément             | Rôle                                                                               |
| ------------------- | ---------------------------------------------------------------------------------- |
| Slider date         | Scrub temporel sur la plage `yearMin` → `yearMax` définie dans `SolarSystemConfig` |
| Slider vitesse      | Contrôle `TimeModel.TimeScale` de `speedMin` à `speedMax`                          |
| Bouton Play/Pause   | Appelle `TimeModel.Play()` / `TimeModel.Pause()`                                   |
| Toggle trajectoires | Active/désactive tous les `OrbitRenderer`                                          |
| Bouton Reset view   | Remet `XROrigin` à l'origine                                                       |
| Bouton Reset scale  | Appelle `ScaleController.Reset()`                                                  |
| Bouton Debug        | Toggle l'overlay debug via `DebugOverlayView.Toggle()` (bouton Grab du Contrôleur) |
| Bouton Close        | Toggle le panneau de contrôle (bouton Grab du Contrôleur)                          |

- Toutes les valeurs de configuration (plages de dates, vitesse min/max, état initial des orbites et du debug) sont centralisées dans `SolarSystemConfig`

---

### B — DebugOverlay

- Canvas **world-space enfant direct de la XR Camera** — toujours visible dans le casque
- Mis à jour toutes les 200ms pour ne pas peser sur les perfs
- Contenu affiché :

```
60 FPS  16.7 ms
PLAY  12 Jun 2025  x10
[XR] Table grabbed
---
[BOOT] Application ready
[UI] Playing
[XR] Scale applied: 1.42
[INPUT] Scale requested: 1.42
```

- FPS coloré en vert / jaune / rouge selon le seuil (60 / 30)
- Dernière action `[INPUT]` ou `[XR]` mise en avant
- Warnings actifs affichés séparément (3 max, dédupliqués)
- 4 derniers logs structurés conservés, les anciens sont écrasés
- Toggle via `DebugOverlayView.Toggle()` — état initial lu depuis `SolarSystemConfig.showDebug`

Setup scène :

```
XR Origin
 └── Camera Offset
      └── Main Camera (XR Camera)
           └── DebugCanvas        <- Canvas World Space
                                     Scale : (0.001, 0.001, 0.001)
                                     Position locale : (0.25, -0.22, 0.5)
                └── DebugText     <- TextMeshProUGUI
                                     Anchor : bottom-right
                                     Font size : 18
```

---

### C — Logs structurés

Tous les événements critiques suivent le format :

| Tag       | Exemples                         |
| --------- | -------------------------------- |
| `[BOOT]`  | initialisation, ready            |
| `[TIME]`  | mise à jour des planètes         |
| `[INPUT]` | scale requested, date scrubbed   |
| `[XR]`    | grabbed, released, scale applied |
| `[UI]`    | play, pause, orbits shown/hidden |
| `[PERF]`  | overlay initialized              |

---

### Capture — UI world-space + Debug Overlay

![UI world-space + DebugOverlay](images%20rapport/Capture%20v1.4.png)

> Panneau de contrôle ouvert en VR avec l'overlay debug visible en bas à droite.

---

### Tag Git

```
v1.4-ui-debug
```

```bash
git tag -a v1.4-ui-debug -m "UI world-space + DebugOverlay + logs structures"
git push origin main
git push origin --tags
```
