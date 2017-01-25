using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// A simple "ReadOnly" PropertyDrawer, draws the Property as expected but it can't be edited
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true;
	}
}

#endif

public class ReadOnlyAttribute : PropertyAttribute
{
}