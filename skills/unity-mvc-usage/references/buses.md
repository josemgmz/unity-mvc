# Buses

## Data bus (`GameDataBusImpl`)

Use `GameDataBusImpl.Instance` as a query bus keyed by return type.

```csharp
private readonly GameDataBusImpl dataBus = GameDataBusImpl.Instance;

private Func<string, PlayerStatsModel> statsById;

private void Awake()
{
    statsById = id => new PlayerStatsModel { Id = id, Score = 10 };
    dataBus.AddListener<PlayerStatsModel, string>(statsById);
}

private void UseData()
{
    var stats = dataBus.GetData<PlayerStatsModel>("player-1");
}

private void OnDestroy()
{
    dataBus.RemoveListener<PlayerStatsModel, string>(statsById);
}
```

Behavior notes:

- Return only one value per query call: the first registered listener is used.
- Match argument count exactly in `GetData<T>(...)`; mismatch throws.
- Register deterministic listener order when several providers share return type.
- Expect clone behavior for `GameModel` results in `GetData<T>(params object[] args)`.

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

