using System.ComponentModel.DataAnnotations;

namespace Hospital_Manager.ViewModels.Patient
{
    public class PatinetEdit
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; }
        [Required][MaxLength(30)] public string LastName { get; set; }
        [Required][EmailAddress] public string Email { get; set; }
        [Required][Phone] public string PhoneNumber { get; set; }
        public List<int> Doctorids { get; set; } = new List<int>();
    }
}
