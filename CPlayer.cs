using System;

namespace Clicker
{
    public class CPlayer
    {
        public const double MinAttackCooldown = 0.3;

        public BigNumber Gold { get; private set; }
        public BigNumber Damage { get; private set; }
        public BigNumber UpgradeCost { get; private set; }
        public int DamageLevel { get; private set; }

        public double AttackCooldownBase { get; private set; }
        public BigNumber CooldownUpgradeCost { get; private set; }
        public int CooldownLevel { get; private set; }

        public CClickCooldown AttackCooldown { get; private set; }

        public CPlayer()
        {
            Gold = BigNumber.Zero;
            Damage = new BigNumber(1);
            UpgradeCost = new BigNumber(10);
            DamageLevel = 1;

            AttackCooldownBase = 2.0;
            CooldownUpgradeCost = new BigNumber(15);
            CooldownLevel = 0;

            AttackCooldown = new CClickCooldown();
        }

        public bool CanUpgrade
        {
            get { return Gold >= UpgradeCost; }
        }

        public bool CanUpgradeCooldown
        {
            get { return AttackCooldownBase > MinAttackCooldown && Gold >= CooldownUpgradeCost; }
        }

        public void AddGold(BigNumber amount)
        {
            Gold = Gold + amount;
        }

        public bool TryUpgrade()
        {
            if (!CanUpgrade) return false;

            Gold = Gold - UpgradeCost;
            Damage = Damage * 11 / 10 + 1;
            UpgradeCost = UpgradeCost * 13 / 10 + 1;
            DamageLevel++;
            return true;
        }

        public bool TryUpgradeCooldown()
        {
            if (!CanUpgradeCooldown) return false;

            Gold = Gold - CooldownUpgradeCost;
            AttackCooldownBase = Math.Max(MinAttackCooldown, AttackCooldownBase * 0.9);
            CooldownUpgradeCost = CooldownUpgradeCost * 13 / 10 + 1;
            CooldownLevel++;
            return true;
        }
    }
}
