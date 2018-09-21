using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;

namespace GasPriceService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        bool isDouble(string s)
        {
            bool b = false;
            double d;
            try
            {
                d = Convert.ToDouble(s);
                b = true;
            }
            catch (Exception e)
            {
                b = false;
            }
            return b;
        }

	//will need to rewrite this if ethgasstation.info changes its html
        void updateprice()
        {

            try
            {
                WebClient webclient1 = new WebClient();
                string result = webclient1.DownloadString("https://ethgasstation.info/index.php");
                string standard = result;
                standard = getfromto(standard, "Standard (<5m)<strong></td>", "</html>");
                standard = getfromto(standard, "#03586A", "</td>");
                standard = standard.Replace("\"", "~");
                standard = standard.Replace("#03586A~>", "");
                standard = standard.Replace("#03586A~ >", "");
                standard = standard.Replace("</td>", "");

                string safelow = result;
                safelow = getfromto(safelow, "SafeLow (<30m)", "</html>");
                safelow = getfromto(safelow, "#1ABB9C", "</td>");
                safelow = safelow.Replace("\"", "~");
                safelow = safelow.Replace("#1ABB9C~>", "");
                safelow = safelow.Replace("#1ABB9C~ >", "");
                safelow = safelow.Replace("</td>", "");

                string fast = result;
                fast = getfromto(fast, "Fast (<2m)", "</html>");
                fast = getfromto(fast, "color:red", "</td>");
                fast = fast.Replace("\"", "~");
                fast = fast.Replace("color:red~>", "");
                fast = fast.Replace("color:red~ >", "");
                fast = fast.Replace("</td>", "");


                if (isDouble(standard) && (isDouble(safelow)) && (isDouble(fast)))
                {
                    runsql("update gasprice set standard=@standard,safelow=@safelow,fast=@fast,lastupdate=getdate() where rownum=1", "@standard", Convert.ToDouble(standard), "@safelow", Convert.ToDouble(safelow), "@fast", Convert.ToDouble(fast));
                }

            }
            catch (Exception e)
            {

            }



        }

        void runsql(string sql, params object[] values)
        {
            string connectionstring = "your_connection_string";
            SqlConnection conn = new SqlConnection(connectionstring);
            conn.Open();
            SqlCommand cm = new SqlCommand(sql, conn);
            int i = 0;
            string paramname = "";
            foreach (object s in values)
            {
                i++;
                if (i % 2 == 1)
                {
                    paramname = s.ToString();
                }
                if (i % 2 == 0)
                {
                    cm.Parameters.AddWithValue(paramname, s);
                }
            }
            cm.ExecuteNonQuery();
            conn.Close();


        }


        private string Left(string param, int length)
        {
            string result = "";
            if (param.Length > 0)
            {
                if (length > param.Length)
                {
                    length = param.Length;
                }

                result = param.Substring(0, length);
            }
            return result;
        }


        private string Right(string param, int length)
        {
            string result = "";
            if (param.Length > 0)
            {
                if (length > param.Length)
                {
                    length = param.Length;
                }

                result = param.Substring(param.Length - length, length);
            }
            return result;
        }



        private string Mid(string param, int start, int length)
        {
            string result = "";
            if ((param.Length > 0) && (start < param.Length))
            {
                if (start + length > param.Length)
                {
                    length = param.Length - start;
                }

                result = param.Substring(start, length);
            }
            return result;
        }


        public string Mid(string param, int startIndex)
        {
            string result = param.Substring(startIndex);
            return result;
        }

        string getfromto(string a, string b, string c)
        {
            string sa1 = "";
            if (a.Length > 0)
            {
                if (a.IndexOf(b) > -1)
                {
                    if (a.Length >= a.IndexOf(b))
                    {
                        sa1 = Right(a, a.Length - a.IndexOf(b));
                    }
                }
            }

            string sb1 = sa1;
            if (sb1.Length > 0)
            {
                if (sb1.IndexOf(c) > -1)
                {
                    if (sb1.IndexOf(c) + c.Length <= sb1.Length)
                    {
                        sb1 = Left(sb1, sb1.IndexOf(c) + c.Length);
                    }
                }
            }

            return sb1;
        }

        
        
        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000;
            timer.Enabled = true;


        }

        protected override void OnStop()
        {            
  

        }

        int ind = 0;
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {

            ind+=5;
            if (ind>=300)
            {
                ind = 0;
                updateprice();
            }
            
        }


    }
}
