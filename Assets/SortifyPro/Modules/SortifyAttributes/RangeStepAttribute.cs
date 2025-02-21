#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class RangeStepAttribute : PropertyAttribute
    {
        public float Min;
        public float Max;
        public float Step;

        /// <summary>
        /// Initializes a new instance of the RangeStepAttribute with specified minimum, maximum values, and step size for the slider.
        /// </summary>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <param name="step">The step size for the slider.</param>
        public RangeStepAttribute(float min, float max, float step)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
#endif
