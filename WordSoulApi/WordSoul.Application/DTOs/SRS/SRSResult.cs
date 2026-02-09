using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSoul.Application.DTOs.SRS
{
    /// <summary>
    /// Result of SRS calculation
    /// </summary>
    public class SRSResult
    {
        public double NewEaseFactor { get; set; }
        public int NewInterval { get; set; }
        public int NewRepetition { get; set; }
        public DateTime NextReviewDate { get; set; }
        public string MemoryState { get; set; } = "Learning";
    }
}
