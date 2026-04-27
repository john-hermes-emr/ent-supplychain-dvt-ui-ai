# DVT Application: Software Requirements Specification (SRS)

## 1. Introduction

### 1.1 Purpose
This document specifies the software requirements for the Data Validation Tool application. It provides both functional and non-functional requirements to guide development.

The DVT tool is used by Emerson divisions to validate all extract files prior to submitting them to corporate procurement (MIN).   
 
Each division loads pipe delimited ASCII files for the following type of extracts: 
- Vouchered Invoice Receipt (VIR)
- Inventory, and Demand.  
- Item (Part Master)
- Supplier
- Purchase Orders
- Purchase Order Items
- MPN (Manufacturer Part Number)
- UOM (Unit of Measure)


In addition to this data, each division will load the following foundation data; Supplier & Item Master File. 
These files must be loaded so that the rollups for Commodities and suppliers can be calculated. 
These loads are performed monthly and must be submitted by noon (Central Standard Time) on the 15th of each month.

### 1.2 Scope
This document will concentrate on the requirements for validating the files provided by the various Emerson divisions. 
The document will not have much detail on the source systems and the Emerson FAW target system.

## 2. Overall Description
The application must present the user with an interface that allows them to select one or more files to import from a source folder defined by the user. 
These files are then validated against the various rules in the system that will be detailed below. 
Once the files are validated, the user will be able to review any validation issues with the files and submit them to be sent to the FAW system.

### 2.1 Product Perspective
Emerson has multiple ERP systems used throughout all its divisions. These ERP system are all different versions and providers which makes it very challenging to ensure that the MIN data that is fed to the FAW system is consistent. The main reason for the DVT is to ensure that all the data going into FAW is consistent and meets the minimum set of requirements to be accepted into the data warehouse.

### 2.3 User Classes
- Administrators: Users that are allowed to perform all actions within the system.

## 3. Functional Requirements

### 3.1 User Account Requirements
- **FR-3.1.1:** All users in the application shall be considered administrators
- **FR-3.1.2:** Users shall be able to log in using their Emerson credentials

### 3.2 Home Folder Selection Requirements
- **FR-3.2.1:** The user shall be able to set their preferred file home folder from within their designated DVT file share.
- **FR-3.2.2:** The user shall be able to select a preferred home folder from the user preferences section.
- **FR-3.2.3:** The user shall be able to select a home folder from a listing of available folders in their designated file share.

TODO: Add log folder and Output folder

### 3.3 File Selection Requirements
- **FR-3.3.1:** The user shall be able to select a division from the division drop-down list in the home screen.
- **FR-3.3.2:** The user shall be able to select a valid feed number (1-99) from the feed number selector in the home screen.    
- **FR-3.3.3:** The application shall select files from the user's home folder based on the selection of division and feed number
    - Once the user has selected a value for division and feed number, then the application shall select the matching files from the user's home folder and display them in the home screen grid.
- **FR-3.3.4:** If the user does not have a home folder defined an error message shall be displayed asking the user to define one in the preferences section.

### 3.4 Home Screen Grid Requirements
- **FR-3.4.1:** The home screen grid shall have the following columns: Table, Filename, File Date, Record Count, Load Date, Status
- **FR-3.4.2:** The table column shall display the name of the table corresponding to the file shown on the grid.
- **FR-3.4.3:** The Filename column shall display the name of the physical file in the user's home folder.
- **FR-3.4.4:** The File Date column shall display the date in which the file was created or last saved.
    - The date shall be displayed in the following format MM/DD/YYYY HH:MM:SS AM/PM 
    - The date shall be shown in the user's local time zone.
- **FR-3.4.5:** The Record Count column shall display the number of records in the particular file.
- **FR-3.4.6:** The Load Date column shall display the date and time when the file was loaded into the DVT system.        
    - The date shall be displayed in the following format MM/DD/YYYY HH:MM:SS AM/PM 
    - The date shall initially be blank when the user first selects file files as the files have not been loaded yet.
    - When the user clicks on the "Load Extract Files" button, the load date column shall be populated.
