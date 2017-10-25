using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Set()
        {
            RaisePropertyChanged(nameof(_Goods));
        }

        private List<Goods> _Goods;
        [ReadOnly(true)]
        public List<Goods> Goods
        {
            get { return _Goods; }
            set { SetProperty(ref _Goods, value); }
        }

        public Model()
        {
            var buf = new List<Goods>();
            for (int i = 0; i < 10; i++)
            {
                buf.Add(new Goods()
                {
                    _Name = "商品" + i,
                    _Price = i * 1000,
                    _isAvailable = (i % 2 == 1) ? true : false,
                    _Vender = Vendor.取引先A,
                });
            }
            _Goods = buf; 
            RaisePropertyChanged(nameof(_Goods));
        }
    }

    public class Goods
    {
        public string _Name { get; set; }
        public int _Price { get; set; }
        public bool _isAvailable { get; set; }
        public Vendor _Vender { get; set; }
    }
    public enum Vendor
    {
        取引先A,
        取引先B,
        取引先C,
        取引先D,
    }
}
