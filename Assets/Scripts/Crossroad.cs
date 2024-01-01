using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class Crossroad : MonoBehaviour
{
    [SerializeField] private GameObject[] _zAxisTurnBarriers;
    [SerializeField] private GameObject[] _negativeZAxisTurnBarriers;
    [SerializeField] private GameObject[] _xAxisTurnBarriers;
    [SerializeField] private GameObject[] _negativeXAxisTurnBarriers;

    private List<Npc> _incomingVehicles = new();

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    [CanBeNull]
    public GameObject Queue(Npc arriving)
    {
        if (!_incomingVehicles.Contains(arriving))
        {
            _incomingVehicles.Add(arriving);
        }

        Vector3 diff = transform.position - arriving.transform.position;
        if (diff.x < 0)
        {
            diff.x *= -1;
        }

        if (diff.z < 0)
        {
            diff.z *= -1;
        }

        Debug.Log("Assignment transform pos: " + transform.position + " car pos: " + arriving.transform.position +
                  " result: " + diff);
        if (diff.x > diff.z)
        {
            return diff.x > 0
                ? _xAxisTurnBarriers[Random.Range(0, _xAxisTurnBarriers.Length)]
                : _negativeXAxisTurnBarriers[Random.Range(0, _negativeXAxisTurnBarriers.Length)];
        }
        else
        {
            return diff.z > 0
                ? _zAxisTurnBarriers[Random.Range(0, _zAxisTurnBarriers.Length)]
                : _negativeZAxisTurnBarriers[Random.Range(0, _negativeZAxisTurnBarriers.Length)];
        }

        //TODO Check time of arrival of existing vehicles in que and signal to slow down if close enough and intersecting
    }

    public void EnterCrossing(Npc entering)
    {
        _incomingVehicles.Remove(entering); //TODO Maybe add a busy bool state to the intersection?
    }
}