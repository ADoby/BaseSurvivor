using DG.DOTweenEditor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(HideableList), false)]
    [CanEditMultipleObjects]
    public class HideableListEditor : Editor
    {
        private SerializedProperty HasShowingEvent;
        private SerializedProperty HasHidingEvent;
        private SerializedProperty HasEvent;
        public SerializedProperty OnFinishedShowing;
        public SerializedProperty OnFinishedHiding;
        public SerializedProperty OnFinished;
        public SerializedProperty StartState;
        public SerializedProperty List;
        public SerializedProperty DelayBetween;
        public SerializedProperty ReverseDelayWhenHiding;

        public SerializedProperty OffsetInfos;

        protected virtual void OnEnable()
        {
            HasShowingEvent = serializedObject.FindProperty("HasShowingEvent");
            HasHidingEvent = serializedObject.FindProperty("HasHidingEvent");
            HasEvent = serializedObject.FindProperty("HasEvent");
            OnFinishedShowing = serializedObject.FindProperty("OnFinishedShowing");
            OnFinishedHiding = serializedObject.FindProperty("OnFinishedHiding");
            OnFinished = serializedObject.FindProperty("OnFinished");

            StartState = serializedObject.FindProperty("StartState");
            List = serializedObject.FindProperty("List");
            DelayBetween = serializedObject.FindProperty("DelayBetween");
            ReverseDelayWhenHiding = serializedObject.FindProperty("ReverseDelayWhenHiding");

            OffsetInfos = serializedObject.FindProperty("OffsetInfos");
        }

        private static float LastSlider;
        private HideableList parent;

        private static SLIDERTYPE type;
        private static SLIDERTYPE lasttype;

        private enum SLIDERTYPE
        {
            SHOW,
            HIDE
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            parent = target as HideableList;

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(new GUIContent(string.Format("State:{0}", parent.State.ToString())));
                if (GUILayout.Button(new GUIContent("Trigger Show Animation", "")))
                {
                    parent.Show();
                }
                if (GUILayout.Button(new GUIContent("Trigger Hide Animation", "")))
                {
                    parent.Hide();
                }

                type = (SLIDERTYPE)EditorGUILayout.EnumPopup(new GUIContent("Choose SliderType"), type);

                float slider = EditorGUILayout.Slider(new GUIContent("Slide"), LastSlider, 0f, 1f);

                if (lasttype != type)
                {
                    slider = 1f - slider;
                }

                if (LastSlider != slider)
                {
                    parent.State = HideableList.STATES.NONE;
                    parent.SetPosition(this, slider, type == SLIDERTYPE.SHOW);
                }
                LastSlider = slider;
                lasttype = type;
                EditorGUILayout.LabelField(new GUIContent("Current Time:" + parent.CurrentTime));
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("Debug Options in PlayMode"));
            }

            EditorGUILayout.PropertyField(StartState);
            EditorGUILayout.PropertyField(DelayBetween);
            EditorGUILayout.PropertyField(ReverseDelayWhenHiding);

            EditorGUILayout.PropertyField(OffsetInfos, true);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(List, true);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("OnShown:" + HasShowingEvent.boolValue)))
            {
                HasShowingEvent.boolValue = !HasShowingEvent.boolValue;
            }
            if (GUILayout.Button(new GUIContent("OnHidden:" + HasHidingEvent.boolValue)))
            {
                HasHidingEvent.boolValue = !HasHidingEvent.boolValue;
            }
            if (GUILayout.Button(new GUIContent("OnFinished:" + HasEvent.boolValue)))
            {
                HasEvent.boolValue = !HasEvent.boolValue;
            }

            EditorGUILayout.EndHorizontal();

            //ArrayGUI(serializedObject, "List");

            if (HasShowingEvent.boolValue)
            {
                EditorGUILayout.PropertyField(OnFinishedShowing);
            }
            if (HasHidingEvent.boolValue)
            {
                EditorGUILayout.PropertyField(OnFinishedHiding);
            }
            if (HasEvent.boolValue)
            {
                EditorGUILayout.PropertyField(OnFinished);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ArrayGUI(SerializedObject obj, string name)
        {
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel += 2;
            obj.FindProperty(name).arraySize = EditorGUILayout.IntField(name + " Size", obj.FindProperty(name).arraySize);

            SerializedProperty prop;
            for (int i = 0; i < obj.FindProperty(name).arraySize; i++)
            {
                prop = obj.FindProperty(name).GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(prop);
            }
            EditorGUI.indentLevel = indent;
        }
    }
}