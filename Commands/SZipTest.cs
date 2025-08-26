using System.Security.Cryptography;
using System.Text;
using Hacknet;
using KT0Mods.Utils;

namespace KT0Mods.KT0Cmd
{
    public class SZipTest
    {
        //TODO: 改为exe
        private const string FILE_HEADER = "SZIP::ENC v2.7------------";
        private static string ret;
        private static string origStr;

        private static string getRandomStr(int length)
        {
            string ret = "";
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                ret += alphabet[random.Next(0, alphabet.Length - 1)];
            }

            return ret;
        }
        
        public static void Trigger(OS os, string[] args)
        {
            //SZip -c fileName or folderName [-p password] for Encrypt
            //Szip -d fileNmae or folderName [-p password] for Decrypt
            Computer c = os.connectedComp ?? os.thisComputer;
            if (!c.PlayerHasAdminPermissions())
            {
                os.write("Permission Denied.");
                os.validCommand = false;
                return;
            }
            
            if (args.Length != 3 && args.Length != 5)
            {
                os.write("x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x");
                os.write("SZip v2.7 usage:");
                os.write("1. SZip -c fileName or folderName [-p password]");
                os.write("Encrypt a file or folder with provided password.");
                os.write("If password not provided, it will use default password to encrypt");
                os.write("x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x");
                os.write("2. SZip -d fileName or folderName [-p password]");
                os.write("Decrypt a file or folder with provided password.");
                os.write("If password not provided, it will use default password to encrypt");
                os.write("x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x.x0x");
                os.validCommand = false;
                return;
            }
            else
            {
                if (args.Length == 3)
                {
                    // -c or -d
                    if (args[1] != "-c" && args[1] != "-d")
                    {
                        os.write("Invalid arguments.");
                        os.validCommand = false;
                        return;
                    }
                    else
                    {
                        if (args[1] == "-c")
                        {
                            string outputFileName = "";
                            Folder src = Programs.getCurrentFolder(os);
                            Folder target = src.searchForFolder(args[2]);
                            if (target == null)
                            {
                                FileEntry fileTarget = src.searchForFile(args[2]);
                                if (fileTarget == null)
                                {
                                    os.write("Argument provided is neither a directory nor a file.");
                                    os.validCommand = false;
                                    return;
                                }
                                else
                                {
                                    origStr = Utils.EncryptUtils.GenerateEncryptFileString(fileTarget);
                                    ret = Utils.EncryptUtils.EncryptMain(origStr);
                                    
                                }
                            }
                            else
                            {
                                
                                origStr = Utils.EncryptUtils.GenerateEncryptString(target);
                                ret = Utils.EncryptUtils.EncryptMain(origStr);
                            }
                            // Generate Default FileName: SHA256(rand())_timestamp_SZIP.szip
                            using (SHA256 sha256 = SHA256.Create())
                            {
                                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(getRandomStr(new Random().Next(8, 16))));
                                string convertedStr = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                string shorter_convertedStr = "";
                                for (int i = 0; i < 16; i++)
                                {
                                    shorter_convertedStr += convertedStr[i];
                                }
                                outputFileName += shorter_convertedStr + "_";
                                
                                long millisecondsTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                outputFileName += millisecondsTimestamp + "_SZIP.szip";
                                
                            }

                            c.makeFile(os.thisComputer.ip, outputFileName, ret, os.navigationPath);

                        }

                        if (args[1] == "-d")
                        {
                            Folder folder = Programs.getCurrentFolder(os);
                            FileEntry file = folder.searchForFile(args[2]);
                            if (file == null)
                            {
                                os.write("File does not exist.");
                                os.validCommand = false;
                                return;
                            }

                            Unzip(os, c, folder, file.data);
                        }
                    }
                    
                }

                if (args.Length == 5)
                {
                    // -c or -d
                    if (args[1] != "-c" && args[1] != "-d")
                    {
                        os.write("Invalid arguments.");
                        os.validCommand = false;
                        return;
                    }
                    else
                    {
                        if (args[1] == "-c")
                        {
                            string outputFileName = "";
                            Folder src = Programs.getCurrentFolder(os);
                            Folder target = src.searchForFolder(args[2]);
                            if (target == null)
                            {
                                FileEntry fileTarget = src.searchForFile(args[2]);
                                if (fileTarget == null)
                                {
                                    os.write("Argument provided is neither a directory nor a file.");
                                    os.validCommand = false;
                                    return;
                                }
                                else
                                {
                                    
                                    origStr = Utils.EncryptUtils.GenerateEncryptFileString(fileTarget);
                                    ret = Utils.EncryptUtils.EncryptMain(origStr, args[4]);
                                    
                                }
                            }
                            else
                            {
                                
                                origStr = Utils.EncryptUtils.GenerateEncryptString(target);
                                ret = Utils.EncryptUtils.EncryptMain(origStr, args[4]);
                            }
                            // Generate Default FileName: SHA256(rand())_timestamp_SZIP.szip
                            using (SHA256 sha256 = SHA256.Create())
                            {
                                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(getRandomStr(new Random().Next(8, 16))));
                                string convertedStr = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                string shorter_convertedStr = "";
                                for (int i = 0; i < 16; i++)
                                {
                                    shorter_convertedStr += convertedStr[i];
                                }
                                outputFileName += shorter_convertedStr + "_";
                                
                                long millisecondsTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                outputFileName += millisecondsTimestamp + "_SZIP.szip";
                                
                            }

                            c.makeFile(os.thisComputer.ip, outputFileName, ret, os.navigationPath);

                        }

                        if (args[1] == "-d")
                        {
                            Folder folder = Programs.getCurrentFolder(os);
                            FileEntry file = folder.searchForFile(args[2]);
                            if (file == null)
                            {
                                os.write("File does not exist.");
                                os.validCommand = false;
                                return;
                            }

                            Unzip(os, c, folder, file.data, args[4]);
                        }
                    }
                }
                
            }
        }

        private static void Unzip(OS os, Computer c, Folder folder, string data, string key = "default_32byte_key_1234567890abc!")
        {
            if (!data.StartsWith(FILE_HEADER))
            {
                os.write("Invalid file header.");
            }
            string decodedstr = EncryptUtils.DecryptMain(data, key);
            if (decodedstr == "Decrypt Error")
            {
                os.write("Password Error.");
                os.validCommand = false;
                return;
            }

            string[] unzip = decodedstr.Split('\n');

            Stack<string> pathStack = new Stack<string>();

            string currentPath = Programs.getCurrentFolder(os).name;

            foreach (string line in unzip)
            {
                Console.WriteLine(currentPath);
                if (line == "//")
                {
                    if (pathStack.Count > 0)
                    {
                        currentPath = pathStack.Pop();
                    }
                }
                else if (line.Contains("//"))
                {
                    string[] parts = line.Split(new[] { "//" }, 2, StringSplitOptions.None);
                    FileEntry file = new()
                    {
                        name = parts[0],
                        data = parts[1]
                    };
                    Folder target = c.getFolderFromPath(currentPath, true);
                    if (target.searchForFile(file.name) != null)
                    {
                        target.searchForFile(file.name).data = file.data;
                    }
                    else
                    {
                        target.files.Add(file);
                    }
                }
                else
                {
                    string newFolder = Path.Combine(currentPath, line);
                    c.makeFolder(os.thisComputer.ip, line, c.getFolderPath(newFolder));
                    pathStack.Push(currentPath);
                    currentPath = newFolder;
                    
                }
            }

        }

        
    }
}