- **FR-3.4.7:** The Status column shall display the status of the file after it has been processed by the DVT system.    
    - The system shall begin processing the files when the user clicks the "Load Extract Files" button.
    - The following status values shall be used by the system:
        - ERRORS - Validation process contains errors
        - LOADED - Validation process contains no errors for all records
        - WARNING - Validation process contains warnings
        - CRITICAL - Cannot transmit any records due to critical validation error 
- **FR-3.4.8:** The Status column shall be blank if the system has not yet processed the files.    
- **FR-3.4.9:** If a file has warnings and errors then errors take precedence. ERRORS shall be displayed in the status column.
- **FR-3.4.10:** If a file has no errors and warnings then warnings take precedence. WARNING shall be displayed in the status column.
- **FR-3.4.11:** If a file has any critical errors then critical takes precedence over all other statuses. CRITICAL shall be displayed in the status column. 


### 3.5 Filename Requirements
- **FR-3.5.1:** The system shall recognize files that follow the following naming convention
    - [division abbrev]_[feed ID number]_vir_o.txt
    - [division abbrev]_[feed ID number]_inv_o.txt
    - [division abbrev]_[feed ID number]_item_o.txt
    - [division abbrev]_[feed ID number]_po_o.txt
    - [division abbrev]_[feed ID number]_poitem_o.txt
    - [division abbrev]_[feed ID number]_supplier_o.txt
    - [division abbrev]_[feed ID number]_mpn_o.txt
    - [division abbrev]_[feed ID number]_uom_o.txt

### 3.2 File Load Requirements
- **FR-3.2.1:** The "Load Extract Files" button shall be disabled when the user first arrives in the home screen.
- **FR-3.2.1:** The "Load Extract Files" button shall be enabled after the user has selected a division, feed number and has selected one or more files from the grid.    

### 3.2 File Dependency Requirements
- **FR-3.2.1:** When the user selects one or more files, any files that have dependencies on the selected files shall also have to be selected.
- **FR-3.2.2:** If the user does not select all dependent files while triggering the file load, the application shall display a warning message stating that dependent files have not been selected.    

### 3.2 File Clear Requirements
- **FR-3.2.1:** When file grid does not have any files, the Clear button shall be disabled
- **FR-3.2.2:** When the user clicks the Clear button, a confirmation dialog shall appear. If the user, clicks to continue in the confirmation dialog, then any active job shall be deleted and the grid will be cleared.
- **FR-3.2.3:** TBD

### 3.2 File Refresh Requirements
- **FR-3.2.1:** When there is no active project being worked on, then the Refresh button shall be disabled.
- **FR-3.2.1:** When there is a job being processed, then the Refresh button shall be disabled.
- **FR-3.2.2:** When the Refresh button is pressed, the application shall display a confirmation dialog asking the user if they would like to continue. If the user clicks on continue, then the application shall locate and reload the files based on the division and feed selections from the user.
- **FR-3.2.3:** TBD

### 3.2 General File Validation Requirements
- **FR-3.2.1:** Files shall be validated on four main criteria:
    - Static Field Validation - Integers shall be positive, string shall be of a certain length, string fields shall contain a specific value.
    - File Dependency - File A depends on File B and C. If File B or C are missing from the process, the validation will fail.
    - Dependent Field Validation - Fields in one file may need to be present on another file.
    - Master Data Validation - Certain fields shall need to exist in a master data table such as unit of measure or material numbers.
    - Field Uniqueness - Certain combinations of fields shall not be repeated in any one file. i.e. First Name and Last Name cannot repeat in a file.    
- **FR-3.2.2:** Fields that are designated as mandatory shall not contain any blanks or nulls. If any mandatory field is missing then, the file status shall be CRITICAL.
- **FR-3.2.3:** File headers shall match the defined headers stated in the file definition. If any file headers are missing or out of order, then the file status shall be CRITICAL.

