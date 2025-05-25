namespace projectone.Config
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string UserspaswordCollectionName { get; set; } = null!;
        public string RefreshTokenCollectionName { get; set; } = null!;
        public string ContentComentsCollectionName { get; set; } = null!;
        public string CommentReplyCollectionName { get; set; } = null!;
        public string CommentReplyReportCollectionName { get; set; } = null!;
        public string ContentComentsReportCollectionName { get; set; } = null!;
        public string ContentCollectionName { get; set; } = null!;
        public string UserLikeCollectionName { get; set; } = null!;
        public string UserLikeCommentCollectionName { get; set; } = null!;

    }

}
