using System;
using System.Collections.Generic;
using System.Text;

namespace KnightPath.Models
{
    public class KnightPathResponse
    {
        private string _starting;
        private string _ending;
        private string _shortestPath;
        private int _numberOfMoves = 0;
        private string _operationId;

        public KnightPathResponse(string Start, string Ending, string ShortestPath, int NumberOfMoves, string OperationId)
        {
            _starting = Start;
            _ending = Ending;
            _shortestPath = ShortestPath;
            _numberOfMoves = NumberOfMoves;
            _operationId = OperationId;
        }

        public string starting
        {
            get { return _starting; }
            set { _starting = value; }
        }

        public string ending
        {
            get { return _ending; }
            set { _ending = value; }
        }

        public string shortestPath
        {
            get { return _shortestPath; }
            set { _shortestPath = value; }
        }

        public int numberOfMoves
        {
            get { return _numberOfMoves; }
            set { _numberOfMoves = value; }
        }

        public string operationId
        {
            get { return _operationId; }
            set { _operationId = value; }
        }

    }
}
