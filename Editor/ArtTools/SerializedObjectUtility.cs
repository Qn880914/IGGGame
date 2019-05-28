/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   14:36
	file base:	SerializedObjectUtility
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class SerializedObjectUtility
{

	public static void DebugPrintAllProperties(SerializedObject pObject)
	{
		SerializedProperty objectIterator = pObject.GetIterator();
		string iteratorResult = "Iterating SerializedObject of "+objectIterator.type+" '"+objectIterator.name+"': \n\t";
		objectIterator.Next(true);
		do
		{
			iteratorResult += string.IsNullOrEmpty(objectIterator.type)? "(no type)" : objectIterator.type;
			iteratorResult += " '";
			iteratorResult += string.IsNullOrEmpty(objectIterator.name)? "(no name)": objectIterator.name;
			iteratorResult += "'\n\t";
		} while (objectIterator.Next(false));
		Debug.Log(iteratorResult);
	}

	/// <summary>
	/// Copies a serialized object to an instance.  Normally we would use EditorUtility.CopySerialized, however we need this function
	/// because Unity does not deal with objects that do not derive from UnityEngine.Object.
	/// </summary>
	/// <param name="destRoot">The first property of the destination</param>
	/// <param name="sourceObj">Source object.</param>
	public static void CopyInstanceToSerializedObject( SerializedProperty destRoot, object sourceObj )
	{
		var allFields = sourceObj.GetType().GetFields(System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
		
		SerializedProperty prop = destRoot.Copy();
		while ( prop.Next(true) )
		{
            var sourceField = allFields.FirstOrDefault( x => x.Name == prop.name );

            if (sourceField != null) {
                var fieldValue = sourceField.GetValue(sourceObj);
                CopySerializedFromValue(prop, fieldValue);
            }
        }
	}
	
	/// <summary>
	/// Copies a value to the destination SerializedProperty
	/// </summary>
	/// <param name="dest">Destination.</param>
	/// <param name="value">Value.</param>
	public static void CopySerializedFromValue( SerializedProperty dest, object value )
	{
		switch( dest.propertyType )
		{
		case SerializedPropertyType.ArraySize:
			dest.arraySize = (int)value;
			break;
			
		case SerializedPropertyType.Boolean:
			dest.boolValue = (bool)value;
			break;
			
		case SerializedPropertyType.Character:
			dest.intValue = (char)value;
			break;
			
		case SerializedPropertyType.Enum:
			dest.enumValueIndex = (int)value;
			break;
			
		case SerializedPropertyType.Float:
			dest.floatValue = (float)value;
			break;
			
		case SerializedPropertyType.Integer:
                if (dest.type == "int")
                    dest.intValue = (int)value;
                break;
			
		case SerializedPropertyType.String:
			dest.stringValue = (string)value;
			break;
			
		case SerializedPropertyType.Vector2:
			dest.vector2Value = (Vector2)value;
			break;
			
		case SerializedPropertyType.Vector3:
			dest.vector3Value = (Vector3)value;
			break;
			
		default:
			UnityEngine.Debug.LogError( "Could not copy value type: " + dest.propertyType );
			break;
		}
	}}