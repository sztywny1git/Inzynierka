using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [Header("Main Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Room Type Color Tilemap (NEW)")]
    [SerializeField] private Tilemap roomTypeTilemap;

    [Header("Floor Tiles")]
    [SerializeField] private TileBase floorTile;

    [Header("Wall Tiles")]
    [SerializeField] private TileBase wallTop, wallSideRight, wallSideLeft,
        wallBottom, wallFull,
        wallInnerCornerDownLeft, wallInnerCornerDownRight,
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft,
        wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    [Header("Room Type Tiles (NEW)")]
    [SerializeField] private TileBase startRoomTile;
    [SerializeField] private TileBase bossRoomTile;
    [SerializeField] private TileBase puzzleRoomTile;
    [SerializeField] private TileBase standardRoomTile;

    public Tilemap FloorTilemap => floorTilemap;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        Vector3Int gridPos = new Vector3Int(position.x, position.y, 0);
        Vector3Int tilePosition = tilemap.WorldToCell((Vector3)gridPos);
        tilemap.SetTile(tilePosition, tile);
    }


    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        if (roomTypeTilemap != null)
            roomTypeTilemap.ClearAllTiles();
    }

    //Ściany

    internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = System.Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallByteTypes.wallTop.Contains(typeAsInt)) tile = wallTop;
        else if (WallByteTypes.wallSideRight.Contains(typeAsInt)) tile = wallSideRight;
        else if (WallByteTypes.wallSideLeft.Contains(typeAsInt)) tile = wallSideLeft;
        else if (WallByteTypes.wallBottom.Contains(typeAsInt)) tile = wallBottom;
        else if (WallByteTypes.wallFull.Contains(typeAsInt)) tile = wallFull;

        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binnaryType)
    {
        int typeAsInt = System.Convert.ToInt32(binnaryType, 2);
        TileBase tile = null;

        if (WallByteTypes.wallInnerCornerDownLeft.Contains(typeAsInt)) tile = wallInnerCornerDownLeft;
        else if (WallByteTypes.wallInnerCornerDownRight.Contains(typeAsInt)) tile = wallInnerCornerDownRight;
        else if (WallByteTypes.wallDiagonalCornerDownLeft.Contains(typeAsInt)) tile = wallDiagonalCornerDownLeft;
        else if (WallByteTypes.wallDiagonalCornerDownRight.Contains(typeAsInt)) tile = wallDiagonalCornerDownRight;
        else if (WallByteTypes.wallDiagonalCornerUpLeft.Contains(typeAsInt)) tile = wallDiagonalCornerUpLeft;
        else if (WallByteTypes.wallDiagonalCornerUpRight.Contains(typeAsInt)) tile = wallDiagonalCornerUpRight;
        else if (WallByteTypes.wallFullEightDirections.Contains(typeAsInt)) tile = wallFull;
        else if (WallByteTypes.wallBottomEightDirections.Contains(typeAsInt)) tile = wallBottom;

        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }

    public void PaintRoomTypes(Dictionary<BoundsInt, RoomType> roomTypes)
    {
        if (roomTypeTilemap == null)
        {
            Debug.LogWarning("RoomTypeTilemap not assigned.");
            return;
        }

        foreach (var kvp in roomTypes)
        {
            PaintSingleRoom(kvp.Key, kvp.Value);
        }
    }

    private void PaintSingleRoom(BoundsInt roomBounds, RoomType type)
    {
        TileBase tile = type switch
        {
            RoomType.Start => startRoomTile,
            RoomType.Boss => bossRoomTile,
            RoomType.Puzzle => puzzleRoomTile,
            RoomType.Standard => standardRoomTile,
            _ => null
        };

        if (tile == null)
            return;

        for (int x = roomBounds.xMin; x <= roomBounds.xMax; x++)
        {
            for (int y = roomBounds.yMin; y <= roomBounds.yMax; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                PaintSingleTile(roomTypeTilemap, tile, pos);
            }
        }
    }
}
