using System;
using System.Collections.Generic;
using System.Configuration;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using Microsoft.IdentityModel.Protocols;

namespace BankOfPratian.Business
{
    public class PolicyFactory : IPolicyFactory
    {
        private static PolicyFactory _instance;
        private static readonly object _lock = new object();
        private readonly Dictionary<string, IPolicy> _policies;

        private PolicyFactory()
        {
            _policies = new Dictionary<string, IPolicy>();
            LoadPolicies();
        }

        public static PolicyFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new PolicyFactory();
                        }
                    }
                }
                return _instance;
            }
        }

        private void LoadPolicies()
        {
            var policiesConfig = ConfigurationManager.AppSettings["Policies"];
            var policyEntries = policiesConfig.Split(';');

            foreach (var entry in policyEntries)
            {
                var parts = entry.Split(':');
                var policyType = parts[0];
                var values = parts[1].Split(',');
                var minBalance = double.Parse(values[0]);
                var rateOfInterest = double.Parse(values[1]);

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