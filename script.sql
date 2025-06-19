CREATE TABLE Customer (
    CustomerID INT PRIMARY KEY IDENTITY,
    OrganizationName NVARCHAR(255),
    ContactPerson NVARCHAR(255),
    [Address] NVARCHAR(255),
    Phone NVARCHAR(50),
    Email NVARCHAR(255),
  [Login] nvarchar(15),
  [Password] nvarchar(15)
);

CREATE TABLE ChiefEnginner (
  ChiefEnginnerID INT PRIMARY KEY IDENTITY,
  FullName NVARCHAR(255),
  Phone NVARCHAR(50),
  Address NVARCHAR(255),
  [Login] nvarchar(15),
  [Password] nvarchar(15)
);

CREATE TABLE LeadSpecialist (
  LeadSpecialistID INT PRIMARY KEY IDENTITY,
  FullName NVARCHAR(255),
  Phone NVARCHAR(50),
  Address NVARCHAR(255),
  [Login] nvarchar(15),
  [Password] nvarchar(15)
);

CREATE TABLE Project (
    ProjectID INT PRIMARY KEY IDENTITY,
    ProjectName NVARCHAR(255),
    Notes NVARCHAR(MAX),
    CustomerID INT,
  ChiefEnginnerID INT,
  LeadSpecialistID INT,
    FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID), 
    FOREIGN KEY (ChiefEnginnerID) REFERENCES ChiefEnginner(ChiefEnginnerID),
  FOREIGN KEY (LeadSpecialistID) REFERENCES LeadSpecialist(LeadSpecialistID)
);

CREATE TABLE Operator (
    OperatorID INT PRIMARY KEY IDENTITY,
    FullName NVARCHAR(255),
    Phone NVARCHAR(50),
    Address NVARCHAR(255),
    [Login] nvarchar(15),
    [Password] nvarchar(15)
);

CREATE TABLE Flight (
    FlightID INT PRIMARY KEY IDENTITY,
    StartDateTime DATETIME,
    EndDateTime DATETIME,
    AltitudeAboveSea FLOAT,
    AltitudeAboveGround FLOAT,
    Speed FLOAT,
    ProjectID INT,
    OperatorID INT,
    FOREIGN KEY (ProjectID) REFERENCES Project(ProjectID),
    FOREIGN KEY (OperatorID) REFERENCES Operator(OperatorID)
);

CREATE TABLE Area (
    AreaID INT PRIMARY KEY IDENTITY,
    GeologicalInfo NVARCHAR(MAX),
    Area FLOAT,
  ProfileCount INT,
  BreaksCount INT,
    ProjectID INT,
    FOREIGN KEY (ProjectID) REFERENCES Project(ProjectID) ON DELETE CASCADE
);

CREATE TABLE [Profile] (
    ProfileID INT PRIMARY KEY IDENTITY,
    BreaksCount INT,
    AreaID INT,
    FOREIGN KEY (AreaID) REFERENCES Area(AreaID) 
);

CREATE TABLE Spectrometer (
    SpectrometerID INT PRIMARY KEY IDENTITY,
    MeasurementTime FLOAT,
    PulseCount INT,
    TotalCount INT,
    EnergyWindowsCount INT,
    FlightID INT,
    FOREIGN KEY (FlightID) REFERENCES Flight(FlightID) 
);

CREATE TABLE Metadata (
    MetadataID INT PRIMARY KEY IDENTITY,
    EquipmentDescription NVARCHAR(MAX),
    Notes NVARCHAR(MAX),
  SpectrometerID INT,
  FOREIGN KEY (SpectrometerID) REFERENCES Spectrometer(SpectrometerID) ON DELETE CASCADE
);

CREATE TABLE AreaCoordinates (
    AreaCoordinatesID INT PRIMARY KEY IDENTITY,
    X FLOAT,
    Y FLOAT,
    AreaID INT,
    FOREIGN KEY (AreaID) REFERENCES Area(AreaID)
);

CREATE TABLE ProfileCoordinates (
    ProfileCoordinatesID INT PRIMARY KEY IDENTITY,
    X FLOAT,
    Y FLOAT,
    ProfileID INT,
    FOREIGN KEY (ProfileID) REFERENCES Profile(ProfileID)
);

CREATE TABLE Channel1 (
    Channel1ID INT PRIMARY KEY IDENTITY,
    MeasurementResult FLOAT,
    ProfileCoordinatesID INT,
    FOREIGN KEY (ProfileCoordinatesID) REFERENCES ProfileCoordinates(ProfileCoordinatesID)
);

CREATE TABLE Channel2 (
    Channel2ID INT PRIMARY KEY IDENTITY,
    MeasurementResult FLOAT,
    ProfileCoordinatesID INT,
    FOREIGN KEY (ProfileCoordinatesID) REFERENCES ProfileCoordinates(ProfileCoordinatesID)
);

CREATE TABLE Channel3 (
    Channel3ID INT PRIMARY KEY IDENTITY,
    MeasurementResult FLOAT,
    ProfileCoordinatesID INT,
    FOREIGN KEY (ProfileCoordinatesID) REFERENCES ProfileCoordinates(ProfileCoordinatesID)
);
