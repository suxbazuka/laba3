namespace Clicker
{

    public abstract class CBonusSphere : CCollectable
    {
        protected CBonusSphere(double x, double y, double lifetime)
            : base(x, y, DefaultRadius, lifetime)
        {
        }

        public override string Color { get { return "#FFC83D"; } }
    }
}
