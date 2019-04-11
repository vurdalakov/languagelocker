using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vurdalakov;

namespace Vurdalakov
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.InitializeComponent();
        }

        private KeyboardLayoutLocker _languageLocker = new KeyboardLayoutLocker();

        private void Form1_Load(Object sender, EventArgs e)
        {
            this._languageLocker.Lock();
        }

        private void MainForm_FormClosing(Object sender, FormClosingEventArgs e)
        {
            this._languageLocker.Unlock();
        }
    }
}
