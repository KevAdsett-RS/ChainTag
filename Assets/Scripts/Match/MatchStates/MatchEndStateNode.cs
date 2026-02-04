using Events;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;

namespace Match.MatchStates
{
    public class MatchEndStateNode : BaseMatchStateNode
    {
        protected override string SceneName => "MatchEnd";
        private MatchState _matchState;

        public override void Enter(bool asServer)
        {
            base.Enter(asServer);
            _matchState = FindFirstObjectByType<MatchState>();
            Debug.Log($"MatchEndStateNode::Enter: {_matchState.WinningTeam.value}, asServer: {asServer}");
        }

        public override void Exit(bool asServer)
        {
            if (!asServer)
            {
                MatchEvents.OnExitingMatchEndState?.Invoke();
            }
            base.Exit(asServer);
        }

        protected override void RegisterEvents(bool asServer)
        {
            base.RegisterEvents(asServer);
            MatchEvents.OnLeaveMatchPressed += OnPlayerLeavingGame;
            MatchEvents.MatchEndUiDestroyed += OnMatchEndUiDestroyed;
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            MatchEvents.OnLeaveMatchPressed -= OnPlayerLeavingGame;
            MatchEvents.MatchEndUiDestroyed -= OnMatchEndUiDestroyed;
        }
        
        private void OnPlayerLeavingGame()
        {
            MatchEvents.OnLeaveMatchPressed -= OnPlayerLeavingGame;
            Debug.Log($"MatchEndStateNode::OnPlayerLeavingGame");

            if (networkManager.isServer)
            {
                networkManager.StopServer();
            }
            else if (networkManager.isClient)
            {
                networkManager.StopClient();
            }
        }
        
        private void OnMatchEndUiDestroyed()
        {
            Debug.Log($"MatchEndStateNode::OnMatchEndUiDestroyed");
            if (networkManager.isServer)
            {
                MatchEvents.OnExitingMatchEndState?.Invoke();
            }
        }
    }
}
