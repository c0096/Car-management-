namespace Orders.Db;

public interface IDatabaseInitializer
{
    Task InitializeAsync();
}
