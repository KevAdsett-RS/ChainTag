using System;
using System.Collections.Generic;
using Events;
using Match;
using UnityEngine;

    public abstract class StateBinder : MonoBehaviour
    {
        private readonly List<IStateBinding> _stateBindings = new();

        private bool _isInitialised;
        private bool _alreadyEnabled;

        protected virtual void Awake()
        {
            Debug.Log($"StateBinder::Awake ({name})");
            var gameState = FindAnyObjectByType<MatchState>();
            if (gameState && gameState.IsReady)
            {
                OnGameStateReady();
            }
            else
            {
                GameEvents.OnGameStateReady += OnGameStateReady;
            }
            Debug.Log(gameState ? $"{this} Found game state object" : $"{this} couldn't find game state object.");
        }

        private void OnGameStateReady()
        {
            Debug.Log($"StateBinder::OnGameStateReady ({name}, _isInitialised: {_isInitialised})");
            
            GameEvents.OnGameStateReady -= OnGameStateReady;
            var gameState = FindAnyObjectByType<MatchState>();
            if (gameState)
            {
                Initialise(gameState);
            }
        }

        private void Initialise(MatchState matchState)
        {
            Debug.Log($"StateBinder::Initialise ({name}, _isInitialised: {_isInitialised}, _alreadyEnabled: {_alreadyEnabled})");
            if (_isInitialised)
            {
                return;
            }

            RegisterBindings(matchState, _stateBindings);
            _isInitialised = true;

            // If we're already enabled (likely true for a client), run bindings
            if (_alreadyEnabled)
            {
                BindAll();
            }
        }
        
        private void OnEnable()
        {
            Debug.Log($"StateBinder::OnEnable ({name}, _isInitialised: {_isInitialised})");
            _alreadyEnabled = true;
            
            // If we're already initialised (likely true for the server), run bindings
            if (_isInitialised)
            {
                BindAll();
            }
        }

        private void OnDisable()
        {
            Debug.Log($"StateBinder::OnDisable ({name})");
            foreach (var stateBinding in _stateBindings)
            {
                stateBinding.Unbind();
            }
        }

        private void BindAll()
        {
            foreach (var stateBinding in _stateBindings)
            {
                stateBinding.Bind();
            }
        }

        protected abstract void RegisterBindings(MatchState matchState, List<IStateBinding> stateBindings);
        
        protected virtual void OnDestroy()
        {
            Debug.Log($"StateBinder::OnDestroy ({name})");
            GameEvents.OnGameStateReady -= OnGameStateReady;
        }
    }
    
