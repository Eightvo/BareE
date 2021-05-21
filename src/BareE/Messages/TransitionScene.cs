using BareE.GameDev;

namespace BareE.Messages
{
    /// <summary>
    /// Triggers the transition of a Scene.
    /// </summary>
    [Message("TransitionScene")]
    public struct TransitionScene
    {
        /// <summary>
        /// If the supplied Gamescene does not require the methods Load and Initialize called for any reason.
        /// </summary>
        public bool Preloaded;
        /// <summary>
        /// The scene to transition to.
        /// </summary>
        public GameSceneBase Scene;
        /// <summary>
        /// The state supplied. 
        /// Information can be passed from one scene to the next via the initial GameState or via The Environment Share.
        /// </summary>
        public GameState State;

        public TransitionScene(GameSceneBase scene, GameState initialState = null)
        {
            Preloaded = false;
            Scene = scene;
            State = initialState;
        }
    }
}