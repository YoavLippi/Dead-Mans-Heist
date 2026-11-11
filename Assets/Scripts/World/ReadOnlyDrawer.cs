using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ReadableOnlyAttribute : PropertyAttribute {} //Custom Attribute for me to point to 
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadableOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label,true);
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label,true);
    }
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var propertyRoot = new PropertyField(property);
        propertyRoot.SetEnabled(false);
        return propertyRoot;
    }
}
#endif
