/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   14:36
	file base:	ProjectImportSettingsEditor
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/
using UnityEditor;
using UnityEngine;

namespace IGG.AssetImportSystem
{
    /// <summary>
    /// Project import settings editor.  We need to pretty-up a bunch of functionality otherwise everything becomes unwieldy.
    /// </summary>
    [CustomEditor( typeof(ProjectImportSettings) )]
	public class ProjectImportSettingsEditor : UnityEditor.Editor
    {
		/// <summary>
		/// Whenever we close this editor window, we should save the settings to a file
		/// </summary>
		void OnDisable()
		{
			//ProjectImportSettings.Save();
		}
		
		public override void OnInspectorGUI ()
		{
			// Start with some help...
			EditorGUILayout.HelpBox( "When adding entries, you'll almost always want to use the template drag 'n drop functionality at the bottom.  Simply drag an asset that satisfies all of the constraints you want to impose.  A new entry will be added for that asset, then modify the folder path manually.", MessageType.Info );
			EditorGUILayout.HelpBox( "Import Settings are matched to the closest folder to where the asset lives.  Folder paths are matched by string-comparison, so the longest folder name that still applies is the best match.", MessageType.Info );

			base.OnInspectorGUI ();
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			// Allow us to create a new entry using an existing Texture as a template.  This is really the only sensible way to create entries...
			Texture template = EditorGUILayout.ObjectField( "Create Entry Using Template", null, typeof(Texture), false ) as Texture;
			if ( !template )
				return;
			
			// First copy over the TextureImportSettings from the template we were just given
			string templatePath = AssetDatabase.GetAssetPath(template);
			
			TextureImporterSettings templateImportSettings = new TextureImporterSettings();
			TextureImporter textureImporter = TextureImporter.GetAtPath( templatePath ) as TextureImporter;
			textureImporter.ReadTextureSettings( templateImportSettings );
			
			// Now we need to copy these settings to a new Texture Folder Rule.  Unfortunately, we're dealing with objects that don't
			// derive from Object so we need to do this all using the SerializedObject+Property system
			var propTextureFolderRules = serializedObject.FindProperty( "_textureFolderRules" );
			
			// Add an entry to the array
			int index = propTextureFolderRules.arraySize;
			propTextureFolderRules.InsertArrayElementAtIndex( index );

			// Now access that new entry and start filling it in with the path
			var propNewEntry = propTextureFolderRules.GetArrayElementAtIndex( index );
			propNewEntry.FindPropertyRelative( "folderPath" ).stringValue = AssetDatabase.GetAssetPath(template);

            // Now copy the template to the texture folder rule's importSettings property
            SerializedObjectUtility.CopyInstanceToSerializedObject( propNewEntry.FindPropertyRelative("importSettings"), templateImportSettings );
			serializedObject.ApplyModifiedProperties();
		}
	}
}
