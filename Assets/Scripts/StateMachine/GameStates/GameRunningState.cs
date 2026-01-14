using System.Collections.Generic;
using Events;
using Match;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace StateMachine.GameStates
{
    public class GameRunningState : BaseGameState
    {
        private NetworkManager _networkManager;

        private MatchState _matchState;

        private SceneID _loadedSceneId;

        protected override bool UseDefaultSceneLoading() => false;

        protected override void OnEnter()
        {
            Debug.Log($"GameRunningState::OnEnter: {SceneName}");
            _networkManager = Object.FindAnyObjectByType<NetworkManager>();
            if (!_networkManager.isServer)
            {
                return;
            }
            _networkManager.sceneModule.onPostSceneLoaded += OnSceneLoaded;
            _networkManager.sceneModule.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        }

        protected override void OnExit()
        {
            Debug.Log($"GameRunningState::OnExit: {SceneName}");
            if (_networkManager)
            {
                if (_networkManager.isServer)
                {
                    Debug.Log($"GameRunningState::Attempting to unload: {SceneName}");
                    _networkManager.sceneModule.UnloadSceneAsync(SceneName);
                }
            }

            base.OnExit();
        }

        private void OnSceneLoaded(SceneID sceneId, bool asServer)
        {
            Debug.Log($"GameRunningState::OnSceneLoaded: {SceneName}");
            _networkManager.sceneModule.onPostSceneLoaded -= OnSceneLoaded;
            if (_networkManager.isServer)
            {
                GameEvents.OnStartGame?.Invoke();
            }
        }
    }
}