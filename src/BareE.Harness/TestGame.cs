using BareE.GameDev;

namespace BareE.Harness
{
    public class TestGame : GameDev.Game
    {
        public TestGame(GameState initialState, GameEnvironment env) : base(new SceneSelectorScene(), initialState, env)
        {
            InputHandler.LoadFromConfig("BareE.Harness.Assets.Def.default.controls");
        }
    }
}