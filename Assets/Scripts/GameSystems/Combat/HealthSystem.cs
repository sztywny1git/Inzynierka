using UnityEngine;

public class HealthSystem
{
    private PlayerStats stats;

    public HealthSystem(PlayerStats playerStats)
    {
        stats = playerStats;
        // Initialize current health at start
        stats.SetCurrentHealth(stats.Health.FinalValue);
    }

    public void TakeDamage(float incomingDamage)
    {
        // 1. Minimum damage percentage (e.g., 20%)
        float minDamagePercent = 0.2f;
        float minDamage = incomingDamage * minDamagePercent;

        // 2. Armor-based damage reduction
        float armorValue = stats.Armor.FinalValue;
        float damageReductionPercent = armorValue / (armorValue + 100f);
        float dmgAfterArmor = incomingDamage * (1f - damageReductionPercent);

        // 3. Flat defense reduction
        float dmgAfterDefense = dmgAfterArmor - stats.Defense.FinalValue;

        // 4. Ensure damage is at least minimum
        float finalDamageFloat = Mathf.Max(dmgAfterDefense, minDamage);

        // 5. Round to nearest integer
        int finalDamage = Mathf.RoundToInt(finalDamageFloat);

        // 6. Apply damage to current health
        stats.SetCurrentHealth(stats.CurrentHealth - finalDamage);
    }

    public void Heal(float amount)
    {
        // Apply healing
        int healAmount = Mathf.RoundToInt(amount);
        stats.SetCurrentHealth(stats.CurrentHealth + healAmount);
    }
}
