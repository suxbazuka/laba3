using System;

namespace Clicker
{

    public class CClickCooldown
    {
        public double Remaining { get; private set; }

        public bool IsReady { get { return Remaining <= 0; } }

        public void Start(double seconds)
        {
            Remaining = Math.Max(0, seconds);
        }

        public void Update(double dt)
        {
            if (Remaining > 0)
                Remaining = Math.Max(0, Remaining - dt);
        }
    }
}
