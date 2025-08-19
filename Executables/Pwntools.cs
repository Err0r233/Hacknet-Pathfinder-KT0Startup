using Hacknet;
using Hacknet.Extensions;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using Microsoft.Xna.Framework.Graphics;

namespace KT0Mods.KT0Exe
{
    public class PwntoolsExe : Pathfinder.Executable.BaseExecutable
    {

        private int AbyssPort;
        
        private float lifetime = 0f;

        private float timer = 0f;

        private float currentStateTimer;

        private float endTimer = 0f;

        private float initStringCharDelay = 0.1f;

        private const float PREMAIN_TIME = 2f;
        private const float MAIN_TIME = 6f;
        private const float ENDING_TIME = 1f;
        private const float IMPORT_TIME = 3f;

        private enum RunningState
        {
            PreMain,
            Main,
            Main2,
            Ending
        }

        private RunningState state;
        

        public const string initString = ">>> Pwntools Initializing <<<";

        public const string mainString = "from ##pwn ##import *###\n" +
                                         "p=process('./Abyss_Process')###\n" +
                                         "context.log_level='debug'###\n" +
                                         "gdb.attach(p)###\n";

        public string[] mainBodyString;

        public const string endString = ">>> Process Abyss Pwned <<<";
        
        
        public PwntoolsExe(Rectangle location, OS os, string[] args) : base(location, os, args)
        {
            ramCost = 350;
            IdentifierName = "Pwntools";
            needsProxyAccess = true;
            name = "Pwntools";
        }

        public override void LoadContent()
        {
            Computer targetComputer = ComputerLookup.FindByIp(targetIP);
            
            if (targetComputer == null)
            {
                os.write("[-] Target not found.");
                needsRemoval = true;
                return;
            }
            
            AbyssPort = targetComputer.GetDisplayPortNumberFromCodePort(9999);

            foreach (var exe in os.exes)
            {
                if (exe is PwntoolsExe)
                {
                    needsRemoval = true;
                    os.terminal.writeLine("[-] Pwntools throw an error: Process is running.");
                    return;
                }
            }

            if (Args.Length != 2)
            {
                os.write("[-] Pwntools throw an error: Illegal Usage.");
                os.write("Usage: Pwntools [Abyss port]");
                needsRemoval = true;
                return;
            }
            else if (Int32.Parse(Args[1]) != AbyssPort)
            {
                os.write("[-] Pwntools throw an error: Illegal Port");
                needsRemoval = true;
                return;
            }

            string extFolder = ExtensionLoader.ActiveExtensionInfo.FolderPath;
            string expPath = Path.Combine(extFolder, "exp.txt");
            if (File.Exists(expPath))
            {
                mainBodyString = File.ReadAllText(expPath).Split(Hacknet.Utils.newlineDelim);
            }
            else
            {
                mainBodyString = new string[] { "exp not found. Exiting..." };
                needsRemoval = true;
                return;
            }

            initStringCharDelay = IMPORT_TIME / mainString.Replace("#", "@@").Length;
            
            targetComputer.hostileActionTaken();
            base.LoadContent();
            os.write("[+] Pwntools is ready to go!");
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");

            Rectangle dest = bounds;
            dest.Inflate(-2, -(PANEL_HEIGHT + 1));
            dest.Y += PANEL_HEIGHT;
            

            switch (state)
            {
                case(RunningState.PreMain):
                    DrawBackgroundInit(dest);
                    DrawInit();
                    break;
                case(RunningState.Main):
                    DrawBackground(dest);
                    DrawMain_Phase1(dest);
                    break;
                case(RunningState.Main2):
                    DrawBackground(dest);
                    DrawMain_Phase2(dest);
                    break;
                case(RunningState.Ending):
                    DrawBackgroundEnd(dest);
                    DrawEnd();
                    break;
            }

        }
        
        private string getDelayDrawString(string original, float timeSec)
        {
            string result = string.Empty;
            float cumulative = 0f;

            foreach (char c in original)
            {
                cumulative += initStringCharDelay;
                if (c == '#') cumulative += initStringCharDelay;
                if (timeSec >= cumulative && c != '#')
                    result += c;
            }

            return result;
        }


        private void DrawMain_Phase1(Rectangle dest)
        {
            var text = getDelayDrawString(mainString, timer-1.8f);
            spriteBatch.DrawString(GuiData.detailfont, text, new Vector2(dest.X + 2, dest.Y + 1), Color.White);
        }

        private void DrawMain_Phase2(Rectangle dest)
        {
            spriteBatch.DrawString(GuiData.detailfont, mainString.Replace("#", string.Empty), new Vector2(dest.X + 2, dest.Y + 1), Color.White * fade);
            // 计算动态速度
            int num3 = 6;
            int num4 = (dest.Height - 30) / num3;
            int totalLines = mainBodyString.Length;
            float scrollDuration = MAIN_TIME; // 3f
            int num = (int)(currentStateTimer / scrollDuration * totalLines);
            if (num > totalLines) num = totalLines;

            int num5 = 0;
            if (num > num4)
            {
                num5 = num - num4;
                if (num5 < 0) num5 = 0;
            }
            Vector2 position = new Vector2(dest.X + 2, dest.Y + 70);
            for (int i = num5; i < num; i++)
            {
                spriteBatch.DrawString(GuiData.detailfont, mainBodyString[i], position, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.3f);
                position.Y += num3;
            }
        }
        private void DrawBackgroundInit(Rectangle dest)
        {
            for (var x = Bounds.X + 4; x < Bounds.X + Bounds.Width - 20; x += 16) for (var y = Bounds.Y + 18; y < Bounds.Y + Bounds.Height - 20; y += 16)
            {
                float size = 0.8f - Math.Abs((timer % 3f) - ((x - Bounds.X + y - Bounds.Y - 22) / 480f + 1)) - 0.1f;
                if (size < 0) continue;
                size = size * size;
                spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(x + 8 - (int)(8 * size), y + 8 - (int)(8 * size), (int)(16 * size), (int)(16 * size)), new Rectangle?(), Utils.DrawUtils.ColorFromHSV((float)(new Random()).NextDouble(), 0.3f, 0.3f), 0, new Vector2(0, 0), SpriteEffects.None, 0f);
            }
        }
        
