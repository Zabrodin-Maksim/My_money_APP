using Microsoft.Extensions.DependencyInjection;
using My_money.Data.Repositories;
using My_money.Data.Repositories.IRepositories;
using My_money.Services;
using My_money.Services.IServices;
using My_money.ViewModel;
using My_money.Views;
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

            if (!File.Exists(dbPath))
            {
                InitializeDatabase(connectionString); 
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
            services.AddSingleton<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<AddViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<MoneyBoxViewModel>();
            services.AddTransient<PlanViewModel>();
            #endregion

            services.AddSingleton<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var window = _serviceProvider.GetRequiredService<MainWindow>();

            // Start with DashboardView
            _serviceProvider.GetRequiredService<NavigationService>().Navigate(ViewID.DashboardView);

            window.Show();
        }

        // TODO: ПРИ ПЕРВОЙ ИНИЦИАЛИЗАЦИИ ДОБАВИТЬ В BudgetCategories "Other" для Scope = "Personal" И "Shared"
        private void InitializeDatabase(string connectionString)
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    CREATE TABLE ""BudgetCategories"" (
	                    ""ID""	INTEGER,
	                    ""Name""	TEXT NOT NULL,
	                    ""Plan""	NUMERIC(10, 3),
	                    ""Spend""	NUMERIC(10, 3),
	                    PRIMARY KEY(""ID"" AUTOINCREMENT)
                    );

                    CREATE TABLE ""Records"" (
	                    ""ID""	INTEGER,
	                    ""Cost""	NUMERIC(10, 3) NOT NULL,
	                    ""CategoryId""	INTEGER,
	                    ""DateTimeOccured""	TEXT,
	                    ""Description""	TEXT,
	                    PRIMARY KEY(""ID"" AUTOINCREMENT),
	                    FOREIGN KEY(""CategoryId"") REFERENCES ""BudgetCategories""(""ID"")
                    );

                    CREATE TABLE ""SavingsGoals"" (
	                    ""ID""	INTEGER,
	                    ""GoalName""	TEXT NOT NULL,
	                    ""Have""	NUMERIC(10, 3),
	                    ""Need""	NUMERIC(10, 3),
	                    PRIMARY KEY(""ID"" AUTOINCREMENT)
                    );

                    CREATE TABLE ""UserFinances"" (
	                    ""ID""	INTEGER,
	                    ""Savings""	NUMERIC(10, 3),
	                    ""Balance""	NUMERIC(10, 3),
	                    PRIMARY KEY(""ID"" AUTOINCREMENT)
                    );
                    ";
                    command.ExecuteNonQuery();
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
