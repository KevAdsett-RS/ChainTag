using System;
using Match;
using PurrNet;
using StateMachine.GameStates;

namespace Events
{
    public struct MatchEvents
    {
        public static Action OnMatchStateReady;
        public static Action<PlayerID, PlayerTeam> OnPlayerChangedTeam;
        public static Action OnLeaveMatchPressed;
        public static Action OnExitingMatchEndState;
        public static Action MatchEndUiDestroyed;
    }
}