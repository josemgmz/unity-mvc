namespace UnityMVC
{
    /// <summary>
    /// Unity lifecycle, physics and UI event hooks that GameView can forward to controllers.
    /// </summary>
    public enum GameMethodEvents
    {
        Start,
        Update,
        LateUpdate,
        FixedUpdate,
        OnDestroy,
        OnEnable,
        OnDisable,
        OnCollisionEnter,
        OnCollisionExit,
        OnCollisionStay,
        OnTriggerEnter,
        OnTriggerExit,
        OnTriggerStay,
        OnMouseDown,
        OnMouseEnter,
        OnMouseExit,
        OnCollisionEnter2D,
        OnCollisionExit2D,
        OnCollisionStay2D,
        OnTriggerEnter2D,
        OnTriggerExit2D,
        OnTriggerStay2D,
        OnBeginDrag,
        OnDrag,
        OnEndDrag,
        OnPointerDown,
        OnPointerExit,
        OnPointerUp,
        OnPointerMove,
        OnPointerEnter,
        OnAnimatorEvent,
        OnBecameInvisible,
        OnParticleSystemStopped,
        OnDrawGizmos,
        OnDrawGizmosSelected,
        OnSelect,
        OnDeselect
    }
}
