using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Security.Principal;
using System.Reflection;
using System.IO;
using System.Drawing.Imaging;

namespace GTO5_Tool
{

    public partial class Form1 : Form
    {
        private readonly int _delayInSeconds;
        private readonly System.Media.SoundPlayer _player = new System.Media.SoundPlayer(Properties.Resources.Click);
        private readonly System.Media.SoundPlayer _camPlayer = new System.Media.SoundPlayer(Properties.Resources.camera);

        public Form1()
        {
            _delayInSeconds = 10;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _player.Play();
            var t = new Thread(new ThreadStart(PauseProcess));
            t.Start();
            t.Join();
        }

        [Flags]
        private enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        private void PauseProcess()
        {
            var process = Process.GetProcessesByName("GTA5")
                ?.FirstOrDefault(x => x.ProcessName == "GTA5");

            if (process == null)
            {
                label1.Text = @"GTA 5 process not found";
                return;
            }
            var pid = process.Id;
            label1.Text = @"Stopping process";
            SuspendProcess(pid);
            Thread.Sleep(_delayInSeconds * 1000);
            ResumeProcess(pid);
            label1.Text = @"Done";
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        private void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            foreach (ProcessThread pT in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        private void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }

        List<int> ports = new List<int>() {61455, 6672, 61457, 61456, 61458};
        private void button2_Click(object sender, EventArgs e)
        {
            _player.Play();
            Process process = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "netsh.exe"
                }
            };
            int i = 0;
            foreach ( var port in ports )
            {
                process.StartInfo.Arguments = $"advfirewall firewall add rule name=GTA-Tool{i} dir=in action=block protocol=UDP localport={port}";
                i++;
                process.Start();
                Thread.Sleep(500);
            }

            foreach (var port in ports)
            {
                process.StartInfo.Arguments = $"advfirewall firewall add rule name=GTA-Tool{i} dir=out action=block protocol=UDP localport={port}";
                i++;
                process.Start();
                Thread.Sleep(500);
            }
            label1.Text = @"Add Rule Done!";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _player.Play();
            Process process = new Process
            {
                StartInfo =
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = ( "netsh.exe" )
                }
            };
            int i = 0;
            foreach ( var port in ports )
            {
                process.StartInfo.Arguments =
                    $"advfirewall firewall delete rule name=GTA-Tool{i} protocol=udp localport={port}";
                i++;
                process.Start();
                Thread.Sleep(500);
            }
            foreach (var port in ports)
            {
                process.StartInfo.Arguments =
                    $"advfirewall firewall delete rule name=GTA-Tool{i} protocol=udp localport={port}";
                i++;
                process.Start();
                Thread.Sleep(500);
            }

            label1.Text = @"Delete Rule Done!";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _player.Play();
            Process.Start("explorer.exe");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _player.Play();

