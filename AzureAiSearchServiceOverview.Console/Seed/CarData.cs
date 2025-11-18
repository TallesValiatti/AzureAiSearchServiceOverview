using AzureAiSearchServiceOverview.Console.Models;

namespace AzureAiSearchServiceOverview.Console.Seed;

public static class CarData
{
    public static List<Car> GetSampleCars() =>
    [
        new Car
        {
            Id = "1",
            Model = "Tesla Model S Plaid",
            Description =
                "Priced at $89,990. High-performance electric sedan with tri-motor AWD, 0-60 mph in 1.99s, 396-mile range, autopilot features, and premium interior with 17-inch touchscreen."
        },

        new Car
        {
            Id = "2",
            Model = "BMW M3 Competition",
            Description =
                "Priced at $75,900. Luxury sport sedan with 503-hp twin-turbo inline-6 engine, rear-wheel drive, carbon fiber roof, M Sport brakes, and advanced driver assistance systems."
        },

        new Car
        {
            Id = "3",
            Model = "Porsche Taycan Turbo S",
            Description =
                "Priced at $185,000. Premium electric sports car with dual-motor AWD, 750 hp, 0-60 mph in 2.6s, 201-mile range, adaptive air suspension, and cutting-edge cockpit technology."
        },

        new Car
        {
            Id = "4",
            Model = "Audi RS6 Avant",
            Description =
                "Priced at $116,500. High-performance wagon with 591-hp twin-turbo V8, Quattro AWD, 22-inch wheels, sport exhaust, panoramic sunroof, and spacious luxury interior."
        },

        new Car
        {
            Id = "5",
            Model = "Mercedes-AMG GT 63 S",
            Description =
                "Priced at $159,900. Four-door coupe with 630-hp twin-turbo V8, AMG Performance 4MATIC+ AWD, active rear-axle steering, MBUX infotainment, and race-inspired aerodynamics."
        },

        new Car
        {
            Id = "6",
            Model = "Ford Mustang Mach-E GT",
            Description =
                "Priced at $63,995. Electric performance SUV with dual-motor AWD, 480 hp, 0-60 mph in 3.5s, 270-mile range, MagneRide suspension, and hands-free driving technology."
        },

        new Car
        {
            Id = "7",
            Model = "Rivian R1T Adventure",
            Description =
                "Priced at $73,000. All-electric pickup truck with quad-motor AWD, 835 hp, 314-mile range, gear tunnel storage, 11,000-lb towing capacity, and off-road capability."
        },

        new Car
        {
            Id = "8",
            Model = "Lucid Air Dream Edition",
            Description =
                "Priced at $169,000. Luxury electric sedan with 1,111 hp, 0-60 mph in 2.5s, 520-mile range, spacious Glass Canopy roof, DreamDrive Pro ADAS, and ultra-fast charging."
        },
    ];
}

