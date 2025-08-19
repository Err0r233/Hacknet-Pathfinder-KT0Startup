


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
                    // TODO: 修改内容为确切的jar文件
                    if (files.name == Args[2] && files.data.ToLower().Contains("blocker"))
                    {
                        fileFlag = true;
                        crackType = crackerJar.jndi;
                        break;
                    }
                    else if (files.name == Args[2] && files.data.ToLower().Contains("shiroblock"))
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

