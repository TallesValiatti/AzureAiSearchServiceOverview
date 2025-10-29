using AzureAiSearchServiceOverview.Console.Models;

namespace AzureAiSearchServiceOverview.Console.Seed;

public static class JobData
{
    public static List<Job> GetSampleJobs() =>
    [
        new Job
        {
            Id = "1",
            Name = "Backend .NET Engineer",
            Salary = 120_000,
            Description =
                "Build and scale APIs with ASP.NET Core, Azure Functions, and SQL. Work on high throughput services."
        },

        new Job
        {
            Id = "2",
            Name = "Data Engineer",
            Salary = 110_000,
            Description =
                "Design data pipelines with Spark, Databricks, and Azure Data Factory. Optimize lakehouse architectures."
        },

        new Job
        {
            Id = "3",
            Name = "AI Engineer",
            Salary = 140_000,
            Description =
                "Productionize RAG and multi-agent solutions with Azure AI Foundry, vector search, and prompt engineering."
        },

        new Job
        {
            Id = "4",
            Name = "SRE / DevOps",
            Salary = 115_000,
            Description =
                "Automate infra with Bicep/Terraform, GitHub Actions, Kubernetes, and observability for 99.9% availability."
        },

        new Job
        {
            Id = "5",
            Name = "Full-Stack Developer",
            Salary = 105_000,
            Description = "React + ASP.NET Core building dashboards, identity, and payments with Azure services."
        },
    ];
}