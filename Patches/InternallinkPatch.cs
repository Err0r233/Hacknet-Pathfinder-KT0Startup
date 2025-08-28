using System.Xml;
using Hacknet;
using HarmonyLib;

namespace KT0Mods.Patches;

// <InternalLink>
// <InternalPC id="PCID" internalIp="10.1.1.1"></InternalPC>
// <InternalPC id="PCID" internalIp="10.1.1.2"></InternalPC>
// </InternalLink>
[HarmonyPatch]
public class InternallinkPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ComputerLoader), "loadComputer")]
    static void Postfix_InternalPC(ref string filename, ref object __result)
    {
        Computer c = (Computer)__result;
        Stream fileStream = File.OpenRead(filename);
        XmlReader xml = XmlReader.Create(fileStream);

        while (!xml.EOF)
        {
            if (xml.Name == "InternalLink")
            {
                InternalIpUtils internalIpUtils = InternalIpUtils.getEntries(xml);
                
                KT0Startup.InteralPcDictionary.Add(c.idName, internalIpUtils);
            }
            
            xml.Read();
            if (xml.EOF)
            {
                return;
            }
        }
        
        
    }
}

public class InternalIpUtils
{
    public List<InternalIpUtils> entries = new List<InternalIpUtils>();
    public string ip;
    public string ComputerId;

    public InternalIpUtils()
    {
        
    }

    public InternalIpUtils(string ip, string computerId)
    {
        this.ip = ip;
        this.ComputerId = computerId;
    }
    public static InternalIpUtils getEntries(XmlReader xml)
    {
        InternalIpUtils entries = new InternalIpUtils();
        while (!xml.EOF)
        {
            if (xml.Name == "InternalPC" && xml.IsStartElement())
            {
                string i = "";
                string cpid = "";
                if (xml.MoveToAttribute("internalIp"))
                {
                    i = xml.ReadContentAsString();
                }

                if (xml.MoveToAttribute("id"))
                {
                    cpid = xml.ReadContentAsString();
                }
                
                entries.entries.Add(new InternalIpUtils(i, cpid));
            }

            if (xml.Name == "InternalLink" && !xml.IsStartElement())
            {
                return entries;
            }
            
            xml.Read();
            
        }

        throw new FormatException("InternalPC format exception");
    }
}