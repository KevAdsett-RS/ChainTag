using Events;
using PurrNet;
using UnityEngine;

namespace Match
{
    public enum PlayerTeam
    {
        Unset,
        ChainTeam,
        FreeTeam
    }

    public class PlayerState : NetworkIdentity
    {
        public readonly SyncVar<string> Name = new();
        public readonly SyncVar<PlayerTeam> Team = new();
        public readonly SyncVar<PlayerID> PlayerId = new();
        public readonly SyncVar<PlayerState> LinkedPlayer = new();
        public readonly SyncVar<bool> IsHead = new();
        public readonly SyncVar<GameObject> Body = new();

        private void Awake()
        {
            Name.onChanged += OnNameChanged;
        }

        private void OnNameChanged(string newValue)
        {
            name = newValue + "State";
        }

        protected override void OnDestroy()
        {
            Debug.Log($"PlayerState::OnDestroy for {name}");
            Name.onChanged -= OnNameChanged;
            base.OnDestroy();
        }

        [ServerOnly]
        public void Server_Initialise(PlayerID target, string displayName, PlayerTeam team)
        {
            Debug.Log($"PlayerState::Server_Initialise for {name} ({target})");
            PlayerId.value = target;
            Name.value = displayName;
            Server_ChangeTeam(team);
        
            IsHead.value = team == PlayerTeam.ChainTeam;
        }

        [ServerOnly]
        public void Server_SetBody(GameObject body)
        {
            Body.value = body;
        }

        [ServerOnly]
        public void Server_ChangeTeam(PlayerTeam newTeam)
        {
            Debug.Log($"PlayerState::Server_ChangeTeam for {name} (Id: {PlayerId.value}) -> {newTeam}");
            Team.value = newTeam;
            MatchEvents.OnPlayerChangedTeam?.Invoke(PlayerId.value, newTeam);
        }

        [ServerOnly]
        public void Server_SetLinkedPlayer(PlayerState player)
        {
            Debug.Log($"PlayerState::Server_SetLinkedPlayer for {name} (Id: {PlayerId.value}) -> {player.Name.value}");
            LinkedPlayer.value = player;
        }
    }
}