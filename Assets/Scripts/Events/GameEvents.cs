using System;
using PurrNet;

namespace Events
{
    public struct GameEvents
    {
        public static Action OnGameStateReady;
        public static Action<PlayerID> OnPlayerDisconnected;
        public static Action<PlayerID, PlayerTeam> OnPlayerChangedTeam;
        public static Action OnStartGame;
    }
}