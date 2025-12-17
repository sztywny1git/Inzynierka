using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterStats Stats { get; private set; }
    public Health Health { get; private set; }
    public AbilityCaster AbilityCaster { get; private set; }

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private const string DEFINITION_MODIFIER_SOURCE = "CharacterDefinitionBonus";

    private void Awake()
    {
        Stats = GetComponent<CharacterStats>();
        Health = GetComponent<Health>();
        AbilityCaster = GetComponent<AbilityCaster>();
    }

    public void ApplyCharacterDefinition(CharacterDefinition charDef)
    {
        this.gameObject.name = charDef.characterName;

        if (spriteRenderer != null && charDef.characterSprite != null)
        {
            spriteRenderer.sprite = charDef.characterSprite;
        }

        if (Stats != null && charDef.statSheet != null)
        {
            Stats.ApplyStatSheet(charDef.statSheet);
            
            if (charDef.statBonuses != null)
            {
                foreach (var config in charDef.statBonuses)
                {
                    var modifier = new StatModifier(config.Value, config.Type, DEFINITION_MODIFIER_SOURCE, -1f);
                    Stats.AddModifier(config.Stat, modifier);
                }
            }
        }

        if (AbilityCaster != null && charDef.abilities != null)
        {
            AbilityCaster.SetAbilities(charDef.abilities);
        }
    }
}