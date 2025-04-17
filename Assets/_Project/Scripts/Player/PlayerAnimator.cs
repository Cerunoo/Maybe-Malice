using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // [SerializeField] private ScriptableStats stats;

    // private Animator _anim;
    private IPlayerController _player;
    // private bool _grounded;

    private void Awake()
    {
        // _anim = GetComponent<Animator>();
        _player = GetComponentInParent<IPlayerController>();
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
    }

    private void Update()
    {
        HandleMovement();
        HandleCharacterTilt();
        HandleGravity();
    }

    private void HandleMovement()
    {
        // if (_player.FrameDirection.y <= -(stats.MaxFallSpeed / 2.2f))
        // {
        //     _anim.SetBool(SpeedKey, false);
        //     return;
        // }

        // var inputStrength = Mathf.Abs(_player.FrameDirection.x);
        // _anim.SetBool(SpeedKey, inputStrength > 0);
        // _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
    }

    private void HandleCharacterTilt()
    {
        // var runningTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x) : Quaternion.identity;
        // _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, runningTilt * Vector2.up, _tiltSpeed * Time.deltaTime, 0f);
    }

    // private bool fallFast;
    private void HandleGravity()
    {
        //     if (FindAnyObjectByType<PlayerController>().GetGround(7) == false)
        //     {
        //         jumpIn = false;
        //     }

        //     if (_player.FrameDirection.y < stats.GroundingForce)
        //         _anim.SetTrigger(FallKey);
        //     else
        //         _anim.ResetTrigger(FallKey);

        //     if (_player.FrameDirection.y <= -(stats.MaxFallSpeed / 2.2f) && !fallFast && !jumpIn)
        //     {
        //         _anim.SetTrigger(FallFastKey);
        //         _anim.ResetTrigger(FallKey);
        //         _anim.SetBool(SpeedKey, false);
        //         fallFast = true;
        //     }
    }

    // private bool jumpIn;
    private void OnJumped(bool doubleJump)
    {
        // _source.PlayOneShot(jump);
        // _anim.SetTrigger(JumpKey);
        // _anim.ResetTrigger(GroundedKey);
        // jumpIn = true;

        // if (_grounded) // Avoid coyote
        // {
        //     // _jumpParticles.Play();
        // }
    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        // _grounded = grounded;

        // if (grounded)
        // {
        //     if (!fallFast) _source.PlayOneShot(land);
        //     _anim.SetTrigger(GroundedKey);
        //     if (fallFast) _source.PlayOneShot(riseup);
        //     fallFast = false;
        //     jumpIn = false;
        //     // _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
        //     // _moveParticles.Play();

        //     // _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
        //     // _landParticles.Play();
        // }
        // else
        // {
        //     // _moveParticles.Stop();
        // }
    }

    // private static readonly int SpeedKey = Animator.StringToHash("Speed");
    // private static readonly int JumpKey = Animator.StringToHash("Jump");
    // private static readonly int FallKey = Animator.StringToHash("Fall");
    // private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    // private static readonly int FallFastKey = Animator.StringToHash("FallFast");
}