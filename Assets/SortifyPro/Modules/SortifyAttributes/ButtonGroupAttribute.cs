#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class ButtonGroupAttribute : PropertyAttribute
    {
        public bool IsVertical;
        public bool ShowVariableName;

        /// <summary>
        /// Initializes a new instance of the ButtonGroupAttribute with options for vertical layout and displaying the variable name.
        /// </summary>
        /// <param name="isVertical">If true, arranges buttons vertically; otherwise, arranges them horizontally. Default is false.</param>
        /// <param name="showVariableName">If true, displays the variable name above the button group. Default is false.</param>
        public ButtonGroupAttribute(bool isVertical = false, bool showVariableName = false)
        {
            IsVertical = isVertical;
            ShowVariableName = showVariableName;
        }
    }
}
#endif
