using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Match
{
    [RequireComponent(typeof(PlayerController)), RequireComponent(typeof(LineRenderer))]
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Sprite freeSprite;
        [SerializeField] private Sprite chainedSprite;
    
        [SerializeField] private TMP_Text displayName;
    
        private readonly List<IStateBinding> _stateBindings = new();

        private LineRenderer _lineRenderer;
        private PlayerController _myPlayerController;
        private PlayerController _linkedPlayerController;

        private void Awake()
        {
            _myPlayerController = GetComponent<PlayerController>();
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void RegisterStateBindings(PlayerState state)
        {
            _stateBindings.Add(new VarStateBinding<string>(state.Name, OnNameChanged));
            _stateBindings.Add(new VarStateBinding<PlayerTeam>(state.Team, OnTeamChanged));
            _stateBindings.Add(new VarStateBinding<PlayerState>(state.LinkedPlayer, OnLinkedPlayerChanged));
            OnEnable();
        }
    
        private void OnEnable()
        {
            foreach (var stateBinding in _stateBindings)
            {
                stateBinding.Bind();
            }
        }

        private void OnDisable()
        {
            foreach (var stateBinding in _stateBindings)
            {
                stateBinding.Unbind();
            }
        }

        void OnNameChanged(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                return;
            }
            Debug.Log($"PlayerView::OnNameChanged: {newName}");
            name = newName;
            displayName.text = newName;
        }

        void OnTeamChanged(PlayerTeam newTeam)
        {
            GetComponent<SpriteRenderer>().sprite = newTeam switch
            {
                PlayerTeam.ChainTeam => chainedSprite,
                PlayerTeam.FreeTeam => freeSprite,
                _ => throw new Exception($"PlayerState::OnTeamChanged: Unknown team {newTeam}")
            };
        }

        private void OnLinkedPlayerChanged(PlayerState newLinkedPlayer)
        {
            if (!newLinkedPlayer)
            {
                return;
            }
            Debug.Log($"{name}: PlayerView::OnLinkedPlayerChanged {newLinkedPlayer}");
            _linkedPlayerController = newLinkedPlayer.Body.value.GetComponent<PlayerController>();
            _lineRenderer.enabled = true;
        }

        private void Update()
        {
            if (!_linkedPlayerController)
            {
                return;
            }

            _lineRenderer.SetPosition(0, _linkedPlayerController.LeftHand.position);
            _lineRenderer.SetPosition(1, _myPlayerController.RightHand.position);
        }
    }
}
