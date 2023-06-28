using System.IO;
using System.Linq;

namespace DJTwojaStara;

public class Helpers
{
    public static float GetUsedCacheSpace()
    {
        var enumOpts = new EnumerationOptions();
        enumOpts.RecurseSubdirectories = true;
        var files = Directory.GetFiles(Path.GetTempPath() + "/djtwojastara-temp", "*", enumOpts);
        var size = files.Select(x => new FileInfo(x).Length).Sum();
        
        return (float) (size / 1024.0 / 1024.0);
    }
}