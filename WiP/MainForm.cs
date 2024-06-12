using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiP
{
    public class MainForm : Form
    {
        public MenuStrip MenuStrip { get; private set; }

        public MainForm()
        {
            ClientSize = new Size(1100, 600);

            MenuStrip = new MenuStrip()
            {
                Parent = this,
                Visible = true,
            };
        }
    }
}
