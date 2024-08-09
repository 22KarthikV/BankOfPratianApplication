using System;
using System.Collections.Generic;
using System.Configuration;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using NLog;

namespace BankOfPratian.Business
{
    public class ExternalBankServiceFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ExternalBankServiceFactory _instance;
        private static readonly object _lock = new object();
        private readonly Dictionary<string, IExternalBankService> _serviceBankPool;

        private ExternalBankServiceFactory()
        {
            _serviceBankPool = new Dictionary<string, IExternalBankService>();
            LoadExternalBankServices();
        }

        public static ExternalBankServiceFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ExternalBankServiceFactory();
                        }
                    }
                }
                return _instance;
            }
        }

        private void LoadExternalBankServices()
        {
            try
            {
                var serviceBanksConfig = ConfigurationManager.AppSettings["ServiceBanks"];
                var bankEntries = serviceBanksConfig.Split(';');

                foreach (var entry in bankEntries)
                {
                    var parts = entry.Split(':');
                    var bankCode = parts[0];
                    var className = parts[1];

                    var type = Type.GetType(className);
                    if (type == null)
                    {
                        throw new TypeLoadException($"Could not load type {className}");
                    }

                    var bankObj = (IExternalBankService)Activator.CreateInstance(type);
                    _serviceBankPool[bankCode] = bankObj;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading external bank services");
                throw;
            }
        }

        public IExternalBankService GetExternalBankService(string bankCode)
        {
            if (_serviceBankPool.TryGetValue(bankCode, out var service))
            {
                return service;
            }
            throw new AccountDoesNotExistException($"No service found for bank code: {bankCode}");
        }
    }
}