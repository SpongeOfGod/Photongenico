using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashInfo : MonoBehaviour
{
    private string message;
    private void Start() => DontDestroyOnLoad(this);
    public void SetMessage(string message) => this.message = message;
    void Update()
    {
        if (ErrorHandler.Instance != null) 
        {
            ErrorHandler.Instance.HandleErrorMessage(message);
            Debug.Log(message + "SENT!");
            Destroy(gameObject);
        }
    }
}
