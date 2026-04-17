using System.Collections.Generic;

namespace TransportesBackend.Models
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalItems { get; set; }
    }
}
