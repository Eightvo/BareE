using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BareE.RNG
{
    //http://www0.cs.ucl.ac.uk/staff/d.jones/GoodPracticeRNG.pdf



    public class KISSPrng:IPrng
    {
        static byte[] Salt = new byte[64]
        {
            28,94,17,145,20,23,143,7,195,63,130,56,13,166,144,16,
            134,217,134,175,82,212,49,52,56,154,249,34,58,138,160,8,
            107,165,27,10,207,52,64,187,59,34,119,196,3,68,95,104,
            23,119,238,103,108,25,50,149,121,150,208,223,134,173,180,2,
        };


         UInt64 x = 123456789;
         UInt64 y = 362436000;
         UInt64 z = 521288629;
         UInt64 c = 7654321; 

        /// <summary>
        /// The seed is combined with a Salt to produce a 64byte value which is then decomposed into 4 64 bit integers
        /// </summary>
        /// <param name="seed"></param>
        public void Seed(String seed=null)
        {
            byte[] seedBytes=new byte[64];
            if (String.IsNullOrEmpty(seed))
            {
                Random r = new Random();
                r.NextBytes(seedBytes);
            }
            else
            {
                seedBytes = GetHash(seed);
                
            }
            x = 0;
            y = 0;
            z = 0;
            c = 0;
            for (int i = 0; i < 16; i++)
            {
                x = x | ((UInt64)(Salt[i + 16 * 0] ^ seedBytes[i + 16 * 0])) << (4 * i);
                y = y | ((UInt64)(Salt[i + 16 * 1] ^ seedBytes[i + 16 * 1])) << (4 * i);
                z = z | ((UInt64)(Salt[i + 16 * 2] ^ seedBytes[i + 16 * 2])) << (4 * i);
                c = c | ((UInt64)(Salt[i + 16 * 3] ^ seedBytes[i + 16 * 3])) << (4 * i);
            }
            return;
        }
        public KISSPrng(String seed=null)
        {
            Seed(seed);
        }
         public uint KISS()
         {
            
            UInt64 t = 698769069;
            UInt64 a = 698769069;
            unchecked
            {
                x = 69069 * x + 12345;

                /* y must never be set to zero! */
                y ^= (y << 13);
                y ^= (y >> 17); 
                y ^= (y << 5);

                /* Also avoid setting z=c=0! */
                t = a * z + c; 
                c = (t >> 32); 


                return (uint)(x + y + (z - t));
            }
        }

        public byte[] GetHash(string inputString)
        {
            Console.WriteLine($"{inputString}");
            using (HashAlgorithm algorithm = SHA512.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        void IPrng.SetSeed(String seed)
        {
            Seed(seed);
        }

        double IPrng.Next()
        {
            double x;
            uint a, b;
            a = KISS() >> 6; /* Upper 26 bits */
            b = KISS() >> 5; /* Upper 27 bits */
            x = (a * 134217728.0 + b) / 9007199254740992.0;
            return x;
        }
    }
}
