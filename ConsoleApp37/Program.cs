using System;
using System.IO;
using System.Threading;

class Program
{
    private static readonly object LockObject = new object();
    private static readonly ManualResetEvent generatorComplete = new ManualResetEvent(false);

    static void Main(string[] args)
    {
        Thread generatorThread = new Thread(GenerateAndSavePairs);
        Thread sumThread = new Thread(CalculateSum);
        Thread productThread = new Thread(CalculateProduct);

        generatorThread.Start();
        sumThread.Start();
        productThread.Start();

        generatorThread.Join();

        generatorComplete.Set();

        sumThread.Join();
        productThread.Join();

        Console.WriteLine("Done");
    }

    static void GenerateAndSavePairs()
    {
        Random random = new Random();
        using (StreamWriter writer = new StreamWriter("pairs.txt"))
        {
            for (int i = 0; i < 10; i++)
            {
                int num1 = random.Next(1, 101);
                int num2 = random.Next(1, 101);
                string pair = $"{num1},{num2}";

                lock (LockObject)
                {
                    writer.WriteLine(pair);
                }

                Console.WriteLine($"Pair: {pair}");
                Thread.Sleep(100);
            }
        }

        generatorComplete.Set();
    }

    static void CalculateSum()
    {
        generatorComplete.WaitOne();

        int sum = 0;
        using (StreamReader reader = new StreamReader("pairs.txt"))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                int num1 = int.Parse(parts[0]);
                int num2 = int.Parse(parts[1]);

                sum += num1 + num2;
            }
        }

        lock (LockObject)
        {
            using (StreamWriter writer = new StreamWriter("sum.txt"))
            {
                writer.WriteLine($"Sum: {sum}");
            }
        }

        Console.WriteLine($"Sum calculated: {sum}");
    }

    static void CalculateProduct()
    {
        generatorComplete.WaitOne();

        int product = 1;
        using (StreamReader reader = new StreamReader("pairs.txt"))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                int num1 = int.Parse(parts[0]);
                int num2 = int.Parse(parts[1]);

                product *= num1 * num2;
            }
        }

        lock (LockObject)
        {
            using (StreamWriter writer = new StreamWriter("product.txt"))
            {
                writer.WriteLine($"Product: {product}");
            }
        }

        Console.WriteLine($"Product calculated: {product}");
    }
}