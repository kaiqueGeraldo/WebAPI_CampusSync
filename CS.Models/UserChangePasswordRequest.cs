namespace CS.Models
{
    public class UserChangePasswordRequest
    {
        public string CPF { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
