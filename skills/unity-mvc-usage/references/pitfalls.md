# Pitfalls

## Architecture mistakes

- Do not place gameplay logic in `GameView`/`GameViewUI`.
- Do not treat models as service classes; keep them as state containers.
- Do not modify `Runtime/` to add feature behavior unless explicitly requested.

## Wiring mistakes

- Do declare model fields with `[GameFieldAttributes.ModelFieldAttribute]`.
- Do declare controller fields with `[GameFieldAttributes.ControllerFieldAttribute]`.
- Do use `GameViewUI` when pointer/drag/select UI callbacks are required.
- Do keep Unity callback signatures exact; UnityMVC binds methods by method name/signature.

## Access mistakes

- Do mutate state through injected `Model` inside `GameController<TView, TModel>`.
- Do remember `GameView.GetModel<T>()` returns a clone; avoid mutating that clone expecting live state updates.
- Do use `TryGetController` / `TryGetModel` for optional cross-object lookups.

## Bus mistakes

- Do store delegates in fields when adding listeners, then remove the same delegate instance.
- Do not rely on provider fan-out in data bus queries; only the first provider result is returned.
- Do keep argument count and types aligned for `GetData<T>(...)`.
- Do remove event listeners in `OnDestroy` unless owner-based lifetime handling is guaranteed by the project.

## Controller order and execution mode

- Do assume default controller initialization order is reverse field declaration order.
- Do add `[GameFieldAttributes.ControllerReverseOrderAttribute]` to preserve declared order when needed.
- Do set controller execution mode explicitly with:
  - `[GameFieldAttributes.ControllerExecutionAttribute(GameFieldAttributes.ControllerExecutionMode.PlayOnly)]`
  - `[GameFieldAttributes.ControllerEditorOnlyAttribute]`
  - `[GameFieldAttributes.ControllerExecuteAlwaysAttribute]`

