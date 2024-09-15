using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BOL.DataModel
{
    [Table("ApiFields")]
    public class ApiField
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Field { get; set; }

        public int ApiConfigurationId { get; set; }
        public ApiConfiguration ApiConfiguration { get; set; }
    }
}
