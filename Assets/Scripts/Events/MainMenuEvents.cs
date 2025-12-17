using System;

namespace Events
{
    public struct MainMenuEvents
    {
        public static Action<string> OnUsernameEdited;
        public static Action OnJoinButtonPressed;
        public static Action OnHostButtonPressed;
    }
}