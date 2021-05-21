using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
