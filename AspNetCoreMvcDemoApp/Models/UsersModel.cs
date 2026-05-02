using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SadLib.Data;
using SadLib.Data.Mapping.Abstractions;
using SadLib.Data.Mapping.Attributes;
using SadLib.Security.Extensions;
using SadLib.Web.Upload;
using SadLib.Web.Upload.Attributes;
using SadLib.Web.Upload.Enums;
using System.ComponentModel.DataAnnotations;

namespace AspNetCoreMvcDemoApp.Models
{
    #region Entity
    public class User : IMapEntity
    {
        public int Id { get; set; }

        public int RoleId { get; set; }

        public string Name { get; set; } = null!;

        public string Surname { get; set; } = null!;

        public string Telephone { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool Active { get; set; }

        public bool IsSuperAdmin { get; set; }

        public DateTime RegisterDate { get; set; }

        public string Avatar { get; set; } = null!;

        public byte[] PwHash { get; set; } = null!;

        public byte[] PwSalt { get; set; } = null!;

        public void Secure(string? Password)
        {
            if (!string.IsNullOrWhiteSpace(Password))
            {
                PwSalt = HashExtensions.GenerateSalt();
                PwHash = Password.ToHashBytes(PwSalt, HashType.SHA512);
            }
        }
    }
    #endregion

    #region Base DTO
    public abstract class UserBaseDto
    {
        public int Id { get; set; }

        [Display(Name = "Role")]
        [Required(ErrorMessage = "{0} require.")]
        [Range(1, int.MaxValue, ErrorMessage = "{0} require.")]
        public int RoleId { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "{0} require.")]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [Display(Name = "Surname")]
        [Required(ErrorMessage = "{0} require.")]
        [StringLength(50)]
        public string Surname { get; set; } = null!;

        [Display(Name = "Telephone")]
        [Required(ErrorMessage = "{0} require.")]
        [StringLength(20)]
        [RegularExpression(@"^(0(5\d{9}|[23489]\d{2}\d{7}))$", ErrorMessage = "Invalid {0} number.")]
        public string Telephone { get; set; } = null!;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} require.")]
        [StringLength(50)]
        [EmailAddress(ErrorMessage = "{0} must be a valid address.")]
        public string Email { get; set; } = null!;

        [Display(Name = "Active")]
        [Required(ErrorMessage = "{0} require.")]
        public bool Active { get; set; }

        [ValidateNever]
        [MapperIgnore]
        public IEnumerable<UserRole> RoleList { get; set; } = null!;

        [ValidateNever]
        [MapperIgnore]
        public string AcceptFileTypes => string.Join(", ", FileTypeRegistry.GetMimeTypes(FileGroupTypes.Images));
    }
    #endregion

    #region Crete DTO
    public class UserCreateDto : UserBaseDto, IMapTo<User>
    {
        [Display(Name = "Password")]
        [Required(ErrorMessage = "{0} require.")]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "{0} must be between {2} and {1} characters.")]
        [MapperIgnore]
        public string Password { get; set; } = null!;

        [Display(Name = "Avatar")]
        [Required(ErrorMessage = "{0} require.")]
        [MapperIgnore]
        [FormFileValidation(AllowedGroup: FileGroupTypes.Images)]
        public IFormFile File { get; set; } = null!;
    }
    #endregion

    #region Update DTO
    public class UserUpdateDto : UserBaseDto, IMapTo<User>
    {
        [Display(Name = "Password")]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "{0} must be between {2} and {1} characters.")]
        [MapperIgnore]
        public string? Password { get; set; }

        [Display(Name = "Avatar")]
        [MapperIgnore]
        [FormFileValidation(AllowedGroup: FileGroupTypes.Images)]
        public IFormFile? File { get; set; }
    }
    #endregion

    #region Delete DTO
    public class UserDeleteDto : BaseDeleteDto<UserSearchDto>
    {

    }
    #endregion

    #region Search
    public class UserSearchDto : BaseSearchDto
    {
        [Display(Name = "Role")]
        [Range(1, int.MaxValue, ErrorMessage = "{0} must be between {1} and {2}.")]
        public int? RoleId { get; set; }

        [Display(Name = "Name Surname")]
        public string? NameSurname { get; set; }

        [Display(Name = "Telephone")]
        public string? Telephone { get; set; }

        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [Display(Name = "Status")]
        public int? Status { get; set; }

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
    public class UserListDto
    {
        [Key]
        [Display(Name = "Id", Order = 1)]
        public int Id { get; set; }

        [Display(Name = "Avatar", Order = 5)]
        public string Avatar { get; set; } = null!;

        [Display(Name = "Role", Order = 10)]
        public string RoleName { get; set; } = null!;

        [Display(Name = "User", Order = 15)]
        public string NameSurname { get; set; } = null!;

        [Display(Name = "Telephone", Order = 20)]
        public string Telephone { get; set; } = null!;

        [Display(Name = "E-mail", Order = 25)]
        public string Email { get; set; } = null!;

        [Display(Name = "Active", Order = 30)]
        public bool Active { get; set; }

        [Display(Name = "Super Admin", Order = 35)]
        public bool IsSuperAdmin { get; set; }

        [Display(Name = "Register", Order = 40)]
        public DateOnly RegisterDate { get; set; }
    }
    #endregion

    #region Login
    public class UserLoginDto
    {
        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "{0} require.")]
        [StringLength(50)]
        [EmailAddress(ErrorMessage = "Invalid {0} address.")]
        public string Email { get; set; } = null!;

        [Display(Name = "Password")]
        [Required(ErrorMessage = "{0} require.")]
        [StringLength(32)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
    #endregion
}
