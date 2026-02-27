using System;
using System.Collections.Generic;
using Nakama;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private bool _networkStarted;
        private PlayerID _localClientId;

        protected override void OnEnter()
        {
            base.OnEnter();
            _networkManager = Object.FindFirstObjectByType<NetworkManager>();
            
            _networkManager.onNetworkStarted += OnNetworkStarted;
            _networkManager.onLocalPlayerReceivedID += OnLocalPlayerReceivedId;

            if (_networkManager.isServer)
            {
                _networkManager.sceneModule.UnloadSceneAsync(PersistentSceneName);
            }
            else if (SceneManager.GetSceneByName(PersistentSceneName).isLoaded)
            {
                SceneManager.UnloadSceneAsync(PersistentSceneName);
            }
            
            Events.MainMenuEvents.OnStartButtonPressed += OnStartGame;
            Events.MainMenuEvents.OnUsernameEdited += OnUsernameEdited;

            _ui = Object.FindFirstObjectByType<MainMenuUi>();
            if (!_ui)
            {
                return;
            }
            
            if (Owner.TryGetStatePacket("displayName", out string existingDisplayName))
            {
                _gameStartPacket.Add("displayName", existingDisplayName);
                _ui.SetDisplayName(string.IsNullOrEmpty(existingDisplayName)
                    ? _potentialNames[Random.Range(0, _potentialNames.Count)]
                    : existingDisplayName);
            }
            else
            {
                _ui.SetDisplayName(_potentialNames[Random.Range(0, _potentialNames.Count)]);
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            
            Events.MainMenuEvents.OnStartButtonPressed -= OnStartGame;
            Events.MainMenuEvents.OnUsernameEdited -= OnUsernameEdited;
        }

        private void OnNetworkStarted(NetworkManager manager, bool asServer)
        {
            Debug.Log($"MainMenuState::OnNetworkStarted: asServer: {asServer}");
            _networkManager.onNetworkStarted -= OnNetworkStarted;
            _networkStarted = true;
            GoToLobbyIfReady();
        }

        private void GoToLobbyIfReady()
        {
            Debug.Log($"MainMenuState::GoToLobbyIfReady: _localClientId: {_localClientId}, _networkStarted: {_networkStarted}");
            if (_localClientId.id != 0 && _networkStarted)
            {
                Owner.Next(_gameStartPacket);
            }
        }
        
        public async void OnStartGame()
        {
            var myIp = "127.0.0.1"; // Or your public IP
            var matchId = await Owner.NakamaClient.EnterGame(myIp);

            if (string.IsNullOrEmpty(matchId))
            {
                return;
            }
            
            // Join the Nakama Match to get the Host IP from the Label
            var socket = await Owner.NakamaClient.GetSocket();
            var match = await socket.JoinMatchAsync(matchId);

            if (string.IsNullOrEmpty(match.Label))
            {
                return;
            }
            
            var labelData = JsonUtility.FromJson<MatchLabel>(match.Label);
            
            // If the IP in the label is MY IP, I'm the host
            if (labelData.host_ip == myIp) {
                _networkManager.StartHost();
            } else {
                // Otherwise, I'm a client connecting to that IP
                // _networkManager.SetAddress(labelData.host_ip);
                _networkManager.StartClient();
            }
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


        private void OnLocalPlayerReceivedId(PlayerID playerId)
        {
            Debug.Log($"MainMenuState::OnLocalPlayerReceivedId: {playerId}");
            _localClientId = playerId;
            _networkManager.onLocalPlayerReceivedID -= OnLocalPlayerReceivedId;
            
            _gameStartPacket.Add("localPlayerId", playerId);
            GoToLobbyIfReady();
        }
    }
}