using System;
using System.Collections.Generic;

namespace CK2LandedTitlesManager.Models
{
    public sealed class CulturalGroupMatchingMode : IEquatable<CulturalGroupMatchingMode>
    {
        static readonly Dictionary<int, CulturalGroupMatchingMode> enumeration = new Dictionary<int, CulturalGroupMatchingMode>
        {
            { FirstOnlyPriority.Value, FirstOnlyPriority },
            { AscendingPriority.Value, AscendingPriority }
        };

        public static IEnumerable<CulturalGroupMatchingMode> Values => enumeration.Values;

        public static CulturalGroupMatchingMode FirstOnlyPriority => new CulturalGroupMatchingMode(1, nameof(FirstOnlyPriority));

        public static CulturalGroupMatchingMode AscendingPriority => new CulturalGroupMatchingMode(2, nameof(AscendingPriority));

        public static CulturalGroupMatchingMode EqualPriority => new CulturalGroupMatchingMode(3, nameof(EqualPriority));

        public int Value { get; }

        public string Name { get; }

        CulturalGroupMatchingMode(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public bool Equals(CulturalGroupMatchingMode other)
        {
            if (other is null)
            {
                return false;
            }

            return
                Value == other.Value &&
                Name == other.Name;
        }

        public override bool Equals(object other)
        {
            if (other is CulturalGroupMatchingMode)
            {
                return Equals((CulturalGroupMatchingMode)other);
            }
            else if (other is int)
            {
                return Value == (int)other;
            }

            return false;
        }
        
        public static bool operator ==(CulturalGroupMatchingMode self, CulturalGroupMatchingMode other) => self.Equals(other);
        public static bool operator !=(CulturalGroupMatchingMode self, CulturalGroupMatchingMode other) => !self.Equals(other);

        public static bool operator ==(CulturalGroupMatchingMode self, int value) => self.Value == value;
        public static bool operator !=(CulturalGroupMatchingMode self, int value) => self.Value != value;

        public override int GetHashCode()
        {
            return
                Value.GetHashCode() ^
                Name.GetHashCode();
        }
    }
}
