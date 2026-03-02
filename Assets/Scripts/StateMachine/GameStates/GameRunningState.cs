using System.Collections.Generic;
using System.Linq;
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
        
        private bool _useDefaultSceneLoading;
        
        private Scene _loadedScene;
        
        protected override bool UseDefaultSceneLoading() => _useDefaultSceneLoading;

        protected override void OnEnter()
        {
            Debug.Log($"GameRunningState::OnEnter: {SceneName}");
            _networkManager = Object.FindAnyObjectByType<NetworkManager>();
            MatchEvents.OnExitingMatchEndState += OnExitingMatchEndState;
            _networkManager.sceneModule.onPostSceneLoaded += OnSceneLoaded;
            
            if (!_networkManager.isServer)
            {
                return;
            }
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

        private void OnExitingMatchEndState()
        {
            Debug.Log($"GameRunningState::OnExitingMatchEndState");
            MatchEvents.OnExitingMatchEndState -= OnExitingMatchEndState;
            // We've disconnected from purrnet by now, so unload the scene the Unity way
            _useDefaultSceneLoading = true;
            Owner.ChangeState("GameLoadingState");
        }

        private void OnSceneLoaded(SceneID sceneId, bool asServer)
        {
            Debug.Log($"GameRunningState::OnSceneLoaded: {SceneName}");
            _networkManager.sceneModule.onPostSceneLoaded -= OnSceneLoaded;

            if (_networkManager.sceneModule.TryGetSceneState(sceneId, out var sceneState))
            {
                _loadedScene = sceneState.scene;
            }

            if (!asServer)
            {
                return;
            }
            
            var spawner = Object.FindFirstObjectByType<StateMachineSpawner>();
            if (spawner)
            {
                spawner.Spawn();
            }
        }
    }
}