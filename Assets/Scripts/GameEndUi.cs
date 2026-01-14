using System;
using System.Collections.Generic;
using Events;
using Match;
using TMPro;
using UnityEngine;

public class GameEndUi : StateBinder
{
    [SerializeField] private TMP_Text ChainPlayerCount;
    [SerializeField] private Color chainColour;
    [SerializeField] private Color freeColour;
    
    
    protected override void RegisterBindings(GameState gameState, List<IStateBinding> stateBindings)
    {
        stateBindings.Add(new VarStateBinding<PlayerTeam>(gameState.WinningTeam, OnWinningTeamChanged));
    }

    private void OnWinningTeamChanged(PlayerTeam newValue)
    {
        Debug.Log($"GameEndUi::OnWinningTeamChanged: {newValue}");
        switch (newValue)
        {
            case PlayerTeam.ChainTeam:
                ChainPlayerCount.text = "CHAIN TEAM WINS!";
                ChainPlayerCount.color = chainColour;
                break;
            case PlayerTeam.FreeTeam:
                ChainPlayerCount.text = "FREE TEAM WINS!";
                ChainPlayerCount.color = freeColour;
                break;
            case PlayerTeam.Unset:
                ChainPlayerCount.text = "DRAW, SOMEHOW?";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
        }
    }

    public void OnLeaveButtonPressed()
    {
        Debug.Log($"GameEndUi::OnLeaveButtonPressed");
        GameEvents.OnLeaveGamePressed?.Invoke();
    }
}
