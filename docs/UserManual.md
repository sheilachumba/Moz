# MOZ Upgrade Portal
## End‑User Manual (Policyholders)

---

## 1. Introduction

The **MOZ Upgrade Portal** is a self‑service web platform for insurance customers (policyholders). It allows you to:

- View and manage your **insurance policies**
- Complete **Know Your Customer (KYC)** requirements
- Track and pay **debit notes / premiums**
- Access **receipts**
- Monitor **claims**
- Review **quotes** and convert them to policies
- Analyze your portfolio using **Reports & Analytics**

This manual explains the full flow from **KYC onboarding** to **using all main features** of the portal.

---

## 2. Getting Started

### 2.1. Accessing the Portal

1. Open your preferred web browser.
2. Navigate to the portal URL provided by your insurer (e.g. `https://yourcompany-portal.com`).
3. Enter your **username/email** and **password**.
4. Click **Login**.

If multi‑factor authentication (OTP/2FA) is enabled, follow the on‑screen instructions.

### 2.2. Main Layout Overview

After logging in, the portal layout typically contains:

- **Top Bar**
  - Portal / company logo
  - (Optional) Language selector
  - User profile menu (Profile, Logout)

- **Left Navigation Menu**
  - **Dashboard**
  - **KYC – Individual** / **KYC – Corporate**
  - **My Policies**
  - **My Payments**
  - **Claims**
  - **Quotes**
  - **Reports**

- **Content Area**
  - Displays the currently selected page (Dashboard, KYC forms, etc.).

Use the left navigation to move between sections; the content area updates accordingly.

---

## 3. KYC & Onboarding Flow

KYC (Know Your Customer) is a mandatory process used to verify your identity and comply with regulations. Until KYC is completed and approved, some actions (e.g. full policy access, payments, claims) may be limited.

### 3.1. Overall KYC Steps

1. **Login** to the portal.
2. Open the appropriate **KYC section**:
   - _KYC Individual_ – if you are registering as a person.
   - _KYC Corporate_ – if you represent a company/organization.
3. Complete all **mandatory fields**.
4. Upload **supporting documents** if required.
5. **Submit** the KYC form.
6. Wait for **verification/approval** by the insurer.
7. Once approved, proceed to use all other features (Dashboard, Policies, Payments, Claims, Quotes, Reports).


---

## 4. KYC for Individual Customers

### 4.1. Opening Individual KYC

1. From the left menu, select **KYC → Individual** (or **KYC Individual**).
2. The **Individual General Info** page appears.

This page is typically divided into sections such as:

- Personal details
- Identification
- Contact information
- Employment & income
- Declarations & consent

### 4.2. Personal Details

Fill in your personal information exactly as it appears on your official documents:

- Full name
- Date of birth
- Gender
- Nationality
- Marital status

> **Tip:** Use the correct date format (e.g. `dd/MM/yyyy`).

### 4.3. Identification Details

Provide your identification details:

- Type of ID (National ID, Passport, Driver’s License, etc.)
- ID number
- Issue date
- Expiry date
- Issuing country/authority

Ensure that:

- Numbers and dates match your physical ID.
- The document is valid (not expired).

### 4.4. Contact & Address Information

Enter your contact details:

- Mobile phone number
- Email address
- Residential address (street, city, postal/ZIP code, country)

These details are used for:

- Login and security notifications
- Policy and claims communication
- Regulatory contact requirements

> **Important:** Keep your phone number and email address up to date.

### 4.5. Employment & Source of Income

For regulatory and risk assessment purposes, provide:

- Employment status (employed, self‑employed, student, retired, etc.)
- Employer name (if applicable)
- Main source of income (salary, business income, investments, etc.)

### 4.6. Declarations & Consent

At the bottom of the KYC form you may find:

- Legal declarations and confirmations
- Anti‑money‑laundering / sanction screening questions
- Checkboxes to confirm accuracy and consent to terms & privacy policy

Read carefully and tick the appropriate checkboxes, then click **Save** or **Submit**.

