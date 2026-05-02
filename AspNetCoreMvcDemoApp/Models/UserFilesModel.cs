using SadLib.Data;
using SadLib.Data.Mapping.Abstractions;
using SadLib.Data.Mapping.Attributes;
using SadLib.Web.Upload;
using SadLib.Web.Upload.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AspNetCoreMvcDemoApp.Models
{
    #region Entity
    public class UserFiles : IMapEntity
    {
        public int Id { get; set; }

        public int UploaderId { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long SizeInBytes { get; set; }

        public decimal FormattedSize { get; set; }

        public string SizeUnit { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime LastUpdate { get; set; }
    }

    #endregion

    #region Base DTO
    public abstract class UserFilesBaseDto
    {
        public int Id { get; set; }

        [Display(Name = "File Description")]
        [Required(ErrorMessage = "{0} require.")]
        [StringLength(150)]
        public string Description { get; set; } = null!;
    }
    #endregion

    #region Crete DTO
    public class UserFilesCreateItem : UserFilesBaseDto
    {
        [Display(Name = "Files")]
        [Required(ErrorMessage = "{0} require.")]
        [FormFileValidation]
        public IFormFile File { get; set; } = null!;
    }

    public class UserFilesCreateDto
    {
        [Display(Name = "Files")]
        [Required(ErrorMessage = "{0} require.")]
        [MinLength(1, ErrorMessage = "You must select at least one file.")]
        public List<UserFilesCreateItem> Items { get; set; } = [];
    }
    #endregion

    #region Update DTO
    public class UserFilesUpdateDto : UserFilesBaseDto, IMapTo<UserFiles>
    {
        [Display(Name = "File")]
        [MapperIgnore]
        [FormFileValidation]
        public IFormFile? File { get; set; }
    }
    #endregion

    #region Delete DTO
    public class UserFilesDeleteDto : BaseDeleteDto<UserFilesSearchDto>
    {

    }
    #endregion

    #region Search
    public class UserFilesSearchDto : BaseSearchDto
    {
        internal int? UploaderId { get; set; }

        [Display(Name = "File Description")]
        public string? Description { get; set; }

        [Display(Name = "Content Type")]
        public string? ContentType { get; set; }

        [Display(Name = "File Size")]
        public long? FileSize { get; set; }

        [Display(Name = "File Size Operator")]
        [Range(1, 3, ErrorMessage = "{0} must be between {1} and {2}.")]
        public int? FileSizeOperator { get; set; }

        [Display(Name = "Fast Times")]
        [Range(1, 4, ErrorMessage = "{0} must be between {1} and {2}.")]
        public int? FastTimeType { get; set; }

        [Display(Name = "Created At (Start)")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Created At (End)")]
        public DateTime? EndDate { get; set; }
    }
    #endregion

    #region View
    public class UserFilesListDto
    {
        [Key]
        [Display(Name = "Id", Order = 1)]
        public int Id { get; set; }

        [Display(Name = "Uploader", Order = 5)]
        public string UploaderName { get; set; } = null!;

        [Display(Name = "File Name", Order = 10)]
        public string Name { get; set; } = null!;

        [Display(Name = "Description", Order = 15)]
        public string Description { get; set; } = null!;

        [Display(Name = "Content Type", Order = 20)]
        public string ContentType { get; set; } = null!;

        [Display(Name = "Size", Order = 25)]
        public string FileSize { get; set; } = null!;

        [Display(Name = "Created At", Order = 30)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Update", Order = 35)]
        public DateTime LastUpdate { get; set; }
    }

    #endregion

}
