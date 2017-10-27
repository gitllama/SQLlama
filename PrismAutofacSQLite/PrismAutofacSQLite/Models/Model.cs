using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.ComponentModel;

namespace PrismAutofacSQLite.Models
{

    public static class ModelBilder
    {
        public static Model Build()
        {
            var path = System.AppDomain.CurrentDomain.BaseDirectory;
            using (var sr = new System.IO.StreamReader(System.IO.Path.Combine(path, "config.yaml")))
            {
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                return deserializer.Deserialize<Model>(sr);
            }
        }
    }

    public class Model : BindableBase
    {
        private string _gridType = "DataGridView";
        [ReadOnly(true)]
        public string GridType
        {
            get { return _gridType; }
            set { SetProperty(ref _gridType, value); }
        }

        private string _sqlFileName = "SQLTEST.db";
        [ReadOnly(true)]
        public string SqlFileName
        {
            get { return _sqlFileName; }
            set { SetProperty(ref _sqlFileName, value); }
        }


        //DataGridView
        // IList : 編集〇追加削除× 
        // IListSource(DataTable、DataSet) : 追加削除〇
        // IBindingList : 編集削除〇追加△(BindingList.AllowNew=true)
        // IBindingListView : 編集追加削除〇
        private object _table;
        public object table
        {
            get { return _table; }
            set { SetProperty(ref _table, value); }
        }


        public string A { get; set; }
        public string InitQuery { get; set; } = "SELECT * FROM 'Lot'";

        public void Init()
        {
            var connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = $"{SqlFileName}"
            };
            try
            {
                using (var con = new SQLiteConnection(connectionString.ToString()))
                {
                    con.Open();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = InitQuery;
                    var reader = cmd.ExecuteReader();
                    
                    //table = reader.GetSchemaTable().DefaultView;

                    var datatable = new System.Data.DataTable();
                    datatable.Load(reader);
                    table = datatable.DefaultView;

                    con.Close(); 
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void Init2()
        {
            var connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = $"{SqlFileName}"
            };
            var con = new SQLiteConnection(connectionString.ToString());
            try
            {
                var context = new DataContext(con);
                var lot = context.GetTable<Lots>();
                var state = context.GetTable<State>();
                var statemaster = context.GetTable<StateMaster>();
                var tags = context.GetTable<Tags>();

                //Linqでnullを処理しようとするとint?とかでないと例外吐く

                //LEFT OUTER JOINで結果の表示
                var buf = 
                    from p in lot
                    join s in state
                    on p.id equals s.id into pGroup
                    from r in pGroup.OrderByDescending(x => x.date).DefaultIfEmpty()
                    select new
                    {
                        id = (int?)p.id,
                        lot = p.Lot,
                        created_date = pGroup.Min(x => (DateTime?)x.date),
                        update = pGroup.Max(x => (DateTime?)x.date),
                        state = (int?)r.state
                    };

                //LEFT OUTER JOINでtagの表示

                table = tags.GroupBy(x => x.id).Select(x => x.ToJoinedString(","));
                return;


                //buf =
                //    from p in buf
                //    join s in tags
                //    on p.id equals s.id into pGroup
                //    //from r in pGroup.DefaultIfEmpty()
                //    from r in pGroup.OrderByDescending(x => x.id).DefaultIfEmpty()
                //    select new
                //    {
                //        id = p.id,
                //        lot = p.lot,
                //        created_date = p.created_date,
                //        update = p.update,
                //        state = p.state,
                //        tags = r.tag
                //    };

                //Masterから値の取り込み
                if (false)
                {
                    //INNER JOIN
                    table =
                        from p in buf
                        join s in statemaster
                        on p.state equals s.id
                        select new
                        {
                            id = p.id,
                            lot = p.lot,
                            created_date = p.created_date,
                            update = p.update,
                            state = s.value,
                            //tag = p.tag
                        };
                }
                else
                {
                    //LEFT OUTER JOIN
                    table =
                        from p in buf
                        join s in statemaster
                        on p.state equals s.id into outerjoin
                        from result in outerjoin.DefaultIfEmpty()
                        select new
                        {
                            id = p.id,
                            lot = p.lot,
                            created_date = p.created_date,
                            update = p.update,
                            state = result.value,
                            //tag = p.tag
                        };
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public object ReadDef()
        {
            return null;
        }

        public void Set2()
        {
            //LINQ で使えるように Entity クラスを作ります。

            string dbConnectionString = "Data Source=SQLTEST.db";
            using (SQLiteConnection cn = new SQLiteConnection(dbConnectionString))
            {
                try
                {
                    cn.Open();
                    SQLiteCommand cmd = cn.CreateCommand();
                    cmd.CommandText = "";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Console.WriteLine("名前:" + reader["id"].ToString());
                            Console.WriteLine(reader.ToString());
                        }
                    }
                    cn.Close();
                }
                catch (Exception e)
                {


                }
            }
        }



    }


    [Table(Name = "Lot")]
    public class Lots
    {
        [Column(Name = "id")]
        public int id { get; set; }

        [Column(Name = "lot")]
        public String Lot { get; set; }

        [Column(Name = "wf")]
        public String Wf { get; set; }

        [Column(Name = "x")]
        public int X { get; set; }

        [Column(Name = "y")]
        public int Y { get; set; }

        [Column(Name = "chip")]
        public int chip { get; set; }
    }


    [Table(Name = "State")]
    public class State
    {
        [Column(Name = "id")]
        public int id { get; set; }

        [Column(Name = "date")]
        public DateTime date { get; set; }

        [Column(Name = "stateid")]
        public int state { get; set; }

        [Column(Name = "note")]
        public string note { get; set; }
    }


    [Table(Name = "StateMaster")]
    public class StateMaster
    {
        [Column(Name = "stateid")]
        public int id { get; set; }

        [Column(Name = "value")]
        public string value { get; set; }
    }

    [Table(Name = "Tags")]
    public class Tags
    {
        [Column(Name = "id")]
        public int id { get; set; }

        [Column(Name = "tag")]
        public string tag { get; set; }
    }

    public static class Ext
    {
        public static string ToJoinedString<T>(this IEnumerable<T> source)
        {
            return source.ToJoinedString("");
        }

        public static string ToJoinedString<T>(this IEnumerable<T> source, string separator)
        {
            var index = 0;
            return source.Aggregate(new StringBuilder(),
                    (sb, o) => (index++ == 0) ? sb.Append(o) : sb.AppendFormat("{0}{1}", separator, o))
                .ToString();
        }
    }
}

                    //tumblrs.InsertOnSubmit(new TTestData { ID = 0, Title = "test" });
                    //context.SubmitChanges();

//foreach (var tumblr in tumblrs)
//    {
//        Console.WriteLine(tumblr.Id);
//    }


//var data = from p in tumblrs select p;