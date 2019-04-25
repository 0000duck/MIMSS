using System;
using System.Windows;
using System.Windows.Input;

namespace MyMVVM
{
    public class MyCommand : ICommand
    {

        /// <summary>
        /// 检查命令是否可以执行的事件，在UI事件发生导致控件状态或数据发生变化时触发
        /// 这个add弹窗会在界面还没加载时弹出来，退出界面时弹出去，说明其实在注册事件
        /// CommandManager.RequerySuggested应该是一个总的队列一样的东西
        /// 而且add在你伤害了我，一笑而过后面，说明是return后触发
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {

                    //注释掉这个，那么控件就不能 从 不可选状态转变为可选状态
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    //注释掉这个对整个程序不会产生影响
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        /// 判断命令是否可以执行的方法   返回一个委托对象
        /// 为什么要使用Func? 因为我们要使用的是委托对象，如果不使用Func，那我们就需要先定义一个委托类型，
        /// 然后实例化一个委托对象，而这里可以直接一步到位
        /// 为什么要定义一个委托，因为点击按钮后，实际上是会找到绑定的command的CanExecute和Excute执行，
        /// 如果不使用委托在生成commad对象时动态绑定方法，那么就需要对每一个按钮都实现一个command类
        /// 
        /// </summary>
        private Func<object, bool> _canExecute;

        /// <summary>
        /// 命令需要执行的方法
        /// </summary>
        private Action<object> _execute;

        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="execute">命令要执行的方法</param>
        public MyCommand(Action<object> execute):this(execute,null)
        {
        }

        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="execute">命令要执行的方法</param>
        /// <param name="canExecute">判断命令是否能够执行的方法</param>
        public MyCommand(Action<object> execute, Func<object,bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }


        /// <summary>
        /// 判断命令是否可以执行
        ///</summary>
        /// 如果将这个函数统一返回true，那么原本不能点击的控件会变得能够点击
        ///将xmal文件中的command=""删除掉，那么按钮也会变得可以点击，说明command属性可以控制按钮isable状态(或许是其他不同的属性，但是表现和isable状态被修改一样)
        ///<param name = "parameter" > 命令传入的参数 </ param >
        ///< returns > 是否可以执行 </ returns >
        public bool CanExecute(object parameter)
        {
            //这个函数竟然会一直重复运行。。。。不科学，不科学
            if (_canExecute == null) return true;
            return _canExecute(parameter);
            //return true;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if(_execute != null && CanExecute(parameter))
            {
                _execute(parameter);
            }
        }

   
    }
}
