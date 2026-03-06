# Event Bus

## Event bus (`IGameEventBus` / `GameEventBusImpl`)

Use an event bus instance provided by the project (usually DI). Do not call `new GameEventBusImpl()` directly.

```csharp
using VContainer;

public sealed class EnemyController : GameController<EnemyView, EnemyModel>
{
    private IGameEventBus eventBus;
    private Action<PlayerDamagedEvent> onPlayerDamaged;

    [Inject]
    private void Construct(IGameEventBus injectedEventBus)
    {
        eventBus = injectedEventBus;
    }

    private void Awake()
    {
        onPlayerDamaged = HandlePlayerDamaged;
        eventBus.AddListener<PlayerDamagedEvent>(onPlayerDamaged);
    }

    private void OnDestroy()
    {
        eventBus.RemoveListener<PlayerDamagedEvent>(onPlayerDamaged);
    }

    private void HandlePlayerDamaged(PlayerDamagedEvent evt)
    {
        if (evt.Amount > 0) { /* react */ }
    }

    private void DealDamage()
    {
        eventBus.RaiseEvent(new PlayerDamagedEvent { Amount = 1 });
    }
}

public sealed class PlayerDamagedEvent
{
    public int Amount;
}
```

Behavior notes:

- Use `RaiseEvent<T>(payload)` with a reference type payload.
- Remove listeners in `OnDestroy` if no owner token is supplied.
- Use coroutine listeners (`Func<IEnumerator>` variants) only when the project enables `UNITYMVC_VCONTAINER`.
