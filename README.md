## Erika Archer - Shadow on Luminascar

### Game Overview & Core Logic

**"Infiltrate the castle. Steal the light. Rebel against the darkness."**

* **Objective:** Retrieve the Star Crystal hidden in the Hall of Dawn.
* **Protagonist:** Erika, an orphan with no powers but a master of infiltration.
* **Setting:** A world where light is imprisoned and darkness reigns.

The gameplay revolves around stealth and tension rather than direct combat. Erika avoids confrontations and relies on shadows to stay undetected. Every visual choice (shadows, lighting, confined spaces) reinforces a sense of oppression and secrecy. Design decisions—animations, AI, menus, shaders—are all driven by the narrative.

**Tutorial:** Gradually introduces core stealth mechanics and environmental navigation through progressive trials.

---

### Visual & Artistic Direction

**"Light and shadow are not just atmosphere but narrative language."**

---

### AI & Navigation

**AI driven by NavMesh:** Enemies patrol, chase, and react to the player’s movements.

**Navigation System & AI Components**

* **NavMesh Surface:** Defines walkable areas.
* **NavMesh Agent:** Used by guards for movement.
* **NavMesh Obstacle:** Two barrels that the player can move, dynamically updating enemy paths.

**Finite State Machine (EnemyController.cs + Animator)**
Guards follow a state-based logic:

1. **Patrol:** Roam predefined routes.
2. **Wait:** Pause at designated waypoints.
3. **Alert:** Triggered by the sound of Erika’s footsteps.
4. **Chase:** Activated when Erika is seen.

---

### Animations & Interactions

**"Controllers and colliders: the engine behind dynamic in-game interactions."**

We use two types of colliders:

* **Trigger Colliders:** For events like checkpoints and hiding spots.
* **Physical Colliders:** For structural elements such as castle walls.

---

### UX: Menu, Audio, Camera & Save System

A polished experience with immersive audio, optimized lighting, dynamic camera, and integrated save menus.

* **Audio:** Ambient sound effects, music tracks, and dialogue cues with reverberation zones for realism.
* **Menu System:** In-scene save, pause, and restart options.
* **Camera:** Free mouse-controlled view or top-down stealth perspective.
* **Lighting:** Hybrid lighting with directional moonlight and static (baked) torch lights for performance.  Note: Lightmaps must be baked locally as they occupy too much space to include in the Git repository.
* **Play Scene Path**: *Assets/Models/DungeonModularPack/Scenes/DemoScene*
---

**Enjoy diving into the shadows and uncovering the light!**
