namespace AssetBuilder.UM.Models
{
    public class UserRoleAssignment
    {
        public bool AlgoEditor { get; set; }
        public bool AlgoTranslator { get; set; }
        public bool Comments { get; set; }
        public bool AlgoAdmin { get; set; }
        public bool AlgoBuilders { get; set; }
        public bool AlgoReviewers { get; set; }
    }
}