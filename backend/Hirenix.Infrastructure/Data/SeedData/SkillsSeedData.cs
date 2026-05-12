namespace Hirenix.Infrastructure.Data.SeedData;

public static class SkillsSeedData
{
    public static List<(string Name, string Slug, string Category)> GetSkills()
    {
        return new List<(string, string, string)>
        {
            // ═══════════════════════════════════════════════════════════════
            // Programming Languages & Frameworks (40 skills)
            // ═══════════════════════════════════════════════════════════════
            ("C#", "csharp", "Programming"),
            ("Java", "java", "Programming"),
            ("Python", "python", "Programming"),
            ("JavaScript", "javascript", "Programming"),
            ("TypeScript", "typescript", "Programming"),
            ("PHP", "php", "Programming"),
            ("Ruby", "ruby", "Programming"),
            ("Go", "go", "Programming"),
            ("Rust", "rust", "Programming"),
            ("Swift", "swift", "Programming"),
            ("Kotlin", "kotlin", "Programming"),
            ("C++", "cpp", "Programming"),
            ("C", "c", "Programming"),
            ("Scala", "scala", "Programming"),
            ("R", "r", "Programming"),
            
            // Frontend Frameworks
            ("React", "react", "Programming"),
            ("Angular", "angular", "Programming"),
            ("Vue.js", "vuejs", "Programming"),
            ("Next.js", "nextjs", "Programming"),
            ("Nuxt.js", "nuxtjs", "Programming"),
            ("Svelte", "svelte", "Programming"),
            
            // Backend Frameworks
            (".NET", "dotnet", "Programming"),
            ("ASP.NET Core", "aspnet-core", "Programming"),
            ("Node.js", "nodejs", "Programming"),
            ("Express.js", "expressjs", "Programming"),
            ("Django", "django", "Programming"),
            ("Flask", "flask", "Programming"),
            ("FastAPI", "fastapi", "Programming"),
            ("Spring Boot", "spring-boot", "Programming"),
            ("Laravel", "laravel", "Programming"),
            ("Ruby on Rails", "ruby-on-rails", "Programming"),
            
            // Mobile Development
            ("React Native", "react-native", "Programming"),
            ("Flutter", "flutter", "Programming"),
            ("Xamarin", "xamarin", "Programming"),
            ("Ionic", "ionic", "Programming"),
            ("SwiftUI", "swiftui", "Programming"),
            ("Android Development", "android-development", "Programming"),
            ("iOS Development", "ios-development", "Programming"),
            
            // Other Programming
            ("HTML", "html", "Programming"),
            ("CSS", "css", "Programming"),
            
            // ═══════════════════════════════════════════════════════════════
            // Database & Data (20 skills)
            // ═══════════════════════════════════════════════════════════════
            ("MySQL", "mysql", "Database"),
            ("PostgreSQL", "postgresql", "Database"),
            ("MongoDB", "mongodb", "Database"),
            ("Redis", "redis", "Database"),
            ("SQL Server", "sql-server", "Database"),
            ("Oracle Database", "oracle-database", "Database"),
            ("SQLite", "sqlite", "Database"),
            ("Cassandra", "cassandra", "Database"),
            ("DynamoDB", "dynamodb", "Database"),
            ("Firebase", "firebase", "Database"),
            ("Elasticsearch", "elasticsearch", "Database"),
            ("Neo4j", "neo4j", "Database"),
            ("MariaDB", "mariadb", "Database"),
            
            // Data Engineering
            ("Apache Kafka", "apache-kafka", "Database"),
            ("Apache Spark", "apache-spark", "Database"),
            ("Hadoop", "hadoop", "Database"),
            ("ETL", "etl", "Database"),
            ("Data Warehousing", "data-warehousing", "Database"),
            ("Data Modeling", "data-modeling", "Database"),
            ("SQL", "sql", "Database"),
            
            // ═══════════════════════════════════════════════════════════════
            // DevOps & Cloud (20 skills)
            // ═══════════════════════════════════════════════════════════════
            ("Docker", "docker", "DevOps"),
            ("Kubernetes", "kubernetes", "DevOps"),
            ("Jenkins", "jenkins", "DevOps"),
            ("GitLab CI/CD", "gitlab-cicd", "DevOps"),
            ("GitHub Actions", "github-actions", "DevOps"),
            ("Terraform", "terraform", "DevOps"),
            ("Ansible", "ansible", "DevOps"),
            ("AWS", "aws", "DevOps"),
            ("Azure", "azure", "DevOps"),
            ("Google Cloud Platform", "gcp", "DevOps"),
            ("Linux", "linux", "DevOps"),
            ("Nginx", "nginx", "DevOps"),
            ("Apache", "apache", "DevOps"),
            ("Microservices", "microservices", "DevOps"),
            ("CI/CD", "cicd", "DevOps"),
            ("Monitoring", "monitoring", "DevOps"),
            ("Logging", "logging", "DevOps"),
            ("Git", "git", "DevOps"),
            ("Shell Scripting", "shell-scripting", "DevOps"),
            ("Infrastructure as Code", "infrastructure-as-code", "DevOps"),
            
            // ═══════════════════════════════════════════════════════════════
            // Design & UI/UX (15 skills)
            // ═══════════════════════════════════════════════════════════════
            ("Figma", "figma", "Design"),
            ("Adobe XD", "adobe-xd", "Design"),
            ("Sketch", "sketch", "Design"),
            ("Photoshop", "photoshop", "Design"),
            ("Illustrator", "illustrator", "Design"),
            ("InDesign", "indesign", "Design"),
            ("UI Design", "ui-design", "Design"),
            ("UX Design", "ux-design", "Design"),
            ("Wireframing", "wireframing", "Design"),
            ("Prototyping", "prototyping", "Design"),
            ("User Research", "user-research", "Design"),
            ("Design Systems", "design-systems", "Design"),
            ("Responsive Design", "responsive-design", "Design"),
            ("Motion Design", "motion-design", "Design"),
            ("Graphic Design", "graphic-design", "Design"),
            
            // ═══════════════════════════════════════════════════════════════
            // Marketing & Sales (15 skills)
            // ═══════════════════════════════════════════════════════════════
            ("SEO", "seo", "Marketing"),
            ("SEM", "sem", "Marketing"),
            ("Content Marketing", "content-marketing", "Marketing"),
            ("Social Media Marketing", "social-media-marketing", "Marketing"),
            ("Email Marketing", "email-marketing", "Marketing"),
            ("Google Analytics", "google-analytics", "Marketing"),
            ("Facebook Ads", "facebook-ads", "Marketing"),
            ("Google Ads", "google-ads", "Marketing"),
            ("Marketing Automation", "marketing-automation", "Marketing"),
            ("Copywriting", "copywriting", "Marketing"),
            ("Brand Management", "brand-management", "Marketing"),
            ("Market Research", "market-research", "Marketing"),
            ("Sales", "sales", "Marketing"),
            ("Customer Relationship Management", "crm", "Marketing"),
            ("Lead Generation", "lead-generation", "Marketing"),
            
            // ═══════════════════════════════════════════════════════════════
            // Business & Management (15 skills)
            // ═══════════════════════════════════════════════════════════════
            ("Project Management", "project-management", "Business"),
            ("Agile", "agile", "Business"),
            ("Scrum", "scrum", "Business"),
            ("Kanban", "kanban", "Business"),
            ("Business Analysis", "business-analysis", "Business"),
            ("Product Management", "product-management", "Business"),
            ("Strategic Planning", "strategic-planning", "Business"),
            ("Financial Analysis", "financial-analysis", "Business"),
            ("Budgeting", "budgeting", "Business"),
            ("Risk Management", "risk-management", "Business"),
            ("Change Management", "change-management", "Business"),
            ("Stakeholder Management", "stakeholder-management", "Business"),
            ("Business Development", "business-development", "Business"),
            ("Operations Management", "operations-management", "Business"),
            ("Supply Chain Management", "supply-chain-management", "Business"),
            
            // ═══════════════════════════════════════════════════════════════
            // Soft Skills (20 skills)
            // ═══════════════════════════════════════════════════════════════
            ("Communication", "communication", "Soft Skills"),
            ("Leadership", "leadership", "Soft Skills"),
            ("Teamwork", "teamwork", "Soft Skills"),
            ("Problem Solving", "problem-solving", "Soft Skills"),
            ("Critical Thinking", "critical-thinking", "Soft Skills"),
            ("Time Management", "time-management", "Soft Skills"),
            ("Adaptability", "adaptability", "Soft Skills"),
            ("Creativity", "creativity", "Soft Skills"),
            ("Attention to Detail", "attention-to-detail", "Soft Skills"),
            ("Decision Making", "decision-making", "Soft Skills"),
            ("Conflict Resolution", "conflict-resolution", "Soft Skills"),
            ("Negotiation", "negotiation", "Soft Skills"),
            ("Presentation Skills", "presentation-skills", "Soft Skills"),
            ("Public Speaking", "public-speaking", "Soft Skills"),
            ("Emotional Intelligence", "emotional-intelligence", "Soft Skills"),
            ("Work Ethic", "work-ethic", "Soft Skills"),
            ("Self-Motivation", "self-motivation", "Soft Skills"),
            ("Collaboration", "collaboration", "Soft Skills"),
            ("Customer Service", "customer-service", "Soft Skills"),
            ("Mentoring", "mentoring", "Soft Skills"),
            
            // ═══════════════════════════════════════════════════════════════
            // Languages (15 skills)
            // ═══════════════════════════════════════════════════════════════
            ("English", "english", "Languages"),
            ("Vietnamese", "vietnamese", "Languages"),
            ("Chinese", "chinese", "Languages"),
            ("Japanese", "japanese", "Languages"),
            ("Korean", "korean", "Languages"),
            ("French", "french", "Languages"),
            ("German", "german", "Languages"),
            ("Spanish", "spanish", "Languages"),
            ("Thai", "thai", "Languages"),
            ("Indonesian", "indonesian", "Languages"),
            ("Malay", "malay", "Languages"),
            ("Russian", "russian", "Languages"),
            ("Arabic", "arabic", "Languages"),
            ("Portuguese", "portuguese", "Languages"),
            ("Italian", "italian", "Languages"),
            
            // ═══════════════════════════════════════════════════════════════
            // Data Science & AI (10 skills)
            // ═══════════════════════════════════════════════════════════════
            ("Machine Learning", "machine-learning", "Data Science"),
            ("Deep Learning", "deep-learning", "Data Science"),
            ("Artificial Intelligence", "artificial-intelligence", "Data Science"),
            ("Data Analysis", "data-analysis", "Data Science"),
            ("Data Visualization", "data-visualization", "Data Science"),
            ("TensorFlow", "tensorflow", "Data Science"),
            ("PyTorch", "pytorch", "Data Science"),
            ("Natural Language Processing", "nlp", "Data Science"),
            ("Computer Vision", "computer-vision", "Data Science"),
            ("Statistical Analysis", "statistical-analysis", "Data Science"),
            
            // ═══════════════════════════════════════════════════════════════
            // Other Tools & Technologies (10 skills)
            // ═══════════════════════════════════════════════════════════════
            ("Microsoft Office", "microsoft-office", "Other"),
            ("Google Workspace", "google-workspace", "Other"),
            ("Jira", "jira", "Other"),
            ("Confluence", "confluence", "Other"),
            ("Slack", "slack", "Other"),
            ("Trello", "trello", "Other"),
            ("Notion", "notion", "Other"),
            ("Excel", "excel", "Other"),
            ("PowerPoint", "powerpoint", "Other"),
            ("Word", "word", "Other"),
        };
    }
}
