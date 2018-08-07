using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCAC_Launcher
{
    class GermanLocalization : Localization
    {
        public string OutOfDate()
        {
            return "Dieser Launcher ist veraltet. Bitte lade die neue Version runter.";
        }

        public string InvalidUsage()
        {
            return "Bitte setze diese Datei als Java Runtime Executable in deinem Minecraft Launcher";
        }
    }
}
