using System;

namespace Clicker
{

    public class CClickerSphereFactory : CSphereFactory
    {
        public override CCollectable CreateRandom(Random random, double x, double y, double lifetime)
        {
            int roll = random.Next(100);

            if (roll < 40) return new CPlainSphere(x, y, lifetime);
            if (roll < 60) return new CDamageBonusSphere(x, y, lifetime);
            if (roll < 80) return new CCooldownBonusSphere(x, y, lifetime);
            return new CGoldBonusSphere(x, y, lifetime);
        }
    }
}
