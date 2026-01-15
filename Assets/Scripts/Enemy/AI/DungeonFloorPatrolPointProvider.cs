// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// /// <summary>
// /// Picks random patrol points from the dungeon generator floor tiles.
// /// Works with the current procedural generation (RoomFirstDungeonGenerator).
// /// </summary>
// public class DungeonFloorPatrolPointProvider : MonoBehaviour, IPatrolPointProvider
// {
//     [SerializeField] private float maxSampleDistanceFromEnemy = 8f;
//     [SerializeField] private int attempts = 12;

//     private RoomFirstDungeonGenerator _generator;
//     private Tilemap _floorTilemap;

//     private readonly List<Vector2Int> _scratch = new List<Vector2Int>(1024);

//     private void Awake()
//     {
//         _generator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
//         _floorTilemap = _generator != null && _generator.TilemapVisualizer != null
//             ? _generator.TilemapVisualizer.FloorTilemap
//             : null;
//     }

//     public bool TryGetNextPoint(Vector3 fromWorldPosition, out Vector3 worldPoint)
//     {
//         worldPoint = default;

//         if (_generator == null || _generator.floorPositions == null || _generator.floorPositions.Count == 0)
//         {
//             return false;
//         }

//         if (_floorTilemap == null)
//         {
//             // Fallback: interpret floorPositions as world-ish grid coords.
//             // (Spawner does this too when tilemap is missing.)
//             foreach (var pos in _generator.floorPositions)
//             {
//                 _scratch.Add(pos);
//             }

//             return TryPickNearby(fromWorldPosition, _scratch, out worldPoint);
//         }

//         _scratch.Clear();
//         foreach (var pos in _generator.floorPositions)
//         {
//             _scratch.Add(pos);
//         }

//         if (!TryPickNearby(fromWorldPosition, _scratch, out var pickedGridWorld))
//         {
//             return false;
//         }

//         // If tilemap exists, snap to cell center.
//         var cell = _floorTilemap.WorldToCell(pickedGridWorld);
//         Vector3 cellWorld = _floorTilemap.CellToWorld(cell);
//         Vector3 cellSize = _floorTilemap.cellSize;
//         worldPoint = cellWorld + new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);
//         worldPoint.z = fromWorldPosition.z;
//         return true;
//     }

//     private bool TryPickNearby(Vector3 fromWorld, List<Vector2Int> floorCells, out Vector3 worldPoint)
//     {
//         worldPoint = default;
//         if (floorCells == null || floorCells.Count == 0) return false;

//         float maxDistSqr = maxSampleDistanceFromEnemy * maxSampleDistanceFromEnemy;

//         for (int i = 0; i < attempts; i++)
//         {
//             var cell = floorCells[Random.Range(0, floorCells.Count)];
//             var candidate = new Vector3(cell.x, cell.y, fromWorld.z);

//             if ((candidate - fromWorld).sqrMagnitude <= maxDistSqr)
//             {
//                 worldPoint = candidate;
//                 return true;
//             }
//         }

//         // Give up: return any.
//         var any = floorCells[Random.Range(0, floorCells.Count)];
//         worldPoint = new Vector3(any.x, any.y, fromWorld.z);
//         return true;
//     }
// }
