# MOZ_UPGRADE – Standard Insurance Client Portal

![.NET](https://img.shields.io/badge/.NET-6.0-purple.svg)
![Blazor](https://img.shields.io/badge/Blazor-Server-blueviolet.svg)
![Status](https://img.shields.io/badge/Status-Active-brightgreen.svg)

Modern client portal for **Standard Insurance**, built on ASP.NET Core Blazor and integrated with **Microsoft Dynamics 365 ERP**. The portal lets customers:

- Complete KYC
- View and acknowledge insurer quotes
- See related policies and debit notes
- Submit and track claims
- Make payments on debit notes

Back‑office users in ERP see the same data via OData endpoints, while management receives **styled email notifications** for key customer actions.

### Key Features

- **KYC & Contact Management**  
  Individual and corporate KYC flows, synchronized with BC `ContactCard`.

- **Product Selection & Quotes**  
  Product recommendations are read from BC `ProductSelection` and exposed as **Insurer Quotes** to the client. Quotes can be **acknowledged** and **selected**, driving BC logic via OData and custom functions.

- **Policies & Debit Notes**  
  For a selected quote, policies are loaded from BC `PolicyCard` by `Quotation_No`.  
  The **MyPyments** page lists debit notes from BC filtered by the customer’s `Insured_Number`.

- **Claims**  
  Customers can create new claims (BC `Claimscsrdpage`) from policies, and view all their claims on **My Claims**, filtered by `InsuredNo`.

- **Payments**  
  Customers submit debit note payments from the portal. BC receives a `DebitNotePayments` record; the portal shows success to the user and notifies management by email.

- **Management Notifications (Email)**  
  SMTP‑driven HTML emails, including:
  - Product submitted for quote review (Summary page)
  - Quote selection notifications
  - Payment notifications

- **Modern UI**  
  Blazor + MudBlazor layouts for dashboard, receipts, claims, quote details, and KYC flows.

## 🏗️ Architecture

### Technology Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Blazor Server, MudBlazor, HTML5, CSS3 |
| **Backend** | ASP.NET Core 6.0, C# |
| **Integration** | Microsoft Dynamics 365 ERP via OData v4 |
| **Auth** | Custom Authentication State Provider, ASP.NET Identity models |
| **UI Components** | MudBlazor Material Design |

### Selected Project Structure

```text
MOZ_UPGRADE/
├── Pages/
│   ├── DASHBOARD/
│   │   ├── Dashboard.razor              # Main dashboard
│   │   ├── Summary.razor                # Plan & beneficiary summary, BC submit + email
│   │   ├── InsurerQuotes.razor          # ProductSelection quotes from BC
│   │   ├── QuoteDetails.razor           # Quote details + related policies & debit notes
│   │   ├── MyClaims.razor               # Claims list filtered by InsuredNo
│   │   ├── MyDebitNotes.razor           # "MyPyments" debit notes listing
│   │   ├── MakePaymentDialog.razor      # Payment modal dialog
│   │   └── Profile.razor                # Profile and linked KYC/contact info
│   ├── KYC_Individual/
│   │   └── Individual_Generalinfo.razor # Individual KYC form synced to BC ContactCard
│   ├── KYC CORPORATE/
│   │   └── Corporate_Generalinfo.razor  # Corporate KYC mapped to BC corporate contact
│   └── Receipts/
│       └── MyReceipts.razor             # Posted receipts, PDF export
├── Interfaces/
│   ├── IUnitOfWork.cs                   # Aggregates BC + domain services
│   ├── IInsurerQuoteService.cs          # ProductSelection + acknowledgements
│   ├── IClaimsService.cs                # Loss types + Claimscsrdpage
│   ├── IDebitNoteService.cs             # Debit notes + payments
│   └── IContactCardService.cs           # ContactCard (KYC) integration
├── Repositories/Services/
│   ├── ClaimsService.cs                 # Calls Claimscsrdpage in BC
│   ├── DebitNoteService.cs              # DebitNote and DebitNotePayments endpoints
│   ├── PolicyCardService.cs             # PolicyCard lookups by Quotation_No
│   └── PostedReceiptService.cs          # Posted receipts by Insured_Number
├── Utils/
│   ├── BusinessCentralConfig.cs         # OData base URL, auth, HttpClient
│   ├── CustomAuthenticationStateProvider.cs
│   └── EmailService.cs                  # 2FA, verification, quote & payment emails
└── Program.cs                           # Application entry point
```

## 🚀 Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022 / VS Code
- Access to the **ERP** environment (OData + credentials)
- Valid SMTP server credentials for email notifications

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/OBASAKILLI/MOZ_UPGRADE.git
   cd MOZ_UPGRADE
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure ERP integration and SMTP**

   In `appsettings.json` (or environment‑specific config), set:

   ```json
   {
     "BcApi": {
       "BaseUrl": "http://196.201.224.102:2048/BC260/ODataV4/",
       "User": "BC_USERNAME",
       "Password": "BC_PASSWORD"
     },
     "Smtp": {
       "Host": "smtp.yourdomain.com",
       "Port": 587,
       "User": "smtp-user@yourdomain.com",
       "Pass": "smtp-password",
       "From": "no-reply@yourdomain.com",
       "FromName": "Standard Insurance Portal",
       "EnableSsl": true
     }
   }
   ```

   The **BusinessCentralConfig** helper composes URLs like:

   - `Company('STANDARD%20INSURANCE')/ProductSelection`
   - `Company('STANDARD%20INSURANCE')/PolicyCard`
   - `Company('STANDARD%20INSURANCE')/Claimscsrdpage`
   - `ContactCard` (via `BuildUrl("ContactCard")`)

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Open your browser and navigate to `https://localhost:7188`

## 📋 Features in Detail

### Individual KYC Form (`/Individual_Generalinfo`)

Comprehensive individual customer onboarding persisted to BC `ContactCard`, including:

#### Personal Information
- Tax Identification Number (NUIT)
- Title, First Name, Middle Name, Last Name
- Full Legal Name
- Date of Birth (18+ validation)
- Gender, Nationality
- Marital Status, Religion
- Country of Residence

#### Identification Document
- ID Document Type (Passport, National ID, Activity License, Driver License)
- Document Number
- Expiry Date

#### Contact Information
- Phone Number
- Email Address

#### Residential Address
- Street Address
- City, Postal Code, Country
- Occupation
- Source of Income

#### Risk Assessment & Compliance
- Politically Exposed Person (PEP) Status
- AML/CFT Compliance Declaration

### Corporate KYC Flow

Multi-step corporate onboarding with integrated timeline and mapping to BC corporate contact fields:

1. **General Info** - Company details and basic information
2. **Shareholders** - Shareholder information and ownership structure
3. **PEP** - Politically Exposed Persons assessment
4. **Documents** - Document upload and verification
5. **Summary** - Review and final submission

### Timeline Component

Reusable `KYCTimeline.razor` component featuring:
- Horizontal timeline visualization
- Dynamic step tracking
- Color-coded status (completed/pending)
- Responsive design
- Easy integration across pages

## 🎨 UI/UX Design

### Design Principles

- **Modern & Professional** – Clean, contemporary interface
- **Intuitive Navigation** – Clear user flow and visual hierarchy
- **Responsive** – Optimized for desktop, tablet, and mobile
- **Accessible** – Uses MudBlazor and semantic HTML
- **Consistent** – Unified design language throughout

### Color Scheme

- **Primary**: #667eea (Purple)
- **Success**: #4caf50 (Green)
- **Warning**: #ff9800 (Orange)
- **Background**: #ffffff (White)

## 🔐 Security & Compliance

- **Authentication** – Custom authentication state provider integrated with ASP.NET Identity models.
- **Authorization** – Portal pages are protected by `[Authorize]`.
- **Validation** – Client and server‑side validation on KYC, claims, and payments.
- **BC Constraints** – Length checks (e.g. `Contact_No`, `Policy_No`) respect BC field limits.
- **Regulatory** – Designed for Mozambique AML/CFT requirements (Law No. 14/2023).

## 📱 Responsive Breakpoints

| Device | Width | Optimization |
|--------|-------|--------------|
| Mobile | < 600px | Single column, touch-friendly |
| Tablet | 600px - 960px | Two columns |
| Desktop | > 960px | Three columns, full layout |

## 🧪 Testing

### Running Tests

```bash
dotnet test
```

### Test Coverage

- Unit tests for business logic
- Integration tests for API endpoints
- UI component tests

## 🔄 Deployment

1. **Build for production**

   ```bash
   dotnet publish -c Release
   ```

2. **Host on IIS / Azure / container**

   - Deploy the published output.
   - Ensure `BcApi` and `Smtp` settings are provided via `appsettings.Production.json` or environment variables.
   - Configure HTTPS and reverse proxy as needed.

## 📊 Data & Integration Notes

- The portal primarily relies on **ERP** entities via OData (ContactCard, ProductSelection, PolicyCard, Claimscsrdpage, DebitNote, PostedReceipts, etc.).
- Local persistence is minimal and mainly for Identity / user records.

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

For support, email support@mozupgrade.com or open an issue on GitHub.

## 🙏 Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/)
- UI Components from [MudBlazor](https://www.mudblazor.com/)
- Compliant with [Mozambique Law No. 14/2023](https://www.example.com)

## 📈 Roadmap

- [ ] Multi-language support (Portuguese, English, Swahili)
- [ ] Advanced document verification with OCR
- [ ] Biometric integration
- [ ] Real-time compliance monitoring
- [ ] Enhanced reporting and analytics
- [ ] Mobile app (iOS/Android)
- [ ] API rate limiting and throttling
- [ ] Advanced audit logging

## 🔗 Links

- [GitHub Repository](https://github.com/OBASAKILLI/MOZ_UPGRADE_New)

---

**Last Updated**: November 20, 2025  
**Maintainer**: OBASAKILLI

