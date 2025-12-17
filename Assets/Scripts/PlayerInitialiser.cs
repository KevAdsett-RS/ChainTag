// using System;
// using System.Collections.Generic;
// using Input;
// using Nakama;
// using PurrNet;
// using TMPro;
// using UnityEngine;
//
// public class PlayerInitialiser : NetworkIdentity
// {
//     private PlayerState _state;
//     private void Awake()
//     {
//         _state = GetComponent<PlayerState>();
//         _state.Name.onChanged += OnNameChanged;
//     }
//
//     protected override void OnDestroy()
//     {
//         Debug.Log($"{GetContext()} PlayerInitialiser::OnDestroy");
//         _state.Name.onChanged -= OnNameChanged;
//     }
//     protected override void OnSpawned()
//     {
//     }
//
//
//     private void OnNameChanged(string newValue)
//     {
//         Debug.Log($"{GetContext()} PlayerInitialiser::OnNameChanged: {newValue}");
//         name = _state.Name.value;
//         var usernameText = transform.Find("Canvas/Username").GetComponent<TextMeshProUGUI>(); 
//         usernameText.text = name;
//     }
//
//     private string GetContext()
//     {
//         return isServer ? "Server" : "Client";
//     }
//
// }
