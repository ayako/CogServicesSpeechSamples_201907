using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace STTBatchWeb202001
{
    public class STTEntity : TableEntity
    {
        public STTEntity()
        {}

        public string WavFileUrl { get; set; }
        public string WavFileName { get; set; }
        public string TxtFileUrl { get; set; }
        public string TxtFileName { get; set; }
    }
}
