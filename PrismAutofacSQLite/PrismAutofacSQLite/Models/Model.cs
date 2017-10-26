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
        private int _height = 1;
        [ReadOnly(true)]
        public int Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }


        private object _table;
        public object table
        {
            get { return _table; }
            set { SetProperty(ref _table, value); }
        }


        public string A { get; set; }
        public string query { get; set; } = "SELECT * FROM 'Lot' INNER JOIN 'Status'";


        public void Set()
        {
            //LINQ で使えるように Entity クラスを作ります。

            string dbConnectionString = "Data Source=SQLTEST.db";
            using (SQLiteConnection cn = new SQLiteConnection(dbConnectionString))
            {
                try
                {
                    cn.Open();
                    SQLiteCommand cmd = cn.CreateCommand();
                    cmd.CommandText = query;
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


        /****/
        public object Set2()
        {
            var connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = "SQLTEST.db",
            };

            var con = new SQLiteConnection(connectionString.ToString());

            try
            {
                var context = new DataContext(con);
                var lot = context.GetTable<Lots>();
                var status = context.GetTable<Status>();
                var statusmaster = context.GetTable<StatusMaster>();

                //tumblrs.InsertOnSubmit(new TTestData { ID = 0, Title = "test" });
                //context.SubmitChanges();

                //foreach (var tumblr in tumblrs)
                //    {
                //        Console.WriteLine(tumblr.Id);
                //    }


                //var data = from p in tumblrs select p;

                var kekka =
                    from x in status
                    group x by new { x.Id } into g
                    orderby g.Key.Id
                    select new
                    {
                        ID = g.Key.Id,
                        A = g.Max(p => p.date),
                    };




                var date =
                    from p in lot
                    join j in status on p.Id equals j.Id
                    select new
                    {
                        Id = p.Id,
                        Lot = p.Lot,
                        Wf = p.Wf,
                        status = j.status,
                        note = j.note
                    };

                var data1 =
                    from p in lot
                    join j in status on p.Id equals j.Id
                    select new
                    {
                        Id = p.Id,
                        Lot = p.Lot,
                        Wf = p.Wf,
                        status = j.status,
                        note = j.note
                    };
                var data2 =
                    from p in data1
                    join j in statusmaster on p.status equals j.Id
                    select new
                    {
                        Id = p.Id,
                        Lot = p.Lot,
                        Wf = p.Wf,
                        status = j.value,
                        note = p.note
                    };
                table = kekka;
                return kekka;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                return null;
            }
        }

    }


    [Table(Name = "Lot")]
    public class Lots
    {
        [Column(Name = "Id")]
        public int Id { get; set; }

        [Column(Name = "Lot")]
        public String Lot { get; set; }

        [Column(Name = "Wf")]
        public String Wf { get; set; }

        [Column(Name = "X")]
        public int X { get; set; }

        [Column(Name = "Y")]
        public int Y { get; set; }
    }


    [Table(Name = "Status")]
    public class Status
    {
        [Column(Name = "id")]
        public int Id { get; set; }

        [Column(Name = "date")]
        public DateTime date { get; set; }

        [Column(Name = "status")]
        public int status { get; set; }

        [Column(Name = "note")]
        public string note { get; set; }
    }


    [Table(Name = "StatusMaster")]
    public class StatusMaster
    {
        [Column(Name = "StatusID")]
        public int Id { get; set; }

        [Column(Name = "Value")]
        public string value { get; set; }
    }
}

