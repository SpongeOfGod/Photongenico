using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] List<Transform> PositionsToSpawn;

    [SerializeField] List<GameObject> PlayerstoSpawn;
    private float timeToSpawn = 3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
