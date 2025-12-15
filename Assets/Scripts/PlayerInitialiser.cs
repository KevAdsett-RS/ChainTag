using System.Collections.Generic;
using Input;
using Nakama;
using ParrelSync;
using PurrNet;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerInitialiser : NetworkIdentity
{
    [SerializeField] private TextMeshProUGUI _usernameText;

    private string _uniqueDeviceId;
    
    private readonly SyncVar<string> _nakamaId = new();
    private readonly SyncVar<string> _name = new();
    private List<string> _potentialNames = new() {"Oliver",
        "George",
        "Noah",
        "Arthur",
        "Harry",
        "Leo",
        "Muhammad",
        "Jack",
        "Charlie",
        "Oscar",
        "Jacob",
        "Henry",
        "Thomas",
        "Freddie",
        "Alfie",
        "Theo",
        "William",
        "Theodore",
        "Archie",
        "Joshua",
        "Alexander",
        "James",
        "Isaac",
        "Edward",
        "Lucas",
        "Tommy",
        "Finley",
        "Max",
        "Logan",
        "Ethan",
        "Mohammed",
        "Teddy",
        "Benjamin",
        "Arlo",
        "Joseph",
        "Sebastian",
        "Harrison",
        "Elijah",
        "Adam",
        "Daniel",
        "Samuel",
        "Louie",
        "Mason",
        "Reuben",
        "Albie",
        "Rory",
        "Jaxon",
        "Hugo",
        "Luca",
        "Zachary",
        "Reggie",
        "Hunter",
        "Louis",
        "Dylan",
        "Albert",
        "David",
        "Jude",
        "Frankie",
        "Roman",
        "Ezra",
        "Toby",
        "Riley",
        "Carter",
        "Ronnie",
        "Frederick",
        "Gabriel",
        "Stanley",
        "Bobby",
        "Jesse",
        "Michael",
        "Elliot",
        "Grayson",
        "Mohammad",
        "Liam",
        "Jenson",
        "Ellis",
        "Harley",
        "Harvey",
        "Jayden",
        "Jake",
        "Ralph",
        "Rowan",
        "Elliott",
        "Jasper",
        "Ollie",
        "Charles",
        "Finn",
        "Felix",
        "Caleb",
        "Chester",
        "Jackson",
        "Hudson",
        "Leon",
        "Ibrahim",
        "Ryan",
        "Blake",
        "Alfred",
        "Oakley",
        "Matthew",
        "Luke",};

    private NakamaClient _nakamaClient;

    private void Awake()
    {
        _nakamaClient = FindFirstObjectByType<NakamaClient>();
        string suffix = "";
#if UNITY_EDITOR
        if (ClonesManager.IsClone())
        {
            var pathTokens = ClonesManager.GetCurrentProjectPath().Split('_');
            suffix = "_" + pathTokens[^1];
        }
#endif
        _uniqueDeviceId = SystemInfo.deviceUniqueIdentifier + suffix;
        Debug.Log($"{GetContext()} PlayerInitialiser::Awake");
        // _nakamaId.onChanged += OnNakamaIdChanged;
        _name.onChanged += OnNameChanged;
    }

    protected override void OnDestroy()
    {
        Debug.Log($"{GetContext()} PlayerInitialiser::OnDestroy");
        _name.onChanged -= OnNameChanged;
        // if (_client)
        // {
        //     _client.OnReady -= InitialiseNakamaId;
        // }
        // base.OnDestroy();
    }

    protected override void OnSpawned()
    {
        if (!isOwner)
        {
            return;
        }
        else
        {
            gameObject.AddComponent<PlayerInputHandler>();
            gameObject.GetComponent<PlayerMovement>().enabled = true;
        }
        Debug.Log($"{GetContext()}: Requesting NakamaId for device: {_uniqueDeviceId}");
        RequestNakamaId(_uniqueDeviceId);
    }


    private void OnNameChanged(string newValue)
    {
        Debug.Log($"{GetContext()} PlayerInitialiser::OnNameChanged: {newValue}");
        name = _name.value;
        _usernameText.text = name;
    }

    private void OnNakamaIdChanged(string newValue)
    {
        Debug.Log($"{GetContext()} PlayerInitialiser::OnNakamaIdChanged: {newValue}");
    }

    private string GetContext()
    {
        return isServer ? "Server" : "Client";
    }

    [ServerRpc]
    private void RequestNakamaId(string uniqueDeviceId)
    {
        Debug.Log($"{GetContext()} PlayerInitialiser::RequestNakamaId: {uniqueDeviceId}");
        _nakamaClient.OnReady += OnNakamaClientReady;
        _nakamaClient.Connect(uniqueDeviceId);
    }

    [ServerOnly]
    private void OnNakamaClientReady(ISession nakamaSession)
    {
        _nakamaClient.OnReady -= OnNakamaClientReady;
        var user = _nakamaClient.User;
        _nakamaId.value = user.Id;
        if (string.IsNullOrEmpty(user.DisplayName))
        {
            AssignNewUsername(nakamaSession);
        }
        else
        {
            _name.value = user.DisplayName;
        }
    }

    [ServerOnly]
    private void AssignNewUsername(ISession nakamaSession)
    {
        var newUsername = _potentialNames[Random.Range(0, _potentialNames.Count)];
        Debug.Log($"{GetContext()} PlayerInitialiser::InitialiseDisplayName: generating new name: {newUsername}");
        _name.value = newUsername;
        _nakamaClient.SetDisplayname(nakamaSession, newUsername);
    }
}
