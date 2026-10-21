using System.Text.Json.Serialization;

namespace EnemyEditor
{
    public class CEnemyTemplate
    {
        [JsonInclude]
        public string Name { get; private set; }

        [JsonInclude]
        public string IconName { get; private set; }

        [JsonInclude]
        public int BaseLife { get; private set; }

        [JsonInclude]
        public double LifeModifier { get; private set; }

        [JsonInclude]
        public int BaseGold { get; private set; }

        [JsonInclude]
        public double GoldModifier { get; private set; }

        [JsonInclude]
        public double SpawnChance { get; private set; }

        public CEnemyTemplate(string name, string iconName, int baseLife,
            double lifeModifier, int baseGold, double goldModifier, double spawnChance)
        {
            Name = name;
            IconName = iconName;
            BaseLife = baseLife;
            LifeModifier = lifeModifier;
            BaseGold = baseGold;
            GoldModifier = goldModifier;
            SpawnChance = spawnChance;
        }

        public void Update(string name, string iconName, int baseLife,
            double lifeModifier, int baseGold, double goldModifier, double spawnChance)
        {
            Name = name;
            IconName = iconName;
            BaseLife = baseLife;
            LifeModifier = lifeModifier;
            BaseGold = baseGold;
            GoldModifier = goldModifier;
            SpawnChance = spawnChance;
        }
    }
}