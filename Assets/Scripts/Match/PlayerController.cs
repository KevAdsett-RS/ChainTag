using Input;
using PurrNet;
using UnityEngine;

namespace Match
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : NetworkIdentity
    {
        public Transform RightHand;
        public Transform LeftHand;
    
        [SerializeField]
        public PlayerState State { get; private set; }

        private readonly SyncVar<Vector3> _initialPosition = new();

        private void Awake()
        {
            _initialPosition.onChanged += OnInitialPositionChanged;
        }
        protected override void OnDestroy()
        {
            _initialPosition.onChanged -= OnInitialPositionChanged;
            base.OnDestroy();
        }

        [ServerOnly]
        public void Server_LinkState(PlayerState state)
        {
            State = state;
            Observers_LinkState(state.PlayerId.value, state);
            ListenToStateChanges();
        }

        [ObserversRpc]
        private void Observers_LinkState(PlayerID targetId, PlayerState state)
        {
            State = state;
            ListenToStateChanges();
            InitialiseVisuals();
            InitialiseMovement();
        }

        private void ListenToStateChanges()
        {
            if (!State)
            {
                Debug.LogError("PlayerController::ListenToStateChanges: No State to listen to");
                return;
            }

            State.LinkedPlayer.onChanged += OnLinkedPlayerChanged;
        }

        private void OnLinkedPlayerChanged(PlayerState linkedPlayer)
        {
            Debug.LogError($"PlayerController::OnLinkedPlayerChanged: {State.Name.value} is now linked to {linkedPlayer.Name.value}");
            if (!linkedPlayer || !this)
            {
                return;
            }
            var springJoint = GetComponent<SpringJoint2D>();
            springJoint.connectedBody = linkedPlayer.Body.value.GetComponent<Rigidbody2D>();
            springJoint.enabled = true;
        }

        private void InitialiseMovement()
        {
            Debug.Log($"PlayerController::InitialiseMovement: isOwner: {isOwner}");
            if (!isOwner)
            {
                return;
            }

            gameObject.AddComponent<PlayerInputHandler>();
            GetComponent<PlayerMovement>().enabled = true;
        }

        private void InitialiseVisuals()
        {
            Debug.Log($"PlayerController::InitialiseVisuals for {State.Name.value}");
            GetComponent<PlayerView>().RegisterStateBindings(State);
        }

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (!isServer || !State.IsHead.value)
            {
                return;
            }

            if (!other.gameObject.TryGetComponent<PlayerController>(out var otherController))
            {
                return;
            }
        
            if (otherController.State && otherController.State.Team.value != PlayerTeam.ChainTeam)
            {
                otherController.State.Server_ChangeTeam(PlayerTeam.ChainTeam);
            }
        }

        [ServerOnly]
        public void Server_SetInitialPosition(Vector3 transformPosition)
        {
            _initialPosition.value = transformPosition;
        }
    
        private void OnInitialPositionChanged(Vector3 newValue)
        {
            Debug.Log($"PlayerController::OnInitialPositionChanged ({name}): {newValue}");
            transform.position = newValue;
        }

    }
}
