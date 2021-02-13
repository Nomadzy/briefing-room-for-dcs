using System;

namespace BriefingRoom4DCSWorld.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DatabaseSourceAttribute : Attribute
    {
        public Type DBEntryType { get; }
        public bool AllowRandom { get; }

        public DatabaseSourceAttribute(Type dbEntryType, bool allowRandom = false)
        {
            DBEntryType = dbEntryType;
            AllowRandom = allowRandom;
        }
    }
}
