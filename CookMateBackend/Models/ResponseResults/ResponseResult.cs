namespace CookMateBackend.Models.ResponseResults
{
    public class ResponseResult<T>
    {
        public Boolean? IsSuccess { get; set; }
        public string? Message { get; set; }

        public T? Result { get; set; }
    }
}
