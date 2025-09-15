using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class SimpleCarController : CarControllerBase, IPunObservable
{
    [SerializeField, Min(0f)] private float _maxForwardSpeedKPH = 180f;
    [SerializeField, Min(0f)] private float _maxBackwardSpeedKPH = 60f;
    [SerializeField, Min(0f)] private float _maxMotorTorque = 300f;
    [SerializeField, Min(0f)] private float _minMotorFrictionTorque = 15f;
    [SerializeField, Min(0f)] private float _maxMotorFrictionTorque = 75f;
    [SerializeField, Min(0.001f)] private float _motorInertia = 0.1f;
    [SerializeField, Min(0f)] private float _finalGearRatio = 8f;

    private float _maxMotorForwardRPM;
    private float _maxMotorBackwardRPM;

    private float _motorRPM;
    private bool _reverse;

    private PhotonView _pv;

    public override bool Reverse
    {
        get => _reverse;
        set => _reverse = value;
    }

    public override float MotorRevolutionRate => _motorRPM / Mathf.Max(_maxMotorForwardRPM, _maxMotorBackwardRPM);
    public float MotorRPM => _motorRPM;

    private bool IsExceedMaxMotorRPM =>
        Mathf.Abs(_motorRPM) > (_reverse ? _maxMotorBackwardRPM : _maxMotorForwardRPM);

    public override float MaxSpeedKPH => Mathf.Max(_maxForwardSpeedKPH, _maxBackwardSpeedKPH);

    protected override void Awake()
    {
        base.Awake();
        _pv = GetComponent<PhotonView>();

        _maxMotorForwardRPM = CalcMotorRPMFromSpeedKPH(_maxForwardSpeedKPH);
        _maxMotorBackwardRPM = CalcMotorRPMFromSpeedKPH(_maxBackwardSpeedKPH);
    }

    protected override void FixedUpdate()
    {
        if (!_pv.IsMine) return;
        base.FixedUpdate();
        AddDriveTorque();
    }

    private void AddDriveTorque()
    {
        var throttleInput = ThrottleInput;
        if (IsExceedMaxMotorRPM)
            throttleInput = 0f;

        if (IsGrounded())
        {
            _motorRPM = CalcMotorRPMFromSpeedKPH(SpeedKPH);
            var motorTorque = GetMotorTorque() * throttleInput;
            var motorFriTorque = GetMotorFrictionTorque() * (1f - throttleInput);

            var driveTorque = motorTorque * _finalGearRatio;
            var friTorque = motorFriTorque * _finalGearRatio;

            AddDriveTorque(driveTorque);
            AddBrakeTorque(friTorque);
        }
        else
        {
            var motorTorque = GetMotorFrictionTorque() * throttleInput;
            var motorFiTorque = GetMotorFrictionTorque() * (1f - throttleInput);
            var totalBrakeTorque = MaxBrakeTorque * BrakeInput * Wheels.Length;

            var driveTorque = motorTorque * _finalGearRatio;
            var drivetrainI = _finalGearRatio * _finalGearRatio * _motorInertia;

            var friTorque = motorFiTorque * _finalGearRatio;
            var brakeTorque = totalBrakeTorque * _finalGearRatio;

            _motorRPM += (driveTorque / drivetrainI) * Time.fixedDeltaTime * RPSToRPM;
            DecelerateMotor(friTorque, drivetrainI);
            DecelerateMotor(brakeTorque, drivetrainI);
        }
    }

    public float CalcMotorRPMFromSpeedKPH(float speedKPH)
    {
        float speedMPS = speedKPH / 3.6f;
        float wheelRPS = speedMPS / (2f * Mathf.PI * _wheelRadius);
        float engineRPS = wheelRPS * _finalGearRatio;
        return engineRPS * 60f;
    }

    private float GetMotorTorque()
    {
        if (IsExceedMaxMotorRPM) return 0f;

        var revRate = Mathf.Clamp01(MotorRevolutionRate);
        var coef = 1f;

        if (revRate >= 0.5f)
        {
            coef = (1f - revRate) * 2f;
            coef *= coef;
        }

        var sign = _reverse ? -1f : 1f;
        return sign * _maxMotorTorque * coef;
    }

    private float GetMotorFrictionTorque()
    {
        var motorRevRate = MotorRevolutionRate;
        return Mathf.Lerp(_minMotorFrictionTorque, _maxMotorFrictionTorque, motorRevRate * motorRevRate);
    }

    private void DecelerateMotor(float torque, float inertia)
    {
        var acc = -Mathf.Sign(_motorRPM) * (torque / inertia) * Time.fixedDeltaTime * RPSToRPM;
        if (Mathf.Abs(acc) > _motorRPM)
            _motorRPM = 0f;
        else
            _motorRPM += acc;
    }

    private const float RPSToRPM = 60f;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ThrottleInput);
            stream.SendNext(BrakeInput);
            stream.SendNext(SteerInput);
            stream.SendNext(_reverse);
            stream.SendNext(_motorRPM);
        }
        else
        {
            ThrottleInput = (float)stream.ReceiveNext();
            BrakeInput = (float)stream.ReceiveNext();
            SteerInput = (float)stream.ReceiveNext();
            _reverse = (bool)stream.ReceiveNext();
            _motorRPM = (float)stream.ReceiveNext();
        }
    }
}
