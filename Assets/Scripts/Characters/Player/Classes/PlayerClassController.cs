using UnityEngine;

public class PlayerClassController : MonoBehaviour
{
    public ClassData startingClass;
    public ClassData currentClass;

    private PlayerStats stats;
    private PlayerAttack attack;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        attack = GetComponent<PlayerAttack>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (startingClass != null)
            ApplyClass(startingClass);
    }

    public void ApplyClass(ClassData newClass)
    {
        currentClass = newClass;

        // Remove previous class modifiers
        stats.Health.RemoveModifierBySource("Class");
        stats.AttackSpeed.RemoveModifierBySource("Class");
        stats.MoveSpeed.RemoveModifierBySource("Class");
        stats.Damage.RemoveModifierBySource("Class");

        // Apply new class multipliers
        if (newClass.healthMultiplier != 1f)
            stats.Health.AddModifier(new StatModifier(newClass.healthMultiplier - 1f, false, "Class"));
        
        if (newClass.attackSpeedMultiplier != 1f)
            stats.AttackSpeed.AddModifier(new StatModifier(newClass.attackSpeedMultiplier - 1f, false, "Class"));
        
        if (newClass.moveSpeedMultiplier != 1f)
            stats.MoveSpeed.AddModifier(new StatModifier(newClass.moveSpeedMultiplier - 1f, false, "Class"));
        
        if (newClass.damageMultiplier != 1f)
            stats.Damage.AddModifier(new StatModifier(newClass.damageMultiplier - 1f, false, "Class"));

        // Set attack behaviour
        attack.attackBehaviour = newClass.attackBehaviour;

        // Set class sprite
        if (newClass.classSprite != null)
            spriteRenderer.sprite = newClass.classSprite;
    }
}
