using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MapGenerator;



public class MapGenerator : MapGeneratorAlgorithms
{
    [SerializeField]
    public TileValue[] tiles;
    public Tilemap tilemapGround;
    public Tilemap tilemapTop;

    public Chunk[,] chunks = new Chunk[0, 0];

    public int smoothIterations = 3;
    public float standardDeviation = 0.15f;  // Adjust as needed. It controls the spread of the distribution.
    public int proccessThreshhold = 25;

    void Start()
    {
        GenerateChunks();
    }

    /// <summary>
    /// Generates chunk data for the entire grid and loads the tiles into the tilemap.
    /// </summary>

    public void GenerateChunks()
    {
        // Initialize the chunks array.
        chunks = new Chunk[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                CreateChunkAtPosition(x, y);
            }
        }
        int[,] fullmap = ConcatenateChunks(chunks);
        int[,] newmap = SetOceanDepth(fullmap);
        for (int i = 0; i <= smoothIterations; i++)
        {

            newmap = BlurOceanDepth(newmap);

        }

        chunks = DivideIntoChunks(fullmap, chunkSize);

        // Load the generated chunks into the tilemap.
        LoadInTiles();
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
        
        newmap = AdjustIsolatedTiles(newmap);
        newmap = ProcessMap(newmap, TileType.Island, proccessThreshhold);
        List<List<Vector2Int>> regions = GetRegions((int)TileType.Island, newmap);
        newmap = SetGrassInRegions(regions, newmap);
        newmap = Sandbanks(regions, newmap);


        chunks[x, y] = new Chunk(pos, size, newmap);


    }

    void LoadInTiles()
    {
        for (int chunkX = 0; chunkX < gridSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < gridSize.y; chunkY++)
            {
                LoadChunkTiles(chunkX, chunkY);
            }
        }
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
                    tilemapTop.SetTile(position, tiles.FirstOrDefault(e => (int)e.Type == value).tile);
                    tilemapGround.SetTile(position, tiles[0].tile);
                    continue;
                }
                tilemapGround.SetTile(position, tiles.FirstOrDefault(e => (int)e.Type == value).tile);


            }
        }
    }
   
    
  
    

    
   
   
}


