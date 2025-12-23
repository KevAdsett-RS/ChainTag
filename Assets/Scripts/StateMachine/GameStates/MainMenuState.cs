using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace StateMachine.GameStates
{
    [Serializable]
    public class MainMenuState : BaseGameState
    {
        private List<string> _potentialNames = new() {"Oliver",
            "George",
            "Noah",
            "Arthur",
            "Harry",
            "Leo",
            "Muhammad",
            "Jack",
            "Charlie",
            "Oscar",
            "Jacob",
            "Henry",
            "Thomas",
            "Freddie",
            "Alfie",
            "Theo",
            "William",
            "Theodore",
            "Archie",
            "Joshua",
            "Alexander",
            "James",
            "Isaac",
            "Edward",
            "Lucas",
            "Tommy",
            "Finley",
            "Max",
            "Logan",
            "Ethan",
            "Mohammed",
            "Teddy",
            "Benjamin",
            "Arlo",
            "Joseph",
            "Sebastian",
            "Harrison",
            "Elijah",
            "Adam",
            "Daniel",
            "Samuel",
            "Louie",
            "Mason",
            "Reuben",
            "Albie",
            "Rory",
            "Jaxon",
            "Hugo",
            "Luca",
            "Zachary",
            "Reggie",
            "Hunter",
            "Louis",
            "Dylan",
            "Albert",
            "David",
            "Jude",
            "Frankie",
            "Roman",
            "Ezra",
            "Toby",
            "Riley",
            "Carter",
            "Ronnie",
            "Frederick",
            "Gabriel",
            "Stanley",
            "Bobby",
            "Jesse",
            "Michael",
            "Elliot",
            "Grayson",
            "Mohammad",
            "Liam",
            "Jenson",
            "Ellis",
            "Harley",
            "Harvey",
            "Jayden",
            "Jake",
            "Ralph",
            "Rowan",
            "Elliott",
            "Jasper",
            "Ollie",
            "Charles",
            "Finn",
            "Felix",
            "Caleb",
            "Chester",
            "Jackson",
            "Hudson",
            "Leon",
            "Ibrahim",
            "Ryan",
            "Blake",
            "Alfred",
            "Oakley",
            "Matthew",
            "Luke",};
        
        private MainMenuUi _ui;
        private NetworkManager _networkManager;

        private Dictionary<string, object> _gameStartPacket = new();

        protected override void OnEnter()
        {
            base.OnEnter();
            _networkManager = Object.FindFirstObjectByType<NetworkManager>();
            
            _networkManager.onNetworkStarted += OnNetworkStarted;
            _networkManager.onClientConnectionState += OnClientConnectionStateChange;
            _networkManager.onLocalPlayerReceivedID += OnLocalPlayerReceivedId;
            
            Events.MainMenuEvents.OnJoinButtonPressed += OnJoin;
            Events.MainMenuEvents.OnHostButtonPressed += OnHost;
            Events.MainMenuEvents.OnUsernameEdited += OnUsernameEdited;

            _ui = Object.FindFirstObjectByType<MainMenuUi>();
            if (_ui)
            {
                var existingDisplayName = Owner.GetStatePacket<string>("displayName");
                _gameStartPacket.Add("displayName", existingDisplayName);
                if (string.IsNullOrEmpty(existingDisplayName))
                {
                    _ui.SetDisplayName(_potentialNames[Random.Range(0, _potentialNames.Count)]);
                }
                else
                {
                    _ui.SetDisplayName(existingDisplayName);
                }
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            _networkManager.onNetworkStarted -= OnNetworkStarted;
            _networkManager.onClientConnectionState -= OnClientConnectionStateChange;
            _networkManager.onLocalPlayerReceivedID -= OnLocalPlayerReceivedId;
            
            Events.MainMenuEvents.OnJoinButtonPressed -= OnJoin;
            Events.MainMenuEvents.OnHostButtonPressed -= OnHost;
            Events.MainMenuEvents.OnUsernameEdited -= OnUsernameEdited;
        }

        public void OnJoin()
        {
            Debug.Log("Starting client");
            _networkManager.StartClient();
                
        }

        public void OnHost()
        {
            Debug.Log("Starting host");
            _networkManager.StartHost();
        }

        public async void OnUsernameEdited(string username)
        {
            try
            {
                await Owner.NakamaClient.SetDisplayname(username);
                if (!_gameStartPacket.TryAdd("displayName", username))
                {
                    _gameStartPacket["displayName"] = username;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Something went wrong setting username: {e.Message}");
            }
        }

        private void OnNetworkStarted(NetworkManager manager, bool asServer)
        {
            Debug.Log($"MainMenuState::OnNetworkStarted, asServer: {asServer}");
        }

        private void OnClientConnectionStateChange(ConnectionState connectionState)
        {
            Debug.Log($"MainMenuState::OnClientConnectionStateChange: {connectionState}");
            if (connectionState == ConnectionState.Connected)
            {
            }
        }

        private void OnLocalPlayerReceivedId(PlayerID playerId)
        {
            Debug.Log($"MainMenuState::OnLocalPlayerReceivedId: {playerId}");
            _gameStartPacket.Add("localPlayerId", playerId);
            Owner.ChangeState("LobbyState", _gameStartPacket);
        }
    }
}