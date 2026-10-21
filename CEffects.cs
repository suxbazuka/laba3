namespace Clicker
{

    public class CDamageEffect : CEffect
    {
        public CDamageEffect(double duration) : base(duration) { }

        public override string Title { get { return "Урон ×2"; } }

        public override BigNumber ModifyDamage(BigNumber damage)
        {
            return damage * 2;
        }
    }

    public class CCooldownEffect : CEffect
    {
        public CCooldownEffect(double duration) : base(duration) { }

        public override string Title { get { return "Перезарядка ×0.5"; } }

        public override double ModifyCooldown(double seconds)
        {
            return seconds * 0.5;
        }
    }
}
