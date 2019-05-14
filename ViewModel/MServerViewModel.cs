using MyMVVM;
using SocketAsyncEventArgsOfficeDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MIMSS.ViewModel
{
    class MServerViewModel : NotifyObject
    {
        public MServerViewModel()
        {
            Mserver =MServer.CreateInstance(200, 1024);
            BtListenIsable = true;
            BtlistenContent = "Listen";
            TbEndPointText = IPAddress.Any.ToString() + ":" + "5730";
        }

        private MServer mserver;
        public MServer Mserver
        {
            get{ return mserver; }
            set
            {
                    mserver = value;
                    RaisePropertyChanged("Mserver");
            }
        }

        private bool btListenIsable;
        public bool BtListenIsable
        {
            get { return btListenIsable; }
            set
            {
                btListenIsable = value;
                RaisePropertyChanged("BtListenIsable");
            }
        }

        private string btlistenContent;
        public string BtlistenContent
        {
            get { return btlistenContent; }
            set
            {
                btlistenContent = value;
                RaisePropertyChanged("BtlistenContent");
            }
        }

        private string tbEndPointText;
        public string TbEndPointText
        {
            get { return tbEndPointText; }
            set
            {
                tbEndPointText = value;
                RaisePropertyChanged("TbEndPointText");
            }
        }


        private MyCommand btlistenStart;
        public MyCommand BtListenStart
        {
            get
            {
                if (btlistenStart == null)
                    btlistenStart = new MyCommand(
                        new Action<object>(
                            o =>
                            {
                                if (BtlistenContent.Equals("Listen"))
                                {
                                    BtListenIsable = false;
                                    string endPoint = TbEndPointText;
                                    string ip = endPoint.Split(':')[0];
                                    string port = endPoint.Split(':')[1];
                                    Mserver.Init();
                                    Mserver.Start(new IPEndPoint(IPAddress.Parse(ip), int.Parse(port)));
                                    BtlistenContent = "Stop";
                                    BtListenIsable = true;
                                }
                                else
                                {
                                    BtListenIsable = false;
                                    Mserver.Stop();
                                    BtlistenContent = "Listen";
                                    BtListenIsable = true;
                                }
                            }));
                return btlistenStart;
            }
        }

        private MyCommand mainWindowClose;
        public MyCommand MainWindowClose
        {
            get
            {
                if (mainWindowClose == null)
                    mainWindowClose = new MyCommand(
                        new Action<object>(
                            o =>
                            {
                                //如果线程处于运行状态，结束它
                                if ((Mserver.scanThread != null) && (Mserver.scanThread.ThreadState == ThreadState.Running))
                                {
                                    Mserver.scanThread.Abort();
                                }
                                if (Mserver.isStart)
                                {
                                    Mserver.Stop();
                                }
                                System.Environment.Exit(0);
                            }));
                return mainWindowClose;
            }
        }

        //private MyCommand tbLogBoxAdd;
        //public MyCommand TbLogBoxAdd
        //{
        //    get
        //}

    }
}
