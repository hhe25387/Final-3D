using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare
}

public class Collectable : MonoBehaviour
{
    // Add a member varible to represent each collectable's rarity. Make sure
    //      it can be tuned from the Inspector.

    private Collider[] _colliders;
    private Rigidbody _rigidbody;
    private GameController _gameController;
    private Vector3 _originalScale;
    private float _collectAfterTime;

    public bool CanCollect => Time.time >= _collectAfterTime;
    public float CollisionRadius { get; private set; }
    [SerializeField] private ItemRarity _rarity;

    // ------------------------------------------------------------------------
    private void Start()
    {
        _colliders = GetComponentsInChildren<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _gameController = FindFirstObjectByType<GameController>();
        _originalScale = transform.lossyScale;

        foreach (Collider itemCollider in _colliders)
        {
            if (!itemCollider.isTrigger)
            {
                CollisionRadius = Mathf.Max(CollisionRadius,
                    Vector3.Distance(transform.position, itemCollider.bounds.center)
                    + itemCollider.bounds.extents.magnitude);
            }
        }
    }

    // ------------------------------------------------------------------------
    // You might want to add parameters to this method.
    public void Collect(Transform katamariSphere)
    {
        foreach (Collider itemCollider in _colliders)
        {
            itemCollider.enabled = false;
        }

        _rigidbody.isKinematic = true;

        transform.SetParent(katamariSphere);
        transform.position = katamariSphere.position;

        _gameController.CollectItem(_rarity);
    }

    // ------------------------------------------------------------------------
    // You might want to add parameters to this method.
    public void Drop(Vector3 dropPosition, Vector3 launchImpulse)
    {
        // Let the item leave the ball before it can be collected again.
        _collectAfterTime = Time.time + 1.5f;
        transform.SetParent(null);
        transform.localScale = _originalScale;
        transform.position = dropPosition;

        NPC npc = GetComponent<NPC>();
        if (npc != null)
        {
            npc.OnEjected();
        }

        _rigidbody.isKinematic = false;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        foreach (Collider itemCollider in _colliders)
        {
            itemCollider.enabled = true;
        }

        _rigidbody.AddForce(launchImpulse, ForceMode.Impulse);

        _gameController.RemoveItem(_rarity);
    }
}