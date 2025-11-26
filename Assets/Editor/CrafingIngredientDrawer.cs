using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


[CustomPropertyDrawer(typeof(ItemID),true)]
public class CrafingIngredientDrawer : PropertyDrawer
{
  //  private bool foldout = true;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

     //   foldout = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), foldout, label);

      //  if (foldout)
  
            EditorGUI.indentLevel++;

            var buttonRect = new Rect(position.x + position.width * 0.5f, position.y + EditorGUIUtility.singleLineHeight + 2, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            var itemRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

            var prop = property.Copy();
            prop.Next(true);
            var endProperty = property.GetEndProperty();
            position.y += 2 * (EditorGUIUtility.singleLineHeight + 2);
            while (prop.NextVisible(true) && !SerializedProperty.EqualContents(prop, endProperty))
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), prop, true);
                position.y += EditorGUIUtility.singleLineHeight + 2;
            }


            var valueRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 2, position.width, EditorGUIUtility.singleLineHeight);
           
   
            var itemId = property.FindPropertyRelative("itemID");
            EditorGUI.PropertyField(itemRect, itemId, new GUIContent("itemID"));
            //EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("number"), new GUIContent("number"));
           

            string buttonText = ItemList.GetItemName(itemId.intValue);
            if (buttonText == null)
            { 
               if (itemId.intValue == -1) buttonText += "Null";
               else buttonText = "Select Item ID";
            }
            else buttonText += $" [ID: {itemId.intValue}]";


            var buttonContent = new GUIContent(buttonText,ItemList.GetIcon(itemId.intValue));
            if (GUI.Button(buttonRect,buttonContent, EditorStyles.popup))
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), new ItemFinder((x) => {
                    itemId.intValue = x; 
                    property.serializedObject.ApplyModifiedProperties();
                }));
            }
            EditorGUI.indentLevel--;
       

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
       // if (foldout)
      //  {
            int count = 0;
            var prop = property.Copy();
            var endProperty = property.GetEndProperty();
            while (prop.NextVisible(true) && !SerializedProperty.EqualContents(prop, endProperty))
            {
                count++;
            }
            return (EditorGUIUtility.singleLineHeight + 2) * (count + 1); // +1 dla g³ównego pola
    //    }
        //else
        //{
        //    return EditorGUIUtility.singleLineHeight;
        //}
    }
}