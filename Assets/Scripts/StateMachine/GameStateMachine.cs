using System;
using System.Collections.Generic;
using Events;
using ParrelSync;
using StateMachine.GameStates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace StateMachine
{
    public class GameStateMachine : MonoBehaviour
    {
        public NakamaClient NakamaClient { get; private set; }
        public string UniqueDeviceId { get; private set; }
        
        [SerializeField] public string currentStateKey;
        
        [SerializeField] public GameStateMachineDefinition Definition;
        
        private BaseGameState _currentState;
        private readonly Dictionary<string, object> _statePackets = new();


        private void Awake()
        {
            Debug.Log($"GameStateMachine::Awake:");
            InitialiseNakama();

            GameEvents.OnMatchFinished += OnMatchFinished;
            
            if (Definition.States.Count == 0)
            {
                return;
            }

            ChangeState(Definition.States[0].StateClassName);
        }

        private void OnDestroy()
        {
            GameEvents.OnMatchFinished -= OnMatchFinished;
            
            _currentState?.Exit();
        }

        private void OnMatchFinished()
        {
            ChangeState("GameEndState");
        }

        public void ChangeState(string stateKey, Dictionary<string, object> statePackets = null)
        {
            if (_currentState?.GetType().Name == stateKey)
            {
                return;
            }
            var fromString = _currentState == null ? "" : $" from {_currentState.SceneName}";

            var stateDefinition = Definition.States.Find((definition => definition.StateClassName == stateKey));
            if (!stateDefinition)
            {
                Debug.LogError($"GameStateMachine::ChangeState: couldn't find definition for state {stateKey}");
                return;
            }
            Debug.Log($"GameStateMachine::ChangeState: ChangingState{fromString} to {stateDefinition.SceneName}");

            _currentState?.Exit();

            if (statePackets != null)
            {
                foreach (var item in statePackets)
                {
                    if (_statePackets.TryAdd(item.Key, item.Value) == false)
                    {
                        _statePackets[item.Key] = item.Value;
                    }
                }
            }

            var stateType = Type.GetType($"StateMachine.GameStates.{stateKey}");
                
            if (stateType != null && typeof(BaseGameState).IsAssignableFrom(stateType))
            {
                var stateInstance = (BaseGameState)Activator.CreateInstance(stateType);
                stateInstance.SetOwner(this);
                stateInstance.SetSceneName(stateDefinition.SceneName);
                _currentState = stateInstance;
                currentStateKey = stateDefinition.StateClassName;
                GameEvents.OnGameStateChanged?.Invoke(currentStateKey);
            }
            else
            {
                Debug.LogError($"GameStateMachine::Awake: Could not find or instantiate state class named '{stateKey}'. Ensure it's a valid BaseState subclass with a parameterless constructor.");
                return;
            }

            _currentState.Enter();
        }

        public bool TryGetStatePacket<T>(string key, out T value)
        {
            if (_statePackets.TryGetValue(key, out var packet) == false)
            {
                value = default;
                return false;
            }
            value = (T)packet;
            Debug.Log($"GameStateMachine::GetStatePacket consumed packet with key {key}");
            _statePackets.Remove(key);
            return true;
        }

        private void Update()
        {
            _currentState?.Update();
        }

        private void InitialiseNakama()
        {
            string suffix = "";
#if UNITY_EDITOR
            if (ClonesManager.IsClone())
            {
                var pathTokens = ClonesManager.GetCurrentProjectPath().Split('_');
                suffix = "_" + pathTokens[^1];
            }
#endif
            UniqueDeviceId = SystemInfo.deviceUniqueIdentifier + suffix;
            NakamaClient = FindFirstObjectByType<NakamaClient>();
        }
    }
}