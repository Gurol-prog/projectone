using System.ComponentModel.DataAnnotations;

namespace projectone.Dtos
{
    // Ana yorum ve yanıt için aynı DTO - ParentCommentId yok!
    public class CommentCreateDto
    {
        [Required(ErrorMessage = "Yorum metni gerekli")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Yorum 1-1000 karakter arasında olmalı")]
        public string Comment { get; set; } = null!;
        
        // ParentCommentId KALDIRILDI - endpoint'ten alınacak
    }

    public class CommentUpdateDto
    {
        [Required(ErrorMessage = "Yorum metni gerekli")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Yorum 1-1000 karakter arasında olmalı")]
        public string Comment { get; set; } = null!;
    }

    public class CommentResponseDto
    {
        public string Id { get; set; } = null!;
        public string ContentId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public string? ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public DateTime InsertTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public List<CommentResponseDto>? Replies { get; set; }
        public bool IsOwner { get; set; }
    }
}