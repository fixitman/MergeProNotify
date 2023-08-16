using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MPReceive
{
    internal partial class SendWindowVM: ObservableObject
    {
		[ObservableProperty]
		private string events = "";

		[ObservableProperty]
		private bool isNotifying = false;

        [ObservableProperty]
        private string iP = "172.16.101.199";

        private readonly string PATH = @"\\172.16.2.1\office\mfc\";

        public SendWindowVM()
        {
            var _ip = GetLocalIPAddress();
            if (!string.IsNullOrEmpty(_ip) )
            {
                IP = _ip;
            }
        }

        [RelayCommand]
        private async Task CreateFile()
        {
            var newFileName = new StringBuilder()
                .Append(PATH)
                .Append(string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now))
                .Append(".abc")
                .ToString();
            Log("Creating File " + newFileName);
            try
            {
                var f = File.Create(newFileName);
                f.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("File IO error - " + e.Message);
            }

            if(IsNotifying )
            {
                try
                {
                    Log("Notifying");
                    await NotifyFileCreated();                    
                }
                catch (Exception e)
                {
                    MessageBox.Show("Notify error - " + e.Message);
                }
            }
        }

        private async Task NotifyFileCreated()
        {           
            using var client = new TcpClient();
            client.SendTimeout = 1000;
            await client.ConnectAsync(IP, 12344);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream);
            await writer.WriteLineAsync("CHECK NOW");
            await writer.FlushAsync();
            writer.Close();     
        }

        partial void OnIsNotifyingChanged(bool value)
        {
            var notifyStatus = value ? "On" : "Off";
            Log(string.Format("Notification is {0}", notifyStatus));
           
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address!");
        }

        private void Log(string _event)
        {
            var time = DateTime.Now.ToLongTimeString();
            Events = string.Format("{0} - {1}\r\n{2}", time, _event, Events);

        }
    }
}
