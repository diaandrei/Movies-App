namespace Movies.Application.Models
{
    public class ResponseModel<T> where T : class
    {
        public ResponseModel()
        {
            Success = false;
        }
        public bool Success { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Exception ExceptionMessage { get; set; }
        public T Content { get; set; }
    }
}
