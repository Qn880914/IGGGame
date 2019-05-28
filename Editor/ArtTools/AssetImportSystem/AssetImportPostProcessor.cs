/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   14:35
	file base:	AssetImportPostProcessor
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Linq;

namespace IGG.AssetImportSystem
{
	/// <summary>
	/// Keep track of assets importing
	/// </summary>
	public class AssetImportPostProcessor : AssetPostprocessor
	{
		/// <summary>
		/// This is a Unity callback for when we are about to import a Texture
		/// </summary>
		private void OnPreprocessTexture()
		{
			// Only apply to new assets
			if ( !IsAssetNew(assetImporter) )
				return;
			
			ProjectImportSettings.ApplyRulesToObject( assetImporter );
		}

		/// <summary>
		/// We're about to import a model, so make sure all of our import settings are suitable.
		///</summary>
		private void OnPreprocessModel()
        {
            if (!IsAssetNew(assetImporter))
                return;

            ProjectImportSettings.ApplyRulesToObject( assetImporter );
		}

		/// <summary>
		/// Is the asset passed-in a new import, or did it already exist?
		/// The idea is that if it already exists, someone could have set it up manually.
		/// </summary>
		private static bool IsAssetNew( AssetImporter assetImporter )
		{
			// We should only override the import settings on a new import.  Allow the existing settings to persist...
			string metaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath( assetImporter.assetPath );
			bool bIsNewImport = !System.IO.File.Exists(metaPath);
			
			return bIsNewImport;
		}
	}
}
