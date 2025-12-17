using System;
using PurrNet;

namespace Events
{
    public struct GameEvents
    {
        public static Action OnGameStateReady;
        public static Action<string, PlayerID, string> StartGame;
        
    }
}