using Hacknet;
using Hacknet.Effects;
using Hacknet.Gui;
using KT0Mods;
using Microsoft.Xna.Framework;
using Pathfinder.Executable;

namespace HacknetPluginTemplate.Executables;

public class Frp : BaseExecutable
{
    private Computer targetComputer;
    
    // 二进制动画相关变量
    private string binary;
    private int binaryIndex = 0;
    private float binaryScrollTimer = 0f;
    private const float SCROLL_RATE = 0.05f; // 更快的滚动速度
    private int binaryCharsPerLine = 0;
    private int binaryLines = 0;
    private float lifetime = 0f;
    private int binaryChars = 0;
    
    private List<ConnectedNodeEffect> nodeeffects = new List<ConnectedNodeEffect>();
    
    // 记录c段ip
    private string IP_C;

    private string addflags;
    
    private MovingBarsEffect bars = new MovingBarsEffect();

    public Frp(Rectangle location, OS os, string[] args) : base(location, os, args)
    {
        ramCost = 140;
        IdentifierName = "FRP Service";
        
        this.nodeeffects.Add(new ConnectedNodeEffect(this.os, true));
        this.nodeeffects.Add(new ConnectedNodeEffect(this.os, true));
        this.nodeeffects.Add(new ConnectedNodeEffect(this.os, true));
        this.nodeeffects.Add(new ConnectedNodeEffect(this.os, true));
        this.nodeeffects.Add(new ConnectedNodeEffect(this.os, true));
        
        this.bars.MinLineChangeTime = 1f;
        this.bars.MaxLineChangeTime = 3f;
    }

    public override void LoadContent()
    {
        base.LoadContent();
        targetComputer = (os.connectedComp != null) ? os.connectedComp : os.thisComputer;
        if (targetComputer == null)
        {
            os.write("[ERROR] Target computer not found");
            needsRemoval = true;
            return;
        }

        if (!targetComputer.PlayerHasAdminPermissions())
        {
            os.write("[ERROR] Permission Denied.");
        }

        if (Args.Length != 3)
        {
            os.write("[ERROR] Invalid arguments");
            os.write("[USAGE] frp -c <internal c class ip>");
            os.write("An example: frp -c 192.168.1.0");
            needsRemoval = true;
            return;
        }

        // 请保证所有的c段ip是一致的
        if (GetIP_C(KT0Startup.InteralPcDictionary[targetComputer.idName].entries[0].ip) != Args[2])
        {
            os.write($"[ERROR] Cannot establish connection to {Args[2]} from {targetComputer.ip}");
            needsRemoval = true;
            return;
        }

        binary = Computer.generateBinaryString(1024);
        binaryCharsPerLine = (bounds.Width - 4) / 8;
        binaryLines = (bounds.Height - 60) / 12;
        binaryChars = binaryCharsPerLine * binaryLines;
        os.write("Creating tunnel...");
        os.write("To terminate this process, use kill pid.");
        IP_C = Args[2];

        addflags = $"FRP_CONNECTION_{IP_C}_ESTABLISHED";
        
        os.Flags.AddFlag(addflags);
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        
        Rectangle drawArea = Hacknet.Utils.InsetRectangle(
            new Rectangle(this.bounds.X, this.bounds.Y + Module.PANEL_HEIGHT, this.bounds.Width,
                this.bounds.Height - Module.PANEL_HEIGHT), 2);
            
        Rectangle rectangle = base.GetContentAreaDest();
        rectangle = Hacknet.Utils.InsetRectangle(rectangle, 1);
        float num = this.os.warningFlashTimer / OS.WARNING_FLASH_TIME;
        float num2 = 2f;
        if (num > 0f)
        {
            num2 += num * ((float)rectangle.Height / 2f - num2);
        }

        Color drawColor = Color.Aqua;
            
        this.bars.Draw(this.spriteBatch, base.GetContentAreaDest(), num2, (drawArea.Width / 14f), 1f, drawColor);

        // 获取当前RAM栏的位置和尺寸
        Rectangle ramRect = bounds;
        ramRect.Width -= 2;
        ramRect.Height -= 2;

        // 绘制二进制滚动背景 - 向下偏移20像素
        Vector2 textPos = new Vector2(ramRect.X + 2, ramRect.Y + 22); // 向下挪20像素
        for (int i = 0; i < binaryLines; i++)
        {
            for (int j = 0; j < binaryCharsPerLine; j++)
            {
                int charIndex = (binaryIndex + j + i * binaryCharsPerLine) % (binary.Length - 1);
                Color charColor = Color.Lerp(Color.Aquamarine, Color.Lime,
                    (float)Math.Sin(lifetime * 2f + i * 0.3f + j * 0.1f) * 0.5f + 0.5f);

                spriteBatch.DrawString(GuiData.UITinyfont,
                    binary[charIndex].ToString(),
                    textPos,
                    charColor);
                textPos.X += 8f;
            }
            textPos.Y += 12f;
            textPos.X = ramRect.X + 2;

            // 如果超出边界则停止绘制
            if (textPos.Y > ramRect.Y + ramRect.Height - 70) break;
        }
        
        this.spriteBatch.Draw(Utils.white, base.GetContentAreaDest(), Color.Black * 0.5f);
        TextItem.doFontLabelToSize(base.GetContentAreaDest(), " Tunnel Established. ", GuiData.titlefont, Color.Lerp(Utils.AddativeRed, this.os.brightLockedColor, 1f), false, false);
        
        
    }
    
    

    

    public override void Update(float t)
    {
        base.Update(t);
        this.bars.Update(t);
        // 更新二进制滚动动画
        lifetime += t;
        binaryScrollTimer += t;
        if (binaryScrollTimer >= SCROLL_RATE)
        {
            binaryIndex = (binaryIndex + 1) % binary.Length;
            binaryScrollTimer = 0f;
        }

        
    }

    private string GetIP_C(string ip)
    {
        string tempip = "";
        string[] tmp = ip.Split('.');
        tempip = tmp[0] + "." + tmp[1] + "." + tmp[2] + ".0";
        return tempip;
    }

    public override void Killed()
    {
        base.Killed();
        os.runCommand("disconnect");
        os.write("Terminating connection...");
        os.Flags.RemoveFlag(addflags);
        os.write("Connection closed.");
        
    }
}