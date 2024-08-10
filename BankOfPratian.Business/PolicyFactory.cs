using System;
using System.Collections.Generic;
using System.Configuration;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;

namespace BankOfPratian.Business
{
    public class PolicyFactory : IPolicyFactory
    {
        private static PolicyFactory _instance;
        private readonly Dictionary<string, IPolicy> _policies;

        private static readonly object _lock = new object();
        public PolicyFactory(Configuration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _policies = new Dictionary<string, IPolicy>();
            LoadPolicies(configuration);
        }
        public static void Initialize(Configuration configuration)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PolicyFactory(configuration);
                    }
                }
            }
        }

        public static PolicyFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("PolicyFactory has not been initialized. Call Initialize method first.");
                }
                return _instance;
            }
        }

        private void LoadPolicies(Configuration configuration)
        {
            var policiesConfig = configuration.AppSettings.Settings["Policies"]?.Value;
            if (string.IsNullOrEmpty(policiesConfig))
            {
                throw new ConfigurationErrorsException("Policies configuration is missing or empty");
            }

            var policyEntries = policiesConfig.Split(';');
            foreach (var entry in policyEntries)
            {
                var parts = entry.Split(':');
                if (parts.Length != 2)
                {
                    throw new ConfigurationErrorsException($"Invalid policy entry format: {entry}");
                }

                var policyType = parts[0];
                var values = parts[1].Split(',');
                if (values.Length != 2)
                {
                    throw new ConfigurationErrorsException($"Invalid policy values format: {parts[1]}");
                }

                if (!double.TryParse(values[0], out var minBalance) || !double.TryParse(values[1], out var rateOfInterest))
                {
                    throw new ConfigurationErrorsException($"Invalid numeric values in policy: {parts[1]}");
                }

                _policies[policyType] = new Policy(minBalance, rateOfInterest);
            }
        }

        public IPolicy CreatePolicy(string accType, string privilege)
        {
            var policyKey = $"{accType}-{privilege}";
            if (_policies.TryGetValue(policyKey, out var policy))
            {
                return policy;
            }
            throw new InvalidPolicyTypeException($"Invalid policy type: {policyKey}");
        }

        public Dictionary<string, IPolicy> GetAllPolicies()
        {
            return new Dictionary<string, IPolicy>(_policies);
        }
    }

    public class Policy : IPolicy
    {
        private readonly double _minBalance;
        private readonly double _rateOfInterest;

        public Policy(double minBalance, double rateOfInterest)
        {
            _minBalance = minBalance;
            _rateOfInterest = rateOfInterest;
        }

        public double GetMinBalance() => _minBalance;
        public double GetRateOfInterest() => _rateOfInterest;
    }
}