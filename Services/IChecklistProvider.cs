using ClientPortal.Models;

namespace ClientPortal.Services;

public interface IChecklistProvider
{
    IEnumerable<ChecklistRequirement> Get(KycType type);
}

// You can later replace with an Excel-based provider.
public class InMemoryChecklistProvider : IChecklistProvider
{
    private static readonly ChecklistRequirement[] IndividualReqs = new[]
{
   
    new ChecklistRequirement("id_proof", "Proof of Identity – Original or scanned certified copy (National ID, Passport, Personal Certificate, Full Birth Certificate, Voter Card, Driving License, Military ID, Political Exile Card, Work ID, Foreigner Residence ID – DIRE, Refugee ID, etc.)", true),
    new ChecklistRequirement("addr_proof", @"Proof of current address – Original or certified copy: 
- Utility bills (water, landline, cable TV, electricity, internet) not older than 2 months, OR
- Municipal authority declaration (issued within 2 months), OR
- Employer letter (on letterhead, signed by senior agent), OR
- For married persons where documents are in spouse’s name: proof, plus certified marriage certificate", true),
    new ChecklistRequirement("source_funds", "Source of funds and income proof: Employer letter (employment, profession, contract type, net salary, dated within 2 months) OR self-declaration of income (if not formally employed)", true),
    new ChecklistRequirement("nuit", "NUIT / Tax Identification Card or Assignment Notice of the Unique Tax Number as issued by the competent authority", true),
    new ChecklistRequirement("passport_photo", "Recent passport photo (optional)", false)
};


    private static readonly ChecklistRequirement[] CompanyReqs = new[]
{
    
    new ChecklistRequirement("inc_cert", @"Certificate of Commercial Registration issued by a supervisory body 
(Original document, or original scanned notarized; with less than 3 months of issue.)", true),
    new ChecklistRequirement("activity_license", @"Activity License (Original or Notary-certified photocopy)", true),
    new ChecklistRequirement("nuit", @"NUIT of the Entity – Communication of Attribution of the Unique Tax Identification Number, 
issued by the competent body", true),
    new ChecklistRequirement("statutes", @"Statutes published in the Bulletin of the Republic and their amendments, where applicable", true),
    new ChecklistRequirement("shareholder_structure", @"Identification of the Partners/Shareholders/Beneficial Owners:
- Minutes of appointment/proxy of legal person’s representatives
- Identification of direct partners and shareholders who hold control, OR 
- Holders of non-registered shares with a value equal to or greater than 10%", true),
    new ChecklistRequirement("rep_proof_id", @"Proof of identity of the representative and beneficial owner:
- National Identity Card (B.I), Passport, B.I Application Slip with Personal Record or Full Certificate of Birth Registration,
   Electoral Refusal Card, Driving License, Military Card, Political Exile Card, Work ID. 
- Foreign Resident: DIRE, Refugee ID Card, Political Exile Card.
- Foreign Non-Resident: Passport.
- For beneficial owners: NUIT and Proof of address.", true),
    new ChecklistRequirement("transaction_volume", @"Transaction Volume evidence:
- Model 02 (Statement of Registration or Business Changes),
- Financial Statements,
- Statements of account domiciled in OIC of last 2 years,
- Finance Model 22,
- Account report,
- Letter by entity indicating origin and expected monthly value,
- Or other relevant document.", true),
    new ChecklistRequirement("translation", @"All documents in a foreign language must be translated into Portuguese by a sworn translator.", true)
};


    public IEnumerable<ChecklistRequirement> Get(KycType type)
    {
        switch (type)
        {
            case KycType.Individual:
            case KycType.SoleProprietor:
                return IndividualReqs;

            case KycType.Company:
            case KycType.Association:
                return CompanyReqs;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), "Unknown KYC type");
        }
    }

}
