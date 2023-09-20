using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UIElements;

public class MapGenerator : MapGeneratorAlgorithms
{
    bool mapIsLoaded = false;
    Chunk[,] chunks = new Chunk[0, 0];
    public Transform playerPos;

    List<List<Vector2Int>> allRegions = new List<List<Vector2Int>>();
    int[,] fullmap;

    [Header("Advanced Settings")]

    [Tooltip("Steps in Cellular Automata")]
    public int smoothIterations = 3;
    
    [Header("Tilemap")]
    [SerializeField]
    public TileValue[] tiles;
    public Tilemap tilemapGround;
    public Tilemap tilemapTop;

    [Header("TileGroups")]
    public TileType[] grassTypes = { (TileType)3, (TileType)5, (TileType)6 };
    public TileType[] flowerTypes;
    public TileType[] islandTiles;

    public RegionLoader regionLoader;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && mapIsLoaded)
        {
            // Find all region centers
            List<Vector2Int> regionCenters = new List<Vector2Int>();
            foreach (var region in allRegions)
            {
                regionCenters.Add(GetRegionCenter(region));
            }

            // Identify the closest region to the player that's within a certain distance
            Vector2Int? closestRegionCenter = GetClosestRegionToPlayer(regionCenters, playerPos.position);

            if (closestRegionCenter.HasValue)
            {
                // Get the actual region based on its center
                List<Vector2Int> closestRegion = allRegions.FirstOrDefault(region => GetRegionCenter(region) == closestRegionCenter.Value);

                if (closestRegion != null)
                {
                    // Extract the map data for this region
                    int[,] regionMap = regionLoader.CreateGrid(closestRegion, islandTiles, fullmap, chunkSize.x, chunkSize.y);

                    // Upscale the region for more details
                    int upscaleFactor = 4;  // Using 4 as the upscale factor
                    int[,] upscaledRegion = regionLoader.UpscaleGrid(regionMap, upscaleFactor);
                    upscaledRegion = SetOceanDepth(upscaledRegion);
                    // Upscale player's position
                    Vector2Int upscaledPlayerTile = new Vector2Int((int)playerPos.position.x, (int)playerPos.position.y);

                    // Find nearest valid tile in upscaled region for the player
                    Vector2Int nearestTile = GetClosestIslandTile(upscaledPlayerTile *upscaledRegion.Length,upscaledRegion,islandTiles);

                    // Assign the upscaled region for display
                    regionLoader.fullmap = upscaledRegion;
                    regionLoader.regionSize = chunkSize * upscaleFactor;
                    regionLoader.tiles = tiles;
                    regionLoader.mapLoaded = true;
                    regionLoader.playerSpawnTile = nearestTile;  // Adjusted to the nearest tile in the upscaled map

                    SceneManager.LoadScene("SampleScene");
                }
                // ...
               





            }

        }
      
    }


    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        playerPos = GameObject.FindGameObjectWithTag("Player").transform;
        GenerateChunks();
        //placeCam.pos = new Vector2Int((gridSize.x * chunkSize.x) / 2, (gridSize.y * chunkSize.y)/2+30);
        //placeCam.GetComponent<Camera>().orthographicSize = (chunkSize.y * gridSize.y)/2+30;
        mapIsLoaded = true;
    }

    /// <summary>
    /// Generates chunk data for the entire grid and loads the tiles into the tilemap.
    /// </summary>

    public void GenerateChunks()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        mapIsLoaded = false;
        // Initialize the chunks array.
        chunks = new Chunk[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                CreateChunkAtPosition(x, y);
            }
        }
        int[,] newmap = ConcatenateChunks(chunks);
        newmap = SetOceanDepth(newmap);
        allRegions = GetRegions(newmap, new Vector2Int(gridSize.x * chunkSize.x,gridSize.y * chunkSize.y),islandTiles);
        fullmap = newmap;
        //for (int i = 0; i < 2; i++)
        //{
        //    newmap = BlurOceanDepthWithNoise(newmap, noiseScale, noiseThreshold);
        //}
        chunks = DivideIntoChunks(newmap, chunkSize, chunks);

       
        // Load the generated chunks into the tilemap.
        LoadInTiles();
        sw.Stop();
        elapsedMsText.text = sw.ElapsedMilliseconds + " ms\n" + "ChunkSize: " + chunkSize.x+"X"+chunkSize.y+"\n"+"GridSize: "+gridSize.x+"X"+gridSize.y;
        statsText.text = "waterfill: " + waterFillPercent+"\n"+"islandSizeThreshold: " + proccessThreshhold;
            
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds + "ms");
    }

    void CreateChunkAtPosition(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        Vector2Int size = chunkSize; // Directly using chunkSize since it's already a Vector2Int.

        // Create a new chunk using the position, size, and a randomly filled map.
        int[,] newmap = FillMapRandom(size);

        for (int i = 0; i <= 3; i++)
        {
            newmap = SmoothMap(size, newmap);
        }


        newmap = ProcessMap(newmap, TileType.Island, proccessThreshhold);
        List<List<Vector2Int>> regions = GetRegions(newmap, chunkSize, islandTiles);
        newmap = SetGrassInRegions(regions, newmap,flowerTypes, grassTypes);
        newmap = Sandbanks(regions, newmap);

        chunks[x, y] = new Chunk(pos, size, newmap, regions);
    }

    void LoadInTiles()
    {
        tilemapGround.ClearAllTiles();
        tilemapTop.ClearAllTiles();
        for (int chunkX = 0; chunkX < gridSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < gridSize.y; chunkY++)
            {
                LoadChunkTiles(chunkX, chunkY);
            }
        }
        mapIsLoaded = true;
    }

    void LoadChunkTiles(int chunkX, int chunkY)
    {
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                // Calculate the global position of the tile.
                Vector3Int position = new Vector3Int(chunkX * chunkSize.x + x, chunkY * chunkSize.y + y, 0);

                // Get the tile type from the chunk's map and set the appropriate tile.
                int value = chunks[chunkX, chunkY].map[x, y];

                if (flowerTypes.Contains((TileType)value))
                {

                    tilemapGround.SetTile(position, tiles[0].tile);
                    continue;
                }
                tilemapGround.SetTile(position, tiles.FirstOrDefault(e => (int)e.Type == value).tile);
                foreach(var region in chunks[chunkX, chunkY].regions)
                {
                    foreach(var tile in region)
                    {
                        position = new Vector3Int(chunkX * chunkSize.x + tile.x, chunkY * chunkSize.y + tile.y, 0);
                        tilemapTop.SetTile(position, tiles[4].tile);
                    }
                }
               
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Loop through all chunks.
        for (int chunkX = 0; chunkX < gridSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < gridSize.y; chunkY++)
            {
                // Get the starting position of the current chunk.
                Vector3 startPosition = new Vector3(chunkX * chunkSize.x, chunkY * chunkSize.y, 0);

                // Calculate the four corners of the chunk.
                Vector3 topLeft = startPosition;
                Vector3 topRight = startPosition + new Vector3(chunkSize.x, 0, 0);
                Vector3 bottomRight = startPosition + new Vector3(chunkSize.x, chunkSize.y, 0);
                Vector3 bottomLeft = startPosition + new Vector3(0, chunkSize.y, 0);

                // Draw the borders using Gizmos.
                Gizmos.color = UnityEngine.Color.red;  // Setting the color to red for visualization.

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }

        if (mapIsLoaded)
        {
        
            foreach (var region in allRegions)
                    {
                        foreach (var tile in region)
                        {
                            if (IsBoundaryTile(tile, fullmap, new Vector2Int(chunkSize.x * gridSize.x,gridSize.y * chunkSize.y),islandTiles))
                            {
                                // Convert tile position to world position
                                Vector3 worldPos = new Vector3(tile.x + 0.5f,
                                                               tile.y + 0.5f,
                                                               0);
                                Gizmos.color = UnityEngine.Color.blue;  // Setting the color to blue for region borders.
                                Gizmos.DrawCube(worldPos, Vector3.one * 0.5f);
                            }
                        }
                    }
                }
            }

        }
    



