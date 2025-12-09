using System;
using UnityEngine;

namespace UnityMVC._1_Example_Base_MVC.Circle
{
    [Serializable]
    public class CircleMovementModel : GameModel
    {
        #region Variables

        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveDistance = 3f;
        [NonSerialized] private Vector3 startPosition = Vector3.zero;

        #endregion

        #region Properties

        public float MoveSpeed => moveSpeed;
        public float MoveDistance => moveDistance;
        public Vector3 StartPosition
        {
            get => startPosition;
            set => startPosition = value;
        }

        #endregion
    }
}

