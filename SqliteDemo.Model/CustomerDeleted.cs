namespace SqliteDemo.Model
{
    public class CustomerDeleted
    {
        public string CustomerID { get; set; } = default!;

        public bool? IsDelete { get; set; }
    }
}
