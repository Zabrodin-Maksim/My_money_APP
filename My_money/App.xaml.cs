using Microsoft.Extensions.DependencyInjection;
using My_money.Data.Repositories;
using My_money.Data.Repositories.IRepositories;
using My_money.Services;
using My_money.Services.IServices;
using My_money.ViewModel;
using My_money.Views;
using System;
using System.IO;
using System.Data.SQLite;
using System.Windows;

namespace My_money
{
    public partial class App : Application
    {
        public static ServiceProvider _serviceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDir = Path.Combine(appData, "My_money");
            Directory.CreateDirectory(appDir);

            string dbPath = Path.Combine(appDir, "DB.db");
            string connectionString = $"Data Source={dbPath};Version=3;";

            if (!File.Exists(dbPath))
            {
                InitializeDatabase(connectionString);
            }

            var services = new ServiceCollection();

            services.AddSingleton<IUserRepository>(new UserRepository(connectionString));
            services.AddSingleton<IHouseholdRepository>(new HouseholdRepository(connectionString));
            services.AddSingleton<IHouseholdMemberRepository>(new HouseholdMemberRepository(connectionString));
            services.AddSingleton<IHouseholdFinanceRepository>(new HouseholdFinanceRepository(connectionString));
            services.AddSingleton<IBudgetCategoryRepository>(new BudgetCategoryRepository(connectionString));
            services.AddSingleton<IRecordRepository>(new RecordRepository(connectionString));
            services.AddSingleton<ISavingsGoalRepository>(new SavingsGoalRepository(connectionString));
            services.AddSingleton<IUserFinanceRepository>(new UserFinanceRepository(connectionString));

            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IHouseholdService, HouseholdService>();
            services.AddSingleton<IHouseholdMemberService, HouseholdMemberService>();
            services.AddSingleton<IHouseholdFinanceService, HouseholdFinanceService>();
            services.AddSingleton<IUserFinanceService, UserFinanceService>();
            services.AddSingleton<IBudgetCategoryService, BudgetCategoryService>();
            services.AddSingleton<IRecordService, RecordService>();
            services.AddSingleton<ISavingsGoalService, SavingsGoalService>();
            services.AddSingleton<IFinancialHealthScoreService, FinancialHealthScoreService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IRegistrationService, RegistrationService>();
            services.AddSingleton<IPasswordResetService, PasswordResetService>();
            services.AddSingleton<NavigationService>();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegistrationViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<AddViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<MoneyBoxViewModel>();
            services.AddTransient<PlanViewModel>();
            services.AddTransient<HouseholdMembersViewModel>();
            services.AddTransient<SettingsViewModel>();

            services.AddSingleton<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var window = _serviceProvider.GetRequiredService<MainWindow>();
            var navigation = _serviceProvider.GetRequiredService<NavigationService>();
            var userService = _serviceProvider.GetRequiredService<IUserService>();

            var hasUsers = userService.GetAllUsersAsync().GetAwaiter().GetResult().Count > 0;
            navigation.Navigate(hasUsers ? ViewID.LoginView : ViewID.RegistrationView);

            window.Show();
        }

        private void InitializeDatabase(string connectionString)
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS ""Users"" (
                    ""ID"" INTEGER NOT NULL UNIQUE,
                    ""Email"" TEXT NOT NULL UNIQUE,
                    ""PasswordHash"" TEXT NOT NULL,
                    ""DisplayName"" TEXT NOT NULL,
                    ""IsActive"" INTEGER NOT NULL DEFAULT 1,
                    PRIMARY KEY(""ID"" AUTOINCREMENT)
                );

                CREATE TABLE IF NOT EXISTS ""Households"" (
                    ""ID"" INTEGER NOT NULL UNIQUE,
                    ""Name"" TEXT NOT NULL,
                    ""CreatedByUserId"" INTEGER NOT NULL,
                    PRIMARY KEY(""ID"" AUTOINCREMENT),
                    FOREIGN KEY(""CreatedByUserId"") REFERENCES ""Users""(""ID"")
                );

                CREATE TABLE IF NOT EXISTS ""HouseholdMembers"" (
                    ""ID"" INTEGER NOT NULL,
                    ""HouseholdId"" INTEGER NOT NULL,
                    ""UserId"" INTEGER NOT NULL UNIQUE,
                    ""Role"" TEXT NOT NULL CHECK(""Role"" IN ('Admin', 'Partner', 'Child')),
                    ""CanManageBudget"" INTEGER NOT NULL DEFAULT 0 CHECK(""CanManageBudget"" IN (0, 1)),
                    ""CanManageMembers"" INTEGER NOT NULL DEFAULT 0 CHECK(""CanManageMembers"" IN (0, 1)),
                    PRIMARY KEY(""ID"" AUTOINCREMENT),
                    FOREIGN KEY(""HouseholdId"") REFERENCES ""Households""(""ID""),
                    FOREIGN KEY(""UserId"") REFERENCES ""Users""(""ID"")
                );

                CREATE TABLE IF NOT EXISTS ""HouseholdFinances"" (
                    ""ID"" INTEGER NOT NULL,
                    ""HouseholdId"" INTEGER NOT NULL UNIQUE,
                    ""Savings"" NUMERIC NOT NULL DEFAULT 0,
                    ""Balance"" NUMERIC NOT NULL DEFAULT 0,
                    PRIMARY KEY(""ID"" AUTOINCREMENT),
                    FOREIGN KEY(""HouseholdId"") REFERENCES ""Households""(""ID"")
                );

