namespace GlobalLib.WebApi.Models
{
    public class UserPasswordRecoveryValidation
    {
        public string Email { get; set; }
        public string Codigo { get; set; }
        public string Password { get; set; }
    }
}