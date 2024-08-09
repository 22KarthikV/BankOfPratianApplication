using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.DataAccess;
using NLog;
using System.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using NLog.Config;
using NLog.Targets;
using BankOfPratian.Core.Exceptions;
using System;

namespace BankOfPratian.Console
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static AccountManager _accountManager;
        private static ExternalTransferService _externalTransferService;
        //private static IAccountManager _accountManager;

        static void Main(string[] args)
        {

            
            try
            {
                // Initialize NLog
                var config = new LoggingConfiguration();

                // Targets where to log to: File and Console
                var logfile = new FileTarget("logfile") { FileName = "${basedir}/logs/${shortdate}.log" };
                var logconsole = new ConsoleTarget("logconsole");

                // Rules for mapping loggers to targets            
                config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

                // Apply config           
                LogManager.Configuration = config;

                var logger = LogManager.GetCurrentClassLogger();
                logger.Info("Application Starting...");
                LogManager.LoadConfiguration("NLog.config");


                ConfigureServices();
                //TestDependencyResolution();
                RunApplication();

                logger.Info("Application shutting down normally");
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<AccountPrivilegeManager>();
            services.AddSingleton<IAccountDAO, AccountDAO>();
            services.AddSingleton<ITransactionDAO, TransactionDAO>();
            services.AddSingleton<IPolicyFactory>(provider => PolicyFactory.Instance);
            services.AddSingleton<AccountManager>();
            services.AddSingleton<ExternalTransferService>();

            // Configure settings
            services.AddSingleton<IConfiguration>(provider =>
            {
                var configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                return configurationBuilder.Build();
            });

            // Register DAO implementations
            services.AddSingleton<IAccountDAO>(s => new AccountDAO(ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString));
            services.AddSingleton<ITransactionDAO>(s => new TransactionDAO(ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString));

            // Register PolicyFactory as a singleton
            services.AddSingleton<IPolicyFactory>(s => PolicyFactory.Instance);

            // Register other services
            services.AddSingleton<AccountPrivilegeManager>();
            services.AddSingleton<AccountManager>();
            services.AddSingleton<ExternalTransferService>();

            

            _serviceProvider = services.BuildServiceProvider();
        }






        private static void RunApplication()
        {
            _accountManager = _serviceProvider.GetRequiredService<AccountManager>();
            _externalTransferService = _serviceProvider.GetRequiredService<ExternalTransferService>();

            _externalTransferService.Start();

            while (true)
            {
                if (!ShowMainMenu())
                {
                    break;
                }
            }

            _externalTransferService.Stop();
        }

        private static bool ShowMainMenu()
        {
            System.Console.Clear();
            System.Console.WriteLine("BankOfPratian Application");
            System.Console.WriteLine("1. Account Management");
            System.Console.WriteLine("2. Transaction Management");
            System.Console.WriteLine("3. Reports");
            System.Console.WriteLine("4. Exit");
            System.Console.Write("Enter your choice: ");

            if (int.TryParse(System.Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1:
                        ShowAccountManagementMenu();
                        break;
                    case 2:
                        ShowTransactionManagementMenu();
                        break;
                    case 3:
                        ShowReportsMenu();
                        break;
                    case 4:
                        return false;
                    default:
                        System.Console.WriteLine("Invalid choice. Press any key to continue.");
                        System.Console.ReadKey();
                        break;
                }
            }
            else
            {
                System.Console.WriteLine("Invalid input. Press any key to continue.");
                System.Console.ReadKey();
            }

            return true;
        }
        private static void ShowAccountManagementMenu()
        {
            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("Account Management");
                System.Console.WriteLine("1. Create Account");
                System.Console.WriteLine("2. Display Account Information");
                System.Console.WriteLine("3. Back to Main Menu");
                System.Console.Write("Enter your choice: ");

                if (int.TryParse(System.Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            CreateAccount();
                            break;
                        case 2:
                            DisplayAccountInfo();
                            break;
                        case 3:
                            return;
                        default:
                            System.Console.WriteLine("Invalid choice. Press any key to continue.");
                            System.Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    System.Console.WriteLine("Invalid input. Press any key to continue.");
                    System.Console.ReadKey();
                }
            }
        }

        private static void ShowTransactionManagementMenu()
        {
            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("Transaction Management");
                System.Console.WriteLine("1. Deposit");
                System.Console.WriteLine("2. Withdraw");
                System.Console.WriteLine("3. Transfer Funds");
                System.Console.WriteLine("4. Back to Main Menu");
                System.Console.Write("Enter your choice: ");

                if (int.TryParse(System.Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            Deposit();
                            break;
                        case 2:
                            Withdraw();
                            break;
                        case 3:
                            TransferFunds();
                            break;
                        case 4:
                            return;
                        default:
                            System.Console.WriteLine("Invalid choice. Press any key to continue.");
                            System.Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    System.Console.WriteLine("Invalid input. Press any key to continue.");
                    System.Console.ReadKey();
                }
            }
        }

        private static void ShowReportsMenu()
        {
            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("Reports");
                System.Console.WriteLine("1. Display Bank Statistics");
                System.Console.WriteLine("2. Display All Transactions");
                System.Console.WriteLine("3. Back to Main Menu");
                System.Console.Write("Enter your choice: ");

                if (int.TryParse(System.Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            DisplayBankStatistics();
                            break;
                        case 2:
                            DisplayAllTransactions();
                            break;
                        case 3:
                            return;
                        default:
                            System.Console.WriteLine("Invalid choice. Press any key to continue.");
                            System.Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    System.Console.WriteLine("Invalid input. Press any key to continue.");
                    System.Console.ReadKey();
                }
            }
        }



        private static void CreateAccount()
        {
            try
            {
                System.Console.Write("Enter name: ");
                string name = System.Console.ReadLine();

                System.Console.Write("Enter PIN (4 digits): ");
                string pin = System.Console.ReadLine();
                if (pin.Length != 4 || !int.TryParse(pin, out _))
                {
                    throw new ArgumentException("PIN must be 4 digits.");
                }

                System.Console.Write("Enter initial balance: ");
                if (!double.TryParse(System.Console.ReadLine(), out double balance) || balance < 0)
                {
                    throw new ArgumentException("Invalid balance amount.");
                }

                System.Console.Write("Enter privilege type (REGULAR/GOLD/PREMIUM): ");
                if (!Enum.TryParse<PrivilegeType>(System.Console.ReadLine(), true, out PrivilegeType privilegeType))
                {
                    throw new ArgumentException("Invalid privilege type.");
                }

                System.Console.Write("Enter account type (SAVINGS/CURRENT): ");
                if (!Enum.TryParse<AccountType>(System.Console.ReadLine(), true, out AccountType accountType))
                {
                    throw new ArgumentException("Invalid account type.");
                }

                IAccount account = _accountManager.CreateAccount(name, pin, balance, privilegeType, accountType);
                System.Console.WriteLine($"Account created successfully. Account number: {account.AccNo}");
            }
            catch (MinBalanceNeedsToBeMaintainedException ex)
            {
                System.Console.WriteLine($"Error creating account: {ex.Message}");
                Logger.Error(ex, "Error creating account");
            }
            catch (UnableToOpenAccountException ex)
            {
                System.Console.WriteLine($"Error creating account: {ex.Message}");
                Logger.Error(ex, "Error creating account");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error creating account: {ex.Message}");
                Logger.Error(ex, "Error creating account");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        private static void DisplayAccountInfo()
        {
            try
            {
                System.Console.Write("Enter account number: ");
                string accNo = System.Console.ReadLine();

                IAccount account = _accountManager.GetAccount(accNo);
                if (account == null)
                {
                    throw new ArgumentException("Account not found.");
                }

                System.Console.WriteLine($"Account Number: {account.AccNo}");
                System.Console.WriteLine($"Name: {account.Name}");
                System.Console.WriteLine($"Account Type: {account.GetAccType()}");
                System.Console.WriteLine($"Balance: {account.Balance:C}");
                System.Console.WriteLine($"Privilege Type: {account.PrivilegeType}");
                System.Console.WriteLine($"Active: {account.Active}");
                System.Console.WriteLine($"Date of Opening: {account.DateOfOpening:d}");

                System.Console.WriteLine("\nRecent Transactions:");
                ResultGenerator.PrintAllLogTransactions(accNo);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error displaying account info: {ex.Message}");
                Logger.Error(ex, "Error displaying account info");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        /*private static void Deposit()
        {
            try
            {
                System.Console.Write("Enter account number: ");
                string accNo = System.Console.ReadLine();

                IAccount account = _accountManager.GetAccount(accNo);
                if (account == null)
                {
                    throw new ArgumentException("Account not found.");
                }

                System.Console.Write("Enter amount to deposit: ");
                if (!double.TryParse(System.Console.ReadLine(), out double amount) || amount <= 0)
                {
                    throw new ArgumentException("Invalid deposit amount.");
                }

                _accountManager.Deposit(account, amount);
                System.Console.WriteLine($"Deposit successful. New balance: {account.Balance:C}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error depositing: {ex.Message}");
                Logger.Error(ex, "Error depositing");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        private static void Withdraw()
        {
            try
            {
                System.Console.Write("Enter account number: ");
                string accNo = System.Console.ReadLine();

                IAccount account = _accountManager.GetAccount(accNo);
                if (account == null)
                {
                    throw new ArgumentException("Account not found.");
                }

                System.Console.Write("Enter amount to withdraw: ");
                if (!double.TryParse(System.Console.ReadLine(), out double amount) || amount <= 0)
                {
                    throw new ArgumentException("Invalid withdrawal amount.");
                }

                System.Console.Write("Enter PIN: ");
                string pin = System.Console.ReadLine();

                _accountManager.Withdraw(account, amount, pin);
                System.Console.WriteLine($"Withdrawal successful. New balance: {account.Balance:C}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error withdrawing: {ex.Message}");
                Logger.Error(ex, "Error withdrawing");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        private static void TransferFunds()
        {
            try
            {
                System.Console.Write("Enter from account number: ");
                string fromAccNo = System.Console.ReadLine();

                IAccount fromAccount = _accountManager.GetAccount(fromAccNo);
                if (fromAccount == null)
                {
                    throw new ArgumentException("From account not found.");
                }

                System.Console.Write("Enter to account number: ");
                string toAccNo = System.Console.ReadLine();

                IAccount toAccount = _accountManager.GetAccount(toAccNo);
                if (toAccount == null)
                {
                    throw new ArgumentException("To account not found.");
                }

                System.Console.Write("Enter amount to transfer: ");
                if (!double.TryParse(System.Console.ReadLine(), out double amount) || amount <= 0)
                {
                    throw new ArgumentException("Invalid transfer amount.");
                }

                System.Console.Write("Enter PIN: ");
                string pin = System.Console.ReadLine();

                var transfer = new Transfer
                {
                    FromAcc = fromAccount,
                    ToAcc = toAccount,
                    Amount = amount,
                    Pin = pin
                };

                _accountManager.TransferFunds(transfer);
                System.Console.WriteLine($"Transfer successful. New balance for account {fromAccNo}: {fromAccount.Balance:C}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error transferring funds: {ex.Message}");
                Logger.Error(ex, "Error transferring funds");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }*/

        private static void Deposit()
        {
            try
            {
                System.Console.Write("Enter account number: ");
                string accNo = System.Console.ReadLine();
                System.Console.Write("Enter amount to deposit: ");
                double amount = double.Parse(System.Console.ReadLine());

                IAccount account = _accountManager.GetAccount(accNo);
                _accountManager.Deposit(account, amount);

                System.Console.WriteLine($"Deposit successful. New balance: {account.Balance:C}");
            }
            catch (AccountDoesNotExistException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InactiveAccountException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (FormatException)
            {
                System.Console.WriteLine("Error: Invalid amount format. Please enter a valid number.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        private static void Withdraw()
        {
            try
            {
                System.Console.Write("Enter account number: ");
                string accNo = System.Console.ReadLine();
                System.Console.Write("Enter amount to withdraw: ");
                double amount = double.Parse(System.Console.ReadLine());
                System.Console.Write("Enter PIN: ");
                string pin = System.Console.ReadLine();

                IAccount account = _accountManager.GetAccount(accNo);
                _accountManager.Withdraw(account, amount, pin);

                System.Console.WriteLine($"Withdrawal successful. New balance: {account.Balance:C}");
            }
            catch (AccountDoesNotExistException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InactiveAccountException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InvalidPinException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InsufficientBalanceException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (FormatException)
            {
                System.Console.WriteLine("Error: Invalid amount format. Please enter a valid number.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        private static void TransferFunds()
        {
            try
            {
                System.Console.Write("Enter from account number: ");
                string fromAccNo = System.Console.ReadLine();
                System.Console.Write("Enter to account number: ");
                string toAccNo = System.Console.ReadLine();
                System.Console.Write("Enter amount to transfer: ");
                double amount = double.Parse(System.Console.ReadLine());
                System.Console.Write("Enter PIN: ");
                string pin = System.Console.ReadLine();

                IAccount fromAccount = _accountManager.GetAccount(fromAccNo);
                IAccount toAccount = _accountManager.GetAccount(toAccNo);

                var transfer = new Transfer
                {
                    FromAcc = fromAccount,
                    ToAcc = toAccount,
                    Amount = amount,
                    Pin = pin
                };

                _accountManager.TransferFunds(transfer);

                System.Console.WriteLine($"Transfer successful. New balance for account {fromAccNo}: {fromAccount.Balance:C}");
            }
            catch (AccountDoesNotExistException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InactiveAccountException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InvalidPinException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InsufficientBalanceException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (DailyLimitExceededException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (FormatException)
            {
                System.Console.WriteLine("Error: Invalid amount format. Please enter a valid number.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        private static void DisplayBankStatistics()
        {
            try
            {
                ResultGenerator.GetTotalNoOfAccounts();
                ResultGenerator.DisplayNoOfAccTypeWise();
                ResultGenerator.DispTotalWorthOfBank();
                ResultGenerator.DispPolicyInfo();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error displaying bank statistics: {ex.Message}");
                Logger.Error(ex, "Error displaying bank statistics");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        private static void DisplayAllTransactions()
        {
            try
            {
                ResultGenerator.DisplayAllTransactions();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error displaying all transactions: {ex.Message}");
                Logger.Error(ex, "Error displaying all transactions");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}