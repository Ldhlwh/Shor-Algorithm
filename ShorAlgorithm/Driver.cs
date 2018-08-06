using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;

namespace ShorAlgorithm
{
    class Driver
    {
        static int OrderFinding(int x, int N, int t, int L)
        {
            // Fiding the least r s.t. x ^ r % N = 1
            // t: number of bits in the 1st register, depends on our demand on accuracy
            // L: number of bits in the 2nd register, depends on the magnitude of N

            using (var sim = new QuantumSimulator())
            {
                // Prepare for Phase Estimation
                var res = QuantumOF.Run(sim, x, N, t, L).Result;
                // The Continued Fractions Algorithm
                int[] frac = new int[2 * t];
                int up = 0;
                while (true)
                {
                    res = 1.0 / res;
                    frac[up] = (int)Math.Floor(res);
                    res = res - frac[up];
                    up++;
                    if (Math.Abs(res) < 1e-6)
                    {
                        break;
                    }
                }
                int nmr = 0, dnm = 1;   // fraction = nmr / dnm
                int temp;
                for (int i = up - 1; i >= 0; i--)
                {
                    nmr = nmr + dnm * frac[i];
                    temp = nmr; // 
                    nmr = dnm;  //  swap nmr and dnm
                    dnm = temp; // 
                }
                int r = dnm;
                return r;
                // Verify that r is the order, return -1 if not
            }
        }

        static int gcd(int a, int b)
        {
            if (b == 0) return a;
            else return gcd(b, a % b);
        }

        static int pow(int a, int r, int N)
        {
            int tmp = 1;
            for (int i = 0; i < r; ++i)
                tmp = tmp * a % N;
            return tmp;
        }

        static void Main(string[] args)
        {
            Random rd = new Random();
            bool[] arr = new bool[31];
            //for (int N = 3; N <= 31; N = N + 2)
            for (int N = 15; N <= 21; N += 6)
            {
                for (int i = 2; i < N; ++i)
                {
                    arr[i] = false;
                }
                int tot = 0;
                Console.WriteLine("Try to factorize " + N + ":");
                Console.WriteLine("--------------------");
                while (true)
                {
                    if (tot + 2 == N)
                    {
                        Console.WriteLine(N + " can't be factorized");
                        break;
                    }
                    int a = rd.Next(2, N);
                    if (arr[a])
                        continue;
                    Console.Write("try a = " + a);
                    tot++;
                    arr[a] = true;
                    if (gcd(N, a) != 1)
                    {
                        Console.WriteLine(": gcd(N, a) != 1, we continue...");
                        continue; //if gcd(a,N) != 1 the factor is done, but it's too easy for factoring small number
                    }
                    int r = OrderFinding(a, N, 5, 7);
                    Console.Write(", r = " + r);
                    if (r >= N)
                    {
                        Console.WriteLine(": FAILED, r >= N");
                        continue;
                    }
                    int mc = r, c = 1, d = a;
                    while (mc > 0)
                    {
                        if (mc % 2 == 1)
                        {
                            c = c * d % N;
                        }
                        d = d * d % N;
                        mc = mc / 2;
                    }
                    if (c != 1)
                    {
                        Console.WriteLine(": FAILED, wrong order.");
                        continue;
                    }
                    if (r % 2 != 0)
                    {
                        Console.WriteLine(": FAILED, r is not even.");
                        continue;
                    }
                    if ((pow(a, r / 2, N) + 1) % N == 0)
                    {
                        Console.WriteLine(": FAILED, (a ^ (r / 2) + 1) % N = 0");
                        continue;
                    }
                    Console.WriteLine(": ACCEPTED!");
                    //Console.WriteLine("a = " + a + ", r = " + r);

                    int x;
                    if (pow(a, r / 2, N) - 1 > N)
                        x = gcd(pow(a, r / 2, N) - 1, N);
                    else
                        x = gcd(N, pow(a, r / 2, N) - 1);
                    int y;
                    if (pow(a, r / 2, N) + 1 > N)
                        y = gcd((int)pow(a, r / 2, N) + 1, N);
                    else
                        y = gcd(N, pow(a, r / 2, N) + 1);
                    Console.WriteLine(N + " = " + x + " * " + y + "\n\n");
                    break;
                }

            }
            Console.ReadKey();
        }
    }
}