namespace App.CreditStrategies
{
    public interface ICreditStrategy
    {
        bool IsSupported(Customer customer);
        void SetCreditData(Customer customer);
    }
}
