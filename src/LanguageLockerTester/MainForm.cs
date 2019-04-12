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

        private void MainForm_Load(Object sender, EventArgs e)
        {
            if (this._languageLocker.Create())
            {
                this._languageLocker.Lock();
            }

            var keyboardLayouts = this._languageLocker.GetKeyboardLayouts(0);

            this.toolStripStatusLabelKeyboardLayouts.Text = $"{keyboardLayouts.Length} keyboard layout(s): ";

            for (var i = 0; i < keyboardLayouts.Length; i++)
            {
                if (i > 0)
                {
                    this.toolStripStatusLabelKeyboardLayouts.Text += ", ";
                }

                var keyboardLayout = keyboardLayouts[i];
                this.toolStripStatusLabelKeyboardLayouts.Text += $"0x{keyboardLayout:X8} '{this._languageLocker.GetKeyboardLayoutName(keyboardLayout)}'";
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern Boolean GetKeyboardLayoutNameA(StringBuilder pwszKLID);

        private String GetKeyboardLayoutName()
        {
            var name = new StringBuilder(16);
            return GetKeyboardLayoutNameA(name) ? name.ToString() : "";
        }

        private void MainForm_FormClosing(Object sender, FormClosingEventArgs e)
        {
            this._languageLocker.Dispose();
        }
    }
}
