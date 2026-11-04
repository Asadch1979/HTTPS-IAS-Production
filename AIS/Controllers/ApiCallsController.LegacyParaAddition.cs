using Microsoft.AspNetCore.Mvc;

namespace AIS.Controllers
{
    public partial class ApiCallsController
    {
        // Deliberately unconditional: configuration or an existing menu grant must
        // never re-enable the retired direct-to-live-table implementation.
        private IActionResult LegacyParaAdditionUnavailable()
        {
            Response.Headers["Cache-Control"] = "no-store";
            return StatusCode(503, new
            {
                Status = false,
                Code = "LEGACY_PARA_ADDITION_DISABLED",
                Message = "Legacy para addition is unavailable pending redesign and database/security review."
            });
        }
    }
}
