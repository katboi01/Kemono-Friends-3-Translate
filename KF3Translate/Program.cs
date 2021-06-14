using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace KF3Translate
{
    class Program
    {
        AssetsManager am;
        string LoadDirectoryPath, SaveDirectoryPath;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Program p = new Program();
            p.ReplaceScenarios();
        }

        private void ReplaceScenarios()
        {
            //MainDirectoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\\SEGA\\けもフレ３\\StreamingAssets\\assets\\";
            LoadDirectoryPath = Path.Combine(Environment.CurrentDirectory, @"Data\Scenarios\");
            SaveDirectoryPath = Path.Combine(Environment.CurrentDirectory, @"Data\Test\");

            am = new AssetsManager();
            am.LoadClassPackage(Path.Combine(Environment.CurrentDirectory, @"Data\classdata.tpk"));

            foreach (string file in Directory.GetFiles(LoadDirectoryPath))
            {
                if(!File.Exists(Path.Combine(SaveDirectoryPath, Path.GetFileNameWithoutExtension(file)))) { continue; }

                Console.WriteLine("Replacing file " + Path.GetFileNameWithoutExtension(file));

                Scenario scenario = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(file));

                //load file
                string selectedFile = Path.Combine(SaveDirectoryPath, Path.GetFileNameWithoutExtension(file));
                BundleFileInstance bundleInst = am.LoadBundleFile(selectedFile, false);

                //Decompress the file to memory (overwrites bundleInst)
                bundleInst.file = Utilities.DecompressToMemory(bundleInst);

                AssetsFileInstance inst = am.LoadAssetsFileFromBundle(bundleInst, 0);
                am.LoadClassDatabaseFromPackage(inst.file.typeTree.unityVersion);
                AssetFileInfoEx inf = inst.table.GetAssetsOfType(114)[0]; //114 = MonoBehaviour

                //Utilities.PrintAssetBundleContent(inst);

                AssetTypeValueField baseField = am.GetTypeInstance(inst.file, inf).GetBaseField();

                //check to see if files match, might remove later for modding
                if( baseField.Get("charaDatas").children[0].childrenCount != scenario.charaDatas.Count
                    || baseField.Get("rowDatas").children[0].childrenCount != scenario.rowDatas.Count)
                {
                    Console.WriteLine("charas in original: " + scenario.charaDatas.Count);
                    Console.WriteLine("charas in replacer: " + baseField.Get("charaDatas").children[0].childrenCount);
                    Console.WriteLine("Lines don't match!");
                    continue;
                }

                for(int i = 0; i < scenario.charaDatas.Count; i++)
                {
                    baseField.Get("charaDatas").children[0].children[i].Get("model").GetValue().Set(scenario.charaDatas[i].model);
                    baseField.Get("charaDatas").children[0].children[i].Get("name").GetValue().Set(scenario.charaDatas[i].name);
                }
                for (int i = 0; i < scenario.miraiDatas.Count; i++)
                {
                    baseField.Get("miraiDatas").children[0].children[i].Get("model").GetValue().Set(scenario.charaDatas[i].model);
                    baseField.Get("miraiDatas").children[0].children[i].Get("name").GetValue().Set(scenario.charaDatas[i].name);
                }
                for (int i = 0; i < scenario.rowDatas.Count; i++)
                {
                    baseField.Get("rowDatas").children[0].children[i].Get("mSerifCharaName").GetValue().Set(scenario.rowDatas[i].mSerifCharaName);
                    for(int j = 0; j<3; j++)
                    {
                        baseField.Get("rowDatas").children[0].children[i].Get("mStrParams").children[0].children[j].GetValue().Set(scenario.rowDatas[i].mStrParams[j]);
                    }
                }

                //commit changes
                byte[] newAssetData;
                byte[] newGoBytes = baseField.WriteToByteArray();
                AssetsReplacerFromMemory repl = new AssetsReplacerFromMemory(0, inf.index, (int)inf.curFileType, 0xFFFF, newGoBytes);
                using (MemoryStream stream = new MemoryStream())
                using (AssetsFileWriter writer = new AssetsFileWriter(stream))
                {
                    inst.file.Write(writer, 0, new List<AssetsReplacer>() { repl }, 0);
                    newAssetData = stream.ToArray();
                }

                BundleReplacerFromMemory bunRepl = new BundleReplacerFromMemory(inst.name, null, true, newAssetData, -1);
                
                //write a modified file (temp)
                using (var stream = File.OpenWrite(selectedFile + "_temp"))
                using (var writer = new AssetsFileWriter(stream))
                {
                    bundleInst.file.Write(writer, new List<BundleReplacer>() { bunRepl });
                }
                bundleInst.file.Close();

                //load the modified file for compression
                bundleInst = am.LoadBundleFile(selectedFile + "_temp");
                using (var stream = File.OpenWrite(selectedFile))
                using (var writer = new AssetsFileWriter(stream))
                {
                    bundleInst.file.Pack(bundleInst.file.reader, writer, AssetBundleCompressionType.LZ4);
                }
                bundleInst.file.Close();

                File.Delete(selectedFile + "_temp");
            }
        }
    }
}
