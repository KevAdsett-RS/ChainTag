using System;
using System.Collections.Generic;
using Match;
using PurrNet;
using TMPro;
using UnityEngine;
using Object = System.Object;

public class LobbyUi : StateBinder
{

    struct NameChangeResponse
    {
        public SyncVar<string> NameVar;
        public Action<string> Handler;
    }
    [SerializeField]
    private GameObject playerListContainer;
    [SerializeField]
    private GameObject playerListEntryPrefab;
    [SerializeField]
    private GameObject startGameButton;
    [SerializeField]
    private GameObject waitingForHostText;

    private readonly Dictionary<PlayerID, GameObject> _playerNameObjects = new();
    private readonly Dictionary<PlayerID, NameChangeResponse> _nameChangeHandlers = new();
    
    private static NameChangeResponse SetupNewNameChangeResponse(SyncVar<string> valueName, Action<string> handler)
    {
        var response = new NameChangeResponse
        {
            NameVar = valueName,
            Handler = handler
        };
        response.NameVar.onChanged += response.Handler;
        return response;
    }

    private static void TearDownNameChangeResponse(NameChangeResponse response)
    {
        response.NameVar.onChanged -= response.Handler;
    }
    
    protected override void Awake()
    {
        Debug.Log("LobbyUi::Awake");
        base.Awake();
        var networkManager = FindFirstObjectByType<NetworkManager>();
        startGameButton.SetActive(networkManager.isHost);
        waitingForHostText.SetActive(networkManager.isClientOnly);
    }
    
    public void OnStartGameButtonPressed()
    {
        Events.LobbyEvents.OnStartGameButtonPressed?.Invoke();
    }
    
    protected override void RegisterBindings(MatchState matchState, List<IStateBinding> stateBindings)
    {
        Debug.Log($"LobbyUi::RegisterBindings {matchState}");
        stateBindings.Add(new DictionaryStateBinding<PlayerID, PlayerState>(matchState.Players, OnPlayerListChanged));
    }

    private void OnPlayerListChanged(SyncDictionaryChange<PlayerID, PlayerState> change)
    {
        Debug.Log($"LobbyUi::OnPlayerListChanged {change}");
        switch (change.operation)
        {
            case SyncDictionaryOperation.Added:
            {
                AddPlayerEntry(change);
                break;
            }
            case SyncDictionaryOperation.Removed:
            {
                var nameObject = _playerNameObjects[change.key];
                Destroy(nameObject);
                _playerNameObjects.Remove(change.key);
                TearDownNameChangeResponse(_nameChangeHandlers[change.key]);
                _nameChangeHandlers.Remove(change.key);
                break;
            }
            case SyncDictionaryOperation.Set:
            {
                if (_playerNameObjects.TryGetValue(change.key, out var playerNameObject))
                {
                    Destroy(playerNameObject);
                }
                AddPlayerEntry(change);
                break;
            }
            case SyncDictionaryOperation.Cleared:
            {
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AddPlayerEntry(SyncDictionaryChange<PlayerID, PlayerState> change)
    {
        Debug.Log($"LobbyUi::AddPlayerEntry: key: {change.key}");
        Debug.Log($"LobbyUi::AddPlayerEntry: value: {change.value}");
        Debug.Log($"LobbyUi::AddPlayerEntry: playerListEntryPrefab: {playerListEntryPrefab}");
        Debug.Log($"LobbyUi::AddPlayerEntry: playerListContainer: {playerListContainer} ");
        PlayerID playerKey = change.key;
        if (_nameChangeHandlers.ContainsKey(playerKey))
        {
            return;
        }
        
        var newPlayerNameObject = Instantiate(playerListEntryPrefab, playerListContainer.transform);
        
        Action<string> handler = (newName) => OnPlayerNameChanged(playerKey, newName);
        _nameChangeHandlers.Add(playerKey, SetupNewNameChangeResponse(change.value.Name, handler));
        change.value.Name.onChanged += handler;
        
        _playerNameObjects.Add(change.key, newPlayerNameObject);
        
        handler(change.value.Name.value);
    }

    protected override void OnDestroy()
    {
        foreach (var keyValuePair in _nameChangeHandlers)
        {
            TearDownNameChangeResponse(keyValuePair.Value);
        }
        _nameChangeHandlers.Clear();
        base.OnDestroy();
    }

    private void OnPlayerNameChanged(PlayerID playerKey, string newValue)
    {
        var playerNameObject = _playerNameObjects[playerKey];
        playerNameObject.GetComponentInChildren<TMP_Text>().text = newValue;
    }
}
