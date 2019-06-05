using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace AssetBundleBrowser
{
    /// <summary>
    /// AssetBundle报告
    /// 生成各个资源的报告Excel
    /// </summary>
    public class AssetBundleReporter
    {
        [MenuItem("Window/AssetBundleReporter")]
        public static void AnalyzePrintCmd()
        {
            string assetBundlePath = EditorUtility.OpenFolderPanel(Application.streamingAssetsPath, "", "");
            if (string.IsNullOrEmpty(assetBundlePath))
            {
                return;
            }

            string outputPath = Path.Combine(assetBundlePath,
                string.Format("AssetBundle报告{0}{1}", DateTime.Now.ToString("yyyyMMdd_HHmmss"), ".xlsx"));

            AnalyzePrint(assetBundlePath, outputPath);
        }

        /// <summary>
        /// 分析打印 AssetBundle
        /// </summary>
        /// <param name="assetBundlePath">AssetBundle 文件所在文件夹路径</param>
        /// <param name="outputPath">Excel 报告文件保存路径</param>
        /// <param name="completedCallback">分析打印完毕后的回调</param>
        public static void AnalyzePrint(string assetBundlePath, string outputPath, UnityAction completedCallback = null)
        {
            AssetBundleFilesAnalyze.analyzeCompletedCallback = () =>
            {
                PrintToExcel(outputPath);

                completedCallback?.Invoke();
            };

            EditorUtility.DisplayProgressBar("AssetBundle报告", "正在分析 AssetBundle 文件...", 0.35f);
            if (!AssetBundleFilesAnalyze.Analyze(assetBundlePath))
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void PrintToExcel(string outputPath)
        {
            EditorUtility.DisplayProgressBar("AssetBundle报告", "正在写入 Excel 文件...", 0.85f);
            var newFile = new FileInfo(outputPath);
            if (newFile.Exists)
            {
                newFile.Delete();
            }

            using (var package = new ExcelPackage(newFile))
            {
                AssetBundleFilesReporter.CreateWorksheet(package.Workbook.Worksheets);
                AssetBundleDetailsReporter.CreateWorksheet(package.Workbook.Worksheets);
                AssetBundleResReporter.CreateWorksheet(package.Workbook.Worksheets);

                AssetBundleFilesReporter.FillWorksheet(package.Workbook.Worksheets[1]);
                AssetBundleDetailsReporter.FillWorksheet(package.Workbook.Worksheets[2]);
                AssetBundleResReporter.FillWorksheet(package.Workbook.Worksheets[3]);

                AssetBundlePropertyReporter.CreateAndFillWorksheet(package.Workbook.Worksheets, AssetFileInfoType.mesh);
                AssetBundlePropertyReporter.CreateAndFillWorksheet(package.Workbook.Worksheets, AssetFileInfoType.texture2D);
                AssetBundlePropertyReporter.CreateAndFillWorksheet(package.Workbook.Worksheets, AssetFileInfoType.material);
                AssetBundlePropertyReporter.CreateAndFillWorksheet(package.Workbook.Worksheets, AssetFileInfoType.animationClip);
                AssetBundlePropertyReporter.CreateAndFillWorksheet(package.Workbook.Worksheets, AssetFileInfoType.audioClip);

                package.Save();
            }

            AssetBundleFilesAnalyze.Clear();
            EditorUtility.ClearProgressBar();
        }

        public static void CreateWorksheetBase(ExcelWorksheet ws, string title, int colCount)
        {
            // 全体颜色
            ws.Cells.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#3d4d65"));
            {
                // 边框样式
                var border = ws.Cells.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style
                    = ExcelBorderStyle.Thin;

                // 边框颜色
                var clr = ColorTranslator.FromHtml("#B2C6C9");
                border.Bottom.Color.SetColor(clr);
                border.Top.Color.SetColor(clr);
                border.Left.Color.SetColor(clr);
                border.Right.Color.SetColor(clr);
            }

            // 标题
            ws.Cells[1, 1].Value = title;
            using (var range = ws.Cells[1, 1, 1, colCount])
            {
                range.Merge = true;
                range.Style.Font.Bold = true;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            ws.Row(1).Height = 30;
        }
    }
}