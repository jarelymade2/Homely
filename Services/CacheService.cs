using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using StackExchange.Redis;

namespace StayGo.Services;

/// <summary>
/// Implementación del servicio de caché distribuido con Redis (con fallback a memoria)
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly bool _isRedisAvailable;

    public CacheService(
        IDistributedCache distributedCache,
        ILogger<CacheService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _distributedCache = distributedCache;
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        // Verificar si Redis está disponible
        _isRedisAvailable = _redis != null && _redis.IsConnected;

        if (!_isRedisAvailable)
        {
            _logger.LogWarning("⚠️ Redis no está disponible. Usando caché en memoria.");
        }
    }

    /// <summary>
    /// Obtiene un valor del caché
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            
            if (string.IsNullOrEmpty(cachedData))
                return null;

            return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener del caché la clave: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Establece un valor en el caché con tiempo de expiración
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var serializedData = JsonSerializer.Serialize(value, _jsonOptions);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            await _distributedCache.SetStringAsync(key, serializedData, options);
            
            _logger.LogDebug("Valor cacheado con clave: {Key}, expiración: {Expiration}", 
                key, expiration ?? TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al establecer en caché la clave: {Key}", key);
        }
    }

    /// <summary>
    /// Elimina un valor del caché
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("Clave eliminada del caché: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar del caché la clave: {Key}", key);
        }
    }

    /// <summary>
    /// Elimina múltiples valores del caché por patrón
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        // Si Redis no está disponible, no hacer nada
        if (!_isRedisAvailable || _redis == null)
        {
            _logger.LogWarning("Redis no disponible. No se pueden eliminar claves por patrón: {Pattern}", pattern);
            return;
        }

        try
        {
            var db = _redis.GetDatabase();
            var endpoints = _redis.GetEndPoints();

            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: pattern);

                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                }
            }

            _logger.LogDebug("Claves eliminadas del caché con patrón: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar del caché por patrón: {Pattern}", pattern);
        }
    }

    /// <summary>
    /// Verifica si existe una clave en el caché
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            return !string.IsNullOrEmpty(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de clave: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Obtiene o crea un valor en el caché
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        try
        {
            // Intentar obtener del caché
            var cachedValue = await GetAsync<T>(key);
            
            if (cachedValue != null)
            {
                _logger.LogDebug("Cache HIT para clave: {Key}", key);
                return cachedValue;
            }

            // Si no existe, crear el valor
            _logger.LogDebug("Cache MISS para clave: {Key}", key);
            var value = await factory();
            
            // Guardar en caché
            await SetAsync(key, value, expiration);
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetOrCreateAsync para clave: {Key}", key);
            // Si hay error, ejecutar la factory sin caché
            return await factory();
        }
    }
}

