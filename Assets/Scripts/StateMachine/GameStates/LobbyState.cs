using Events;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace StateMachine.GameStates
{
    public class LobbyState : BaseGameState
    {
        private NetworkManager _networkManager;

        private GameState _gameState;

        private const string PersistentSceneName = "ConnectedPersistent";

        protected override bool UseDefaultSceneLoading() => false;

        protected override void OnEnter()
        {
            _networkManager = Object.FindAnyObjectByType<NetworkManager>();
            _networkManager.sceneModule.onPostSceneLoaded += OnSceneLoaded;
            if (!_networkManager.isServer)
            {
                return;
            }
            _networkManager.sceneModule.LoadSceneAsync(PersistentSceneName);

            LobbyEvents.OnStartGameButtonPressed += OnStartGameButtonPressed;
        }

        protected override void OnExit()
        {
            if (_networkManager)
            {
                _networkManager.sceneModule.onPostSceneLoaded -= OnSceneLoaded;
                _networkManager.sceneModule.UnloadSceneAsync(SceneName);
            }
            LobbyEvents.OnStartGameButtonPressed -= OnStartGameButtonPressed;

            base.OnExit();
        }

        private void OnSceneLoaded(SceneID sceneId, bool asServer)
        {
            Debug.Log($"LobbyState::OnSceneLoaded: {sceneId}");
            if (!_networkManager)
            {
                return;
            }
            var sceneName = _networkManager.sceneModule.sceneStates[sceneId].scene.name;
            Debug.Log($"LobbyState::OnSceneLoaded: loaded scene: {sceneName}");
            if (sceneName == SceneName)
            {
                OnLobbySceneLoaded();
            }
            else if (sceneName == PersistentSceneName)
            {
                OnPersistentSceneLoaded(asServer);
            }
        }

        private void OnPersistentSceneLoaded(bool asServer)
        {
            if (asServer)
            {
                _networkManager.sceneModule.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            }
        }

        private void OnLobbySceneLoaded()
        {
            AddPlayerToGame(_networkManager.isHost);
        }

        private void AddPlayerToGame(bool asHost)
        {
            LobbyEvents.AddPlayerToGame?.Invoke(Owner.UniqueDeviceId,
                Owner.GetStatePacket<PlayerID>("localPlayerId"),
                Owner.GetStatePacket<string>("displayName"),
                asHost);
        }

        private void OnStartGameButtonPressed()
        {
            Owner.ChangeState("GameRunningState");
        }
    }
}