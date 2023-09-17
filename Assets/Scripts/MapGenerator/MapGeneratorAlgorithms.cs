using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class MapGeneratorAlgorithms : MapGeneratorHelper
{




    public bool useRandom;
    public string seed;
    public float waterFillPercent = 50;
    public float flowerFillPercent = 1;
    public float grassChance = 70;
    public int maxDistanceForIsolatedLand = 3;

    public int[,] FillMapRandom(Vector2Int size)
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
                    newMap[x, y] = 1; // Border always filled
                }
                else
                {
                    newMap[x, y] = rand.Next(0, 100) < waterFillPercent ? 1 : 0;
                }
            }
        }

        return newMap;
    }
    #region Cellular Automata Smoothing
    public int[,] SmoothMap(Vector2Int size, int[,] map)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int neighbourCount = GetNeighbourCount(x, y, map, size);

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
    public int[,] SmoothMapInRegion(List<Vector2Int> region, int[,] map, TileType targetTile)
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
    #endregion

    #region FloodFill Region Detection
    public List<List<Vector2Int>> GetRegions(int[,] map, params TileType[] tileTypes)
    {
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        int[,] mapFlags = new int[chunkSize.x, chunkSize.y];
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                if (mapFlags[x, y] == 0 && tileTypes.Contains((TileType)map[x, y]))
                {
                    List<Vector2Int> newRegion = GetRegionTiles(x, y, map, tileTypes);
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

    public List<Vector2Int> GetRegionTiles(int startX, int startY, int[,] map, params TileType[] treatedAsSameTypes)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        int[,] mapFlags = new int[chunkSize.x, chunkSize.y];
        TileType currentTileType = (TileType)map[startX, startY];

        // If no specific types are provided, use only the current tile type.
        if (treatedAsSameTypes.Length == 0)
        {
            treatedAsSameTypes = new TileType[] { currentTileType };
        }

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

                if (IsInMapRange(neighbourX, neighbourY, chunkSize) && mapFlags[neighbourX, neighbourY] == 0)
                {
                    // If flood-filling a water region we encounter Sand or Grass,treat them as barriers/boundaries
                    if (!treatedAsSameTypes.Contains(currentTileType) && currentTileType == TileType.Water &&
                       (map[neighbourX, neighbourY] == (int)TileType.Sand))
                    {
                        continue;
                    }

                    // If the neighboring tile is one of the types we're treating as the same, explore it.
                    if (treatedAsSameTypes.Contains((TileType)map[neighbourX, neighbourY]))
                    {
                        mapFlags[neighbourX, neighbourY] = 1;
                        queue.Enqueue(new Vector2Int(neighbourX, neighbourY));
                    }
                }
            }
        }
        return tiles;
    }


    public List<List<Vector2Int>> FindAllSubRegionsInLargerRegion(List<Vector2Int> largerRegion, int[,] map, TileType targetTileType)
    {
        // To keep track of tiles that have been processed
        HashSet<Vector2Int> processedTiles = new HashSet<Vector2Int>();

        List<List<Vector2Int>> subRegions = new List<List<Vector2Int>>();

        foreach (Vector2Int tile in largerRegion)
        {
            if (!processedTiles.Contains(tile) && map[tile.x, tile.y] == (int)targetTileType)
            {
                List<Vector2Int> subRegion = GetRegionTiles(tile.x, tile.y, map, targetTileType);

                subRegions.Add(subRegion);

                foreach (Vector2Int subTile in subRegion)
                {
                    processedTiles.Add(subTile);
                }
            }
        }
        return subRegions;
    }

    public int[,] ProcessMap(int[,] map, TileType tileType, int threshold)
    {
        List<List<Vector2Int>> regions = GetRegions(map,tileType);

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
    #endregion

    #region Populate Map
    public int[,] PopulateGrassWithFlowers(List<Vector2Int> region, int[,] map)
    {
        if (grassTypes.Contains((TileType)map[region[0].x, region[0].y]))
        {
            int rand = Random.Range(0, flowerTypes.Length);
            foreach (var tile in region)
            {
                int generatedValue = Random.Range(0, 100);
                if (generatedValue < flowerFillPercent)
                {
                    map[tile.x, tile.y] = (int)flowerTypes[rand];
                }
            }
        }
        return map;
    }
    public int[,] Sandbanks(List<List<Vector2Int>> regions, int[,] map)
    {
        for (int i = 0; i < regions.Count; i++)
        {
            foreach (var region in regions)
            {
                foreach (var tile in region)
                {
                    int waterCount = WaterCount(tile.x, tile.y, map, chunkSize);
                    if (waterCount > 0)
                    {
                        map[tile.x, tile.y] = 2;
                    }
                }
            }
        }
        return map;
    }
    public int[,] SetGrassInRegions(List<List<Vector2Int>> regions, int[,] map)
    {
        foreach (var region in regions)
        {

            foreach (Vector2Int tile in region)
            {
                // Check if the current tile is an island tile
                if (map[tile.x, tile.y] == 0) // assuming 0 is island
                {
                    // For every second island tile, check with randomness
                    if (UnityEngine.Random.Range(0f, 100f) >= grassChance)
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
                //map = PopulateGrassWithFlowers(subRegion, map);
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
    public int[,] SetOceanDepth(int[,] fullMap)
    {
        int width = fullMap.GetLength(0);
        int height = fullMap.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fullMap[x, y] == (int)TileType.Water)
                {
                    int distance = DistanceToNearestTile(new Vector2Int(x, y), (int)TileType.Island, fullMap);

                    if (distance >= 0) // Ensure there's an island in proximity
                    {
                        if (distance < 2)
                        {
                            fullMap[x, y] = (int)TileType.shallowWater;
                        }
                        else if (distance < 5)
                        {
                            fullMap[x, y] = (int)TileType.lowWater;
                        }
                        else if (distance < 9)
                        {
                            fullMap[x, y] = (int)TileType.mediumWater;
                        }
                        else
                        {
                            fullMap[x, y] = (int)TileType.deepWater;
                        }
                    }
                }
            }
        }
        return fullMap;
    }
    public int[,] BlurOceanDepth(int[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int[,] blurredMap = new int[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                int sum = 0;
                // Loop over the cell and its neighbors
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        sum += map[x + i, y + j];
                    }
                }
                blurredMap[x, y] = sum / 9; // There are 9 cells (3x3) being averaged
            }
        }

        // Copy edges from original map to blurred map, since we didn't process them
        for (int x = 0; x < width; x++)
        {
            blurredMap[x, 0] = map[x, 0];
            blurredMap[x, height - 1] = map[x, height - 1];
        }

        for (int y = 0; y < height; y++)
        {
            blurredMap[0, y] = map[0, y];
            blurredMap[width - 1, y] = map[width - 1, y];
        }

        return blurredMap;
    }
    public int[,] AdjustIsolatedTiles(int[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        // tweak this value as needed

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0) // land tile
                {
                    int distance = DistanceToNearestTile(new Vector2Int(x, y), 1, map); // distance to nearest water tile
                    if (distance > maxDistanceForIsolatedLand)
                    {
                        map[x, y] = 1; // Convert this land to water as it's too isolated
                    }
                }
            }
        }

        return map;
    }

    #endregion

}
