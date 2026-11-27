using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace WebApiTemplateWinAuth.Data
{
    public static class DataReaderMapper
    {
        // =====================================================================
        // Execute via ISqlConnectionFactory (preferred - connection string from DI/options)
        // =====================================================================

        public static Task<List<T>> GetDataAsync<T>(
            this SqlCommand sqlCommand,
            ISqlConnectionFactory connectionFactory,
            SqlServer target,
            string? database = null,
            CancellationToken cancellationToken = default
        ) where T : new()
            => ExecuteAsync(sqlCommand, ct => connectionFactory.OpenAsync(target, ct), database, ReadListAsync<T>, cancellationToken);

        public static Task<T?> GetEntryAsync<T>(
            this SqlCommand sqlCommand,
            ISqlConnectionFactory connectionFactory,
            SqlServer target,
            string? database = null,
            CancellationToken cancellationToken = default
        ) where T : new()
            => ExecuteAsync(sqlCommand, ct => connectionFactory.OpenAsync(target, ct), database, ReadSingleAsync<T>, cancellationToken);

        public static Task<int?> GetIntAsync(
            this SqlCommand sqlCommand,
            ISqlConnectionFactory connectionFactory,
            SqlServer target,
            string? database = null,
            CancellationToken cancellationToken = default
        )
            => ExecuteAsync(sqlCommand, ct => connectionFactory.OpenAsync(target, ct), database, ReadIntAsync, cancellationToken);

        // =====================================================================
        // Mapping (public - usable with any DbDataReader, e.g. multiple result sets)
        // =====================================================================

        public static async Task<List<T>> MapToListAsync<T>(
            DbDataReader reader,
            CancellationToken cancellationToken = default
        ) where T : new()
        {
            var columnMap = BuildColumnMap(reader, GetPropertyMap<T>());
            var result = new List<T>();

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(MapRow<T>(reader, columnMap));
            }

            return result;
        }

        public static async Task<T?> MapToSingleAsync<T>(
            DbDataReader reader,
            CancellationToken cancellationToken = default
        ) where T : new()
        {
            var columnMap = BuildColumnMap(reader, GetPropertyMap<T>());

            // first row
            if (!await reader.ReadAsync(cancellationToken))
                return default;

            var result = MapRow<T>(reader, columnMap);

            // verify there is no second row
            if (await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException("Expected one row, but multiple returned.");

            return result;
        }

        // =====================================================================
        // Internals
        // =====================================================================

        private static async Task<TResult> ExecuteAsync<TResult>(
            SqlCommand command,
            Func<CancellationToken, Task<SqlConnection>> openConnection,
            string? database,
            Func<SqlCommand, CancellationToken, Task<TResult>> execute,
            CancellationToken cancellationToken)
        {
            await using var connection = await openConnection(cancellationToken);

            // Must happen AFTER the connection is open.
            // The pool resets the database context when the connection is reused.
            if (database is not null)
                await connection.ChangeDatabaseAsync(database, cancellationToken);

            command.Connection = connection;
            try
            {
                return await execute(command, cancellationToken);
            }
            finally
            {
                // don't leave the command pointing at a disposed connection
                command.Connection = null;
            }
        }

        private static async Task<List<T>> ReadListAsync<T>(SqlCommand command, CancellationToken cancellationToken) where T : new()
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await MapToListAsync<T>(reader, cancellationToken);
        }

        private static async Task<T?> ReadSingleAsync<T>(SqlCommand command, CancellationToken cancellationToken) where T : new()
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await MapToSingleAsync<T>(reader, cancellationToken);
        }

        private static async Task<int?> ReadIntAsync(SqlCommand command, CancellationToken cancellationToken)
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result is null || result is DBNull)
                return null;

            return Convert.ToInt32(result, CultureInfo.InvariantCulture);
        }

        private static T MapRow<T>(DbDataReader reader, PropertyInfo?[] columnMap) where T : new()
        {
            // boxed once, so this also works when T is a struct
            object obj = new T();

            for (int i = 0; i < columnMap.Length; i++)
            {
                var prop = columnMap[i];
                if (prop is null || reader.IsDBNull(i))
                    continue;

                var value = reader.GetValue(i);
                try
                {
                    prop.SetValue(obj, ConvertValue(value, prop.PropertyType));
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException or ArgumentException)
                {
                    throw new InvalidOperationException(
                        $"Cannot map column '{reader.GetName(i)}' ({value.GetType().Name}) " +
                        $"to {typeof(T).Name}.{prop.Name} ({prop.PropertyType.Name}).", ex);
                }
            }

            return (T)obj;
        }

        /// <summary>Converts the value only when the DB type doesn't already match the property type.</summary>
        private static object ConvertValue(object value, Type propertyType)
        {
            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (type.IsInstanceOfType(value))
                return value;

            if (type.IsEnum)
                return value is string s ? Enum.Parse(type, s, ignoreCase: true) : Enum.ToObject(type, value);

            if (type == typeof(DateOnly) && value is DateTime dateTime)
                return DateOnly.FromDateTime(dateTime);

            if (type == typeof(TimeOnly) && value is TimeSpan timeSpan)
                return TimeOnly.FromTimeSpan(timeSpan);

            // int -> long, decimal -> double, byte -> int, ...
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        // Reflection runs once per type, not on every query.
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyMaps = new();

        private static Dictionary<string, PropertyInfo> GetPropertyMap<T>()
            => PropertyMaps.GetOrAdd(typeof(T), BuildPropertyMap);

        private static Dictionary<string, PropertyInfo> BuildPropertyMap(Type type)
        {
            var map = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // skip read-only / computed properties, indexers and [NotMapped]
                if (prop.SetMethod is not { IsPublic: true }
                    || prop.GetIndexParameters().Length > 0
                    || prop.IsDefined(typeof(NotMappedAttribute)))
                    continue;

                var columnName = prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name;

                if (!map.TryAdd(columnName, prop))
                    throw new InvalidOperationException(
                        $"{type.Name}: more than one property maps to column '{columnName}'.");
            }

            return map;
        }

        private static PropertyInfo?[] BuildColumnMap(DbDataReader reader, Dictionary<string, PropertyInfo> propertyMap)
        {
            var columnMap = new PropertyInfo?[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                propertyMap.TryGetValue(reader.GetName(i), out var prop);
                columnMap[i] = prop;
            }

            return columnMap;
        }
    }
}
