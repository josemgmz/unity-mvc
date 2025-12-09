using UnityEngine;

namespace UnityMVC._2_Example_Events_MVC.Triangle
{
    public class TriangleController : GameController<TriangleView, TriangleModel>
    {
        #region Variables

        private Rigidbody2D rb;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            ApplyColor(Model.BaseColor);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsTarget(other.gameObject))
            {
                return;
            }

            Model.IsContact = true;
            ApplyColor(Model.ContactColor);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsTarget(other.gameObject))
            {
                return;
            }

            Model.IsContact = false;
            ApplyColor(Model.ExitColor);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsTarget(collision.gameObject))
            {
                return;
            }

            Model.IsContact = true;
            ApplyColor(Model.ContactColor);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!IsTarget(collision.gameObject))
            {
                return;
            }

            Model.IsContact = false;
            ApplyColor(Model.ExitColor);
        }

        #endregion

        #region Methods

        private bool IsTarget(GameObject other) => other.name == Model.TargetName;

        private void ApplyColor(Color color)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
                return;
            }

            var renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = color;
            }
        }

        private void Jump()
        {
            if (rb == null)
            {
                return;
            }

            rb.AddForce(Vector2.up * Model.JumpForce, ForceMode2D.Impulse);
        }

        #endregion
    }
}

