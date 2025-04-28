using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private float boredDelay;

    [SerializeField, Space(5)] private float _maxTilt = 5;
    [SerializeField] private float _tiltSpeed = 20;

    private Animator _anim;
    private IPlayerController _player;
    private bool _grounded;

    private float _time;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _player = GetComponentInParent<IPlayerController>();
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.DashedChanged += OnDashedChanged;
        _player.Disturb += OnDisturb;
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.DashedChanged -= OnDashedChanged;
        _player.Disturb -= OnDisturb;
    }

    private void Update()
    {
        _time += Time.deltaTime;

        HandleIdle();
        HandleMovement();
        HandleCharacterTilt();
        HandleGravity();
    }

    private void HandleIdle()
    {
        if (_time >= boredDelay)
        {
            if (Random.Range(0, 2) == 0)
                _anim.SetTrigger(Bored1Key);
            else
                _anim.SetTrigger(Bored2Key);
            _time = 0;
        }
    }
    private void OnDisturb() => _time = 0;

    private void HandleMovement()
    {
        float input = Mathf.Abs(_player.HorizontalInput);
        if (!_player.SprintMove)
        {
            _anim.SetBool(RunKey, false);
            _anim.SetBool(WalkKey, input > 0);
        }
        else
        {
            _anim.SetBool(WalkKey, false);
            _anim.SetBool(RunKey, input > 0);
        }
    }

    private void HandleCharacterTilt()
    {
        float rot = _maxTilt * _player.HorizontalInput;
        if (_player.SprintMove) rot *= _player.Stats.SprintMultiplier;
        Quaternion runningTilt = _grounded ? Quaternion.Euler(0, 0, rot) : Quaternion.identity;
        _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, runningTilt * Vector2.up, _tiltSpeed * Time.deltaTime, 0f);
    }

    private bool veryFall;
    private void HandleGravity()
    {
        if (_player.FrameDirection.y < _player.Stats.GroundingForce)
            _anim.SetTrigger(FallKey);
        else
            _anim.ResetTrigger(FallKey);

        if (_player.FrameDirection.y <= -_player.Stats.MaxVeryFallSpeed)
        {
            if (!veryFall)
            {
                _anim.SetTrigger(VeryFallKey);
                _anim.ResetTrigger(GroundedKey);
            }
            _anim.ResetTrigger(FallKey);
            _anim.SetBool(WalkKey, false);
            _anim.SetBool(RunKey, false);
            veryFall = true;
        }
    }

    private void OnJumped(bool doubleJump)
    {
        if (!doubleJump) _anim.SetTrigger(JumpKey);
        else _anim.SetTrigger(DoubleJumpKey);
        _anim.ResetTrigger(GroundedKey);
    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;

        if (grounded)
        {
            _anim.SetTrigger(GroundedKey);
            _anim.ResetTrigger(FallKey);
            _anim.ResetTrigger(VeryFallKey);
            veryFall = false;
        }
    }

    private void OnDashedChanged(bool inDash)
    {
        if (inDash) _anim.SetTrigger(DashedKey);
        else
        {
            _anim.SetTrigger(UnDashKey);
            if (_grounded) _anim.SetTrigger(GroundedKey);
        }
    }

    private static readonly int WalkKey = Animator.StringToHash("Walk");
    private static readonly int RunKey = Animator.StringToHash("Run");
    private static readonly int Bored1Key = Animator.StringToHash("Bored1");
    private static readonly int Bored2Key = Animator.StringToHash("Bored2");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int DoubleJumpKey = Animator.StringToHash("DoubleJump");
    private static readonly int FallKey = Animator.StringToHash("Fall");
    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int VeryFallKey = Animator.StringToHash("VeryFall");
    private static readonly int DashedKey = Animator.StringToHash("Dashed");
    private static readonly int UnDashKey = Animator.StringToHash("UnDash");
}