               using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GTO5_Tool.file.antiAFK.exe"))
               using ( var destination = new MemoryStream())
               {
                   if ( stream == null )
                   {
                       MessageBox.Show( @"Could not read antiAFK script from resources" );
                       return;  
                   }
                   stream.CopyTo( destination  );
                   File.WriteAllBytes( "antiAFK.exe", destination.ToArray() );
               }
               Process.Start(@"antiAFK.exe");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _player.Play();
            const string message = "This tool Allows You To Have Your Own Public Lobby To Get Away From Greifers/Hackers";
            const string title = "About - Made By oc66";
            MessageBox.Show(message, title);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _player.Play();
            if (checkBox1.Checked)
            {
                HotKeyManager.UnregisterHotKey(2);
                HotKeyManager.RegisterHotKey(0, 0);
                HotKeyManager.RegisterHotKey(Keys.F2, KeyModifiers.Shift);
                HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(button5_Click);
            }
            if (checkBox1.Checked & checkBox2.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox2.CheckState = 0;
            }
            if (checkBox1.Checked & checkBox3.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox3.CheckState = 0;
            }
            if (checkBox1.Checked & checkBox4.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox4.CheckState = 0;
            }
            if (checkBox1.Checked & checkBox5.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox5.CheckState = 0;
            }
            if ( checkBox1.CheckState != 0 ) return;
            HotKeyManager.UnregisterHotKey(4);
            HotKeyManager.HotKeyPressed -= new EventHandler<HotKeyEventArgs>(button5_Click);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            _player.Play();
            if (checkBox2.Checked)
            {
                HotKeyManager.UnregisterHotKey(4);
                HotKeyManager.RegisterHotKey(Keys.End, KeyModifiers.Control);
                HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(button7_Click);
            }
            if (checkBox2.Checked & checkBox1.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox1.CheckState = 0;
            }
            if (checkBox2.Checked & checkBox3.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox3.CheckState = 0;
            }
            if (checkBox2.Checked & checkBox4.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox4.CheckState = 0;
            }
            if (checkBox2.Checked & checkBox5.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox5.CheckState = 0;
            }
            if ( checkBox2.CheckState != 0 ) return;
            HotKeyManager.UnregisterHotKey(2);
            HotKeyManager.HotKeyPressed -= new EventHandler<HotKeyEventArgs>(button7_Click);
        }
        // Screenshot
        private void Shooter(string filePath)
        {
            // Screenshot block to screenshot Multiple screens
            using (var screenshot = new Bitmap(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height,
                                    PixelFormat.Format32bppArgb))
            using ( var screenGraph = Graphics.FromImage( screenshot ) )
            {
                screenGraph.CopyFromScreen(SystemInformation.VirtualScreen.X, SystemInformation.VirtualScreen.Y, 0, 0,
                    SystemInformation.VirtualScreen.Size,
                    CopyPixelOperation.SourceCopy);
                screenshot.Save(filePath, System.Drawing.Imaging.ImageFormat.Png); 
            }
            _camPlayer.Play();
            // Application.Restart();
            // Environment.Exit(0);
        }

        [DllImport("kernel32.dll")]
        static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

        static uint GetTerminalServicesSessionId()
        {
            var proc = Process.GetCurrentProcess();
            var pid = proc.Id;

            return ProcessIdToSessionId((uint)pid, out var sessionId) ? sessionId : 1U;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            _player.Play();
            // Checks for too many args -> creates a string from array -> checks for no args -> checks if the file already exists -> screenshot
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            const string args = "";
            var filePath = string.Join("", args);

            if (filePath == null || !filePath.Any())
            {
                filePath = appPath + "\\" + DateTime.Now.ToString("M-dd-yyyy_HH-mm-ss") + ".png";
            }
            else if (File.Exists(filePath))
            {
                label1.Text = @"File already exists!";
                return;
            }
            else if (Directory.Exists(filePath))
            {
                if (filePath.EndsWith("\\"))
                    filePath = filePath + DateTime.Now.ToString("M-dd-yyyy_HH-mm-ss") + ".png";
                else
                    filePath = filePath + "\\" + DateTime.Now.ToString("M-dd-yyyy_HH-mm-ss") + ".png";
            }
            else
            {
                if (filePath.EndsWith("\\"))
                    filePath = filePath + DateTime.Now.ToString("M-dd-yyyy_HH-mm-ss") + ".png";
                else
                    filePath = filePath + "\\" + DateTime.Now.ToString("M-dd-yyyy_HH-mm-ss") + ".png";
            }

            try
            {
                Shooter(filePath);
                label1.Text = @"Screenshot Saved to Desktop!";
            }
            catch (System.ComponentModel.Win32Exception)
            {
                bool isElevated;
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                if (isElevated)
                {
                    var sessionId = GetTerminalServicesSessionId();
                    if (sessionId == 0)
                    {
                        return;
                    }
                    else
                    {
                        var winDir = System.Environment.GetEnvironmentVariable("WINDIR");
                        Process.Start(Path.Combine(winDir, "system32", "tscon.exe"),
                            String.Format("{0} /dest:console", GetTerminalServicesSessionId()))
                            ?.WaitForExit();
                        Thread.Sleep(1000);
                        Shooter(filePath);
                        label1.Text = @"Screenshot Saved to Desktop!";
                    }
                }
            }
            catch (ExternalException)
            {
                filePath = appPath + "\\" + DateTime.Now.ToString("M-dd-yyyy_HH-mm-ss") + ".png";
                Shooter(filePath);
                label1.Text = @"Screenshot Saved to Desktop!";
            }
            catch (Exception)
            {
                label1.Text = @"There was a problem saving the file!";
            }
        }

        private void label1_Click( object sender, EventArgs e )
        {
            throw new NotImplementedException();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        //private void button8_Click(object sender, EventArgs e)
        //{
        //    _player.Play();
        //    Process process = new Process
        //    {
        //        StartInfo =
        //        {
        //            WindowStyle = ProcessWindowStyle.Hidden,
        //            FileName = "netsh.exe"
        //        }
        //    };
        //    string value = "127.0.0.1";
        //    if (InputBox("Add New IP", "Please Enter IP address To White list:", ref value) == DialogResult.OK)
        //    {
        //        string ip = value;
        //        //interface ipv4 install
        //        process.StartInfo.Arguments = $"interface portproxy add v4tov4 listenaddress=127.0.0.1 listenport=6672 connectaddress={ip} connectport=9001";
        //        process.Start();
        //        Thread.Sleep(500);
        //        label1.Text = @"IP White List Done!";
        //    }
        //}

        //private void button9_Click(object sender, EventArgs e)
        //{
        //    _player.Play();
        //    Process process = new Process
        //    {
        //        StartInfo =
        //        {
        //            WindowStyle = ProcessWindowStyle.Hidden,
        //            FileName = "netsh.exe"
        //        }
        //    };

        //    process.StartInfo.Arguments = $"interface portproxy delete v4tov4 listenaddress=127.0.0.1 listenport=6672";
        //    process.Start();
        //    Thread.Sleep(500);
        //    label1.Text = @"IP Deleted!";
        //}

        private void button10_Click(object sender, EventArgs e)
        {
            _player.Play();
            Process process = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "netsh.exe"
                }
            };
            string value = "127.0.0.1";
            if (InputBox("Block IP", "Please Enter IP address To Block:", ref value) == DialogResult.OK)
            {
                string ip = value;
                process.StartInfo.Arguments = $"advfirewall firewall add rule name=GTA-Tool-Block-IP dir=in interface=any action=block remoteip={ip}";
                process.Start();
                Thread.Sleep(500);
                process.StartInfo.Arguments = $"advfirewall firewall add rule name=GTA-Tool-Block-IP dir=out interface=any action=block remoteip={ip}";
                process.Start();
                Thread.Sleep(500);
                label1.Text = @"IP Blocked!";
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            _player.Play();
            if (checkBox3.Checked)
            {
                HotKeyManager.UnregisterHotKey(2);
                HotKeyManager.RegisterHotKey(0, 0);
                HotKeyManager.RegisterHotKey(Keys.F3, KeyModifiers.Shift);
                HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(button1_Click);
            }
            if (checkBox3.Checked & checkBox2.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox2.CheckState = 0;
            }
            if (checkBox3.Checked & checkBox1.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox1.CheckState = 0;
            }
            if (checkBox3.Checked & checkBox4.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox4.CheckState = 0;
            }
            if (checkBox3.Checked & checkBox5.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox5.CheckState = 0;
            }
            if (checkBox3.CheckState != 0) return;
            HotKeyManager.UnregisterHotKey(4);
            HotKeyManager.HotKeyPressed -= new EventHandler<HotKeyEventArgs>(button1_Click);
            
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            _player.Play();
            if (checkBox4.Checked)
            {
                HotKeyManager.UnregisterHotKey(2);
                HotKeyManager.UnregisterHotKey(4);
                HotKeyManager.RegisterHotKey(0, 0);
                HotKeyManager.RegisterHotKey(Keys.E, KeyModifiers.Alt);
                HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(button2_Click);
            }
            if (checkBox4.Checked & checkBox2.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox2.CheckState = 0;
            }
            if (checkBox4.Checked & checkBox3.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox3.CheckState = 0;
            }
            if (checkBox4.Checked & checkBox5.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox5.CheckState = 0;
            }
            if (checkBox4.Checked & checkBox1.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox1.CheckState = 0;
            }
            if (checkBox4.CheckState != 0) return;
            HotKeyManager.UnregisterHotKey(2);
            HotKeyManager.UnregisterHotKey(4);
            HotKeyManager.HotKeyPressed -= new EventHandler<HotKeyEventArgs>(button2_Click);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            _player.Play();
            if (checkBox5.Checked)
            {
                HotKeyManager.UnregisterHotKey(2);
                HotKeyManager.UnregisterHotKey(4);
                HotKeyManager.RegisterHotKey(0, 0);
                HotKeyManager.RegisterHotKey(Keys.D, KeyModifiers.Alt);
                HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(button3_Click);
            }
            if (checkBox5.Checked & checkBox1.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox1.CheckState = 0;
            }
            if (checkBox5.Checked & checkBox2.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox2.CheckState = 0;
            }
            if (checkBox5.Checked & checkBox4.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox4.CheckState = 0;
            }
            if (checkBox5.Checked & checkBox3.Checked)
            {
                const string message = "Please select only one Hotkey!";
                const string title = "Error!";
                MessageBox.Show(message, title);
                checkBox3.CheckState = 0;
            }
            if (checkBox5.CheckState != 0) return;
            HotKeyManager.UnregisterHotKey(1);
            HotKeyManager.HotKeyPressed -= new EventHandler<HotKeyEventArgs>(button3_Click);
        }
    }
}