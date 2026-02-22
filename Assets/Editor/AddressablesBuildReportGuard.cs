using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace VampireSurvivorLike.Editor
{
    internal sealed class AddressablesBuildReportGuard : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private static readonly PropertyInfo AutoOpenAddressablesReportProperty =
            typeof(ProjectConfigData).GetProperty("AutoOpenAddressablesReport", BindingFlags.NonPublic | BindingFlags.Static);

        private static bool s_HasPreviousAutoOpenState;
        private static bool s_PreviousAutoOpenState;

        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            RemoveMissingBuildReportRecords();
            CacheAndDisableAutoOpenAddressablesReport();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            RestoreAutoOpenAddressablesReport();
        }

        private static void RemoveMissingBuildReportRecords()
        {
            var reportPaths = ProjectConfigData.BuildReportFilePaths;
            for (var i = reportPaths.Count - 1; i >= 0; i--)
            {
                var path = reportPaths[i];
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    ProjectConfigData.RemoveBuildReportFilePathAtIndex(i);
                }
            }
        }

        private static void CacheAndDisableAutoOpenAddressablesReport()
        {
            if (AutoOpenAddressablesReportProperty == null || !AutoOpenAddressablesReportProperty.CanRead)
            {
                return;
            }

            var currentValue = (bool)AutoOpenAddressablesReportProperty.GetValue(null);
            s_HasPreviousAutoOpenState = true;
            s_PreviousAutoOpenState = currentValue;

            if (AutoOpenAddressablesReportProperty.CanWrite && currentValue)
            {
                AutoOpenAddressablesReportProperty.SetValue(null, false);
            }
        }

        private static void RestoreAutoOpenAddressablesReport()
        {
            if (!s_HasPreviousAutoOpenState)
            {
                return;
            }

            s_HasPreviousAutoOpenState = false;

            if (AutoOpenAddressablesReportProperty == null || !AutoOpenAddressablesReportProperty.CanWrite)
            {
                return;
            }

            AutoOpenAddressablesReportProperty.SetValue(null, s_PreviousAutoOpenState);
        }
    }
}
