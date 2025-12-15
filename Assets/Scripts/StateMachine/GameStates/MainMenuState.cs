using UnityEngine;

namespace StateMachine
{
    public class MainMenuState : GameState
    {
        public MainMenuState(StateMachine stateMachine): base(stateMachine)
        {
            setSceneName("MainMenu");
        }

        public void OnPlay(string username)
        {
            Debug.Log($"Let's go {username}!");
            Owner.ChangeState(new GameRunningState(Owner));
        }
        
        
    }
}