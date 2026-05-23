using System;
using Google.Cloud.BigQuery.V2;

namespace BigQueryMapper;

public static class BigQueryParameterFactory
{
    public static BigQueryParameter Create(string name, object parameterValue)
    {
        return parameterValue switch
        {
            string s => new BigQueryParameter(name, BigQueryDbType.String, s),
            DateTime dt => new BigQueryParameter(name, BigQueryDbType.DateTime, dt),
            DateTimeOffset dto => new BigQueryParameter(name, BigQueryDbType.Timestamp, dto),
            _ => throw new ArgumentOutOfRangeException(nameof(parameterValue), parameterValue, null)
        };
    }
}