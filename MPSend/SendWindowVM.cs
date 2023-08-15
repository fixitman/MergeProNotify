using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Net.Http;
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

        private bool _processing = false;

        private readonly string PATH = @"c:\tmp\";
        private readonly string PATTERN = @"*.abc";

        

        public SendWindowVM()
        {
           
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
            var f = System.IO.File.Create(newFileName);
            f.Close();

            if(isNotifying )
            {
                try
                {
                    Log("Notifying");
                    await NotifyFileCreated();
                    
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }


        }

        private async Task  NotifyFileCreated()
        {
            using var client = new TcpClient();
            await client.ConnectAsync("172.16.101.199", 12344);
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

        private void Log(string _log)
        {
            var time = DateTime.Now.ToLongTimeString();
            Events = string.Format("{0} - {1}\r\n{2}", time, _log, Events);

        }
    }
}
