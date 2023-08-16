using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Threading;

namespace MPReceive
{
    internal partial class ReceiveWindowVM: ObservableObject
    {
        private DispatcherTimer Timer { get; set; }

        private Listener Listener;

		[ObservableProperty]
		private string events = "";

		[ObservableProperty]
		private bool isListening = false;

        [ObservableProperty]
        private int interval = 3;

        private bool _processing = false;

        private readonly string PATH = @"\\172.16.2.1\office\mfc\";
        private readonly string PATTERN = @"*.abc";

        public ReceiveWindowVM()
        {
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(Interval);
            Timer.Tick += new EventHandler(Tick);
            Timer.Start();
            
            Listener = new Listener(ListenerCallback, 12344);
            if(isListening) Listener.Start();
        }

        partial void OnIsListeningChanged(bool value)
        {
            var listeningStatus = value ? "On" : "Off";
            Log(string.Format("Listening is {0}", listeningStatus));
            if(value == true)
            {
                Listener.Start();
            }
            else
            {
                Listener.Stop();
            }
        }

        partial void OnIntervalChanged(int value)
        {
            Timer.Interval = TimeSpan.FromSeconds(value);
        }

        void Tick(object? sender, EventArgs e)
        {
            if (!_processing)
            {
                _processing = true;
                
                if(e.GetType() == typeof(CheckEventArgs))
                {
                    Log("Check forced");
                }
                else
                {
                    Log("Checking... ");
                }

                string? filename = LookForFile();

                if(filename !=null && filename.Length > 0)
                {
                    Log("Found " + filename);
                    ProcessFile(filename);
                }

                _processing = false;
            }
        }

        private void ProcessFile(string filename)
        {
            //Do something with the file 
            Log("Processing " + filename);
            
            //rename file
            string newFileName = filename.Substring(0, filename.Length - 3)+"cba";
            System.IO.File.Move(filename, newFileName);
        }

        private string? LookForFile()
        {
            string[] files = System.IO.Directory.GetFiles(PATH,PATTERN);
            if(files.Length > 0)
            {
                return files[0];
            }
            return null;
        }

        void ListenerCallback()
        {
            var e = new CheckEventArgs();
            Tick(this,e);
        }

        private void Log(string _log)
        {
            var time = DateTime.Now.ToLongTimeString();
            Events = string.Format("{0} - {1}\r\n{2}", time, _log, Events);

        }
    }
}
