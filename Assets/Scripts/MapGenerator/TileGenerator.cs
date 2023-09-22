using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
public class TileGenerator : MonoBehaviour
{

    [Header("Tilemap")]
    [SerializeField]
    public TileValue[] tiles;
    public Tilemap tilemapGround;
    public Tilemap tilemapTop;


    public void LoadFullMapTiles(Vector2Int fullmapSize, int[,] fullmap)
    {
        tilemapGround.ClearAllTiles();
        tilemapTop.ClearAllTiles();
        for (int x = 0; x < fullmapSize.x; x++)
        {
            for (int y = 0; y < fullmapSize.y; y++)
            {
                // Calculate the position for each tile in the entire map.
                Vector3Int position = new Vector3Int(x, y, 0);

                // Get the tile type from fullmap and set the appropriate tile.
                int value = fullmap[x, y];
                tilemapGround.SetTile(position, tiles.FirstOrDefault(e => (int)e.Type == value).tile);
                //Set Tiles into the collider Tilemap
                //foreach (var region in chunks[chunkX, chunkY].regions)
                //{
                //    foreach (var tile in region)
                //    {
                //        position = new Vector3Int(chunkX * chunkSize.x + tile.x, chunkY * chunkSize.y + tile.y, 0);
                //        tilemapTop.SetTile(position, tiles[4].tile);
                //    }
                //}

            }
        }
    }
    public void SetColliderTiles(List<List<Vector2Int>> regions)
    {
        foreach(var region in regions)
        {
            foreach(var tile in region)
            {
                tilemapTop.SetTile(new Vector3Int(tile.x,tile.y), tiles[0].tile);
            }
        }
    }
    public void ClearAllTilemap()
    {
        tilemapGround.ClearAllTiles() ;
        tilemapTop.ClearAllTiles();
    }
}
