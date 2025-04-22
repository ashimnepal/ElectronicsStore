
# 🛒 Electronics Store E-commerce (ASP.NET Core)

This is a simple e-commerce web application for an Electronics Store built using **ASP.NET Core**, **Entity Framework Core**, and **SQLite**. It features user authentication with **ASP.NET Core Identity** and has separate interfaces for **Admin** and **Users**.

---

## 🚀 Features

### 🧑‍💼 Admin Panel
- Login with admin credentials
- Create, Read, Update, Delete (CRUD) products
- View list of registered users
- Monitor orders and user activities

### 👥 User Panel
- Register/Login using ASP.NET Core Identity
- Browse products
- Add products to cart
- Checkout and view order history

---

## 🛠️ Tech Stack

| Tech                     | Purpose                            |
|--------------------------|------------------------------------|
| ASP.NET Core MVC         | Web Framework                      |
| Entity Framework Core    | ORM for database access            |
| SQLite                   | Lightweight relational database    |
| ASP.NET Core Identity    | Authentication and user management |
| Razor Views              | Frontend rendering engine          |
| Bootstrap                | UI components and layout           |

---

## 📁 Project Structure

```
ElectronicsStore/
│
├── Controllers/           # MVC Controllers
├── Models/                # Data Models
├── Views/                 # Razor Views
├── Data/                  # Database context and migrations
├── wwwroot/               # Static files (CSS, JS, images)
├── appsettings.json       # Configuration
└── Startup.cs             # Middleware and service configuration
```

---

## 🔧 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/ElectronicsStore.git
   cd ElectronicsStore
   ```

2. **Install dependencies**
   Make sure you have the .NET SDK installed.

   ```bash
   dotnet restore
   ```

3. **Apply Migrations and Seed Database**
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to `https://localhost:5001`

---

## 🔐 Admin Access

Seed data includes a default admin:

- **Email**: `admin@electronicsstore.com`
- **Password**: `Admin@123!`

You can update this in the `Data/DbInitializer.cs` file.

---

## 🧪 Testing

Use the application locally to:

- Register as a new user
- Login and browse products
- Add items to your cart and simulate a checkout
- Login as admin and manage inventory

---

## 📌 To-Do

- Add order processing and email confirmation
- Enhance UI with responsive design
- Add product categories and search filter
- Implement payment gateway integration

---

## 📃 License

This project is open-source and available under the [MIT License](LICENSE).

