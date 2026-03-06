# Cross-Object Controller Access

Use UnityMVC extension methods to get controllers directly from Unity objects involved in gameplay interactions.

## Quick rule

- Use `GetController<T>()` when the controller must exist.
- Use `TryGetController<T>(out var controller)` when the target may not have a `GameView` or the controller is optional.

## 1) From GameObject

```csharp
if (targetGameObject.TryGetController<EnemyController>(out var enemy))
{
    enemy.InvokeControllerMethod("ApplyDamage", 10);
}
```

```csharp
var enemy = targetGameObject.GetController<EnemyController>();
```

## 2) From 3D physics callbacks

```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.TryGetController<DoorController>(out var door))
    {
        door.InvokeControllerMethod("Open");
    }
}
```

```csharp
private void OnCollisionEnter(Collision collision)
{
    var breakable = collision.GetController<BreakableController>();
    breakable.InvokeControllerMethod("Break");
}
```

## 3) From 2D physics callbacks

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.TryGetController<CollectibleController>(out var collectible))
    {
        collectible.InvokeControllerMethod("Collect");
    }
}
```

```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.TryGetController<DamageableController>(out var damageable))
    {
        damageable.InvokeControllerMethod("ApplyDamage", 1);
    }
}
```

## 4) Runtime type access

Use runtime type overloads only when generic type is not available at compile time.

```csharp
var controller = other.gameObject.GetController(typeof(IMarkerController));
```

## 5) Usage boundaries

- Use these helpers from controllers or integration scripts.
- Keep view classes declarative.
- Prefer direct typed controller methods over string-based invocation when possible.
