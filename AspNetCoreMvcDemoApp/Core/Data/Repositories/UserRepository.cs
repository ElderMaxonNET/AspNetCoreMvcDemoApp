using AspNetCoreMvcDemoApp.Core.Common.Extensions;
using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using AspNetCoreMvcDemoApp.Core.Data.Dapper;
using Dapper;
using SadLib.Data.Abstractions;
using SadLib.Data.Providers;
using System.Data;

namespace AspNetCoreMvcDemoApp.Core.Data.Repositories
{
    public class UserRepository(IDbClient dbClient) : BaseRepository<User, UserListDto>(dbClient)
    {
        public event Action<IEnumerable<int>>? OnActiveUsersChanged;

        private Task<User?> GetUserInternal(string condition, object param) =>
            QuerySingleAsync<User>($"SELECT * FROM dbo.Users WHERE {condition}", param);

        public Task<PagedResult<UserListDto>> GetPagedResultAsync(UserSearchDto search) =>
            QueryPagedAsync(search, builder =>
            {
                // Base query
                const string query = @"SELECT [Id], [RoleName], [NameSurname], [Telephone], [Email], 
                                      [Active], [IsSuperAdmin], [RegisterDate], [Avatar], [RoleId] 
                               FROM [dbo].[Users_View] WHERE 1=1";

                // Initialize the SQL query builder
                builder.Append(query);

                // If search is not enabled, return all records without filters
                if (!search.Enable)
                {
                    return;
                }

                // Role filter
                if (search.RoleId.HasValue && search.RoleId > 0)
                {
                    builder.And("RoleId = @RoleId", new { RoleId = search.RoleId });
                }

                // Name Surname filter
                if (!string.IsNullOrWhiteSpace(search.NameSurname))
                {
                    builder.And("NameSurname LIKE @NameSurname", new { NameSurname = $"%{search.NameSurname}%" });
                }

                // Telephone filter
                if (!string.IsNullOrWhiteSpace(search.Telephone))
                {
                    builder.And("Telephone LIKE @Telephone", new { Telephone = $"%{search.Telephone}%" });
                }

                // Email filter
                if (!string.IsNullOrWhiteSpace(search.Email))
                {
                    builder.And("Email LIKE @Email", new { Email = $"%{search.Email}%" });
                }

                // Status filter
                if (search.Status.HasValue && search.Status > 0)
                {
                    builder.And("Active = @Active", new { Active = search.Status == 1 });
                }

                // Date filters
                if (search.FastTimeType?.GetDateFromTimeType()?.Date is { } fastTimeDate)
                {
                    // FastTimeType filter
                    builder.And("RegisterDate >= @FastTimeDate", new { FastTimeDate = fastTimeDate });
                }
                else if (search.StartDate?.Date is { } start && search.EndDate?.Date is { } end)
                {
                    // Range date filter
                    builder.And("RegisterDate BETWEEN @Start AND @End", new { Start = start, End = end });
                }

            });

        public Task<User?> GetAsync(int id) =>
            GetUserInternal("Id = @Id", new { Id = id });

        public Task<User?> GetAsync(string email) =>
            GetUserInternal("Email = @Email AND Active = 1", new { Email = email });

        public Task<IEnumerable<int>> GetActiveIdsAsync() =>
            QueryListAsync<int>("SELECT Id FROM dbo.Users_ActiveIds_View");

        public async Task SaveAsync(User model)
        {
            var activeIds = await QueryListAsync<int>("Users_Save", new
            {
                model.Id,
                model.RoleId,
                model.Name,
                model.Surname,
                model.Telephone,
                model.Email,
                model.Active,
                model.Avatar,
                model.PwHash,
                model.PwSalt
            }, CommandType.StoredProcedure);

            OnActiveUsersChanged?.Invoke(activeIds);
        }

        public async Task DeleteAsync(IEnumerable<int> ids)
        {
            var activeIds = await QueryListAsync<int>(
               sql: "Users_Delete", 
               param: new { Ids = ids.ToTableValueParameter() }, 
               commandType: CommandType.StoredProcedure);

            OnActiveUsersChanged?.Invoke(activeIds);
        }

        public Task<IEnumerable<string>> GetAllFileNamesAsync(IEnumerable<int> ids) =>
            QueryListAsync<string>("Users_GetAllFileNames", new { Ids = ids.ToTableValueParameter() }, CommandType.StoredProcedure);
    }
}
