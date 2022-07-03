# SyncfusionHelpDesk WebAssembly

![image](https://user-images.githubusercontent.com/1857799/177015099-b270961a-eb97-41ad-89be-100ee9391c47.png)

## Covered in the Book:
[Blazor WebAssembly Succinctly](https://www.syncfusion.com/succinctly-free-ebooks/blazor-webassembly-succinctly)

### To Install

1) Create a Database on your SQL server, and run scripts in **!SQL directory**
2) Edit *appsettings.json* to set the database connection in the **DefaultConnection** property
3) Run the application, click the *Register* link and create a user named **Admin@email**
4) Log out and log back in as **Admin@email**. You will now be the **Administrator** 


### To Enable Emails

1) Get an **API key** from [app.sendgrid.com](https://app.sendgrid.com)
2) Open **appsettings.json**: 
- *Uncomment* the '//' before **SENDGRID_APIKEY** and enter your **SendGrid API key** in place of: **{{ uncomment and enter your key from app.sendgrid.com }}**
- *Uncomment* the '//' before **SenderEmail** and enter your Email address in place of: **{{ uncomment and enter your email address }}**

### Also See
* [SyncfusionHelpDesk - Sever Side Blazor version](https://github.com/ADefWebserver/SyncfusionHelpDesk)
