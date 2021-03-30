using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Lab1._2
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();

        //điền tên và path process mong muốn
        public static string processname = "Lightshot";
        public static string PATH = "C:\\Program Files (x86)\\Skillbrains\\lightshot\\5.5.0.7\\Lightshot.exe";
        public int processId = 0;

        //setup thời gian bắt đầu chạy và tắt chương trình
        public DateTime start = new DateTime(2021, 3, 30, 14, 26, 00);
        public DateTime stop = new DateTime(2021, 3, 30, 14, 27, 00);
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000;
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            //Kiểm tra chương trình đang ở trạng thái nào
            if (checkservicestatus())
                WriteToFile(DateTime.Now + ": " + processname + " running");
            else
                WriteToFile(DateTime.Now + ": " + processname + " not running");

            //thực hiện bật tắt chương trình theo lịch trình
            setschedule();
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
            "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
            ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
        public bool checkservicestatus()
        {
            //lấy list thông tin tất cả các process đang chạy
            Process[] processlist = Process.GetProcesses();
            int flag = 0;
            
            //Kiểm tra process có đang trong list process hay không
            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName == "Lightshot")
                {
                    flag += 1;
                    
                    //lấy process id nếu process đang chạy
                    processId = theprocess.Id;
                }
            }
            if (flag > 0)
                return true;
            else
                return false;

        }
        public void setschedule()
        {
            //So sánh dòng thời gian hiện tại và start/stop để khoanh vùng thời gian sẽ chạy chương trình
            //Example: nằm trong khoảng thời gian xác định trước-chương trình sẽ chạy, còn ngoài khoảng thời gian này chương trình sẽ tự động đóng
            int result_start = DateTime.Compare(DateTime.Now, start);
            int result_stop = DateTime.Compare(DateTime.Now, stop);
            if (result_start > 0 && result_stop < 0)
            {
                if (checkservicestatus() == false)
                    processId = Start(PATH);
            }
            else
            {
                if (checkservicestatus() == true)
                    Stop(processId);
            }
        }

        //Mở process dựa trên processPath
        public static int Start(string processPath)
        {
            var process =
                Process.Start(processPath);
            return process.Id;
        }

        //Tắt process dựa trên process ID
        public static void Stop(int processId)
        {
            var process =
                Process.GetProcessById(processId);
            process.Kill();
        }
    }
}
