namespace projectone.Dtos
{
    public class ContentCreateDto
    {
        public string ContentName { get; set; } = null!;
        public string? ContentDescription { get; set; } // İsteğe bağlı açıklama
        public string? ContentText { get; set; } // İçerik metni
        public string? ContentUrl { get; set; } // Link/URL varsa
        public string? ContentType { get; set; } // "text", "image", "video" vs.
    }
}