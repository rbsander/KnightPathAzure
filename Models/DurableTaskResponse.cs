using System;
using System.Collections.Generic;
using System.Text;

namespace KnightPath.Models
{
    class DurableTaskResponse
    {
        //Partial response of needed elements
        private string _runtimeStatus;
        private KnightPathResponse _output;

        public string runtimeStatus
        {
            get { return _runtimeStatus; }
            set { _runtimeStatus = value; }
        }

        public KnightPathResponse output
        {
            get { return _output; }
            set { _output = value; }
        }
    }
}
