#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class ToggleAttribute : PropertyAttribute
    {
        public float AnimationSpeed { get; }

        /// <summary>
        /// Initializes a new instance of the ToggleAttribute with an optional animation speed for the toggle effect.
        /// </summary>
        /// <param name="animationSpeed">The speed of the toggle animation in the Inspector. Default is 4f.</param>
        public ToggleAttribute(float animationSpeed = 4f)
        {
            AnimationSpeed = animationSpeed;
        }
    }
}
#endif
