using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovementController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementStats movementStats;

    public event Action OnRotation, OnInitiateJump, OnLand, OnBumpHead, OnStartFalling;
    
    private Rigidbody2D _rb;
    private PlayerInputController _inputController;
    private PlayerCollisionController  _collisionController;

    private Vector2 _velocity;
    
    private bool _isFacingRight = true;
    
    private bool _isJumping;
    private bool _isFalling;
    
    private bool _isFastFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    
    private bool _isPastApexThreshold;
    private float _timePastApexThreshold;
    
    private bool _jumpReleasedDuringBuffer;
    private float _jumpBufferTimer;
    
    private float _jumpCoyoteTimer;
    
    private void  Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inputController = GetComponent<PlayerInputController>();
        _collisionController = GetComponent<PlayerCollisionController>();
        _inputController.OnMove += TryToRotate;
    }

    private void OnEnable()
    {
        _inputController.OnJumpPressed += OnJumpPressed;
        _inputController.OnJumpReleased += OnJumpReleased;
    }

    private void Update()
    {
        if (!IsOwner) return;
        CheckJumpBuffer();
        CheckLanding();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Jump();
        
        _rb.linearVelocity = new Vector2(_velocity.x, _rb.linearVelocity.y);
        
        if(_collisionController.IsGrounded)
            Move(movementStats.Acceleration, movementStats.Deceleration, _inputController.Movement);
        else
            Move(movementStats.AirAcceleration, movementStats.AirDeceleration, _inputController.Movement);
    }

    private void OnDisable()
    {
        _inputController.OnJumpPressed -= OnJumpPressed;
        _inputController.OnJumpReleased -= OnJumpReleased;
    }
    
    #region Movement
    
    private void Move(float acceleration, float deceleration, Vector2 movement)
    {
        if (movement != Vector2.zero)
        {
            float targetVelocityX = movement.x* movementStats.MaxSpeed;
            _velocity.x = Mathf.Lerp(_velocity.x, targetVelocityX, acceleration * Time.deltaTime);
        }
        else
        {
            _velocity.x = Mathf.Lerp(_velocity.x, 0f, deceleration * Time.deltaTime);
        }
        _rb.linearVelocityX = _velocity.x;
    }
    
    #endregion

    #region Jump

    private void OnJumpPressed()
    {
        _jumpBufferTimer = movementStats.JumpBufferTime;
        _jumpReleasedDuringBuffer = false;
    }

    private void OnJumpReleased()
    {
        if(_jumpBufferTimer > 0)
            _jumpReleasedDuringBuffer = true;

        if (_isJumping && _velocity.y > 0)
        {
            if (_isPastApexThreshold)
            {
                _isPastApexThreshold = false;
                _isFastFalling = true;
                _fastFallTime = movementStats.TimeForUpwardsCancel;
                _velocity.y = 0;
            }
            else
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = _velocity.y;
            }
        }
    }

    private void CheckJumpBuffer()
    {
        if (_jumpBufferTimer <= 0f) return;
        
        if(!_isJumping && (_collisionController.IsGrounded || _jumpCoyoteTimer > 0f))
        {
            InitiateJump();
            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = _velocity.y;
            }
        }
    }

    private void CheckLanding()
    {
        if ((_isJumping || _isFalling) && _collisionController.IsGrounded && _velocity.y <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _velocity.y = 0f;
            OnLand?.Invoke();
        }
    }

    private void InitiateJump()
    {
        if(!_isJumping)
            _isJumping = true;
        
        _jumpBufferTimer = 0f;
        _velocity.y = movementStats.InitialJumpVelocity;
        OnInitiateJump?.Invoke();
    }

    private void Jump()
    {
        if (_isJumping)
        {
            if (_collisionController.BumpedHead)
            {
                _isFastFalling = true;
            }

            if (_velocity.y >= 0f)
            {
                float apexPoint = Mathf.InverseLerp(movementStats.InitialJumpVelocity, 0f, _velocity.y);

                if (apexPoint > movementStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < movementStats.ApexHangTime)
                            _velocity.y = 0f;
                        else
                            _velocity.y = -.01f;
                    }
                }
                else
                {
                    _velocity.y += movementStats.Gravity * Time.fixedDeltaTime;
                    if(_isPastApexThreshold)
                        _isPastApexThreshold = false;
                }
            }
            else if (!_isFastFalling)
            {
                _velocity.y += movementStats.Gravity * movementStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
            } 
            else if (_velocity.y < 0f)
            {
                if (!_isFalling)
                {
                    StartFalling();
                }
            }
        }
        
        if (_isFastFalling)
        {
            if(_fastFallTime >= movementStats.TimeForUpwardsCancel)
            {
                _velocity.y += movementStats.Gravity * movementStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < movementStats.TimeForUpwardsCancel)
            {
                _velocity.y = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime/movementStats.TimeForUpwardsCancel));
            }
            
            _fastFallTime += Time.fixedDeltaTime;
        }

        if (!_collisionController.IsGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                StartFalling();
            }
            
            _velocity.y += movementStats.Gravity * Time.fixedDeltaTime;
        }
        
        _velocity.y = Mathf.Clamp(_velocity.y, -movementStats.MaxFallSpeed, 50f);
        _rb.linearVelocityY = _velocity.y;
    }

    private void StartFalling()
    {
        _isFalling = true;
        OnStartFalling?.Invoke();
    }
    
    #endregion
    
    #region Rotation

    private void TryToRotate(Vector2 movement)
    {
        if(_isFacingRight && movement.x < 0)
            Rotate(false);
        else if (!_isFacingRight && movement.x > 0)
            Rotate(true);
    }
    
    private void Rotate(bool faceRight)
    {
        _isFacingRight = faceRight;
        if (_isFacingRight)
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        else
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        OnRotation?.Invoke();
    }
    #endregion
    
}