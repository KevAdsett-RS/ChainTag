using System.Collections.Generic;
using StateMachine.GameStates;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(fileName = "StateMachineDefinition", menuName = "StateMachines/StateMachineDefinition", order = 0)]
    public class GameStateMachineDefinition : ScriptableObject
    {
        [SerializeField]
        public List<GameStateDefinition> States;
    }
}