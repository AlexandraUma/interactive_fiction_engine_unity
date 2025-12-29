#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Property drawer for all <see cref="BaseObjectProperty"/> types.
///
/// This works with the <see cref="SerializeReference"/> list on <see cref="BaseObject"/>,
/// and shows the generated Id (the concrete type name) as a read-only field above
/// the normal property contents.
/// </summary>
[CustomPropertyDrawer(typeof(BaseObjectProperty), true)]
public class BaseObjectPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Extra height for the Id and Description fields + spacing,
        // plus whatever Unity needs for the rest of the property.
        float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);
        float line = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        // Two extra lines: Id and Description.
        return baseHeight + (line * 2f);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        var managed = property.managedReferenceValue as BaseObjectProperty;
        string idValue = managed != null ? managed.Id : "(null)";
        string descriptionValue = managed != null ? managed.Description : string.Empty;

        // First line: read-only Id
        Rect idRect = new Rect(position.x, position.y, position.width, lineHeight);

        // Second line: read-only Description
        Rect descriptionRect = new Rect(
            position.x,
            position.y + lineHeight + spacing,
            position.width,
            lineHeight);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUI.TextField(idRect, "Id", idValue);
            EditorGUI.TextField(descriptionRect, "Description", descriptionValue);
        }

        // Rest of the property: draw default inspector for this managed reference
        Rect contentRect = new Rect(
            position.x,
            position.y + (lineHeight + spacing) * 2f,
            position.width,
            position.height - (lineHeight + spacing) * 2f
        );

        EditorGUI.indentLevel++;
        EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }
}
#endif

