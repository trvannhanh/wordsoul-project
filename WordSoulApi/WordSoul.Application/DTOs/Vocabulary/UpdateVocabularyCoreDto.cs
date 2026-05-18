namespace WordSoul.Application.DTOs.Vocabulary
{
    public class UpdateVocabularyCoreDto
    {
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public string? Pronunciation { get; set; }
        public string? ExampleSentence { get; set; }
        public string? Description { get; set; }
        /// <summary>
        /// Được controller gán sau khi upload ảnh lên Cloudinary. Frontend không gửi trực tiếp.
        /// </summary>
        public string? ImageUrl { get; set; }
    }
}
