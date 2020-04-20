namespace Library.Models
{
    using System.Collections.Generic;

    public class RegionalBloc
    {
        public string Acronym { get; set; }

        public string Name { get; set; }

        public List<string> OtherAcronyms { get; set; }

        public List<string> OtherNames { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }

        public override bool Equals(object obj)
        {
            RegionalBloc regionalBloc = (RegionalBloc)obj;

            if (regionalBloc == null)
            {
                return false;
            }

            return regionalBloc.Acronym == this.Acronym;
        }
    }
}
