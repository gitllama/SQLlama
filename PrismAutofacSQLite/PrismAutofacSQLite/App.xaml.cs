﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PrismAutofacSQLite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IContainer modelcontainer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var modelbuilder = new ContainerBuilder();
            modelbuilder.RegisterInstance(Models.ModelBilder.Build()).SingleInstance();
            modelcontainer = modelbuilder.Build();

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
