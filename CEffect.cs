namespace Clicker
{

    public abstract class CEffect
    {
        protected CEffect(double duration)
        {
            Remaining = duration;
        }

        public double Remaining { get; private set; }
        public bool IsExpired { get { return Remaining <= 0; } }

        public abstract string Title { get; }

        public void Update(double dt)
        {
            Remaining -= dt;
        }

        public virtual BigNumber ModifyDamage(BigNumber damage)
        {
            return damage;
        }

        public virtual double ModifyCooldown(double seconds)
        {
            return seconds;
        }
    }
}
