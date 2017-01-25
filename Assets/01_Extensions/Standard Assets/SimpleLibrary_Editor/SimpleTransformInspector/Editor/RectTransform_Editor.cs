using System.Collections;

using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace SimpleLibrary
{
	[CustomEditor(typeof(Object))]
	public class RectTransform_Editor : Editor
	{
		RectTransform rect = null;
		public void OnEnable()
		{
			rect = null;
			if (target != null)
				rect = ((GameObject)target).GetComponent<RectTransform>();
		}

		public void OnSceneGUI()
		{
			if (rect == null)
				return;
			Handles.BeginGUI();
			Rect position = new Rect(0, 0, 0, 0);

			position.width = 100;
			position.height = 20;
			GUI.Button(position, new GUIContent("Round Position", ""));
			Handles.EndGUI();
		}
	}
}

#endif