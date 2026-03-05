# MVC Quickstart

## 1) Create model

```csharp
using System;
using UnityEngine;

[Serializable]
public class PlayerModel : GameModel
{
    [SerializeField] private float moveSpeed = 3f;
    [NonSerialized] private bool isGrounded;

    public float MoveSpeed => moveSpeed;
    public bool IsGrounded
    {
        get => isGrounded;
        set => isGrounded = value;
    }
}
```

## 2) Create view

Use `GameView` for world objects and `GameViewUI` for EventSystem UI callbacks.

```csharp
using UnityEngine;

public class PlayerView : GameView
{
    [SerializeField, GameFieldAttributes.ModelFieldAttribute]
    private PlayerModel playerModel;

    [GameFieldAttributes.ControllerFieldAttribute]
    private PlayerController playerController;
}
```

## 3) Create controller

```csharp
using UnityEngine;

public class PlayerController : GameController<PlayerView, PlayerModel>
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var velocity = rb.velocity;
        velocity.x = horizontal * Model.MoveSpeed;
        rb.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Model.IsGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        Model.IsGrounded = false;
    }
}
```

## 4) Add extra models when needed

Declare extra model fields in the same view and read them inside controller:

```csharp
var movementModel = GetModel<PlayerMovementModel>();
```

## 5) Access rules

- Mutate current typed model through `Model`.
- Read sibling models with `GetModel<T>()` from controller.
- Read from view with `GetModel<T>()` only for snapshot/inspection (view returns a clone).

