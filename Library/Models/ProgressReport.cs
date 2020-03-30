using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Models
{
    public class ProgressReport
    {
        public int PercentageComplete { get; set; }

        public List<Country> Countries { get; set; }
    }
}
