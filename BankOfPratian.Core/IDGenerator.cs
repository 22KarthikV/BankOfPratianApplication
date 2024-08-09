namespace BankOfPratian.Core
{
    public static class IDGenerator
    {
        private static int ID = 1000;

        public static int GenerateID()
        {
            return ID++;
        }
    }
}