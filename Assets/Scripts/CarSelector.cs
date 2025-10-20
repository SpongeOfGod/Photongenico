using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelector : MonoBehaviour
{
    public List<GameObject> cars = new List<GameObject>();
    public GameObject CarToSelect;
    public string SceneToLoad;
    private PhotonView PhotonView;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (GameManager.Instance != null /*&& PhotonView.IsMine*/ && CarToSelect != null) 
        {
            GameManager.Instance.CreateNewPlayer(CarToSelect);

            Destroy(gameObject);
        }
    }

    public void SelectCar(int index) 
    {
        //if (!PhotonView.IsMine) return;

        CarToSelect = cars[index];
        //PhotonNetwork.LoadLevel(SceneToLoad);
    }
}
