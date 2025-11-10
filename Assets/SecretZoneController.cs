using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretZoneController : MonoBehaviourPunCallbacks, IPunObservable
{
    [System.Serializable]
    public struct Garage 
    {
        public GameObject GameObject;
        public TriggerObject trigger;
    }
    public enum Mode { Inactive, active}
    public float timeToWait = 10f;
    public Mode currentMode;
    public float InitialY = -9.95765f;
    public float EndY = -21f;
    public float pressSpeed = 0.5f;
    public List<GroundButton> groundButtons = new List<GroundButton>();
    public List<Garage> garageDoors = new();
    public List<Collider> colliders = new ();
    public List<Vector3> rewardsPosition = new();
    public List<ItemBoxController> rewards = new();
    public ItemBoxController rewardPrefab;
    public bool SecretActive;
    private GameObject currentGarage;
    void Update()
    {
        if (GameManager.Instance.GameState != GameManager.GameStates.InRound) return;

        for (int i = rewards.Count - 1; i >= 0; i--)
        {
            if (rewards.Count == 0) break;

            if (rewards[i] == null)
                rewards.RemoveAt(i);
            continue;
        }


        switch (currentMode) 
        {
            case Mode.Inactive:

                bool allActive = true;

                foreach (var item in garageDoors)
                {
                    var newPos = item.GameObject.transform.localPosition;
                    newPos.y = InitialY;
                    item.GameObject.transform.localPosition = Vector3.Lerp(item.GameObject.transform.localPosition, newPos, pressSpeed);
                }

                if (rewards.Count != 6) 
                {
                    foreach (var item in rewardsPosition)
                    {
                        var gameobj = PhotonNetwork.Instantiate(rewardPrefab.name, rewardsPosition[Random.Range(0, rewardsPosition.Count)], Quaternion.identity);
                        rewards.Add(gameobj.GetComponent<ItemBoxController>());
                    }
                }

                foreach (var item in colliders)
                    item.enabled = true;

                foreach (GroundButton groundButton in groundButtons) 
                    if (groundButton.pressMode != GroundButton.PressMode.pressed)
                        allActive = false;

                if (allActive) 
                {
                    foreach (var item in colliders)
                        item.enabled = false;

                    currentMode = Mode.active;
                    currentGarage = garageDoors[Random.Range(0, garageDoors.Count)].GameObject;

                    foreach (var item in garageDoors)
                    {
                        if (item.GameObject == currentGarage)
                        {
                            item.trigger.LastTimeInside = Time.time;
                        }
                    }
                }
                break;


            case Mode.active:
                    var playerPos = currentGarage.transform.localPosition;
                    playerPos.y = EndY;
                    currentGarage.transform.localPosition = Vector3.Lerp(currentGarage.transform.localPosition, playerPos, pressSpeed);

                foreach (var item in garageDoors) 
                {
                    if (item.GameObject == currentGarage && !item.trigger.Inside && item.trigger.LastTimeInside < Time.time - timeToWait) 
                    {
                        currentMode = Mode.Inactive;
                    }
                }
                break;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        foreach (var item in garageDoors) 
        {
            if (stream.IsWriting)
                 stream.SendNext(item.GameObject.transform.localPosition);
            else if (stream.IsReading)
                item.GameObject.transform.localPosition = (Vector3)stream.ReceiveNext();
        }
    }
}
