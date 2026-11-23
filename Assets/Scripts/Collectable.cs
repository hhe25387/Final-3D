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
    [SerializeField] private ItemRarity _rarity;

    // ------------------------------------------------------------------------
    private void Start()
    {
    _colliders = GetComponentsInChildren<Collider>();
    _rigidbody = GetComponent<Rigidbody>();
    _gameController = FindFirstObjectByType<GameController>();
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
        transform.SetParent(null);
        transform.position = dropPosition;

        _rigidbody.isKinematic = false;
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