### 4.7. Individual KYC Status

After submission, your KYC status may show as:

- **Pending / Under Review** – KYC is submitted and awaiting review.
- **Approved** – KYC is verified; you can use full functionality.
- **Rejected / Incomplete** – Additional information or corrections are required.

If your KYC is rejected or marked incomplete, return to the **KYC Individual** page, correct the highlighted fields, and resubmit.


---

## 5. KYC for Corporate Customers

### 5.1. Opening Corporate KYC

1. From the left menu, select **KYC → Corporate** (or **KYC Corporate**).
2. The **Corporate General Info** page appears.

This page focuses on the legal entity and its controllers.

### 5.2. Company Identity

Provide information such as:

- Company legal name
- Registration/incorporation number
- Tax identification number
- Country of incorporation
- Registered office and business address
- General contact information (phone, email)

### 5.3. Ownership & Control

You may need to provide details for:

- **Directors / Legal Representatives**
  - Full names, roles, ID details
- **Beneficial Owners**
  - Individuals who ultimately own/control the company
  - Ownership percentages or other control indicators

There may be specific fields for:

- Legal representative ID details
- Beneficial owner ID details

All such information must match official company documents.

### 5.4. Business Profile & Compliance

Provide:

- Nature of business / industry
- Estimated annual turnover
- Source of funds

Answer compliance questions (e.g. PEP status, sanctions, high‑risk activities) truthfully.

### 5.5. Supporting Documents

Depending on regulatory rules, you may be asked to upload:

- Certificate of incorporation / registration
- Memorandum & articles
- Board resolutions
- IDs of directors, beneficial owners, and representatives

Ensure files are:

- Clear scans or photos
- In acceptable formats (PDF, JPG, PNG, etc.)
- Within any specified size limits

### 5.6. Corporate KYC Status

After submitting, monitor your KYC Corporate status:

- **Pending** – under compliance review
- **Approved** – your company can fully use the portal
- **Rejected / Needs More Information** – revisit the form, update data or documents, and resubmit


---

## 6. User Verification & Account Linking

The system links your online account to your insurance records using two key pieces of information (managed internally by the system):

- **User ID** (login identity)
- **Contact Number** and resulting **Insured Number** from back‑office systems

Behind the scenes:

1. When you log in, the system obtains your user ID.
2. It retrieves your user profile and **Contact_No**.
3. It uses **Contact_No** to fetch your **Contact Card** from the insurer’s core system.
4. From the Contact Card it gets your **Insured_Number**.
5. That Insured Number is then used to fetch policies, debit notes, receipts, claims, and quotes specifically linked to you.

> **Impact:** If contact information in KYC is wrong or not aligned with the insurer’s records, your policies and payments may not appear correctly in the portal.


---

## 7. Dashboard

### 7.1. Accessing the Dashboard

After login, or by clicking **Dashboard** in the left menu, you reach the main overview page.

### 7.2. Dashboard Content

The Dashboard may show:

- Overview of **KYC status** (e.g. Incomplete, Pending, Approved)
- Summary of **policies** (count by status such as Active / Pending / Expired)
- Summary of **payments** (total premium due/paid, recent debit notes)
- Summary of **claims** (open vs closed, total claimed amounts)
- Quick links to key actions:
  - Complete/update KYC
  - View My Policies
  - Go to My Payments
  - Submit/View Claims
  - Open Reports

The Dashboard gives a high‑level snapshot of your overall relationship with the insurer.


---

## 8. My Policies

### 8.1. Opening My Policies

1. Click **My Policies** in the left navigation.
2. The system fetches your policies using your **Insured Number**.

### 8.2. Policy List

The page usually displays a table with columns like:

- Policy Number
- Policy Type / Class
- Status (Active, Pending, Expired, Cancelled)
- Start Date
- End Date
- Premium
- Sum Insured

You can scroll or page through all policies linked to your account.

### 8.3. Viewing Policy Details

Selecting or opening a policy (if supported) may show additional details:

