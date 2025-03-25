#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

namespace Sortify
{
    public class SortifyCustomAttributeSample : MonoBehaviour
    {
#if SORTIFY_ATTRIBUTES
        public enum ButtonGroup
        {
            Option1,
            Option2,
            Option3,
            Option4,
            Option5,
        }

        [Toggle(1.5f)]
        public bool SlowToggle = false;
        [Toggle]
        public bool NormalToggle = false;
        [Toggle(6f)]
        public bool FastToggle = false;

        [Space(5)]

        [ButtonGroup]
        public ButtonGroup HorizontalGroup;

        [Space(5)]

        [ShowIf("HorizontalGroup", ButtonGroup.Option2)]
        public string option2Description = "This is shown when Option2 is selected.";

        [ShowIf("HorizontalGroup", ButtonGroup.Option3, true)]
        public string option3Description = "This is shown when Option3 is deselected.";

        [Space(5)]

        [ButtonGroup(true, true)]
        public ButtonGroup VerticalGroup;

        [Toggle]
        public bool showHealthBar;

        [Space(5)]
        [ShowIf("showHealthBar")]
        public float healthBar = 100f;

        [Space(5)]

        [Button("Change Bool Button")]
        public bool Button = false;

        [Space(5)]
        [Message(MessageAttribute.MessageType.Info, "Example info message.")]
        public string InfoMessage = "Info Message";

        [Space(5)]
        [Message(MessageAttribute.MessageType.Warning, "Example warning message.")]
        public string WarningMessage = "Warning Message";

        [Space(5)]
        [Message(MessageAttribute.MessageType.Error, "Example error message.")]
        public string ErrorMessage = "Error Message";

        [Space(5)]
        [MinMaxSlider(0, 1)]
        public Vector2 MinMaxSlider;

        [Space(5)]
        [RangeStep(0f, 1f, 0.2f)]
        public float RangeStep;

        [Space(5)]
        [ReadOnly]
        public float ReadOnly = 5;

        [Space(5)]
        [ProgressBar(0, 100, true)]
        public float HealthBar = 100;

        [Space(5)]
        [ProgressBar(0, 150)]
        public float ManaBar = 56;


        [Space(5)]
        [BetterHeader("Better Header Default")]
        public float BetterHeader_Default;

        [Space(5)]
        [BetterHeader("Better Header Middle Center", 16, TextAnchor.MiddleCenter)]
        public float BetterHeader_MiddleCenter;

        [Space(5)]
        [BetterHeader("Better Header Middle Right", 22, TextAnchor.MiddleRight)]
        public float BetterHeader_MiddleRight;

        [Space(5)]
        [Validate("CheckValidateFloat", "Variable is negative!")]
        public float ValidateFloat = 1f;
        private bool CheckValidateFloat()
        {
            return ValidateFloat > 0;
        }

        [Space(5)]
        [Unit("kg")]
        public float Weight = 60f;
        [Unit("m")]
        public float Height = 1.75f;
        [Unit("cm")]
        public float Width = 45f;

        [Space(5)]
        [Editable]
        public ScriptableObject ScriptableObject;

        [Editable]
        public Material materialReference;

        [Editable]
        public GameObject prefab;

        [Space(5)]

        [Dropdown("GetOptions")]
        public string selectedStringOption;
        private IEnumerable<string> GetOptions()
        {
        return new List<string> { "Option A", "Option B", "Option C", "Option D" };
        }

        [Dropdown("GetOptionsWithValues", true)]
        public int selectedIntOption;
        private List<KeyValuePair<string, object>> GetOptionsWithValues()
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Low", 1),
                new KeyValuePair<string, object>("Medium", 2),
                new KeyValuePair<string, object>("High", 3),
                new KeyValuePair<string, object>("Ultra", 4)
            };
        }
        
        [Dropdown("GetMaterials")]
        public Material selectedMaterialOption;
        private IEnumerable<UnityEngine.Object> GetMaterials()
        {
            return new List<UnityEngine.Object>(Resources.FindObjectsOfTypeAll<Material>());
        }
        
        [Dropdown("GetCustomDataFromAssetDatabase")]
        public MyCustomData selectedCustomData;
        private IEnumerable<UnityEngine.Object> GetCustomDataFromAssetDatabase()
        {
            var guids = AssetDatabase.FindAssets("t:MyCustomData");
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<MyCustomData>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        [Space(5)]
        
        [Tag]
        public string tag;
        [Layer]
        public int layer;
        
        [Serializable]
        public class TestClass
        {
            [Toggle]
            public bool NormalToggle = false;

            public string name;
            public int length;
            [ButtonGroup]
            public ButtonGroup HorizontalGroup;

            [ClassDrawer]
            public TestClass2 testClass;
        }

        [Serializable]
        public class TestClass2
        {
            public string name;
            public int length;
            [Space(5)]
            [ProgressBar(0, 100, true)]
            public float HealthBar = 100;

            [Space(5)]
            [ProgressBar(0, 150)]
            public float ManaBar = 56;

            [ClassDrawer]
            public TestClass3 testClass;
        }

        [Serializable]
        public class TestClass3
        {
            public string name;
            public int length;
            [ClassDrawer]
            public TestClass4 testClass;
        }

        [Serializable]
        public class TestClass4
        {
            public string name;
            public int length;

            [Button("Invoke Methode")]
            public void ButtonMethode()
            {
                Debug.Log("Invoke!");
            }
        }

        [Space(5)]
        [ClassDrawer]
        public TestClass testClass;

#if SORTIFY_COLLECTIONS
        [Space(5)]
        public SDictionary<ButtonGroup, List<TestClass>> testSDictionary2;
        public SDictionary<int, Transform> testSDictionary;
#endif

        [Space(5)]
        [AudioPreview]
        public AudioClip audioClip;

        [Button("Invoke Methode")]
        public void ButtonMethode()
        {
            Debug.Log("Invoke!");
        }

        private void Start()
        {
            testSDictionary.Add(3, transform);
            testSDictionary.Add(4, transform);
        }
#endif
    }
}
#endif


