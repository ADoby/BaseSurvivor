// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/12 16:03

using System.Collections.Generic;
using System.IO;

using DG.DOTweenEditor.Core;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

#if DOTWEEN_TMP

using TMPro;

#endif

namespace DG.DOTweenEditor
{
    [CustomPropertyDrawer(typeof(DOTweenAnimationEx))]
    public class DOTweenAnimationExInspector : PropertyDrawer
    {
        private enum FadeTargetType
        {
            CanvasGroup,
            Image
        }

        private enum ChooseTargetMode
        {
            None,
            BetweenCanvasGroupAndImage
        }

        private readonly Dictionary<DOTweenAnimationTypeEx, System.Type[]> _AnimationTypeToComponent = new Dictionary<DOTweenAnimationTypeEx, System.Type[]>() {
            { DOTweenAnimationTypeEx.Move, new[] { typeof(Rigidbody), typeof(Rigidbody2D), typeof(RectTransform), typeof(Transform) } },
            { DOTweenAnimationTypeEx.LocalMove, new[] { typeof(Transform) } },
            { DOTweenAnimationTypeEx.Rotate, new[] { typeof(Rigidbody), typeof(Rigidbody2D), typeof(Transform) } },
            { DOTweenAnimationTypeEx.LocalRotate, new[] { typeof(Transform) } },
            { DOTweenAnimationTypeEx.Scale, new[] { typeof(Transform) } },
            { DOTweenAnimationTypeEx.Color, new[] { typeof(SpriteRenderer), typeof(Renderer), typeof(Image), typeof(Text), typeof(Light) } },
            { DOTweenAnimationTypeEx.Fade, new[] { typeof(SpriteRenderer), typeof(Renderer), typeof(Image), typeof(Text), typeof(CanvasGroup), typeof(Light) } },
            { DOTweenAnimationTypeEx.Text, new[] { typeof(Text) } },
            { DOTweenAnimationTypeEx.PunchPosition, new[] { typeof(RectTransform), typeof(Transform) } },
            { DOTweenAnimationTypeEx.PunchRotation, new[] { typeof(Transform) } },
            { DOTweenAnimationTypeEx.PunchScale, new[] { typeof(Transform) } },
            { DOTweenAnimationTypeEx.ShakePosition, new[] { typeof(RectTransform), typeof(Transform) } },
            { DOTweenAnimationTypeEx.ShakeRotation, new[] { typeof(Transform) } },
            { DOTweenAnimationTypeEx.ShakeScale, new[] { typeof(Transform) } },
            { DOTweenAnimationTypeEx.CameraAspect, new[] { typeof(Camera) } },
            { DOTweenAnimationTypeEx.CameraBackgroundColor, new[] { typeof(Camera) } },
            { DOTweenAnimationTypeEx.CameraFieldOfView, new[] { typeof(Camera) } },
            { DOTweenAnimationTypeEx.CameraOrthoSize, new[] { typeof(Camera) } },
            { DOTweenAnimationTypeEx.CameraPixelRect, new[] { typeof(Camera) } },
            { DOTweenAnimationTypeEx.CameraRect, new[] { typeof(Camera) } },
            { DOTweenAnimationTypeEx.UIWidthHeight, new[] { typeof(RectTransform) } },
            { DOTweenAnimationTypeEx.UIPivot, new[] { typeof(RectTransform) } },
            { DOTweenAnimationTypeEx.UIAnchor, new[] { typeof(RectTransform) } },
        };

#if DOTWEEN_TK2D
        static readonly Dictionary<DOTweenAnimationTypeEx, Type[]> _Tk2dAnimationTypeToComponent = new Dictionary<DOTweenAnimationTypeEx, Type[]>() {
            { DOTweenAnimationTypeEx.Scale, new[] { typeof(tk2dBaseSprite), typeof(tk2dTextMesh) } },
            { DOTweenAnimationTypeEx.Color, new[] { typeof(tk2dBaseSprite), typeof(tk2dTextMesh) } },
            { DOTweenAnimationTypeEx.Fade, new[] { typeof(tk2dBaseSprite), typeof(tk2dTextMesh) } },
            { DOTweenAnimationTypeEx.Text, new[] { typeof(tk2dTextMesh) } }
        };
#endif
#if DOTWEEN_TMP

