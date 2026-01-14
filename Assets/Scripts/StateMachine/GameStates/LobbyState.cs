using Events;
using Match;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace StateMachine.GameStates
{
    public class LobbyState : BaseGameState
    {
        private NetworkManager _networkManager;

        private bool _lobbyInitialised;

        private GameState _gameState;

        protected override bool UseDefaultSceneLoading() => false;

        protected override void OnEnter()
        {
            _networkManager = Object.FindAnyObjectByType<NetworkManager>();
            LobbyEvents.OnStartGameButtonPressed += OnStartGameButtonPressed;
            
            
            AddPlayerToGame(_networkManager.isHost);
            
            if (_networkManager.isServer)
            {
                _networkManager.sceneModule.LoadSceneAsync(PersistentSceneName, LoadSceneMode.Additive);
                _networkManager.sceneModule.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            }
        }

        protected override void OnExit()
        {
            LobbyEvents.OnStartGameButtonPressed -= OnStartGameButtonPressed;
            if (_networkManager)
            {
                if (_networkManager.isServer)
                {
                    _networkManager.sceneModule.UnloadSceneAsync(SceneName);
                }
            }

            base.OnExit();
        }

        private void AddPlayerToGame(bool asHost)
        {
            if (Owner.TryGetStatePacket<PlayerID>("localPlayerId", out var localPlayerId) &&
                Owner.TryGetStatePacket<string>("displayName", out var displayName))
            {
                LobbyEvents.AddPlayerToGame?.Invoke(Owner.UniqueDeviceId,
                    localPlayerId,
                    displayName,
                    asHost);
            }
        }

        private void OnStartGameButtonPressed()
        {
            Owner.ChangeState("GameRunningState");
        }
    }
}