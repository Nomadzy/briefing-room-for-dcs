using System;

namespace BriefingRoom4DCSWorld.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IntegerSourceAttribute : Attribute
    {
        public int Min { get; }
        public int Max { get; }
        public int Increment { get; }
        public string Format { get; }
        public int? RandomValue { get; }

        public IntegerSourceAttribute(int min, int max, int increment = 1, string format = "%i", bool useRandomValue = false, int randomValue = -1)
        {
            Min = Math.Min(min, max);
            Max = Math.Max(min, max);
            Increment = Math.Max(1, increment);
            Format = format;
            if (useRandomValue) RandomValue = randomValue;
            else RandomValue = null;
        }
    }
}
