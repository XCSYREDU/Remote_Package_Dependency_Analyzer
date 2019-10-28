using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Navigator
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App:Application
  {
       private void poppop(object sender, StartupEventArgs e)
        {
            MessageBox.Show("Use Connect to initialize the remote data.\nUse importall to import all .cs files in current directory.\nUse importselect to import files in select directory");
        }
  }
}
