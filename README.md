# README

## Project: E-Commerce Platform Backend

### Overview
This project is the **backend** for an end-to-end E-Commerce system, built using **C# ASP.NET Core Web API 8.0** and a **MongoDB Cloud** database. The backend serves a **central web service** that processes all client requests from both the web application (React) and mobile application (Android). It handles user management, product management, order processing, and vendor management, following a client-server architecture.

### Features

#### 1. User Management
- Role-based access control: **Administrator**, **Vendor**, and **Customer Service Representative (CSR)**.
- CSRs can approve customer accounts and handle order cancellations.
- Vendors can create and manage products, while Administrators oversee the system.

#### 2. Product Management
- Vendors can create, update, and delete products with a unique Product ID.
- Activation and deactivation of product categories.
  
#### 3. Order Management
- Order creation and tracking from "processing" to "delivered."
- CSRs and Administrators can cancel orders upon customer requests, with notifications sent to customers.
- Support for multi-vendor orders, where each vendor can update the status of their products.

#### 4. Inventory Management
- Real-time inventory tracking for vendors.
- Alerts and notifications for low stock levels, sent to the vendor.
- Inventory auto-updates based on product creation and sales.

#### 5. Vendor Management
- Admins can create vendor accounts.
- Customer ratings and comments for vendors are visible but cannot be modified.
- Average vendor ratings are calculated and displayed.

### Technologies Used
- **Framework**: ASP.NET Core Web API 8.0
- **Database**: MongoDB Cloud (NoSQL)
- **Hosting**: IIS (Internet Information Services)
  
### Installation Instructions

#### Prerequisites:
- **.NET SDK 8.0** or later
- **MongoDB Cloud** account for the NoSQL database setup
- **IIS** for web service hosting

#### Steps to Set Up the Backend

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   ```
   
2. **Navigate to the Backend Folder**:
   ```bash
   cd WebService
   ```

3. **Restore NuGet Packages**:
   ```bash
   dotnet restore
   ```

4. **Configure MongoDB**:
   - Open `appsettings.json` and update the MongoDB connection string:
     ```json
     "MongoDB": {
       "ConnectionString": "<your-mongodb-connection-string>",
       "Database": "ECommerceDB"
     }
     ```

5. **Build the Solution**:
   ```bash
   dotnet build
   ```

6. **Run the Application Locally**:
   ```bash
   dotnet run
   ```

7. **Publish to IIS**:
   - Publish the project from Visual Studio or use the command line:
     ```bash
     dotnet publish --configuration Release --output <iis-publish-path>
     ```

8. **IIS Setup**:
   - Ensure that IIS is properly configured to host the service.
   - Set the appropriate application pool and deploy the files.


### Configuration

- **MongoDB Cloud**: Ensure you have set up MongoDB Atlas and configured the connection string in `appsettings.json`.
- **IIS**: The web service must be hosted on **IIS** for production deployment. Ensure that the proper application pool settings are in place.

### Testing

- Use **Postman** or any other API testing tool to test the API endpoints.
- Mock data for testing can be inserted directly into MongoDB using a GUI like **MongoDB Compass**.

### Future Enhancements

- Add real-time inventory update notifications using WebSockets or SignalR.
- Implement payment gateway integration for real transactions.
- Introduce automated email notifications for order updates.

### License
This project is licensed under the MIT License.

---
