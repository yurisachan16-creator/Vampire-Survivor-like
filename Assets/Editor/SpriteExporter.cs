using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExporter
{
    [MenuItem("Assets/Export Sliced Sprites")]
    static void ExportSprites()
    {
        // 获取当前选中的图片
        Texture2D sourceTex = Selection.activeObject as Texture2D;
        if (sourceTex == null) 
        {
            Debug.LogError("请先在 Project 窗口选中一张图片！");
            return;
        }

        string path = AssetDatabase.GetAssetPath(sourceTex);
        
        // 1. 确保图片开启了 Read/Write Enabled (否则无法读取像素)
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        // 2. 创建导出文件夹
        string dirPath = Path.GetDirectoryName(path) + "/" + sourceTex.name + "_Slices";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        // 3. 【核心修改】直接加载该路径下的所有资源（包含切好的 Sprite）
        // 这样就避开了已废弃的 spritesheet API
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);

        int count = 0;
        foreach (Object asset in allAssets)
        {
            // 只处理 Sprite 类型的子对象
            if (asset is Sprite sprite)
            {
                // 创建临时纹理用于保存
                // 注意：这里我们创建一个新的 Texture2D，大小等于切片的大小
                Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                
                // 从原图中复制像素
                // sprite.rect 记录了该切片在原图中的位置坐标和宽高
                Color[] pixels = sourceTex.GetPixels(
                    (int)sprite.rect.x, 
                    (int)sprite.rect.y, 
                    (int)sprite.rect.width, 
                    (int)sprite.rect.height
                );
                
                newTex.SetPixels(pixels);
                newTex.Apply();

                // 编码为 PNG 并写入文件
                byte[] bytes = newTex.EncodeToPNG();
                File.WriteAllBytes(dirPath + "/" + sprite.name + ".png", bytes);
                count++;
            }
        }

        Debug.Log($"导出成功！共导出 {count} 张小图。路径: {dirPath}");
        // 刷新资源窗口，让你能立刻看到新文件夹
        AssetDatabase.Refresh();
    }
}