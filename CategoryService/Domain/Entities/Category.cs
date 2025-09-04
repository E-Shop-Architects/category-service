using Core.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Category : Entity<Guid>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Order { get; set; }
        public string Status { get; set; }

        public Category()
        {
            Status = "Aktif";
        }

        public Category(Guid id, string name, string? description, int order, string status)
        {
            Id = id;
            Name = name;
            Description = description;
            Order = order;
            Status = status;
        }
    }
}
