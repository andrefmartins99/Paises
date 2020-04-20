namespace Library.Models
{
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
            Currency currency = (Currency)obj;

            if (currency == null)
            {
                return false;
            }

            return currency.Code == this.Code;
        }
    }
}