### 3.2 VIR File Validation Requirements
**Description**
Vouchered Invoice Receipts maintains the history of vouchered receipts from Account Payable system. The receipt history is a record of invoiced items that have been received and approved as payable.  This should include items received in the designated time period AND vouchered prior to extract, though not necessarily paid.

#### FR - Column enumeration

| Field Name           | Data Type | Length | Mandatory | Error Status |Description                                                           |
|----------------------|-----------|--------|-----------|--------------|----------------------------------------------------------------------|
| DIVISION ID          | TEXT      | 100    | M         | CRITICAL     | Business Unit ID|
| LOCAL SITE ID        | TEXT      | 100    | M         | CRITICAL     | Local Site ID   |
| RECEIPT NUMBER       | TEXT      | 50     | M         | ERRORS       | Unique identifier for the receipt|
| SUPPLIER ID          | TEXT      | 100    | M         | WARNING      | ID of the supplier as reflected in the supplier file|
| PART NUMBER          | TEXT      | 50     | M         | WARNING      | Business Units internal part number received|
| QUANTITY RECEIVED    | Number    | 15     | M         | ERRORS       | Total Quantity in which was received/delivered|
| INVOICE PRICE PAID   | Number    | 38     | M         | ERRORS       | Total Invoice price paid for the items received (value should be in currency it will be paid)|
| INVOICE PRICE PAID   | Number    | 38     | M         | WARNING      | Total Invoice price paid for the items received (value should be in currency it will be paid)|
| UNIT PRICE           | Number    | 38     | M         | ERRORS       | Unit price for the items received|
| PURE_LOADED COST     | TEXT      | 50     | M         | ERRORS       | P = Pure Cost; <br/>L = Loaded Cost (cost may include freight, duty, overhead etc)|
| CURRENCY CODE        | TEXT      | 10     | M         | ERRORS       | Currency associated with unit price and Invoice Price Paid|
| UOM                  | TEXT      | 20     | M         | ERRORS       | Unit of Measure for Quantity Received associated with Unit Price|
| DIRECT_INDIRECT      | TEXT      | 10     | M         | ERRORS       | Default = D (Direct)|
| INTRA-DIV            | TEXT      | 10     | M         | ERRORS       | Intra Company Purchase. Default = N|
| FREIGHT TERMS        | TEXT      | 50     | O         | ERRORS       | Freight Term (Incoterms or domestic freight term)|
| DATE RECEIVED        | DATE      | 8      | M         | ERRORS       | Date when material was received on the dock|
| PO TERMS             | TEXT      | 128    | M         | CRITICAL     | Purchase order terms in A/P system|
| PO Number            | TEXT      | 50     | M         | CRITICAL     | Unique identifier for the PO|
| PO Line Number       | TEXT      | 50     | M         | CRITICAL     | Unique identifier for a PO Line item number|
| Supplier Part Number | TEXT      | 50     | O         | -            | Supplier Part Number |
| Quantity Ordered     | Number    | 15     | O         | -            | Total quantity ordered (reference only)|
| Title Transfer       | TEXT      | 50     | O         | -            | Title Transfer codes|
| Port                 | TEXT      | 10     | O         | -            | Specify Air or Sea port code for all named place, named port of destination, named port of shipment|
| Release#             | Number    | 50     | O         | -            | The number associated to all blanket purchase order with PO type flagged as “B” used to release material for delivery depending on the information on the requisition line|
| Committed Date       |           |        |           |              | The date committed to by the supplier for delivery. Specify in YYYYMMDD format. This is equivalent to supplier “promise date”, or default “need by date” that has been confirmed as committed delivery date by the supplier |

