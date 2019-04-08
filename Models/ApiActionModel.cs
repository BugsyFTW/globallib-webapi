
namespace GlobalLib.WebApi.Models
{
    public class ApiActionModel
    {
        public int ActionID { get; set; }
        public string ActionName { get; set; }
        public string ActionCode { get; set; }
        public string AllowedHttpVerbs { get; set; }
    }
}