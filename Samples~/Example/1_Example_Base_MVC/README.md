## Base MVC Example (UnityMVC)

Brief guide to how MVC is structured in this example (`Circle`).

- **Model (data/state):** Serializable class exposing private fields through properties. Holds state only; no logic. Example: `CircleModel.cs`.
- **View (Unity binding):** Script added to the GameObject in the scene. Declares references to Model and Controller via UnityMVC attributes for injection, but contains no logic. Example: `CircleView.cs` (added to the Circle GameObject).
- **Controller (logic):** Inherits from `GameController<View, Model>`. Reads/writes Model state, works with Unity components (`Renderer`, `Transform`, etc.), and orchestrates behavior. Example: `CircleController.cs`.

### `Circle` flow
1. In `Awake`, the Controller stores the initial scale in the Model and applies the Model color to the `SpriteRenderer`/`Renderer`.
2. In `Update`, the Controller rotates the object, adjusts its scale with a pulse using Model values (`RotationSpeed`, `PulseSpeed`, `PulseAmplitude`, `BaseColor`, `InitialScale`), and moves it horizontally using a second Model fetched via `GetModel<CircleMovementModel>()`.
3. The View only links the Model(s) and Controller to the GameObject; it exposes no methods.

### What goes in each file
- `CircleModel.cs`: serialized private fields and public properties to expose them to the Controller. No methods.
- `CircleMovementModel.cs`: second Model with movement settings (speed, distance, start position). Accessed from the Controller via `GetModel<CircleMovementModel>()`.
- `CircleView.cs`: script attached to the GameObject for references. Holds both Models and the Controller. No logic or methods.
- `CircleController.cs`: all behavior and Unity access (`Transform`, `Renderer`, etc.). Uses Model properties; does not depend on methods in the View.

### How to use/create an object with this pattern
1. Create a GameObject in the scene.
2. Assign the View script (e.g., `CircleView`) to the GameObject. This is the script added in the Inspector.
3. Assign the Controller script (e.g., `CircleController`) to the same GameObject.
4. In the Inspector, set the serialized fields of the Models (rotation speed, pulse speed/amplitude, base color, movement speed, movement distance). The Models are serialized and referenced from the View.
5. Play the scene: the Controller reads the Model and updates the GameObject.

### Notes and conventions
- Comments in English.
- Naming: classes and properties in PascalCase; private fields in camelCase; constants in UPPER_SNAKE_CASE.
- Unity methods (`Awake`, `Update`, etc.) go in `#region Lifecycle`; custom Controller logic goes in `#region Methods`.
