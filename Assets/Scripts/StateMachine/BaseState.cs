namespace StateMachine
{
    public abstract class BaseState
    {
        protected StateMachine Owner { get; private set; }

        public BaseState(StateMachine owner)
        {
            Owner = owner;
        }
        public void Enter()
        {
            OnEnter();
        }

        public void Update()
        {
            OnUpdate();
        }

        public void Exit()
        {
            OnExit();
        }

        protected abstract void OnEnter();
        protected abstract void OnUpdate();
        protected abstract void OnExit();
    }
}