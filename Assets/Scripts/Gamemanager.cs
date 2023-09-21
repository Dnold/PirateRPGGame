using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ToolExtensions;
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public GameObject MapGeneratorPrefab;
    public GameObject MapGeneratorObject;
    Queue<Map> generatedMaps = new Queue<Map>();
    public MapGenerator mapGenerator;
    public TileGenerator tileGenerator;
    public RegionLoader regionLoader;

    List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
    int[,] fullmap;
    #region Singleton

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion
    public void Start()
    {
        MapGeneratorObject = Instantiate(MapGeneratorPrefab);
        mapGenerator = MapGeneratorObject.GetComponent<MapGenerator>();
        tileGenerator = MapGeneratorObject.GetComponent<TileGenerator>();
        regionLoader = MapGeneratorObject.GetComponent<RegionLoader>();
        Map map = mapGenerator.GenerateMap();
        tileGenerator.LoadFullMapTiles(new Vector2Int(map.fullMap.GetLength(0), map.fullMap.GetLength(1)), map.fullMap);
        tileGenerator.SetColliderTiles(map.regions);
        regions = map.regions;
        fullmap = map.fullMap;
        generatedMaps.Enqueue(map);

    }
    public void GenerateRegionLoader(Vector2Int playerPos)
    {
        List<Vector2Int> region = ChunkTools.GetClosestRegion(playerPos, regions,mapGenerator.islandTiles);
        tileGenerator.ClearAllTilemap();
        int[,] regionMap = regionLoader.CreateGrid(region,fullmap,50,50);
        tileGenerator.LoadFullMapTiles(new Vector2Int(regionMap.GetLength(0), regionMap.GetLength(1)),regionMap);
    }
    







}