#### FR - Static Field Validation  
The following fields shall be validated using static field validation 
    - RECEIPT NUMBER
      - Shall only contain ASCII English Characters    
    - QUANTITY RECEIVED
      - Shall not be 0 or a negative number
      - Shall not be a non-numeric char      
    - INVOICE PRICE PAID
      - Shall not be 0 or a negative number
      - Shall not be a non-numeric char      
    - UNIT PRICE
      - Shall not be 0 or a negative number
      - Shall not be a non-numeric char      
    - DATE RECEIVED
      - Date provided shall not be in a future month
      - Date format shall be YYYYMMDD
      - Validation Message: INVALID FORMAT    
    - PURE_LOADED COST
      - Acceptable values shall be P or L
      - Validation Message: INVALID VALUE
      - Status Message: ERRORS
    - DIRECT_INDIRECT
      - Acceptable values shall be D
      - Validation Message: INVALID VALUE
      - Status Message: ERRORS
    - INTRA-DIV
      - Acceptable values shall be N
      - Validation Message: INVALID VALUE
      - Status Message: ERRORS

- **FR-3.2.2:** Character Limit Validation
    - The fields listed on Column Enumeration section shall be validated for character length against the length provided in the table.
    - If any text field in a particular row is longer than the permitted limit then:
      - Validation Message: CHARACTER LIMIT HAS BEEN EXCEEDED
      - Status Message: ERRORS

- **FR-3.2.3:** Mandatory Field Validation
    - Fields listed in the Column Enumeration section having an M in the *Mandatory* column shall be designated as mandatory.
    - Mandatory fields that are missing in the source file shall cause the validation to fail.
      - Validation Message: NULL VALUE FOUND IN MANDATORY FIELDS
      - Status Message: CRITICAL

- **FR-3.2.4:** Master Data Validation
The following fields shall be validated against the master data tables
    - DIVISION ID
      - The DIVISION ID shall exist in the list of divisions.
      - Validation Message: DIVISION ID NOT FOUND
      - Status Message: CRITICAL
    - LOCAL SITE ID
      - The LOCAL SITE ID shall exist in the list of sites.
      - Validation Message: LOCAL SITE ID NOT FOUND
      - Status Message: CRITICAL
    - UOM
      - The UOM shall exist in the list of UOM codes.
      - Validation Message: UOM CODE NOT FOUND
      - Status Message: ERRORS
    - CURRENCY CODE
      - The CURRENCY CODE shall exist in the list of CURRENCY CODE.
      - Validation Message: CURRENCY CODE NOT FOUND
      - Status Message: ERRORS
    - FREIGHT TERMS
      - The FREIGHT TERMS shall exist in the list of FREIGHT TERMS.
      - Validation Message: FREIGHT TERMS NOT FOUND
      - Status Message: ERRORS

- **FR-3.2.5:** Duplicate Record Validation  
Each row in the file shall be unique as defined by a uniqueness key.  
If one or more rows have the same uniqueness key, they shall be flagged as duplicates with the following messages:  
  - Validation Message: DUPLICATE SOURCE RECORD FOUND
  - Status Message: CRITICAL
  
  The uniqueness key is defined by the concatenation of the following fields:
  - DIVISION ID
  - LOCAL SITE ID
  - RECEIPT_NUMBER
  - PO_NUMBER
  - PO_LINE_NUMBER
  - PART_NUMBER
  - DATE_RECEIVED
  - COMMITTED_DATE
  - RELEASE#  

- **FR-3.2.6:** Dependent File Validation  
As mentioned in previous sections, files may have dependencies on other files. The VIR file depends on the supplier, item.  
  - Supplier File
    - Dependent Field: SUPPLIER ID
    - Validation Criteria: The SUPPLIER ID field must exist in the supplier file.
    - Validation Message: SUPPLIER ID NOT FOUND
    - Status Message: WARNING
  - Item File  
    - Dependent Field: PART NUMBER  
    - Validation Criteria: Part number must exist in the item file.
    - Validation Message: PART NUMBER NOT FOUND
    - Status Message: WARNING  
  
- **FR-3.2.7:** Dynamic Field Validation  
The INVOICE PRICE PAID field shall be validated using a formula  
    - Validation Criteria: Invoice Price Paid shall be equal to Unit Price x Quantity Received  
    - Validation Message: INVOICE PRICE PAID MISMATCH
    - Status Message: WARNING



### 3.2 File Analysis Requirements
- **FR-3.2.1:** TBD
- **FR-3.2.2:** TBD
- **FR-3.2.3:** TBD

