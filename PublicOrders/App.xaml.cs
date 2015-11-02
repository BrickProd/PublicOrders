using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PublicOrders
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
            if (mvm.cdProcessor != null) mvm.cdProcessor.Stop();
            if (mvm.csProcessor != null) mvm.csProcessor.Stop();
            if (mvm.cwProcessor != null) mvm.cwProcessor.Stop();
            if (mvm.lpProcessor != null) mvm.lpProcessor.Stop();
            if (mvm.lsProcessor != null) mvm.lsProcessor.Stop();
        }
    }
}