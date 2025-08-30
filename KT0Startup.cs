using System.Xml.Linq;
using AutoCrackFirewall;
using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using HacknetPluginTemplate.Executables;
using KT0Mods.Daemons;
using KT0Mods.KT0Cmd;
using KT0Mods.KT0Exe;
using KT0Mods.Patches;
using Pathfinder.Command;
using KT0Mods.Tags;
using Pathfinder.Action;
using Pathfinder.Daemon;
using Pathfinder.Event;
using Pathfinder.Event.Loading;
using Pathfinder.Event.Saving;
using Pathfinder.Executable;
using Pathfinder.Port;
using Pathfinder.Util.XML;
using RedisSploit;
using SZip;

namespace KT0Mods;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class KT0Startup : HacknetPlugin
{
    public const string ModGUID = "com.KT0.KT0Mods";
    public const string ModName = "KT0_Toolkit";
    public const string ModVer = "1.2.1";

    public static Dictionary<string, InternalIpUtils> InteralPcDictionary = new Dictionary<string, InternalIpUtils>();

    public override bool Load()
    {
        // Load Art text
        WriteLine("    __ __                   ______           _                ____", ConsoleColor.Cyan);
        WriteLine("   / //_/__  ___  ____     /_  __/______  __(_)___  ____ _   / __ \\", ConsoleColor.Cyan);
        WriteLine("  / ,< / _ \\/ _ \\/ __ \\     / / / ___/ / / / / __ \\/ __ `/  / / / /", ConsoleColor.Cyan);
        WriteLine(" / /| /  __/  __/ /_/ /    / / / /  / /_/ / / / / / /_/ /  / /_/ /", ConsoleColor.Cyan);
        WriteLine("/_/ |_\\___/\\___/ .___/    /_/ /_/   \\__, /_/_/ /_/\\__, /   \\____/", ConsoleColor.Cyan);
        WriteLine("              /_/                  /____/        /____/", ConsoleColor.Cyan);
        
        // Load Special Thanks
        WriteLine("[+] Original Code's Inspiration: ", ConsoleColor.Yellow);
        WriteLine("[+] ZeroDayToolKit", ConsoleColor.Yellow);
        WriteLine("[+] StuxnetHN", ConsoleColor.Yellow);
        WriteLine("[+] TempestGadgets", ConsoleColor.Yellow);
        
        // Load Seperator
        WriteSeperator(ConsoleColor.Magenta);
        
        // Register RAM(Tags) and Patches Below
        WriteLine("[+] Adding Tags...", ConsoleColor.Green);
        Pathfinder.Action.ActionManager.RegisterAction<RAMSetter>("SetRAM");
        WriteSeperator(ConsoleColor.Magenta);
        
        // Patches
        WriteLine("[+] Patching...", ConsoleColor.Green);
        HarmonyInstance.PatchAll(typeof(KT0Startup).Assembly);
        WriteSeperator(ConsoleColor.Magenta);
        
        WriteLine("[+] Loading Daemons...", ConsoleColor.Green);
        DaemonManager.RegisterDaemon<InternalServiceDaemon>();
        WriteSeperator(ConsoleColor.Magenta);
        
        // Register Commands Below
        WriteLine("[+] Adding Commands...", ConsoleColor.Green);
        CommandManager.RegisterCommand("base64", Base64Encode.Trigger);
        CommandManager.RegisterCommand("SZip", SZipTest.Trigger);
        CommandManager.RegisterCommand("IScan", IScan.Trigger);
        WriteSeperator(ConsoleColor.Magenta);
        
        // Register Ports Below
        WriteLine("[+] Adding Ports...", ConsoleColor.Green);
        PortManager.RegisterPort("shiro", "Shiro Services", 8080); // java -jar ShiroAttack.jar
        PortManager.RegisterPort("pwn", "Abyss", 9999); // Pwntools.exe
        PortManager.RegisterPort("smb", "Server Message Block", 445); // EternalBlue.exe
        PortManager.RegisterPort("redis", "Redis", 6379); //RedisExploit.exe
        PortManager.RegisterPort("jndi","LDAP Service", 389); // java -jar JNDIMap.jar 
        WriteSeperator(ConsoleColor.Magenta);
        
        // Register Executables Below
        WriteLine("[+] Adding Executables...", ConsoleColor.Green);
        ExecutableManager.RegisterExecutable<PwntoolsExe>("#PWN_EXE#");
        ExecutableManager.RegisterExecutable<Java>("#JAVA_EXE#");
        ExecutableManager.RegisterExecutable<AutoCrackFirewallExe>("#FIREWALL_AUTO_SOLVER#"); // Try use this to solve Unbreakable Firewall!
        ExecutableManager.RegisterExecutable<EternalBlue>("#ETERNALBLUE#");
        ExecutableManager.RegisterExecutable<RedisSploitExe>("#REDIS_EXE#");
        ExecutableManager.RegisterExecutable<Frp>("#FRP_EXE#");
        WriteSeperator(ConsoleColor.Magenta);
        
        WriteLine("[+] Adding Events...", ConsoleColor.Green);
        Action<SaveComputerEvent> IlinkSaveDelegate = SaveIScan;
        Action<SaveComputerLoadedEvent> IlinkLoadDelegate = LoadIScan;
        
        EventManager<SaveComputerEvent>.AddHandler(IlinkSaveDelegate);
        EventManager<SaveComputerLoadedEvent>.AddHandler(IlinkLoadDelegate);
        WriteSeperator(ConsoleColor.Magenta);
        
        WriteLine("[!] Done! Enjoy!", ConsoleColor.Green);
        
        return true;
    }

