using UnityEngine;

namespace StateMachine
{
    public class StateMachine : MonoBehaviour
    {
        public BaseState CurrentState { get; private set; }

        public void ChangeState(BaseState newState)
        {
            if (newState == null)
            {
                Debug.LogError($"StateMachine::ChangeState: can't change to a null state");
                return;
            }
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        private void Update()
        {
            CurrentState?.Update();
        }
    }
}