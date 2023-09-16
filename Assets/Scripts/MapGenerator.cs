using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ToolExtensions
{
    public static float NextGaussianFloat(float mean, float standardDeviation)
    {
        System.Random r = new System.Random();
        float u1 = 1.0f - (float)r.NextDouble();
        float u2 = 1.0f - (float)r.NextDouble();
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        float randNormal = mean + standardDeviation * (float)randStdNormal;
        return Mathf.Clamp(randNormal, 0f, 1f);  // Ensuring the result stays between 0 and 1
    }
}
public enum TileType
{
    Island = 0,
    Water = 1,
    Sand = 2,
    SmallGrass = 3,
    Empty = 4,
    MediumGrass = 5,
    BigGrass = 6,
    SmallFlowerRed = 7,
    SmallFlowerYellow = 8,
    MediumFlowerRed = 9,
    MediumFlowerYellow = 10,


    // ... add other types if needed
}
public class MapGenerator : MonoBehaviour
{
    public Tilemap tilemapGround;
    public Tilemap tilemapTop;
    public Tile islandTile;
    public Tile waterTile;
    public Tile sandTile;

    public Tile smallGrass;
    public Tile mediumGrass;
    public Tile bigGrass;

    public Tile smallFlowerRed;
    public Tile smallFlowerYellow;
    public Tile mediumFlowerRed;
    public Tile mediumFlowerYellow;

    [SerializeField]

    public TileType[] grassTypes = { (TileType)3, (TileType)5, (TileType)6 };
    public TileType[] flowerTypes;

    public Chunk[,] chunks;

    public Vector2Int gridSize;
    public Vector2Int chunkSize;

    public bool useRandom;
    public string seed;

    public float waterFillPercent = 50;
    public float flowerFillPercent = 1;

    public int smoothIterations = 3;
    public float standardDeviation = 0.15f;  // Adjust as needed. It controls the spread of the distribution.

    public class World
    {

    }
    public class Chunk
    {

        public Vector2Int center;
        public int[,] map;
        public Vector2Int size;
        public List<List<Vector2Int>> regions { get; set; }

        public Chunk(Vector2Int _center, Vector2Int _size, int[,] _map/*, List<List<Vector2Int>> _regions*/)
        {
            center = _center;
            map = _map;
            size = _size;
            //regions = _regions;
        }

    }
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

