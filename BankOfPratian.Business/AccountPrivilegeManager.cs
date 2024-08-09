using System;
using System.Collections.Generic;
using System.Configuration;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using NLog;

namespace BankOfPratian.Business
{
    public class AccountPrivilegeManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<PrivilegeType, double> _dailyLimits;

        public AccountPrivilegeManager()
        {
            _dailyLimits = LoadDailyLimits();
        }

        // This constructor is for testing purposes
        public AccountPrivilegeManager(Dictionary<PrivilegeType, double> dailyLimits)
        {
            _dailyLimits = dailyLimits;
        }

        private static Dictionary<PrivilegeType, double> LoadDailyLimits()
        {
            var limits = new Dictionary<PrivilegeType, double>();
            try
            {
                var dailyLimitsConfig = ConfigurationManager.AppSettings["DailyLimits"];
                if (string.IsNullOrEmpty(dailyLimitsConfig))
                {
                    Logger.Warn("DailyLimits configuration is missing or empty. Using default values.");
                    return GetDefaultDailyLimits();
                }

                var limitEntries = dailyLimitsConfig.Split(';');
                foreach (var entry in limitEntries)
                {
                    var parts = entry.Split(':');
                    if (parts.Length != 2)
                    {
                        Logger.Warn($"Invalid entry in DailyLimits configuration: {entry}. Skipping.");
                        continue;
                    }

                    if (Enum.TryParse(parts[0], out PrivilegeType privilegeType) &&
                        double.TryParse(parts[1], out double limit))
                    {
                        limits[privilegeType] = limit;
                    }
                    else
                    {
                        Logger.Warn($"Invalid entry in DailyLimits configuration: {entry}. Skipping.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading daily limits from configuration. Using default values.");
                return GetDefaultDailyLimits();
            }

            return limits.Count > 0 ? limits : GetDefaultDailyLimits();
        }

        private static Dictionary<PrivilegeType, double> GetDefaultDailyLimits()
        {
            return new Dictionary<PrivilegeType, double>
            {
                { PrivilegeType.REGULAR, 100000.0 },
                { PrivilegeType.GOLD, 200000.0 },
                { PrivilegeType.PREMIUM, 300000.0 }
            };
        }

        public double GetDailyLimit(PrivilegeType privilegeType)
        {
            if (_dailyLimits.TryGetValue(privilegeType, out double limit))
            {
                return limit;
            }
            throw new InvalidPrivilegeTypeException($"Invalid privilege type: {privilegeType}");
        }
    }
}