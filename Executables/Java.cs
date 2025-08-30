


using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using System.Text;
using System.Security.Cryptography;
using Hacknet.Effects;
using Hacknet.Extensions;
using Microsoft.Xna.Framework.Graphics;

namespace KT0Mods.KT0Exe
{
    public class Java : Pathfinder.Executable.BaseExecutable
    {
        private int ShiroPort;
        private int JNDIPort;

        private int JNDIRamCost = 100;
        private int ShiroRamCost = 150;
        private int JavaCost = 200;

        private int ArgLength;

        private bool fileFlag = false;

        private MD5 md5 = MD5.Create();
        
        private Texture2D _javaIcon = null;

        private int sliceCount = 1;

        private float animationProgress = 0f;
        
        private MovingBarsEffect bars = new MovingBarsEffect();

        private float readingTime = 2f;
        
        private float crackTime = 6f;

        private float endTime = 1f;

        private float timer = 0f;

        private float currentStateTimer = 0f;

        private float percentComplete;

        private Color brightDrawColor;

        private int realCost;

        private enum crackerJar
        {
            jndi,
            shiro
        }

        private crackerJar crackType;

        public enum jndiState
        {
            Waiting,
            Reading,
            Running,
            Ending
        }

        private jndiState state;

        public Java(Rectangle location, OS os, string[] args) : base(location, os, args)
        {
            ramCost = 200;
            IdentifierName = "Java";
            needsProxyAccess = true;
            name = "Java";
            this.bars.MinLineChangeTime = 1f;
            this.bars.MaxLineChangeTime = 3f;
        }

        public override void LoadContent()
        {
            
            string extFolder = ExtensionLoader.ActiveExtensionInfo.FolderPath;
            
            string expPath = Path.Combine(extFolder, "java.png");

            using (FileStream fileStream = File.OpenRead(expPath))
            {
                _javaIcon = Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, fileStream);
            }
            
            
            Computer targetComputer = ComputerLookup.FindByIp(targetIP);

            if (targetComputer == null && Args.Length == 5)
            {
                os.write("[-] Target not found.");
                needsRemoval = true;
                return;
            }
            
            ShiroPort = targetComputer.GetDisplayPortNumberFromCodePort(8080);
            JNDIPort = targetComputer.GetDisplayPortNumberFromCodePort(389);

            foreach (var exe in os.exes)
            {
                if (exe is Java)
                {
                    needsRemoval = true;
                    os.terminal.writeLine("Runtime Exception: Java.exe is already running");
                    return;
                }
            }
            

            if (Args.Length != 1 && Args.Length != 5)
            {
                os.write("NullPointer exception: Invalid arguments.");
                os.write("Usage1: java");
                os.write("Usage2: java -jar <JarFile> -p <Exploit Port Number>");
                Console.WriteLine("[DEBUG] Bug1");
                needsRemoval = true;
                return;
            }

            ArgLength = Args.Length;

