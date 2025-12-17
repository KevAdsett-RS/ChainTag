using System;
using UnityEngine;

namespace StateMachine
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameStateDefinition", menuName = "StateMachines/GameStateDefinition", order = 0)]
    public class GameStateDefinition : ScriptableObject
    {
        [SerializeField] public string SceneName;
        public string StateClassName => SceneName + "State";
    }
}