using UnityEngine;

public class ChaseCamera : CameraBase
{
    [SerializeField, Min(0f)] private float _distance = 6f;
    [SerializeField, Min(0f)] private float _height = 2f;
    [SerializeField, Min(0f)] private float _lookAtHeight = 1f;

    [SerializeField, Min(0f)] private float _rotationDamping = 5f;
    [SerializeField, Min(0f)] private float _heightDamping = 5f;

    private int _directionSign = -1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            _directionSign *= -1;
        }
    }

    private void LateUpdate()
    {
        if (_targetObject == null)
            return;

        Vector3 targetPos = _targetObject.transform.position;
        float targetAngleY = _targetObject.transform.eulerAngles.y;

        float desiredAngleY = targetAngleY;

        if (_directionSign == 1)
        {
            desiredAngleY = targetAngleY + 180f;
        }

        float newAngleY = Mathf.LerpAngle(transform.eulerAngles.y, desiredAngleY, _rotationDamping * Time.deltaTime);

        float currY = transform.position.y;
        float targetY = targetPos.y + _height;
        float newY = Mathf.Lerp(currY, targetY, _heightDamping * Time.deltaTime);

        Quaternion rot = Quaternion.Euler(0f, newAngleY, 0f);

        Vector3 camPos = targetPos + rot * Vector3.back * _distance;

        camPos.y = newY;
        transform.position = camPos;

        Vector3 lookAtPos = targetPos + Vector3.up * _lookAtHeight;

        transform.LookAt(lookAtPos);
    }
}