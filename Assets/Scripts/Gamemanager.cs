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
    GameObject MapGeneratorObject;
    Queue<Map> generatedMaps = new Queue<Map>();
    MapGenerator mapGenerator;
    TileGenerator tileGenerator;
    RegionLoader regionLoader;

    List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

    public GameObject characterPlayer;
    public GameObject shipPlayer;

    GameObject shipPlayerObj;
    GameObject characterPlayerObj;

    shipCamFollow camFollow;
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
      Map map =  GenerateMap();
        SpawnShipPlayer(map.fullMap);
    }
    Map GenerateMap()
    {
        camFollow = Camera.main.GetComponent<shipCamFollow>();

        MapGeneratorObject = Instantiate(MapGeneratorPrefab);
        mapGenerator = MapGeneratorObject.GetComponent<MapGenerator>();
        tileGenerator = MapGeneratorObject.GetComponent<TileGenerator>();
        regionLoader = MapGeneratorObject.GetComponent<RegionLoader>();
        Map map = mapGenerator.GenerateMap();
        regions = map.regions;
        fullmap = map.fullMap;
        tileGenerator.LoadFullMapTiles(new Vector2Int(map.fullMap.GetLength(0), map.fullMap.GetLength(1)), map.fullMap);
        tileGenerator.SetColliderTiles(regions);
        generatedMaps.Enqueue(map);
        return map;
    }
    void SpawnShipPlayer(int[,] map)
    {
        Vector2Int randomWaterPos = ChunkTools.FindWaterRandomWaterTile(map);
        shipPlayerObj = Instantiate(shipPlayer, new Vector3(randomWaterPos.x, randomWaterPos.y), Quaternion.identity);
        camFollow.playerPos = shipPlayerObj.transform;
    }
    void SpawnCharacterPlayer(int[,] upscaledRegion, Vector2Int playerPos)
    {
        Destroy(shipPlayerObj);
        Vector2Int nearestIslandTile = ChunkTools.GetClosestIslandTile(playerPos*4, upscaledRegion, mapGenerator.islandTiles);
        characterPlayerObj = Instantiate(characterPlayer, new Vector3(nearestIslandTile.x,nearestIslandTile.y), Quaternion.identity);
        camFollow.playerPos = characterPlayerObj.transform;
    }
    public void GenerateRegionLoader(Vector2Int playerPos)
    {
        List<Vector2Int> region = ChunkTools.GetClosestRegion(playerPos, regions,mapGenerator.islandTiles);
        tileGenerator.ClearAllTilemap();
        int[,] regionMap = regionLoader.CreateGrid(region,fullmap,50,50);
        regionMap = mapGenerator.SetOceanDepth(regionMap);
        tileGenerator.LoadFullMapTiles(new Vector2Int(regionMap.GetLength(0), regionMap.GetLength(1)),regionMap);
        SpawnCharacterPlayer(regionMap,playerPos);
    }
    







}