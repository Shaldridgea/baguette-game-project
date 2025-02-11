﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UpgradeValueInt))]
[CustomPropertyDrawer(typeof(UpgradeValueFloat))]
[CustomPropertyDrawer(typeof(UpgradeValueBool))]
[CustomPropertyDrawer(typeof(UpgradeValueString))]
public class UpgradeValueDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);
        
        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rects
        var amountRect = new Rect(position.x, position.y, position.width, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("rawValue"), GUIContent.none);

        EditorGUI.EndProperty();
    }
}