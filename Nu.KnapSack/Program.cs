using System;
using System.Linq;
using FSharpFuncUtil;
using Microsoft.FSharp.Core;
using Nu.GeneticAlgorithm;

namespace Nu.KnapSack
{
    class Program
    {
        static void Main(string[] args)
        {
            var maxWeight = 20.0;
            var baseItems = new[]
            {
                new KnapSack.Item(10, 5, false),
                new KnapSack.Item(5, 2.5, false),
                new KnapSack.Item(5, 2.5, false),
                new KnapSack.Item(1, 10, false),
                new KnapSack.Item(1, 10, false),
                new KnapSack.Item(3, 7, false),
                new KnapSack.Item(23, 4, false),
                new KnapSack.Item(3, 1, false),
                new KnapSack.Item(15, 19, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(3, 7, false),
                new KnapSack.Item(23, 4, false),
                new KnapSack.Item(3, 1, false),
                new KnapSack.Item(15, 19, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(3, 7, false),
                new KnapSack.Item(23, 4, false),
                new KnapSack.Item(3, 1, false),
                new KnapSack.Item(15, 19, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
                new KnapSack.Item(0, 10, false),
            };
            var pop = Enumerable.Repeat(0, 200).Select(x => KnapSack.Randomize(maxWeight, baseItems)).ToArray();
            var fitness = FuncConvert.ToFSharpFunc<KnapSack, double>(KnapSack.Fitness);
            var crossover = new Func<KnapSack, KnapSack, KnapSack>(KnapSack.Crossover).ToFSharpFunc();
            var mutation = FuncConvert.ToFSharpFunc<KnapSack, KnapSack>(KnapSack.Mutate);
            var ga = new GeneticAlgorithm<KnapSack>(pop, fitness, crossover, mutation, .05);
            var result = ga.Run();
            Console.WriteLine($"Best Fitness: {KnapSack.Fitness(result)} Weight: {result.Weight}");
            Console.ReadKey();
        }
    }


    class KnapSack
    {
        private static readonly Random rand = new Random();
        public static KnapSack Randomize(double maxWeight, Item[] contents)
        {
            var newConetents = contents.Select(x => new Item(x.Value, x.Weight, false)).ToArray();
            var result = new KnapSack(maxWeight, newConetents);
            while (true)
            {
                var smallEnough = result.Contents.Where(x => result.MaxWeight >= (result.Weight + x.Weight) && !x.InSack).ToArray();
                if (!smallEnough.Any()) break;
                var i = rand.Next(0, smallEnough.Length);
                smallEnough[i].InSack = true;
            }
            return result;
        }

        public static KnapSack Crossover(KnapSack a, KnapSack b)
        {
            return new KnapSack(a.MaxWeight, Enumerable.Range(0, a.Contents.Length).Select(i => rand.Next(0, 2) == 0 ? a.Contents[i] : b.Contents[i]).ToArray());
        }

        public static double Fitness(KnapSack a)
        {
            if (a.Weight > a.MaxWeight)
            {
                return 0;
            }
            return a.Value;
        }

        public static KnapSack Mutate(KnapSack a)
        {
            var smallEnough = a.Contents.Where(x => !x.InSack && x.Weight + a.Weight <= a.MaxWeight).ToArray();
            if (smallEnough.Any())
            {
                smallEnough[rand.Next(0, smallEnough.Length)].InSack = true;
            }
            else
            {
                var inSack = a.Contents.Where(x => x.InSack).ToArray();
                if (inSack.Any())
                {
                    inSack[rand.Next(0, inSack.Length)].InSack = false;
                }
            }
            return a;
        }


        public class Item
        {
            public Item(double value, double weight, bool inSack)
            {
                Value = value;
                Weight = weight;
                InSack = inSack;
            }

            public double Value { get; set; }

            public double Weight { get; set; }

            public bool InSack { get; set; }

            public override string ToString()
            {
                return $"Value: {Value} Weight:{Weight} InSack:{InSack}";
            }
        }


        public double Value
        {
            get { return Contents.Sum(x => x.InSack ? x.Value : 0); }
        }

        public double Weight
        {
            get { return Contents.Sum(x => x.InSack ? x.Weight : 0); }
        }

        public double MaxWeight { get; set; }

        public Item[] Contents { get; }

        public KnapSack(double maxWeight, Item[] contents)
        {
            MaxWeight = maxWeight;
            Contents = contents;
        }

        public override string ToString()
        {
            return $"Fitness: {Fitness(this)} Value: {Value} Weight: {Weight}";
        }
    }
}