        private  readonly Dictionary<DOTweenAnimationTypeEx, Type[]> _TMPAnimationTypeToComponent = new Dictionary<DOTweenAnimationTypeEx, Type[]>() {
            { DOTweenAnimationTypeEx.Color, new[] { typeof(TextMeshPro), typeof(TextMeshProUGUI) } },
            { DOTweenAnimationTypeEx.Fade, new[] { typeof(TextMeshPro), typeof(TextMeshProUGUI) } },
            { DOTweenAnimationTypeEx.Text, new[] { typeof(TextMeshPro), typeof(TextMeshProUGUI) } }
        };

#endif

        private readonly string[] _AnimationType = new[] {
            "None",
            "Move", "LocalMove",
            "Rotate", "LocalRotate",
            "Scale",
            "Color", "Fade",
            "Text",
            "UIWidthHeight", "UIPivot", "UIAnchor",
            "Punch/Position", "Punch/Rotation", "Punch/Scale",
            "Shake/Position", "Shake/Rotation", "Shake/Scale",
            "Camera/Aspect", "Camera/BackgroundColor", "Camera/FieldOfView", "Camera/OrthoSize", "Camera/PixelRect", "Camera/Rect"
        };

        private string[] _animationTypeNoSlashes; // _AnimationType list without slashes in values
        private string[] _datString; // String representation of DOTweenAnimation enum (here for caching reasons)

        #region MonoBehaviour Methods

        private bool _runtimeEditMode = false;

        private SerializedProperty isActive;
        private SerializedProperty id;
        private SerializedProperty duration;
        private SerializedProperty delay;
        private SerializedProperty isRelative;
        private SerializedProperty hasFrom;
        private SerializedProperty loops;
        private SerializedProperty useTargetAsV3;
        private SerializedProperty isValid;
        private SerializedProperty isSpeedBased;
        private SerializedProperty isIndependentUpdate;
        private SerializedProperty targetComponent;
        private SerializedProperty targetType;
        private SerializedProperty DefaultTarget;
        private SerializedProperty Target;
        private SerializedProperty easeCurve;
        private SerializedProperty loopType;
        private SerializedProperty optionalRotationMode;
        private SerializedProperty optionalScrambleMode;
        private SerializedProperty optionalString;

        private SerializedProperty optionalBool0;
        private SerializedProperty useStartAsEndValue;
        private SerializedProperty useStartAsStartValue;
        private SerializedProperty optionalFloat0;
        private SerializedProperty optionalInt0;

        private SerializedProperty endValueFloat;
        private SerializedProperty endValueV2;
        private SerializedProperty endValueV3;
        private SerializedProperty endValueRect;
        private SerializedProperty endValueString;
        private SerializedProperty endValueColor;
        private SerializedProperty endValueTransform;

        private SerializedProperty startValueFloat;
        private SerializedProperty startValueV2;
        private SerializedProperty startValueV3;
        private SerializedProperty startValueRect;
        private SerializedProperty startValueString;
        private SerializedProperty startValueColor;
        private SerializedProperty startValueTransform;

        private SerializedProperty animationType;
        private SerializedProperty easeType;
        private SerializedProperty forcedTargetType;

        private GameObject parent;

        private void OnEnable()
        {
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0f;
            //return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            OnInspectorGUI(property);
        }