        // Load the generated chunks into the tilemap.
        LoadInTiles();
    }

    /// <summary>
    /// Creates a new chunk at the specified grid position.
    /// </summary>
    /// <param name="x">X coordinate in the grid.</param>
    /// <param name="y">Y coordinate in the grid.</param>
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
        newmap = ProcessMap(newmap, TileType.Island, 25);
        List<List<Vector2Int>> regions = GetRegions((int)TileType.Island, newmap);
        newmap = SetGrassInRegions(regions, newmap);
        newmap = Sandbanks(regions, newmap);

        chunks[x, y] = new Chunk(pos, size, newmap);
    }

    /// <summary>
    /// Fills a 2D map of the given size with random values based on waterFillPercent.
    /// </summary>
    /// <param name="size">The size of the map to be generated.</param>
    /// <returns>A 2D int array representing the generated map.</returns>
    int[,] FillMapRandom(Vector2Int size)
    {
        int[,] newMap = new int[size.x, size.y];


        if (useRandom)
        {
            seed = Time.time.ToString() + UnityEngine.Random.Range(0, 2000);
        }

        System.Random rand = new System.Random(seed.GetHashCode());

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (IsBorder(x, y, size))
                {
                    newMap[x, y] = 1;
                }
                else
                {
                    // Generate a value based on a normal distribution around waterFillPercent.
                    float generatedValue = ToolExtensions.NextGaussianFloat(waterFillPercent, standardDeviation);
                    newMap[x, y] = generatedValue < waterFillPercent ? 1 : 0;
                }
            }
        }

        return newMap;
    }

    /// <summary>
    /// Determines if a given cell is on the border of the map.
    /// </summary>
    /// <param name="x">X coordinate of the cell.</param>
    /// <param name="y">Y coordinate of the cell.</param>
    /// <param name="size">Size of the map.</param>
    /// <returns>True if the cell is on the border, false otherwise.</returns>
    bool IsBorder(int x, int y, Vector2Int size)
    {
        return x == 0 || y == 0 || x == size.x - 1 || y == size.y - 1;
    }
    /// <summary>
    /// Loads tiles into the tilemap based on the chunks' map data.
    /// </summary>
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

    /// <summary>
    /// Loads tiles for a specific chunk into the tilemap.
    /// </summary>
    /// <param name="chunkX">X coordinate of the chunk.</param>
    /// <param name="chunkY">Y coordinate of the chunk.</param>
    void LoadChunkTiles(int chunkX, int chunkY)
    {
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                // Calculate the global position of the tile.
                Vector3Int position = new Vector3Int(chunkX * chunkSize.x + x, chunkY * chunkSize.y + y, 0);

                // Get the tile type from the chunk's map and set the appropriate tile.
                switch (chunks[chunkX, chunkY].map[x, y])
                {
                    case 1:
                        tilemapGround.SetTile(position, waterTile);
                        break;
                    case 2:
                        tilemapGround.SetTile(position, sandTile);
                        break;
                    case 3:
                        tilemapGround.SetTile(position, islandTile);
                        tilemapTop.SetTile(position, smallGrass);
                        break;
                    case 4:
                        tilemapGround.SetTile(position, new Tile());
                        break;
                    case 5:
                        tilemapGround.SetTile(position, islandTile);
                        tilemapTop.SetTile(position, mediumGrass);
                        break;
                    case 6:
                        tilemapGround.SetTile(position, islandTile);
                        tilemapTop.SetTile(position, bigGrass);
                        break;
                    case 7:
                        tilemapGround.SetTile(position, islandTile);
                        tilemapTop.SetTile(position, smallFlowerRed);
                        break;
                    case 8:
                        tilemapGround.SetTile(position, islandTile);
                        tilemapTop.SetTile(position, smallFlowerYellow);
                        break;
                    case 9:
                        tilemapGround.SetTile(position, islandTile);
                        tilemapTop.SetTile(position, mediumFlowerRed);
                        break;
                    case 10:
                        tilemapGround.SetTile(position, islandTile);
                        tilemapTop.SetTile(position, mediumFlowerYellow);
                        break;

                    default:
                        tilemapGround.SetTile(position, islandTile);
                        break;
                }
            }
        }
    }
    int[,] SmoothMap(Vector2Int size, int[,] map)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int neighbourCount = GetNeighbourCount(x, y, map);

                if (neighbourCount > 4)
                {
                    map[x, y] = 1;  // Convert to the target tile type
                }
                else if (neighbourCount < 4)
                {
                    // You might want to define another TileType as the "dead" type or have a way to determine it. 
                    // For simplicity, I'm setting it to Empty.
                    map[x, y] = (int)TileType.Island;
                }
                // If neighbourCount is exactly 4, the cell remains unchanged.
            }
        }
        return map;
    }

    /// <summary>
    /// Counts the number of alive neighbors for a cell.
    /// </summary>
    /// <param name="gridX">X coordinate of the cell.</param>
    /// <param name="gridY">Y coordinate of the cell.</param>
    /// <param name="map">The map data to check.</param>
    /// <returns>The number of alive neighbors.</returns>
    int GetNeighbourCount(int gridX, int gridY, int[,] map)
    {
        int count = 0;

        // Directions representing the 8 neighboring cells
        int[] dirX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dirY = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int i = 0; i < 8; i++)
        {
            int neighbourX = gridX + dirX[i];
            int neighbourY = gridY + dirY[i];

            // If the neighbor is within map boundaries, check its status
            if (IsInMapRange(neighbourX, neighbourY))
            {
                count += map[neighbourX, neighbourY];
            }
            else
            {
                // Out-of-boundary neighbors are considered alive to create a solid boundary.
                count++;
            }
        }

        return count;
    }

    int[,] SmoothMapInRegion(List<Vector2Int> region, int[,] map, TileType targetTile)
    {
        // Iterate only over the tiles in the specified region
        foreach (Vector2Int tile in region)
        {
            int x = tile.x;
            int y = tile.y;

            int neighbourCount = GetNeighbourCountInRegion(x, y, map, targetTile, region);

            if (neighbourCount > 3)
            {
                map[x, y] = (int)targetTile;  // Convert to the target tile type
            }
            else if (neighbourCount < 3)
            {
                map[x, y] = 0; // Set to Empty for simplicity
            }
            // If neighbourCount is exactly 4, the cell remains unchanged.
        }
        return map;
    }
    int GetNeighbourCountInRegion(int x, int y, int[,] map, TileType targetType, List<Vector2Int> region)
    {
        int count = 0;

        int[] dirX = { -1, 0, 1, 0 };
        int[] dirY = { 0, -1, 0, 1 };

        for (int i = 0; i < 4; i++)
        {
            int neighbourX = x + dirX[i];
            int neighbourY = y + dirY[i];
            Vector2Int neighbourTile = new Vector2Int(neighbourX, neighbourY);

            if (region.Contains(neighbourTile) && map[neighbourX, neighbourY] == (int)targetType)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Checks if the given coordinates are within the map's boundaries.
    /// </summary>
    /// <param name="x">X coordinate to check.</param>
    /// <param name="y">Y coordinate to check.</param>
    /// <returns>True if the coordinates are within boundaries, false otherwise.</returns>
    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < chunkSize.x && y >= 0 && y < chunkSize.y;
    }
    int[,] ProcessMap(int[,] map, TileType tileType, int threshold)
    {
        List<List<Vector2Int>> regions = GetRegions((int)tileType, map);

        foreach (var region in regions)
        {
            if (region.Count < threshold)
            {
                foreach (Vector2Int tile in region)
                {
                    map[tile.x, tile.y] = (int)TileType.Water; //convert the tiles to Water if they're below the threshold
                }
            }
        }
        return map;
    }
    List<List<Vector2Int>> GetRegions(int tileType, int[,] map)
    {
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        int[,] mapFlags = new int[chunkSize.x, chunkSize.y];
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Vector2Int> newRegion = GetRegionTiles(x, y, map);
                    regions.Add(newRegion);
                    foreach (Vector2Int tile in newRegion)
                    {
                        mapFlags[tile.x, tile.y] = 1;
                    }
                }
            }
        }
        return regions;
    }
    List<Vector2Int> GetRegionTiles(int startX, int startY, int[,] map)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        int[,] mapFlags = new int[chunkSize.x, chunkSize.y];
        TileType currentTileType = (TileType)map[startX, startY];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        mapFlags[startX, startY] = 1;

        int[] dirX = { 0, 0, 1, -1 };
        int[] dirY = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            Vector2Int tile = queue.Dequeue();
            tiles.Add(tile);

            for (int i = 0; i < 4; i++)
            {
                int neighbourX = tile.x + dirX[i];
                int neighbourY = tile.y + dirY[i];

                if (IsInMapRange(neighbourX, neighbourY) && mapFlags[neighbourX, neighbourY] == 0)
                {
                    // If flood-filling a water region we encounter Sand or Grass,treat them as barriers/boundaries
                    if (currentTileType == TileType.Water &&
                       (map[neighbourX, neighbourY] == (int)TileType.Sand))
                    {
                        continue;
                    }

                    if (map[neighbourX, neighbourY] == (int)currentTileType)
                    {
                        mapFlags[neighbourX, neighbourY] = 1;
                        queue.Enqueue(new Vector2Int(neighbourX, neighbourY));
                    }
                }
            }
        }
        return tiles;
    }
    List<Vector2Int> GetRegionTilesOfType(int startX, int startY, int[,] map, TileType targetTileType)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        int[,] mapFlags = new int[map.GetLength(0), map.GetLength(1)];
        int tileType = map[startX, startY];

        if (tileType != (int)targetTileType)
            return tiles;  // Return an empty list if the tile type does not match the target tile type

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        mapFlags[startX, startY] = 1;

        int[] dirX = { 0, 0, 1, -1 };
        int[] dirY = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            Vector2Int tile = queue.Dequeue();
            tiles.Add(tile);

            for (int i = 0; i < 4; i++)
            {
                int neighbourX = tile.x + dirX[i];
                int neighbourY = tile.y + dirY[i];

                Vector2Int neighbour = new Vector2Int(neighbourX, neighbourY);

                if (IsInMapRange(neighbourX, neighbourY, map) && mapFlags[neighbourX, neighbourY] == 0 && map[neighbourX, neighbourY] == tileType)
                {
                    mapFlags[neighbourX, neighbourY] = 1;
                    queue.Enqueue(neighbour);
                }
            }
        }
        return tiles;

    }
    /// <summary>
    /// Finds the region of a specific tile type within a larger region.
    /// </summary>
    /// <param name="targetTileType">The tile type you are looking for.</param>
    /// <param name="largerRegion">The larger region within which to search.</param>
    /// <param name="map">The map containing the regions.</param>
    /// <returns>A list of tiles that represent the found sub-region.</returns>
    List<List<Vector2Int>> FindAllSubRegionsInLargerRegion(List<Vector2Int> largerRegion, int[,] map, TileType targetTileType)
    {
        // To keep track of tiles that have been processed
        HashSet<Vector2Int> processedTiles = new HashSet<Vector2Int>();

        List<List<Vector2Int>> subRegions = new List<List<Vector2Int>>();

        foreach (Vector2Int tile in largerRegion)
        {
            if (!processedTiles.Contains(tile) && map[tile.x, tile.y] == (int)targetTileType)
            {
                List<Vector2Int> subRegion = GetRegionTilesOfType(tile.x, tile.y, map, targetTileType);

                subRegions.Add(subRegion);

                foreach (Vector2Int subTile in subRegion)
                {
                    processedTiles.Add(subTile);
                }
            }
        }

        return subRegions;
    }
    bool IsInMapRange(int x, int y, int[,] map)
    {
        return x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
    }
    int[,] SetGrassInRegions(List<List<Vector2Int>> regions, int[,] map)
    {
        foreach (var region in regions)
        {

            foreach (Vector2Int tile in region)
            {
                // Check if the current tile is an island tile
                if (map[tile.x, tile.y] == 0) // assuming 0 is island
                {
                    // For every second island tile, check with randomness
                    if (UnityEngine.Random.Range(0f, 100f) >= 40f)
                    {
                        map[tile.x, tile.y] = 3; // 3 for grass
                    }
                }
            }
            map = SmoothMapInRegion(region, map, TileType.SmallGrass);
            List<List<Vector2Int>> subRegions = FindAllSubRegionsInLargerRegion(region, map, TileType.SmallGrass);
            foreach (var subRegion in subRegions)
            {
                map = RandomizeGrassTypeInRegion(subRegion, map);
                map = PopulateGrassWithFlowers(subRegion, map);
            }


        }
        return map;
    }
    public int[,] RandomizeGrassTypeInRegion(List<Vector2Int> region, int[,] map)
    {
        if (grassTypes.Contains((TileType)map[region[0].x, region[0].y]))
        {
            int rand = Random.Range(0, grassTypes.Length);
            foreach (var tile in region)
            {
                map[tile.x, tile.y] = (int)grassTypes[rand];
            }
        }
        return map;
    }
    public int[,] PopulateGrassWithFlowers(List<Vector2Int> region, int[,] map)
    {
        if (grassTypes.Contains((TileType)map[region[0].x, region[0].y]))
        {

            int rand = Random.Range(0, flowerTypes.Length);
            foreach (var tile in region)
            {
                float generatedValue = ToolExtensions.NextGaussianFloat(flowerFillPercent, standardDeviation);
                if (generatedValue < waterFillPercent)
                {
                    map[tile.x, tile.y] = (int)flowerTypes[rand];
                }
            }
        }
        return map;
    }
    int[,] Sandbanks(List<List<Vector2Int>> regions, int[,] map)
    {
        for (int i = 0; i < regions.Count; i++)
        {
            foreach (var region in regions)
            {
                foreach (var tile in region)
                {
                    int waterCount = WaterCount(tile.x, tile.y, map);
                    if (waterCount > 0)
                    {
                        map[tile.x, tile.y] = 2;
                    }
                }
            }
        }
        return map;
    }
    int WaterCount(int xTile, int yTile, int[,] map)
    {
        int count = 0;
        for (int x = xTile - 1; x < xTile + 1; x++)
        {
            for (int y = yTile - 1; y < yTile + 1; y++)
            {
                if (IsInMapRange(x, y))
                {
                    if (map[x, y] == 1)
                    {
                        count++;
                    }
                }
            }
        }
        return count;

    }
}
