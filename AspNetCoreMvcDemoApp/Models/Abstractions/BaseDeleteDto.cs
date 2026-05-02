using System.ComponentModel.DataAnnotations;

namespace AspNetCoreMvcDemoApp.Models.Abstractions
{
    public abstract class BaseDeleteDto
    {
        [Display(Name = "Ids")]
        [Required(ErrorMessage = "{0} require.")]
        [MinLength(1, ErrorMessage = "At least one record must be selected.")]

        public List<int> Ids { get; set; } = [];
    }

    public abstract class BaseDeleteDto<TSearchDto> : BaseDeleteDto
        where TSearchDto : BaseSearchDto
    {
        [Display(Name = "Filters")]
        [Required(ErrorMessage = "{0} cannot be left blank.")]
        public TSearchDto SearchDto { get; set; } = null!;
    }
}
