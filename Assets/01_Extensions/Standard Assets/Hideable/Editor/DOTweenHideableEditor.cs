using DG.DOTweenEditor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

using TargetType = DOTweenHideable;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(TargetType), true)]
    [CanEditMultipleObjects]
    public class DOTweenHideableEditor : HideableEditor
    {
        public SerializedProperty ShowAnimations;
        public SerializedProperty HideAnimations;

        protected override void OnEnable()
        {
            base.OnEnable();
            ShowAnimations = serializedObject.FindProperty(ShowAnimations.GetName(() => ShowAnimations));
            HideAnimations = serializedObject.FindProperty(HideAnimations.GetName(() => HideAnimations));
        }

        public static bool showShowing;
        public static bool showHiding;

        private TargetType parent;

        protected override void DrawContent()
        {
            base.DrawContent();

            parent = target as TargetType;

            ArrayUpdate(ShowAnimations);
            ArrayUpdate(HideAnimations);

            EditorGUILayout.PropertyField(ShowAnimations, true);
            EditorGUILayout.PropertyField(HideAnimations, true);
        }

        private void ArrayUpdate(SerializedProperty obj)
        {
            if (obj == null)
                return;
            SerializedProperty prop;
            SerializedProperty DefaultTarget = null;
            for (int i = 0; i < obj.arraySize; i++)
            {
                prop = obj.GetArrayElementAtIndex(i);
                if (prop == null)
                    continue;
                DefaultTarget = prop.FindPropertyRelative(DefaultTarget.GetName(() => DefaultTarget));
                if (DefaultTarget == null)
                    continue;
                DefaultTarget.objectReferenceValue = parent.gameObject;
            }
        }
    }
}