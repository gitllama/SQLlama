using Autofac;
using Prism.Autofac;
using PrismAutofacSQLite.Views;
using System.Windows;

namespace PrismAutofacSQLite
{
    class Bootstrapper : AutofacBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            var model = App.modelcontainer.Resolve<Models.Model>();
            if(model.GridType == "DataGridView")
                return Container.Resolve<MainWindow>();
            else
                return Container.Resolve<MainDataGridWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }
    }
}
