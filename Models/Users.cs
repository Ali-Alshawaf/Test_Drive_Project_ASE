namespace Test_Drive.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Password { get; set; }
        public Roles Roles { get; set; }
        public int RolesId { get; set; }

    }
}
