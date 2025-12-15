using UnityEngine;

namespace StateMachine
{
    public class GameRunningState : GameState
    {
        public GameRunningState(StateMachine stateMachine): base(stateMachine)
        {
            setSceneName("SampleScene");
        }
    }
}