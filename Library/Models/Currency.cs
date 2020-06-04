namespace Library.Models
{
    using System.Windows.Data;

    public class Currency
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }

        public override bool Equals(object obj)
        {
            if (obj != BindingOperations.DisconnectedSource)
            {
                Currency currency = (Currency)obj;

                if (currency == null)
                {
                    return false;
                }

                if (currency.Code == this.Code && currency.Name == this.Name && currency.Symbol == this.Symbol)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
