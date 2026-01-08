using System;
using PurrNet;

namespace Events
{
    public struct LobbyEvents
    {
        public static Action<
            string /* UniqueDeviceId */,
            PlayerID /* localPlayerId */,
            string /* username */,
            bool /* asHost */> AddPlayerToGame;
        public static Action OnStartGameButtonPressed;
    }
}