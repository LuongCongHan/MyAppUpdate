using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MyUpdateApp
{
    public class UpdateVersion
    {
        public string Version { get; set; }
        public string Url { get; set; }
        public override string ToString()
        {
            return "Version: " + Version + Environment.NewLine + "Url: " + Url;
        }
    }
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        HttpClient httpclient = new HttpClient();
        System.Diagnostics.FileVersionInfo fvi;
        UpdateVersion update;
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "MyUpdateApp + v" + this.ProductVersion;
        }

        private async void btnCheckUpdate_Click(object sender, EventArgs e)
        {
            if (await CheckUpdate())
            {
                DialogResult dialogResult;
                dialogResult = MessageBox.Show(
                                $@"Bạn ơi, phần mềm của bạn có phiên bản mới {fvi.FileVersion}. Phiên bản bạn đang sử dụng hiện tại  {update.Version}. Bạn có muốn cập nhật phần mềm không?", @"Cập nhật phần mềm",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                {
                    try
                    {
                        if (await DownloadNewVersion())
                        {
                            if(UnZip())
                            {
                                string[] fileInstalls = Directory.GetFiles(@"Temp", "*.exe", SearchOption.TopDirectoryOnly);
                                Process.Start(fileInstalls[0]);
                                Application.Exit();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(@"Phiên bản bạn đang sử dụng đã được cập nhật mới nhất.", @"Cập nhật phần mềm",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task<bool> CheckUpdate()
        {
            try
            {
                // Để không bị lỗi: WebException: The request was aborted: Could not create SSL/TLS secure channel.
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)192 |
                                                        (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            }
            catch (NotSupportedException)
            {
            }
            string content = null;
            content = await httpclient.GetStringAsync("https://raw.githubusercontent.com/LuongCongHan/MyAppUpdate/master/MyUpdateApp/update.json");
            //Chuyển đổi Version về dạng dữ liệu đối tượng
            update = JsonConvert.DeserializeObject<UpdateVersion>(content);
            //Lấy thông tin version hiện tại
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            var currentVersion = new Version(fvi.FileVersion);
            var jsonVersion = new Version(update.Version);
            //So sánh Version
            int result = currentVersion.CompareTo(jsonVersion); //Icomparea
            if (result < 0)
                return true;
            else
                return false;
        }

        private async Task<bool> DownloadNewVersion()
        {
            if (!Directory.Exists(@"Temp"))
                Directory.CreateDirectory(@"Temp");
            try
            {
                var stream = await httpclient.GetStreamAsync(update.Url);
                using (var fileStream = File.Create(Path.Combine(@"Temp", Path.GetFileName(update.Url))))
                {
                    stream.CopyTo(fileStream);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool UnZip()
        {
            if (Directory.Exists(Path.Combine(@"Temp", Path.GetFileName(update.Url))))
            {
                try
                {
                    ZipFile.ExtractToDirectory(Path.Combine(@"Temp", Path.GetFileName(update.Url)), @"Temp");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }
    }
}
