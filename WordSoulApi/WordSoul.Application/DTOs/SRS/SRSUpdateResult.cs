using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSoul.Application.DTOs.SRS
{
    public class SRSUpdateResult
    {
        public bool Success { get; set; }
        public double NewEaseFactor { get; set; }
        public int NewInterval { get; set; }
        public DateTime NextReviewDate { get; set; }
        public string MemoryState { get; set; }
        public decimal RetentionScore { get; set; }
        public string Message { get; set; }  // Encouragement message

        public double OldEaseFactor { get; set; }
        public int OldInterval { get; set; }
        public int OldRepetition { get; set; }
        public DateTime OldNextReviewDate { get; set; }
    }
}
