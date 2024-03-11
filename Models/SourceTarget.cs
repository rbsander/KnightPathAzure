using System;
using System.Collections.Generic;
using System.Text;

namespace KnightPath.Models
{
    public class SourceTarget
    {
        private string _source;
        private string _target;

        public SourceTarget(string start, string end)
        {
            _source = start;
            _target = end;
        }

        public string source
        {
            get {return _source; }
            set { _source = value; }
        }

        public string target
        {
            get { return _target; }
            set { _target = value; }
        }
    }
}
