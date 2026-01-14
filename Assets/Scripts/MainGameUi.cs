using System;
using System.Collections.Generic;
using Match;
using TMPro;
using UnityEngine;

public class MainGameUi : StateBinder
{
    [SerializeField] private TMP_Text ChainPlayerCount;
    [SerializeField] private TMP_Text FreePlayerCount;
    [SerializeField] private TMP_Text TimeRemaining;
    
    protected override void RegisterBindings(MatchState matchState, List<IStateBinding> stateBindings)
    {
        stateBindings.Add(new VarStateBinding<int>(matchState.ChainPlayerCount, OnChainPlayerCountChanged));
        stateBindings.Add(new VarStateBinding<int>(matchState.FreePlayerCount, OnFreePlayerCountChanged));
        stateBindings.Add(new VarStateBinding<string>(matchState.TimeRemainingString, OnTimeRemainingChanged));
    }

    private void OnChainPlayerCountChanged(int newValue)
    {
        ChainPlayerCount.text = "" + newValue;
    }

    private void OnFreePlayerCountChanged(int newValue)
    {
        FreePlayerCount.text = "" + newValue;
    }
    
    private void OnTimeRemainingChanged(string newValue)
    {
        TimeRemaining.text = newValue;
    }

}
