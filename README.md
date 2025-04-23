# ASP.NET Core MVC Project Setup Guide (Other Device)

This README provides complete step-by-step instructions to run this ASP.NET Core MVC project on another device. It includes two methods for setting up the SQL Server database:

---

## 🛠 Requirements

Make sure the following software is installed:

- Visual Studio 2022 or later (with ASP.NET and web development workload)
- SQL Server Management Studio (SSMS)
- SQL Server (LocalDB or SQL Express)
- .NET SDK (same version as used in the project)
- Git (if cloning from repository)

---

## 📁 Step 1: Clone or Copy the Project

If you're using Git:

```bash
git clone https://github.com/ITZ-SALAM/Online-Mobile-Recharge.git
```

Or copy the full project folder manually to the new device.

---

## 🔧 Step 2: Open the Project

1. Open Visual Studio.
2. Click on `Open a project or solution`.
3. Browse and select the `.sln` file in the root of the project.

---

## ☝️ Step 3: Choose a Database Setup Method

You can either attach an existing `.mdf` database file or generate a new one using EF Core Migrations.

---

## 🔹 Method 1: Attach Existing Database (.mdf) in SQL Server

1. Open **SQL Server Management Studio (SSMS)**.
2. Right-click on **Databases > Attach...**
3. Click **Add** and select the `.mdf` file (usually located in `App_Data` or another folder in your project).
4. Once attached, copy the database name.

5. Open `appsettings.json` in the project and update the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

6. Save the file.

7. Build and run the project (Ctrl + F5).

---

## 🔸 Method 2: Use EF Core Migrations (Recommended)

> This method will automatically create the database schema using Entity Framework Core.

### Step A: Check `appsettings.json` Connection String

Make sure your `appsettings.json` has the correct server name:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Replace `YOUR_SERVER_NAME` with your local server (e.g., `(localdb)\MSSQLLocalDB` or `DESKTOP-XXXX\SQLEXPRESS`).

### Step B: Open Package Manager Console

1. Go to **Tools > NuGet Package Manager > Package Manager Console**
2. Run the following commands:

```powershell
Update-Database
```

> This will create the database with all the required tables.

If you don't have a migration created yet:

```powershell
Add-Migration InitialCreate
Update-Database
```

---

## 🔐 Default Admin Credentials

You can log in using the following admin account (already seeded):

- **Email:** admin@example.com
- **Password:** Admin@123

> Make sure you change the password in production for security purposes.

---

## ✅ Final Step: Run the Project

- Press `Ctrl + F5` to build and run the project.
- The default browser will open and load the home page.

---

## 📝 Additional Notes

- Run the project as **Administrator** if you face permission issues.
- Restore all NuGet packages before building the solution.
- Check `Program.cs` or `Startup.cs` for any required service configuration (like authentication, role management, etc.).
- Ensure your SQL Server instance is running if you're using a custom named instance.
- The project may use ASP.NET Core Identity for authentication and role management.

---

## 💬 Questions?

If you face any issue running the project, feel free to contact the project maintainer or raise an issue in the repository.

##  Github Repo



