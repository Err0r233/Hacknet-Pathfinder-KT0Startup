
using System.Security.Cryptography;
using System.Text;
using Hacknet;
using KT0Mods.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Executable;
using Utils = Hacknet.Utils;


// Warning: this .cs script is deprecated, Do not use it for exe files
// Warning: this .cs script is deprecated, Do not use it for exe files.
// Warning: this .cs script is deprecated, Do not use it for exe files.
namespace SZip;

public class SZipExe : BaseExecutable
{
    private Computer c;
    private const string FILE_HEADER = "SZIP::ENC v2.7------------";
    private static string ret;
    private static string origStr;
    
    public List<Vector2> _points = new List<Vector2>();
    private int _currentIndex;
    private float _drawProgress;
    private Texture2D _pixelTexture;

    private float elapsed;

    public Vector2 StartPosition = new Vector2(100, 100);
    public float Size = 300f;
    public int Depth  = 4;
    public float DrawSpeed  = 0.15f;
    public Color StartColor = Color.Blue;
    public Color EndColor = Color.Red;
    public float LineThickness = 3f;
    
    
    public void Initialize()
    {
        _pixelTexture = new Texture2D(Game1.getSingleton().GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        
        // 预生成希尔伯特曲线路径点
        GenerateHilbertPoints(StartPosition, Size, Depth, 0);
    }

    public void GenerateHilbertPoints(Vector2 start, float size, int depth, int direction)
    {
        if (depth <= 0) return;

        float halfSize = size / 2;
        Vector2[] offsets = 
        {
            new Vector2(-halfSize, halfSize),  // 左上象限偏移
            new Vector2(-halfSize, -halfSize),// 左下象限偏移
            new Vector2(halfSize, -halfSize), // 右下象限偏移
            new Vector2(halfSize, halfSize)   // 右上象限偏移
        };

        // 根据方向确定象限遍历顺序
        int[] order = direction == 0 
            ? new[] { 0, 3, 2, 1 }  // 逆时针连接
            : new[] { 0, 1, 2, 3 }; // 顺时针连接

        // 递归生成子象限路径 [1,6](@ref)
        foreach (int idx in order)
        {
            Vector2 quadrantStart = start + offsets[idx];
            int nextDir = (idx == 0 || idx == 3) ? 1 - direction : direction;
            GenerateHilbertPoints(quadrantStart, halfSize, depth - 1, nextDir);
        }

        // 添加连接线段的端点（关键路径点）[3](@ref)
        if (depth > 1) return;
        _points.Add(start + new Vector2(-size, size)); // 起点
        _points.Add(start + new Vector2(-size, -size));
        _points.Add(start + new Vector2(size, -size));
        _points.Add(start + new Vector2(size, size));   // 终点
    }
    
    public SZipExe(Rectangle location, OS os, string[] args) : base(location, os, args)
    {
        ramCost = 150;
        IdentifierName = "SZip";
    }

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
    
    public override void LoadContent()
    {
        base.LoadContent();
        
        
        Initialize();

        foreach (var v in _points)
        {
            Console.WriteLine($"{v.X}, {v.Y}");
        }
        
        c = (os.connectedComp != null) ? os.connectedComp : os.thisComputer;
        if (c == null)
        {
            os.write("ERROR: Target Computer not found!");
            needsRemoval = true;
            return;
        }

        if (!c.PlayerHasAdminPermissions())
        {
            os.write("Permission Denied.");
            needsRemoval = true;
            return;
        }
        
        if (Args.Length != 3 && Args.Length != 5)
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
            needsRemoval = true;
            return;
        }
        else
        {
            if (Args.Length == 3)
            {
                // -c or -d
                if (Args[1] != "-c" && Args[1] != "-d")
                {
                    os.write("Invalid arguments.");
                    os.validCommand = false;
                    needsRemoval = true;
                    return;
                }
                else
                {
                    if (Args[1] == "-c")
                    {
                        string outputFileName = "";
                        Folder src = Programs.getCurrentFolder(os);
                        Folder target = src.searchForFolder(Args[2]);
                        if (target == null)
                        {
                            FileEntry fileTarget = src.searchForFile(Args[2]);
                            if (fileTarget == null)
                            {
                                os.write("Argument provided is neither a directory nor a file.");
                                os.validCommand = false;
                                needsRemoval = true;
                                return;
                            }
                            else
                            {
                                origStr = EncryptUtils.GenerateEncryptFileString(fileTarget);
                                ret = EncryptUtils.EncryptMain(origStr);

                            }
                        }
                        else
                        {

                            origStr = EncryptUtils.GenerateEncryptString(target);
                            ret = EncryptUtils.EncryptMain(origStr);
                        }

                        // Generate Default FileName: SHA256(rand())_timestamp_SZIP.szip
                        using (SHA256 sha256 = SHA256.Create())
                        {
                            byte[] hashBytes =
                                sha256.ComputeHash(Encoding.UTF8.GetBytes(getRandomStr(new Random().Next(8, 16))));
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

                    if (Args[1] == "-d")
                    {
                        Folder folder = Programs.getCurrentFolder(os);
                        FileEntry file = folder.searchForFile(Args[2]);
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

            if (Args.Length == 5)
            {
                // -c or -d
                if (Args[1] != "-c" && Args[1] != "-d")
                {
                    os.write("Invalid arguments.");
                    os.validCommand = false;
                    needsRemoval = true;
                    return;
                }
                else
                {
                    if (Args[1] == "-c")
                    {
                        string outputFileName = "";
                        Folder src = Programs.getCurrentFolder(os);
                        Folder target = src.searchForFolder(Args[2]);
                        if (target == null)
                        {
                            FileEntry fileTarget = src.searchForFile(Args[2]);
                            if (fileTarget == null)
                            {
                                os.write("Argument provided is neither a directory nor a file.");
                                os.validCommand = false;
                                needsRemoval = true;
                                return;
                            }
                            else
                            {

                                origStr = EncryptUtils.GenerateEncryptFileString(fileTarget);
                                ret = EncryptUtils.EncryptMain(origStr, Args[4]);

                            }
                        }
                        else
                        {

                            origStr = EncryptUtils.GenerateEncryptString(target);
                            ret = EncryptUtils.EncryptMain(origStr, Args[4]);
                        }

                        // Generate Default FileName: SHA256(rand())_timestamp_SZIP.szip
                        using (SHA256 sha256 = SHA256.Create())
                        {
                            byte[] hashBytes =
                                sha256.ComputeHash(Encoding.UTF8.GetBytes(getRandomStr(new Random().Next(8, 16))));
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

                    if (Args[1] == "-d")
                    {
                        Folder folder = Programs.getCurrentFolder(os);
                        FileEntry file = folder.searchForFile(Args[2]);
                        if (file == null)
                        {
                            os.write("File does not exist.");
                            os.validCommand = false;
                            needsRemoval = true;
                            return;
                        }

                        Unzip(os, c, folder, file.data, Args[4]);
                    }
                }
            }
        }
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawOutline();
        drawTarget("app:");

        Rectangle dest = bounds;
        dest.Inflate(-2, -(PANEL_HEIGHT + 1));
        dest.Y += PANEL_HEIGHT;
        
        if (_points.Count == 0 || _currentIndex <= 0) return;

        for (int i = 0; i < _currentIndex; i++)
        {
            Vector2 start = _points[i];
            Vector2 end = _points[i + 1];
            
            // 计算线段颜色渐变（从起点蓝到终点红）
            float change = i / (float)(_points.Count - 1);
            Color color = Color.Lerp(StartColor, EndColor, change);
            
            // 绘制线段 [5](@ref)
            DrawLine(start, end, color, LineThickness);
        }
        
    }
    
    private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 edge = end - start;
        float angle = MathHelper.PiOver2 + (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();
        
        spriteBatch.Draw(
            texture: _pixelTexture,
            position: start,
            sourceRectangle: null,
            color: color,
            rotation: angle,
            origin: Vector2.Zero,
            scale: new Vector2(length, thickness),
            effects: SpriteEffects.None,
            layerDepth: 0
        );
    }

    public override void Update(float t)
    {
        base.Update(t);

        elapsed += t;
        
        if (_currentIndex >= _points.Count - 1) return;
        
        // 基于时间更新绘制进度
        _drawProgress += DrawSpeed * (float)t;
        if (_drawProgress < 1) return;
        
        _currentIndex += (int)_drawProgress;
        _drawProgress -= (int)_drawProgress;
        if (_currentIndex >= _points.Count - 2)
        {
            _currentIndex = _points.Count - 2;
        }
    }

    private static void Unzip(OS os, Computer c, Folder folder, string data,
        string key = "default_32byte_key_1234567890abc!")
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