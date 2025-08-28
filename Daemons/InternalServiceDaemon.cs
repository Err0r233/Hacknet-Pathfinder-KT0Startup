using System.Text.RegularExpressions;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Daemon;
using Pathfinder.Util;

namespace KT0Mods.Daemons;

public class InternalServiceDaemon : BaseDaemon
{
    private string IP_C;
    public InternalServiceDaemon(Computer c, string serviceName, OS os):base(c, serviceName, os){}

    public override void navigatedTo()
    {
        base.navigatedTo();
        string tempip = this.comp.ip;
        string[] tmp = tempip.Split('.');
        IP_C = tmp[0] + "." + tmp[1] + "." + tmp[2] + ".0";
        if (!os.Flags.HasFlag($"FRP_CONNECTION_{IP_C}_ESTABLISHED"))
        {
            DisconnectTarget();
        }
    }

    private void DisconnectTarget()
    {
        os.execute("disconnect");
        os.display.command = "disconnect";
        os.delayer.Post(ActionDelayer.NextTick(), delegate
        {
            os.display.command = "disconnect";
        });
        os.write(" ");
        os.write(" ");
        os.write("------------------------------");
        os.write("------------------------------");
        os.write(" ");
        os.write("---  " + LocaleTerms.Loc("CONNECTION ERROR") + "  ---");
        os.write(" ");
        os.write(LocaleTerms.Loc("Message from Server:"));
        os.write(string.Format(LocaleTerms.Loc("You are connecting to an internal IP {0}"), comp.ip));
        os.write("---  " + LocaleTerms.Loc($"Connection cannot be established between {os.thisComputer.ip} and {comp.ip}") + "  ---");
        os.write(" ");
        os.write("------------------------------");
        os.write("------------------------------");
        os.write(" ");
    }

    public override void draw(Rectangle bounds, SpriteBatch sb)
    {
        base.draw(bounds, sb);
        
        Rectangle dest = Hacknet.Utils.InsetRectangle(bounds, 2);

        Color color = os.highlightColor;
        
        PatternDrawer.draw(dest, 1f, Color.Black * 0.1f, color * 0.4f, sb, PatternDrawer.thinStripe);
        
        Rectangle dest2 = new Rectangle(dest.X + 10, dest.Y + dest.Height / 4, dest.Width - 20, 40);
        if (Button.doButton(8711133, dest2.X, dest2.Y - 30, dest.Width / 4, 24, LocaleTerms.Loc("Proceed"), new Color?(this.os.highlightColor)))
        {
            this.os.display.command = "connect";
        }
        string text = LocaleTerms.Loc("Connection established.");
        sb.Draw(Hacknet.Utils.white, new Rectangle(bounds.X + 1, dest2.Y - 3, bounds.Width - 2, dest2.Height + 6), Color.Black * 0.7f);
        TextItem.doFontLabelToSize(dest2, text, GuiData.font, color, true, true);
        Rectangle dest3 = new Rectangle(dest2.X, dest2.Y + dest2.Height + 6, dest2.Width, dest.Height / 2);
        string text2 = LocaleTerms.Loc("Internal connection established");
        
        TextItem.doFontLabelToSize(dest3, text2, GuiData.smallfont, Color.White * 0.8f, true, true);
        
    }
    
}