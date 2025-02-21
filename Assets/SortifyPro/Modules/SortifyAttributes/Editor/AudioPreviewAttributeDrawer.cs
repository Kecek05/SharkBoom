#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(AudioPreviewAttribute))]
    public class AudioPreviewAttributeDrawer : PropertyDrawer
    {
        private static AudioSource _editorAudioSource;
        private static float _volume = 1f;
        private static bool _isPlaying = false;
        private static bool _isPaused = false;

        static AudioPreviewAttributeDrawer()
        {
            EditorApplication.focusChanged += OnFocusChanged;
        }

        ~AudioPreviewAttributeDrawer()
        {
            EditorApplication.focusChanged -= OnFocusChanged;
        }

        private static void OnFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
                StopAudioClip();
        }

        private static void InitializeAudioSource()
        {
            if (_editorAudioSource == null)
            {
                GameObject editorAudioObject = new GameObject("EditorAudioSource");
                editorAudioObject.hideFlags = HideFlags.HideAndDontSave;
                _editorAudioSource = editorAudioObject.AddComponent<AudioSource>();
                _editorAudioSource.playOnAwake = false;
                _editorAudioSource.loop = false;
                _editorAudioSource.volume = _volume;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is AudioClip)
                return EditorGUIUtility.singleLineHeight * 2 + 6;

            return EditorGUIUtility.singleLineHeight + 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is AudioClip audioClip)
            {
                InitializeAudioSource();

                Rect firstLine = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                Rect secondLine = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(firstLine, property, label);

                float buttonWidth = 24f;
                float timeLabelWidth = 40f;
                float volumeSliderWidth = 120f;
                float sliderWidth = secondLine.width - (buttonWidth * 3 + timeLabelWidth * 2 + volumeSliderWidth + 25f);
                float spacing = 4f;

                GUIContent playIcon = EditorGUIUtility.IconContent("PlayButton");
                GUIContent pauseIcon = EditorGUIUtility.IconContent("PauseButton");
                GUIContent stopIcon = EditorGUIUtility.IconContent("PreMatQuad");

                Rect playButtonRect = new Rect(secondLine.x, secondLine.y, buttonWidth, secondLine.height);
                if (GUI.Button(playButtonRect, playIcon))
                    PlayAudioClip(audioClip);

                Rect pauseButtonRect = new Rect(playButtonRect.xMax + spacing, secondLine.y, buttonWidth, secondLine.height);
                if (GUI.Button(pauseButtonRect, pauseIcon))
                    PauseAudioClip();

                Rect stopButtonRect = new Rect(pauseButtonRect.xMax + spacing, secondLine.y, buttonWidth, secondLine.height);
                if (GUI.Button(stopButtonRect, stopIcon))
                    StopAudioClip();

                Rect currentTimeRect = new Rect(stopButtonRect.xMax + spacing, secondLine.y, timeLabelWidth, secondLine.height);
                EditorGUI.LabelField(currentTimeRect, FormatTime(_editorAudioSource != null ? _editorAudioSource.time : 0f));

                Rect timeSliderRect = new Rect(currentTimeRect.xMax + spacing, secondLine.y, sliderWidth, secondLine.height);
                DrawCustomSlider(timeSliderRect, _editorAudioSource != null ? _editorAudioSource.time : 0f, 0f, audioClip.length, time =>
                {
                    if (_editorAudioSource != null)
                    {
                        _editorAudioSource.time = time;
                        RepaintInspectors();
                    }
                });

                Rect totalTimeRect = new Rect(timeSliderRect.xMax + spacing, secondLine.y, timeLabelWidth, secondLine.height);
                EditorGUI.LabelField(totalTimeRect, FormatTime(audioClip.length));

                Rect volumeSliderRect = new Rect(totalTimeRect.xMax + spacing, secondLine.y, volumeSliderWidth, secondLine.height);
                DrawCustomVolumeSlider(volumeSliderRect);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        private static void PlayAudioClip(AudioClip clip)
        {
            if (_editorAudioSource != null)
            {
                if (_isPaused)
                {
                    _editorAudioSource.Play();
                    _isPaused = false;
                }
                else
                {
                    _editorAudioSource.clip = clip;
                    _editorAudioSource.Play();
                }
                _isPlaying = true;
                EditorApplication.update += EditorUpdate;
            }
        }

        private static void PauseAudioClip()
        {
            if (_editorAudioSource != null && _editorAudioSource.isPlaying)
            {
                _editorAudioSource.Pause();
                _isPlaying = false;
                _isPaused = true;
            }
        }

        private static void StopAudioClip()
        {
            if (_editorAudioSource != null)
            {
                _editorAudioSource.Stop();
                _editorAudioSource.time = 0f;
                _isPlaying = false;
                _isPaused = false;
                EditorApplication.update -= EditorUpdate;
                RepaintInspectors();
            }
        }

        private static void DrawCustomSlider(Rect position, float value, float min, float max, System.Action<float> onValueChanged)
        {
            EditorGUI.DrawRect(position, new Color(0.2f, 0.2f, 0.2f));
            float normalizedValue = Mathf.InverseLerp(min, max, value);
            Rect fillRect = new Rect(position.x, position.y, position.width * normalizedValue, position.height);
            EditorGUI.DrawRect(fillRect, new Color(0.4f, 0.8f, 0.4f));

            Event e = Event.current;
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (position.Contains(e.mousePosition))
                {
                    float newValue = Mathf.Lerp(min, max, (e.mousePosition.x - position.x) / position.width);
                    onValueChanged(newValue);
                    e.Use();
                }
            }
        }

        private static void DrawCustomVolumeSlider(Rect position)
        {
            EditorGUI.DrawRect(position, new Color(0.2f, 0.2f, 0.2f));

            Rect fillRect = new Rect(position.x, position.y, position.width * _volume, position.height);
            EditorGUI.DrawRect(fillRect, new Color(0.8f, 0.4f, 0.4f));

            Event e = Event.current;
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (position.Contains(e.mousePosition))
                {
                    _volume = Mathf.Clamp01((e.mousePosition.x - position.x) / position.width);
                    if (_editorAudioSource != null)
                        _editorAudioSource.volume = _volume;

                    e.Use();
                }
            }
        }

        private static string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:0}:{seconds:00}";
        }

        private static void EditorUpdate()
        {
            if (_isPlaying && _editorAudioSource != null)
            {
                RepaintInspectors();
                if (!_editorAudioSource.isPlaying)
                {
                    _isPlaying = false;
                    _isPaused = false;
                    EditorApplication.update -= EditorUpdate;
                }
            }
        }

        private static void RepaintInspectors()
        {
            var inspectors = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var inspector in inspectors)
            {
                if (inspector.GetType().Name == "InspectorWindow")
                    inspector.Repaint();
            }
        }
    }
}
#endif
