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
    //This program prints out prime numbers that get generated using the Crptography and Parallel classes
    //It requires an integer arguement for the number of bits that the prime number should be as well as
    //a count for the total number to print.
    static class PrimeGen
    {
        static Found fnd = new Found();
        static int count = 1;
        static Boolean done = false;
        static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        static int byteCount = 0;

        //Main function that reads in the arguments and outputs an error message if anything is incorrect
        //Uses the Parallel class to generate threads that create and check if numbers are prime
        //Use the stopwatch to record the total time it took to print all the numbers
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
            catch (IndexOutOfRangeException)
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

            Console.WriteLine(String.Format("BitLength: {0} bits", byteCount));

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

        //This is used in the Parallel for loop to continue spawning threads until the total count has been found
        static IEnumerable<bool> LoopTillFalse()
        {
            while (!done) yield return true;
        }

        //Generates a number with the same bit count as the number specified in the arguments
        //checks if the generated number is prime. If it is then it outputs the number and adds one to the found count
        static void FindPrime()
        {
            Byte[] num = new byte[byteCount];
            rng.GetBytes(num);
            var result = new BigInteger(num);
            if (result.IsProbablyPrime() & !done)
            {
                if (fnd.found >= count)
                {
                    done = true;
                }
                if (!done)
                {
                    lock(fnd)
                    {
                        Console.WriteLine(String.Format("{0}: {1}", ++fnd.found, result));
                    }
                }
            }
        }

        //used to check if the given number is prime
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

    //This class is used so that the found variable can be locked
    //Used to make sure that the output is always outputting the numbers in order.
    class Found
    {
        public int found { get; set; }
    }
}
