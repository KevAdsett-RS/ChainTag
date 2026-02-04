using System;
using System.Collections.Generic;
using Events;
using Match;
using TMPro;
using UnityEngine;

public class MatchEndUi : StateBinder
{
    [SerializeField] private TMP_Text ChainPlayerCount;
    [SerializeField] private Color chainColour;
    [SerializeField] private Color freeColour;
    
    
    protected override void RegisterBindings(MatchState matchState, List<IStateBinding> stateBindings)
    {
        stateBindings.Add(new VarStateBinding<PlayerTeam>(matchState.WinningTeam, OnWinningTeamChanged));
    }

    protected override void OnDestroy()
    {
        Debug.Log($"MatchEndUi::OnDestroy");
        MatchEvents.MatchEndUiDestroyed?.Invoke();
        base.OnDestroy();
    }

    private void OnWinningTeamChanged(PlayerTeam newValue)
    {
        Debug.Log($"MatchEndUi::OnWinningTeamChanged: {newValue}");
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
        Debug.Log($"MatchEndUi::OnLeaveButtonPressed");
        MatchEvents.OnLeaveMatchPressed?.Invoke();
    }
}
