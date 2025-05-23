
Absolutely! You're using a **Clean Architecture** folder structure, which typically separates concerns into distinct layers.  
Let's break down the dependencies and references step-by-step so you know which project should reference which.

---

## 📁 Typical Clean Architecture Layers

Here's a recap of your structure:

### **Core**
- **Domain**: Contains entities and interfaces (no dependency on any other project).
- **Application**: Contains business logic and use cases (depends on **Domain**).

### **Infrastructure**
- **Infrastructure**: External services like email, file handling, etc. (depends on **Application** & **Domain**).
- **Persistence**: Contains EF Core, DB context, migrations (depends on **Application** & **Domain**).

### **Presentation**
- **WebApi**: ASP.NET Core API project (depends on **Application**, **Infrastructure**, and **Persistence**).

---

## ✅ Project Reference Setup

Below is the recommended way to add references between projects:

| From           | Reference Needed                  |
|----------------|------------------------------------|
| Application    | ➡ Domain                           |
| Infrastructure | ➡ Application, Domain              |
| Persistence    | ➡ Application, Domain              |
| WebApi         | ➡ Application, Infrastructure, Persistence |

---

## 🔧 How to Add References (in Visual Studio)

- Right-click on the project that needs to reference another.
- Choose **Add** → **Project Reference…**
- In the dialog, check the box for the project you want to reference.
- Click **OK**.

---

## 🛠 Example

In your case:

- Right-click on **Application** → Add Reference → Check **Domain**.
- Right-click on **Infrastructure** → Add Reference → Check **Application** and **Domain**.
- Right-click on **Persistence** → Add Reference → Check **Application** and **Domain**.
- Right-click on **WebApi** → Add Reference → Check **Application**, **Infrastructure**, and **Persistence**.

---

## 📺 Learning Resource

If you are looking for a playlist on Clean Architecture, follow the link below:

[🔗 Clean Architecture Playlist by CodeWithHanif](https://www.youtube.com/playlist?list=PLyTjFFFANHHfPsdxw_BX5IxK0Ayk-fCWv)

OR

Simply search on YouTube: **CodeWithHanif**

---
