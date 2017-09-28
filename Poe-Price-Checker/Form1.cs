using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

using System.Diagnostics;
namespace PoePriceChecker
{
    public partial class Form1 : Form
    {
        public static List<string> Update_Urls = new List<string>() //For another part of the project not working
        {
            "Currency",
            "Divination",
            "Essence",
            "UniqueJewel",
            "Map",
            "UniqueMap",
            "Prophecy",
            "UniqueWeapon"
        };

        public static string ItemData_0 = "";

        #region ThreadInformation class and shit
        public class ThreadInformation
        {
            public int id { get; set; }
            public bool isEnabled { get; set; }
            public double Started { get; set; }
            public object richBox { get; set; }
            public object valueBox { get; set; }
            public object TimeWait { get; set; }

            public ThreadInformation(int _id, bool _isEnabled, double _Started, object _richBox, object _valueBox, object _TimeWait)
            {
                id = _id;
                isEnabled = _isEnabled;
                Started = _Started;
                richBox = _richBox;
                valueBox = _valueBox;
                TimeWait = _TimeWait;
            }
        }
        public static List<ThreadInformation> Threads = new List<ThreadInformation>();
        #endregion

        #region Hotkey_Import
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        public Int32[] HotKeyHashes = { Keys.D1.GetHashCode(), Keys.D2.GetHashCode(), Keys.D3.GetHashCode(), Keys.D4.GetHashCode(), Keys.D5.GetHashCode(), Keys.D6.GetHashCode() };
        #endregion

        #region CopyPasta_Import
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        #endregion


        public Form1()
        {
            InitializeComponent();
            this.Icon = PoePriceChecker.Properties.Resources.bulldog; //make cute doggy icon
            int iOffSet = 0;

            for (int i = 0; i < 6; i++)
            {
                iOffSet = i + 1;

                RegisterHotKey(this.Handle, i, (int)KeyModifier.Alt, HotKeyHashes[i]); //Register Hotkeys Alt-1 through Alt-6 ::TODO:: Add Shift-1 through Shift-5 for swapping tabs
                Threads.Add(new ThreadInformation(i, false, 0.0f,
                    this.Controls.Find("richTextBox" + iOffSet, true)[0], //Link "ItemBox" richtextbox to thread.
                    this.Controls.Find("Value" + iOffSet, true)[0],       //Link "Value" label to thread.
                    this.Controls.Find("pictureBox" + iOffSet, true)[0]   //Link Spinny Picture shit to thread.
                ));
            }
        }

        public static async Task<string> DownloadStringAsync(int index, int timeOut = 15000)
        {
            string output = null;
            bool cancelledOrError = false;
            using (var client = new WebClient())
            {
                client.DownloadStringCompleted += (sender, e) =>
                {
                    if (e.Error != null || e.Cancelled)
                    {
                        cancelledOrError = true;
                    }
                    else
                    {
                        output = e.Result;
                    }
                };
                client.DownloadStringAsync(new Uri(string.Format("http://cdn.poe.ninja/api/Data/Get{0}Overview?league=Harbinger", Update_Urls[index])));
                var n = DateTime.Now;
                while (output == null && !cancelledOrError && DateTime.Now.Subtract(n).TotalMilliseconds < timeOut)
                {
                    await Task.Delay(100); // wait for respsonse
                }
            }
            return output;
        }

        protected override void WndProc(ref Message m) //listen for hotkeys
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);
                int id = m.WParam.ToInt32();

