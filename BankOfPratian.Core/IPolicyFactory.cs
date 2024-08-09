namespace BankOfPratian.Core
{
    public interface IPolicyFactory
    {
        IPolicy CreatePolicy(string accType, string privilege);
        Dictionary<string, IPolicy> GetAllPolicies();
    }
}