        private void DrawBackgroundEnd(Rectangle dest)
        {
            for (var x = Bounds.X + 4; x < Bounds.X + Bounds.Width - 20; x += 16) for (var y = Bounds.Y + 18; y < Bounds.Y + Bounds.Height - 20; y += 16)
            {
                float size = 0.8f - Math.Abs((endTimer % 3f) - ((x - Bounds.X + y - Bounds.Y - 22) / 480f + 1)) - 0.1f;
                if (size < 0) continue;
                size = size * size;
                spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(x + 8 - (int)(8 * size), y + 8 - (int)(8 * size), (int)(16 * size), (int)(16 * size)), new Rectangle?(), Utils.DrawUtils.ColorFromHSV((float)(new Random()).NextDouble(), 0.3f, 0.3f), 0, new Vector2(0, 0), SpriteEffects.None, 0f);
            }
        }


        private void DrawBackground(Rectangle dest)
        {
            spriteBatch.Draw(Hacknet.Utils.gradient, dest, Color.Lime * 0.2f);
        }

        private void DrawInit()
        {
            // 绘制绘画区域
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(
                new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width,
                    this.bounds.Height - Module.PANEL_HEIGHT), 2);
            
            // 绘制横幅大小
            int bannerHeight = 30;
            int bannerY = drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight - 10;

            bannerY = Math.Max(bannerY, drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight - 10); // 确保在边界内
            
            // 绘制横幅颜色
            Color color = new Color(0, 0, 0, 200);
            spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(drawArea.X, bannerY, drawArea.Width, bannerHeight), color);
            
            // 绘制文本(居中)
            Vector2 textSize = GuiData.tinyfont.MeasureString(initString);
            Vector2 textPos = new Vector2(drawArea.X + (drawArea.Width - textSize.X) / 2,
                drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight - 10 + (bannerHeight - textSize.Y) / 2);
            
            spriteBatch.DrawString(GuiData.tinyfont, initString, textPos, Color.Lerp(Color.Gray, os.brightLockedColor, Hacknet.Utils.rand(1f)));

        }
        
        private void DrawEnd()
        {
            // 绘制绘画区域
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(
                new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width,
                    this.bounds.Height - Module.PANEL_HEIGHT), 2);
            
            // 绘制横幅大小
            int bannerHeight = 30;
            int bannerY = drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight - 10;

            bannerY = Math.Max(bannerY, drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight - 10); // 确保在边界内
            
            // 绘制横幅颜色
            Color color = new Color(0, 0, 0, 200);
            spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(drawArea.X, bannerY, drawArea.Width, bannerHeight), color);
            
            // 绘制文本(居中)
            Vector2 textSize = GuiData.tinyfont.MeasureString(endString);
            Vector2 textPos = new Vector2(drawArea.X + (drawArea.Width - textSize.X) / 2,
                drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight - 10 + (bannerHeight - textSize.Y) / 2);
            
            spriteBatch.DrawString(GuiData.tinyfont, endString, textPos, Color.Lerp(Color.Gray, os.brightLockedColor, Hacknet.Utils.rand(1f)));

        }
        

        public override void Update(float t)
        {
            Computer comp = Programs.getComputer(os, targetIP);
            lifetime += t;
            timer += t;
            currentStateTimer += t;
            var previous = state;
            UpdateState();
            if (state != previous)
            {
                currentStateTimer = 0f;
            }
            if (state != RunningState.Ending)
            {
                endTimer = 0f;
            }
            else
            {
                endTimer += (t % PREMAIN_TIME + MAIN_TIME + IMPORT_TIME + ENDING_TIME);
            }
            
            if (lifetime >= PREMAIN_TIME + IMPORT_TIME + MAIN_TIME + ENDING_TIME && isExiting == false)
            {
                comp.openPort(AbyssPort, os.thisComputer.ip);
                os.write("[+] Switching to interactive modes...");
                os.write("[+] Target pwned, enjoy it!");
                
                isExiting = true;
            }
            
            base.Update(t);
        }

        private void UpdateState()
        {
            if (timer < PREMAIN_TIME)
            {
                state = RunningState.PreMain;
            }
            else if (timer >= PREMAIN_TIME && timer < PREMAIN_TIME + IMPORT_TIME)
            {
                state = RunningState.Main;
            }
            else if (timer >= PREMAIN_TIME + IMPORT_TIME && timer < PREMAIN_TIME + IMPORT_TIME + MAIN_TIME)
            {
                state = RunningState.Main2;
            }
            else if (timer >= PREMAIN_TIME + IMPORT_TIME + MAIN_TIME && timer < PREMAIN_TIME + IMPORT_TIME + MAIN_TIME + ENDING_TIME)
            {
                state = RunningState.Ending;
            }
        }
    }
}

