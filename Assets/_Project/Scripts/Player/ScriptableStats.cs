using UnityEngine;

[CreateAssetMenu]
public class ScriptableStats : ScriptableObject
{
    [Header("LAYERS")]
    public LayerMask GroundLayer;

    [Header("INPUT")]
    public bool SnapInput = true;
    public float HorizontalDeadZoneThreshold = 0.1f;

    [Header("MOVEMENT")]
    public float MaxSpeed = 14;
    public float airMultiplier = 1.5f;
    public float Acceleration = 120;
    public float GroundDeceleration = 60;
    public float AirDeceleration = 30;
    public float GroundingForce = -1.5f;
    public float GrounderDistance = 0.05f;

    [Header("DASH")]
    public float DashPower = 14;
    public AnimationCurve DashCurve;
    public float DashDuration = 0.15f;
    public float DashDelay = 0.5f;

    [Header("JUMP")]
    public float JumpPower = 36;
    public float DoubleJumpPower = 28;
    public float MaxFallSpeed = 40;
    public float FallAcceleration = 110;
    public float JumpEndEarlyGravityModifier = 3;
    public float CoyoteTime = .15f;
    public float JumpBuffer = .2f;
}