/****************************************************************************
 * Copyright (c) 2021.3 liangxie
 * 
 * http://qframework.io
 * https://github.com/liangxiegame/QFramework
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 ****************************************************************************/

using UnityEditor;
using UnityEngine;
using System;

namespace QFramework
{
    public class ResKitEditorAPI
    {
        private static readonly BuildTarget[] TrackedBundleTargets =
        {
            BuildTarget.StandaloneWindows64,
            BuildTarget.WebGL,
            BuildTarget.Android
        };

        public static void BuildAssetBundles()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
            BuildScript.BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("QFramework/Toolkits/Res Kit/Build Tracked AssetBundles")]
        public static void BuildTrackedAssetBundles()
        {
            var originalTarget = EditorUserBuildSettings.activeBuildTarget;
            var originalTargetGroup = BuildPipeline.GetBuildTargetGroup(originalTarget);

            try
            {
                for (var i = 0; i < TrackedBundleTargets.Length; i++)
                {
                    var target = TrackedBundleTargets[i];
                    var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
                    if (!BuildPipeline.IsBuildTargetSupported(targetGroup, target))
                    {
                        throw new InvalidOperationException(
                            $"Unity build support for '{target}' is not installed. Install the module before rebuilding tracked AssetBundles.");
                    }

                    EditorUtility.DisplayProgressBar(
                        "Build Tracked AssetBundles",
                        $"Building {AssetBundlePathHelper.GetPlatformForAssetBundles(target)} ({i + 1}/{TrackedBundleTargets.Length})",
                        (i + 1f) / TrackedBundleTargets.Length);

                    if (EditorUserBuildSettings.activeBuildTarget != target &&
                        !EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target))
                    {
                        throw new InvalidOperationException(
                            $"Failed to switch active build target to '{target}'. AssetBundle rebuild was aborted.");
                    }

                    BuildAssetBundles();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(originalTargetGroup, originalTarget);
                }
            }
        }

        public static bool SimulationMode
        {
            get => AssetBundlePathHelper.SimulationMode;
            set => AssetBundlePathHelper.SimulationMode = value;
        }

        public static void ForceClearAssetBundles()
        {
            ResKitAssetsMenu.AssetBundlesOutputPath.DeleteDirIfExists();
            (Application.streamingAssetsPath + "/AssetBundles").DeleteDirIfExists();

            AssetDatabase.Refresh();
        }
    }
}
