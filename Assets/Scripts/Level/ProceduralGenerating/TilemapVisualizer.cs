using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;
    [SerializeField]
    private TileBase floorTile, wallTop, wallSideRight, wallSideLeft, wallBottom, wallFull,
        wallInnerCornerDownLeft, wallInnerCornerDownRight,
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;
    
    public void PaintFloorTiles(IEnumerable<Vector2Int>  floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions) {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition,tile);
    }
    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        if(WallByteTypes.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }else if (WallByteTypes.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallByteTypes.wallSideLeft.Contains(typeAsInt))
        {
            tile = wallSideLeft;
        }
        else if (WallByteTypes.wallBottom.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallByteTypes.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }

        if (tile != null)
        {
            PaintSingleTile(wallTilemap, tile, position);
        }
        
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binnaryType)
    {
        int typeAsInt = Convert.ToInt32(binnaryType, 2);
        TileBase tile = null;
        if(WallByteTypes.wallInnerCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallByteTypes.wallInnerCornerDownRight.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallByteTypes.wallDiagonalCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallByteTypes.wallDiagonalCornerDownRight.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallByteTypes.wallDiagonalCornerUpLeft.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallByteTypes.wallDiagonalCornerUpRight.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallByteTypes.wallFullEightDirections.Contains(typeAsInt))
        {
            tile = wallFull;
        }
        else if (WallByteTypes.wallBottomEightDirections.Contains(typeAsInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
        {
            PaintSingleTile(wallTilemap,tile,position);
        }
    }
}
