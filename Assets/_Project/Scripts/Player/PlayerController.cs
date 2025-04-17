using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerController
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private ScriptableStats _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;

    [Header("States")]
    public bool facingRight;
    public bool disableMove;

    #region Interface

    public Vector2 FrameDirection => _frameVelocity;
    public float HorizontalInput => _frameInput.Horizontal;
    public event Action<bool, float> GroundedChanged;
    public event Action<bool> Jumped;
    public event Action Dashed;

    #endregion

    private float _time;

    private void Awake()
    {
        _timeJumpWasPressed = -_stats.JumpBuffer;

        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
    }

    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            Horizontal = Input.GetAxisRaw("Horizontal"),
            JumpDown = Input.GetKeyDown(KeyCode.Space),
            JumpHeld = Input.GetKey(KeyCode.Space),
            DashDown = Input.GetKeyDown(KeyCode.LeftShift),
        };
        if (_stats.SnapInput)
            _frameInput.Horizontal = Mathf.Abs(_frameInput.Horizontal) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Horizontal);

        if (disableMove)
        {
            _frameInput.Horizontal = 0;
            _frameInput.JumpDown = false;
            _frameInput.JumpHeld = false;
        }

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        if (_frameInput.DashDown)
            _dashToConsume = true;
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleFlip();
        HandleDash();
        HandleGravity();

        ApplyMovement();
    }

    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        Vector2 scale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size * scale, _col.direction, 0, Vector2.down, _stats.GrounderDistance, _stats.GroundLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size * scale, _col.direction, 0, Vector2.up, _stats.GrounderDistance, _stats.GroundLayer);

        if (!inDash) _frameVelocity = _rb.linearVelocity;
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion

    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;
    private bool afterDoubleJump;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;
        if (_grounded) afterDoubleJump = false;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote) ExecuteJump();
        else if (!afterDoubleJump) ExecuteJump(true);

        _jumpToConsume = false;
    }

    private void ExecuteJump(bool doubleJump = false)
    {
        if (inDash) DashAbort();
        if (doubleJump) afterDoubleJump = true;

        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = !doubleJump ? _stats.JumpPower : _stats.DoubleJumpPower;
        Jumped?.Invoke(doubleJump);
    }

    #endregion

    #region Horizontal

    private void HandleDirection()
    {
        if (_frameInput.Horizontal == 0)
        {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Horizontal * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleFlip()
    {
        if (inDash) return;
        if (!(_frameInput.Horizontal > 0 && !facingRight) && !(_frameInput.Horizontal < 0 && facingRight)) return;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        facingRight = !facingRight;
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            float inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Dash

    private bool _dashToConsume;
    private bool inDash;
    private bool afterDash;
    private float dashDelay;
    private float dashDir;
    private Coroutine executeDash;

    private void HandleDash()
    {
        dashDelay -= Time.deltaTime;
        if (_grounded) afterDash = false;

        if (_dashToConsume && !afterDash && dashDelay < 0) executeDash = StartCoroutine(ExecuteDash());
        _dashToConsume = false;
    }

    private IEnumerator ExecuteDash()
    {
        dashDelay = _stats.DashDelay + _stats.DashDuration;
        inDash = true;
        afterDash = true;
        dashDir = facingRight ? 1 : -1;
        Dashed?.Invoke();

        float elapsedTime = 0f;
        while (elapsedTime < _stats.DashDuration)
        {
            float power = _stats.DashPower * _stats.DashCurve.Evaluate(elapsedTime / _stats.DashDuration);
            power *= dashDir;
            _frameVelocity.x = power;
            _frameVelocity.y = 0;

            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        inDash = false;
    }

    private void DashAbort()
    {
        if (executeDash != null) StopCoroutine(executeDash);
        inDash = false;
    }

    #endregion

    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;
}

public struct FrameInput
{
    public float Horizontal;
    public bool JumpDown;
    public bool JumpHeld;
    public bool DashDown;
}

public interface IPlayerController
{
    public Vector2 FrameDirection { get; }
    public float HorizontalInput { get; }

    public event Action<bool, float> GroundedChanged;
    public event Action<bool> Jumped;
    public event Action Dashed;
}