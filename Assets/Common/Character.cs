using UnityEngine;

public class Character : MonoBehaviour, ICharacter
{
    public IStatsProvider Stats { get; private set; }
    public Health Health { get; private set; }
    public AbilityCaster AbilityCaster { get; private set; }

    public CharacterType Type => defaultDefinition != null ? defaultDefinition.Type : default;
    public GameObject GameObject => gameObject;
    
    [Header("Default Configuration")]
    [SerializeField] private CharacterDefinition defaultDefinition;

    private const string DEFINITION_MODIFIER_SOURCE = "CharacterDefinitionBonus";
    private bool _isInitialized = false;

    private void Awake()
    {
        Stats = GetComponent<IStatsProvider>();
        Health = GetComponent<Health>();
        AbilityCaster = GetComponent<AbilityCaster>();

        if (!_isInitialized && defaultDefinition != null)
        {
            ApplyCharacterDefinition(defaultDefinition);
        }
    }

    private void Start()
    {
        if (!_isInitialized && defaultDefinition != null)
        {
             ApplyCharacterDefinition(defaultDefinition);
        }
    }

    public void ApplyCharacterDefinition(CharacterDefinition charDef)
    {
        if (charDef == null) return;

        _isInitialized = true;
        defaultDefinition = charDef;
        gameObject.name = charDef.characterName;

        if (Stats is CharacterStats characterStats && charDef.statSheet != null)
        {
            characterStats.ApplyStatSheet(charDef.statSheet);

            if (charDef.statBonuses != null)
            {
                foreach (var config in charDef.statBonuses)
                {
                    var modifier = new StatModifier(config.Value, config.Type, DEFINITION_MODIFIER_SOURCE, -1f);
                    characterStats.AddModifier(config.Stat, modifier);
                }
            }
        }

        if (AbilityCaster != null && charDef.abilities != null)
        {
            AbilityCaster.Initialize(charDef.abilities);
        }
    }
}