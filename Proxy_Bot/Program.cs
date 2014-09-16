using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Net;

using System.IO;
using System.Windows.Forms;



namespace Proxy_Bot
{
     
    class Program
    {

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool settingsReturn, refreshReturn;
        static string [] ip;
        static string [] port;
        static string list = AppDomain.CurrentDomain.BaseDirectory + "/proxy/list.txt";
        static string valid = AppDomain.CurrentDomain.BaseDirectory + "/proxy/valid.txt";
        static string invalid = AppDomain.CurrentDomain.BaseDirectory + "/proxy/invalid.txt";

        static int counter = 0;
        static int timeout = 5000;
        

        static void Main(string[] args)
        {
            exists(list);
            exists(valid);
            exists(invalid);
            readfile();
            run();
            Console.ReadLine();
        }

        static void append(string file,string data)
        {
        using (StreamWriter w = File.AppendText(file))
        {
            w.WriteLine(data);
        }
        }

        static void run()
        {
                foreach (string element in ip)
                {
                    if (validate(timeout))
                    {
                        append(valid,ip[counter] + ':' + port[counter]);
                        Console.WriteLine("VALID)" + counter + '|' + ip[counter] + ':' + port[counter] + ':' + true);
                    }
                    else
                    {
                        append(invalid, ip[counter] + ':' + port[counter]);
                        Console.WriteLine("VALID)" + counter + '|' + ip[counter] + ':' + port[counter] + ':' + false);
                    }
                    counter++;
                }
                counter=0;
        }

        static void exists(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("FILE)" + path);
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("");
                    Console.WriteLine("FILE)" + "fail");
                }
            }
            else
            {
                Console.WriteLine("FILE)" + "open");
            }
        }

        static void readfile()
        {
            string data1 = "";
            string data2 = "";
            using (StreamReader sr = new StreamReader(list))
            {
                while (sr.Peek() >= 0)
                {
                    string[] split = sr.ReadLine().Split(':');
                    data1 = split[0] + "," + data1;
                    data2 = split[1] + "," + data2;
                    Console.WriteLine("LOAD)" + '|' + sr.ReadLine());
                }
                ip = data1.Split(',');
                port = data2.Split(',');
            }
        }

        static void proxy(string ip1,string port1)
        {

            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registry.SetValue("ProxyEnable", 1);
            registry.SetValue("ProxyServer", ip1 + ':' + port1);
            Console.WriteLine("NEW)" + ip1 + ':' + port1);
            settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        static bool validate(int timeout)
        {
            bool OK = false;
            try
            {
                TimeoutWebClient wc = new TimeoutWebClient { Timeout = timeout };
                wc.Proxy = new WebProxy(ip[counter], Convert.ToInt32(port[counter]));
                wc.DownloadString("http://google.com/ncr");
                OK = true;
            }
            catch { }
            return OK;

        }

        static void disable()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registry.SetValue("ProxyEnable", 0);
        }

       
    }

   public class TimeoutWebClient : WebClient
    {
        public int Timeout { get; set; }

        public TimeoutWebClient()
        {
            Timeout = 60000;
        }

        public TimeoutWebClient(int timeout)
        {
            Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            request.Timeout = Timeout;
            return request;
        }
    }
}
