using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UIElements;

public class MapGenerator : MapGeneratorAlgorithms
{
    Chunk[,] chunks = new Chunk[0, 0];
    public Transform playerPos;

    List<List<Vector2Int>> allRegions = new List<List<Vector2Int>>();
    int[,] fullmap;

    [Header("Advanced Settings")]

    [Tooltip("Steps in Cellular Automata")]
    public int smoothIterations = 3;

    [Header("TileGroups")]
    public TileType[] grassTypes = { (TileType)3, (TileType)5, (TileType)6 };
    public TileType[] flowerTypes;
    public TileType[] islandTiles;
    public TileType[] waterTiles;

    public RegionLoader regionLoader;

    public Map GenerateMap()
    {
        GenerateChunks();

        //Save the map
        Map newmap = new Map(this.GetInstanceID(), new Vector2Int(fullmap.GetLength(0) / 2, fullmap.GetLength(1) / 2), fullmap, chunks, allRegions, gridSize, chunkSize);
        return newmap;
    }

    public void GenerateChunks()
    {
 
        // Initialize the chunks array.
        chunks = new Chunk[gridSize.x, gridSize.y];

        //Create Each Chunk
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                CreateChunkAtPosition(x, y);
            }
        }
       
        int[,] newmap = ConcatenateChunks(chunks);  //Put all chunks into a full map


        newmap = SetOceanDepth(newmap,1); //Apply Ocean Depth
    

        //Get all regions from the full map
        allRegions = GetRegions(newmap, new Vector2Int(gridSize.x * chunkSize.x, gridSize.y * chunkSize.y), islandTiles);
        fullmap = newmap;
       
        //Divide the full map back into chunks
        chunks = DivideIntoChunks(newmap, chunkSize, chunks);

    }

    void CreateChunkAtPosition(int x, int y)
    {
        //Set Position and Size of the chunk
        Vector2Int pos = new Vector2Int(x, y);
        Vector2Int size = chunkSize;

        //Generate Chunk
        int[,] newmap = FillMapRandom(size);  //Fill with random
        
        //Process the chunk
        for (int i = 0; i <= 3; i++)
        {
            newmap = SmoothMap(size, newmap);  //Applies Cellular Automata
        }
        newmap = ProcessMap(newmap, TileType.Island, proccessThreshhold); //Deletes Regions under the set threshold
        
        List<List<Vector2Int>> regions = GetRegions(newmap, chunkSize, islandTiles); //Get All Regions
        newmap = SetGrassInRegions(regions, newmap, flowerTypes, grassTypes); // randomly set grass in regions
        newmap = Sandbanks(regions, newmap); //Set SandTiles on the border of each region

        //Save Chunk
        chunks[x, y] = new Chunk(pos, size, newmap, regions);
    }
}
    



