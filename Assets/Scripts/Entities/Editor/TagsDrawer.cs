using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


[CustomPropertyDrawer(typeof(TagSelection),true)]
public class TagsDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);


            EditorGUI.indentLevel++;

            var buttonRect = new Rect(position.x + position.width * 0.5f, position.y + EditorGUIUtility.singleLineHeight + 1, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            var itemRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 1, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

            var prop = property.Copy();
            prop.Next(true);
            var endProperty = property.GetEndProperty();
            position.y += 2 * (EditorGUIUtility.singleLineHeight + 1);
            while (prop.NextVisible(true) && !SerializedProperty.EqualContents(prop, endProperty))
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), prop, true);
                position.y += EditorGUIUtility.singleLineHeight + 1;
            }


            var valueRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 1) * 2, position.width, EditorGUIUtility.singleLineHeight);
           
   
            var itemId = property.FindPropertyRelative("tagID");
            EditorGUI.PropertyField(itemRect, itemId, new GUIContent("tagID"));
           

            string buttonText = TagList.GetTagName(itemId.intValue);
            if (buttonText == null)
            { 
               if (itemId.intValue == -1) buttonText += "Null";
               else buttonText = "Select Tag";
            }
            else buttonText += $" [ID: {itemId.intValue}]";


            var buttonContent = new GUIContent(buttonText,TagList.GetIcon(itemId.intValue));
            if (GUI.Button(buttonRect,buttonContent, EditorStyles.popup))
            {
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), new TagFinder((x) =>
            {
                itemId.intValue = x;
                property.serializedObject.ApplyModifiedProperties();
            }));
        }
            EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int count = 0;
        var prop = property.Copy();
        var endProperty = property.GetEndProperty();
        while (prop.NextVisible(true) && !SerializedProperty.EqualContents(prop, endProperty))
        {
            count++;
        }
        return (EditorGUIUtility.singleLineHeight + 1) * (count + 1);
    }
}