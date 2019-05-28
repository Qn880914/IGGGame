/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   14:36
	file base:	TextureAssetValidation
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace IGG.Validation
{
	using Debug = UnityEngine.Debug;
	using IGG.AssetImportSystem;

	/// <summary>
	/// Figure out if any given texture assets violate our texture specifications
	/// </summary>
	public class TextureAssetValidation : AssetPostprocessor
	{
		/// <summary>
		/// Give us the ability to test selected textures against our project settings.
		/// </summary>
		[MenuItem("IGG/Performance/Selected/Test Textures", true)]
		private static bool CanPerformanceTestSelectedObjects()
		{
			return Selection.objects.Any( x => x is Texture );
		}

		/// <summary>
		/// Give us the ability to test selected textures against our project settings.
		/// </summary>
		[MenuItem("IGG/Performance/Selected/Test Textures", false)]
		private static void PerformanceTestSelectedObjects()
		{
			var selectedTextures = Selection.objects.Where(x => x is Texture).Cast<Texture>();
			var invalidTextures = ValidateTextures( selectedTextures.ToArray() );
			
			if ( invalidTextures.Count > 0 )
			{
				EditorUtility.DisplayDialog( "Performance Warning", "Some of the selected assets exceed the performance guidelines set for the project.  Look in the Console Window for details.", "Thanks!" );
				EditorApplication.ExecuteMenuItem( "Window/Console" );
			}
			else
			{
				EditorUtility.DisplayDialog( "Passed", "All of the selected textures conform to their rules and are deemed suitable for shipping.", "Woo hoo!" );
			}
		}
		
		[MenuItem("IGG/Performance/Project/Test All Textures")]
		private static void VerifyAllTextureAssets()
		{
			Texture[] allTextures = Resources.FindObjectsOfTypeAll<Texture>();
			var allTextureAssets = allTextures.Where( x => EditorUtility.IsPersistent(x) );

			var invalidTextures = ValidateTextures( allTextureAssets );
			if ( invalidTextures.Count > 0 )
			{
				EditorUtility.DisplayDialog( "Project Errors", "Some texture assets in the project do not conform to their guidelines.  Check the console for more details.", "Thanks!" );
				EditorApplication.ExecuteMenuItem( "Window/Console" );
			}
			else
			{
				EditorUtility.DisplayDialog( "It's a Miracle", "Somehow, someway, there are no issues with texture assets in the project.  Double-check the Asset Importer to ensure it's set-up correctly, otherwise it's a release-day miracle!", "Woo hoo!" );
			}
		}
		
		/// <summary>
		/// Are the texture rules optimal (do they conform to our project settings?)
		/// </summary>
		/// <returns>A list of textures that violate our validation policies</returns>
		/// <param name="allTextures">All textures to check.</param>
		public static List<Texture> ValidateTextures( IEnumerable<Texture> allTextures )
		{
			List<Texture>	violatedTextures = new List<Texture>();
			
			foreach( var texture in allTextures )
			{
				if ( !EditorUtility.IsPersistent(texture) )
					continue;
					
				string assetPath = AssetDatabase.GetAssetPath( texture );

				bool isInternalAsset = string.IsNullOrEmpty(assetPath);
				if ( isInternalAsset )
					continue;
				
				TextureImporter textureImporter = TextureImporter.GetAtPath( assetPath ) as TextureImporter;
				if ( !textureImporter )
				{
					Debug.LogError( string.Format("Asset '{0}' with path '{1}' had no texture importer (weird, skipping)", texture.name, assetPath), texture );
					continue;
				}

				string formatErrors = GetCompressFormatErrors( textureImporter );
				if ( !string.IsNullOrEmpty(formatErrors) )
				{
					Debug.LogError( "Asset '" + assetPath + "' has non-optimal format: " + formatErrors, texture );
					violatedTextures.Add(texture);

					continue;
				}

				var bestFolderRule = ProjectImportSettings.FindBestRule( textureImporter, ProjectImportSettings.GetAllTextureFolderRules() );
				string errorString = GetTextureFolderRuleErrors(bestFolderRule, textureImporter);		
				if ( !string.IsNullOrEmpty(errorString) )
				{
					Debug.LogError( "Asset '" + assetPath + "' violates its folder rule: " + bestFolderRule.folderPath + " (" + errorString + ")", texture );
					violatedTextures.Add(texture);
					
					continue;
				}
			}
			
			return violatedTextures;
		}
		
		/// <summary>
		/// Inspects a given texture and sees if the compression format is optimal
		/// </summary>
		/// <returns>The compress format error as a string (or null if no errors).</returns>
		/// <param name="textureImporter">Texture importer.</param>
		private static string GetCompressFormatErrors( TextureImporter textureImporter )
		{
			bool bHasAlpha = textureImporter.DoesSourceTextureHaveAlpha();
			
			if ( bHasAlpha )
			{
				switch ( textureImporter.textureFormat )
				{
					case TextureImporterFormat.Alpha8:
					case TextureImporterFormat.ASTC_RGBA_10x10:
					//case TextureImporterFormat.ATF_RGBA_JPG:
					case TextureImporterFormat.AutomaticCompressed:
					case TextureImporterFormat.DXT5:
					case TextureImporterFormat.ETC2_RGBA8:
					case TextureImporterFormat.PVRTC_RGBA2:
					case TextureImporterFormat.PVRTC_RGBA4:
						return null;
						
					default:
						return "Texture has alpha but compression format does not support alpha (alpha will not show up in-game)";
				}
			}
			else
			{
				switch ( textureImporter.textureFormat )
				{
					case TextureImporterFormat.ASTC_RGB_4x4:
					case TextureImporterFormat.ASTC_RGB_5x5:
					case TextureImporterFormat.ASTC_RGB_6x6:
					case TextureImporterFormat.ASTC_RGB_8x8:
					case TextureImporterFormat.ETC_RGB4:
					//case TextureImporterFormat.ATF_RGB_DXT1:
					//case TextureImporterFormat.ATF_RGB_JPG:
					case TextureImporterFormat.AutomaticCompressed:
					case TextureImporterFormat.Automatic16bit:
					case TextureImporterFormat.DXT1:
					case TextureImporterFormat.ETC2_RGB4:
					case TextureImporterFormat.PVRTC_RGB2:
					case TextureImporterFormat.PVRTC_RGB4:
						return null;
				
					default:
						return "Texture does not have alpha but compression format assumes alpha (memory waste)";
				}
			}
		}
		
		/// <summary>
		/// Determines if a texture conforms to a given TextureFolderRules.
		/// </summary>
		/// <returns><c>an error string</c> if is texture rule validated the specified bestFolderRule textureImporter; otherwise, <c>null</c>.</returns>
		/// <param name="bestFolderRule">Best folder rule.</param>
		/// <param name="textureImporter">Texture importer.</param>
		static string GetTextureFolderRuleErrors (ProjectImportSettings.TextureFolderRule bestFolderRule, TextureImporter textureImporter)
		{
			if ( bestFolderRule == null )
				return null;
			
			// See if the texture import settings are equal
			TextureImporterSettings textureImportSettings = new TextureImporterSettings();
			textureImporter.ReadTextureSettings( textureImportSettings );

			string[] ignoreProperties = new string[]
			{
				"spriteAlignment",
				"spriteExtrude",
				"spriteMeshType",
				"spriteMode",
				"spritePivot",
				"spritePixelsToUnits"		
			};
			
			var violateProperty = ShallowEquals(bestFolderRule.importSettings, textureImportSettings, ignoreProperties);
			if ( violateProperty != null )
				return "Import Settings are not the same as expected: " + violateProperty;
				
			return null;
		}
		
		private static System.Reflection.PropertyInfo ShallowEquals(object obj1, object obj2, IEnumerable<string> ignore)
		{
			// we're not even checking whether 'obj' is of the same type
			foreach (var property in obj1.GetType().GetProperties())
			{
				if ( ignore != null && ignore.Contains(property.Name) )
					continue;
					
				if (!property.GetValue(obj1, null).Equals(property.GetValue(obj2, null)))
					return property;
			}
			
			return null;
		}
	}
}
