using System;
using PurrNet;
using PurrNet.StateMachine;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match.MatchStates
{

   public abstract class BaseMatchStateNode : StateNode
    {
        protected abstract string SceneName { get; }
        private readonly SyncVar<bool> _sceneIsLoaded = new();
        private AsyncOperation _sceneLoadOperation;

        private void Awake()
        {
            _sceneIsLoaded.onChanged += Client_OnSceneLoaded;
        }

        public override void Enter(bool asServer)
        {
            Debug.Log($"BaseMatchStateNode({name})::Enter {asServer}");
            base.Enter(asServer);
            RegisterEvents(asServer);
            
            if (!asServer)
            {
                return;
            }

            networkManager.sceneModule.onSceneLoaded += Server_OnSceneLoaded;
            Debug.Log($"BaseMatchStateNode({name})::Exit Attempting to load scene: {SceneName}");
            _sceneLoadOperation = networkManager.sceneModule.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        }

        public override bool CanExit()
        {
            return _sceneLoadOperation.isDone;
        }

        public override void Exit(bool asServer)
        {
            Debug.Log($"BaseMatchStateNode({name})::Exit {asServer}");
            base.Exit(asServer);

            if (asServer)
            {
                if (networkManager.serverState != ConnectionState.Connected)
                {
                    return;
                }
                Debug.Log($"BaseMatchStateNode({name})::Exit: Server is attempting to unload scene: {SceneName}");
                networkManager.sceneModule.UnloadSceneAsync(SceneName);
                return;
            }

            if (!networkManager.isOffline)
            {
                return;
            }
            
            Debug.Log($"BaseMatchStateNode({name})::Exit: Network manager offline: Attempting to unload scene with Unity scene management: {SceneName}");
            SceneManager.UnloadSceneAsync(SceneName);
        }

        [ServerOnly]
        protected virtual void Server_OnSceneLoaded(SceneID scene, bool asServer)
        {
            if (!asServer)
            {
                return;
            }
            _sceneIsLoaded.value = true;
            networkManager.sceneModule.TryGetSceneState(scene, out var sceneState);
            Debug.Log($"BaseMatchStateNode({name})::OnSceneLoaded {sceneState.scene.name}");
            networkManager.sceneModule.onSceneLoaded -= Server_OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            UnregisterEvents();
            base.OnDestroy();
        }

        protected virtual void RegisterEvents(bool asServer)
        {
        }

        protected virtual void UnregisterEvents()
        {
            _sceneIsLoaded.onChanged -= Client_OnSceneLoaded;
        }

        [Client]
        protected virtual void Client_OnSceneLoaded(bool asServer)
        {
            Debug.Log($"BaseMatchStateNode({name})::Client_OnSceneLoaded {asServer}");
        }
        
    }
    public abstract class BaseMatchStateNode<T> : StateNode<T>
    {
        protected abstract string SceneName { get; }
        protected readonly SyncVar<bool> _sceneIsLoaded = new();
        
        public override void Enter(T data, bool asServer)
        {
            Debug.Log($"BaseMatchStateNode({name})::Enter {asServer}");
            base.Enter(data, asServer);
            RegisterEvents(asServer);
            
            if (!asServer)
            {
                return;
            }
            
            networkManager.sceneModule.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        }

        public override void Exit(bool asServer)
        {
            Debug.Log($"BaseMatchStateNode({name})::Exit {asServer}");
            base.Exit(asServer);
            
            if (!asServer)
            {
                return;
            }

            networkManager.sceneModule.UnloadSceneAsync(SceneName);
            UnregisterEvents();
        }

        protected override void OnDestroy()
        {
            UnregisterEvents();
            base.OnDestroy();
        }

        protected virtual void RegisterEvents(bool asServer)
        {
            _sceneIsLoaded.onChanged += Client_OnSceneLoaded;
        }

        protected virtual void UnregisterEvents()
        {
            _sceneIsLoaded.onChanged -= Client_OnSceneLoaded;
        }

        protected virtual void Client_OnSceneLoaded(bool asServer)
        {
            Debug.Log($"BaseMatchStateNode({name})::Client_OnSceneLoaded {asServer}");
        }
    }
}