using UnityEditor;
using UnityEngine;

/// <summary>
/// Code from this gist by vertxxyz https://gist.github.com/vertxxyz/8f0f73251cfad898407ceff3a2a2a432
/// </summary>
/// 
namespace Alchemy
{
	[CustomPropertyDrawer(typeof(KeyCodeAttribute))]
	public class KeyCodeAttributeDrawer : PropertyDrawer
	{
		private GUIStyle objectFieldStyle;
		private GUIStyle ObjectFieldStyle => objectFieldStyle ?? (objectFieldStyle = "IN ObjectField");

		const float widthInput = 18;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//KeyCodeAttribute a = attribute as KeyCodeAttribute;
			position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

			Rect pickerRect = new Rect(position.x + position.width, position.y, widthInput, position.height);
			int id = GUIUtility.GetControlID((int)pickerRect.x, FocusType.Keyboard, pickerRect);

			EditorGUI.PropertyField(position, property, GUIContent.none);
			position.width = widthInput;

			if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = id;
				Event.current.Use();
			}

			position.y -= 2;
			position.x += 1;
			GUI.Label(position, GUIContent.none, ObjectFieldStyle);
			if (GUIUtility.keyboardControl == id && Event.current.type == EventType.KeyUp)
			{
				if (Event.current.keyCode != KeyCode.Escape)
					property.enumValueIndex = KeyCodeToEnumIndex(property, Event.current.keyCode);
				Event.current.Use();
				GUIUtility.keyboardControl = -1;
			}
			else if (GUIUtility.keyboardControl == id && Event.current.isKey)
				Event.current.Use();
		}

		private static int KeyCodeToEnumIndex(SerializedProperty keyCodeProperty, KeyCode keyCode)
		{
			string[] keyCodeNames = keyCodeProperty.enumNames;
			string query = keyCode.ToString();
			for (int i = 0; i < keyCodeNames.Length; i++)
			{
				if (keyCodeNames[i].Equals(query))
					return i;
			}

			return 0;
		}
	}
}