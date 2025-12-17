using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.GameStates
{
    public class GameLoadingState : BaseGameState
    {
        private bool _ready;
        protected override void OnEnter()
        {
            Debug.Log("GameLoadingState::OnEnter");
            RequestNakamaId();
        }
        
        private void RequestNakamaId()
        {
            Debug.Log($"GameLoadingState::RequestNakamaId: {Owner.UniqueDeviceId}");
            Owner.NakamaClient.OnReady += OnNakamaClientReady;
            Owner.NakamaClient.WaitForClient(Owner.UniqueDeviceId);
        }

        private void OnNakamaClientReady()
        {
            if (!_ready)
            {
                _ready = true;
                Owner.NakamaClient.OnReady -= OnNakamaClientReady;
                Debug.Log($"GameLoadingState::OnNakamaClientReady: {Owner.UniqueDeviceId}");
                var packet = new Dictionary<string, object> { { "displayName", Owner.NakamaClient.User.DisplayName } };
                Owner.ChangeState("MainMenuState", packet);
            }
        }
    }
}