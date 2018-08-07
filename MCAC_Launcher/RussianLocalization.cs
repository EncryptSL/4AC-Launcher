using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCAC_Launcher
{
    class RussianLocalization : Localization
    {
        public string InvalidUsage()
        {
            return "Установите этот файл в качестве Java Runtime Executable в вашем Minecraft лаунчере";
        }

        public string OutOfDate()
        {
            return "Этот лаунчер устарел, пожалуйста загрузите последнюю версию";
        }
    }
}
