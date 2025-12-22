using Input;
using PurrNet;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ChainUnit : NetworkIdentity
{
    public Transform RightHand;
    public Transform LeftHand;
    
    private LineRenderer _lineRenderer;

    [SerializeField]
    private SyncVar<ChainUnit> _linkedUnit;

    [ServerOnly]
    public void SetLinkedUnit(ChainUnit unit)
    {
        Debug.Log($"Server call to link {name} to {unit.name}");
        _linkedUnit.value = unit;
    }

    private void SyncLinkedUnit(ChainUnit unit)
    {
        if (unit == null)
        {
            return;
        }
        Debug.Log($"{name} is now linked to {unit.name}");
        
        _linkedUnit.value = unit;

        var springJoint = gameObject.GetComponent<SpringJoint2D>();
        springJoint.connectedBody = unit.GetComponent<Rigidbody2D>();
        springJoint.enabled = true;
        _lineRenderer.enabled = true;
    }

    protected override void OnSpawned()
    {
        Debug.Log($"{name} spawned!");
        
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        _linkedUnit.onChanged += SyncLinkedUnit;
        SyncLinkedUnit(_linkedUnit.value);
    }

    protected override void OnDestroy()
    {
        _linkedUnit.onChanged -= SyncLinkedUnit;
        base.OnDestroy();
    }

    private void Update()
    {
        if (!_linkedUnit.value)
        {
            return;
        }

        _lineRenderer.SetPosition(0,_linkedUnit.value.LeftHand.position);
        _lineRenderer.SetPosition(1, RightHand.position);
    }
}