### 3.2 XXX Requirements
- **FR-3.2.1:** TBD
- **FR-3.2.2:** TBD
- **FR-3.2.3:** TBD

### 3.2 XXX Requirements
- **FR-3.2.1:** TBD
- **FR-3.2.2:** TBD
- **FR-3.2.3:** TBD

### 3.2 XXX Requirements
- **FR-3.2.1:** TBD
- **FR-3.2.2:** TBD
- **FR-3.2.3:** TBD

### 3.2 XXX Requirements
- **FR-3.2.1:** TBD
- **FR-3.2.2:** TBD
- **FR-3.2.3:** TBD



## 4. Non-Functional Requirements

### 4.1 Security
- **NFR-4.1.1:** Access to the application shall be controlled using an Active Directory group
- **NFR-4.1.2:** User authentication shall employ secure protocols
- **NFR-4.1.3:** The system shall protect against common security vulnerabilities (SQL injection, XSS, CSRF)
- **NFR-4.1.4:** User sessions shall timeout after a period of inactivity
- **NFR-4.1.5:** Container images shall be scanned for vulnerabilities before deployment
- **NFR-4.1.6:** Each user will have a dedicated file share that shall be accessible to the user via a network drive on their machine.
- **NFR-4.1.7:** Only the individual user and system admins shall have access to each users designated file share.
- **NFR-4.1.8:** Users shall be allowed to create and delete folders inside their designated file share.

### 4.2 Performance
- **NFR-4.2.1:** The application shall load within 5 seconds under normal conditions
- **NFR-4.2.2:** The system shall support multiple concurrent users without performance degradation
- **NFR-4.2.3:** Database operations shall complete within 1 second
- **NFR-4.2.4:** The system shall remain responsive during peak usage

### 4.3 Usability
- **NFR-4.3.1:** The interface shall follow modern design principles with intuitive navigation
- **NFR-4.3.2:** The application shall be accessible to users with different abilities
- **NFR-4.3.3:** Error messages shall be clear and provide guidance for resolution
- **NFR-4.3.4:** The application shall store all times in UTC time zone.

### 4.4 Compatibility
- **NFR-4.4.1:** The application shall function correctly on major web browsers
- **NFR-4.4.2:** The application shall adapt to different screen sizes and orientations

### 4.5 Maintainability
- **NFR-4.5.1:** The codebase shall follow consistent coding standards
- **NFR-4.5.2:** The architecture shall be modular to allow for future extensions
- **NFR-4.5.3:** The system shall be designed to accommodate additional features

### 4.6 Documentation
- **NFR-4.6.1:** User documentation shall be provided explaining all features
- **NFR-4.6.2:** Technical documentation shall be maintained for developers
- **NFR-4.6.3:** Deployment and infrastructure documentation shall be maintained

### 4.7 Deployment and Infrastructure
- **NFR-4.7.1:** The application shall be containerized following best practices
- **NFR-4.7.2:** The system shall be deployed on Azure Kubernetes Service (AKS)
- **NFR-4.7.3:** The deployment shall support horizontal scaling for increased load
- **NFR-4.7.4:** The CI/CD pipeline shall include automated testing stages
- **NFR-4.7.5:** The deployment process shall support rolling updates with minimal downtime
- **NFR-4.7.6:** Infrastructure shall be defined as code using appropriate tools
- **NFR-4.7.7:** Monitoring and logging solutions shall be implemented for observability

## 5. Glossary
| Term | Definition |
|------|------------|
|FAW|Fusion Analytics Warehouse|
|VIR|Vouchered Invoice Receipts|
|INV|Inventory|
|PO|Purchase Order|
|UOM|Unit of Measure|
|MIN|Material Information Network            |
|MRP/ERP|Material Resource Planning / Enterprise Resource Planning|

## 6. Future Enhancements
- Task priorities
- Collaboration features
- Task dependencies
- Advanced search capabilities including tag combinations
- Integration with other productivity tools
- Tag analytics and usage statistics