- Risk description
- Coverages and exclusions
- Beneficiaries (if applicable)
- Policy documents (e.g. schedule, wording) for download

Use **My Policies** to confirm coverage and check renewal or expiry dates.


---

## 9. My Payments (Debit Notes)

### 9.1. Opening My Payments

1. Click **My Payments** (or **MyPyments**) in the left menu.
2. The system locates your **Insured Number** and fetches related **debit notes** from the insurer’s core system.

### 9.2. Summary Indicators

At the top of the page you may see:

- **Total Notes** – total count of debit notes found
- **Total Premium** – sum of all premiums due
- **Open Notes** – count of notes with status like "Open" or "Pending"

These indicators reflect your overall premium position.

### 9.3. Filtering and Searching

My Payments typically offers:

- **Status tabs** – for example:
  - Open
  - Pending
  - Paid (Posted)
- **Search box** – to locate specific notes by:
  - Policy number
  - Document number (No)
  - Policy description
  - Insured name or number

Use these controls to focus on items you care about, such as only unpaid items.

### 9.4. Debit Note Table

Each row (debit note) commonly includes:

- Policy description
- Insured name
- Coverage period (From Date – To Date)
- Total Sum Insured
- Total Premium Amount
- Payment Status (with color chips indicating Open/Pending/Posted)
- Action button (e.g. **Make Payment**)

### 9.5. Making a Payment

To pay a debit note:

1. Click the **Make Payment** button in the row of the relevant debit note.
2. A payment dialog appears asking for:
   - **Amount** to pay
   - **Mode of Payment** (e.g. Cash, Bank Transfer, Mobile Money, Card)
   - **Payment Reference Number**
   - **Payment Date**
3. Fill in all fields and click **Confirm / Submit**.
4. If successful, you will normally see:
   - A success message
   - A change of status (e.g. from Open to Posted) after the system updates

If the payment fails, an error message will be displayed. Check the details and try again or contact support.


---

## 10. Receipts

### 10.1. Accessing Receipts

Receipts may be visible either:

- Within **My Payments**, or
- As a separate **Receipts** section, or
- Through the **Reports → Receipts** tab.

### 10.2. Receipt Information

Receipt data can include:

- Receipt Number
- Policy Number or "On behalf of" reference
- Posting/Issue Date
- Amount
- Payment Method
- Status (e.g. Posted, Pending)

Use this section to:

- Confirm that a payment was successfully posted
- Keep printouts or exports for your own accounting records


---

## 11. Claims

### 11.1. Viewing Claims

1. Click **Claims** in the left menu (or open the **Claims** tab under Reports).
2. The system fetches claims tied to your **Insured Number**.

### 11.2. Claim List

Typical columns include:

- Claim Number
- Policy Number
- Date Reported
- Claim Type or Loss Type
- Status (Open, In Progress, Pending, Approved, Paid, Closed, Rejected)
- Amount Claimed
- Amount Paid

### 11.3. Understanding Claim Statuses

- **Open / In Progress** – claim is being handled.
- **Pending** – awaiting additional information or approval.
- **Approved** – accepted by the insurer.
- **Paid** – payment has been made (fully or partially).
- **Closed** – finalized; no further action expected.
- **Rejected** – not accepted.

### 11.4. Submitting or Updating a Claim

If online submission is enabled, you may:

- Start a new claim by clicking **New Claim** (or similar) and filling details.
- Upload supporting documents (photos, invoices, reports).
- Add additional comments or updates while the claim is in progress.

Use the Claims area to monitor the progress of your claims and track any payments made.


---

## 12. Quotes

### 12.1. Viewing Quotes

1. Click **Quotes** in the left menu or open the **Quotes** tab under Reports.
2. Quotes linked to your Insured Number are listed.

### 12.2. Quote Details

Common fields:

- Quote Number
- Policy Type
- Date Created
- Expiry Date
- Status (Draft, Pending, Converted, Expired)
- Premium
- Sum Insured

