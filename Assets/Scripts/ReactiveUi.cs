using System;
using System.Collections.Generic;
using UnityEngine;

    public abstract class ReactiveUi : MonoBehaviour
    {
        private readonly List<IStateBinding> _stateBindings = new();

        private void Awake()
        {
            var gameState = FindAnyObjectByType<GameState>();
            RegisterBindings(gameState, _stateBindings);
        }

        private void OnEnable()
        {
            foreach (var stateBinding in _stateBindings)
            {
                stateBinding.Bind();
            }
        }

        private void OnDisable()
        {
            foreach (var stateBinding in _stateBindings)
            {
                stateBinding.Unbind();
            }
        }

        protected abstract void RegisterBindings(GameState gameState, List<IStateBinding> stateBindings);
    }
    
