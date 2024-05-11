using Automation.BDaq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCI1753Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitPCI1753();
        }
        InstantDiCtrl instantDiCtrl;  
        ErrorCode errorCode = ErrorCode.Success; //错误代码
        //string deviceDescription = "PCIE-1753,BID#0"; //设备描述
        string deviceDescription = "DemoDevice,BID#2"; //设备描述
        int startPort = 0;  //起始通道
        int portCount = 12; //通道数
        byte[] buffer = new byte[64];

        private void InitPCI1753()
        {
            instantDiCtrl = new InstantDiCtrl(); //创建一个InstantDiCtrl对象
            try
            {
                instantDiCtrl.SelectedDevice = new DeviceInformation(deviceDescription); //选择获取设备

                errorCode = instantDiCtrl.SnapStart();
                if (BioFailed(errorCode))
                {
                    throw new Exception();
                }

                PCI1753_Read();  
            }
            catch (Exception ex)
            {
                string errStr = BioFailed(errorCode) ? " Some error occurred. And the last error code is " + errorCode.ToString()
                                                         : ex.Message;
                Console.WriteLine(errStr);
            }
        }

        private void PCI1753_Read()
        {
            ThreadStart plc = new ThreadStart(Read);
            Thread ip = new Thread(plc);
            ip.IsBackground = true;
            ip.Start();
        }

        private void Read()
        {
            try
            {
                while (true)
                {
                    errorCode = instantDiCtrl.Read(startPort, portCount, buffer); //从portStart端口到portCount端口，读取的值存至buffer数组中

                    if (BioFailed(errorCode))
                    {
                        throw new Exception();
                    }
                    for (int i = 0; i < portCount; ++i)
                    {
                        Console.WriteLine(" DI port {0} status : 0x{1:x}\n", startPort + i, buffer[i]);
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static bool BioFailed(ErrorCode err)
        {
            return err < ErrorCode.Success && err >= ErrorCode.ErrorHandleNotValid;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            instantDiCtrl.Dispose(); 
        }
    }
}
