using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Configuration;

namespace BigQueryMapper.Tests;

public class Tests
{
    private BigQueryClient? _client;
    
    [SetUp]
    public async Task SetupAsync()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .AddUserSecrets<Tests>()
            .Build();
        
        _client = await BigQueryClient.CreateAsync(configurationRoot["projectId"]);
    }

    [Test]
    public async Task Test1()
    {
        // Test using https://console.cloud.google.com/bigquery?ws=!1m4!1m3!3m2!1sbigquery-public-data!2sstackoverflow
        
        IAsyncEnumerable<StackOverflowVotes>? votes = _client?.QueryAsync<StackOverflowVotes>(
            $"""
             select *
             from bigquery-public-data.stackoverflow.votes
             where creation_date > {new DateTimeOffset(new DateTime(2022, 9, 20))}
             """);

        // ReSharper disable PossibleMultipleEnumeration
        Assert.That(votes, Is.Not.Null);
        
        StackOverflowVotes? firstOrDefaultAsync = await votes!.FirstOrDefaultAsync();
        
        Assert.That(firstOrDefaultAsync, Is.Not.Null);
        Assert.That(firstOrDefaultAsync.id, Is.Not.Zero.And.Positive);
    }

    public class StackOverflowVotes
    {
        public long id { get; set; }
        public DateTime creation_date { get; set; }
        public long post_id { get; set; }
        public long vote_type_id { get; set; }
    }
}