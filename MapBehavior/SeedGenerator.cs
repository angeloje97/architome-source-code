using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SeedGenerator : MonoBehaviour
    {
        // Start is called before the first frame update
        public static SeedGenerator active { get; private set; }
        public int seed;
        public string seedString;

        //public List<int> factors;
        public int factorCount;

        

        public void GenerateSeed()
        {
            if (seedString.Length == 0)
            {
                seedString = RandomGen.RandomString(10);
            }

            if (seedString != null && seedString.Length > 0)
            {
                UseSeedString();
            }
            else
            {
                CreateSeed();
            }

            GenerateFactors();

            void CreateSeed()
            {
                seed = Random.Range(1000, 1000000);
            }

            void UseSeedString()
            {
                seed = seedString.GetHashCode();
            }
            void GenerateFactors()
            {
                for (int i = 0; i < factorCount; i++)
                {
                    if (i == 0)
                    {
                        //factors.Add(0);
                    }
                    else
                    {
                        var current = seed % i;

                        if (current < 0)
                        {
                            current *= -1;
                        }
                        //factors.Add(current);
                    }
                }

                Random.InitState(seed);
            }
        }
        void Awake()
        {
            active = this;
            AcquireSeed();
            GenerateSeed();
        }
        void AcquireSeed()
        {
            if (Core.dungeonSeed == null) return;
            if (Core.dungeonSeed.Length == 0) return;


            seedString = Core.dungeonSeed;

        }

        // Update is called once per frame
        void Update()
        {

        }


        public int Factor2(int num1, int max)
        {
            if (max == 0) { return 0; }
            var num = num1 * seed;

            var result = num % max;
            if (result < 0) { result *= -1; }

            Debugger.InConsole(1942, $"{num1} * {seed} = {num}\n" +
                $"{result}");

            return result;
        }

        public int FactorMultiplied(int multiplier, int max)
        {
            if (max == 0) { return 0; }

            var newFactor = (seed * multiplier);
            int result = 0;

            return result;
        }
    }

}
