using WordSoul.Domain.Enums;
using System.Collections.Generic;

namespace WordSoul.Domain.DomainServices
{
    public static class TypeEffectivenessCalculator
    {
        private static readonly Dictionary<PetType, Dictionary<PetType, double>> TypeChart = new()
        {
            [PetType.Normal] = new() { [PetType.Rock] = 0.5, [PetType.Ghost] = 0, [PetType.Steel] = 0.5 },
            [PetType.Fire] = new() { [PetType.Fire] = 0.5, [PetType.Water] = 0.5, [PetType.Grass] = 2.0, [PetType.Ice] = 2.0, [PetType.Bug] = 2.0, [PetType.Rock] = 0.5, [PetType.Dragon] = 0.5, [PetType.Steel] = 2.0 },
            [PetType.Water] = new() { [PetType.Fire] = 2.0, [PetType.Water] = 0.5, [PetType.Grass] = 0.5, [PetType.Ground] = 2.0, [PetType.Rock] = 2.0, [PetType.Dragon] = 0.5 },
            [PetType.Electric] = new() { [PetType.Water] = 2.0, [PetType.Electric] = 0.5, [PetType.Grass] = 0.5, [PetType.Ground] = 0, [PetType.Flying] = 2.0, [PetType.Dragon] = 0.5 },
            [PetType.Grass] = new() { [PetType.Fire] = 0.5, [PetType.Water] = 2.0, [PetType.Grass] = 0.5, [PetType.Poison] = 0.5, [PetType.Ground] = 2.0, [PetType.Flying] = 0.5, [PetType.Bug] = 0.5, [PetType.Rock] = 2.0, [PetType.Dragon] = 0.5, [PetType.Steel] = 0.5 },
            [PetType.Ice] = new() { [PetType.Fire] = 0.5, [PetType.Water] = 0.5, [PetType.Grass] = 2.0, [PetType.Ice] = 0.5, [PetType.Ground] = 2.0, [PetType.Flying] = 2.0, [PetType.Dragon] = 2.0, [PetType.Steel] = 0.5 },
            [PetType.Fighting] = new() { [PetType.Normal] = 2.0, [PetType.Ice] = 2.0, [PetType.Poison] = 0.5, [PetType.Flying] = 0.5, [PetType.Psychic] = 0.5, [PetType.Bug] = 0.5, [PetType.Rock] = 2.0, [PetType.Ghost] = 0, [PetType.Dark] = 2.0, [PetType.Steel] = 2.0, [PetType.Fairy] = 0.5 },
            [PetType.Poison] = new() { [PetType.Grass] = 2.0, [PetType.Poison] = 0.5, [PetType.Ground] = 0.5, [PetType.Rock] = 0.5, [PetType.Ghost] = 0.5, [PetType.Steel] = 0, [PetType.Fairy] = 2.0 },
            [PetType.Ground] = new() { [PetType.Fire] = 2.0, [PetType.Electric] = 2.0, [PetType.Grass] = 0.5, [PetType.Poison] = 2.0, [PetType.Flying] = 0, [PetType.Bug] = 0.5, [PetType.Rock] = 2.0, [PetType.Steel] = 2.0 },
            [PetType.Flying] = new() { [PetType.Electric] = 0.5, [PetType.Grass] = 2.0, [PetType.Fighting] = 2.0, [PetType.Bug] = 2.0, [PetType.Rock] = 0.5, [PetType.Steel] = 0.5 },
            [PetType.Psychic] = new() { [PetType.Fighting] = 2.0, [PetType.Poison] = 2.0, [PetType.Psychic] = 0.5, [PetType.Dark] = 0, [PetType.Steel] = 0.5 },
            [PetType.Bug] = new() { [PetType.Fire] = 0.5, [PetType.Grass] = 2.0, [PetType.Fighting] = 0.5, [PetType.Poison] = 0.5, [PetType.Flying] = 0.5, [PetType.Psychic] = 2.0, [PetType.Ghost] = 0.5, [PetType.Dark] = 2.0, [PetType.Steel] = 0.5, [PetType.Fairy] = 0.5 },
            [PetType.Rock] = new() { [PetType.Fire] = 2.0, [PetType.Ice] = 2.0, [PetType.Fighting] = 0.5, [PetType.Ground] = 0.5, [PetType.Flying] = 2.0, [PetType.Bug] = 2.0, [PetType.Steel] = 0.5 },
            [PetType.Ghost] = new() { [PetType.Normal] = 0, [PetType.Psychic] = 2.0, [PetType.Ghost] = 2.0, [PetType.Dark] = 0.5 },
            [PetType.Dragon] = new() { [PetType.Dragon] = 2.0, [PetType.Steel] = 0.5, [PetType.Fairy] = 0 },
            [PetType.Dark] = new() { [PetType.Fighting] = 0.5, [PetType.Psychic] = 2.0, [PetType.Ghost] = 2.0, [PetType.Dark] = 0.5, [PetType.Fairy] = 0.5 },
            [PetType.Steel] = new() { [PetType.Fire] = 0.5, [PetType.Water] = 0.5, [PetType.Electric] = 0.5, [PetType.Ice] = 2.0, [PetType.Rock] = 2.0, [PetType.Steel] = 0.5, [PetType.Fairy] = 2.0 },
            [PetType.Fairy] = new() { [PetType.Fire] = 0.5, [PetType.Fighting] = 2.0, [PetType.Poison] = 0.5, [PetType.Dragon] = 2.0, [PetType.Dark] = 2.0, [PetType.Steel] = 0.5 }
        };

        public static double Calculate(PetType attackType, PetType defendType1, PetType? defendType2 = null)
        {
            double multiplier = 1.0;

            if (TypeChart.TryGetValue(attackType, out var attackMatches))
            {
                if (attackMatches.TryGetValue(defendType1, out double m1))
                {
                    multiplier *= m1;
                }

                if (defendType2.HasValue && defendType2.Value != defendType1)
                {
                    if (attackMatches.TryGetValue(defendType2.Value, out double m2))
                    {
                        multiplier *= m2;
                    }
                }
            }

            return multiplier;
        }

        public static string GetEffectivenessText(double multiplier)
        {
            if (multiplier > 1.0) return "Super Effective!";
            if (multiplier < 1.0 && multiplier > 0) return "Not very effective...";
            if (multiplier == 0) return "No effect!";
            return "";
        }
    }
}
