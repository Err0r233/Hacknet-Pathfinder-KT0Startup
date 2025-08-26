using AutoCreakFirewall;
using BepInEx;
using BepInEx.Hacknet;
using HacknetPluginTemplate.Executables;
using KT0Mods.KT0Cmd;
using KT0Mods.KT0Exe;
using Pathfinder.Command;
using KT0Mods.Tags;
using Pathfinder.Executable;
using Pathfinder.Port;
using RedisSploit;

namespace KT0Mods;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class KT0Startup : HacknetPlugin
{
    public const string ModGUID = "com.KT0.KT0Mods";
    public const string ModName = "KT0_Toolkit";
    public const string ModVer = "1.1.0";

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
        
        
        // Register Commands Below
        WriteLine("[+] Adding Commands...", ConsoleColor.Green);
        CommandManager.RegisterCommand("base64", Base64Encode.Trigger);
        CommandManager.RegisterCommand("SZip", SZipTest.Trigger);
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
        ExecutableManager.RegisterExecutable<AutoCreakFirewallExe>("#FIREWALL_AUTO_SOLVER#"); // Try use this to solve Unbreakable Firewall!
        ExecutableManager.RegisterExecutable<EternalBlue>("#ETERNALBLUE#");
        ExecutableManager.RegisterExecutable<RedisSploitExe>("#REDIS_EXE#");
        
        
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
}