                CREATE TABLE IF NOT EXISTS ""UserFinances"" (
                    ""ID"" INTEGER NOT NULL,
                    ""Savings"" NUMERIC NOT NULL DEFAULT 0,
                    ""Balance"" NUMERIC NOT NULL DEFAULT 0,
                    ""UserId"" INTEGER NOT NULL UNIQUE,
                    PRIMARY KEY(""ID"" AUTOINCREMENT),
                    FOREIGN KEY(""UserId"") REFERENCES ""Users""(""ID"")
                );

                CREATE TABLE IF NOT EXISTS ""BudgetCategories"" (
                    ""ID"" INTEGER NOT NULL,
                    ""Name"" TEXT NOT NULL,
                    ""Plan"" NUMERIC NOT NULL DEFAULT 0,
                    ""HouseholdId"" INTEGER NOT NULL,
                    ""OwnerUserId"" INTEGER,
                    ""Scope"" TEXT NOT NULL CHECK(( ""Scope"" = 'Personal' AND ""OwnerUserId"" IS NOT NULL) OR (""Scope"" = 'Shared' AND ""OwnerUserId"" IS NULL)),
                    ""CreatedByUserId"" INTEGER NOT NULL,
                    PRIMARY KEY(""ID"" AUTOINCREMENT),
                    FOREIGN KEY(""CreatedByUserId"") REFERENCES ""Users""(""ID""),
                    FOREIGN KEY(""HouseholdId"") REFERENCES ""Households""(""ID""),
                    FOREIGN KEY(""OwnerUserId"") REFERENCES ""Users""(""ID"")
                );

                CREATE TABLE IF NOT EXISTS ""Records"" (
                    ""ID"" INTEGER NOT NULL,
                    ""Amount"" NUMERIC NOT NULL,
                    ""CategoryId"" INTEGER,
                    ""DateTimeOccured"" TEXT,
                    ""Description"" TEXT,
                    ""HouseholdId"" INTEGER NOT NULL,
                    ""OwnerUserId"" INTEGER,
                    ""CreatedByUserId"" INTEGER NOT NULL,
                    ""Scope"" TEXT NOT NULL CHECK(( ""Scope"" = 'Personal' AND ""OwnerUserId"" IS NOT NULL) OR (""Scope"" = 'Shared' AND ""OwnerUserId"" IS NULL)),
                    ""Type"" TEXT NOT NULL CHECK(""Type"" IN ('Expense', 'Income')),
                    ""IncomeTarget"" TEXT CHECK(( ""Type"" = 'Income' AND ""IncomeTarget"" IN ('Balance', 'Savings')) OR (""Type"" = 'Expense' AND ""IncomeTarget"" IS NULL)),
                    PRIMARY KEY(""ID"" AUTOINCREMENT),
                    FOREIGN KEY(""CategoryId"") REFERENCES ""BudgetCategories""(""ID""),
                    FOREIGN KEY(""CreatedByUserId"") REFERENCES ""Users""(""ID""),
                    FOREIGN KEY(""HouseholdId"") REFERENCES ""Households""(""ID""),
                    FOREIGN KEY(""OwnerUserId"") REFERENCES ""Users""(""ID"")
                );

                CREATE TABLE IF NOT EXISTS ""SavingsGoals"" (
                    ""ID"" INTEGER NOT NULL,
                    ""GoalName"" TEXT NOT NULL,
                    ""Have"" NUMERIC NOT NULL DEFAULT 0,
                    ""Need"" NUMERIC NOT NULL DEFAULT 0,
                    ""HouseholdId"" INTEGER NOT NULL,
                    ""OwnerUserId"" INTEGER,
                    ""CreatedByUserId"" INTEGER NOT NULL,
                    ""Scope"" TEXT NOT NULL CHECK(( ""Scope"" = 'Personal' AND ""OwnerUserId"" IS NOT NULL) OR (""Scope"" = 'Shared' AND ""OwnerUserId"" IS NULL)),
                    PRIMARY KEY(""ID"" AUTOINCREMENT),
                    FOREIGN KEY(""CreatedByUserId"") REFERENCES ""Users""(""ID""),
                    FOREIGN KEY(""HouseholdId"") REFERENCES ""Households""(""ID""),
                    FOREIGN KEY(""OwnerUserId"") REFERENCES ""Users""(""ID"")
                );
            ";
            command.ExecuteNonQuery();

            EnsureOtherCategories(connection);
        }

        private static void EnsureOtherCategories(SQLiteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ""BudgetCategories"" (""Name"", ""Plan"", ""HouseholdId"", ""OwnerUserId"", ""Scope"", ""CreatedByUserId"")
                SELECT 'Other', 0, h.""ID"", NULL, 'Shared', h.""CreatedByUserId""
                FROM ""Households"" h
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ""BudgetCategories"" bc
                    WHERE bc.""HouseholdId"" = h.""ID""
                      AND bc.""Scope"" = 'Shared'
                      AND bc.""OwnerUserId"" IS NULL
                      AND bc.""Name"" = 'Other'
                );

                INSERT INTO ""BudgetCategories"" (""Name"", ""Plan"", ""HouseholdId"", ""OwnerUserId"", ""Scope"", ""CreatedByUserId"")
                SELECT 'Other', 0, hm.""HouseholdId"", u.""ID"", 'Personal', u.""ID""
                FROM ""Users"" u
                INNER JOIN ""HouseholdMembers"" hm ON hm.""UserId"" = u.""ID""
                WHERE hm.""Role"" <> 'Child'
                  AND NOT EXISTS (
                    SELECT 1
                    FROM ""BudgetCategories"" bc
                    WHERE bc.""HouseholdId"" = hm.""HouseholdId""
                      AND bc.""Scope"" = 'Personal'
                      AND bc.""OwnerUserId"" = u.""ID""
                      AND bc.""Name"" = 'Other'
                );
            ";
            command.ExecuteNonQuery();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
