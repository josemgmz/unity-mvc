## Events + MVC Example (UnityMVC)

Controllers receive the same Unity events you already know—UnityMVC forwards them so you handle them inside the controller. This sample focuses on 2D triggers/collisions, but the framework supports many more events (see `Library/PackageCache/com.unitymvc.core@d0829cafaf8f/Runtime/GameMethodEvents.cs` for the full list, including lifecycle, mouse, pointer/drag, animator, gizmos, etc.).

### Supported events (GameMethodEvents)
- Lifecycle/physics: `Start`, `Update`, `LateUpdate`, `FixedUpdate`, `OnDestroy`, `OnEnable`, `OnDisable`.
- 3D collisions/triggers: `OnCollisionEnter`, `OnCollisionExit`, `OnCollisionStay`, `OnTriggerEnter`, `OnTriggerExit`, `OnTriggerStay`.
- 2D collisions/triggers: `OnCollisionEnter2D`, `OnCollisionExit2D`, `OnCollisionStay2D`, `OnTriggerEnter2D`, `OnTriggerExit2D`, `OnTriggerStay2D`.
- Mouse: `OnMouseDown`, `OnMouseEnter`, `OnMouseExit`.
- Pointer/drag (EventSystem): `OnBeginDrag`, `OnDrag`, `OnEndDrag`, `OnPointerDown`, `OnPointerExit`, `OnPointerUp`, `OnPointerMove`, `OnPointerEnter`, `OnSelect`, `OnDeselect`.
- Animator/gizmos/particles/visibility: `OnAnimatorEvent`, `OnBecameInvisible`, `OnParticleSystemStopped`, `OnDrawGizmos`, `OnDrawGizmosSelected`.

### Files (in `Triangle/`)
- `TriangleModel.cs`: serialized colors (base, contact, exit) and the target name to react to. Holds a runtime flag `IsContact` and a `JumpForce`.
- `TriangleView.cs`: attaches to the Triangle GameObject and references the Model and Controller. No logic.
- `TriangleController.cs`: handles Unity events (trigger/collision), checks the target name, updates `IsContact`, changes color via `SpriteRenderer`/`Renderer`, and listens for `Space` to jump.
- `Scene.unity`: place the objects described below.

### Scene setup (2D)
1. Create a **Plane** GameObject:
   - Add `BoxCollider2D`.
   - Name it `Ground` (or match `targetName` in `TriangleModel`).
   - Keep it static (no `Rigidbody2D`) or set a Static `Rigidbody2D`.
2. Create a **Triangle** GameObject:
   - Add a `SpriteRenderer` (any triangle sprite or placeholder).
   - Add `PolygonCollider2D` (or suitable 2D collider).
   - Add a `Rigidbody2D` (Dynamic).
   - Decide the event mode:
     - **Trigger mode:** check `Is Trigger` on the collider. `OnTriggerEnter2D/Exit` will fire.
     - **Collision mode:** uncheck `Is Trigger`. `OnCollisionEnter2D/Exit` will fire.
   - Add scripts: `TriangleView` and `TriangleController`.
   - In the `TriangleView` inspector, assign the `TriangleModel` reference.
   - Configure Model fields in the inspector: base/contact/exit colors, target name (`Ground` by default), and jump force.

### Runtime behavior
- On enter (trigger or collision) with a GameObject whose name matches `targetName`, the Controller sets `IsContact = true` and switches to the contact color.
- On exit, it sets `IsContact = false` and switches to the exit color.
- Press `Space` to apply an upward impulse using the `JumpForce` from the Model (requires `Rigidbody2D` on the Triangle).

### Notes
- All Unity message methods live in the Controller; the Model only stores state.
- The View is just the bridge added to the GameObject to wire Model and Controller.
