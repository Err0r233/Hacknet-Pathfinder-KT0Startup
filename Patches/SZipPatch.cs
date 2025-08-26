using System.Text;
using System.Xml;
using Hacknet;
using HarmonyLib;
using KT0Mods.KT0Exe;
using KT0Mods.Utils;

namespace KT0Mods.Patches
{   
    
    // <SZipFile OutputFolder="bin" ParentFolder="bin" Name="Name.szip" Key="EncryptKey/Default">
    // <SZip OutputFileName="xxx" Subfolder="1" Data="fileData"></SZip>
    // <SZip OutputFileName="yyy" Subfolder="1" Data="fileData"></SZip>
    // <SZip OutputFileName="zzz" Subfolder="1//2" Data="fileData"></SZip>
    // </SZipFile>
    
    // 解释：subfolder默认为Folder下的子目录
    [HarmonyPatch]
    public class SZipPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComputerLoader), "loadComputer")]
        static void Postfix_SZipFiles(ref string filename, ref object __result)
        {
            Computer c = (Computer)__result;
            Stream fileStream = File.OpenRead(filename);
            XmlReader xml = XmlReader.Create(fileStream);
            while(!xml.EOF)
            {
                if (xml.Name == "SZipFile")
                {
                    xml.MoveToAttribute("OutputFolder");
                    string outputFolder = xml.ReadContentAsString();
                    
                    xml.MoveToAttribute("ParentFolder");
                    string parentFolder = xml.ReadContentAsString();

                    xml.MoveToAttribute("Name");
                    string encFileName = xml.ReadContentAsString();

                    xml.MoveToAttribute("Key");
                    string key = xml.ReadContentAsString();
                    if (key == "Default")
                    {
                        key = "default_32byte_key_1234567890abc!";
                    }
                    
                    SZipUtils entries = SZipUtils.GetSZipAttributes(xml);

                    // DEBUG
                    /*foreach (var tmp in entries.entries)
                    {
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine($"outputFileName: {tmp.outputFileName}");
                        Console.WriteLine($"Data: {tmp.Data}");
                        Console.WriteLine($"subFolder: {tmp.subFolder}");
                        Console.WriteLine("-----------------------------------------------------");
                    }*/
                    // DEBUG

                    string origStr = SZipUtils.GenerateOutputString(entries, parentFolder);
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine($"subFolder: {origStr}");
                    Console.WriteLine("-----------------------------------------------------");

                    string encStr = EncryptUtils.EncryptMain(origStr, key);
                    
                    Folder targetFolder1 = c.getFolderFromPath(outputFolder, true);
                    
                    if(targetFolder1.searchForFile(encFileName) != null)
                    {
                        targetFolder1.searchForFile(encFileName).data = encStr;
                    } else
                    {
                        targetFolder1.files.Add(new FileEntry(encStr, encFileName));
                    }

                }
                
                xml.Read();
                if(xml.EOF)
                {
                    return;
                }
            }
            
            
        }
    }

    public class SZipUtils
    {
        // SZIP outputFileName//conent\n
        public string outputFileName;
        public string Data;
        public string ret; // 返回的加密内容
        public string subFolder;
        public const string DELIM = "//";
        public List<SZipUtils> entries = new List<SZipUtils>();

        public SZipUtils(string outputFileName, string Data, string subFolder)
        {
            this.outputFileName = outputFileName;
            this.Data = Data;
            this.subFolder = subFolder;
        }

        public SZipUtils()
        {
            
        }

        public static SZipUtils GetSZipAttributes(XmlReader xml)
        {
            SZipUtils entries = new SZipUtils();
            while (!xml.EOF)
            {
                if (xml.Name == "SZip" && xml.IsStartElement())
                {
                    string o = "";
                    string s = "";
                    string d = "";
                    if (xml.MoveToAttribute("OutputFileName"))
                    {
                        o = xml.ReadContentAsString();
                    }

                    if (xml.MoveToAttribute("Subfolder"))
                    {
                        s = xml.ReadContentAsString();
                    }

                    if (xml.MoveToAttribute("Data"))
                    {
                        d = xml.ReadContentAsString();
                    }
                    
                    entries.entries.Add(new SZipUtils(o,d,s));
                }
                if(xml.Name == "SZipFile" && !xml.IsStartElement())
                {
                    return entries;
                }
                xml.Read();
            }
            throw new FormatException("Unexpected End of File");
        }

        public static string GenerateOutputString(SZipUtils instance, string rootFolder)
        {
            TreeNode root = new TreeNode { Name = rootFolder };

            Dictionary<string, TreeNode> nodeMap = new Dictionary<string, TreeNode>();
            nodeMap[rootFolder] = root;

            foreach (var entry in instance.entries)
            {
                string relativePath = entry.subFolder.Replace(rootFolder + "//", "").Replace("//", "/");
                string[] segments = relativePath.Split('/');

                TreeNode currentNode = root;
                foreach (string segment in segments.Where(s => !string.IsNullOrEmpty(s)))
                {
                    if (!currentNode.Children.TryGetValue(segment, out TreeNode nextNode))
                    {
                        nextNode = new TreeNode { Name = segment };
                        currentNode.Children[segment] = nextNode;
                        nodeMap[$"{currentNode.Name}//{segment}"] = nextNode;
                    }

                    currentNode = nextNode;
                }

                currentNode.Files.Add(entry);
;            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(root.Name);
            TraverseNode(root, sb, skipRoot: true);
            string tmp = sb.ToString().Replace("\r\n", "\n");
            tmp = tmp.Substring(0, tmp.Length - 1);
            return tmp;
        }
        
        private class TreeNode
        {
            public string Name { get; set; }
            public Dictionary<string, TreeNode> Children { get; } = new Dictionary<string, TreeNode>();
            public List<SZipUtils> Files { get; } = new List<SZipUtils>();
        }
        
        private static void TraverseNode(TreeNode node, StringBuilder sb, bool skipRoot = false)
        {
            if (!skipRoot)
            {
                sb.AppendLine(node.Name); // 非根节点写入名称
            }

            // 递归处理子文件夹
            foreach (var child in node.Children.Values.OrderBy(c => c.Name))
            {
                TraverseNode(child, sb);
            }

            // 写入文件
            foreach (var file in node.Files.OrderBy(f => f.outputFileName))
            {
                sb.AppendLine($"{file.outputFileName}{DELIM}{file.Data}");
            }

            // 添加层级结束标记
            sb.AppendLine(DELIM);
        }
        
    }
}