        public void OnInspectorGUI(SerializedProperty serializedObject)
        {
            int len = _AnimationType.Length;
            _animationTypeNoSlashes = new string[len];
            for (int i = 0; i < len; ++i)
            {
                string a = _AnimationType[i];
                a = a.Replace("/", "");
                _animationTypeNoSlashes[i] = a;
            }

            int _totComponentsOnSrc = 0; // Used to determine if a Component is added or removed from the source
            bool _isLightSrc = false; // Used to determine if we're tweening a Light, to set the max Fade value to more than 1
            ChooseTargetMode _chooseTargetMode = ChooseTargetMode.None;

            Target = serializedObject.FindPropertyRelative(Target.GetName(() => Target));
            DefaultTarget = serializedObject.FindPropertyRelative(DefaultTarget.GetName(() => DefaultTarget));
            easeCurve = serializedObject.FindPropertyRelative(easeCurve.GetName(() => easeCurve));
            loopType = serializedObject.FindPropertyRelative(loopType.GetName(() => loopType));
            optionalRotationMode = serializedObject.FindPropertyRelative(optionalRotationMode.GetName(() => optionalRotationMode));
            optionalScrambleMode = serializedObject.FindPropertyRelative(optionalScrambleMode.GetName(() => optionalScrambleMode));

            optionalString = serializedObject.FindPropertyRelative(optionalString.GetName(() => optionalString));

            isActive = serializedObject.FindPropertyRelative(isActive.GetName(() => isActive));
            id = serializedObject.FindPropertyRelative(id.GetName(() => id));
            duration = serializedObject.FindPropertyRelative(duration.GetName(() => duration));
            delay = serializedObject.FindPropertyRelative(delay.GetName(() => delay));
            isRelative = serializedObject.FindPropertyRelative(isRelative.GetName(() => isRelative));
            hasFrom = serializedObject.FindPropertyRelative(hasFrom.GetName(() => hasFrom));
            loops = serializedObject.FindPropertyRelative(loops.GetName(() => loops));
            useTargetAsV3 = serializedObject.FindPropertyRelative(useTargetAsV3.GetName(() => useTargetAsV3));
            isValid = serializedObject.FindPropertyRelative(isValid.GetName(() => isValid));
            isSpeedBased = serializedObject.FindPropertyRelative(isSpeedBased.GetName(() => isSpeedBased));
            isIndependentUpdate = serializedObject.FindPropertyRelative(isIndependentUpdate.GetName(() => isIndependentUpdate));

            optionalBool0 = serializedObject.FindPropertyRelative(optionalBool0.GetName(() => optionalBool0));
            useStartAsEndValue = serializedObject.FindPropertyRelative(useStartAsEndValue.GetName(() => useStartAsEndValue));
            useStartAsStartValue = serializedObject.FindPropertyRelative(useStartAsStartValue.GetName(() => useStartAsStartValue));
            optionalFloat0 = serializedObject.FindPropertyRelative(optionalFloat0.GetName(() => optionalFloat0));
            optionalInt0 = serializedObject.FindPropertyRelative(optionalInt0.GetName(() => optionalInt0));

            endValueColor = serializedObject.FindPropertyRelative(endValueColor.GetName(() => endValueColor));
            endValueFloat = serializedObject.FindPropertyRelative(endValueFloat.GetName(() => endValueFloat));
            endValueRect = serializedObject.FindPropertyRelative(endValueRect.GetName(() => endValueRect));
            endValueString = serializedObject.FindPropertyRelative(endValueString.GetName(() => endValueString));
            endValueTransform = serializedObject.FindPropertyRelative(endValueTransform.GetName(() => endValueTransform));
            endValueV2 = serializedObject.FindPropertyRelative(endValueV2.GetName(() => endValueV2));
            endValueV3 = serializedObject.FindPropertyRelative(endValueV3.GetName(() => endValueV3));

            startValueColor = serializedObject.FindPropertyRelative(startValueColor.GetName(() => startValueColor));
            startValueFloat = serializedObject.FindPropertyRelative(startValueFloat.GetName(() => startValueFloat));
            startValueRect = serializedObject.FindPropertyRelative(startValueRect.GetName(() => startValueRect));
            startValueString = serializedObject.FindPropertyRelative(startValueString.GetName(() => startValueString));
            startValueTransform = serializedObject.FindPropertyRelative(startValueTransform.GetName(() => startValueTransform));
            startValueV2 = serializedObject.FindPropertyRelative(startValueV2.GetName(() => startValueV2));
            startValueV3 = serializedObject.FindPropertyRelative(startValueV3.GetName(() => startValueV3));

            animationType = serializedObject.FindPropertyRelative(animationType.GetName(() => animationType));
            easeType = serializedObject.FindPropertyRelative(easeType.GetName(() => easeType));
            targetComponent = serializedObject.FindPropertyRelative(targetComponent.GetName(() => targetComponent));
            targetType = serializedObject.FindPropertyRelative(targetType.GetName(() => targetType));
            forcedTargetType = serializedObject.FindPropertyRelative(forcedTargetType.GetName(() => forcedTargetType));

            bool playMode = Application.isPlaying;
            _runtimeEditMode = _runtimeEditMode && playMode;
            EditorGUIUtils.SetGUIStyles();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("Tween:{0}", id.stringValue));
            EditorGUILayout.PropertyField(isActive);
            EditorGUILayout.EndHorizontal();

            if (playMode)
            {
                if (_runtimeEditMode)
                {
                }
                else
                {
                    GUILayout.Space(8);
                    GUILayout.Label("Animation Editor disabled while in play mode", EditorGUIUtils.wordWrapLabelStyle);
                    if (!isActive.boolValue)
                    {
                        GUILayout.Label("This animation has been toggled as inactive and won't be generated", EditorGUIUtils.wordWrapLabelStyle);
                        //GUI.enabled = false;
                    }
                    if (GUILayout.Button(new GUIContent("Activate Edit Mode", "Switches to Runtime Edit Mode, where you can change animations values and restart them")))
                    {
                        _runtimeEditMode = true;
                    }
                    GUILayout.Label("NOTE: when using DOPlayNext, the sequence is determined by the DOTweenAnimation Components order in the target GameObject's Inspector", EditorGUIUtils.wordWrapLabelStyle);
                    GUILayout.Space(10);
                    if (!_runtimeEditMode) return;
                }
            }

