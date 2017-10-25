using Autofac;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;

namespace PrismAutofacSQLite.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Autofac Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public ReactiveCommand<object> LoadedCommand { get; private set; }
        public ReactiveProperty<object> griddata { get; private set; }

        public MainWindowViewModel()
        {
            var model = App.modelcontainer.Resolve<Models.Model>();

            this.griddata = model.ObserveProperty(x => (object)(x.Goods)).ToReactiveProperty();

            LoadedCommand = new ReactiveCommand();
            LoadedCommand.Subscribe(_ =>
            {
                model.Set();
            });
        }
    }
}
