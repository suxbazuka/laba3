using System;

namespace Clicker
{
    public abstract class CSphereFactory
    {
        public abstract CCollectable CreateRandom(Random random, double x, double y, double lifetime);
    }
}
