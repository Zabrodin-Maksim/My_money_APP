using Microsoft.Extensions.DependencyInjection;
using My_money.Data.Repositories;
using My_money.Data.Repositories.IRepositories;
using My_money.Services;
using My_money.Services.IServices;
using My_money.ViewModel;
using System;
using System.IO;
using System.Windows;

namespace My_money
{
    public partial class App : Application
    {
        public static ServiceProvider _serviceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            #region DB directory

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDir = Path.Combine(appData, "My_money");
            Directory.CreateDirectory(appDir);

            string dbPath = Path.Combine(appDir, "DB.db");
            string connectionString = $"Data Source={dbPath};Version=3;";

            string seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "DB.db");
            if (!File.Exists(dbPath) && File.Exists(seedPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                File.Copy(seedPath, dbPath);
            }

            #endregion

            var services = new ServiceCollection();

            #region Repositories
            services.AddSingleton<IBudgetCategoryRepository>(new BudgetCategoryRepository(connectionString));
            services.AddSingleton<IRecordRepository>(new RecordRepository(connectionString));
            services.AddSingleton<ISavingsGoalRepository>(new SavingsGoalRepository(connectionString));
            services.AddSingleton<IUserFinanceRepository>(new UserFinanceRepository(connectionString));
            #endregion

            #region Services
            services.AddSingleton<IBudgetCategoryService, BudgetCategoryService>();
            services.AddSingleton<IRecordService, RecordService>();
            services.AddSingleton<ISavingsGoalService, SavingsGoalService>();
            services.AddSingleton<IUserFinanceService, UserFinanceService>();
            services.AddSingleton<NavigationService>();
            #endregion

            #region ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<AddViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<MoneyBoxViewModel>();
            services.AddTransient<PlanViewModel>();
            #endregion

            _serviceProvider = services.BuildServiceProvider();

            var window = _serviceProvider.GetRequiredService<MainWindow>();
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
