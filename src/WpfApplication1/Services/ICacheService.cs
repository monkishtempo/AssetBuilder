using System;

namespace AssetBuilder.Services
{
    public interface ICacheService<T> : IDisposable
    {
        T GetOrCreate(object key, Func<T> createItem);
    }
}