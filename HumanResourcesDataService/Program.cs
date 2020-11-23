using System;
using BusinessManagers;
using Data.HumanResources.DataAccess;
using Data.HumanResources.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HumanResourcesDataService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((b, c) =>
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffK}]info: Initializing application.  Environment: {b.HostingEnvironment.EnvironmentName}");

                    c.AddJsonFile("appsettings.json", false, true);
                    c.AddJsonFile($"appsettings.{b.HostingEnvironment.EnvironmentName}.json", false, true);

                    if (args != null)
                    {
                        c.AddCommandLine(args);
                    }
                })
                .ConfigureLogging((c, l) =>
                {
                    l.ClearProviders();
                    l.AddConfiguration(c.Configuration.GetSection("Logging"));
                    l.AddConsole();
                    if (c.HostingEnvironment.IsDevelopment())
                    {
                        l.AddDebug();
                    }
                })
                .ConfigureServices((c, s) =>
                {
                    // repositories
                    string connString = c.Configuration.GetSection("DBConnectionString").Value;
                    var dbOptions = new DbContextOptionsBuilder<HumanResourcesDataContext>().UseSqlServer(connString);
                    s.AddSingleton(dbOptions.Options);
                    s.AddTransient<IEmployeeInfoRepository, EmployeeInfoRepository>();
                    s.AddTransient<ICompanyInfoRepository, CompanyInfoRepository>();

                    // business logic managers
                    s.AddTransient<IEmployeeInfoManager, EmployeeInfoManager>();
                    s.AddTransient<ICompanyInfoManager, CompanyInfoManager>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
