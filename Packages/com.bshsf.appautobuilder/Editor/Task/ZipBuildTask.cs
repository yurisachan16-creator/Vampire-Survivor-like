using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;
using IOCompressionLevel = System.IO.Compression.CompressionLevel;

namespace zFramework.AppBuilder
{
    [CreateAssetMenu(fileName = "ZipBuildTask", menuName = "Auto Builder/Task/Zip Build Task")]
    public class ZipBuildTask : BaseTask
    {
        public bool includeBaseDirectory = true;
        public IOCompressionLevel compressionLevel = IOCompressionLevel.Optimal;
        public bool overwriteExisting = true;
        public string outputZipPath;

        private void OnEnable()
        {
            taskType = TaskType.PostBuild;
            Description = "将构建产物（文件或目录）打包为 zip，并将 zip 路径传递给后续任务。";
        }

        public override async Task<BuildTaskResult> RunAsync(string output)
        {
            try
            {
                var zipPath = ResolveZipPath(output);
                await Task.Run(() => CreateZip(output, zipPath));
                ReportResult(zipPath, () => $"{nameof(ZipBuildTask)}: ");
                return BuildTaskResult.Successful(zipPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(ZipBuildTask)}: 执行失败 - {e.Message}");
                return BuildTaskResult.Failed(output, e.Message);
            }
        }

        private string ResolveZipPath(string output)
        {
            if (!string.IsNullOrEmpty(outputZipPath))
            {
                var path = Environment.ExpandEnvironmentVariables(outputZipPath);
                if (Path.IsPathRooted(path))
                    return path;

                var root = GetOutputRoot(output);
                if (string.IsNullOrEmpty(root))
                    root = Directory.GetCurrentDirectory();
                return Path.Combine(root, path);
            }

            if (Directory.Exists(output))
            {
                var trimmed = output.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return $"{trimmed}.zip";
            }

            var outputRoot = GetOutputRoot(output);
            if (string.IsNullOrEmpty(outputRoot))
                outputRoot = Directory.GetCurrentDirectory();
            var productName = GetProductNameFromOutput(output);
            if (string.IsNullOrEmpty(productName))
                productName = "Build";
            return Path.Combine(outputRoot, $"{productName}.zip");
        }

        private void CreateZip(string output, string zipPath)
        {
            if (overwriteExisting && File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            if (Directory.Exists(output))
            {
                ZipFile.CreateFromDirectory(output, zipPath, compressionLevel, includeBaseDirectory);
                return;
            }

            if (!File.Exists(output))
            {
                throw new FileNotFoundException($"输出路径不存在: {output}");
            }

            using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using var archive = new ZipArchive(fs, ZipArchiveMode.Create, false);
            archive.CreateEntryFromFile(output, Path.GetFileName(output), compressionLevel);
        }
    }
}
