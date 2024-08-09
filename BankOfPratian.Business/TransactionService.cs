using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using System;

namespace BankOfPratian.Business
{
    public class TransactionService
    {
        private readonly IAccountManager _accountManager;

        public TransactionService(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        public void ProcessDeposit(string accountNumber, double amount)
        {
            try
            {
                var account = _accountManager.GetAccount(accountNumber);
                _accountManager.Deposit(account, amount);
                Console.WriteLine($"Deposit successful. New balance: {account.Balance:C}");
            }
            catch (AccountDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InactiveAccountException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (DatabaseOperationException ex)
            {
                Console.WriteLine($"Error: Unable to process deposit. Please try again later.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public void ProcessWithdrawal(string accountNumber, double amount, string pin)
        {
            try
            {
                var account = _accountManager.GetAccount(accountNumber);
                _accountManager.Withdraw(account, amount, pin);
                Console.WriteLine($"Withdrawal successful. New balance: {account.Balance:C}");
            }
            catch (AccountDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InactiveAccountException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InvalidPinException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (DatabaseOperationException ex)
            {
                Console.WriteLine($"Error: Unable to process withdrawal. Please try again later.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public void ProcessTransfer(string fromAccountNumber, string toAccountNumber, double amount, string pin)
        {
            try
            {
                var fromAccount = _accountManager.GetAccount(fromAccountNumber);
                var toAccount = _accountManager.GetAccount(toAccountNumber);
                var transfer = new Transfer
                {
                    FromAcc = fromAccount,
                    ToAcc = toAccount,
                    Amount = amount,
                    Pin = pin
                };
                _accountManager.TransferFunds(transfer);
                Console.WriteLine($"Transfer successful. New balance for account {fromAccountNumber}: {fromAccount.Balance:C}");
            }
            catch (AccountDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InactiveAccountException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InvalidPinException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (DailyLimitExceededException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (DatabaseOperationException ex)
            {
                Console.WriteLine($"Error: Unable to process transfer. Please try again later.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}