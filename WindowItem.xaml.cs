using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RadTreeView
{
    /// <summary>
    /// Логика взаимодействия для WindowItem.xaml
    /// </summary>
    public partial class WindowItem : IDisposable
    {
        public bool Disposable { get; private set; }
        public WindowItem()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            Disposable = true;
            Close();
            DataContext = null;
        }
    }
}
