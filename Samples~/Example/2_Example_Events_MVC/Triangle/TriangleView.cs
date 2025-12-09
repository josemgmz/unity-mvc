using UnityEngine;

namespace UnityMVC._2_Example_Events_MVC.Triangle
{
    public class TriangleView : GameView
    {
        #region Variables

        [GameFieldAttributes.ModelFieldAttribute, SerializeField] private TriangleModel triangleModel;
        [GameFieldAttributes.ControllerFieldAttribute] private TriangleController triangleController;

        #endregion
    }
}

