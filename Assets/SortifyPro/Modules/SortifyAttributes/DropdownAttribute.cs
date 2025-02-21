#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class DropdownAttribute : PropertyAttribute
    {
        public string MethodName;
        public bool IsValuePair;

        /// <summary>
        /// Constructor for the Dropdown attribute.
        /// </summary>
        /// <param name="methodName">The name of the method that returns the list of options.</param>
        /// <param name="isValuePair">Indicates whether the method returns key-value pairs.</param>
        public DropdownAttribute(string methodName, bool isValuePair = false)
        {
            MethodName = methodName;
            IsValuePair = isValuePair;
        }
    }
}
#endif
