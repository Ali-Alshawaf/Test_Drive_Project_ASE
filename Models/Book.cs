using System;
using System.ComponentModel.DataAnnotations;

namespace Test_Drive.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string City { get; set; }
        public DateTime Date { get; set; }

        [Display(Name= "Booking time")]
        public DateTime Time { get; set; }

        public Cars Cars { get; set; }
        public int CarsId { get; set; }

        public Users Users { get; set; }
        public int UsersId { get; set; }
    }
}