                uint KEYEVENTF_KEYUP = 2;
                byte VK_CONTROL = 0x11;
                keybd_event(VK_CONTROL, 0, 0, 0);
                keybd_event(0x43, 0, 0, 0);
                keybd_event(0x43, 0, KEYEVENTF_KEYUP, 0);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);

                Thread.Sleep(25);

                ItemData_0 = Clipboard.GetText();

                if (!Threads[id].isEnabled)
                {
                    Threads[id].isEnabled = true;
                    ((PictureBox)Threads[id].TimeWait).Visible = true;
                    HotKeyPressed(id);
                }
            }
        }

        private void HotKeyPressed(int _id)
        {
            tabControl1.SelectTab(_id);

            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                int id = (int)e.Argument;
                e.Result = id;

                string Value = "";
                string ItemTextCopy = ItemData_0;
                string Corruption_Test = "";
                string Corruption_richtextbox = "";

                if (string.IsNullOrEmpty(ItemTextCopy) || !ItemTextCopy.Contains("Rarity:"))
                    return;

                ((RichTextBox)Threads[id].richBox).Invoke((MethodInvoker)delegate {
                    Corruption_richtextbox = ((RichTextBox)Threads[id].richBox).Text;
                    ((RichTextBox)Threads[id].richBox).Text = ItemTextCopy;
                    //((RichTextBox)Threads[id].richBox).Lines[1] = string.Join(((RichTextBox)Threads[id].richBox).Lines[1], "*");
                });

                ((Label)Threads[id].valueBox).Invoke((MethodInvoker)delegate { ((Label)Threads[id].valueBox).Text = "----"; });

                if(!string.IsNullOrEmpty(ItemTextCopy) && !string.IsNullOrEmpty(Corruption_richtextbox))
                    Corruption_Test = Corruption_richtextbox + Environment.NewLine + "--------" + Environment.NewLine + "Corrupted";

                if (string.IsNullOrEmpty(ItemTextCopy))
                    return;
                else if (ItemData_0 == Corruption_Test)
                    Debug.WriteLine("The item test was successful and will return instead of rechecking price.");

                Debug.WriteLine("Got itemtext as : " + ItemTextCopy);
                while (true)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            var postParam = new NameValueCollection();
                            postParam.Add("itemtext", ItemTextCopy);
                            postParam.Add("league", "Harbinger");
                            postParam.Add("auto", "auto");
                            postParam.Add("submit", "Submit");

                            byte[] responsebytes = wc.UploadValues("http://www.poeprices.info/query", "POST", postParam);
                            var tmpp = Encoding.UTF8.GetString(responsebytes);

                                /*string[] tokens = tmpp.Split(new[] { "Recommended Price: " }, StringSplitOptions.None);
                                int EndPointer = tokens[1].IndexOf('<');
                                Value = tokens[1].Substring(0, EndPointer);*/

                            if (tmpp.Contains("Recommended Price"))
                                Value = tmpp.Split(new[] { "Recommended Price: " }, StringSplitOptions.None)[1].Substring(0, tmpp.Split(new[] { "Recommended Price: " }, StringSplitOptions.None)[1].IndexOf('<'));
                            else
                                Value = "Unknown.";

                            break;
                        }
                    } catch (Exception eR) { Debug.WriteLine(eR.ToString()); }
                }

                ((Label)Threads[id].valueBox).Invoke((MethodInvoker)delegate { ((Label)Threads[id].valueBox).Text = Value; });
            };


            //Background Thread has completed
            bg.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                int id = (int)e.Result;

                ((RichTextBox)Threads[id].richBox).Invoke((MethodInvoker)delegate {
                    //((RichTextBox)Threads[id].richBox).Lines[1] = string.Join(((RichTextBox)Threads[id].richBox).Lines[1], "\u221A");
                });

                ((PictureBox)Threads[id].TimeWait).Visible = false;
                Threads[id].isEnabled = false;

                bg.Dispose();
            };

            bg.RunWorkerAsync(_id);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < 6; i++)
                UnregisterHotKey(this.Handle, i);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (((RichTextBox)sender).Text == "")
                return;
            else
                if (((RichTextBox)sender).Lines.Length > 2)
                    ((RichTextBox)sender).Parent.Text = ((RichTextBox)sender).Lines[1];
                else
                    return;
        }
    }
}
