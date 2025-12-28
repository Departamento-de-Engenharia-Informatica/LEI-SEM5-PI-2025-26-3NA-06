using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.ContainerAggregate.ValueObjects;
using ProjArqsi.Domain.StorageArea.ValueObjects;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ProjArqsi
{
    public class BootstrapData
    {
        public static async Task SeedDataAsync(AppDbContext context)
        {
            Console.WriteLine("Starting bootstrap data seeding...");

            // Check if we should clear data first
            var clearDataFirst = Environment.GetEnvironmentVariable("CLEAR_DATA_FIRST") == "true";
            
            if (clearDataFirst)
            {
                Console.WriteLine("\n⚠️  Clearing all existing data...");
                await ClearAllDataAsync(context);
                Console.WriteLine("✓ Data cleared.\n");
            }

            // ===== CREATE VESSEL TYPES =====
            Console.WriteLine("\n--- Creating Vessel Types ---");
            
            var containerShipType = new VesselType(
                new TypeName("Container Ship"),
                new TypeDescription("Large container cargo vessel"),
                new TypeCapacity(10000),
                new MaxRows(20),
                new MaxBays(15),
                new MaxTiers(8)
            );
            
            var bulkCarrierType = new VesselType(
                new TypeName("Bulk Carrier"),
                new TypeDescription("Dry bulk cargo carrier"),
                new TypeCapacity(8000),
                new MaxRows(16),
                new MaxBays(12),
                new MaxTiers(6)
            );

            try {
                context.VesselTypes.Add(containerShipType);
                context.VesselTypes.Add(bulkCarrierType);
                await context.SaveChangesAsync();
                Console.WriteLine($"✓ Created VesselType: {containerShipType.TypeName.Value} (ID: {containerShipType.Id.AsGuid()})");
                Console.WriteLine($"✓ Created VesselType: {bulkCarrierType.TypeName.Value} (ID: {bulkCarrierType.Id.AsGuid()})");
            } catch (Exception ex) {
                Console.WriteLine($"[WARN] VesselTypes: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
            var options = context.GetService<DbContextOptions<AppDbContext>>();
            context.Dispose();
            context = new AppDbContext(options);

            // ===== CREATE VESSELS =====
            Console.WriteLine("\n--- Creating Vessels ---");
            
            var vesselC = new Vessel(
                new IMOnumber("5555555"),
                containerShipType.Id,
                new VesselName("VesselC"),
                new Capacity(8000),
                new Rows(18),
                new Bays(12),
                new Tiers(7),
                new Length(250.5)
            );

            var vesselD = new Vessel(
                new IMOnumber("6666662"),
                containerShipType.Id,
                new VesselName("VesselD"),
                new Capacity(9500),
                new Rows(20),
                new Bays(14),
                new Tiers(8),
                new Length(280.0)
            );

            var vesselE = new Vessel(
                new IMOnumber("7777779"),
                bulkCarrierType.Id,
                new VesselName("VesselE"),
                new Capacity(7000),
                new Rows(16),
                new Bays(10),
                new Tiers(6),
                new Length(220.0)
            );

            try {
                context.Vessels.AddRange(vesselC, vesselD, vesselE);
                await context.SaveChangesAsync();
                Console.WriteLine($"✓ Created Vessel: {vesselC.VesselName.Name} (IMO: {vesselC.IMO.Value})");
                Console.WriteLine($"✓ Created Vessel: {vesselD.VesselName.Name} (IMO: {vesselD.IMO.Value})");
                Console.WriteLine($"✓ Created Vessel: {vesselE.VesselName.Name} (IMO: {vesselE.IMO.Value})");
            } catch (Exception ex) {
                Console.WriteLine($"[WARN] Vessels: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
            options = context.GetService<DbContextOptions<AppDbContext>>();
            context.Dispose();
            context = new AppDbContext(options);

            // ===== CREATE DOCKS =====
            Console.WriteLine("\n--- Creating Docks ---");
            
            var dockC = new Dock(
                new DockName("DockC"),
                new Location("North Terminal C"),
                new DockLength(300.0),
                new Depth(15.0),
                new Draft(12.0),
                new AllowedVesselTypes([containerShipType.Id.AsGuid(), bulkCarrierType.Id.AsGuid()])
            );

            var dockD = new Dock(
                new DockName("DockD"),
                new Location("South Terminal D"),
                new DockLength(350.0),
                new Depth(18.0),
                new Draft(14.0),
                new AllowedVesselTypes([containerShipType.Id.AsGuid()])
            );

            var dockE = new Dock(
                new DockName("DockE"),
                new Location("East Terminal E"),
                new DockLength(280.0),
                new Depth(14.0),
                new Draft(11.0),
                new AllowedVesselTypes([bulkCarrierType.Id.AsGuid()])
            );

            try {
                context.Docks.AddRange(dockC, dockD, dockE);
                await context.SaveChangesAsync();
                Console.WriteLine($"✓ Created Dock: {dockC.DockName.Value} at {dockC.Location}");
                Console.WriteLine($"✓ Created Dock: {dockD.DockName.Value} at {dockD.Location}");
                Console.WriteLine($"✓ Created Dock: {dockE.DockName.Value} at {dockE.Location}");
            } catch (Exception ex) {
                Console.WriteLine($"[WARN] Docks: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
            options = context.GetService<DbContextOptions<AppDbContext>>();
            context.Dispose();
            context = new AppDbContext(options);

            // ===== CREATE STORAGE AREAS =====
            Console.WriteLine("\n--- Creating Storage Areas ---");
            
            var storageArea1 = new StorageArea(
                new AreaName("Container Yard A"),
                new AreaType(AreaTypeEnum.Yard),
                new Location("Zone A-1"),
                new MaxCapacity(500),
                servesEntirePort: true,
                new ServedDocks([dockC.Id, dockD.Id])
            );

            var storageArea2 = new StorageArea(
                new AreaName("Bulk Storage B"),
                new AreaType(AreaTypeEnum.Warehouse),
                new Location("Zone B-2"),
                new MaxCapacity(300),
                servesEntirePort: false,
                new ServedDocks([dockE.Id])
            );

            try {
                context.StorageAreas.AddRange(storageArea1, storageArea2);
                await context.SaveChangesAsync();
                Console.WriteLine($"✓ Created Storage Area: {storageArea1.Name.Value} (Capacity: {storageArea1.MaxCapacity.Value})");
                Console.WriteLine($"✓ Created Storage Area: {storageArea2.Name.Value} (Capacity: {storageArea2.MaxCapacity.Value})");
            } catch (Exception ex) {
                Console.WriteLine($"[WARN] StorageAreas: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
            options = context.GetService<DbContextOptions<AppDbContext>>();
            context.Dispose();
            context = new AppDbContext(options);

            // ===== CREATE CONTAINERS =====
            Console.WriteLine("\n--- Creating Containers ---");
            
            // Using well-known real container codes
            var container1 = new Container(
                new IsoCode("MSCU6639817"),
                isHazardous: true,
                new CargoType("Chemicals"),
                new ContainerDescription("Hazardous chemical substances")
            );

            var container2 = new Container(
                new IsoCode("APZU4812097"),
                isHazardous: false,
                new CargoType("Electronics"),
                new ContainerDescription("Electronic equipment and devices")
            );

            var container3 = new Container(
                new IsoCode("FCIU9876545"),
                isHazardous: true,
                new CargoType("Flammables"),
                new ContainerDescription("Flammable liquids")
            );

            var container4 = new Container(
                new IsoCode("TEMU1234561"),
                isHazardous: false,
                new CargoType("Textiles"),
                new ContainerDescription("Clothing and fabric materials")
            );

            var container5 = new Container(
                new IsoCode("MAEU1122334"),
                isHazardous: false,
                new CargoType("Food Products"),
                new ContainerDescription("Non-perishable food items")
            );

            try {
                context.Containers.AddRange(container1, container2, container3, container4, container5);
                await context.SaveChangesAsync();
                Console.WriteLine($"✓ Created Container: {container1.IsoCode.Value} - {container1.CargoType} (Hazardous: {container1.IsHazardous})");
                Console.WriteLine($"✓ Created Container: {container2.IsoCode.Value} - {container2.CargoType} (Hazardous: {container2.IsHazardous})");
                Console.WriteLine($"✓ Created Container: {container3.IsoCode.Value} - {container3.CargoType} (Hazardous: {container3.IsHazardous})");
                Console.WriteLine($"✓ Created Container: {container4.IsoCode.Value} - {container4.CargoType} (Hazardous: {container4.IsHazardous})");
                Console.WriteLine($"✓ Created Container: {container5.IsoCode.Value} - {container5.CargoType} (Hazardous: {container5.IsHazardous})");
            } catch (Exception ex) {
                Console.WriteLine($"[WARN] Containers: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
        }

        private static async Task ClearAllDataAsync(AppDbContext context)
        {
            // Delete in correct order (respecting foreign keys)
            // Use IF EXISTS to avoid errors if table doesn't exist
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.ManifestEntries', 'U') IS NOT NULL DELETE FROM [ManifestEntries]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.CargoManifests', 'U') IS NOT NULL DELETE FROM [CargoManifests]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.VesselVisitNotifications', 'U') IS NOT NULL DELETE FROM [VesselVisitNotifications]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.OperationPlans', 'U') IS NOT NULL DELETE FROM [OperationPlans]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.Containers', 'U') IS NOT NULL DELETE FROM [Containers]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.StorageAreas', 'U') IS NOT NULL DELETE FROM [StorageAreas]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.Docks', 'U') IS NOT NULL DELETE FROM [Docks]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.Vessels', 'U') IS NOT NULL DELETE FROM [Vessels]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.VesselTypes', 'U') IS NOT NULL DELETE FROM [VesselTypes]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DELETE FROM [Users]");
            await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DELETE FROM [AuditLogs]");
        }
    }
}
