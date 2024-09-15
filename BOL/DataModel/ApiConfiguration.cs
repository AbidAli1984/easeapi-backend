using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BOL.DataModel
{
    [Table("ApiConfiguration")]
    public class ApiConfiguration
    {
        public int Id { get; set; }
        public int UserID { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string APIEndpoint { get; set; }

        public ICollection<ApiField> ApiFields { get; set; }
    }
}
