using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MIMSS.ViewModel.MServerViewModel;

namespace MIMSS.ViewModel
{
    class MainWindowViewModel
    {
        //public DelegateCommand GetMsg
        //{
        //    get { return new DelegateCommand(GetMessage); }
        //}

        public void GetMessage(object parameter)
        {
            MessageBox.Show("Hello World");
        }
    }
}
