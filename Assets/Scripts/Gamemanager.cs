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
    List<Map> generatedMaps = new List<Map>();
    MapGenerator mapGenerator;
    TileGenerator tileGenerator;
    RegionLoader regionLoader;

    List<Region> loadedRegions = new List<Region>();

    List<Region> regions = new List<Region>();

    public GameObject characterPlayer;
    public GameObject shipPlayer;
    public GameObject parkedShip;

    public Vector2Int parkedShipPos;
    public Vector2Int lastShipPos;
    public float regionLoadDistance = 3f;

    GameObject shipPlayerObj;
    GameObject characterPlayerObj;
    GameObject currentParkedShip;

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
        Map map = GenerateMap();
        SpawnShipPlayer(ChunkTools.FindWaterRandomWaterTile(map.fullMap));
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
        generatedMaps.Add(map);
        return map;
    }
    void SpawnShipPlayer(Vector2Int pos)
    {
        shipPlayerObj = Instantiate(shipPlayer, new Vector3(pos.x, pos.y), Quaternion.identity);
        camFollow.playerPos = shipPlayerObj.transform;
    }
    void SpawnCharacterPlayer(int[,] upscaledRegion, Vector2Int playerPos)
    {
        Destroy(shipPlayerObj);
        Vector2Int nearestIslandTile = ChunkTools.GetClosestTileOfType(playerPos * 4, upscaledRegion, mapGenerator.islandTiles);
        Vector2Int nextWaterTile = ChunkTools.GetClosestTileOfType(nearestIslandTile, upscaledRegion, mapGenerator.waterTiles);
        currentParkedShip = Instantiate(parkedShip, new Vector3(nextWaterTile.x, nextWaterTile.y), Quaternion.identity);
        parkedShipPos = nextWaterTile;
        characterPlayerObj = Instantiate(characterPlayer, new Vector3(nearestIslandTile.x, nearestIslandTile.y), Quaternion.identity);
        camFollow.playerPos = characterPlayerObj.transform;
    }
    public void LoadMap()
    {
        for (int i = 0; i < regionLoader.allSpawnedIslandObjects.Count; i++)
        {
            Destroy(regionLoader.allSpawnedIslandObjects[i]);
        }
        Destroy(characterPlayerObj);
        Destroy(currentParkedShip);

        SpawnShipPlayer(lastShipPos);
        Map currentMap = generatedMaps[0];
        tileGenerator.LoadFullMapTiles(new Vector2Int(currentMap.fullMap.GetLength(0), currentMap.fullMap.GetLength(1)), generatedMaps[0].fullMap);
    }
    public void GenerateRegionLoader(Vector2Int playerPos)
    {
        lastShipPos = new Vector2Int((int)shipPlayerObj.transform.position.x, (int)shipPlayerObj.transform.position.y);
        Region region = ChunkTools.GetClosestRegion(playerPos, regions, mapGenerator.islandTiles);
        tileGenerator.ClearAllTilemap();

        if (ChunkTools.GetDistanceToNearestIsland(playerPos, region.regionTiles) < regionLoadDistance)
        {
            int[,] regionMap;
            Region loadedRegion = loadedRegions.FirstOrDefault(e => e.ID == region.ID);
            if (loadedRegion == null)
            {
                regionMap = regionLoader.CreateGrid(region, fullmap, 50, 50);
                regionMap = mapGenerator.SetOceanDepth(regionMap, 2);
                region.upscaledMap = regionMap;
                
                loadedRegions.Add(region);
            }
            else
            {
                regionMap = regionLoader.PlaceTrees(loadedRegion.upscaledMap);
            }
            tileGenerator.LoadFullMapTiles(new Vector2Int(regionMap.GetLength(0), regionMap.GetLength(1)), regionMap);
            SpawnCharacterPlayer(regionMap, playerPos);
        }
    }








}