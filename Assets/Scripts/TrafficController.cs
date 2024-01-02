using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private List<GameObject> npcs;

    private readonly List<Npc> _vehicles = new();
    private TerrainMap _map;

    // Start is called before the first frame update
    private void Start()
    {
        _map = terrain.GetComponent<TerrainMap>();

        foreach (GameObject npc in npcs)
        {
            _vehicles.Add(npc.GetComponent<Npc>());
        }
    }

    public void AddNpc(Npc vehicle)
    {
        _vehicles.Add(vehicle);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public Vector3 CalcDirection(Npc vehicle)
    {
        Transform vehicleTransform = vehicle.transform;
        Vector3 vehicleFrontCenter = vehicle.GetVehicleFront();

        const float directionsToCheck = 30f;

        List<Vector3> scanResults = new();

        Vector3 vehicleTransformRight = vehicleTransform.right;
        Vector3 vehicleTransformForward = vehicleTransform.forward;
        Vector3 arcStart = -vehicleTransformRight + vehicleTransformForward;
        Vector3 arcEnd = vehicleTransformRight + vehicleTransformForward; // Cone shape: \/
        
        for (int i = 0; i < directionsToCheck; i++)
        {
            // We scan left to right (i=0 means left, i=29 means right)
            // DirectionBias is from -1 (scanning '\') to 1 (scanning '/')
            float directionBias = (2 * i - (directionsToCheck - 1)) / (directionsToCheck - 1);
            Vector3 direction = arcStart * (1 + directionBias * -1) / 2 +  // 1 to 0
                                arcEnd * (1 + directionBias) / 2;  // 0 to 1
            //Debug.DrawRay(vehicleFrontCenter, direction, Color.red);
            float len = _map.CalculateRoadLength(vehicleFrontCenter, direction);
            scanResults.Add(direction * len);
        }
        
        //Get the average of all scan direction results
        Vector3 scanAverage = new();
        scanAverage = scanResults.Aggregate(scanAverage, (current, scanResult) => current + scanResult);
        scanAverage /= scanResults.Count;
        
        Debug.DrawRay(vehicleFrontCenter + scanAverage, vehicleTransform.up * 15, Color.gray);


        #region VehicleCollision

        Vector3 target = new();

        //TODO check single vehicle at the end that's the highest priority. Closest/Most easily hit.
        foreach (Npc npc in _vehicles)
        {
            float distance = Vector3.Distance(npc.transform.position, vehicleTransform.position);
            //Margin reduced due to both cars moving towards each other => margin * 2 should be max 1.0f
            if (vehicle.CanCorner(distance, 0.4f) && vehicle.CanBrakeInTime(distance, 0.4f))
            {
                //-180 to 180
                float signedAngle = Vector3.SignedAngle(vehicleTransformForward, npc.transform.forward, vehicleTransform.right);

                target += vehicleTransformForward * (distance / vehicle.Speed) +
                         vehicleTransformRight * Mathf.Cos(signedAngle); //TODO multiplication with signedAngle
            }
        }
        
        Debug.DrawRay(vehicleFrontCenter + target, vehicleTransform.up * 10, Color.green);
        
        #endregion

        Vector3 calcDirection = vehicleFrontCenter + scanAverage + target;
        Debug.DrawRay(calcDirection, vehicleTransform.up * 10, Color.red);
        
        return calcDirection; //TODO Harder road limit, right side of road bias (likely done on the TerrainMap side).
    }
}