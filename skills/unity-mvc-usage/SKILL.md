---
name: unity-mvc-usage
description: Create or update user-facing UnityMVC code with GameModel, GameView/GameViewUI, GameController, and the framework buses. Use when implementing gameplay/UI features in projects that use com.unitymvc.core and the request involves wiring MVC fields, handling Unity events in controllers, querying/publishing cross-object data, or invoking controller methods from views.
---

# UnityMVC Usage

## Goal

Implement features with UnityMVC without editing framework internals. Keep logic in controllers, state in models, and wiring in views.

## Workflow

1. Identify whether the feature is world-space (`GameView`) or UI (`GameViewUI`).
2. Create or update a `GameModel` for serialized state and runtime flags.
3. Declare model/controller fields in the view with UnityMVC attributes.
4. Implement behavior in `GameController<TView, TModel>` lifecycle/event methods.
5. Add bus usage only for cross-object communication:
- Use `GameDataBusImpl.Instance` for query/pull style data.
- Use an injected/provided `IGameEventBus` for publish/subscribe events.
6. Validate scene wiring and event signatures against the references.

## References

- For scaffolding and standard file layout, read [references/mvc-quickstart.md](references/mvc-quickstart.md).
- For `GameDataBusImpl` and `IGameEventBus` usage patterns, read [references/buses.md](references/buses.md).
- For common mistakes and behavior traps, read [references/pitfalls.md](references/pitfalls.md).

## Rules

1. Keep view classes declarative: fields plus attributes only.
2. Declare only model and controller fields in views. Do not add service fields, gameplay state, or behavior methods.
3. Put feature logic in controllers, not in views or models.
4. Use model properties/fields as source of truth for controller state.
5. Match Unity callback signatures exactly (`OnTriggerEnter2D(Collider2D other)`, etc.).
6. Prefer typed access (`Model`, `GetModel<T>()`, `GetController<T>()`) over reflection-based calls.
7. Avoid modifying `Runtime/` unless explicitly requested.

## Output Contract

When generating code, produce:

1. One model file per bounded state group.
2. One view file with `[GameFieldAttributes.ModelFieldAttribute]` and `[GameFieldAttributes.ControllerFieldAttribute]`.
3. One controller file with lifecycle/event methods and no Unity logic in the view.
4. Optional bus registration/unregistration snippets when cross-object communication is required.

## Scope Boundary

Use UnityMVC as a consumer:

1. Implement feature code under the consuming project scripts.
2. Treat package internals (`Runtime/`) as read-only reference for API behavior.