            GUILayout.BeginHorizontal();
            int prevAnimType = animationType.enumValueIndex;
            animationType.enumValueIndex = (int)AnimationToDOTweenAnimationTypeEx(_AnimationType[EditorGUILayout.Popup(DOTweenAnimationTypeExToPopupId((DOTweenAnimationTypeEx)(animationType.enumValueIndex)), _AnimationType)]);
            GUILayout.EndHorizontal();
            GUI.enabled = isActive.boolValue;

            if (prevAnimType != (int)DOTweenAnimationTypeEx.None)
            {
                EditorGUILayout.PropertyField(Target);
                if (Target.objectReferenceValue != null)
                    parent = (GameObject)Target.objectReferenceValue;
                else
                    parent = (GameObject)DefaultTarget.objectReferenceValue;
            }

            if (prevAnimType != animationType.enumValueIndex)
            {
                if (prevAnimType == (int)DOTweenAnimationTypeEx.None)
                {
                    duration.floatValue = 0.25f;
                    easeType.enumValueIndex = (int)Ease.InOutQuad;
                    loops.intValue = 1;
                    isActive.boolValue = true;
                }

                // Set default optional values based on animation type
                endValueTransform.SetObjectValue(null);
                useTargetAsV3.boolValue = false;
                useStartAsEndValue.boolValue = false;
                useStartAsStartValue.boolValue = false;

                switch ((DOTweenAnimationTypeEx)animationType.enumValueIndex)
                {
                    case DOTweenAnimationTypeEx.Move:
                    case DOTweenAnimationTypeEx.LocalMove:
                    case DOTweenAnimationTypeEx.Rotate:
                    case DOTweenAnimationTypeEx.LocalRotate:
                    case DOTweenAnimationTypeEx.Scale:
                        endValueV3.vector3Value = Vector3.zero;
                        startValueV3.vector3Value = Vector3.zero;
                        endValueFloat.floatValue = 0;
                        startValueFloat.floatValue = 0;
                        optionalBool0.boolValue = animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.Scale;
                        break;

                    case DOTweenAnimationTypeEx.UIWidthHeight:
                        endValueV3.vector3Value = Vector3.zero;
                        endValueFloat.floatValue = 0;
                        optionalBool0.boolValue = animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.UIWidthHeight;
                        break;

                    case DOTweenAnimationTypeEx.UIPivot:
                        endValueV2.vector2Value = new Vector2(0.5f, 0.5f);
                        break;

                    case DOTweenAnimationTypeEx.Color:
                    case DOTweenAnimationTypeEx.Fade:
                        if (parent != null)
                            _isLightSrc = parent.GetComponent<Light>() != null;
                        endValueFloat.floatValue = 0;
                        break;

                    case DOTweenAnimationTypeEx.Text:
                        optionalBool0.boolValue = true;
                        break;

                    case DOTweenAnimationTypeEx.PunchPosition:
                    case DOTweenAnimationTypeEx.PunchRotation:
                    case DOTweenAnimationTypeEx.PunchScale:
                        endValueV3.vector3Value = animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.PunchRotation ? new Vector3(0, 180, 0) : Vector3.one;
                        optionalFloat0.floatValue = 1;
                        optionalInt0.intValue = 10;
                        optionalBool0.boolValue = false;
                        break;

                    case DOTweenAnimationTypeEx.ShakePosition:
                    case DOTweenAnimationTypeEx.ShakeRotation:
                    case DOTweenAnimationTypeEx.ShakeScale:
                        endValueV3.vector3Value = animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.ShakeRotation ? new Vector3(90, 90, 90) : Vector3.one;
                        optionalInt0.intValue = 10;
                        optionalFloat0.floatValue = 90;
                        optionalBool0.boolValue = false;
                        break;

                    case DOTweenAnimationTypeEx.CameraAspect:
                    case DOTweenAnimationTypeEx.CameraFieldOfView:
                    case DOTweenAnimationTypeEx.CameraOrthoSize:
                        endValueFloat.floatValue = 0;
                        break;

                    case DOTweenAnimationTypeEx.UIAnchor:
                    case DOTweenAnimationTypeEx.CameraPixelRect:
                    case DOTweenAnimationTypeEx.CameraRect:
                        endValueRect.rectValue = new Rect(0, 0, 0, 0);
                        break;
                }
            }
            if (animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.None)
            {
                isValid.boolValue = false;
                isActive.boolValue = false;
                GUI.enabled = true;
                return;
            }

