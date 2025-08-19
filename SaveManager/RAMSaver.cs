

using System.Xml.Linq;
using Pathfinder.Event.Saving;
using Pathfinder.Meta.Load;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;

namespace KT0Mods.SaveManager
{
    [SaveExecutor("HacknetSave.RAMAmount")]
    public class RAMSaver : SaveLoader.SaveExecutor
    {
        [Event]
        public static void Save(SaveEvent e)
        {
            var elelemnt = new XElement("RAMAmount");
            elelemnt.SetAttributeValue("ram", e.Os.totalRam);
            e.Save.Add(elelemnt);
        }

        public override void Execute(EventExecutor executor, ElementInfo info)
        {
            Load(info);
        }

        public void Load(ElementInfo info)
        {
            Os.totalRam = int.Parse(info.Attributes["ram"]);
        }
    }
}