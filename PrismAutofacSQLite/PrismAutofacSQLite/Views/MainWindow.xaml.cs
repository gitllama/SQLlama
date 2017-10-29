using Prism.Events;
using System.Data;

using System.Windows;

namespace PrismAutofacSQLite.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Messenger : EventAggregator
        {
            private static Messenger _instance;
            public static Messenger Instance { get => _instance ?? (_instance = new Messenger()); }
        }

        public MainWindow()
        {
            InitializeComponent();

            dgvList.RowEnter += DgvList_RowEnter;

            Messenger.Instance.GetEvent<PubSubEvent<object>>().Subscribe(m =>
            {
                dgvList.DataSource = m;
            });
        }

        private void DgvList_RowEnter(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            //Action(e.RowIndex);
            ((dynamic)DataContext).row.Value = e.RowIndex;
        }
    }
}
