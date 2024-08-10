using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
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
                if (string.IsNullOrEmpty(serviceBanksConfig))
                {
                    throw new ConfigurationErrorsException("ServiceBanks configuration is missing or empty");
                }

                var bankEntries = serviceBanksConfig.Split(';');
                foreach (var entry in bankEntries)
                {
                    var parts = entry.Split(':');
                    if (parts.Length != 2)
                    {
                        Logger.Warn($"Invalid bank entry format: {entry}");
                        continue;
                    }

                    var bankCode = parts[0];
                    var className = parts[1];

                    Type type = FindType(className);
                    if (type == null)
                    {
                        Logger.Error($"Could not load type {className}");
                        continue;
                    }

                    if (!typeof(IExternalBankService).IsAssignableFrom(type))
                    {
                        Logger.Error($"Type {className} does not implement IExternalBankService");
                        continue;
                    }

                    var bankObj = (IExternalBankService)Activator.CreateInstance(type);
                    _serviceBankPool[bankCode] = bankObj;
                    Logger.Info($"Loaded external bank service: {bankCode} - {className}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading external bank services");
                throw;
            }
        }

        private Type FindType(string typeName)
        {
            // First, try to get the type from the current assembly
            var type = Type.GetType(typeName);
            if (type != null) return type;

            // If not found, search in all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;
            }

            // If still not found, try to load the assembly and get the type
            var assemblyName = typeName.Substring(0, typeName.LastIndexOf('.'));
            try
            {
                var assembly = Assembly.Load(assemblyName);
                type = assembly.GetType(typeName);
                if (type != null) return type;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to load assembly {assemblyName}");
            }

            return null;
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