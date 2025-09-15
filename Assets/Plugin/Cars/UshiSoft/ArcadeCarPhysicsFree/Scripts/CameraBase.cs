using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBase : MonoBehaviour
{
    [SerializeField] protected GameObject _targetObject;

    public virtual GameObject TargetObject
    {
        get => _targetObject;
        set => _targetObject = value;
    }
}
