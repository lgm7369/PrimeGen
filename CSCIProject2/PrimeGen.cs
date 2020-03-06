//Logan McHerron
//CSCI PrimeGen Project
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PrimeGen
{
    static class PrimeGen
    {
        static int found = 0;
        static int count = 1;
        static Boolean done = false;
        static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        static int byteCount = 0;
        static void Main(string[] args)
        {
            var Error = "Invalid arguments. \n" +
                        "dotnet run <bits> <count=1> \n" +
                        "- bits - the number of bits of the prime number, this must be a \n" +
                        "multiple of 8, and at least 32 bits.\n" +
                        " - count - the number of prime numbers to generate, defaults to 1";
          
            try
            {
                if (Int32.Parse(args[0]) < 32 || Int32.Parse(args[0]) % 8 != 0)
                {
                    Console.WriteLine(Error);
                    Environment.Exit(0);
                }
                byteCount = Int32.Parse(args[0]);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine(Error);
                Environment.Exit(0);
            }
            try
            {
                if (args.Length > 1)
                {
                    count = Int32.Parse(args[1]);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine(Error);
                Environment.Exit(0);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine(Error);
                Environment.Exit(0);
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Parallel.ForEach(LoopTillFalse(), (Action) =>
             {
                 FindPrime();
             });
            watch.Stop();
            var ts = watch.Elapsed;
            Console.WriteLine(
                String.Format("Time to Generate: {0:00}:{1:00}:{2:00}.{3:00}",
                ts.TotalHours,ts.TotalMinutes,ts.TotalSeconds, ts.TotalMilliseconds / 10)
            );

        }

        static IEnumerable<bool> LoopTillFalse()
        {
            while (!done) yield return true;
        }

        static void FindPrime()
        {
            Byte[] num = new byte[byteCount];
            rng.GetBytes(num);
            var result = new BigInteger(num);
            if (result.IsProbablyPrime() & !done)
            {
                if (found >= count)
                {
                    done = true;
                }
                if (!done)
                {
                    Console.WriteLine(String.Format("{0}: {1}", Interlocked.Increment(ref found), result));                 
                }
            }
        }

        static Boolean IsProbablyPrime(this BigInteger value, int witnesses = 10)
        {
            if (value <= 1) return false;
            if (witnesses <= 0) witnesses = 10;
            BigInteger d = value - 1;
            int s = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            Byte[] bytes = new Byte[value.ToByteArray().LongLength];
            BigInteger a;
            for (int i = 0; i < witnesses; i++)
            {
                do
                {
                    var Gen = new Random();
                    Gen.NextBytes(bytes);
                    a = new BigInteger(bytes);
                } while (a < 2 || a >= value - 2);
                BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1) continue;
                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, value);
                    if (x == 1) return false;
                    if (x == value - 1) break;
                }
                if (x != value - 1) return false;
            }
            return true;
        }

    }
}
