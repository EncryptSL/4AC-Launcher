using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCAC_Launcher
{
    public partial class LaunchGUI : Form
    {
        private readonly object StatusLock = new object();
        private string Status;
        private bool ShouldHide = false;
        private bool ShouldClose = false;
        private PrivateFontCollection pfc = new PrivateFontCollection();

        public LaunchGUI()
        {
            InitializeComponent();
            InitFont();
        }

        private void InitFont()
        {
            var res = Properties.Resources.Orbitron;
            var mem = Marshal.AllocHGlobal(res.Length);
            Marshal.Copy(res, 0, mem, res.Length);
            pfc.AddMemoryFont(mem, res.Length);

            LabelTitle.Font = new Font(pfc.Families[0], LabelTitle.Font.Size);
            LabelStatus.Font = new Font(pfc.Families[0], LabelStatus.Font.Size);

            this.Refresh();
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            lock (StatusLock)
            {
                if (Status == null) return;
                LabelStatus.Text = Status;
                //this.Refresh();
                if (ShouldHide)
                {
                    this.Hide();
                    ShouldHide = false;
                }
                if (ShouldClose)
                {
                    this.Close();
                    ShouldClose = false;
                }
            }
        }

        public void SetStatus(string status)
        {
            lock (StatusLock)
            {
                Status = status;
            }
        }

        public void SafeHide()
        {
            lock (StatusLock)
            {
                ShouldHide = true;
            }
        }

        public void SafeClose()
        {
            lock (StatusLock)
            {
                ShouldClose = true;
            }
        }

        private void LaunchGUI_Load(object sender, EventArgs e)
        {
            StatusUpdateTimer.Start();
        }
    }
}
