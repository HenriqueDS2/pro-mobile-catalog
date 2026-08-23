# 🧀 Practical Digital Catalog

A streamlined and elegant Android application developed in C# using **.NET MAUI**. It is designed to automate product catalog updates and manage local inventory in a direct, hassle-free manner.

---

## 🚀 Features

- **Quick Product Registration:** Easily record name, description, weight, and price.
- **Flexible Image Capture:** Option to take real-time photos using the device's native camera or select existing images directly from the gallery.
- **Smart Inventory Control:** Dynamic activation (via Switch) to display and manage the quantity of available items.
- **PDF Catalog Generation:** Automatic compilation of all saved products into a unified, elegant PDF document (crafted in an artisan card style).
- **Native Sharing:** Seamless integration with the system API for immediate distribution of the generated PDF via WhatsApp, Instagram, Messenger, and other networks.

---

## 🗄️ Project Architecture

The project was structured following software development best practices for separation of concerns (*specialized layers*), ensuring that each file manages an exclusive engine of the system:

1. **`Produto.cs` (Model):** Defines the identity and attributes that characterize a product (Name, Price, Inventory, etc.).
2. **`DatabaseService.cs` (Data Persistence):** Functions as the long-term memory (*Memory Card*). It leverages **SQLite** to automatically create tables and store data locally on the device using asynchronous operations (`async/await`).
3. **`MainPage.xaml` (User Interface - UI):** The visual layout of the application. It is responsible for organizing the fields, the elegant artisan beige color palette, and the intuitive buttons.
4. **`MainPage.xaml.cs` (Controller/Brain):** The logical core that intercepts screen interactions, handles media file streams, commands database persistence, and orchestrates the **QuestPDF** engine to render the document.

---

## 🛠️ Technologies and Dependencies

- **Core Framework:** .NET 10.0 (MAUI)
- **Database:** `sqlite-net-pcl` (Lightweight and asynchronous local storage)
- **PDF Generation:** `QuestPDF` (Modern engine for fluid layout rendering)

---

## 🧠 Lessons Learned and Challenges Overcome

This application marked my transition from a Console environment (text-based interface) to a native Mobile development workflow with user interfaces and data persistence. Throughout the project, I mastered practical concepts such as:

- **Media Lifecycle Management:** Manipulating and copying local image file streams from the gallery or camera into the app's secure directories.
- **Asynchronous Lifecycle (`Task`, `async/await`):** Implementing database operations and background file rendering to prevent user interface freezes.
- **Third-Party Library Integration:** Managing and consuming NuGet packages (SQLite and QuestPDF) integrated directly with C# business logic.
