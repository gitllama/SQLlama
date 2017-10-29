using Autofac;
using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;

namespace PrismAutofacSQLite.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Autofac DataGridView Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public ReactiveCommand<object> LoadedCommand { get; private set; }
        //public ReactiveProperty<object> table { get; private set; }

        public ReactiveProperty<int> row { get; private set; }

        public MainWindowViewModel()
        {
            var model = App.modelcontainer.Resolve<Models.Model>();

            //this.table = model.ObserveProperty(x => (object)(x.table)).ToReactiveProperty();
            model.ObserveProperty(x => x.table).Subscribe(x =>
            {
                Views.MainWindow.Messenger.Instance.GetEvent<PubSubEvent<object>>().Publish(x);
            });

            LoadedCommand = new ReactiveCommand();
            LoadedCommand.Subscribe(_ => model.Init() );

            row = model.ToReactivePropertyAsSynchronized(x => x.Row);
        }
    }
}

