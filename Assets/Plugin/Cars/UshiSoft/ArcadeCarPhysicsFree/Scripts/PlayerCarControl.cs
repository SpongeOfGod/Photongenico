using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Threading;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public class PlayerCarControl : DriverBase
{
    [SerializeField, Min(0.001f)] private float _steerTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _steerReleaseTime = 0.1f;

    [SerializeField, Min(0.001f)] private float _throttleTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _throttleReleaseTime = 0.1f;

    [SerializeField, Min(0.001f)] private float _brakeTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _brakeReleaseTime = 0.1f;

    [SerializeField] private bool _steerLimitByFriction = false;
    [SerializeField, Min(0f)] private float _steerMu = 2f;

    [SerializeField] private bool _autoShiftToReverse = true;
    [SerializeField, Min(0f)] private float _switchToReverseSpeedKPH = 1f;

    [SerializeField] private bool _enableVirtualPad = true;

    [SerializeField] private Transform upray;

    [SerializeField] private LayerMask drivable;

    private PhotonView _photonView;
    private bool IsRotating;
    public float timeToRotate = 0.75f;
    private float Timer;

    public bool SteerLimitByFriction { get => _steerLimitByFriction; set => _steerLimitByFriction = value; }
    public bool AutoSwitchToReverse { get => _autoShiftToReverse; set => _autoShiftToReverse = value; }
    public bool EnableVirtualPad { get => _enableVirtualPad; set => _enableVirtualPad = value; }

    protected override void Awake()
    {
        base.Awake();
        _photonView = GetComponent<PhotonView>();
    }

    protected override void Drive()
    {
        if (!_photonView.IsMine || IsRotating) return;

        UpdateSteerInput();
        UpdateThrottleAndBrakeInput();
        restartRotation();
    }

    protected override void Stop()
    {
        if (!_photonView.IsMine) return;

        _carController.BrakeInput = 1f;

        var throttleInput = GetRawThrottleInput();
        var throttleTime = throttleInput != 0f ? _throttleTime : _throttleReleaseTime;
        _carController.ThrottleInput = Mathf.MoveTowards(_carController.ThrottleInput, throttleInput, Time.deltaTime / throttleTime);
    }

    #region INPUTS
    private float GetRawSteerInput()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) return -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) return 1f;
        return 0f;
    }

    private  void restartRotation()
    {
        Debug.Log($"{transform.eulerAngles}");

        if ((transform.eulerAngles.x > 260 && transform.eulerAngles.x < 280))
            Timer += Time.deltaTime;

        if (!IsRotating && (Timer > 1))
        {
            Timer = 0;
            Debug.Log("Rotating");
            IsRotating = true;
            StartCoroutine(RotatingPlayer());
        }
    }

    private IEnumerator RotatingPlayer() 
    {
        float elapsedTime = 0;
        Vector3 initialRotation = transform.rotation.eulerAngles;
        Vector3 endRotation = initialRotation;
        endRotation.x = 0f;
        endRotation.z = 0f;
        endRotation.y = 0f;

        while (elapsedTime < timeToRotate) 
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / timeToRotate;
            transform.eulerAngles = Vector3.Lerp(initialRotation, endRotation, t);
            yield return null;
        }

        transform.eulerAngles = endRotation;
        IsRotating = false;
    }

    private float GetRawThrottleInput()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.C)) return 1f;
        return 0f;
    }

    private float GetRawBrakeInput()
    {
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.X)) return 1f;
        return 0f;
    }
    #endregion

    private void UpdateSteerInput()
    {
        var maxSteerInput = 1f;
        if (_steerLimitByFriction)
        {
            var speed = _carController.Speed;
            var minTurnR = (speed * speed) / (_steerMu * Physics.gravity.magnitude);
            if (minTurnR > 0f)
            {
                var optimalSteerAngle = Mathf.Asin(_carController.Wheelbase / minTurnR) * Mathf.Rad2Deg;
                maxSteerInput = Mathf.Min(optimalSteerAngle / _carController.MaxSteerAngle, 1f);
            }
        }

        var steerInput = GetRawSteerInput();
        steerInput = Mathf.Clamp(steerInput, -maxSteerInput, maxSteerInput);
        var steerTime = steerInput != 0f ? _steerTime : _steerReleaseTime;

        if (steerInput != 0f && Mathf.Sign(steerInput) != Mathf.Sign(_carController.SteerInput))
        {
            _carController.SteerInput = 0f;
        }

        _carController.SteerInput = Mathf.MoveTowards(_carController.SteerInput, steerInput, Time.deltaTime / steerTime);
    }

    private void UpdateThrottleAndBrakeInput()
    {
        var throttleInput = GetRawThrottleInput();
        var brakeInput = GetRawBrakeInput();

        if (_autoShiftToReverse && _carController.IsGrounded())
        {
            var speedKPH = _carController.ForwardSpeed * 3.6f;
            if (_carController.Reverse)
            {
                if (throttleInput > 0f && speedKPH > -_switchToReverseSpeedKPH) _carController.Reverse = false;
            }
            else
            {
                if (brakeInput > 0f && speedKPH < _switchToReverseSpeedKPH) _carController.Reverse = true;
            }

            if (_carController.Reverse)
            {
                (throttleInput, brakeInput) = (brakeInput, throttleInput);
            }
        }

        var throttleTime = throttleInput != 0f ? _throttleTime : _throttleReleaseTime;
        _carController.ThrottleInput = Mathf.MoveTowards(_carController.ThrottleInput, throttleInput, Time.deltaTime / throttleTime);

        var brakeTime = brakeInput != 0f ? _brakeTime : _brakeReleaseTime;
        _carController.BrakeInput = Mathf.MoveTowards(_carController.BrakeInput, brakeInput, Time.deltaTime / brakeTime);
    }
}