### 12.3. Quote Lifecycle

- **Draft** – initial quote not yet final.
- **Pending** – awaiting underwriting or further info.
- **Converted** – converted into an actual policy.
- **Expired** – no longer valid; a new quote must be requested.

You can use this section to compare or review historical quotes and follow up on those marked Pending or Expiring soon.


---

## 13. Reports & Analytics

### 13.1. Opening Reports

1. Click **Reports** in the left menu.
2. You will see a **Reports & Analytics** page with multiple tabs, such as:
   - Policies
   - Payments
   - Receipts
   - Claims
   - Quotes

### 13.2. Filters

At the top of the Reports page there may be filters such as:

- **Date range** (From Date, To Date)
- **Status** (Active, Pending, Expired, etc., depending on tab)

Use filters to refine which records are included in the tables and statistics.

### 13.3. Policies Tab

Shows a structured list of your policies with key metrics. Useful for:

- Exporting all current policies at once
- Analyzing total premium and sum insured

### 13.4. Payments Tab

Displays payment‑related information (based on debit notes or payment records), including:

- Receipt/Document Number
- Policy Number
- Payment Date
- Amount
- Payment Method
- Payment Status

This helps you:

- Review your historical premium payments
- Identify high‑payment months or patterns

### 13.5. Receipts Tab

Summarizes the receipts that have been posted:

- Receipt Number
- Issue Date
- Amount
- Method
- Status

Useful for reconciliation and proof of payment.

### 13.6. Claims Tab

Provides an overview of claims:

- Total count
- Status distribution (Open, Closed, etc.)
- Total amounts claimed and paid

Enables you to understand your loss experience over time.

### 13.7. Quotes Tab

Provides an overview of quotes:

- Counts and statuses (Draft, Pending, Converted, Expired)
- Premiums and sums insured quoted

Good for reviewing outstanding quotes and following up with your insurer.

### 13.8. Exporting to PDF or Excel

On each tab you will usually find:

- **Export to PDF** button
- **Export to Excel** button

Click the button to download the current view as a file. Save it locally and open it with:

- A PDF reader (for PDF)
- Microsoft Excel, LibreOffice, or similar (for Excel files)


---

## 14. Notifications & Communication

The portal may send notifications for:

- KYC submissions and approvals
- New or updated policies
- New debit notes or payment due reminders
- Claim status changes
- Quote creation and expiry

Ensure your email and mobile phone in KYC are correct so you receive these notifications.


---

## 15. Troubleshooting & FAQs

### 15.1. I Cant Log In

- Verify your username/email and password.
- If available, use **Forgot Password** to reset.
- If you still cannot log in, contact support and provide your user identifier.

### 15.2. My KYC is Stuck on "Pending"

- KYC review may take some time depending on the insurers compliance procedures.
- If it remains pending for an unusually long period, contact support with your full name and contact number.

### 15.3. My Policies or Payments Do Not Appear

- Confirm that the **Contact Number** in your profile/KYC matches what the insurer has in their records.
- Ensure KYC is **Approved**.
- If the issue persists, share your policy number with support so they can check linkages.

### 15.4. My Payments Page Shows No Data but I Expect Debit Notes

- Ensure KYC is complete and that you have active policies.
- Check that you are using the correct account/profile.
- If `/My Payments` remains empty while you have been billed, contact support with your policy number or debit note reference.

### 15.5. I See an Error When Making a Payment or Submitting a Claim

- Carefully read the error message; it often indicates missing or invalid data.
- Correct the indicated fields and try again.
- If the problem continues, take a screenshot and contact support.


---

## 16. Best Practices for Using the Portal

- Complete and keep your **KYC information** up to date.
- Check the **Dashboard** regularly for new debit notes, claims, or expiring policies/quotes.
- Use **Reports & Analytics** to download and save your portfolio data.
- Always log out, especially when using shared or public devices.
- Contact your insurer promptly if any data looks incorrect or if you suspect unauthorized access.


---

_End of User Manual_
