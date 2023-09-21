using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ToolExtensions;
public class MapGeneratorHelper : MonoBehaviour
{

    [SerializeField]
    [Header("Dimensions")]
    public Vector2Int gridSize;
    public Vector2Int chunkSize;
    [Header("Settings")]
    public float waterFillPercent = 50;
    public int proccessThreshhold = 25;


    [Header("Benchmark")]
    public TMP_Text elapsedMsText;
    public TMP_Text statsText;
    public placeCam placeCam;
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
            if (ToolExtensions.ChunkTools.IsInMapRange(neighbourX, neighbourY, size))
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
        // Directions representing the 8 neighboring cells
        int[] dirX = { -1, 0, 1, 0 };
        int[] dirY = { 0, -1, 0, 1 };

        for (int i = 0; i < 4; i++)
        {
            int neighbourX = x + dirX[i];
            int neighbourY = y + dirY[i];
            Vector2Int neighbourTile = new Vector2Int(neighbourX, neighbourY);

            //if the neighbour is still in the region and is the target tileType the tile is counted alive
            if (region.Contains(neighbourTile) && map[neighbourX, neighbourY] == (int)targetType)
            {
                count++;
            }
        }
        return count;
    }

    public int WaterCount(int xTile, int yTile, int[,] map, Vector2Int size)
    {
        int count = 0;
        for (int x = xTile - 1; x < xTile + 1; x++)
        {
            for (int y = yTile - 1; y < yTile + 1; y++)
            {
                if (ToolExtensions.ChunkTools.IsInMapRange(x, y, size))
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


    

    public static int[,] ConcatenateChunks(Chunk[,] chunks)
    {
        // chunk-,grid- and fullmap- size 
        int chunkSizeX = chunks[0, 0].size.x;
        int chunkSizeY = chunks[0, 0].size.y;
        int numChunksX = chunks.GetLength(0);
        int numChunksY = chunks.GetLength(1);
        int fullMapSizeX = chunkSizeX * numChunksX;
        int fullMapSizeY = chunkSizeY * numChunksY;

        int[,] fullMap = new int[fullMapSizeX, fullMapSizeY]; //initialitze fullmap

        for (int cx = 0; cx < numChunksX; cx++)
        {
            for (int cy = 0; cy < numChunksY; cy++)
            {
                for (int x = 0; x < chunkSizeX; x++)
                {
                    for (int y = 0; y < chunkSizeY; y++)
                    {
                        //Set local chunkTile position to fullmap position
                        fullMap[cx * chunkSizeX + x, cy * chunkSizeY + y] = chunks[cx, cy].map[x, y];
                    }
                }
            }
        }
        return fullMap;
    }
    public Chunk[,] DivideIntoChunks(int[,] fullMap, Vector2Int chunkSize, Chunk[,] chunks)
    {
        //Get fullmap and gridSize
        int width = fullMap.GetLength(0);
        int height = fullMap.GetLength(1);
        int numChunksX = width / chunkSize.x;
        int numChunksY = height / chunkSize.y;

        //Initialize 2D Chunk Map
        Chunk[,] chunkse = new Chunk[numChunksX, numChunksY];

        for (int cx = 0; cx < numChunksX; cx++)
        {
            for (int cy = 0; cy < numChunksY; cy++)
            {
                Vector2Int chunkCenter = new Vector2Int(cx * chunkSize.x + chunkSize.x / 2, cy * chunkSize.y + chunkSize.y / 2); //calculate the center of the chunk
                int[,] chunkMap = new int[chunkSize.x, chunkSize.y]; // initialize chunk map
                for (int x = 0; x < chunkSize.x; x++)
                {
                    for (int y = 0; y < chunkSize.y; y++)
                    {
                        chunkMap[x, y] = fullMap[cx * chunkSize.x + x, cy * chunkSize.y + y]; //Set chunkTile positon to full map position
                    }
                }
                chunkse[cx, cy] = new Chunk(chunkCenter, chunkSize, chunkMap, chunks[cx, cy].regions); //Save Chunk into 2D ChunkMap

            }
        }

        return chunkse;
    }

    


    

}