            if (parent == null)
            {
                isValid.boolValue = false;
                GUI.enabled = true;
                return;
            }

            if (prevAnimType != animationType.enumValueIndex || ComponentsChanged(_totComponentsOnSrc))
            {
                isValid.boolValue = Validate();
                // See if we need to choose between multiple targets

                if (animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.Fade && parent.GetComponent<CanvasGroup>() != null && parent.GetComponent<Image>() != null)
                {
                    _chooseTargetMode = ChooseTargetMode.BetweenCanvasGroupAndImage;
                    // Reassign target and forcedTargetType.enumValueIndex if lost
                    if (forcedTargetType.enumValueIndex == (int)TargetType.Unset) forcedTargetType.enumValueIndex = targetType.enumValueIndex;
                    switch ((TargetType)forcedTargetType.enumValueIndex)
                    {
                        case TargetType.CanvasGroup:
                            targetComponent.objectReferenceValue = parent.GetComponent<CanvasGroup>();
                            break;

                        case TargetType.Image:
                            targetComponent.objectReferenceValue = parent.GetComponent<Image>();
                            break;
                    }
                }
                else
                {
                    _chooseTargetMode = ChooseTargetMode.None;
                    forcedTargetType.enumValueIndex = (int)TargetType.Unset;
                }
            }

            if (!isValid.boolValue)
            {
                GUI.color = Color.red;
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("No valid Component was found for the selected animation", EditorGUIUtils.wordWrapLabelStyle);
                GUILayout.EndVertical();
                GUI.color = Color.white;

                GUI.enabled = true;
                return;
            }

            // Special cases in which multiple target types could be used (set after validation)
            if (_chooseTargetMode == ChooseTargetMode.BetweenCanvasGroupAndImage && forcedTargetType.enumValueIndex != (int)TargetType.Unset)
            {
                FadeTargetType fadeTargetType = (FadeTargetType)forcedTargetType.enumValueIndex;
                int prevTargetType = forcedTargetType.enumValueIndex;
                forcedTargetType.enumValueIndex = (int)(TargetType)EditorGUILayout.EnumPopup((TargetType)animationType.enumValueIndex + " Target", fadeTargetType);
                if (forcedTargetType.enumValueIndex != prevTargetType)
                {
                    // Target type change > assign correct target
                    switch ((TargetType)forcedTargetType.enumValueIndex)
                    {
                        case TargetType.CanvasGroup:
                            targetComponent.objectReferenceValue = parent.GetComponent<CanvasGroup>();
                            break;

                        case TargetType.Image:
                            targetComponent.objectReferenceValue = parent.GetComponent<Image>();
                            break;
                    }
                }
            }

            GUILayout.BeginHorizontal();
            duration.floatValue = EditorGUILayout.FloatField("Duration", duration.floatValue);
            if (duration.floatValue < 0) duration.floatValue = 0;
            isSpeedBased.boolValue = EditorGUILayout.Toggle(new GUIContent("Speed Based", "If selected, the duration will count as units / degree x second"), isSpeedBased.boolValue);
            GUILayout.EndHorizontal();
            delay.floatValue = EditorGUILayout.FloatField("Delay", delay.floatValue);
            if (delay.floatValue < 0) delay.floatValue = 0;
            isIndependentUpdate.boolValue = EditorGUILayout.Toggle("Ignore TimeScale", isIndependentUpdate.boolValue);
            easeType.enumValueIndex = (int)EditorGUIUtils.FilteredEasePopup((Ease)easeType.enumValueIndex);
            if (easeType.enumValueIndex == (int)Ease.INTERNAL_Custom)
            {
                EditorGUILayout.PropertyField(easeCurve);
            }
            loops.intValue = EditorGUILayout.IntField(new GUIContent("Loops", "Set to -1 for infinite loops"), loops.intValue);
            if (loops.intValue < -1) loops.intValue = -1;
            if (loops.intValue > 1 || loops.intValue == -1)
                EditorGUILayout.PropertyField(loopType);
            id.stringValue = EditorGUILayout.TextField("ID", id.stringValue);

