namespace PSICO.Data.Responses
{
    public class Response
    {
        public bool result { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public long? consecutivoId { get; set; }

        public Response() { result = false; message = ""; consecutivoId = null; }
    }
}
