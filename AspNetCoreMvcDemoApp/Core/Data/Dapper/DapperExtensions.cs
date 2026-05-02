using Dapper;
using SadLib.Data;
using SadLib.Data.Abstractions;
using System.Data;

namespace AspNetCoreMvcDemoApp.Core.Data.Dapper
{
    public static class DapperExtensions
    {
        public static SqlMapper.ICustomQueryParameter ToTableValueParameter<T>(
            this IEnumerable<T> items,
            string typeName = "dbo.IdList",
            string columnName = "Id")
        {
            var dt = new DataTable();
            dt.Columns.Add(columnName, typeof(T));
            foreach (var item in items) dt.Rows.Add(item);
            return dt.AsTableValuedParameter(typeName);
        }

        public static DynamicParameters ToDapperParameters(this ISqlBuilder builder)
        {
            var (Templates, ExplicitParams) = builder.GetParameterState();
            var dapperParams = new DynamicParameters();

            foreach (var template in Templates)
            {
                dapperParams.AddDynamicParams(template);
            }

            foreach (var p in ExplicitParams)
            {
                dapperParams.Add(p.Name, p.Value, p.DbType, p.Direction, p.Size, p.Precision, p.Scale);
            }

            return dapperParams;
        }

        public static void AddCustomTypeHandlers()
        {
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        }
    }
}
