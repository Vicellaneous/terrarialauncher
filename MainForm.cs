using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace Launcher
{
    public partial class MainForm : DevComponents.DotNetBar.Metro.MetroForm
    {
        string[] fileconf;
        bool isUpdateLauncher;
        bool isUpdateGame;
        string server;
        string name;
        string[] ver;
        bool updateGame = false;

        public MainForm()
        {
            InitializeComponent();
        }
        
        private void MenuResolution_Click(object sender, EventArgs e)
        {

        }

        private void MenuPlay_Click(object sender, EventArgs e)
        {
            Process.Start(fileconf[3] + "Steam.exe", "-applaunch 105600");
            this.Close();
        }

        private void MenuLanguage0_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            fileconf = File.ReadAllText("Server.txt").Split('\n');
            if (fileconf.Length < 4)
            {
                lblStatus.Text = "Configuring for first time usage.";
                rtbUpdate.AppendText("\n- Configuring for first time usage.");
                File.WriteAllText("Server.txt", "http://127.0.0.1/"+ Environment.NewLine +"true" + Environment.NewLine +"true" + Environment.NewLine + @"C:\Program Files\Steam\");
                fileconf = File.ReadAllText("Server.txt").Split('\n');
            }
            server = fileconf[0].Trim();
            isUpdateLauncher = bool.Parse(fileconf[1]);
            isUpdateGame = bool.Parse(fileconf[2]);

            using (WebClient ww = new WebClient())
            {
                ww.DownloadStringAsync(new Uri(server + "changelog.txt"));
                ww.DownloadStringCompleted += ww_DownloadchangelogCompleted;
            }

            using (WebClient wc = new WebClient())
            {
                lblStatus.Text = "Contacting Server...";
                rtbUpdate.AppendText("\n- Contacting Server...");
                wc.DownloadStringAsync(new Uri(server + "updt.txt"));
                wc.DownloadProgressChanged += wc_DownloadupdtChanged;
                wc.DownloadStringCompleted +=wc_DownloadupdtCompleted;
            }
        }

        void ww_DownloadchangelogCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            rtbChangeLog.Text = e.Result;
        }

        private void wc_DownloadupdtChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lblStatus.Text = "Reply Received.";
            rtbUpdate.AppendText("\n- Reply Received.");
            lblData.Text = e.BytesReceived.ToString() + "/" + e.TotalBytesToReceive.ToString();
            pgLoading.Value = e.ProgressPercentage;
        }

        private void wc_DownloadupdtCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            lblStatus.Text = "Read Updates...";
            rtbUpdate.AppendText("\n- Read Updates...");
            lblData.Text = "No Data";
            pgLoading.Value = 0;
            ver = e.Result.Split('\n');
            if (int.Parse(FileVersionInfo.GetVersionInfo("Terraria.exe").ProductVersion.Replace(".", "")) < int.Parse(ver[0].Trim().Replace(".", "")))
            {
                name = @"Updates\Terraria-" + ver[0] + ".zip";
                if (!Directory.Exists("Updates"))
                {
                    Directory.CreateDirectory("Updates");
                }

                if (isUpdateGame)
                {
                    lblStatus.Text = "Found New Game Update, Downloading...";
                    rtbUpdate.AppendText("\n- Found New Game Update, Downloading...");
                    using (WebClient wc = new WebClient())
                    {
                        updateGame = true;
                        wc.DownloadFileAsync(new Uri(server + "terraria.zip"), name);
                        wc.DownloadProgressChanged += wc_DownloadzipChanged;
                        wc.DownloadFileCompleted += wc_DownloadzipCompleted;
                    }
                }
                else
                {
                    lblStatus.Text = "Found New Pending Game Update..";
                    rtbUpdate.AppendText("\n- Found New Pending Game Update...");
                    updateLauncher();
                }
            }
            else
            {
                MainMenu.Enabled = true;
                lblStatus.Text = "Already Updated.";
                rtbUpdate.AppendText("\n- Already Updated.");
                updateLauncher();
            }
        }

        void updateLauncher()
        {
            if (int.Parse(Application.ProductVersion.Replace(".", "")) < int.Parse(ver[1].Trim().Replace(".", "")))
            {
                name = @"Updates\Launcher-" + ver[1] + ".zip";
                if (!Directory.Exists("Updates"))
                {
                    Directory.CreateDirectory("Updates");
                }

                if (isUpdateGame)
                {
                    lblStatus.Text = "Found New Launcher Update, Downloading...";
                    rtbUpdate.AppendText("\n- Found New Launcher Update, Downloading.");
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFileAsync(new Uri(server + "launcher.zip"), name);
                        wc.DownloadProgressChanged += wc_DownloadzipChanged;
                        wc.DownloadFileCompleted += wc_DownloadzipCompleted;
                    }
                }
                else
                {
                    lblStatus.Text = "Found New Pending Launcher Update..";
                    rtbUpdate.AppendText("\n- Found New Pending Launcher Update.");
                }
            }
            else
            {
                MainMenu.Enabled = true;
                lblStatus.Text = "Already Updated.";
                rtbUpdate.AppendText("\n- Already Updated.");
            }
        }

        void wc_DownloadzipCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "unzip.exe",
                    Arguments = "-o " + name,
                    UseShellExecute = true,
                    CreateNoWindow = false
                }
            };
            proc.Start();

            if (int.Parse(Application.ProductVersion.Replace(".", "")) < int.Parse(ver[1].Replace(".", "")) && updateGame)
            {
                updateLauncher();
            }
            else
            {
                this.Close();
            }
        }

        private void wc_DownloadzipChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lblData.Text = e.BytesReceived.ToString() + "/" + e.TotalBytesToReceive.ToString();
            pgLoading.Value = e.ProgressPercentage;
        }

    }
}