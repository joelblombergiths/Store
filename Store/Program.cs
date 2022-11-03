using Store;
using System.Text;

List<Customer> customers = new();

List<Product> products = new()
{
    new("Plain Burger",50),
    new("Cheese Burger", 65),
    new("Gut Breaker", 125)
};

do
{
    try
    {
        customers = Customer.LoadCustomers();
    }
    catch (Exception)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("Failed to load list of customers");
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    Console.WriteLine("Welcome to the store Put-Awesome-Name-Here.");
    Console.WriteLine();
    Console.WriteLine("1. Login");
    Console.WriteLine("2. Create new Customer");
    Console.WriteLine("9. Exit");
    Console.Write(":");
    ConsoleKeyInfo input = Console.ReadKey();

    switch (input.KeyChar)
    {
        case '1': Login(); break;
        case '2': NewCustomer(); break;
        case '9': return;
    }
} while (true);

void NewCustomer(string? name = "")
{
    Console.Clear();

    Console.WriteLine("Register new customer");
    Console.WriteLine();

    Console.Write("Enter Name: ");
    if (string.IsNullOrWhiteSpace(name)) name = Console.ReadLine();
    else Console.Write($"{name}");

    if (string.IsNullOrWhiteSpace(name)) return;

    Console.WriteLine();
    string? password;
    do
    {
        Console.Write("Enter password: ");
        password = Console.ReadLine();
    } while (string.IsNullOrWhiteSpace(password));

    Console.Clear();

    if (!Customer.AddCustomer(name, password))
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("Customer already exists!");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("Customer created!");
    }

    Console.ForegroundColor = ConsoleColor.Gray;
}

void Login()
{
    string? name;
    do
    {
        Console.Clear();
        Console.WriteLine("Login");
        Console.Write("Name: ");
        name = Console.ReadLine();
    } while (string.IsNullOrWhiteSpace(name));

    Customer? customer = customers.Find(c => c.Name == name.ToLower());


    if (customer == null)
    {
        Console.Write("Customer does not exists, do you want to create it? [YES/no]: ");
        string? createChoice = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(createChoice)) createChoice = "yes";
        if (createChoice.ToLower().StartsWith("y")) NewCustomer(name);

        Console.Clear();
        return;
    }

    do
    {
        Console.Write("Password: ");

        StringBuilder pwd = new();
        do
        {
            ConsoleKeyInfo keyPress = Console.ReadKey(true);
            if (keyPress.Key == ConsoleKey.Enter) break;

            if (keyPress.Key == ConsoleKey.Backspace)
            {
                if (pwd.Length < 1) continue;

                pwd = pwd.Remove(pwd.Length - 1, 1);
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Console.Write(" ");
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                continue;
            }

            if (!char.IsAscii(keyPress.KeyChar)) continue;

            pwd.Append(keyPress.KeyChar);
            Console.Write("*");
        } while (true);

        string password = pwd.ToString();

        Console.WriteLine();
        if (string.IsNullOrEmpty(password)) return;

        if (customer.CheckPassword(password)) break;

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("Wrong Password!");
        Console.ForegroundColor = ConsoleColor.Gray;
    } while (true);

    Console.Clear();

    if (Customer.HasSavedCart(customer.Name))
    {
        try
        {
            customer.LoadCart();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Loaded previous saved cart");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        catch (Exception)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Failed to Load previous saved cart");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    ShowStore(customer);
}

void ShowStore(Customer customer)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Welcome {customer.Name}!");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine();

    bool doneShopping = false;
    do
    {
        Console.WriteLine("Store Menu.");
        Console.WriteLine();
        Console.WriteLine("1. Shop Products");
        Console.WriteLine("2. Show Cart");
        Console.WriteLine("3. Checkout");
        Console.Write(":");
        ConsoleKeyInfo input = Console.ReadKey();

        switch (input.KeyChar)
        {
            case '1': ShopProducts(customer); break;
            case '2': ShowCart(customer); break;
            case '3': doneShopping = Checkout(customer); break;
        }

        Console.Clear();
    } while (!doneShopping);
}

void ShopProducts(Customer customer)
{
    Console.Clear();

    do
    {
        Console.WriteLine("Awesome Products");
        Console.WriteLine();
        for (int i = 0; i < products.Count; i++)
        {
            Console.WriteLine($"{i + 1}: {products[i].Name} ({products[i].Price} kr)");
        }

        Console.WriteLine($"{products.Count + 1}: Done");
        Console.Write("Add to cart: ");
        ConsoleKeyInfo input = Console.ReadKey();

        if (!int.TryParse(input.KeyChar.ToString(), out int choice)
            || choice <= 0
            || choice > products.Count + 1)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Not a valid choice.");
            Console.ForegroundColor = ConsoleColor.Gray;
            continue;
        }

        choice -= 1;

        if (choice == products.Count) return;

        customer.AddToCart(products[choice]);

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"{products[choice].Name} added to cart.");
        Console.ForegroundColor = ConsoleColor.Gray;
    } while (true);
}

void ShowCart(Customer customer)
{
    Console.Clear();

    Console.WriteLine(customer);

    Console.WriteLine("Press the Any key to return to the store.");
    Console.ReadKey(true);
}

bool Checkout(Customer customer)
{
    Console.Clear();

    string cartContent = customer.GetCart();

    if (string.IsNullOrEmpty(cartContent))
    {
        Console.Write("Cart is empty.\nLeave store? [YES/no]: ");
        string? leaveChoice = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(leaveChoice)) leaveChoice = "yes";
        return leaveChoice.ToLower().StartsWith("y");
    }

    do
    {
        Console.Clear();
        Console.WriteLine();
        Console.Write(cartContent);
        Console.WriteLine();
        Console.WriteLine("1. Pay now");
        Console.WriteLine("2. Save cart");
        Console.WriteLine("3. Continue shoping");
        Console.Write(":");
        ConsoleKeyInfo cartChoice = Console.ReadKey();

        Console.WriteLine();
        switch (cartChoice.KeyChar)
        {
            case '1':
                customer.Checkout();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Payment Successful!");
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case '2':
                customer.SaveCart();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Cart saved");
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case '3':
                return false;
            default:
                continue;
        }

        Console.WriteLine("Press the Any key to return to the login screen.");
        Console.ReadKey(true);
        return true;
    } while (true);
}
