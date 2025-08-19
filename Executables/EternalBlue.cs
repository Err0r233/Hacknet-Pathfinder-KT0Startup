using Hacknet;
using Hacknet.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;

namespace HacknetPluginTemplate.Executables;


public class EternalBlue : Pathfinder.Executable.BaseExecutable
{
    private float crackTime = 20f;
    private float timer;

    private float elpased;
    
    private int smbPort;

    private float X;
    private float tmp = 1f;
    private float temp;
    
    private RaindropsEffect RainEffect = new RaindropsEffect();

    
    private RaindropsEffect BackgroundRainEffect = new RaindropsEffect();

    private string statusStr = "Preparing Payloads...";
    

    private Random random = new Random();
    
    public EternalBlue(Rectangle location, OS os, string[] args) : base(location, os, args)
    {
        ramCost = 380;
        IdentifierName = "EternalBlue";
        needsProxyAccess = true;
        name = "EternalBlue";
        
        BackgroundRainEffect.Init(this.os.content);
        BackgroundRainEffect.MaxVerticalLandingVariane += 0.05f;
        BackgroundRainEffect.FallRate = 0.8f;
        
        RainEffect.Init(this.os.content);
        RainEffect.ForceSpawnDrop(new Vector3(0.5f, 0f, 0f));
        
        

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
        
        smbPort = targetComputer.GetDisplayPortNumberFromCodePort(445);
        
        foreach (var exe in os.exes)
        {
            if (exe is EternalBlue)
            {
                needsRemoval = true;
                os.terminal.writeLine("EternalBlue is Running.");
                return;
            }
        }

        if (Args.Length != 2)
        {
            os.write("Usage: EternalBlue [smb port]");
            needsRemoval = true;
            return;
        }
        else if (Int32.Parse(Args[1]) != smbPort)
        {
            os.write("Invalid Port.");
            needsRemoval = true;
            return;
        }
        Rectangle drawArea = Hacknet.Utils.InsetRectangle(
            new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width,
                this.bounds.Height - Module.PANEL_HEIGHT), 2);
        Vector2 textSize = GuiData.tinyfont.MeasureString(statusStr);
        X = drawArea.X + (drawArea.Width - textSize.X) / 2;
        temp = X;
        
        targetComputer.hostileActionTaken();
        base.LoadContent();
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawOutline();
        drawTarget("app:");
        
        Rectangle dest = bounds;
        dest.Inflate(-2, -(PANEL_HEIGHT + 1));
        dest.Y += PANEL_HEIGHT;

        Color color = Color.Blue;
        RainEffect.Render(base.GetContentAreaDest(), spriteBatch, color, 50f, 100f);
        
        PatternDrawer.draw(bounds, 1.2f, Color.Transparent, this.os.lockedColor, this.spriteBatch, PatternDrawer.binaryTile);
        
        // TODO: 下方的滚动横幅
        // 绘制文本(居中)

        if (timer <= 20f)
        {
            DrawBanner();
        }
        
        
        
        base.Draw(t);
    }


    private void DrawBanner()
    {
        // 绘制绘画区域
        Rectangle drawArea = Hacknet.Utils.InsetRectangle(
            new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width,
                this.bounds.Height - Module.PANEL_HEIGHT), 2);
            
        // 绘制横幅大小
        int bannerHeight = 30;
        int bannerY = drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight - 10;

        bannerY = Math.Max(bannerY, drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight + 180); // 确保在边界内
            
        // 绘制横幅颜色
        Color color = new Color(0, 0, 0, 200);
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(drawArea.X, bannerY, drawArea.Width, bannerHeight), color);
        
        // 绘制文本(居中)
        Vector2 textSize = GuiData.tinyfont.MeasureString(statusStr);

        //X = drawArea.X + (drawArea.Width - textSize.X) / 2 + 20 * elpased * tmp;
        
        Console.WriteLine(temp);
        
        if (X > drawArea.Width - textSize.X)
        {
            UpdateElpased();
            tmp = -1f;
            temp = X - 2f;
            
        }
        else if (X < drawArea.X)
        {
            UpdateElpased();
            tmp = 1f;
            temp = X + 2f;

        }
        
        X = temp + 40 * elpased * tmp;
        
        
        Vector2 textPos = new Vector2(X,
            drawArea.Y + (int)(drawArea.Height / 2.0f) - bannerHeight - 10 + (bannerHeight - textSize.Y) / 2 + 190);
            
        spriteBatch.DrawString(GuiData.tinyfont, statusStr, textPos, Color.Lerp(Color.Gray, os.brightLockedColor, Hacknet.Utils.rand(1f)));
    }


    private void UpdateElpased()
    {
        elpased = 0f;
    }
   

    public override void Update(float t)
    {
        base.Update(t);
        timer += t;
        elpased += t;
        Computer comp = Programs.getComputer(os, targetIP);

        float num = timer * 3f;
        
        this.RainEffect.Update(t, num);
        this.BackgroundRainEffect.Update(t, num * 3f);
        
        // 20s 1-4s 准备payload 4-8s Connecting
        if (timer > 4f &&timer <= 8f)
        {
            statusStr = "Connecting...";
        }
        else if (timer > 8f && timer <= 12)
        {
            statusStr = "Sending buffers...";
        }
        else if (timer >= 12f && timer <= 19f)
        {
            statusStr = "Buffer overwriting...";
        }
        else if (timer >= 19f)
        {
            statusStr = "SUCCESS! Exiting...";
        }
        
        
        if (timer >= crackTime && isExiting == false)
        {
            comp.openPort(smbPort, os.thisComputer.ip);
            os.write("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            os.write("=-=-=-=-=-=-=-=-=-=-=-=-=-WIN-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            os.write("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                
            isExiting = true;
        }
    }
}