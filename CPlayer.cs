namespace Clicker
{
    /// <summary>
    /// Игрок. Состояние наружу доступно только для чтения,
    /// менять его можно лишь через AddGold и TryUpgrade.
    /// </summary>
    public class CPlayer
    {
        public BigNumber Gold { get; private set; }
        public BigNumber Damage { get; private set; }
        public BigNumber UpgradeCost { get; private set; }
        public int DamageLevel { get; private set; }

        public CPlayer()
        {
            Gold = BigNumber.Zero;
            Damage = new BigNumber(1);
            UpgradeCost = new BigNumber(10);
            DamageLevel = 1;
        }

        public bool CanUpgrade
        {
            get { return Gold >= UpgradeCost; }
        }

        public void AddGold(BigNumber amount)
        {
            Gold = Gold + amount;
        }

        /// <summary>Покупает улучшение урона, если хватает золота.</summary>
        public bool TryUpgrade()
        {
            if (!CanUpgrade) return false;

            Gold = Gold - UpgradeCost;
            Damage = Damage * 11 / 10 + 1;            // +10% и ещё +1
            UpgradeCost = UpgradeCost * 13 / 10 + 1;  // цена растёт на 30%
            DamageLevel++;
            return true;
        }
    }
}
