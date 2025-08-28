using Hacknet;
using KT0Mods.Patches;
using Pathfinder.Util;

namespace KT0Mods.KT0Cmd;

public class IScan
{
    public static void Trigger(OS os, string[] args)
    {
        Computer c = os.connectedComp ?? os.thisComputer;
        if (!c.PlayerHasAdminPermissions())
        {
            os.write("Permission Denied.");
            os.validCommand = false;
            return;
        }

        if (args.Length != 1)
        {
            os.write("Too many arguments.");
            os.write("Usage: Iscan");
            os.validCommand = false;
            return;
        }

        InternalIpUtils entries = GetEntries(c);
        
        foreach (var e in entries.entries)
        {
            Console.WriteLine(e.ComputerId);
        }

        if (entries.entries == null)
        {
            os.write("IScan completed, found 0 computer.");
            os.validCommand = true;
            return;
        }

        foreach (var e in entries.entries)
        {
            Computer cp = ComputerLookup.FindById(e.ComputerId);
            
            os.netMap.discoverNode(cp);
            
            
        }
        
        


    }

    private static InternalIpUtils GetEntries(Computer c)
    {
        return KT0Startup.InteralPcDictionary[c.idName];
    }
}