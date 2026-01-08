using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameUi : StateBinder
{
    [SerializeField] private TMP_Text ChainPlayerCount;
    [SerializeField] private TMP_Text FreePlayerCount;
    [SerializeField] private TMP_Text TimeRemaining;
    
    protected override void RegisterBindings(GameState gameState, List<IStateBinding> stateBindings)
    {
        stateBindings.Add(new VarStateBinding<int>(gameState.ChainPlayerCount, OnChainPlayerCountChanged));
        stateBindings.Add(new VarStateBinding<int>(gameState.FreePlayerCount, OnFreePlayerCountChanged));
        stateBindings.Add(new VarStateBinding<string>(gameState.TimeRemainingString, OnTimeRemainingChanged));
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
