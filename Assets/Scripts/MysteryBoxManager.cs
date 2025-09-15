using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MysteryBoxManager : MonoBehaviour
{
    public GameObject ItemBoxPrefab;
    public Transform ItemHolder;
    public Transform PossiblePositions;
    public float timeToSpawn = 7f;
    public float maxNumberOfItemsActive = 4;

    private List<GameObject> items = new List<GameObject>();
    private float elapsedTime = 0;
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || GameManager.Instance.GameState != GameManager.GameStates.InRound)
        {
            elapsedTime = 0;
            return;
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            for (int i = items.Count - 1; i >= 0 ; i--)
            {
                if (items[i] == null)
                    items.RemoveAt(i);
            }

            if (ItemHolder.childCount < maxNumberOfItemsActive)
            {
                SpawnItemBoxes();
            }
        }
    }

    private void SpawnItemBoxes()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= timeToSpawn)
        {
            int rdmIndex = 0;
            bool RepeatedPosition = true;
            while (RepeatedPosition)
            {
                RepeatedPosition = false;
                System.Random random = new System.Random();

                rdmIndex = random.Next(PossiblePositions.childCount);

                for (int i = 0; i < items.Count; i++)
                {
                    if (rdmIndex < PossiblePositions.childCount && items[i].transform.position == PossiblePositions.GetChild(rdmIndex).transform.position)
                    {
                        RepeatedPosition = true;
                    }
                }

                if (!RepeatedPosition)
                    break;
            }

            var itemBox = PhotonNetwork.Instantiate/*RoomObject*/(ItemBoxPrefab.name, PossiblePositions.GetChild(rdmIndex).transform.position, Quaternion.identity);
            itemBox.transform.parent = ItemHolder;
            elapsedTime = 0;
            items.Add(itemBox);
        }
    }
}
