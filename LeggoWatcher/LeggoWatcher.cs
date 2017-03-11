using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace LeggoWatcher
{
    public partial class LeggoWatcher : ServiceBase
    {
        public EventLog customEventLog;
        //private string path2watch = "";
        public LeggoWatcher()
        {
            InitializeComponent();
            this.customEventLog = new EventLog();
            this.customEventLog.Source = "LeggoWatcher";
            this.customEventLog.Log = "Application";
            //this.path2watch = path2watch;

            ((ISupportInitialize)(this.customEventLog)).BeginInit();
            if (!EventLog.SourceExists(this.customEventLog.Source))
            {

                EventLog.CreateEventSource(
                    this.customEventLog.Source, this.customEventLog.Log);
                // configure the event log instance to use this source name

            }
            ((ISupportInitialize)(this.customEventLog)).EndInit();
        }

        private System.Threading.Thread _thread, _thread1, _thread2;
        protected override void OnStart(string[] args)
        {
            try
            {
                // Uncomment this line to debug...
                //System.Diagnostics.Debugger.Break();

                // Create the thread object that will do the service's work.
                _thread = new System.Threading.Thread(DoWork);

                // Start the thread.
                _thread.Start();

                // Log an event to indicate successful start.
                //EventLog.WriteEntry("Successful start.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                // Log the exception.
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }

        private void DoWork()
        {
            // Do the service work here...
            //try
            //{
            //Monitor monitorFiles = new Monitor(this.path2watch);
            // Create the thread object that will do the service's work.
            _thread1 = new System.Threading.Thread(start_watcher_thread1);

            // Start the thread.

            _thread2 = new System.Threading.Thread(start_watcher_thread2);

            // Start the thread.
            _thread1.Start();
            _thread2.Start();

            // Log an event to indicate successful start.
            //EventLog.WriteEntry("Successful start.", EventLogEntryType.Information);
            //}
            //catch (Exception ex)
            //{
            //    // Log the exception.
            //    EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            //}
            //Monitor monitorFiles = new Monitor(@"C:\Users\arefe-lenovo\Desktop\test1");
            //Monitor monitorFiles1 = new Monitor(@"C:\Users\arefe-lenovo\Desktop\test1");
            //Monitor monitorFiles2 = new Monitor(@"C:\Windows\test");

        }
        private void start_watcher_thread1()
        {
            Monitor monitorFiles = new Monitor(@"C:\Users\arefe-lenovo\Desktop\test");


        }
        private void start_watcher_thread2()
        {
            Monitor monitorFiles = new Monitor(@"C:\Users\arefe-lenovo\Desktop\test1");


        }
        protected override void OnStop()
        {
            try
            {
                // uncomment this line to debug...
                //system.diagnostics.debugger.break();
                // log an event to indicate successful start.
                //eventlog.writeentry("successful stop.", eventlogentrytype.information);
            }
            catch (Exception ex)
            {
                // Log the exception.
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }
    }
}
