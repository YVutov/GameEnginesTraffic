using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TerrainMap : MonoBehaviour
{
    public enum TerrainType
    {
        Plain,
        Road,
        Crossroad
    }

    [field: SerializeField] public float Granularity { get; } = 0.1f;
    [SerializeField] private Terrain terrain;
    [SerializeField] private bool loadSavedMap = true;
    [SerializeField] private bool drawDebugRays = true;

    private Dictionary<Vector3, TerrainType> Map { get; } = new();

    private List<List<TerrainType>> terrainTypeGrid { get; } = new();

    private void SaveList()
    {
        File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "listMap.txt"));
        using StreamWriter file = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "listMap.txt"));
        file.Flush();
        foreach (List<TerrainType> list in terrainTypeGrid)
        {
            for (int i = 0; i < list.Count; i++)
            {
                file.Write(list[i]);
                if (i < list.Count - 1)
                {
                    file.Write(';');
                }
            }

            file.WriteLine();
        }

        file.Close();
    }

    private void LoadList()
    {
        using StreamReader file = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "listMap.txt"));
        bool cont = true;
        while (cont)
        {
            string line = file.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                cont = false;
            }
            else
            {
                terrainTypeGrid.Add(new List<TerrainType>());
                string[] split = line.Split(';');
                foreach (string s in split)
                {
                    terrainTypeGrid.Last().Add(Enum.Parse<TerrainType>(s));
                }
            }
        }

        file.Close();
    }

    public float CalculateRoadLength(Vector3 startPos, Vector3 direction)
    {
        TerrainType terrainType = Get(startPos);
        float distance = 0;

        Vector3 pos = startPos;
        Vector3 step = new(direction.x, 0, direction.z);
        step.Normalize();
        step *= Granularity;
        
        // TODO check for bounds or try-catch bound errors
        while (terrainType is not TerrainType.Plain)
        {
            distance += Granularity;
            pos += step;
            terrainType = Get(pos);
        }

        return distance;
    }

    public bool IsRoadOrCrossroad(Vector3 position)
    {
        TerrainMap.TerrainType terrainType = Get(position);
        bool isRoadOrCrossroad = terrainType is TerrainMap.TerrainType.Road or TerrainMap.TerrainType.Crossroad;
        return isRoadOrCrossroad;
    }

    private TerrainType Get(Vector3 vec)
    {
        int x = (int)Math.Round(vec.x / Granularity);
        int z = (int)Math.Round(vec.z / Granularity);
        Vector3 intVec = new(x, 0, z);
        TerrainType terrainType = this.terrainTypeGrid[x][z];
        //Debug.Log("Vector: " + vec + "Intvec: " + intVec + " index x: " + x + " index z: " + z + "Terrain; " + terrainType);
        //Debug.DrawRay(intVec, Vector3.up * 10, Color.white);
        return terrainType;
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (loadSavedMap)
        {
            LoadList();
        }
        else
        {
            ListOutTerrain();
            SaveList();
        }

        Debug.Log(terrainTypeGrid.Count + " - " + terrainTypeGrid[0].Count);
    }

    private void ListOutTerrain()
    {
        Vector3 terrainPos = terrain.GetPosition();
        int cellsHorizontal = (int)(terrain.terrainData.size.x / Granularity);
        int cellsVertical = (int)(terrain.terrainData.size.z / Granularity);
        for (int x = 0; x < cellsHorizontal; x++)
        {
            terrainTypeGrid.Add(new List<TerrainType>());
            for (int z = 0; z < cellsVertical; z++)
            {
                Vector3 location = terrainPos + new Vector3(x * Granularity, 5, z * Granularity);

                RaycastHit[] hits = Physics.RaycastAll(location, terrain.transform.up * -1);

                bool isRoad = false;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.gameObject.CompareTag("Road"))
                    {
                        Debug.Log("Road found " + hit.transform.position);
                        if (!isRoad)
                        {
                            terrainTypeGrid[x].Add(TerrainType.Road);
                            isRoad = true;
                        }
                        else
                        {
                            Debug.Log("Crossroad found " + hit.transform.position);
                            terrainTypeGrid[x][terrainTypeGrid[x].Count - 1] = TerrainType.Crossroad;
                            break;
                        }
                    }
                }

                if (!isRoad)
                {
                    terrainTypeGrid[x].Add(TerrainType.Plain);
                }
            }
        }
    }

    private void Update()
    {
        if (drawDebugRays)
        {
            for (int i = 0; i < terrainTypeGrid.Count; i++)
            {
                for (int k = 0; k < terrainTypeGrid[i].Count; k++)
                {
                    switch (terrainTypeGrid[i][k])
                    {
                        case TerrainType.Plain:
                            Debug.DrawRay(new Vector3(i, 0, k) * Granularity, Vector3.up, Color.blue);
                            break;
                        case TerrainType.Road:
                            Debug.DrawRay(new Vector3(i, 0, k) * Granularity, Vector3.up, Color.yellow);
                            break;
                        case TerrainType.Crossroad:
                            Debug.DrawRay(new Vector3(i, 0, k) * Granularity, Vector3.up, Color.green);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}