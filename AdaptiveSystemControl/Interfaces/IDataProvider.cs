
namespace AdaptiveSystemControl.Interfaces
{
    public interface IDataProvider
    {
        Task<double> ReadValueAsync();
    }
}
