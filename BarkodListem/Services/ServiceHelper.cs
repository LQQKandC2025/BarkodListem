using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BarkodListem.Services
{
    public static class ServiceHelper
    {
        public static T GetService<T>() => MauiApplication.Current.Services.GetService<T>();
    }
}
