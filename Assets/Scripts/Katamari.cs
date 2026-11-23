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
    private List<Collectable> _katamariCollection = new List<Collectable>();

    // Add a member variable to represent a LIST of all of the Collectable 
    //      objects we've collected named _katamariCollection.
    // Then, uncomment the code in SetSpherePosition().

    // ------------------------------------------------------------------------
    // There's probably a member variable you need to declare in a built-in
    //      Unity method that runs once, before Update starts...
    

    // ------------------------------------------------------------------------
    private void OnTriggerEnter(Collider otherCollider)
    {
        Collectable collectable =
            otherCollider.GetComponentInParent<Collectable>();

        if (collectable == null ||
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

        _sphereTransform.localScale += Vector3.one * _radiusGrowAmount;
        SetSpherePosition();

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
        if (!collision.gameObject.CompareTag("Obstacle") ||
            _katamariCollection.Count == 0)
        {
            return;
        }

        int index = Random.Range(0, _katamariCollection.Count);
        Collectable collectable = _katamariCollection[index];
        _katamariCollection.RemoveAt(index);

        Vector3 dropPosition = _sphereTransform.position
            + Vector3.up * (_katamariCollider.bounds.extents.y + 1f);

        Vector3 launchImpulse = Vector3.up * 5f
            - transform.parent.forward * 2f;

        collectable.Drop(dropPosition, launchImpulse);

        _sphereTransform.localScale -= Vector3.one * _radiusGrowAmount;
        SetSpherePosition();

        _unequipAudioSource.Play();
    }

    // ------------------------------------------------------------------------
    // Uncomment this method by removing the /* and */ AFTER you've added the
    //      _katamariCollection member variable.
    // You don't need to code anything else in this method :)
    // This method sets up the sphere to be in the corret position in front
    //      of the player, and ensures all of the objects inside the katamari
    //      remain in its center.
    private void SetSpherePosition ()
    {
        // place the sphere in front of the player
        _sphereTransform.position = transform.parent.position +
                transform.parent.up * _sphereTransform.lossyScale.x/2.0f +
                transform.parent.forward * (_sphereTransform.lossyScale.x/2.0f + 1.0f);

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
