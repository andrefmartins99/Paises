using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Models
{
    public class RegionalBloc
    {
        public string Acronym { get; set; }

        public string Name { get; set; }

        public List<string> OtherAcronyms { get; set; }

        public List<string> OtherNames { get; set; }
    }
}
