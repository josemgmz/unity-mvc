using System;
using UnityEngine;

namespace UnityMVC._1_Example_Base_MVC.Circle
{
    [Serializable]
    public class CircleModel : GameModel
    {
        #region Variables

        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float pulseAmplitude = 0.15f;
        [SerializeField] private Color baseColor = Color.cyan;
        [NonSerialized] private Vector3 _initialScale = Vector3.one;

        #endregion

        #region Properties

        public float RotationSpeed => rotationSpeed;
        public float PulseSpeed => pulseSpeed;
        public float PulseAmplitude => pulseAmplitude;
        public Color BaseColor => baseColor;
        public Vector3 InitialScale
        {
            get => _initialScale;
            set => _initialScale = value;
        }

        #endregion
    }
}

