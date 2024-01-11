namespace PollApp.Api.Models;

public class StoreDatabaseSettings {
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required string PollCollectionName { get; set; }
    public required string UserCollectionName { get; set; }
}