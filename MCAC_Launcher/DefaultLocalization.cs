using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCAC_Launcher
{
    class DefaultLocalization : Localization
    {
        public string OutOfDate()
        {
            return "This Launcher is outdated. Please download the updated version.";
        }

        public string InvalidUsage()
        {
            return "Please set this file as Java Runtime Executable in your Minecraft Launcher";
        }
    }
}
