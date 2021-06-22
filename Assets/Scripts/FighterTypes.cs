using UnityEngine;
using System;

namespace Fighter.Types
{
    [Serializable]
    public class RangedFloat
    {
        public float min; 
        public float max; 

        public float Get()
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static implicit operator float(RangedFloat value)
        {
            return value.Get();
        }
    }
}