            if (ArgLength == 5)
            {
                if (Args[1] != "-jar")
                {
                    os.write("NullPointer Exception: Invalid arguments.");
                    os.write("Usage1: java");
                    os.write("Usage2: java -jar <JarFile> <Exploit Port Number>");
                    needsRemoval = true;
                    return;
                }
                
                if (!Args[2].EndsWith(".jar"))
                {
                    os.write("NullPointer Exception: Invalid arguments.");
                    os.write("Usage1: java");
                    os.write("Usage2: java -jar <JarFile> <Exploit Port Number>");
                    os.write("JarFile must be end with '.jar'");
                    needsRemoval = true;
                    return;
                }
                
                
                Folder binFolder = os.thisComputer.getFolderFromPath("/bin");
                foreach (var files in binFolder.files)
                {
                    if (files.name == Args[2] && files.data.ToLower().Equals("1101011101000100101100110110001010010010011001111100010101111011010000010001101001100011110101101110000100100000101000010000110011100101000011111011001100010110110000001001111000001111100001101110111100000111111001111100100101101001011100110110111110101100000111101011110010010010111011110011111101001100011110000000100111011010111011110100100010100100111110010110111011101011101100110101110110111101011111100011110011000100111001111100110010001110111000110000111101100000111001001010000001101011010111001001001110001011010110001111100010101111101100101101000010101110011001111101001011110110101000110011111010001111011001100011000110001110111000101001101110100111001010101101111010110001010010100111100011110011001111011000111011110011001101101110110110011100000100110011110010101000100010011010100111011110011010110110011101101011010101101000011000111011011111001011101000111110010110111101101111100110000101101010111001010000101101010011100000100101111010001110100101001000001011011010111001011011011011000101100101100010"))
                    {
                        fileFlag = true;
                        crackType = crackerJar.jndi;
                        break;
                    }
                    else if (files.name == Args[2] && files.data.ToLower().Equals("1110110000001001100101011001101000100000100011110010110011110010010110010010100110110111011011101011000011001111110010001100111001001111010011100110010000100111000000100011100100110100001110001110010001010001110011100110001110110110110100111010000010110100110101111001000110010000000111111000010110100010101000000000011110000001000101111110011111110110001010010000011110111111010011100101101110100000101110011011100000010111100001110110000100111110000100110111000011010000101110111010000100000110101011001001100110101000110101010111001001101001011100110010110000101110101000100010101000000001100100111000101100000010011100000001011101100000111000111101110110000111011010010111001101100000100100001000001110000001000110001101111000000010110000110011011010100100001000110000001011100101110100010000101011100011000000001111001010001010000100101111110110100110110110011000100101101110000011010001011000001100101000000101110110001111011111010010100100101101110111001001110100111001110100111000010100100111100010001001110011101011"))
                    {
                        fileFlag = true;
                        crackType = crackerJar.shiro;
                        crackTime = 8f;
                        break;
                    }
                    
                }
                

                if (!fileFlag)
                {
                    os.write("FileNotFound Exception: File Not Found.");
                    needsRemoval = true;
                    return;
                }

                if ( crackType == crackerJar.jndi && Int32.Parse(Args[4]) != JNDIPort)
                {
                    os.write("Runtime Exception: Target Port Closed.");
                    needsRemoval = true;
                    return;
                }

                if (Int32.Parse(Args[4]) != ShiroPort && crackType == crackerJar.shiro)
                {
                    os.write("Runtime Exception: Target Port Closed.");
                    needsRemoval = true;
                    return;
                }
                
                brightDrawColor = os.unlockedColor; 
                brightDrawColor.A = 0;

                int tmp = 0;
                if (crackType == crackerJar.jndi)
                {
                    tmp = JNDIRamCost;
                }
                else if (crackType == crackerJar.shiro)
                {
                    tmp = ShiroRamCost;
                }
                
                int curCost = JavaCost + tmp;
                
                if (os.ramAvaliable < curCost)
                {
                    os.write("Runtime Exception: Memory Insufficient");
                    needsRemoval = true;
                    return;
                }

                realCost = tmp;

                targetComputer.hostileActionTaken();
            }
            
            base.LoadContent();

        }

        public override void Draw(float t)
        {
            
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            Rectangle rectangle = base.GetContentAreaDest();
            Rectangle dest = bounds;
            dest.Inflate(-2, -(PANEL_HEIGHT + 1));
            dest.Y += PANEL_HEIGHT;

            switch (ArgLength)
            {
                case(1):
                    DrawArg1(dest);
                    break;
                case(5):
                    switch (state)
                    {
                        case(jndiState.Reading):
                            PatternDrawer.draw(bounds, 1.2f, Color.Transparent, this.os.lockedColor, this.spriteBatch, PatternDrawer.binaryTile);
                            Rectangle rectangle1 = new Rectangle(this.bounds.X + 1, this.bounds.Y + 20, 200, 50);
                            spriteBatch.Draw(Hacknet.Utils.white, rectangle1, Color.Black);
                            spriteBatch.DrawString(GuiData.tinyfont, LocaleTerms.Loc("Initializing") + "...", new Vector2((float)(this.bounds.X + 6), (float)(this.bounds.Y + 24)), Color.White);
                            rectangle1.Height = 20;
                            rectangle1.Width = 240;
                            rectangle1.Y += 28;
                            rectangle1 = this.DrawLoadingMessage("Reading payload...", 0f, rectangle1, true, false);
                            break;
                        case(jndiState.Running):
                            PatternDrawer.draw(bounds, 1.2f, Color.Transparent, this.os.lockedColor, this.spriteBatch, PatternDrawer.binaryTile);
                            rectangle = Hacknet.Utils.InsetRectangle(rectangle, 1);
                            float num = this.os.warningFlashTimer / OS.WARNING_FLASH_TIME;
                            float num2 = 2f;
                            if (num > 0f)
                            {
                                num2 += num * ((float)rectangle.Height - num2);
                            }
                            Color drawColor = Color.Lerp(Hacknet.Utils.AddativeWhite * 0.5f, Hacknet.Utils.AddativeRed, num);
                            this.bars.Draw(this.spriteBatch, base.GetContentAreaDest(), num2, 4f, 1f, drawColor);
                            DrawArg5(dest);
                            break;
                        case(jndiState.Ending):
                            PatternDrawer.draw(bounds, 1.2f, Color.Transparent, this.os.lockedColor, this.spriteBatch, PatternDrawer.binaryTile);
                            DrawEnding(dest);
                            break;
                    }
                    break;
            }
        }

