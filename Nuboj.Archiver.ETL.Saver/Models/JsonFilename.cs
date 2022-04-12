using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuboj.Archiver.ETL.Saver.Models
{
    public class JsonFilename
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string fileName { get; set; }
        public DateTime savedDate { get; set; } = DateTime.UtcNow;
    }
}