            bool canBeRelative = true;
            // End value and eventual specific options
            switch ((DOTweenAnimationTypeEx)animationType.enumValueIndex)
            {
                case DOTweenAnimationTypeEx.Move:
                case DOTweenAnimationTypeEx.LocalMove:

                    //GUIEndValueV3(false, animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.Move);
                    GUIEndValueV3(true, animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.Move);

                    optionalBool0.boolValue = EditorGUILayout.Toggle("    Snapping", optionalBool0.boolValue);
                    canBeRelative = !useTargetAsV3.boolValue;
                    break;

                case DOTweenAnimationTypeEx.Rotate:
                case DOTweenAnimationTypeEx.LocalRotate:
                    if (parent.GetComponent<Rigidbody2D>()) GUIEndValueFloat();
                    else
                    {
                        GUIEndValueV3(true);
                        EditorGUILayout.PropertyField(optionalRotationMode);
                    }
                    break;

                case DOTweenAnimationTypeEx.Scale:
                    useStartAsEndValue.boolValue = EditorGUILayout.Toggle("Use Starting Value", useStartAsEndValue.boolValue);

                    if (useStartAsEndValue.boolValue)
                        LoadCurrentValues();
                    GUI.enabled = !useStartAsEndValue.boolValue;
                    if (optionalBool0.boolValue) GUIEndValueFloat();
                    else GUIEndValueV3(true);
                    optionalBool0.boolValue = EditorGUILayout.Toggle("Uniform Scale", optionalBool0.boolValue);
                    GUI.enabled = true;
                    break;

                case DOTweenAnimationTypeEx.UIWidthHeight:
                    useStartAsEndValue.boolValue = EditorGUILayout.Toggle("Use Starting Value", useStartAsEndValue.boolValue);
                    if (useStartAsEndValue.boolValue)
                        LoadCurrentValues();
                    GUI.enabled = !useStartAsEndValue.boolValue;
                    if (optionalBool0.boolValue) GUIEndValueFloat();
                    else GUIEndValueV2();
                    GUI.enabled = true;
                    optionalBool0.boolValue = EditorGUILayout.Toggle("Uniform Scale", optionalBool0.boolValue);
                    break;

                case DOTweenAnimationTypeEx.UIPivot:
                    useStartAsEndValue.boolValue = EditorGUILayout.Toggle("Use Starting Value", useStartAsEndValue.boolValue);
                    if (useStartAsEndValue.boolValue)
                        LoadCurrentValues();
                    GUI.enabled = !useStartAsEndValue.boolValue;
                    GUIEndValueV2();
                    GUI.enabled = true;
                    break;

                case DOTweenAnimationTypeEx.UIAnchor:
                    useStartAsEndValue.boolValue = EditorGUILayout.Toggle("Use Starting Value", useStartAsEndValue.boolValue);
                    if (useStartAsEndValue.boolValue)
                        LoadCurrentValues();
                    GUI.enabled = !useStartAsEndValue.boolValue;
                    GUIEndValueRect();
                    GUI.enabled = true;
                    break;

                case DOTweenAnimationTypeEx.Color:
                    useStartAsEndValue.boolValue = EditorGUILayout.Toggle("Use Starting Value", useStartAsEndValue.boolValue);

                    if (useStartAsEndValue.boolValue)
                        LoadCurrentValues();

                    GUI.enabled = !useStartAsEndValue.boolValue;
                    GUIEndValueColor();
                    GUI.enabled = true;
                    canBeRelative = false;
                    break;

                case DOTweenAnimationTypeEx.Fade:
                    useStartAsEndValue.boolValue = EditorGUILayout.Toggle("Use Starting Value", useStartAsEndValue.boolValue);

                    if (useStartAsEndValue.boolValue)
                        LoadCurrentValues();

                    GUI.enabled = !useStartAsEndValue.boolValue;
                    GUIEndValueFloat();
                    GUI.enabled = true;
                    if (endValueFloat.floatValue < 0) endValueFloat.floatValue = 0;
                    if (!_isLightSrc && endValueFloat.floatValue > 1) endValueFloat.floatValue = 1;
                    canBeRelative = false;
                    break;

                case DOTweenAnimationTypeEx.Text:
                    GUIEndValueString();
                    optionalBool0.boolValue = EditorGUILayout.Toggle("Rich Text Enabled", optionalBool0.boolValue);
                    EditorGUILayout.PropertyField(optionalScrambleMode);
                    optionalString.stringValue = EditorGUILayout.TextField(new GUIContent("Custom Scramble", "Custom characters to use in case of ScrambleMode.Custom"), optionalString.stringValue);
                    break;

                case DOTweenAnimationTypeEx.PunchPosition:
                case DOTweenAnimationTypeEx.PunchRotation:
                case DOTweenAnimationTypeEx.PunchScale:
                    GUIEndValueV3(true);
                    canBeRelative = false;
                    optionalInt0.intValue = EditorGUILayout.IntSlider(new GUIContent("    Vibrato", "How much will the punch vibrate"), optionalInt0.intValue, 1, 50);
                    optionalFloat0.floatValue = EditorGUILayout.Slider(new GUIContent("    Elasticity", "How much the vector will go beyond the starting position when bouncing backwards"), optionalFloat0.floatValue, 0, 1);
                    if (animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.PunchPosition) optionalBool0.boolValue = EditorGUILayout.Toggle("    Snapping", optionalBool0.boolValue);
                    break;

                case DOTweenAnimationTypeEx.ShakePosition:
                case DOTweenAnimationTypeEx.ShakeRotation:
                case DOTweenAnimationTypeEx.ShakeScale:
                    GUIEndValueV3(true);
                    canBeRelative = false;
                    optionalInt0.intValue = EditorGUILayout.IntSlider(new GUIContent("    Vibrato", "How much will the shake vibrate"), optionalInt0.intValue, 1, 50);
                    optionalFloat0.floatValue = EditorGUILayout.Slider(new GUIContent("    Randomness", "The shake randomness"), optionalFloat0.floatValue, 0, 90);
                    if (animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.ShakePosition) optionalBool0.boolValue = EditorGUILayout.Toggle("    Snapping", optionalBool0.boolValue);
                    break;

                case DOTweenAnimationTypeEx.CameraAspect:
                case DOTweenAnimationTypeEx.CameraFieldOfView:
                case DOTweenAnimationTypeEx.CameraOrthoSize:
                    GUIEndValueFloat();
                    canBeRelative = false;
                    break;

                case DOTweenAnimationTypeEx.CameraBackgroundColor:
                    GUIEndValueColor();
                    canBeRelative = false;
                    break;

                case DOTweenAnimationTypeEx.CameraPixelRect:
                case DOTweenAnimationTypeEx.CameraRect:
                    GUIEndValueRect();
                    canBeRelative = false;
                    break;
            }

