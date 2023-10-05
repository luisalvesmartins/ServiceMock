namespace ServiceMock
{
    public class DatabaseRecord
    {
        public DateTime DateCreated { get; set; }
        public DateTime DateLastAccess { get; set; }
        public DateTime DateLastUpdate { get; set; }

        public string request { get; set; }
        public string response { get; set; }
        public bool toProcess {  get; set; }
    }
}