using UnityEngine;

public interface IPlayerFactory
{
    GameObject CreatePlayer(CharacterDefinition classDef, Vector3 position);
}