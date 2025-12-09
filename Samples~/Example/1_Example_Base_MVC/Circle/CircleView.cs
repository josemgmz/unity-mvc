using UnityEngine;

namespace UnityMVC._1_Example_Base_MVC.Circle
{
    public class CircleView : GameView
    {
        #region Variables

        [GameFieldAttributes.ModelFieldAttribute, SerializeField] private CircleModel circleModel;
        [GameFieldAttributes.ModelFieldAttribute, SerializeField] private CircleMovementModel circleMovementModel;
        [GameFieldAttributes.ControllerFieldAttribute] private CircleController _circleController;

        #endregion
    }
}

