#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class ClassDrawerAttribute : PropertyAttribute
    {
        public string Header;

        /// <summary>
        /// Adds a custom header above a serialized class in the inspector.
        /// </summary>
        /// <param name="header">The text displayed as the header.</param>
        public ClassDrawerAttribute(string header = "")
        {
            Header = header;
        }
    }
}
#endif
