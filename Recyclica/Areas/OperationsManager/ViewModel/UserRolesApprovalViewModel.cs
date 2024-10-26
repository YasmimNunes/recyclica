namespace Recyclica.ViewModels
{
    public class UserRolesApprovalViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public bool Approved { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovalDate { get; set; } // Alterado para string para exibir a data formatada
    }
}