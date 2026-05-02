namespace AspNetCoreMvcDemoApp.Models.Abstractions
{
    public record AuthUser(
        int Id,
        string Email,
        string FullName,
        int RoleId,
        string Avatar
    );
}
