using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGeneratorHelper : MonoBehaviour
{
    [SerializeField]
    public Vector2Int gridSize;
    public Vector2Int chunkSize;

    public TileType[] grassTypes = { (TileType)3, (TileType)5, (TileType)6 };
    public TileType[] flowerTypes;
    public TileType[] islandTiles;
    public bool IsBorder(int x, int y, Vector2Int size)
    {
        return x == 0 || y == 0 || x == size.x - 1 || y == size.y - 1;
    }
    public int GetNeighbourCount(int gridX, int gridY, int[,] map, Vector2Int size)
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
            if (IsInMapRange(neighbourX, neighbourY, size))
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
    public int GetNeighbourCountInRegion(int x, int y, int[,] map, TileType targetType, List<Vector2Int> region)
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
    public bool IsInMapRange(int x, int y, Vector2Int size)
    {
        return x >= 0 && x < size.x && y >= 0 && y < size.y;
    }
    public int WaterCount(int xTile, int yTile, int[,] map, Vector2Int size)
    {
        int count = 0;
        for (int x = xTile - 1; x < xTile + 1; x++)
        {
            for (int y = yTile - 1; y < yTile + 1; y++)
            {
                if (IsInMapRange(x, y, size))
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
    public static int[,] ConcatenateChunks(Chunk[,] chunkse)
    {
        int chunkSizeX = chunkse[0, 0].size.x;
        int chunkSizeY = chunkse[0, 0].size.y;
        int numChunksX = chunkse.GetLength(0);
        int numChunksY = chunkse.GetLength(1);
        int fullMapSizeX = chunkSizeX * numChunksX;
        int fullMapSizeY = chunkSizeY * numChunksY;
        int[,] fullMap = new int[fullMapSizeX, fullMapSizeY];

        for (int cx = 0; cx < numChunksX; cx++)
        {
            for (int cy = 0; cy < numChunksY; cy++)
            {
                for (int x = 0; x < chunkSizeX; x++)
                {
                    for (int y = 0; y < chunkSizeY; y++)
                    {
                        fullMap[cx * chunkSizeX + x, cy * chunkSizeY + y] = chunkse[cx, cy].map[x, y];
                    }
                }
            }
        }

        return fullMap;
    }
    public Chunk[,] DivideIntoChunks(int[,] fullMap, Vector2Int chunkSize, Chunk[,] chunks)
    {
        int width = fullMap.GetLength(0);
        int height = fullMap.GetLength(1);
        int numChunksX = width / chunkSize.x;
        int numChunksY = height / chunkSize.y;
        Chunk[,] chunkse = new Chunk[numChunksX, numChunksY];

        for (int cx = 0; cx < numChunksX; cx++)
        {
            for (int cy = 0; cy < numChunksY; cy++)
            {
                Vector2Int chunkCenter = new Vector2Int(cx * chunkSize.x + chunkSize.x / 2, cy * chunkSize.y + chunkSize.y / 2);
                int[,] chunkMap = new int[chunkSize.x, chunkSize.y];
                for (int x = 0; x < chunkSize.x; x++)
                {
                    for (int y = 0; y < chunkSize.y; y++)
                    {
                        chunkMap[x, y] = fullMap[cx * chunkSize.x + x, cy * chunkSize.y + y];
                    }
                }
                chunkse[cx, cy] = new Chunk(chunkCenter, chunkSize, chunkMap, chunks[cx,cy].regions);
                
            }
        }

        return chunks;
    }

    public int DistanceToNearestTile(Vector2Int start, int targetTileType, int[,] map)
    {
        // Directions for left, right, up, down
        Vector2Int[] directions =
        {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1)
    };

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        bool[,] visited = new bool[width, height];
        Queue<DataTile> queue = new Queue<DataTile>();
        queue.Enqueue(new DataTile(start, 0));

        while (queue.Count > 0)
        {
            DataTile currentTile = queue.Dequeue();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int nextPos = currentTile.Position + dir;

                // Check if the tile is within map boundaries and hasn't been visited yet
                if (nextPos.x >= 0 && nextPos.x < width && nextPos.y >= 0 && nextPos.y < height && !visited[nextPos.x, nextPos.y])
                {
                    visited[nextPos.x, nextPos.y] = true;

                    // If the next tile matches the targetTileType, return its distance from the start
                    if (map[nextPos.x, nextPos.y] == targetTileType)
                    {
                        return currentTile.Distance + 1;
                    }

                    queue.Enqueue(new DataTile(nextPos, currentTile.Distance + 1));
                }
            }
        }

        return -1;  // Return -1 if no target tile is found
    }
    public bool IsBoundaryTile(Vector2Int tile, int[,] map, params TileType[] tileTypes)
    {
        int[] dirX = { 0, 0, 1, -1 };
        int[] dirY = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int neighbourX = tile.x + dirX[i];
            int neighbourY = tile.y + dirY[i];

            // If out of range, continue to next iteration
            if (!IsInMapRange(neighbourX, neighbourY, chunkSize))
                continue;

            if (!tileTypes.Contains((TileType)map[neighbourX, neighbourY]))
            {
                return true;  // This tile has a neighbor of a different type, so it's a boundary tile.
            }
        }
        return false;
    }
    public Vector3Int GetTileGlobalPosition(Vector2Int chunkPos, Vector2Int gridPos)
    {
       return new Vector3Int(chunkPos.x * chunkSize.x + gridPos.x, chunkPos.y * chunkSize.y + gridPos.y, 0);
    }


}
