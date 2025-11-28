namespace ProjArqsi.Domain.Shared
{
    public class AuditService
    {
        private readonly Serilog.ILogger _logger;

        public AuditService(Serilog.ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
