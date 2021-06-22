using UnityEngine;
using UnityEditor;
using Fighter.Attributes;
using Fighter.Types;

namespace Fighter.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // cast the attribute to make life easier
            MinMaxAttribute minMax = attribute as MinMaxAttribute;

            // This only works on a vector2 and vector2Int! ignore on any other property type (we should probably draw an error message instead!)
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                // if we are flagged to draw in a special mode, lets modify the drawing rectangle to draw only one line at a time
                if (minMax.showDebugValues || minMax.showEditRange)
                {
                    position = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                }

                // pull out a bunch of helpful min/max values....
                float minValue = property.vector2Value.x; // the currently set minimum and maximum value
                float maxValue = property.vector2Value.y;
                float minLimit = minMax.minLimit; // the limit for both min and max, min cant go lower than minLimit and max cant top maxLimit
                float maxLimit = minMax.maxLimit;

                // and ask unity to draw them all nice for us!
                EditorGUI.MinMaxSlider(position, label, ref minValue, ref maxValue, minLimit, maxLimit);

                var vec = Vector2.zero; // save the results into the property!
                vec.x = minValue;
                vec.y = maxValue;

                property.vector2Value = vec;

                // Do we have a special mode flagged? time to draw lines!
                if (minMax.showDebugValues || minMax.showEditRange)
                {
                    bool isEditable = false;
                    if (minMax.showEditRange)
                    {
                        isEditable = true;
                    }

                    if (!isEditable)
                        GUI.enabled = false; // if were just in debug mode and not edit mode, make sure all the UI is read only!

                    // move the draw rect on by one line
                    position.y += EditorGUIUtility.singleLineHeight;

                    float[] vals = new float[] { minLimit, minValue, maxValue, maxLimit }; // shove the values and limits into a vector4 and draw them all at once
                    EditorGUI.MultiFloatField(position, new GUIContent("Range"), new GUIContent[] { new GUIContent("minLimit"), new GUIContent("MinVal"), new GUIContent("MaxVal"), new GUIContent("maxLimit") }, vals);

                    GUI.enabled = false; // the range part is always read only
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.FloatField(position, "Selected Range", maxValue - minValue);
                    GUI.enabled = true; // remember to make the UI editable again!

                    if (isEditable)
                    {
                        property.vector2Value = new Vector2(vals[1], vals[2]); // save off any change to the value~
                    }
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                // if we are flagged to draw in a special mode, lets modify the drawing rectangle to draw only one line at a time
                if (minMax.showDebugValues || minMax.showEditRange)
                {
                    position = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                }

                // pull out a bunch of helpful min/max values....
                float minValue = property.vector2IntValue.x; // the currently set minimum and maximum value
                float maxValue = property.vector2IntValue.y;
                int minLimit = Mathf.RoundToInt(minMax.minLimit); // the limit for both min and max, min cant go lower than minLimit and max cant top maxLimit
                int maxLimit = Mathf.RoundToInt(minMax.maxLimit);

                // and ask unity to draw them all nice for us!
                EditorGUI.MinMaxSlider(position, label, ref minValue, ref maxValue, minLimit, maxLimit);

                var vec = Vector2Int.zero; // save the results into the property!
                vec.x = Mathf.RoundToInt(minValue);
                vec.y = Mathf.RoundToInt(maxValue);

                property.vector2IntValue = vec;

                // Do we have a special mode flagged? time to draw lines!
                if (minMax.showDebugValues || minMax.showEditRange)
                {
                    bool isEditable = false;
                    if (minMax.showEditRange)
                    {
                        isEditable = true;
                    }

                    if (!isEditable)
                        GUI.enabled = false; // if were just in debug mode and not edit mode, make sure all the UI is read only!

                    // move the draw rect on by one line
                    position.y += EditorGUIUtility.singleLineHeight;

                    float[] vals = new float[] { minLimit, minValue, maxValue, maxLimit }; // shove the values and limits into a vector4 and draw them all at once
                    EditorGUI.MultiFloatField(position, new GUIContent("Range"), new GUIContent[] { new GUIContent("minLimit"), new GUIContent("MinVal"), new GUIContent("MaxVal"), new GUIContent("maxLimit") }, vals);

                    GUI.enabled = false; // the range part is always read only
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.FloatField(position, "Selected Range", maxValue - minValue);
                    GUI.enabled = true; // remember to make the UI editable again!

                    if (isEditable)
                    {
                        property.vector2IntValue = new Vector2Int(Mathf.RoundToInt(vals[1]), Mathf.RoundToInt(vals[2])); // save off any change to the value~
                    }
                }
            }
            else
            {
                SerializedProperty minProperty = property.FindPropertyRelative("min");
                SerializedProperty maxProperty = property.FindPropertyRelative("max");

                if (minProperty == null || maxProperty == null)
                {
                    return;
                }

                // if we are flagged to draw in a special mode, lets modify the drawing rectangle to draw only one line at a time
                if (minMax.showDebugValues || minMax.showEditRange)
                {
                    position = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                }

                // pull out a bunch of helpful min/max values....
                float minValue = minProperty.floatValue;
                float maxValue = maxProperty.floatValue;
                float minLimit = minMax.minLimit; // the limit for both min and max, min cant go lower than minLimit and max cant top maxLimit
                float maxLimit = minMax.maxLimit;

                // and ask unity to draw them all nice for us!
                EditorGUI.MinMaxSlider(position, label, ref minValue, ref maxValue, minLimit, maxLimit);

                var vec = Vector2.zero; // save the results into the property!
                vec.x = minValue;
                vec.y = maxValue;

                minProperty.floatValue = vec.x;
                maxProperty.floatValue = vec.y;

                // Do we have a special mode flagged? time to draw lines!
                if (minMax.showDebugValues || minMax.showEditRange)
                {
                    bool isEditable = false;
                    if (minMax.showEditRange)
                    {
                        isEditable = true;
                    }

                    if (!isEditable)
                        GUI.enabled = false; // if were just in debug mode and not edit mode, make sure all the UI is read only!

                    // move the draw rect on by one line
                    position.y += EditorGUIUtility.singleLineHeight;

                    float[] vals = new float[] { minLimit, minValue, maxValue, maxLimit }; // shove the values and limits into a vector4 and draw them all at once
                    EditorGUI.MultiFloatField(position, new GUIContent("Range"), new GUIContent[] { new GUIContent("minLimit"), new GUIContent("MinVal"), new GUIContent("MaxVal"), new GUIContent("maxLimit") }, vals);

                    GUI.enabled = false; // the range part is always read only
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.FloatField(position, "Selected Range", maxValue - minValue);
                    GUI.enabled = true; // remember to make the UI editable again!

                    if (isEditable)
                    {
                        minProperty.floatValue = vals[1];
                        maxProperty.floatValue = vals[2];
                    }
                }
            }
        }

        // this method lets unity know how big to draw the property. We need to override this because it could end up being more than one line big
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            MinMaxAttribute minMax = attribute as MinMaxAttribute;

            // by default just return the standard line height
            float size = EditorGUIUtility.singleLineHeight;

            // if we have a special mode, add two extra lines!
            if (minMax.showEditRange || minMax.showDebugValues)
            {
                size += EditorGUIUtility.singleLineHeight * 2;
            }

            return size;
        }

    }
}