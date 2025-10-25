using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;

namespace StayGo.Integration;

public class MercadoPagoIntegration
{
    private readonly string _accessToken;
    private readonly ILogger<MercadoPagoIntegration> _logger;

    public MercadoPagoIntegration(IConfiguration config, ILogger<MercadoPagoIntegration> logger)
    {
        _accessToken = config["MercadoPago:AccessToken"] ?? "";
        _logger = logger;
        
        if (string.IsNullOrEmpty(_accessToken))
        {
            _logger.LogWarning("MercadoPago AccessToken no configurado");
        }
        else
        {
            MercadoPagoConfig.AccessToken = _accessToken;
        }
    }

    /// <summary>
    /// Crea una preferencia de pago en MercadoPago
    /// </summary>
    /// <param name="reservaId">ID de la reserva</param>
    /// <param name="titulo">Título del producto/servicio</param>
    /// <param name="descripcion">Descripción del producto/servicio</param>
    /// <param name="monto">Monto total a pagar</param>
    /// <param name="urlSuccess">URL de retorno en caso de éxito</param>
    /// <param name="urlFailure">URL de retorno en caso de fallo</param>
    /// <param name="urlPending">URL de retorno en caso de pago pendiente</param>
    /// <returns>URL de inicio de pago de MercadoPago</returns>
    public async Task<string?> CrearPreferenciaPagoAsync(
        Guid reservaId,
        string titulo,
        string descripcion,
        decimal monto,
        string urlSuccess,
        string urlFailure,
        string urlPending)
    {
        try
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                _logger.LogError("No se puede crear preferencia: AccessToken no configurado");
                return null;
            }

            _logger.LogInformation("Creando preferencia de pago para reserva {ReservaId}", reservaId);
            _logger.LogInformation("Monto: {Monto} PEN", monto);
            _logger.LogInformation("URLs - Success: {Success}, Failure: {Failure}, Pending: {Pending}",
                urlSuccess, urlFailure, urlPending);

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = titulo,
                        Description = descripcion,
                        Quantity = 1,
                        CurrencyId = "PEN", // Soles peruanos
                        UnitPrice = monto
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = urlSuccess,
                    Failure = urlFailure,
                    Pending = urlPending
                },
                // Eliminar AutoReturn para evitar el error de validación
                ExternalReference = reservaId.ToString(), // Para identificar la reserva
                StatementDescriptor = "StayGo Reserva"
            };

            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(request);

            _logger.LogInformation($"Preferencia de pago creada: {preference.Id} para reserva {reservaId}");

            // Retorna la URL de inicio de pago
            return preference.InitPoint;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al crear preferencia de pago para reserva {reservaId}");
            return null;
        }
    }

    /// <summary>
    /// Verifica el estado de un pago en MercadoPago
    /// </summary>
    /// <param name="paymentId">ID del pago en MercadoPago</param>
    /// <returns>Estado del pago</returns>
    public async Task<string?> VerificarEstadoPagoAsync(long paymentId)
    {
        try
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                _logger.LogError("No se puede verificar pago: AccessToken no configurado");
                return null;
            }

            // Aquí podrías usar el PaymentClient para verificar el estado
            // Por ahora retornamos null, pero puedes implementarlo según necesites
            _logger.LogInformation($"Verificando estado de pago: {paymentId}");
            
            return await Task.FromResult<string?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al verificar estado de pago {paymentId}");
            return null;
        }
    }
}

