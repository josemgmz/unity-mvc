# UnityMVC (UPM package)

Lightweight MVC backbone for Unity: auto-wired controllers/views/models, lifecycle binding, and utility buses. Git-installable with importable samples.

## Install via Package Manager
1. Open **Window → Package Manager**.
2. Click the "+" → **Add package from git URL...**.
3. Use this repo URL (e.g. `https://github.com/tu-org/unity-mvc.git`).
4. Unity (2023.1+) will download the package and list it under "In Project".

## Layout
- `Runtime/`: core framework (GameView, controllers/models base, event/data buses, addressable loader).
- `Runtime/VContainer/`: optional DI integration (compiled when VContainer is present and `UNITYMVC_VCONTAINER` is defined).
- `Editor/`: inspector utilities and reload handler for GameView.
- `Samples~/`: importable samples.

## Samples
- **UnityMVC Example** → imports to `Assets/Samples/UnityMVC/Example` with multiple scenario notes.

## Quick start (Bird)
```csharp
// Model
[Serializable]
public class BirdModel : GameModel {
    public bool IsFlying;
    public bool IsLookingLeft;
    public Animator Animator;
    public SpriteRenderer SpriteRenderer;
    public string PlayerTag = "Player";
}

// Controller
public class BirdController : GameController<BirdView, BirdModel> {
    void Awake() {
        Model.Animator = GetComponent<Animator>();
        Model.SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag(Model.PlayerTag) || Model.IsFlying) return;
        StartCoroutine(FlyRoutine());
    }

    private IEnumerator FlyRoutine() {
        Model.IsFlying = true;
        if (Model.Animator) Model.Animator.SetBool("Flying", true);
        yield return new WaitForSeconds(2f);
        if (Model.Animator) Model.Animator.SetBool("Flying", false);
        Model.IsFlying = false;
        Destroy(gameObject);
    }
}

// View (MonoBehaviour component attached to the GameObject)
public class BirdView : GameView {
    [SerializeField, GameFieldAttributes.ModelField] private BirdModel model;
    [GameFieldAttributes.ControllerField] private BirdController controller;
}
```

## Notes
- Addressables: declared dependency (`com.unity.addressables`). Ensure the package is present in your project.
- Optional VContainer: install `jp.hadashikick.vcontainer` (OpenUPM or Git `https://github.com/hadashiA/VContainer.git#v1.16.5`). This enables `UNITYMVC_VCONTAINER` and compiles `UnityMVC.VContainer`.
- If you do not install VContainer, the base package still compiles, but controller DI and EventBus coroutine helpers will not run (they rely on `GameDependency`). To force the integration manually, add `UNITYMVC_VCONTAINER` in **Project Settings → Player → Scripting Define Symbols** and ensure VContainer is present.
