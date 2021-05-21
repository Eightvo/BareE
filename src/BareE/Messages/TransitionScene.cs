using BareE.GameDev;

namespace BareE.Messages
{
    [Message("TransitionScene")]
    public struct TransitionScene
    {
        public bool Preloaded;
        public GameSceneBase Scene;
        public GameState State;

        public TransitionScene(GameSceneBase scene, GameState initialState = null)
        {
            Preloaded = false;
            Scene = scene;
            State = initialState;
        }
    }
}