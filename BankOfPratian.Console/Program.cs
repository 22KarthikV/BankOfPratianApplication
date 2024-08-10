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
using System.Data.SqlClient;

namespace BankOfPratian.Console
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static AccountManager _accountManager;
        private static ExternalTransferService _externalTransferService;
        //private static IExternalAccountDAO _externalAccountDAO;
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

            // Configuration
            var configurationManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            services.AddSingleton<Configuration>(configurationManager);

            // Initialize PolicyFactory
            PolicyFactory.Initialize(configurationManager);

            string connectionString = ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString;

            // Register DAOs
            services.AddSingleton<ITransactionDAO>(new TransactionDAO(connectionString));
            services.AddSingleton<IExternalTransferDAO>(new ExternalTransferDAO(connectionString));

            // Register factories and managers
            services.AddSingleton<IPolicyFactory, PolicyFactory>();
            
            /*services.AddSingleton<IPolicyFactory>(sp => new PolicyFactory(configurationManager));*/
            services.AddSingleton<ExternalBankServiceFactory>(s => ExternalBankServiceFactory.Instance);
            services.AddSingleton<AccountPrivilegeManager>();

            // Register AccountDAO with PolicyFactory dependency
            services.AddSingleton<IAccountDAO>(sp =>
                new AccountDAO(
                    connectionString,
                    sp.GetRequiredService<IPolicyFactory>()
                )
            );

            // Register AccountManager
            services.AddSingleton<AccountManager>();

            // Register ExternalTransferService with its dependencies
            services.AddSingleton<ExternalTransferService>(sp =>
            {
                var accountManager = sp.GetRequiredService<AccountManager>();
                var externalTransferDAO = sp.GetRequiredService<IExternalTransferDAO>();
                var externalBankServiceFactory = sp.GetRequiredService<ExternalBankServiceFactory>();
                return new ExternalTransferService(
                    externalTransferDAO,
                    externalBankServiceFactory,
                    accountManager.GetAccount,
                    accountManager.Withdraw,
                    accountManager.GetDailyLimit,
                    accountManager.GetDailyTransferAmount
                );
            });

            

            _serviceProvider = services.BuildServiceProvider();
        }






        private static void RunApplication()
        {
            _accountManager = _serviceProvider.GetRequiredService<AccountManager>();
            _externalTransferService = _serviceProvider.GetRequiredService<ExternalTransferService>();
            //_externalAccountDAO = _serviceProvider.GetRequiredService<IExternalAccountDAO>();
            var externalTransferService = _serviceProvider.GetRequiredService<ExternalTransferService>();
            externalTransferService.Start();

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
                System.Console.WriteLine("4. External Transfer");
                System.Console.WriteLine("5. Back to Main Menu");
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
                            InitiateExternalTransfer();
                            return;
                        case 5:
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
                System.Console.WriteLine("3. Display All Transactions for Today");
                System.Console.WriteLine("4. Display All Deposits");
                System.Console.WriteLine("5. Display All Withdrawals");
                System.Console.WriteLine("6. Display All Transfers");
                System.Console.WriteLine("7. Back to Main Menu");
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
                            DisplayAllTransactionsForToday();
                            break;
                        case 4:
                            DisplayAllDeposits();
                            break;
                        case 5:
                            DisplayAllWithdrawals();
                            break;
                        case 6:
                            DisplayAllTransfers();
                            break;
                        case 7:
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
                System.Console.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
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
            catch (SqlException ex)
            {
                System.Console.WriteLine($"Database error occurred: {ex.Message}");
                
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
                System.Console.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

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
                System.Console.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
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

        
        private static void InitiateExternalTransfer()
        {
            try
            {
                System.Console.Write("Enter from account number: ");
                string fromAccountNo = System.Console.ReadLine();
                System.Console.Write("Enter to external account number: ");
                string toExternalAccountNo = System.Console.ReadLine();
                System.Console.Write("Enter amount: ");
                if (double.TryParse(System.Console.ReadLine(), out double amount))
                {
                    System.Console.Write("Enter PIN: ");
                    string pin = System.Console.ReadLine();

                    var transfer = new ExternalTransfer
                    {
                        FromAccountNo = fromAccountNo,
                        ToExternalAcc = toExternalAccountNo,
                        Amount = amount,
                        FromAccPin = pin
                    };
                    _accountManager.TransferFundsToExternal(transfer);
                    Logger.Info($"External transfer initiated: From {fromAccountNo} to {toExternalAccountNo}, Amount: {amount}");
                    System.Console.WriteLine("External transfer initiated successfully.");
                    /*System.Console.WriteLine("Press any key to continue..");
                    System.Console.Read();*/
                }
                else
                {
                    System.Console.WriteLine("Invalid amount entered.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error initiating external transfer");
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
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
        private static void DisplayAllTransactionsForToday()
        {
            try
            {
                ResultGenerator.DisplayAllTransactionsForToday();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error displaying transactions for today: {ex.Message}");
                Logger.Error(ex, "Error displaying transactions for today");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        private static void DisplayAllDeposits()
        {
            try
            {
                ResultGenerator.DisplayAllDeposits();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error displaying deposit transactions: {ex.Message}");
                Logger.Error(ex, "Error displaying deposit transactions");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        private static void DisplayAllWithdrawals()
        {
            try
            {
                ResultGenerator.DisplayAllWithdrawals();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error displaying withdrawal transactions: {ex.Message}");
                Logger.Error(ex, "Error displaying withdrawal transactions");
            }
            System.Console.WriteLine("Press any key to continue.");
            System.Console.ReadKey();
        }

        private static void DisplayAllTransfers()
        {
            try
            {
                ResultGenerator.DisplayAllTransfers();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error displaying transfer transactions: {ex.Message}");
                Logger.Error(ex, "Error displaying transfer transactions");
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