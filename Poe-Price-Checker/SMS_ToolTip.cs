using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PoePriceChecker
{
    public class SMS_ToolTip
    {
        public bool Enabled { get; set; }
        public int SelectedTab { get; set; }
        private System.Windows.Forms.ColorDialog fontColorSelector { get; set; }
        /* What a fucking mess...... Why can't WPF accept integers? */
        private Color _fc { set { fontColor[0] = Convert.ToByte(value.A); fontColor[1] = Convert.ToByte(value.R); fontColor[2] = Convert.ToByte(value.G); fontColor[3] = Convert.ToByte(value.B); } }
        private Color ___fc { get; set; }
        public Byte[] fontColor { get; set; }
        /*public Color fontColor {
            get { return _fc; }
            set
            {
                _fc = value;
                ColorBox.BackColor = value;
                ItemData_0 = "moo";
                //TODO :: Add saving.
            }
            
        }*/

        public SMS_ToolTip(bool _en = false, int _st = 0)
        {
            Enabled = _en;
            SelectedTab = _st;
            fontColorSelector = new System.Windows.Forms.ColorDialog();
            fontColor = new Byte[4] {0xFF, 0x0, 0x0, 0x0};
            _fc = Color.White;
        }

        public void SetFontColorForToolTip()
        {
            //Console.WriteLine(_fc);
            if (fontColorSelector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _fc = fontColorSelector.Color;
        }
    }
}
