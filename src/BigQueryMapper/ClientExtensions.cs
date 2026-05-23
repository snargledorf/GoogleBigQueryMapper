using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using FastMember;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;

namespace BigQueryMapper;

public static class ClientExtensions
{
    extension(BigQueryClient client)
    {
        public async IAsyncEnumerable<T> QueryAsync<T>(FormattableString formattableString, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            BigQueryParameter[] parameters =
            [
                ..formattableString.GetArguments()
                    .Select((arg, i) =>
                    {
                        var parameterName = $"param_{i}";
                        return BigQueryParameterFactory.Create(parameterName, arg);
                    })
            ];

            object[] parameterNames = [..parameters.Select(param => $"@{param.Name}")];
            string queryString = string.Format(formattableString.Format, parameterNames);

            BigQueryResults results =
                await client.ExecuteQueryAsync(queryString, parameters, cancellationToken: cancellationToken);

            foreach (BigQueryRow row in results)
                yield return BigQueryRowToObjectMapper<T>.Instance.Create(row);
        }
    }
}

public class BigQueryRowToObjectMapper<T>
{
    private static readonly TypeAccessor TypeAccessor = TypeAccessor.Create(typeof(T));
    public static BigQueryRowToObjectMapper<T> Instance => field ??= new BigQueryRowToObjectMapper<T>();
    
    public T Create(BigQueryRow row)
    {
        var obj = (T)TypeAccessor.CreateNew();
        
        foreach (TableFieldSchema field in row.Schema.Fields)
            TypeAccessor[obj, field.Name] = row[field.Name];

        return obj;
    }
}