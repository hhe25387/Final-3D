using System.Collections.Generic;
using UnityEngine;

public class Katamari : MonoBehaviour
{
    [SerializeField] private AudioSource _equipAudioSource;
    [SerializeField] private AudioSource _bonkAudioSource;
    [SerializeField] private AudioSource _unequipAudioSource;
    [SerializeField] private float _radiusGrowAmount = 1.0f;
    [SerializeField] private Collider _katamariCollider;
    [SerializeField] private Transform _sphereTransform;
    private Rigidbody _ballRigidbody;
    private List<Collectable> _katamariCollection = new List<Collectable>();

    private void Start()
    {
        _ballRigidbody = GetComponent<Rigidbody>();
        _ballRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        SetSpherePosition();
    }

    private void FixedUpdate()
    {
        // If only the ball falls, restore it beside the player.
        // Player handles restoring both bodies when the player also falls.
        if (_ballRigidbody.position.y < -10f && transform.parent.position.y >= -10f)
        {
            RecoverToPlayer();
        }
    }

    public void RecoverToPlayer()
    {
        _ballRigidbody.linearVelocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;
        SetSpherePosition();
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        TryCollect(otherCollider.GetComponentInParent<Collectable>());
    }

    private void TryCollect(Collectable collectable)
    {
        if (collectable == null || !collectable.CanCollect ||
            _katamariCollection.Contains(collectable))
        {
            return;
        }

        Collider solidCollider = null;

        foreach (Collider candidate in collectable.GetComponentsInChildren<Collider>())
        {
            if (!candidate.isTrigger)
            {
                solidCollider = candidate;
                break;
            }
        }

        if (solidCollider == null ||
            solidCollider.bounds.extents.magnitude >=
            _katamariCollider.bounds.extents.magnitude)
        {
            return;
        }

        ResizeSphere(_radiusGrowAmount);

        _katamariCollection.Add(collectable);
        collectable.Collect(_sphereTransform);

        NPC npc = collectable.GetComponent<NPC>();
        if (npc != null)
        {
            npc.PickUp();
        }

        _equipAudioSource.Play();
    }

    // ------------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        // A solid collider can stop the ball before it reaches an item's trigger.
        Collectable item = collision.collider.GetComponentInParent<Collectable>();
        if (item != null)
        {
            TryCollect(item);
            return;
        }

        if (!collision.gameObject.CompareTag("Obstacle") ||
            _katamariCollection.Count == 0)
        {
            return;
        }

        int index = Random.Range(0, _katamariCollection.Count);
        Collectable collectable = _katamariCollection[index];
        _katamariCollection.RemoveAt(index);

        // Eject to the side, outside the ball, rather than behind the camera.
        Vector3 side = transform.parent.right;
        float clearance = _katamariCollider.bounds.extents.magnitude
            + collectable.CollisionRadius + 0.2f;
        Vector3 dropPosition = _sphereTransform.position
            + side * clearance + Vector3.up * clearance;

        Vector3 launchImpulse = Vector3.up * 4f + side * 3f;

        collectable.Drop(dropPosition, launchImpulse);

        ResizeSphere(-_radiusGrowAmount);

        _unequipAudioSource.Play();
    }

    private void ResizeSphere(float amount)
    {
        float oldScale = _sphereTransform.localScale.x;
        _sphereTransform.localScale += Vector3.one * amount;

        float newScale = _sphereTransform.localScale.x;
        float compensation = oldScale / newScale;

        // Preserve item size while the uniformly scaled sphere changes size.
        foreach (Collectable item in _katamariCollection)
        {
            if (item != null)
            {
                item.transform.localScale *= compensation;
            }
        }

        SetSpherePosition();
    }

    // Keep the collider centered on its rotating Rigidbody. Moving only the
    // child sphere makes it orbit the body and swing through the ground.
    private void SetSpherePosition()
    {
        _sphereTransform.localPosition = Vector3.zero;
        float radius = _katamariCollider.bounds.extents.y;
        Vector3 center = transform.parent.position
            + transform.parent.up * radius
            + transform.parent.forward * (radius + 1f);

        _ballRigidbody.position = center;

        // move all of the katamari objects to the new katamari center
        foreach(Collectable collectable in _katamariCollection)
        {
            if(collectable != null)
            {
                collectable.transform.position = _sphereTransform.position;   
            }
            else
            {
                Debug.LogError("null item in katamari collectables list");
            }
        }
    }
}
