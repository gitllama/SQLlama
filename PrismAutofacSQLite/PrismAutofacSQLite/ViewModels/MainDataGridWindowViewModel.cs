using Autofac;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;

namespace PrismAutofacSQLite.ViewModels
{
    public class MainDataGridWindowViewModel : BindableBase
    {
        private string _title = "Prism Autofac DataGrid Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public ReactiveCommand<object> LoadedCommand { get; private set; }
        public ReactiveProperty<object> table { get; private set; }

        public MainDataGridWindowViewModel()
        {

            var model = App.modelcontainer.Resolve<Models.Model>();

            this.table = model.ObserveProperty(x => (object)(x.table)).ToReactiveProperty();

            LoadedCommand = new ReactiveCommand();
            LoadedCommand.Subscribe(_ => model.Init());
        }
    }
}
