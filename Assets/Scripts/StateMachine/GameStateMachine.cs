using System;

namespace StateMachine
{
    public class GameStateMachine : StateMachine
    {
        public static GameStateMachine Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            ChangeState(new MainMenuState(this));
        }
    }
}