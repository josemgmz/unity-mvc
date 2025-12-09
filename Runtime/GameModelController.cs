namespace UnityMVC
{
    /// <summary>
    /// Controller variant that owns a typed model alongside the view.
    /// </summary>
    public class GameController<TView, TModel> : GameController<TView> where TView : GameView where TModel : GameModel
    {
        #region Variables

        private TModel _model;

        #endregion

        #region Properties

        /// <summary>
        /// Strongly-typed model assigned to this controller.
        /// </summary>
        protected TModel Model => _model;

        #endregion

        #region Methods

        /// <summary>
        /// Assigns the backing model instance (called internally by GameView).
        /// </summary>
        public void SetInternalModel(TModel model)
        {
            _model = model;
        }

        #endregion
    }
}
