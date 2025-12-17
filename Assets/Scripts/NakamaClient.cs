using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;
using ParrelSync;
public class NakamaClient : MonoBehaviour
{
    public event Action OnReady;
    public bool IsReady { get; private set; }
    public IApiUser User => _account.User;

    // in production, use https and custom key
    private const string Scheme = "http";
    private const string Host = "localhost";
    private const int Port = 7350;
    private const string ServerKey = "defaultkey";
    
    private IClient _client;
    private IApiAccount _account;
    private ISession _session;
    
    private event Action _clientCreated;
    
    void Start()
    {
        DontDestroyOnLoad(this);
        _client = new Client(Scheme, Host, Port, ServerKey, UnityWebRequestAdapter.Instance);
        Debug.Log("NakamaClient started");
        _clientCreated?.Invoke();
    }

    public async void WaitForClient(string deviceId)
    {
        Debug.Log("Waiting to connect to nakama client");
        
        if (_client != null)
        {
            Debug.Log("Can do so immediately");
            await ConnectToClient(deviceId);
        }
        else
        {
            Debug.Log("Scene needs to be ready first");
            
            async void OnClientCreated()
            {
                _clientCreated -= OnClientCreated;
                await ConnectToClient(deviceId);
            }

            _clientCreated += OnClientCreated;
        }
    }

    private async Task ConnectToClient(string deviceId)
    {
        // try
        // {
        Debug.Log("Connecting to nakama client");
            if (_client == null)
            {
                throw new Exception("No Nakama client exists. Can't connect");
            }
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new Exception("Empty device ID, can't establish Nakama connection");
            }
            _session = await _client.AuthenticateDeviceAsync(deviceId);
            _account = await _client.GetAccountAsync(_session);
            IsReady = true;
            OnReady?.Invoke();
        // }
        // catch (Exception exception)
        // {
        //     Debug.LogError($"Nakama failed to connect: {exception.Message}");
        // }
    }

    public async Task SetDisplayname(string newName)
    {
        if (!IsReady || _session == null)
        {
            Debug.LogError("Nakama client is not ready or session is null.");
            return;
        }

        // try
        // {
            await _client.UpdateAccountAsync(_session, User.Username, newName);
            _account = await _client.GetAccountAsync(_session);
            Debug.Log($"Successfully updated display name to: {User.DisplayName}");
        // }
        // catch (ApiResponseException exception)
        // {
            // Debug.LogError($"Failed to update display name: {exception.Message}");
        // }
    }
}
