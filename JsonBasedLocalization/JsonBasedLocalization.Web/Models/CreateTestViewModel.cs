using System.ComponentModel.DataAnnotations;

namespace JsonBasedLocalization.Web.Models
{
    public class CreateTestViewModel
    {
        [Display(Name="name"), Required(ErrorMessage ="required")]
        public string Name { get; set; }
    }
}
