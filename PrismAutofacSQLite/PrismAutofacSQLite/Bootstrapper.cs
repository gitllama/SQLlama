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
#if DATAGRID
            return Container.Resolve<MainDataGridWindow>();
#else
            return Container.Resolve<MainWindow>();
#endif
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }
    }
}