            // Final settings
            if (canBeRelative) isRelative.boolValue = EditorGUILayout.Toggle("    Relative", isRelative.boolValue);

            GUI.enabled = true;

            // Events
            //AnimationInspectorGUI.AnimationEvents(this, _src);
        }

        #endregion MonoBehaviour Methods

        #region Methods

        private void LoadCurrentValues()
        {
        }

        // Returns TRUE if the Component layout on the src gameObject changed (a Component was added or removed)
        private bool ComponentsChanged(int _totComponentsOnSrc)
        {
            int prevTotComponentsOnSrc = _totComponentsOnSrc;
            _totComponentsOnSrc = parent.GetComponents<Component>().Length;
            return prevTotComponentsOnSrc != _totComponentsOnSrc;
        }

        // Checks if a Component that can be animated with the given animationType is attached to the src
        private bool Validate()
        {
            if (animationType.enumValueIndex == (int)DOTweenAnimationTypeEx.None)
                return false;

            // First check for external plugins
#if DOTWEEN_TMP
            if (_TMPAnimationTypeToComponent.ContainsKey((DOTweenAnimationTypeEx)animationType.enumValueIndex))
            {
                foreach (Type t in _TMPAnimationTypeToComponent[(DOTweenAnimationTypeEx)animationType.enumValueIndex])
                {
                    if (TryGettingTarget(t))
                        return true;
                }
            }
#endif
            // Then check for regular stuff
            if (_AnimationTypeToComponent.ContainsKey((DOTweenAnimationTypeEx)animationType.enumValueIndex))
            {
                foreach (System.Type t in _AnimationTypeToComponent[(DOTweenAnimationTypeEx)animationType.enumValueIndex])
                {
                    if (TryGettingTarget(t))
                        return true;
                }
            }
            return false;
        }

        private bool TryGettingTarget(System.Type t)
        {
            Component srcTarget = parent.GetComponent(t);
            if (srcTarget != null)
            {
                targetComponent.objectReferenceValue = srcTarget;
                targetType.enumValueIndex = (int)DOTweenAnimation.TypeToDOTargetType(t);
                return true;
            }
            return false;
        }

        private DOTweenAnimationTypeEx AnimationToDOTweenAnimationTypeEx(string animation)
        {
            if (_datString == null) _datString = System.Enum.GetNames(typeof(DOTweenAnimationTypeEx));
            animation = animation.Replace("/", "");
            return (DOTweenAnimationTypeEx)(System.Array.IndexOf(_datString, animation));
        }

