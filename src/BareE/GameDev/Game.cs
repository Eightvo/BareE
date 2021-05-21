namespace BareE.GameDev
{
    public class Game
    {
        public Game(GameSceneBase initialScene, GameState initialState, GameEnvironment env)
        {
            ActiveScene = initialScene;
            State = initialState??new GameState();
            Environment =env??GameEnvironment.Load();
        }
        public GameState State
        {
            get { return ActiveScene.State; }
            set
            {
                ActiveScene.State = value;
            }
        }
        public GameSceneBase ActiveScene { get; set; }
        public GameEnvironment Environment { get; set; }

        
    }
}
