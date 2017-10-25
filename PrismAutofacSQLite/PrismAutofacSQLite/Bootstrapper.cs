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
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }
    }
}
