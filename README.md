# Bank of Pratian Application

## Overview

The Bank of Pratian Application is a robust banking system that provides a wide range of functionalities for managing accounts, transactions, and external transfers. This application is designed to handle various banking operations efficiently and securely.

## Features

- **Account Management**
  - Create new accounts (Savings and Current)
  - Display account information
  - Support for different privilege types (Regular, Gold, Premium)

- **Transaction Management**
  - Deposit funds
  - Withdraw funds
  - Transfer funds between accounts
  - External fund transfers

- **Reporting**
  - Display bank statistics
  - View all transactions
  - Display all deposits, withdrawals, and transfers

- **Policy Management**
  - Dynamic policy application based on account type and privilege

- **External Bank Integration**
  - Support for external bank transfers
  - Configurable external bank services

## Technical Stack

- **Language**: C#
- **Framework**: .NET Framework
- **Database**: SQL Server
- **Logging**: NLog
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

## Setup Instructions

1. **Clone the Repository**
   ```
   git clone https://github.com/your-username/BankOfPratianApplication.git
   ```

2. **Database Setup**
   - Create a new SQL Server database named `BankOfPratianDB`
   - Run the SQL scripts in the `DatabaseScripts` folder to create the necessary tables

3. **Configuration**
   - Update the connection string in `App.config` to point to your SQL Server instance
   - Configure external bank services in `App.config` under the `ServiceBanks` app setting

4. **Build the Solution**
   - Open the solution in Visual Studio
   - Restore NuGet packages
   - Build the solution

5. **Run the Application**
   - Set `BankOfPratian.Console` as the startup project
   - Run the application

## Usage

Upon running the application, you'll be presented with a menu-driven interface. Follow the on-screen prompts to navigate through different banking operations.

## Contributing

Contributions to the Bank of Pratian Application are welcome. Please follow these steps to contribute:

1. Fork the repository
2. Create a new branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Contact

KARTHIK V - karthikmudaliar20@gmail.com

Project Link: https://github.com/your-username/BankOfPratianApplication

## Acknowledgements

- [NLog](https://nlog-project.org/)
- [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)
