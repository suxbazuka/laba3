using EnemyEditor;

namespace Clicker
{

    public class CEnemy
    {
        private const int ModifierPrecision = 1000;

        public string Name { get; private set; }
        public string IconName { get; private set; }
        public BigNumber MaxHealth { get; private set; }
        public BigNumber CurrentHealth { get; private set; }
        public BigNumber Reward { get; private set; }

        public CEnemy(CEnemyTemplate template, int stage)
        {
            Name = template.Name;
            IconName = template.IconName;
            MaxHealth = Scale(template.BaseLife, template.LifeModifier, stage);
            if (MaxHealth.IsZero) MaxHealth = BigNumber.One;
            CurrentHealth = MaxHealth;
            Reward = Scale(template.BaseGold, template.GoldModifier, stage);
        }

        public bool IsDefeated
        {
            get { return CurrentHealth.IsZero; }
        }

        public bool TakeDamage(BigNumber damage)
        {
            if (IsDefeated) return true;

            if (damage >= CurrentHealth)
                CurrentHealth = BigNumber.Zero;
            else
                CurrentHealth = CurrentHealth - damage;

            return IsDefeated;
        }

        public int HealthPercent
        {
            get { return int.Parse((CurrentHealth * 100 / MaxHealth).ToString()); }
        }
        private static BigNumber Scale(int baseValue, double modifier, int stage)
        {
            if (baseValue <= 0 || modifier <= 0) return BigNumber.Zero;

            int numerator = (int)System.Math.Round(modifier * ModifierPrecision);
            BigNumber n = BigNumber.Pow(new BigNumber(numerator), stage);
            BigNumber d = BigNumber.Pow(new BigNumber(ModifierPrecision), stage);
            return new BigNumber(baseValue) * n / d;
        }
    }
}