        private void DrawArg5(Rectangle dest)
        {
            // 绘制绘画区域
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(
                new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width,
                    this.bounds.Height - Module.PANEL_HEIGHT), 2);
            
            Rectangle rectangle = base.GetContentAreaDest();
            rectangle = Hacknet.Utils.InsetRectangle(rectangle, 1);
            Rectangle bounds = rectangle;
            float num = this.os.warningFlashTimer / OS.WARNING_FLASH_TIME;
            float num2 = 2f;
            if (num > 0f)
            {
                num2 += num * ((float)rectangle.Height - num2);
            }
            
            bounds.Height = (int)((float)bounds.Height * (this.currentStateTimer / crackTime));
            bounds.Y = rectangle.Y + rectangle.Height - bounds.Height + 1;
            bounds.Width += 4;
            this.bars.Draw(this.spriteBatch, bounds, num2, 4f, 1f, this.os.brightLockedColor);
            
        }
        
        private Rectangle DrawLoadingMessage(string message, float startPoint, Rectangle dest, bool showLoading = true, bool highlight = false)
        {
            float num = 0.18f;
            float num2 = (percentComplete - startPoint) / num;
            if (percentComplete > startPoint)
            {
                dest.Y += 22;
                spriteBatch.Draw(Hacknet.Utils.white, dest, Color.Black);
                float point;
                if (percentComplete > startPoint + num)
                {
                    point = 1f;
                }
                else
                {
                    point = num2;
                }
                dest.Width = (int)((float)dest.Width * Hacknet.Utils.QuadraticOutCurve(Hacknet.Utils.QuadraticOutCurve(point)));
                this.spriteBatch.Draw(Hacknet.Utils.white, dest, os.brightLockedColor);
                this.spriteBatch.DrawString(GuiData.tinyfont, message, new Vector2((float)(this.bounds.X + 6), (float)(dest.Y + 2)), highlight ? Color.Black : Color.White);
                if (showLoading)
                {
                    if (this.percentComplete > startPoint + num)
                    {
                        this.spriteBatch.DrawString(GuiData.tinyfont, LocaleTerms.Loc("COMPLETE"), new Vector2((float)(this.bounds.X + 172), (float)(dest.Y + 2)), Color.Black);
                    }
                    else
                    {
                        this.spriteBatch.DrawString(GuiData.tinyfont, (num2 * 100f).ToString("00") + "%", new Vector2((float)(this.bounds.X + 195), (float)(dest.Y + 2)), Color.White);
                    }
                }
            }
            return dest;
        }
        private void DrawArg1(Rectangle dest)
        {
            // 绘制绘画区域
            Rectangle drawArea = Hacknet.Utils.InsetRectangle(
                new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width,
                    this.bounds.Height - Module.PANEL_HEIGHT), 2);
            
            Rectangle rectangle = base.GetContentAreaDest();
            rectangle = Hacknet.Utils.InsetRectangle(rectangle, 1);
            float num = this.os.warningFlashTimer / OS.WARNING_FLASH_TIME;
            float num2 = 2f;
            if (num > 0f)
            {
                num2 += num * ((float)rectangle.Height - num2);
            }
            Color drawColor = Color.Lerp(Hacknet.Utils.AddativeWhite * 0.5f, Hacknet.Utils.AddativeRed, num);
            
            this.bars.Draw(this.spriteBatch, base.GetContentAreaDest(), num2, 4f, 1f, drawColor);
            
            
            
            // 计算当前可见源高度
            int visibleSourceHeight = (int)(_javaIcon.Height * animationProgress);
            int sliceHeight = _javaIcon.Height / sliceCount;
            int remainingHeight = _javaIcon.Height % sliceCount; // 余数高度
            

            int drawnHeight = 0; // 已绘制的源高度
            for (int i = 0; i < sliceCount; i++)
            {
                // 计算当前切片源高度（最后一片加上余数）
                int currentSliceHeight = (i == sliceCount - 1) ? 
                    sliceHeight + remainingHeight : sliceHeight;
            
                // 若已超过可见高度则终止
                if (drawnHeight >= visibleSourceHeight) break;
            
                // 计算实际绘制的切片高度（可能部分可见）
                int drawHeight = currentSliceHeight;
                if (drawnHeight + currentSliceHeight > visibleSourceHeight)
                {
                    drawHeight = visibleSourceHeight - drawnHeight;
                }

                // 源矩形（从纹理顶部开始）
                Rectangle sourceRect = new Rectangle(
                    0, 
                    drawnHeight, // Y坐标从已绘制高度开始
                    _javaIcon.Width, 
                    drawHeight
                );
            
                // 目标矩形（严格比例映射）
                float heightRatio = (float)drawHeight / _javaIcon.Height;
                int destHeight = (int)(drawArea.Height * heightRatio);
                int destY = drawArea.Y + (int)(drawArea.Height * ((float)drawnHeight / _javaIcon.Height));
            
                Rectangle destRect = new Rectangle(
                    drawArea.X,
                    destY,
                    drawArea.Width,
                    destHeight
                );

                spriteBatch.Draw(_javaIcon, destRect, sourceRect, Color.White);
                drawnHeight += drawHeight; // 更新已绘制高度
            }
            
        }
        
        private void DrawEnding(Rectangle dest)
        {
            float num = this.currentStateTimer;
            this.currentStateTimer = 5f;
            this.currentStateTimer = num;
            Rectangle destinationRectangle = new Rectangle(dest.X, dest.Y + dest.Height / 3, dest.Width, dest.Height / 3);
            this.spriteBatch.Draw(Hacknet.Utils.white, destinationRectangle, this.os.unlockedColor * 0.8f);
            destinationRectangle.Height -= 6;
            destinationRectangle.Y += 3;
            this.spriteBatch.Draw(Hacknet.Utils.white, destinationRectangle, this.os.indentBackgroundColor * 0.8f);
            string text = "SUCCESS";
            Vector2 vector = GuiData.font.MeasureString(text);
            Vector2 vector2 = new Vector2((float)(destinationRectangle.X + destinationRectangle.Width / 2) - vector.X / 2f, (float)(destinationRectangle.Y + destinationRectangle.Height / 2) - vector.Y / 2f);
            this.spriteBatch.DrawString(GuiData.font, text, vector2 - Vector2.One, this.brightDrawColor * this.fade);
            this.spriteBatch.DrawString(GuiData.font, text, vector2 + Vector2.One, this.brightDrawColor * this.fade);
            this.spriteBatch.DrawString(GuiData.font, text, vector2, Color.White * this.fade);
        }

        public override void Update(float t)
        {
            Computer comp = Programs.getComputer(os, targetIP);
            base.Update(t);
            this.bars.Update(t);
            animationProgress += (float)t / 3f; // 3s完成动画
            animationProgress = MathHelper.Clamp(animationProgress, 0, 3);
            
            // 检测timer
            if (ArgLength != 5)
            {
                timer = 0;
            }
            else if(ArgLength == 5 && fileFlag)
            {
                timer += t;
                currentStateTimer += t;
            }
            
            var previous = state;
            UpdateState();
            if (state != previous)
            {
                currentStateTimer = 0f;
            }
            
            percentComplete = currentStateTimer / (readingTime + 7.8f);
 
            float addDuration = crackTime;
            float elapsed = timer - readingTime;

            if (elapsed < 0f)
            {
                elapsed = 0f;
            }
            
            if (state == jndiState.Running)
            {
                if (elapsed < addDuration)
                {
                    float tNorm = elapsed / addDuration;
                    ramCost = JavaCost + (int)(realCost * tNorm);
                }
            }

            bounds.Height = ramCost;
            
            // 破解端口逻辑
            if (timer >= crackTime + readingTime + endTime && isExiting == false)
            {
                if (crackType == crackerJar.jndi)
                {
                    comp.openPort(JNDIPort, os.thisComputer.ip);
                    os.write("> JNDIMap Finished.");
                }
                else if (crackType == crackerJar.shiro)
                {
                    comp.openPort(ShiroPort, os.thisComputer.ip);
                    os.write("> ShiroAttack Finished.");
                }
                isExiting = true;
            }
            
            base.Update(t);
            
        }

        private void UpdateState()
        {
            if (timer == 0)
            {
                state = jndiState.Waiting;
            }
            if (timer > 0 && timer < readingTime)
            {
                state = jndiState.Reading;
            }
            else if (timer >= readingTime && timer < readingTime + crackTime)
            {
                state = jndiState.Running;
            }
            else if (timer >= readingTime + crackTime && timer <= readingTime + crackTime + endTime)
            {
                state = jndiState.Ending;
            }
        }
        
        
    }
}

