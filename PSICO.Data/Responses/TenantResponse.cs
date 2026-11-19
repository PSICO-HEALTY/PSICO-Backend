using System;
using System.Collections.Generic;
using System.Text;

namespace PSICO.Data.Responses
{
    public class TenantResponse
    {
        public string tenant { get; set; }
        public bool isAdmin { get; set; } = false;
        public string nombre { get; set; }
        public string tenantLogo { get; set; }
    }
}
