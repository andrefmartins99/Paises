namespace Library.Models
{
    public class Language
    {
        public string Iso639_1 { get; set; }

        public string Iso639_2 { get; set; }

        public string Name { get; set; }

        public string NativeName { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }

        public override bool Equals(object obj)
        {
            Language language = (Language)obj;

            if (language == null)
            {
                return false;
            }

            return language.Iso639_2 == this.Iso639_2;
        }
    }
}
