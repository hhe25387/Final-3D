using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    [SerializeField] private float _interactDistance = 5.0f;
    [SerializeField] private float _runDistance = 5.0f;
    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _navAgent;
    [SerializeField] private GameObject _explosionVFX;
    [SerializeField] private AudioClip _explosionSound;

    private Transform _playerTransform;
    private bool _wasCaptured;
    private bool _wasEjected;
    private bool _hasExploded;
    private float _nextEscapeUpdate;
    private NavMeshPath _escapePath;

    // ------------------------------------------------------------------------
    // You don't need to code anything in this method :)
    private void Start()
    {
        _escapePath = new NavMeshPath();
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    // ------------------------------------------------------------------------
    private void Update()
    {
        if (_playerTransform == null || !_navAgent.enabled || !_navAgent.isOnNavMesh)
        {
            _animator.SetBool("running", false);
            return;
        }

        // Keep a destination briefly instead of restarting pathfinding every frame.
        if (Time.time >= _nextEscapeUpdate)
        {
            _nextEscapeUpdate = Time.time + 0.35f;
            if (Vector3.Distance(transform.position, _playerTransform.position)
                <= _interactDistance)
            {
                if (TryGetEscapeDestination(out Vector3 destination))
                {
                    _navAgent.SetDestination(destination);
                }
            }
            else if (_navAgent.hasPath)
            {
                _navAgent.ResetPath();
            }
        }

        _animator.SetBool("running", _navAgent.velocity.sqrMagnitude > 0.01f);
    }

    private bool TryGetEscapeDestination(out Vector3 destination)
    {
        // Nonserialized native wrappers need rebuilding after an Editor script reload.
        _escapePath ??= new NavMeshPath();

        Vector3 origin = transform.position;
        Vector3 away = origin - _playerTransform.position;
        away.y = 0f;
        away = away.sqrMagnitude > 0.001f ? away.normalized : transform.forward;

        destination = origin;
        float bestScore = float.NegativeInfinity;
        bool found = false;
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = _navAgent.agentTypeID,
            areaMask = _navAgent.areaMask
        };

        // Shorter and sideways routes provide alternatives at walls and map edges.
        for (int distanceStep = 0; distanceStep < 3; distanceStep++)
        {
            float distance = Mathf.Max(1f, _runDistance / (1 << distanceStep));
            for (int directionStep = 0; directionStep < 8; directionStep++)
            {
                Vector3 direction = Quaternion.Euler(0f, directionStep * 45f, 0f) * away;
                Vector3 candidate = origin + direction * distance;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1f, filter)
                    || Mathf.Abs(hit.position.y - origin.y) > 1f
                    || Vector3.Distance(origin, hit.position) < 0.75f)
                {
                    continue;
                }

                if (!_navAgent.CalculatePath(hit.position, _escapePath)
                    || _escapePath.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                Vector3[] corners = _escapePath.corners;
                float pathLength = 0f;
                for (int i = 1; i < corners.Length; i++)
                {
                    pathLength += Vector3.Distance(corners[i - 1], corners[i]);
                }

                float score = Vector3.Distance(hit.position, _playerTransform.position)
                    - pathLength * 0.15f;
                if (score > bestScore)
                {
                    bestScore = score;
                    destination = hit.position;
                    found = true;
                }
            }
        }

        return found;
    }

    // ------------------------------------------------------------------------
    // You don't need to code anything in this method :)
    public void PickUp ()
    {
        _wasCaptured = true;
        _wasEjected = false;
        Debug.Log($"[NPC] {name} collected and stored inside the ball.", this);
        _navAgent.enabled = false;
        enabled = false;
        _animator.SetBool("trapped", true);
    }

    public void OnEjected()
    {
        _wasEjected = true;
        Debug.Log($"[NPC] {name} ejected at {transform.position}.", this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stored items are kinematic; only explode after being ejected.
        if (!_wasCaptured || !_wasEjected || _hasExploded
            || GetComponent<Rigidbody>().isKinematic)
        {
            return;
        }

        _hasExploded = true;
        Debug.Log($"[NPC] {name} exploded after hitting {collision.gameObject.name} "
            + $"at {transform.position}.", this);

        if (_explosionVFX != null)
        {
            GameObject effect = Instantiate(
                _explosionVFX, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }

        // The sound needs its own object so destroying the NPC does not cut it off.
        if (_explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(_explosionSound, transform.position);
        }

        Destroy(gameObject);
    }
}
