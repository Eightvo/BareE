namespace BareE.GameDev
{
    public class Game
    {
        public Game(GameState initialState, GameEnvironment env)
        {
            State = initialState??new GameState();
            Environment =env??GameEnvironment.Load();
        }
        public GameState State { get; set; }
        public GameSceneBase ActiveScene { get; set; }
        public GameEnvironment Environment { get; set; }

        
    }
}
