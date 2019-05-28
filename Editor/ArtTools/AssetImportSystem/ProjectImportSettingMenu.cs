using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace IGG.AssetImportSystem
{
    public class ProjectImportSettingMenu : EditorWindow
    {

        //[MenuItem("辅助工具/资源导入/保存设置")]
        //static void SaveProjectImportSetting()
        //{
        //    ProjectImportSettings.Save();
        //}

        [MenuItem("辅助工具/资源管理/更新导入设置")]
        static void ReadProjectImportSetting()
        {
            ProjectImportSettings.ReadProjectImportSetting();
            BuildingAssets.ReadBuildingImportSetting();
        }

    }
}
