using System;

namespace BriefingRoom4DCSWorld.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ParentPropertyAttribute : Attribute
    {
        public string PropertyName { get; }

        public ParentPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
