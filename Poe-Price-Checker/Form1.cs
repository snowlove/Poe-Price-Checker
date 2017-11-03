using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;

using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using System.Globalization;

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

        //import for hotkeys, keyboard events, and other.
        private const string lib = "user32.dll";
        [DllImport(lib)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport(lib)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport(lib)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        [DllImport(lib)]
        private static extern bool GetCursorPos(out Point lpPoint); //shrug
        [DllImport(lib)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport(lib)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);



        private static string ItemData_0 = "";

        private static string[] Default_Config = {
                                                     "#This file is used to save hotkey preferences please do not edit as that may lead to load errors when starting the program.\r\n\r\n",
                                                     "['Modifier_0'] = 0x1", "['Hotkey_0'] = 0x31",
                                                     "['Modifier_1'] = 0x1", "['Hotkey_1'] = 0x32",
                                                     "['Modifier_2'] = 0x1", "['Hotkey_2'] = 0x33",
                                                     "['Modifier_3'] = 0x1", "['Hotkey_3'] = 0x34",
                                                     "['Modifier_4'] = 0x1", "['Hotkey_4'] = 0x35",
                                                     "['Modifier_5'] = 0x1", "['Hotkey_5'] = 0x36",
                                                     "['SMS_0'] = 0x0"
                                                 };
        private static bool _OnLoad = true;
        private bool onLoad_Error = false;
        private static Int32[] PreviousHotkey = { 0, 0 };

        private static readonly Dictionary<Keys, int> KeyModifiers = new Dictionary<Keys, int> {
            { Keys.Alt, 1 },
            { Keys.Control, 2 },
            { Keys.Alt ^ Keys.Control, 3 },
            { Keys.Shift, 4 },
            { Keys.Alt ^ Keys.Shift, 5 },
            { Keys.Shift ^ Keys.Control, 6 },
            { Keys.Alt ^ Keys.Shift ^ Keys.Control, 7 }
        };

        #region SMS_Tooltip class and init
        public class SMS_ToolTip_C
        {
            public bool Enabled { get; set; }
            public int SelectedTab { get; set; }

            public SMS_ToolTip_C(bool _en = false, int _st = 0)
            {
                Enabled = _en;
                SelectedTab = _st;
            }
        }

        public SMS_ToolTip_C SMS_ToolTip = new SMS_ToolTip_C();
        #endregion

        #region league api json class and init
        public class League_json
        {
            public string id { get; set; }
            public string url { get; set; }
            public DateTime startAt { get; set; }
            public DateTime? endAt { get; set; }
        }
        #endregion

        #region ThreadInformation class and shit
        public class ThreadInformation
        {
            public int id { get; set; }
            public bool isEnabled { get; set; }
            public Int32 Started { get; set; }
            public object richBox { get; set; }
            public object valueBox { get; set; }
            public object TimeWait { get; set; }
            public object hotBox { get; set; }
            public Int32 HK_Modifier { get; set; }
            public Int32 Hotkey { get; set; }

            public ThreadInformation(int _id, bool _isEnabled, Int32 _Started, object _richBox, object _valueBox, object _TimeWait, object _hotBox, Int32 _HKM = 0, Int32 _HK = 0)
            {
                isEnabled = _isEnabled;
                Started = _Started;
                richBox = _richBox;
                valueBox = _valueBox;
                TimeWait = _TimeWait;
                hotBox = _hotBox;

                if (_HKM == 0)
                    HK_Modifier = 1;
                if (_HK == 0)
                    Hotkey = 49 + _id;
            }
        }
        public static List<ThreadInformation> Threads = new List<ThreadInformation>();
        #endregion


        public Form1()
        {
            InitializeComponent();
            this.Icon = PoePriceChecker.Properties.Resources.bulldog; //make cute doggy icon
            int iOffSet = 0;


            for (int i = 0; i < 6; i++) //since I'm loading everything on to threads they need to be created first and foremost
            {
                iOffSet = i + 1;

                Threads.Add(new ThreadInformation(i, false, 0,             //Id, enabled, timestart
                    this.Controls.Find("richTextBox" + iOffSet, true)[0],  //Link "ItemBox" richtextbox to thread.
                    this.Controls.Find("Value" + iOffSet, true)[0],        //Link "Value" label to thread.
                    this.Controls.Find("pictureBox" + iOffSet, true)[0],   //Link Spinny Picture shit to thread.
                    this.Controls.Find("hotBox" + iOffSet, true)[0]        //Link hotkey setting box to thread.
                ));
            }

            Load_Config();

            for (int i = 0; i < 6; i++)
                Set_Hotkey(i, Threads[i].HK_Modifier, Threads[i].Hotkey, this.Handle, true);

            RegisterHotKey(this.Handle, 6, 4, Keys.Right.GetHashCode());
            RegisterHotKey(this.Handle, 7, 4, Keys.Left.GetHashCode());

            Label_Version.Text = string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);

            ToolTip SMS_Tooltip_tooltip = new ToolTip();
            SMS_Tooltip_tooltip.SetToolTip(checkBox1, "If enabled it will show a tooltip popup in game when you price check an item. So you don't have to Alt+TAB to see the program.");

            checkBox1.Checked = SMS_ToolTip.Enabled;

            comboBox1.Items.Add("Connection Error.");
            comboBox1.SelectedIndex = 0;

            Get_Leagues();
        }

        private void Get_Leagues()
        {
            using (WebClient wc = new WebClient())
            {
                try { wc.DownloadStringAsync(new Uri(@"http://api.pathofexile.com/leagues?type=main&compact=1")); }
                catch { Get_Leagues_Failover(); }

                wc.DownloadStringCompleted += (s, e) =>
                {
                    try { List<League_json> _tmp = JsonConvert.DeserializeObject<List<League_json>>(e.Result); }
                    catch {
                        Get_Leagues_Failover();
                        return;
                    }

                    List<League_json> leagues = JsonConvert.DeserializeObject<List<League_json>>(e.Result);

                    comboBox1.Items.Clear();

                    foreach (League_json f in leagues)
                    {
                        if (f.id.Contains("SSF"))
                            continue;

                        comboBox1.Items.Add(f.id);
                    }
                    comboBox1.SelectedIndex = 2;

                };
            }
        }


        private void Get_Leagues_Failover() //internal faillover incase we can't connect to path of exile api servers.
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Standard");
            comboBox1.Items.Add("Hardcore");
            comboBox1.Items.Add("Harbinger");
            comboBox1.SelectedIndex = 2;

            MessageBox.Show("Could not connect to Path of Exile servers to retrieve current leagues.\r\nDefaulting to Standard, Hardcore, Harbinger (Current league when the program was made.)\r\n\r\nIf problem persists for a long duration please submit bug by clicking 'New Issue' at https://github.com/snowlove/Poe-Price-Checker/issues", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void Load_Config(bool error = false)
        {
            string[] lines;

            if (!error)
            {
                if (!File.Exists("config.cfg"))
                {
                    File.WriteAllLines("config.cfg", Default_Config);
                    lines = Default_Config;
                }
                else
                    lines = File.ReadAllLines("config.cfg");
            }
            else
                lines = Default_Config;


            try
            {
                string[] IVT_Values = { null, null, null };
                string IVT;
                int Key;
                int Key_Value;

                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line) || line.Contains("#"))
                        continue;

                    IVT = Regex.Replace(line, @"[\['\]\s]", "");
                    IVT = Regex.Replace(IVT, @"[_\=]", "-");

                    IVT_Values = IVT.Split('-');

                    Key = Convert.ToInt32(IVT_Values[1]);
                    Key_Value = Convert.ToInt32(IVT_Values[2], 16);

                    if (IVT_Values[0] == "Modifier")
                        Threads[Key].HK_Modifier = Key_Value;
                    else if (IVT_Values[0] == "Hotkey")
                        Threads[Key].Hotkey = Key_Value;
                    else if (IVT_Values[0] == "SMS")
                        SMS_ToolTip.Enabled = Convert.ToBoolean(Key_Value);
                    else
                        throw new Exception();

                }
            }
            catch
            {
                MessageBox.Show("Problem while reading config file please delete the config.cfg file if problems persist\r\nloading default config for now.", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Load_Config(true);
            }
        }

        private void Save_Config(int id, Int32 mod = 0, Int32 key = 0, int sms = -1)
        {
            bool _Edited = false;
            int _Line_ID = 0;
            string[] lines;
            List<string> NewFile;

            try
            {
                if (File.Exists("config.cfg"))
                    lines = File.ReadAllLines("config.cfg");
                else
                    lines = Default_Config;

                NewFile = new List<string>(lines);

                /*
                 * if we call this void something is getting written to config we need to figure out what 
                 * -1 sms = default = we're not writing to sms (so default to writing mod/key values?)
                 * 
                 * 
                 * 
                 */

                foreach (string line in lines)
                {
                    string[] IVT_Values = { null, null, null };
                    string IVT;

                    if (string.IsNullOrEmpty(line) || line.Contains("#"))
                    {
                        NewFile[_Line_ID] = line;
                    }
                    else if(line.Contains("SMS"))
                    {
                        if (sms >= 0)
                        {
                            NewFile[_Line_ID] = string.Format("['SMS_0'] = {0:x}", sms);
                            _Edited = true;
                        }
                        else
                            NewFile[_Line_ID] = line;
                    }
                    else if (key != 0 && sms < 0)
                    {
                        IVT = Regex.Replace(line, @"[\['\]\s]", "");
                        IVT = Regex.Replace(IVT, @"[_\=]", "-");

                        IVT_Values = IVT.Split('-');

                        if (IVT_Values[0] == "Modifier" || IVT_Values[0] == "Hotkey")
                        {
                            if (Convert.ToInt32(IVT_Values[1]) == id)
                            {
                                NewFile[_Line_ID] = string.Format("['{0}_{1}'] = 0x{2:x}", IVT_Values[0], IVT_Values[1], IVT_Values[0] == "Modifier" ? mod : key);
                                _Edited = true;
                            }
                            else
                                NewFile[_Line_ID] = line;
                        }
                    }
                    _Line_ID++;
                }

                if (_Edited)
                    File.WriteAllLines("config.cfg", NewFile);
                else
                    throw new Exception();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save user pref to config file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(string.Format("{0}", ex));
            }
        }

        private void Set_Hotkey(int id, Int32 mod, Int32 key, IntPtr hwnd, bool onLoad = false)
        {
            bool Registration = false;
            int status = 0;

            if (!onLoad)
                for (int i = 0; i < 6; i++)
                    if (id != i)
                        if (key == Threads[i].Hotkey)
                            status = 3;

            if (status == 0)
            {
                UnregisterHotKey(hwnd, id);
                Registration = RegisterHotKey(hwnd, id, mod, key);

                if (!Registration)
                    status = 0;
                else
                {
                    Threads[id].HK_Modifier = mod;
                    Threads[id].Hotkey = key;
                    status = 1;
                }
            }

            /*
             * Status error handling
             * 0 = tried and failed at this point in registering the hotkey
             * 1 = success we registered the hotkey do some cleanup
             * 3 = hotkey already in use by this program
             * 
             * if 0 is called we need to re-reregister the old hotkey because we already unloaded the old (previoushk[] should work)
             */

            #region Error_Handling
            if (status != 1)
            {
                if (onLoad)
                {
                    onLoad_Error = true;

                    if (status == 3)
                        MessageBox.Show(string.Format("WARNING: Failed to register Hotkey.\r\nReason: It's already in use. Please close all instances of the application.\r\n\r\nIf problem persists please delete config.cfg file and retry."), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else if (status == 0)
                        MessageBox.Show(string.Format("WARNING: Failed to register Hotkey.\r\nReason: Unknown (Possibly already registered by another program or forbidden hotkey). Please close all instances of the application.\r\n\r\nIf problem persists please delete config.cfg file and retry."), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if (status == 0 || status == 3)
                    {
                        UnregisterHotKey(hwnd, id);
                        Registration = RegisterHotKey(hwnd, id, PreviousHotkey[1], PreviousHotkey[0]);

                        if (Registration)
                        {
                            ((TextBox)Threads[id].hotBox).Text = "None";
                            Threads[id].HK_Modifier = PreviousHotkey[1];
                            Threads[id].Hotkey = PreviousHotkey[0];
                            MessageBox.Show("Error: Unable to register hotkey (Already registered or invalid hotkey). Reverted to old hotkey.", "things failed to happen.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("WE'RE ALL FUCKING DOOMED IT FAILED AND THE FUCKING PROGRMA IS GOING TO EXPLODE", "Cheerio", MessageBoxButtons.YesNo, MessageBoxIcon.Question); //I should probably change this.
                        } 
                    }
                }
            }
            #endregion

            if (!onLoad && status == 1)
                Save_Config(id, mod, key);

            ((TextBox)Threads[id].hotBox).Parent.Focus();
            PreviousHotkey[0] = 0;
            PreviousHotkey[1] = 0;
            Make_HotKey_HumanReadable(id, Threads[id].HK_Modifier, Threads[id].Hotkey);
        }

        private static void Make_HotKey_HumanReadable(int id, Int32 mod, Int32 key)
        {
            string Modifier;
            string vk = "s";
            Regex r = new Regex(@"[D0-9]"); //fix currently matches all [D]

            switch(mod)
            {
                case 1:
                    Modifier = "Alt+";
                    break;
                case 3:
                    Modifier = "Ctrl+Alt+";
                    break;
                case 4:
                    Modifier = "Shift+";
                    break;
                case 5:
                    Modifier = "Alt+Shift+";
                    break;
                case 6:
                    Modifier = "Shift+Ctrl+";
                    break;
                case 7:
                    Modifier = "Shift+Ctrl+Alt+";
                    break;
                default:
                    Modifier = "N";
                    break;
            } //zlg was here

            foreach(var s in Enum.GetValues(typeof(Keys)))
                if (key == ((Keys)s).GetHashCode())
                {
                    Match m = r.Match(s.ToString());

                    if (m.Success)
                        vk = Regex.Replace(s.ToString(), @"[D]", string.Empty);
                    else
                        vk = s.ToString();
                }

            ((TextBox)Threads[id].hotBox).Text = (Modifier == "N" ? "" : Modifier) + vk;
        }

        private static Point GetCursorPosition()
        {
            Point lpPoint;
            GetCursorPos(out lpPoint);

            return lpPoint;
        }


        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }


        private void Create_WPF_Window()
        {
            //int scrnHeight = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight);
            int scrnWidth = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth);

            /****************************
             * WPF Window
             ****************************/
            WPFControl1 n = new WPFControl1();
            n.Left = GetCursorPosition().X;
            n.Top = GetCursorPosition().Y + 5;
            n.Height = 200;
            n.Width = 450;
            n.Show();
            

            /****************************
             * Label for WPF content
             ****************************/
            System.Windows.Controls.Label _fs = new System.Windows.Controls.Label();
            _fs.Foreground = System.Windows.Media.Brushes.White;
            _fs.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            _fs.FontSize = 18;
            _fs.FontWeight = System.Windows.FontWeights.Bold;
            _fs.Effect = new DropShadowEffect
            {
                Color = new System.Windows.Media.Color { A = 255, R = 0, G = 0, B = 0 },
                Direction = 0,
                ShadowDepth = 0,
                Opacity = 1
            };
            _fs.Opacity = 1;
            n.Content = _fs;


            /****************************
             * Timer to lock window to
             * cursor pos and keep
             * label.text up to date
             ****************************/
            System.Windows.Forms.Timer _t1 = new System.Windows.Forms.Timer();
            int _tt = 0;
            _t1.Tick += (sender, e) =>
            {
                if (_tt >= 220)
                {
                    _t1.Enabled = false;
                    DoubleAnimation anim = new DoubleAnimation(0, (System.Windows.Duration)TimeSpan.FromSeconds(1));
                    anim.Completed += (s, _) => n.Close();
                    n.BeginAnimation(System.Windows.UIElement.OpacityProperty, anim);

                    return;
                }
                string[] lines = ((RichTextBox)Threads[SMS_ToolTip.SelectedTab].richBox).Lines;
                int v;
                string sample;

                _fs.Content = string.Format("Name: {0}\r\nRecommended Value: {1}", lines.Length > 1 ? lines[1] : "E", ((Label)Threads[SMS_ToolTip.SelectedTab].valueBox).Text);
                v = string.Format("Name: {0}", lines.Length > 1 ? lines[1] : "E").Length > string.Format("Recommended Value: {0}", ((Label)Threads[SMS_ToolTip.SelectedTab].valueBox).Text).Length ? 1 : 2;

                if (v == 2)
                    sample = string.Format("Recommended Value: {0}", ((Label)Threads[SMS_ToolTip.SelectedTab].valueBox).Text);
                else
                    sample = string.Format("Name: {0}", lines.Length > 1 ? lines[1] : "E");


                System.Windows.Media.Typeface myTypeface = new System.Windows.Media.Typeface("Segoe UI");
                System.Windows.Media.FormattedText ft = new System.Windows.Media.FormattedText(sample, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, myTypeface, 20, System.Windows.Media.Brushes.White);

                if ((GetCursorPosition().X + ft.Width) + 25 > scrnWidth)
                {
                    n.Left = (GetCursorPosition().X - 25) - ft.Width;
                    n.Top = GetCursorPosition().Y;
                }
                else
                {
                    n.Left = GetCursorPosition().X + 25;
                    n.Top = GetCursorPosition().Y;
                }
                _tt++;
            };
            _t1.Interval = 10;
            _t1.Enabled = true;
        }


        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                if (GetActiveWindowTitle() != "Path of Exile")
                    return;

                //I work by ID so I don't need these but I want to keep them around incase I ever decide I need to see what was pressed
                //Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                //KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);
                int id = m.WParam.ToInt32();

                #region Tab Swapping
                if (id>5) //swap tabs back and forth
                {
                    int _maxIndex = tabControl1.TabCount;

                    if(id == 6) //right
                    {
                        var c = this.tabControl1.TabCount;
                        if (tabControl1.SelectedIndex + 2 < _maxIndex)
                            tabControl1.SelectedIndex = tabControl1.SelectedIndex + 1;
                        else
                            tabControl1.SelectedIndex = 0;
                    } else if(id == 7) //left
                    {
                        if (tabControl1.SelectedIndex - 1 < 0)
                            tabControl1.SelectedIndex = 5;
                        else
                            tabControl1.SelectedIndex = tabControl1.SelectedIndex - 1;
                    }
                    return;
                }
                #endregion

                //SendKeys.Send(key.ToString()); I may bring this back later.

                uint KEYEVENTF_KEYUP = 2;
                byte VK_CONTROL = 0x11;
                keybd_event(VK_CONTROL, 0, 0, 0);
                keybd_event(0x43, 0, 0, 0);
                keybd_event(0x43, 0, KEYEVENTF_KEYUP, 0);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);

                Thread.Sleep(50);

                ItemData_0 = Clipboard.GetText();

                if (!Threads[id].isEnabled)
                {
                    Threads[id].isEnabled = true;
                    Threads[id].Started = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    ((PictureBox)Threads[id].TimeWait).Visible = true;
                    HotKeyPressed(id);
                }
            }
        }

        private void HotKeyPressed(int _id) //brains of the operation hehe.
        {
            tabControl1.SelectTab(_id);
            /*
             * TODO: Create Cursor Busy for SMS
             * Should I delete users clipboard content ? if I don't the key will be blocked
             * or blacklist all alphanumeric keys without a modifier
             */
            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                int id = (int)e.Argument;
                e.Result = id;

                string ItemTextCopy = ItemData_0;
                string league = "Standard";
                string Response = "";
                string Value = "";
                NameValueCollection postParam = new NameValueCollection();
                byte[] rBytes;

                if (string.IsNullOrEmpty(ItemTextCopy) || !ItemTextCopy.Contains("Rarity:"))
                {
                    ((RichTextBox)Threads[id].richBox).Invoke((MethodInvoker)delegate
                    {
                        ((RichTextBox)Threads[id].richBox).Clear();
                    });
                    return;
                }
                else
                {
                    ((RichTextBox)Threads[id].richBox).Invoke((MethodInvoker)delegate
                    {
                        ((RichTextBox)Threads[id].richBox).Text = ItemTextCopy;
                        //TODO: Add notification to tab text for processing and finished processing request.
                    });
                }
                ((Label)Threads[id].valueBox).Invoke((MethodInvoker)delegate { ((Label)Threads[id].valueBox).Text = "----"; });

                ((ComboBox)comboBox1).Invoke((MethodInvoker)delegate { league = comboBox1.Text; });

                postParam.Add("itemtext", ItemTextCopy);
                postParam.Add("league", league);
                postParam.Add("auto", "auto");
                postParam.Add("submit", "Submit");

                while (true)
                {
                    try {
                        using (WebClient wc = new WebClient())
                        {
                            if ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds - Threads[id].Started > 20) {
                                Value = "Error: Timed out (Website down?)."; break;
                            }
                            wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");

                            rBytes = wc.UploadValues("http://www.poeprices.info/query", "POST", postParam);
                            Response = Encoding.UTF8.GetString(rBytes);

                            if (Response.Contains("Recommended Price"))
                                Value = Response.Split(new[] { "Recommended Price: " }, StringSplitOptions.None)[1].Substring(0, Response.Split(new[] { "Recommended Price: " }, StringSplitOptions.None)[1].IndexOf('<'));
                            else
                                Value = "Unknown.";

                            break;
                        }
                    } catch { }
                }

                ((Label)Threads[id].valueBox).Invoke((MethodInvoker)delegate { ((Label)Threads[id].valueBox).Text = Value; });
            };


            //Background Thread has completed
            bg.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                int id = (int)e.Result;

                ((PictureBox)Threads[id].TimeWait).Visible = false;
                Threads[id].Started = 0;
                Threads[id].isEnabled = false;

                SMS_ToolTip.SelectedTab = id;
                Create_WPF_Window();

                bg.Dispose();
            };

            bg.RunWorkerAsync(_id);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < 8; i++)
                UnregisterHotKey(Process.GetCurrentProcess().MainWindowHandle, i);
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

        private void hotBox1_KeyDown(object sender, KeyEventArgs e)
        {
            string _senderName = ((TextBox)sender).Name;
            int _sender_id = Convert.ToInt32(_senderName.Split(new[] { "hotBox" }, StringSplitOptions.None)[1]) - 1;

            Keys pressedKey = e.KeyData ^ e.Modifiers;

            /*
             * BLACKLIST: ctrl+pgdn, ctrl+pgup, caps lock, tab, shift+all numpad (and all combinations of shift+{X}+num pad), F12, ctrl+shift{e,r,a,j}
             * Q W E R T X D Z A ENT O C S L I H Y K N B U P G M space F8 F1 F 1 2 3 4 5
             */
            Keys[] Blacklist = { Keys.Capital, Keys.CapsLock, Keys.F12, Keys.Tab, Keys.Q, Keys.W, Keys.E, Keys.R, Keys.T, Keys.X, Keys.D, Keys.Z, Keys.A, Keys.Enter, Keys.O, Keys.C, Keys.S, Keys.L, Keys.I,
                                 Keys.H, Keys.Y, Keys.K, Keys.N, Keys.B, Keys.U, Keys.P, Keys.G, Keys.M, Keys.Space, Keys.F8, Keys.F1, Keys.F, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.Left, Keys.Right };

            foreach (Keys k in Blacklist)
            {
                if (k == pressedKey)
                {
                    if (!KeyModifiers.ContainsKey(e.Modifiers))
                    {
                        //TODO: Fix not being able to block specific key combinations (Shift+F)
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        return;
                    }
                }
            }
            
            if (e.KeyCode == Keys.Escape)
                ((TextBox)sender).Parent.Focus();
            else if (pressedKey != Keys.ShiftKey && pressedKey != Keys.Menu && pressedKey != Keys.ControlKey && pressedKey != Keys.None)
            {
                Set_Hotkey(_sender_id, KeyModifiers.ContainsKey(e.Modifiers) ? KeyModifiers[e.Modifiers] : 0, e.KeyCode.GetHashCode(), this.Handle);
            }
            else
                ((TextBox)sender).Text = new KeysConverter().ConvertToString(e.Modifiers);


            e.Handled = true;
            e.SuppressKeyPress = true;
        }


        private void hotBox1_Enter(object sender, EventArgs e)
        {
            string _senderName = ((TextBox)sender).Name;
            int _sender_id = Convert.ToInt32(_senderName.Split(new[] { "hotBox" }, StringSplitOptions.None)[1]) - 1;

            PreviousHotkey[0] = Threads[_sender_id].HK_Modifier;
            PreviousHotkey[1] = Threads[_sender_id].Hotkey;

            ((TextBox)sender).Text = "Esc to cancel.";
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            if (onLoad_Error)
                this.Close();

            _OnLoad = false;
        }


        /***********************************************
         * When HK is set or user cancels setting HK
         * focus is lost, reset hotbox to readable text
         ***********************************************/
        private void hotBox1_Leave(object sender, EventArgs e)
        {
            int _sender_id = Convert.ToInt32(((TextBox)sender).Name.Split(new[] { "hotBox" }, StringSplitOptions.None)[1]) - 1;

            if (string.IsNullOrEmpty(((TextBox)sender).Text) || ((TextBox)sender).Text == "Esc to cancel." || ((TextBox)sender).Text.Contains("None"))
                Make_HotKey_HumanReadable(_sender_id, PreviousHotkey[0], PreviousHotkey[1]);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SMS_ToolTip.Enabled = checkBox1.Checked;

            if(!_OnLoad)
                Save_Config(0, 0, 0, checkBox1.Checked == true ? 1 : 0);
        }
    }
}
