using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;
using ParrelSync;
public class NakamaClient : MonoBehaviour
{
    public event Action<ISession> OnReady;
    public bool IsReady { get; private set; }
    public IApiUser User => _account.User;

    // in production, use https and custom key
    private const string Scheme = "http";
    private const string Host = "localhost";
    private const int Port = 7350;
    private const string ServerKey = "defaultkey";
    
    private IClient _client;
    private IApiAccount _account;
    void Start()
    {
        _client = new Client(Scheme, Host, Port, ServerKey, UnityWebRequestAdapter.Instance);
    }

    public async void Connect(string deviceId)
    {
        try
        {
            var session = await _client.AuthenticateDeviceAsync(deviceId);
            _account = await _client.GetAccountAsync(session);
            IsReady = true;
            OnReady?.Invoke(session);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Nakama failed to connect: {exception.Message}");
        }
    }

    public async Task SetDisplayname(ISession session, string newName)
    {
        if (!IsReady || session == null)
        {
            Debug.LogError("Nakama client is not ready or session is null.");
            return;
        }

        try
        {
            await _client.UpdateAccountAsync(session, User.Username, newName);
            _account = await _client.GetAccountAsync(session);
            Debug.Log($"Successfully updated display name to: {User.DisplayName}");
        }
        catch (ApiResponseException exception)
        {
            Debug.LogError($"Failed to update display name: {exception.Message}");
        }
    }
}
