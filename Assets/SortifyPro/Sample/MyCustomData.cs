using UnityEngine;

namespace Sortify
{
    [CreateAssetMenu(menuName = "Sortify/MyCustomData")]
    public class MyCustomData : ScriptableObject
    {
        public string dataName;
        public int dataValue;

#if SORTIFY_ATTRIBUTES
        [Button]
#endif
        private void TestMethod()
        {
            Debug.Log("Scriptable Object test method.");
        }
    }
}
