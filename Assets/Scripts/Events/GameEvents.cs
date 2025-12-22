using System;
using PurrNet;

namespace Events
{
    public struct GameEvents
    {
        public static Action OnGameStateReady;
        public static Action<
            string /* UniqueDeviceId */,
            PlayerID /* localPlayerId */,
            string /* username */,
            bool /* asHost */> StartGame;
        public static Action<PlayerState, PlayerTeam> OnPlayerChangedTeam;
    }
}