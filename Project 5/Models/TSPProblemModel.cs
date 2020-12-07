using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_5.Models
{
    public class TSPProblemModel
    {
        public string FileName { get; set; }
        public List<Coordinate> Coords { get; set; } = new List<Coordinate>();
        public List<int> Path { get; set; } = new List<int>();
        public double TotalDistance { get; set; }
        public double MillisecondsToRun { get; set; }
    }
}