using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Apimania : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button catfactButton;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
  public void callrequest()
    {
        StartCoroutine(chuckreq());
    }

    public IEnumerator chuckreq()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.chucknorris.io/jokes/random"))
        {
            yield return www.SendWebRequest();
            if(www.result == UnityWebRequest.Result.Success)
            {
                chuckdata data = JsonUtility.FromJson<chuckdata>(www.downloadHandler.text);
                print(www.downloadHandler.text);
                text.text = data.value;
            }
        }



    }

    
}
[System.Serializable]
public class chuckdata
{
    public string id;
    public string value;
}
