using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class SimpleCarController : CarControllerBase, IPunObservable
{
    [Header("Animation")]
    [SerializeField] private Animator _animator;

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
    private bool hurtAnotherPlayer;

    public PhotonView _pv;

    public override bool Reverse
    {
        get => _reverse;
        set => _reverse = value;
    }

    public override float MotorRevolutionRate => _motorRPM / Mathf.Max(_maxMotorForwardRPM, _maxMotorBackwardRPM);
    public float MotorRPM => _motorRPM;

    private bool IsExceedMaxMotorRPM =>
        Mathf.Abs(_motorRPM) > (_reverse ? _maxMotorBackwardRPM : _maxMotorForwardRPM);

    public override float MaxSpeedKPH => Mathf.Max(_maxForwardSpeedKPH, _maxMotorBackwardRPM);

    protected override void Awake()
    {
        base.Awake();
        _pv = GetComponent<PhotonView>();

        _maxMotorForwardRPM = CalcMotorRPMFromSpeedKPH(_maxForwardSpeedKPH);
        _maxMotorBackwardRPM = CalcMotorRPMFromSpeedKPH(_maxBackwardSpeedKPH);
    }

    private void Start()
    {
        GameManager.Instance.leaderboardManager.CreateNewItem(this);
    }

    protected void Update()
    {
        if (_pv.IsMine)
        {
            UpdateAnimator();
        }

        if (hurtAnotherPlayer)
        {
            GameManager.Instance.leaderboardManager.PhotonView.RPC("AddScore", RpcTarget.All, 10, _pv.Owner.NickName);
            hurtAnotherPlayer = false;
        }

        if (GameManager.Instance.mashButtonManager.winner == _pv.Owner.NickName && _pv.IsMine)
        {
            GameManager.Instance.leaderboardManager.PhotonView.RPC("AddScore", RpcTarget.All, 20, _pv.Owner.NickName);
            GameManager.Instance.mashButtonManager.player1Nickname = string.Empty;
            GameManager.Instance.mashButtonManager.player2Nickname = string.Empty;
            GameManager.Instance.mashButtonManager.winner = string.Empty;
        }
    }

    protected override void FixedUpdate()
    {
        bool sameNickname = GameManager.Instance.mashButtonManager.player1Nickname == PhotonNetwork.LocalPlayer.NickName || GameManager.Instance.mashButtonManager.player2Nickname == PhotonNetwork.LocalPlayer.NickName;
        if (!_pv.IsMine)
            return;

        if (sameNickname)
            Rigidbody.velocity = Vector3.zero;

        base.FixedUpdate();
        AddDriveTorque();
    }

    [PunRPC]
    private void MadeDamage()
    {
        if (_pv.IsMine)
            hurtAnotherPlayer = true;
    }

    private void UpdateAnimator()
    {
        if (_animator == null) return;

        Vector3 localVelocity = transform.InverseTransformDirection(Rigidbody.velocity);

        float speedXZ = new Vector2(localVelocity.x, localVelocity.z).magnitude;

        _animator.SetFloat("SpeedX", speedXZ);
        bool onAir = Mathf.Abs(localVelocity.y) > 0.5f && !IsGrounded();

        _animator.SetBool("OnAir", onAir);
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

            Vector3 localVelocity = transform.InverseTransformDirection(Rigidbody.velocity);
            float speedXZ = new Vector2(localVelocity.x, localVelocity.z).magnitude;
            bool onAir = Mathf.Abs(localVelocity.y) > 0.5f && !IsGrounded();

            stream.SendNext(speedXZ);
            stream.SendNext(onAir);
        }
        else
        {
            ThrottleInput = (float)stream.ReceiveNext();
            BrakeInput = (float)stream.ReceiveNext();
            SteerInput = (float)stream.ReceiveNext();
            _reverse = (bool)stream.ReceiveNext();
            _motorRPM = (float)stream.ReceiveNext();

            float speedXZ = (float)stream.ReceiveNext();
            bool onAir = (bool)stream.ReceiveNext();

            if (_animator != null)
            {
                _animator.SetFloat("SpeedX", speedXZ);
                _animator.SetBool("OnAir", onAir);
            }
        }
    }
}