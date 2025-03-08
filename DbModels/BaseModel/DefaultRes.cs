using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModels.BaseModel
{
    public class DefaultMesRes<T> where T : class
    {
        public int Code { get; set; }
        public required string Message { get; set; }
        public required T Data { get; set; }

    }
    public class DefaultMesRes
    {
        public int Code { get; set; }
        public required string Message { get; set; }
        public  object Data { get; set; }

    }
}
