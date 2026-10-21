namespace Clicker
{

    public class CDamageBonusSphere : CBonusSphere
    {
        private const double Duration = 8.0;

        public CDamageBonusSphere(double x, double y, double lifetime) : base(x, y, lifetime) { }

        public override string Color { get { return "#FF5A5A"; } }
        public override string Label { get { return "УРОН"; } }

        protected override void OnClick(CClickerGame game)
        {
            game.AddEffect(new CDamageEffect(Duration));
        }
    }

    public class CCooldownBonusSphere : CBonusSphere
    {
        private const double Duration = 8.0;

        public CCooldownBonusSphere(double x, double y, double lifetime) : base(x, y, lifetime) { }

        public override string Color { get { return "#5AD17A"; } }
        public override string Label { get { return "КД"; } }

        protected override void OnClick(CClickerGame game)
        {
            game.AddEffect(new CCooldownEffect(Duration));
        }
    }

    public class CGoldBonusSphere : CBonusSphere
    {
        public CGoldBonusSphere(double x, double y, double lifetime) : base(x, y, lifetime) { }

        public override string Color { get { return "#FFD84D"; } }
        public override string Label { get { return "ЗОЛ."; } }

        protected override void OnClick(CClickerGame game)
        {

            game.Player.AddGold(new BigNumber(50 + 10 * game.Stage));
        }
    }
}
