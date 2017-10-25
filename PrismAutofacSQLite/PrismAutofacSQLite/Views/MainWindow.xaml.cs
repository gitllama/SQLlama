using System.Data;

using System.Windows;

namespace PrismAutofacSQLite.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            int col = 200;
            DataTable dt = new DataTable();
            for (int i = 0; i < col; i++)
            {
                dt.Columns.Add(i.ToString());
            }

            for (int i = 0; i < 24 * 100; i++)
            {
                dt.Rows.Add(dt.NewRow());
                for (int j = 0; j < col; j++)
                {
                    dt.Rows[i][j] = i;
                }
            }

            dgvList.DataSource = dt;
        }
    }
}
