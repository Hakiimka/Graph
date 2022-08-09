using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.Model
{
    internal class GraphRecord
    {
        public GraphRecord() { }
        [Name("FirstPoint")]
        public double FirstPoint { get; set; }
        [Name("SecondPoint")]
        public double SecondPoint { get; set; }
    }
}
