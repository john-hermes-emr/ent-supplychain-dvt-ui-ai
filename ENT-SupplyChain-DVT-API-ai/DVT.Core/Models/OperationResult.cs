namespace DVT.Core.Models
{
    public class OperationResult
    {
        public string Operation { get; set; }
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public object? Data { get; set; } = null;

        public List<OperationResult> ChildResults { get; set; } = new List<OperationResult>();

        public void AddChildResult(OperationResult operationResult)
        {
            ChildResults.Add(operationResult);
        }
    }
}
