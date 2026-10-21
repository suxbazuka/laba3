namespace Clicker
{
    public class CPlainSphere : CCollectable
    {
        public CPlainSphere(double x, double y, double lifetime)
            : base(x, y, DefaultRadius, lifetime)
        {
        }

        public override string Color { get { return "#7F8C9A"; } }
        public override string Label { get { return "+10"; } }

        protected override void OnClick(CClickerGame game)
        {
            game.Player.AddGold(new BigNumber(10));
        }
    }
}
