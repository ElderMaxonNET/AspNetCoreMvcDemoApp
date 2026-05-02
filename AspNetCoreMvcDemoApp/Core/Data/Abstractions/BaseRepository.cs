using System.Data;
using Dapper;

namespace AspNetCoreMvcDemoApp.Core.Data.Abstractions
{
    using Core.Data.Dapper;
    using Core.Web.Extensions;
    using SadLib.Data.Abstractions;
    using SadLib.Data.Providers;

    public abstract class BaseRepository<TEntity>(IDbClient dbClient) where TEntity : class
    {
        protected readonly IDbClient Db = dbClient;

        protected Task<int> ExecuteAsync(string sql, object? param = null, CommandType commandType = CommandType.Text) =>
            Db.Use(con => con.ExecuteAsync(sql, param, commandType: commandType));

        protected Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CommandType commandType = CommandType.Text) =>
            Db.Use(con => con.ExecuteScalarAsync<T>(sql, param, commandType: commandType));

        protected Task<T?> QuerySingleAsync<T>(string sql, object? param = null, CommandType commandType = CommandType.Text) =>
            Db.Use(con => con.QuerySingleOrDefaultAsync<T>(sql, param, commandType: commandType));

        protected Task<IEnumerable<T>> QueryListAsync<T>(string sql, object? param = null, CommandType commandType = CommandType.Text) =>
            Db.Use(con => con.QueryAsync<T>(sql, param, commandType: commandType));

        protected Task ExecuteBatchAsync<T>(string sql, IEnumerable<T> items, CommandType commandType = CommandType.StoredProcedure) =>
            Db.Transaction(async (con, trans) =>
            {
                foreach (var item in items)
                    await con.ExecuteAsync(sql, item, trans, commandType: commandType);
            });
    }

    public abstract class BaseRepository<TEntity, TListItemDto>(IDbClient dbClient) : BaseRepository<TEntity>(dbClient) where TEntity : class where TListItemDto : class
    {
        protected static readonly IReadOnlyList<string> AllowedColumns =
            [.. typeof(TListItemDto).ExtractColumns().Select(x => x.PropName)];

        protected async Task<PagedResult<TListItemDto>> QueryPagedAsync(BaseSearchDto search, Action<ISqlBuilder> buildAction)
        {
            return await Db.Use(async (con, builder) =>
            {
                buildAction(builder);
                builder.OrderByDynamic(search, AllowedColumns);
                builder.Build(search);

                using var multi = await con.QueryMultipleAsync(builder.Sql, builder.ToDapperParameters());
                var totalCount = await multi.ReadFirstAsync<int>();
                var items = await multi.ReadAsync<TListItemDto>();

                return new PagedResult<TListItemDto>(search, [.. items], totalCount);
            });
        }
    }
}