    public void WriteLine(string str, ConsoleColor color)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(str);
        Console.ForegroundColor = originalColor; // 恢复原颜色
    }

    public void WriteSeperator(ConsoleColor color)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine("------------------------------------");
        Console.ForegroundColor = originalColor; // 恢复原颜色
    }

    public void SaveIScan(SaveComputerEvent saveComp)
    {
        Computer c = saveComp.Comp;
        Log.LogDebug($"Saving InternalIP data on node {c.idName}");
        
        XElement InternalLinkElement = new XElement("InternalLink");
        
        if (InteralPcDictionary.ContainsKey(c.idName))
        {
            InternalIpUtils e = InteralPcDictionary[c.idName];
            XElement compElement = saveComp.Element;
            foreach (var entry in e.entries)
            {
                XElement InternalPCElement = new XElement("InternalPC");
                XAttribute iid = new XAttribute("id", entry.ComputerId);
                XAttribute iip = new XAttribute("internalIp", entry.ip);
                
                InternalPCElement.Add(iid, iip);
                InternalLinkElement.Add(InternalPCElement);
            }
            compElement.FirstNode.AddAfterSelf(InternalLinkElement);
        }
    }

    public void LoadIScan(SaveComputerLoadedEvent saveComp)
    {
        Computer comp = saveComp.Comp;
        ElementInfo xCompElement = saveComp.Info;

        if (xCompElement.Children.FirstOrDefault(e => e.Name == "InternalLink") != null)
        {
            ElementInfo InternalLinkElement = xCompElement.Children.First(e => e.Name == "InternalLink");
            InternalIpUtils entries = new InternalIpUtils();

            for (var i = 0; i < InternalLinkElement.Children.Count; i++)
            {
                ElementInfo e = InternalLinkElement.Children[i];

                string cpid = e.Attributes["id"];
                string iip = e.Attributes["internalIp"];
                InternalIpUtils internalIpUtils = new InternalIpUtils(iip, cpid);
                entries.entries.Add(internalIpUtils);
            }
            
            InteralPcDictionary.Add(comp.idName, entries);
        }
    }
}
