# 🧀 Practical Digital Catalog (Dash)

A streamlined, lightweight, and elegant Android application developed in **C# (.NET 10 MAUI)**. The project was custom-built to automate product catalog updates and manage local inventory in a direct, hassle-free manner.

---

## 🚀 Key Features
* **Quick Product Registration:** Record name, description, weight, and price with automatic local persistence.
* **Flexible Image Capture:** Capture real-time photos using the device's camera or select existing images directly from the gallery.
* **Smart Inventory Control:** Dynamic toggle (via Switch) to display, hide, and manage the quantity of available items individually.
* **Flexible Generation:** Allows the catalog to be created even for items containing only a picture, ensuring a smooth user workflow.
* **Native PDF Generation (Zero Crashes):** Automatically compiles all saved products into a unified PDF document using an artisan card-style layout (centered cover page and framed item pages).
* **Direct Sharing:** Full integration with the operating system's native printing API for immediate distribution of the catalog via WhatsApp, Instagram, and other networks.

---

## 🗄️ Project Architecture
The project was structured following software development principles for separation of concerns, ensuring that each layer manages a specific core engine of the system:

* **`Produto.cs` (Model):** Defines the attributes and properties that characterize a product (Name, Price, Image Path, Inventory Control).
* **`DatabaseService.cs` (Data Persistence):** Functions as the local memory of the application. It utilizes **SQLite** asynchronously (`async/await`) to manage data storage directly on the device.
* **`MainPage.xaml` (User Interface - UI):** The visual layout of the application. It organizes the entry fields, intuitive buttons, and an artisan beige color palette, featuring a clean full-screen design with a footer section.
* **`MainPage.xaml.cs` (Controller/Logic):** The logical core that handles user interactions, manages file streams from media pickers (`Stream`), coordinates database persistence, and triggers the Android native printing engine.

---

## 🛠️ Technologies and Dependencies
* **Core Framework:** .NET 10.0 (MAUI)
* **Database:** `sqlite-net-pcl` (Lightweight and asynchronous local storage)
* **PDF Generation:** Powered by HTML5, CSS3, and the native Android `WebView` printing mechanism (Adoption choice to ensure Release mode stability and avoid heavy external desktop rendering dependencies like SkiaSharp).

---

## 🧠 Lessons Learned and Challenges Overcome
This application marked my transition from a Console environment (text-based interfaces) to native **Mobile development** workflows with user interfaces and data persistence. Throughout the project, I mastered practical concepts such as:

* **Media Lifecycle Management:** Manipulating, copying, and securing local image file streams from the hardware camera or gallery into the application's secure directories.
* **Production Build Optimization (Release Mode):** Resolving build conflicts, type injection issues, and Android permissions to produce a lightweight, fast, and stable APK.
* **Strategic Refactoring:** Migrating from heavy third-party rendering engines (QuestPDF) to built-in operating system solutions (Android Print APIs), prioritizing user experience and eliminating runtime failures (`TypeInitializationException`).

---

## 👨‍💻 About the Developer
**Marcelo Henrique**  
*Aspiring software developer focused on practical learning and building functional solutions to real-world problems.*

📫 **Contact:** marcelodamascenoh@gmail.com  
*If you need a dedicated, resilient developer focused on building functional solutions, or if you know of matching opportunities in the field, feel free to reach out!*
