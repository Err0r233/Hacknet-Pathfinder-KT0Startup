using Hacknet;

namespace KT0Mods.KT0Cmd
{
    public class Base64Encode
    {
        public static void Trigger(OS os, string[] args)
        {
            if (args.Length != 2)
            {
                os.write("usage: base64 <string>; output: a string that is base64 encoded.");
            }
            else
            {
                os.write(Utils.Utils.B64Encode(args[1]));
            }
        }
    }
}

