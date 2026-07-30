using HospitalManagementSystem.Data;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public abstract class AdoNetRepositoryBase
    {
        protected readonly DbConnectionHelper DbConnectionHelper;

        protected AdoNetRepositoryBase(DbConnectionHelper dbConnectionHelper)
        {
            DbConnectionHelper = dbConnectionHelper;
        }

        protected SqlConnection CreateConnection()
        {
            return DbConnectionHelper.GetConnection();
        }

        protected static string? GetNullableString(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        protected static int GetInt32(SqlDataReader reader, string columnName)
        {
            return reader.GetInt32(reader.GetOrdinal(columnName));
        }

        protected static decimal GetDecimal(SqlDataReader reader, string columnName)
        {
            return reader.GetDecimal(reader.GetOrdinal(columnName));
        }

        protected static bool GetBoolean(SqlDataReader reader, string columnName)
        {
            return reader.GetBoolean(reader.GetOrdinal(columnName));
        }

        protected static DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            return reader.GetDateTime(reader.GetOrdinal(columnName));
        }

        protected static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }

        protected static object DbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }
    }
}