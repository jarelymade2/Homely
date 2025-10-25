namespace StayGo.Services;

/// <summary>
/// Interfaz para el servicio de caché distribuido con Redis
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtiene un valor del caché
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Establece un valor en el caché con tiempo de expiración
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Elimina un valor del caché
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Elimina múltiples valores del caché por patrón
    /// </summary>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Verifica si existe una clave en el caché
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Obtiene o crea un valor en el caché
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;
}

