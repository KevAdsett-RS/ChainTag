using System;
using System.Collections.Generic;
using Nakama;
#if UNITY_EDITOR
using ParrelSync;
#endif
using StateMachine.GameStates;
using UnityEngine;

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
            
            if (Definition.States.Count == 0)
            {
                return;
            }

            ChangeState(Definition.States[0].StateClassName);
        }

        private void OnDestroy()
        {
            _currentState?.Exit();
        }

        public void Next(Dictionary<string, object> statePackets = null)
        {
            var index = Definition.States.FindIndex(definition => definition.StateClassName == currentStateKey);
            if (index == Definition.States.Count - 1)
            {
                return;
            }
            ChangeState(Definition.States[index + 1].StateClassName, statePackets);
        }

        public async void ChangeState(string stateKey, Dictionary<string, object> statePackets = null)
        {
            if (_currentState?.GetType().Name == stateKey)
            {
                return;
            }
            var fromString = _currentState == null ? "" : $" from {_currentState.SceneName}";

            var stateDefinition = Definition.States.Find((definition => definition.StateClassName == stateKey));
            if (!stateDefinition)
            {
                throw new Exception($"GameStateMachine::ChangeState: couldn't find definition for state {stateKey}");
            }
            Debug.Log($"GameStateMachine::ChangeState: ChangingState{fromString} to {stateDefinition.SceneName}");

            if (_currentState != null)
            {
                await _currentState.Exit();
            }

            if (statePackets != null)
            {
                foreach (var item in statePackets)
                {
                    _statePackets[item.Key] = item.Value;
                }
            }

            var stateType = Type.GetType($"StateMachine.GameStates.{stateKey}");
            var stateInstance = (BaseGameState)Activator.CreateInstance(stateType);
            stateInstance.SetOwner(this);
            stateInstance.SetSceneName(stateDefinition.SceneName);
            
            _currentState = stateInstance;
            currentStateKey = stateDefinition.StateClassName;

            await _currentState.Enter();
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