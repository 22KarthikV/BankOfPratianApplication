-- Create ACCOUNT table
CREATE TABLE ACCOUNT (
    accNo VARCHAR(15) PRIMARY KEY,
    name VARCHAR(30) NOT NULL,
    pin VARCHAR(4) NOT NULL,
    active BIT NOT NULL,
    dtOfOpening DATE NOT NULL,
    balance FLOAT NOT NULL,
    privilegeType VARCHAR(15) NOT NULL,
    accType VARCHAR(15) NOT NULL,
    CONSTRAINT CHK_PrivilegeType CHECK (privilegeType IN ('REGULAR', 'GOLD', 'PREMIUM')),
    CONSTRAINT CHK_AccountType CHECK (accType IN ('SAVINGS', 'CURRENT'))
);

-- Create TRANSACTION table
CREATE TABLE [TRANSACTION] (
    TransID INT PRIMARY KEY,
    TransactionType VARCHAR(20) NOT NULL,
    accNo VARCHAR(15) NOT NULL,
    TransDate DATETIME NOT NULL,
    amount FLOAT NOT NULL,
    status VARCHAR(10) NOT NULL,
    FOREIGN KEY (accNo) REFERENCES ACCOUNT(accNo),
    CONSTRAINT CHK_TransactionType CHECK (TransactionType IN ('DEPOSIT', 'WITHDRAW', 'TRANSFER', 'EXTERNALTRANSFER')),
    CONSTRAINT CHK_TransactionStatus CHECK (status IN ('OPEN', 'CLOSED'))
);

-- Create POLICY table
CREATE TABLE POLICY (
    PolicyID INT PRIMARY KEY IDENTITY(1,1),
    PolicyType VARCHAR(30) NOT NULL,
    MinBalance FLOAT NOT NULL,
    RateOfInterest FLOAT NOT NULL,
    UNIQUE (PolicyType)
);

-- Insert default policies
INSERT INTO POLICY (PolicyType, MinBalance, RateOfInterest) VALUES
('SAVINGS-REGULAR', 5000.0, 4.0),
('SAVINGS-GOLD', 25000.0, 4.25),
('SAVINGS-PREMIUM', 100000.0, 4.75),
('CURRENT-REGULAR', 25000.0, 2.0),
('CURRENT-GOLD', 100000.0, 2.25),
('CURRENT-PREMIUM', 300000.0, 2.75);

-- Create EXTERNALBANK table
CREATE TABLE EXTERNALBANK (
    BankCode VARCHAR(10) PRIMARY KEY,
    BankName VARCHAR(50) NOT NULL
);

-- Insert some example external banks
INSERT INTO EXTERNALBANK (BankCode, BankName) VALUES
('ICICI', 'ICICI Bank'),
('CITI', 'Citibank');