using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.IO;

namespace KF3Translate
{
    public class Utilities
    {
        public static AssetBundleFile DecompressToMemory(BundleFileInstance bundleInst)
        {
            AssetBundleFile bundle = bundleInst.file;

            MemoryStream bundleStream = new MemoryStream();
            bundle.Unpack(bundle.reader, new AssetsFileWriter(bundleStream));

            bundleStream.Position = 0;

            AssetBundleFile newBundle = new AssetBundleFile();
            newBundle.Read(new AssetsFileReader(bundleStream), false);

            bundle.reader.Close();
            return newBundle;
        }

        public static void PrintAssetBundleContent(AssetsFileInstance inst)
        {
            for (int i = 0; i < inst.table.assetFileInfoCount; i++)
            {
                Console.WriteLine($"File {i}: {AssetHelper.GetAssetNameFastNaive(inst.table.file, inst.table.assetFileInfo[i])} ({inst.table.assetFileInfo[i].curFileType})");
            }
        }
    }
}
