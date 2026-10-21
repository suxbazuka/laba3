namespace Clicker
{

    public abstract class CCollectable
    {
        public const double DefaultRadius = 26;

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Radius { get; private set; }
        public double TimeLeft { get; private set; }
        public bool IsCollected { get; private set; }

        protected CCollectable(double x, double y, double radius, double lifetime)
        {
            X = x;
            Y = y;
            Radius = radius;
            TimeLeft = lifetime;
        }

        public virtual string Color { get { return "#4FA3FF"; } }
        public virtual string Label { get { return ""; } }

        public bool IsAlive { get { return !IsCollected && TimeLeft > 0; } }

        public void Update(double dt)
        {
            TimeLeft -= dt;
        }

        public bool Click(CClickerGame game)
        {
            if (!IsAlive) return false;

            IsCollected = true;
            OnClick(game);
            return true;
        }

        protected abstract void OnClick(CClickerGame game);
    }
}
