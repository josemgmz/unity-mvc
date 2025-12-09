using System;
using UnityEngine;

namespace UnityMVC._2_Example_Events_MVC.Triangle
{
    [Serializable]
    public class TriangleModel : GameModel
    {
        #region Variables

        [SerializeField] private Color baseColor = Color.green;
        [SerializeField] private Color contactColor = Color.yellow;
        [SerializeField] private Color exitColor = Color.cyan;
        [SerializeField] private float jumpForce = 4f;
        [SerializeField] private string targetName = "Ground";
        [NonSerialized] private bool isContact;

        #endregion

        #region Properties

        public Color BaseColor => baseColor;
        public Color ContactColor => contactColor;
        public Color ExitColor => exitColor;
        public float JumpForce => jumpForce;
        public string TargetName => targetName;
        public bool IsContact
        {
            get => isContact;
            set => isContact = value;
        }

        #endregion
    }
}

