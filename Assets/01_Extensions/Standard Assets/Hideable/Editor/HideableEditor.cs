using DG.DOTweenEditor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

using TargetType = Hideable;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(TargetType), false)]
    [CanEditMultipleObjects]
    public class HideableEditor : Editor
    {
        private SerializedProperty HasShowEvent;
        private SerializedProperty HasShowInstantEvent;
        private SerializedProperty HasShownEvent;
        private SerializedProperty HasHideEvent;
        private SerializedProperty HasHideInstantEvent;
        private SerializedProperty HasHiddenEvent;
        private SerializedProperty HasFinishedEvent;

        private SerializedProperty OnShow;
        private SerializedProperty OnShowInstant;
        private SerializedProperty OnShown;
        private SerializedProperty OnHide;
        private SerializedProperty OnHideInstant;
        private SerializedProperty OnHidden;
        private SerializedProperty OnFinished;

        public SerializedProperty StartState;

        protected virtual void OnEnable()
        {
            HasShowEvent = serializedObject.FindProperty(HasShowEvent.GetName(() => HasShowEvent));
            HasShowInstantEvent = serializedObject.FindProperty(HasShowInstantEvent.GetName(() => HasShowInstantEvent));
            HasShownEvent = serializedObject.FindProperty(HasShownEvent.GetName(() => HasShownEvent));
            HasHideEvent = serializedObject.FindProperty(HasHideEvent.GetName(() => HasHideEvent));
            HasHideInstantEvent = serializedObject.FindProperty(HasHideInstantEvent.GetName(() => HasHideInstantEvent));
            HasHiddenEvent = serializedObject.FindProperty(HasHiddenEvent.GetName(() => HasHiddenEvent));
            HasFinishedEvent = serializedObject.FindProperty(HasFinishedEvent.GetName(() => HasFinishedEvent));

            OnShow = serializedObject.FindProperty(OnShow.GetName(() => OnShow));
            OnShowInstant = serializedObject.FindProperty(OnShowInstant.GetName(() => OnShowInstant));
            OnShown = serializedObject.FindProperty(OnShown.GetName(() => OnShown));
            OnHide = serializedObject.FindProperty(OnHide.GetName(() => OnHide));
            OnHideInstant = serializedObject.FindProperty(OnHideInstant.GetName(() => OnHideInstant));
            OnHidden = serializedObject.FindProperty(OnHidden.GetName(() => OnHidden));
            OnFinished = serializedObject.FindProperty(OnFinished.GetName(() => OnFinished));

            StartState = serializedObject.FindProperty(StartState.GetName(() => StartState));
        }

        private TargetType typedTarget;

        private enum SLIDERTYPE
        {
            SHOW,
            HIDE
        }

        private float LastSliderValue;
        private static SLIDERTYPE sliderType;
        private static SLIDERTYPE lastSliderType;

        private static bool EventsShown;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            typedTarget = target as TargetType;

            DrawDebugMenu();

            DrawContent();

            EventsShown = EditorGUILayout.Foldout(EventsShown, new GUIContent("Events"));
            if (EventsShown)
            {
                DrawEventButtons();
                DrawEvents();
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawContent()
        {
            EditorGUILayout.LabelField(new GUIContent("Basic Setup"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(StartState);
        }

        protected virtual void DrawDebugMenu()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(new GUIContent("Debugging"), EditorStyles.boldLabel);
                EditorGUILayout.LabelField(new GUIContent(string.Format("Current State:{0}", typedTarget.State.ToString())));
                if (GUILayout.Button(new GUIContent("Trigger Show Animation", "")))
                {
                    typedTarget.Show();
                }
                if (GUILayout.Button(new GUIContent("Trigger Hide Animation", "")))
                {
                    typedTarget.Hide();
                }

                sliderType = (SLIDERTYPE)EditorGUILayout.EnumPopup(new GUIContent("Choose SliderType"), sliderType);

                float slider = EditorGUILayout.Slider(new GUIContent("Slider"), LastSliderValue, 0f, 1f);

                if (lastSliderType != sliderType)
                {
                    slider = 1f - slider;
                }

                if (LastSliderValue != slider)
                {
                    typedTarget.State = Hideable.STATES.NONE;
                    typedTarget.SetPosition(this, slider, sliderType == SLIDERTYPE.SHOW);
                }
                LastSliderValue = slider;
                lastSliderType = sliderType;
                EditorGUILayout.LabelField(new GUIContent("Current Time:" + typedTarget.CurrentTime));
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("Debug Options in PlayMode"), EditorStyles.boldLabel);
            }
        }

        protected virtual void DrawEventButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Show:" + HasShowEvent.boolValue)))
            {
                HasShowEvent.boolValue = !HasShowEvent.boolValue;
            }
            if (GUILayout.Button(new GUIContent("ShowInstant:" + HasShowInstantEvent.boolValue)))
            {
                HasShowInstantEvent.boolValue = !HasShowInstantEvent.boolValue;
            }
            if (GUILayout.Button(new GUIContent("Shown:" + HasShownEvent.boolValue)))
            {
                HasShownEvent.boolValue = !HasShownEvent.boolValue;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Hide:" + HasHideEvent.boolValue)))
            {
                HasHideEvent.boolValue = !HasHideEvent.boolValue;
            }
            if (GUILayout.Button(new GUIContent("HideInstant:" + HasHideInstantEvent.boolValue)))
            {
                HasHideInstantEvent.boolValue = !HasHideInstantEvent.boolValue;
            }
            if (GUILayout.Button(new GUIContent("Hidden:" + HasHiddenEvent.boolValue)))
            {
                HasHiddenEvent.boolValue = !HasHiddenEvent.boolValue;
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(new GUIContent("Finished:" + HasFinishedEvent.boolValue)))
            {
                HasFinishedEvent.boolValue = !HasFinishedEvent.boolValue;
            }
        }

        protected virtual void DrawEvents()
        {
            if (HasShowEvent.boolValue)
            {
                EditorGUILayout.PropertyField(OnShown);
            }
            if (HasHideEvent.boolValue)
            {
                EditorGUILayout.PropertyField(OnHide);
            }
            if (HasFinishedEvent.boolValue)
            {
                EditorGUILayout.PropertyField(OnFinished);
            }
        }
    }
}