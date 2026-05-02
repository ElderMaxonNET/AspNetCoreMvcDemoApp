using AspNetCoreMvcDemoApp.Core.Common.Extensions;
using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using AspNetCoreMvcDemoApp.Core.Data.Dapper;
using Dapper;
using SadLib.Data.Abstractions;
using SadLib.Data.Providers;
using System.Data;

namespace AspNetCoreMvcDemoApp.Core.Data.Repositories
{
    public class UserFilesRepository(IDbClient dbClient) : BaseRepository<UserFiles, UserFilesListDto>(dbClient)
    {
        public Task<PagedResult<UserFilesListDto>> GetPagedResultAsync(UserFilesSearchDto search) =>
            QueryPagedAsync(search, builder =>
            {
                // Base query
                const string query = "SELECT [Id],[UploaderName],[Name],[Description],[ContentType],CONCAT([FormattedSize],' ',[SizeUnit]) AS [FileSize],[CreatedAt],[LastUpdate] FROM [dbo].[UserFiles_View] WHERE 1=1";

                // Initialize the SQL query builder
                builder.Append(query);

                // Non-admin users can only see their own uploads
                if (search.UploaderId.HasValue)
                    builder.And("UploaderId = @UploaderId", new { UploaderId = search.UploaderId });

                if (!search.Enable)
                {
                    return; // If search is not enabled, return all records without filters
                }

                // Description Filter (Contains)
                if (!string.IsNullOrWhiteSpace(search.Description))
                {
                    builder.And("Description LIKE @Description", new { Description = $"%{search.Description}%" });
                }

                // Content Type Filter (Exact Match)
                if (!string.IsNullOrWhiteSpace(search.ContentType))
                {
                    builder.And("ContentType = @ContentType", new { ContentType = search.ContentType });
                }

                // File size filtering
                if (search.FileSize > 0 && search.FileSizeOperator > 0)
                {
                    var operatorSql = search.FileSizeOperator switch
                    {
                        1 => "SizeInBytes < @Size",
                        2 => "SizeInBytes = @Size",
                        3 => "SizeInBytes > @Size",
                        _ => null
                    };

                    if (operatorSql != null)
                        builder.And(operatorSql, new { Size = search.FileSize });
                }


                // Date filters
                if (search.FastTimeType?.GetDateFromTimeType()?.Date is { } fastTimeDate)
                {
                    // FastTimeType filter
                    builder.And("CreatedAt >= @FastTimeDate", new { FastTimeDate = fastTimeDate });
                }
                else if (search.StartDate?.Date is { } start && search.EndDate?.Date is { } end)
                {
                    // Range date filter
                    builder.And("CreatedAt BETWEEN @Start AND @End", new { Start = start, End = end });
                }
            });

        public Task<UserFiles?> GetAsync(int id) => 
            QuerySingleAsync<UserFiles>(sql: "SELECT * FROM dbo.UserFiles WHERE Id = @Id", param: new { Id = id });

        public Task<IEnumerable<string>> GetFileNamesAsync(IEnumerable<int> ids, int? uploaderId = null)
        {
            var sql = "SELECT Name FROM dbo.UserFiles WHERE Id IN @Ids";

            if (uploaderId.HasValue)
            {
                sql += " AND UploaderId = @UploaderId";
            }

            return QueryListAsync<string>(
                sql: sql,
                param: new { Ids = ids, UploaderId = uploaderId });
        }

        public Task SaveAsync(IEnumerable<UserFiles> models) =>
            ExecuteBatchAsync("UserFiles_Save", models.Select(m => new
            {
                m.Id,
                m.UploaderId,
                m.Name,
                m.Description,
                m.ContentType,
                m.SizeInBytes,
                m.FormattedSize,
                m.SizeUnit
            }));

        public Task DeleteAsync(IEnumerable<int> ids, int? uploaderId = null) =>
            ExecuteAsync(
                sql: "UserFiles_Delete",
                param: new
                {
                    Ids = ids.ToTableValueParameter(),
                    UploaderId = uploaderId
                },
                commandType: CommandType.StoredProcedure);
    }
}
