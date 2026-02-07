using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSoul.Application.DTOs.AnswerRecord
{
    public class ReviewStats
    {
        public int TotalReviews { get; set; }
        public int CorrectCount { get; set; }
        public bool? LastIsCorrect { get; set; }

        public double? LastResponseTimeSeconds { get; set; }
    }
}
