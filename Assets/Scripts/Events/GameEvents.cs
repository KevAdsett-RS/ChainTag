using System;
using Match;
using PurrNet;
using StateMachine.GameStates;

namespace Events
{
    public struct GameEvents
    {
        public static Action<string> OnGameStateChanged;
        public static Action OnGameStateReady;
        public static Action<PlayerID, PlayerTeam> OnPlayerChangedTeam;
        public static Action OnStartGame;
        public static Action OnLeaveGamePressed;
        public static Action OnMatchFinished;
    }
}