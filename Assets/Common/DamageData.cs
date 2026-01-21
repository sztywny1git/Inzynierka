using System;
using UnityEngine;

[Serializable]
public struct DamageData
{
    public float Amount;
    public bool IsCritical;
    public GameObject Instigator;
    public Vector3 SourcePosition;

    public DamageData(float amount, bool isCrit, GameObject instigator, Vector3 sourcePosition)
    {
        Amount = amount;
        IsCritical = isCrit;
        Instigator = instigator;
        SourcePosition = sourcePosition;
    }
}