        private int DOTweenAnimationTypeExToPopupId(DOTweenAnimationTypeEx animation)
        {
            if (animation.ToString() == "-1")
                animation = DOTweenAnimationTypeEx.None;
            return System.Array.IndexOf(_animationTypeNoSlashes, animation.ToString());
        }

        #endregion Methods

        #region GUI Draw Methods

        private void GUIEndValueFloat()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            endValueFloat.floatValue = EditorGUILayout.FloatField(endValueFloat.floatValue);
            GUILayout.EndHorizontal();
        }

        private void GUIEndValueColor()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            endValueColor.colorValue = EditorGUILayout.ColorField(endValueColor.colorValue);
            GUILayout.EndHorizontal();
        }

        private void GUIEndValueV3(bool end, bool optionalTransform = false)
        {
            SerializedProperty toggle = useStartAsStartValue;
            if (end)
                toggle = useStartAsEndValue;

            toggle.boolValue = EditorGUILayout.Toggle("Use Starting Value", toggle.boolValue);
            if (toggle.boolValue)
                LoadCurrentValues();
            bool before = GUI.enabled;
            if (before == true)
                GUI.enabled = !toggle.boolValue;
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            if (useTargetAsV3.boolValue)
            {
                Transform prevT = endValueTransform.GetObjectValue<Transform>();
                endValueTransform.SetObjectValue(EditorGUILayout.ObjectField(endValueTransform.GetObjectValue<Transform>(), typeof(Transform), true));
                if (endValueTransform.GetObjectValue<Transform>() != prevT && endValueTransform.GetObjectValue<Transform>() != null)
                {
                    // Check that it's a Transform for a Transform or a RectTransform for a RectTransform
                    if (parent.GetComponent<RectTransform>() != null)
                    {
                        if (endValueTransform.GetObjectValue<Transform>().GetComponent<RectTransform>() == null)
                        {
                            EditorUtility.DisplayDialog("DOTween Pro", "For Unity UI elements, the target must also be a UI element", "Ok");
                            endValueTransform.SetObjectValue(null);
                        }
                    }
                    else if (endValueTransform.GetObjectValue<Transform>().GetComponent<RectTransform>() != null)
                    {
                        EditorUtility.DisplayDialog("DOTween Pro", "You can't use a UI target for a non UI object", "Ok");
                        endValueTransform.SetObjectValue(null);
                    }
                }
            }
            else
            {
                endValueV3.vector3Value = EditorGUILayout.Vector3Field("", endValueV3.vector3Value, GUILayout.Height(16));
            }
            if (optionalTransform)
            {
                if (GUILayout.Button(useTargetAsV3.boolValue ? "target" : "value", EditorGUIUtils.sideBtStyle, GUILayout.Width(44))) useTargetAsV3.boolValue = !useTargetAsV3.boolValue;
            }
            GUILayout.EndHorizontal();
            if (useTargetAsV3.boolValue && endValueTransform.GetObjectValue<Transform>() != null && targetComponent.objectReferenceValue is RectTransform)
            {
                EditorGUILayout.HelpBox("NOTE: when using a UI target, the tween will be created during Start instead of Awake", MessageType.Info);
            }
            GUI.enabled = before;
        }

        private void GUIEndValueV2()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            endValueV2.vector2Value = EditorGUILayout.Vector2Field("", endValueV2.vector2Value, GUILayout.Height(16));
            GUILayout.EndHorizontal();
        }

        private void GUIEndValueString()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            endValueString.stringValue = EditorGUILayout.TextArea(endValueString.stringValue, EditorGUIUtils.wordWrapTextArea);
            GUILayout.EndHorizontal();
        }

        private void GUIEndValueRect()
        {
            GUILayout.BeginHorizontal();
            GUIToFromButton();
            endValueRect.rectValue = EditorGUILayout.RectField(endValueRect.rectValue);
            GUILayout.EndHorizontal();
        }

        private void GUIToFromButton()
        {
            hasFrom.boolValue = false;

            //if (GUILayout.Button(isFrom.boolValue ? "FROM" : "TO", EditorGUIUtils.sideBtStyle, GUILayout.Width(90))) isFrom.boolValue = !isFrom.boolValue;
            //GUILayout.Space(16);
        }

        #endregion GUI Draw Methods
    }

    public static class Extensions
    {
        public static T GetObjectValue<T>(this SerializedProperty prop) where T : UnityEngine.Object
        {
            return (T)prop.objectReferenceValue;
        }

        public static void SetObjectValue(this SerializedProperty prop, UnityEngine.Object transform)
        {
            prop.objectReferenceValue = transform;
        }
    }
}