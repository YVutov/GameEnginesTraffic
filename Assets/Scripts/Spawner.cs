using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab;

    [SerializeField] private float releaseDelay = 10;

    [SerializeField] private int releaseMaximum = 50;
    
    private static Terrain _terrain;
    private static TrafficController _trafficController;
    
    // Start is called before the first frame update
    private void Start()
    {
        _terrain ??= Terrain.activeTerrain;
        _trafficController ??= _terrain.GetComponent<TrafficController>();
        InvokeRepeating(nameof(Spawn), 2, releaseDelay);
    }

    private void Spawn()
    {
        if (releaseMaximum < 0)
        {
            CancelInvoke();
        }

        releaseMaximum--;
        GameObject car = Instantiate(npcPrefab, transform.position, Quaternion.identity);
    }
}
