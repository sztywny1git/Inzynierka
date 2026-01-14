using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameConstants", menuName = "Core/Game Constants")]
public class GameConstants : ScriptableObject
{
    [Header("Player")]
    [SerializeField] private CharacterDefinition _defaultPlayerClass;
    public CharacterDefinition DefaultPlayerClass => _defaultPlayerClass;

    [Header("Core Prefabs")]
    [SerializeField] private GameObject _gameplayScopePrefab;
    public GameObject GameplayScopePrefab => _gameplayScopePrefab;
    
    [Header("Run Definitions")]
    [SerializeField] private RunDefinition _standardRun;
    public RunDefinition StandardRun => _standardRun;

    public int MaxProfileSlots = 3;
}