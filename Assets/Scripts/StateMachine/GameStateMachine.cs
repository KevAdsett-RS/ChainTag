using System;
using System.Collections.Generic;
using System.Linq;
using ParrelSync;
using StateMachine.GameStates;
using Unity.VisualScripting;
using UnityEngine;

namespace StateMachine
{
    public class GameStateMachine : MonoBehaviour
    {
        public static GameStateMachine Instance { get; private set; }
        public NakamaClient NakamaClient { get; private set; }
        public string UniqueDeviceId { get; private set; }
        
        private Dictionary<string, BaseGameState> _states = new ();

        [SerializeField] public GameStateMachineDefinition Definition;
        private BaseGameState _currentState;

        private readonly Dictionary<string, object> _statePackets = new();
        

        private void Awake()
        {
            Instance = this;
            InitialiseNakama();
            DontDestroyOnLoad(this);
            if (Definition.States.Count == 0)
            {
                return;
            }

            foreach (var definition in Definition.States)
            {
                var stateClassName = definition.StateClassName;
                if (string.IsNullOrEmpty(stateClassName))
                {
                    return;
                }

                var stateType = Type.GetType($"StateMachine.GameStates.{stateClassName}");
                
                if (stateType != null && typeof(BaseGameState).IsAssignableFrom(stateType))
                {
                    var stateInstance = (BaseGameState)Activator.CreateInstance(stateType);
                    stateInstance.SetOwner(this);
                    stateInstance.SetSceneName(definition.SceneName);
                    _states.Add(definition.StateClassName, stateInstance);
                    Debug.Log($"Added {definition.StateClassName} to state dictionary");
                }
                else
                {
                    Debug.LogError($"StateMachine::Awake: Could not find or instantiate state class named '{stateClassName}'. Ensure it's a valid BaseState subclass with a parameterless constructor.");
                }
            }
            ChangeState(Definition.States[0].StateClassName);
        }

        private void OnDestroy()
        {
            _currentState?.Exit();
        }

        public void ChangeState(string stateKey, Dictionary<string, object> statePackets = null)
        {
            var fromString = _currentState == null ? "" : $"from {_currentState.SceneName}";
            if (_currentState == _states[stateKey])
            {
                Debug.LogWarning($"ChangingState {fromString} to {_states[stateKey].SceneName}");
                return;
            }
            Debug.Log($"ChangingState {fromString} to {_states[stateKey].SceneName}");
            
            if (statePackets != null)
            {
                _statePackets.AddRange(statePackets);
            }
            if (_states[stateKey] == null)
            {
                Debug.LogError($"StateMachine::ChangeState: can't change to a null state");
                return;
            }
            _currentState?.Exit();
            _currentState = _states[stateKey];
            _currentState.Enter();
        }

        public T GetStatePacket<T>(string key)
        {
            if (_statePackets.ContainsKey(key) == false)
            {
                throw new Exception($"GameStateMachine::GetStatePacket: State packet with key {key} doesn't exist");
            }
            var value = _statePackets[key];
            _statePackets.Remove(key);
            return (T)value;
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