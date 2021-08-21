using System;

namespace BareE.RNG
{
    public interface IPrng
    {
        public void SetSeed(String seed);
        public double Next();
    }
    public class SystemRng : IPrng
    {
        System.Random rng;
        public SystemRng()
        {
            rng = new Random();
        }
        public SystemRng(int seed, params object[] exDat)
        {
            rng = new Random(seed);
        }
        /// <summary>
        /// Seed is a string Representation of an integer
        /// </summary>
        /// <param name="seed"></param>
        public void SetSeed(String seed)
        {
            rng = new Random(int.Parse(seed));
        }
        public double Next()
        {
            return rng.NextDouble();
        }


    }

    
}
