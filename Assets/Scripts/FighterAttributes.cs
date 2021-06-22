using UnityEngine;

namespace Fighter.Attributes
{
    public class MinMaxAttribute : PropertyAttribute
    {
        public float minLimit = 0;
        public float maxLimit = 1;
        public bool showEditRange;
        public bool showDebugValues;

        public MinMaxAttribute(int min, int max)
        {
            minLimit = min;
            maxLimit = max;
        }
    }
}