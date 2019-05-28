/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   14:35
	file base:	ProjectImportSettings
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IGG.AssetImportSystem
{
	using Debug = UnityEngine.Debug;
	
	/// <summary>
	/// Project import settings
	/// </summary>
	public class ProjectImportSettings : ScriptableObject
	{
		private const string SERIALIZED_FILE_PATH = "Assets/ProjectImportSettings.asset";
		
		/// <summary>
		/// All folder rules derive from this common class
		/// </summary>
		public abstract class FolderRule
		{
			// This needs to be relative to the project so we don't end up screwing everything up between team members
			public string						folderPath;
            public virtual void DoCustomRule(AssetImporter ai) { }
		}
		
		[System.Serializable]
		public class TextureFolderRule : FolderRule
		{
			public TextureImporterSettings		importSettings;
		}
		
		[System.Serializable]
		public class ModelFolderRule : FolderRule
		{
			public int		maxVertices;
			public bool		shouldBatch;

            //------------------Model-------------------
            public ModelImporterMeshCompression m_MeshCompression;
            public bool m_IsReadable;
            public bool optimizeMeshForGPU;
            public bool m_ImportBlendShapes;
            public bool m_AddColliders;
            public bool keepQuads;
            public bool weldVertices;
            public bool m_ImportVisibility;
            public bool m_ImportCameras;
            public bool m_ImportLights;
            public bool swapUVChannels;
            public bool generateSecondaryUV;
            public ModelImporterNormals normalImportMode;
            public ModelImporterTangents tangentImportMode;

            //-------------------Rig & Animation--------------------
            public ModelImporterAnimationType m_AnimationType;
            /// <summary>
            /// 只有animationType==legacy时，此值才有效
            /// </summary>
            public ModelImporterGenerateAnimations m_LegacyGenerateAnimations;

            public bool importAnimation;
            public ModelImporterAnimationCompression m_AnimationCompression;
            public bool m_OptimizeGameObjects;
            //---------------------Material------------------------
            public bool importMaterials;
			
            public int maxBones;
            public int maxBoneWeights;

            public ModelFolderRule(ProjectImportSettings.ModelFolderRule rule)
            {
                maxVertices = rule.maxVertices;
                shouldBatch = rule.shouldBatch;

                m_MeshCompression = rule.m_MeshCompression;
                m_IsReadable = rule.m_IsReadable;
                optimizeMeshForGPU = rule.optimizeMeshForGPU;
                m_ImportBlendShapes = rule.m_ImportBlendShapes;
                m_AddColliders = rule.m_AddColliders;
                keepQuads = rule.keepQuads;
                weldVertices = rule.weldVertices;
                m_ImportVisibility = rule.m_ImportVisibility;
                m_ImportCameras = rule.m_ImportCameras;
                m_ImportLights = rule.m_ImportLights;
                swapUVChannels = rule.swapUVChannels;
                generateSecondaryUV = rule.generateSecondaryUV;
                normalImportMode = rule.normalImportMode;
                tangentImportMode = rule.tangentImportMode;

                m_AnimationType = rule.m_AnimationType;
                m_LegacyGenerateAnimations = rule.m_LegacyGenerateAnimations;

                importAnimation = rule.importAnimation;
                m_AnimationCompression = rule.m_AnimationCompression;
                m_OptimizeGameObjects = rule.m_OptimizeGameObjects;

                importMaterials = rule.importMaterials;

                maxBones = rule.maxBones;
                maxBoneWeights = rule.maxBoneWeights;
            }
        }

        [System.Serializable]
        public class MapFolderRule : FolderRule {
            public bool OpenCollisionDetection = true;
        }

        /// <summary>
        /// The rules for textures
        /// </summary>
        [SerializeField]	List<TextureFolderRule>		_textureFolderRules = new List<TextureFolderRule>();
		
		[SerializeField]	List<ModelFolderRule>		_modelFolderRules = new List<ModelFolderRule>();

        [SerializeField]    List<MapFolderRule>         _mapFolderRule = new List<MapFolderRule>();

        /// <summary>
        /// Need to make sure we always load the data from the settings files!
        /// </summary>
        private static ProjectImportSettings	_instance;
		static ProjectImportSettings 			Instance
		{
			get
			{
                if (_instance)
                    return _instance;

                if ( !_instance )
				{
                    ReadProjectImportSetting();
                }
				
				if ( !_instance )
					_instance = ScriptableObject.CreateInstance<ProjectImportSettings>();
					
				return _instance;
			}
		}

        public static void ReadProjectImportSetting()
        {
            Object[] allObjs = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(SERIALIZED_FILE_PATH);
            foreach (var obj in allObjs)
            {
                ProjectImportSettings serializedInstance = obj as ProjectImportSettings;
                if (serializedInstance != null)
                    _instance = serializedInstance;
            }
        }

		public static void ApplyRulesToObject( AssetImporter assetImporter )
		{
			TextureImporter textureImporter = assetImporter as TextureImporter;
			ModelImporter modelImporter = assetImporter as ModelImporter;
			//ShaderImporter shaderAssetImporter = assetImporter as ShaderImporter;

			if ( textureImporter )
			{
				ApplyRulesToTexture(textureImporter);
			}
			else if ( modelImporter )
			{
				ApplyRulesToModel(modelImporter);
            }
		}
		
		private static void ApplyRulesToTexture( TextureImporter textureImporter )
		{
            TextureImportDataTool.TextureImport(textureImporter);
            //var rule = FindBestRule<TextureFolderRule>(textureImporter, GetAllTextureFolderRules());
            //ApplyTextureRule( textureImporter, rule );			
        }
		
		private static void ApplyRulesToModel( ModelImporter modelImporter )
		{
			var rule = FindBestRule<ModelFolderRule>(modelImporter, GetAllModelFolderRules());
			ApplyModelRule( modelImporter, rule );
		}
		
		/// <summary>
		/// Gets all texture folder rules configured for the project
		/// </summary>
		/// <returns>The all texture folder rules.</returns>
		public static IEnumerable<TextureFolderRule>	GetAllTextureFolderRules()
		{
			return Instance._textureFolderRules.AsReadOnly();
		}
		
		/// <summary>
		/// Gets all of the model folder rules configured for the project.
		/// </summary>
		/// <returns>The all model folder rules.</returns>
		public static IEnumerable<ModelFolderRule>		GetAllModelFolderRules()
		{
			return Instance._modelFolderRules.AsReadOnly();
		}
		
		/// <summary>
		/// Save the configured project import settings to the save file.
		/// </summary>
		public static void Save()
		{
			UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget( new Object[] { Instance }, SERIALIZED_FILE_PATH, true );
		}
		
		/// <summary>
		/// Allow us to inspect the asset import settings as if it were part of the editor
		/// </summary>
		[MenuItem("Edit/Project Settings/Asset Import Settings")]
		private static void InspectAssetImportSettings()
		{
			Selection.activeObject = Instance;
		}


        [MenuItem("IGG/Performance/Map Rule Test")]
        static void DealMapRuleTest() {
            GameObject[] selectedsGameobject = Selection.gameObjects;
            var select = Selection.activeObject;
            string currpath = AssetDatabase.GetAssetPath(select);

            for (int i = 0; i < selectedsGameobject.Length; i++) {
                GameObject obj = selectedsGameobject[i];
                string path = AssetDatabase.GetAssetPath(obj);
                bool bRet = IsPathLegal(path);
                if (!bRet) {
                    break;
                }

                DebugColliderFromChild(obj.transform, "");
            }

            if (0 >= selectedsGameobject.Length && 0 < currpath.Length) {
                if (!IsPathLegal(currpath)) {
                    return;
                }

                DirectoryInfo TheFolder = new DirectoryInfo(currpath);
                if (TheFolder.Exists) {
                    foreach (FileInfo NextFile in TheFolder.GetFiles()) {
                        if (null != NextFile && NextFile.Name.Contains(".prefab") && !NextFile.Name.Contains(".prefab.")) {
                            GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(currpath + "/" + NextFile.Name, typeof(GameObject));
                            if (null != obj) {
                                DebugColliderFromChild(obj.transform, currpath);
                            }
                        }
                    }
                }
            }
        }

        public static bool IsPathLegal(string strPath) {
            for (int i = 0; i < _instance._mapFolderRule.Count; i++) {
                if (strPath.Contains(_instance._mapFolderRule[i].folderPath) && _instance._mapFolderRule[i].folderPath.Length > 2) {
                    if (_instance._mapFolderRule[i].OpenCollisionDetection) {
                        return true;
                    }
                }
            }

            return false;
        }
        public static void DebugColliderFromChild(Transform trans, string path) {
            string strPath = path + "/" + trans.name;
            if (trans.GetComponent<BoxCollider>()
                || trans.GetComponent<SphereCollider>()
                || trans.GetComponent<MeshCollider>()
                || trans.GetComponent<CapsuleCollider>()
                || trans.GetComponent<WheelCollider>()
                || trans.GetComponent<Collider>()) {
                Debug.LogError("Path:" + strPath + ":" + "There is a collider in gameobject.");
            }

            foreach (Transform child in trans) {
                DebugColliderFromChild(child, strPath);
            }
        }
   

        /// <summary>
        /// Find the best-match rule that corresponds to a given importer.
        /// </summary>
        /// <returns>The best matching rule (or null)</returns>
        /// <param name="assetImporter">The AssetImporter (comes from an asset)</param>
        /// <param name="folderRules">The list of rules to choose from</param> 
        public static T FindBestRule<T>( AssetImporter assetImporter, IEnumerable<T> folderRules ) where T : FolderRule
		{
			string assetPath = System.IO.Path.GetFullPath(assetImporter.assetPath);
			
			// Find best matching texture folder rule.  Best match is the longest matching full path
			FolderRule bestMatch = null;
			foreach( var rule in folderRules )
			{
				// Is this rule a parent of the path?
				string folderPath = System.IO.Path.GetFullPath(rule.folderPath);
				if ( assetPath.StartsWith(folderPath) )
				{
					// Best match is the match with the longest path (i.e. this is the closest folder)
					if ( (bestMatch == null) || rule.folderPath.Length > bestMatch.folderPath.Length )
						bestMatch = rule;
				}
			}
			
			return (T)bestMatch;
		}
		
		/// <summary>
		/// Applies the texture rule.
		/// </summary>
		/// <param name="textureImporter">Texture importer</param>
		/// <param name="rule">Rule</param>
		private static void ApplyTextureRule( TextureImporter textureImporter, TextureFolderRule rule )
		{
			if ( rule != null )
			{
				Debug.Log( "<color=#00ff00>Applying Rule: " + rule.folderPath + " to asset '" + textureImporter.assetPath + "'</color>");
				textureImporter.SetTextureSettings( rule.importSettings );
			}
		}
		
		private static void ApplyModelRule( ModelImporter modelImporter, ModelFolderRule rule )
		{
			if ( rule == null )
				return;

            ModelFolderRule newRule = CustomImportRuleFactory.Create(modelImporter, rule);
            if (null == newRule)
            {
                return;
            }
			
			SerializedObject modelSerObj = new SerializedObject(modelImporter);
			SerializedObjectUtility.CopyInstanceToSerializedObject( modelSerObj.GetIterator(), newRule);
			modelSerObj.ApplyModifiedProperties();
            AssetDatabase.Refresh();
            newRule.DoCustomRule(modelImporter);
		}
    }
}
