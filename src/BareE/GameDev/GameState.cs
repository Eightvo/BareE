
using BareE.DataStructures;
using BareE.Messages;

namespace BareE.GameDev
{

    public class GameState
    {
        public GameClock Clock;
        public InputHandler Input;
        public MessageQueue Messages;
        public EntityComponentContext ECC;
        public GameState()
        {
            Clock = new GameClock();
            Input = new InputHandler();
            Messages = new MessageQueue();
            ECC = new EntityComponentContext();
        }
    }
}
