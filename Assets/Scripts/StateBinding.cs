using System;
using PurrNet;

namespace DefaultNamespace
{
    public interface IStateBinding
    {
        void Bind();
        void Unbind();
        void TriggerCurrent();
    }
    public class StateBinding<T> : IStateBinding
    {
        private readonly SyncVar<T> _var;
        private readonly Action<T> _handler;
        
        public StateBinding(SyncVar<T> var, Action<T> handler)
        {
            _var = var;
            _handler = handler;
        }
        
        public void Bind()
        {
            _var.onChanged += _handler;
        }

        public void Unbind()
        {
            _var.onChanged -= _handler;
        }

        public void TriggerCurrent()
        {
            _handler?.Invoke(_var.value);
        }
    }
}