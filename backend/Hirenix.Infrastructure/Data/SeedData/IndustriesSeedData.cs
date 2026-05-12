namespace Hirenix.Infrastructure.Data.SeedData;

public static class IndustriesSeedData
{
    public static List<(string Name, string Slug)> GetIndustries()
    {
        return new List<(string, string)>
        {
            ("Information Technology", "information-technology"),
            ("Software Development", "software-development"),
            ("Finance & Banking", "finance-banking"),
            ("Healthcare & Medical", "healthcare-medical"),
            ("Education & Training", "education-training"),
            ("E-commerce & Retail", "ecommerce-retail"),
            ("Manufacturing", "manufacturing"),
            ("Real Estate", "real-estate"),
            ("Hospitality & Tourism", "hospitality-tourism"),
            ("Media & Entertainment", "media-entertainment"),
            ("Telecommunications", "telecommunications"),
            ("Transportation & Logistics", "transportation-logistics"),
            ("Construction & Engineering", "construction-engineering"),
            ("Agriculture", "agriculture"),
            ("Energy & Utilities", "energy-utilities"),
            ("Consulting", "consulting"),
            ("Marketing & Advertising", "marketing-advertising"),
            ("Human Resources", "human-resources"),
            ("Legal Services", "legal-services"),
            ("Accounting & Auditing", "accounting-auditing"),
            ("Insurance", "insurance"),
            ("Automotive", "automotive"),
            ("Food & Beverage", "food-beverage"),
            ("Fashion & Apparel", "fashion-apparel"),
            ("Non-profit & NGO", "non-profit-ngo"),
        };
    }
}
