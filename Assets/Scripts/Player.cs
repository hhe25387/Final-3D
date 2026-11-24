using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _torqueForce;
    [SerializeField] private Rigidbody _ballRigidbody;
    [SerializeField] private Rigidbody _playerRigidbody;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jump;
    [SerializeField] private float _mouseSensitivity;

    private Transform _cameraTrans;
    private float _rotationX;
    private float _rotationY;
    private bool _isGrounded;
    private Vector3 _moveDirection;
    private bool _jumpRequested;
    private Vector3 _spawnPosition;
    private Katamari _katamari;

    private void Awake()
    {
        _spawnPosition = transform.position;
        _katamari = _ballRigidbody.GetComponent<Katamari>();
    }

    // ------------------------------------------------------------------------
    // You don't need to code anything in this method :)
    private void Start()
    {
        _playerRigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
        Cursor.lockState = CursorLockMode.Locked;
        _cameraTrans = Camera.main.transform;
    }

    // ------------------------------------------------------------------------
    private void Update()
    {
        // Rotates the player based on where the mouse is pointed.
        // You don't need to change this code.
        float mouseY = Input.GetAxis("Mouse Y");
        _rotationY += mouseY * _mouseSensitivity;
        _rotationY = Mathf.Clamp(_rotationY, -60.0f, 60.0f);

        float mouseX = Input.GetAxis("Mouse X");
        _rotationX += mouseX * _mouseSensitivity;

        _cameraTrans.localEulerAngles = new Vector3(-_rotationY, 0, 0);
        transform.localEulerAngles = new Vector3(0, _rotationX, 0);

        // Code the player's left/right movement and jump using the veritcal
        //      and horizontal input.
        // Read only the keyboard so joystick axes cannot move the player.
        float vertical = ReadKeyboardAxis(KeyCode.S, KeyCode.DownArrow,
            KeyCode.W, KeyCode.UpArrow);
        float horizontal = ReadKeyboardAxis(KeyCode.A, KeyCode.LeftArrow,
            KeyCode.D, KeyCode.RightArrow);
        _moveDirection = Vector3.ClampMagnitude(
            transform.forward * vertical + transform.right * horizontal,
            1f
        );

        _animator.SetBool("isMoving", _moveDirection.sqrMagnitude > 0.01f);

        _animator.SetBool("isGrounded", _isGrounded);

        if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _jumpRequested = true;
        }
        

        // This rotates the ball as you move.
        // You don't need to change this code.
        _ballRigidbody.AddTorque((
            ((horizontal + mouseX * _mouseSensitivity) * transform.forward) +
            (vertical * transform.right))
            * _torqueForce
        );
    }
    private static float ReadKeyboardAxis(KeyCode negative, KeyCode negativeArrow,
        KeyCode positive, KeyCode positiveArrow)
    {
        float value = 0f;
        if (Input.GetKey(negative) || Input.GetKey(negativeArrow)) value -= 1f;
        if (Input.GetKey(positive) || Input.GetKey(positiveArrow)) value += 1f;
        return value;
    }

    private void FixedUpdate()
    {
        if (_playerRigidbody.position.y < -10f)
        {
            _playerRigidbody.position = _spawnPosition;
            _playerRigidbody.linearVelocity = Vector3.zero;
            _playerRigidbody.angularVelocity = Vector3.zero;
            _moveDirection = Vector3.zero;
            _jumpRequested = false;
            _isGrounded = false;
            _animator.SetBool("isMoving", false);
            _animator.SetBool("isGrounded", false);
            _katamari.RecoverToPlayer();
            return;
        }

        Vector3 velocity = _moveDirection * _moveSpeed;
        velocity.y = _playerRigidbody.linearVelocity.y;

        if (_jumpRequested && _isGrounded)
        {
            velocity.y = _jump;
            _isGrounded = false;
        }

        _jumpRequested = false;
        _playerRigidbody.linearVelocity = velocity;
    }

    private void OnDisable()
    {
        _moveDirection = Vector3.zero;
        _jumpRequested = false;

        if (_playerRigidbody != null)
        {
            Vector3 velocity = _playerRigidbody.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            _playerRigidbody.linearVelocity = velocity;
        }

        if (_animator != null)
        {
            _animator.SetBool("isMoving", false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
            Debug.Log("Player landed");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
        }
    }

    // ------------------------------------------------------------------------
    // Utilize a COLLISION method to prevent the player from double jumping.
    // The player should only be able to jump after making contact with the ground.
}