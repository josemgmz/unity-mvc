using UnityEngine;

namespace UnityMVC._1_Example_Base_MVC.Circle
{
    public class CircleController : GameController<CircleView, CircleModel>
    {
        #region Lifecycle

        private void Awake()
        {
            Model.InitialScale = transform.localScale;
            var movementModel = GetModel<CircleMovementModel>();
            if (movementModel != null)
            {
                movementModel.StartPosition = transform.position;
            }
            ApplyModelColor();
        }

        private void Update()
        {
            RotateCircle();
            PulseScale();
            MoveCircle();
        }

        #endregion

        #region Methods

        private void ApplyModelColor()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Model.BaseColor;
                return;
            }

            var renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = Model.BaseColor;
            }
        }

        private void RotateCircle()
        {
            var angle = Model.RotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, angle);
        }

        private void PulseScale()
        {
            var pulse = 1f + Mathf.Sin(Time.time * Model.PulseSpeed) * Model.PulseAmplitude;
            transform.localScale = Model.InitialScale * pulse;
        }

        private void MoveCircle()
        {
            var movementModel = GetModel<CircleMovementModel>();
            if (movementModel == null)
            {
                return;
            }

            var offset = Mathf.PingPong(Time.time * movementModel.MoveSpeed, movementModel.MoveDistance) - (movementModel.MoveDistance * 0.5f);
            var position = movementModel.StartPosition;
            position.x += offset;
            transform.position = position;
        }

        #endregion
    }
}

