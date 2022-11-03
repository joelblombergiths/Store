using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Store;

public class Customer
{
    private static readonly DirectoryInfo DataPath = new(Path
        .Combine(Environment
        .GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Store"));

    public string Name { get; }

    [JsonInclude]
    public string Password { private get; init; }

    public bool CheckPassword(string? password) => Password == password;

    private List<Product> _cart;

    public Customer(string name, string password)
    {
        Name = name.ToLower();
        Password = password;
        _cart = new();
    }

    public void AddToCart(Product product)
    {
        _cart.Add(product);
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine($"{Name} (Password: {Password})");
        sb.AppendLine();

        if (_cart?.Count > 0) sb.Append(GetCart());
        else sb.AppendLine("No items in cart.");

        return sb.ToString();
    }

    public string GetCart()
    {
        if (_cart.Count <= 0) return string.Empty;

        StringBuilder sb = new();

        IEnumerable<string> productTypes = _cart.DistinctBy(p => p.Name).Select(p => p.Name);

        int totCost = 0;
        foreach (string productType in productTypes)
        {
            int count = _cart.Count(p => p.Name == productType);
            int cost = _cart.Where(p => p.Name == productType).Sum(p => p.Price);
            Product item = _cart.First(p => p.Name == productType);
            
            sb.AppendLine($"{count}x {item.Name} ({item.Price} kr): {cost} kr");

            totCost += cost;
        }

        sb.AppendLine($"Total: {totCost} kr");

        return sb.ToString();
    }

    public static List<Customer> LoadCustomers()
    {
        FileInfo file = new(Path.Join(DataPath.FullName, "customers.json"));
        if (!file.Exists) return Array.Empty<Customer>().ToList();

        using StreamReader reader = new(file.FullName);
        string json = reader.ReadToEnd();

        List<Customer>? customers = JsonSerializer.Deserialize<List<Customer>>(json);

        return customers ?? Array.Empty<Customer>().ToList();
    }

    public static bool AddCustomer(string name, string password)
    {
        List<Customer> customers = LoadCustomers();
        if (customers.Any(c => c.Name == name)) return false;
        
        customers.Add(new(name, password));

        if (!DataPath.Exists) DataPath.Create();

        string jsonString = JsonSerializer.Serialize(customers, options: new() { WriteIndented = true });

        using StreamWriter writer = new(Path.Join(DataPath.FullName, "customers.json"), false);
        writer.Write(jsonString);

        return true;
    }

    public void SaveCart()
    {
        if (!DataPath.Exists) DataPath.Create();

        string jsonString = JsonSerializer.Serialize(_cart, options: new() { WriteIndented = true });

        using StreamWriter writer = new(Path.Join(DataPath.FullName, $"{Name}.json"), false);
        writer.Write(jsonString);
    }

    public static bool HasSavedCart(string name) =>
        new FileInfo(Path.Combine(DataPath.FullName, $"{name}.json")).Exists;

    public void LoadCart()
    {
        if(!HasSavedCart(Name)) return;

        using StreamReader reader = new(Path.Combine(DataPath.FullName, $"{Name}.json"));
        string json = reader.ReadToEnd();

        _cart = JsonSerializer.Deserialize<List<Product>>(json);
    }

    public void Checkout()
    {
        if (!HasSavedCart(Name)) return;
        
        FileInfo savedCart = new(Path.Combine(DataPath.FullName, $"{Name}.json"));
        savedCart.Delete();
    }
}
