#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class ProgressBarAttribute : PropertyAttribute
    {
        public float Min;
        public float Max;
        public bool IsGradient;

        /// <summary>
        /// Initializes a new instance of the ProgressBarAttribute with specified minimum and maximum values and an option to enable a gradient.
        /// </summary>
        /// <param name="min">The minimum value of the progress bar.</param>
        /// <param name="max">The maximum value of the progress bar.</param>
        /// <param name="isGradient">If true, enables a gradient effect from the start to the end of the progress bar. Default is false.</param>
        public ProgressBarAttribute(float min, float max, bool isGradient = false)
        {
            Min = min;
            Max = max;
            IsGradient = isGradient;
        }
    }
}
#endif
