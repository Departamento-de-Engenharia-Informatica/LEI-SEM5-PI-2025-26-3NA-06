using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Infrastructure;

namespace Infrastructure
{
    public class VesselVisitNotificationRepository : IVesselVisitNotificationRepository
    {
        private readonly AppDbContext _context;

        public VesselVisitNotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VesselVisitNotification>> GetAllSubmittedAsync()
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .Where(vvn => vvn.StatusValue == (int)StatusEnum.Submitted)
                .ToListAsync();
        }
        public async Task<VesselVisitNotification?> GetSubmittedByIdAsync(VesselVisitNotificationId vesselVisitNotificationId)
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .FirstOrDefaultAsync(vvn => vvn.Id == vesselVisitNotificationId && vvn.StatusValue == (int)StatusEnum.Submitted);
        }
        public async Task<List<VesselVisitNotification>> GetAllReviewedAsync()
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .Where(vvn => vvn.StatusValue == (int)StatusEnum.Accepted || vvn.StatusValue == (int)StatusEnum.Rejected)
                .ToListAsync();
        }

        public async Task<List<VesselVisitNotification>> GetAllApprovedAsync()
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .Where(vvn => vvn.StatusValue == (int)StatusEnum.Accepted)
                .ToListAsync();
        }

        public async Task<VesselVisitNotification?> GetReviewedByIdAsync(VesselVisitNotificationId vesselVisitNotificationId)
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .FirstOrDefaultAsync(vvn => vvn.Id == vesselVisitNotificationId && (vvn.StatusValue == (int)StatusEnum.Accepted || vvn.StatusValue == (int)StatusEnum.Rejected));
        }
        public async Task<List<VesselVisitNotification>> GetAllDraftsAsync()
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .Where(vvn => vvn.StatusValue == (int)StatusEnum.InProgress)
                .ToListAsync();
        }
        public async Task<VesselVisitNotification?> GetDraftByIdAsync(VesselVisitNotificationId vesselVisitNotificationId)
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .FirstOrDefaultAsync(vvn => vvn.Id == vesselVisitNotificationId && vvn.StatusValue == (int)StatusEnum.InProgress);
        }
        public async Task<VesselVisitNotification> DraftVVN(VesselVisitNotification vesselVisitNotification)
        {
            _context.VesselVisitNotifications.Add(vesselVisitNotification);
            await _context.SaveChangesAsync();
            return vesselVisitNotification;
        }
        public async Task<VesselVisitNotification> SubmitVVN(VesselVisitNotification vesselVisitNotification)
        {
            _context.VesselVisitNotifications.Update(vesselVisitNotification);
            await _context.SaveChangesAsync();
            return vesselVisitNotification;
        }

        public async Task<VesselVisitNotification?> GetByIdAsync(VesselVisitNotificationId id)
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .FirstOrDefaultAsync(vvn => vvn.Id == id);
        }

        public async Task<VesselVisitNotification> AddAsync(VesselVisitNotification entity)
        {
            _context.VesselVisitNotifications.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<VesselVisitNotification> UpdateAsync(VesselVisitNotification entity)
        {
            _context.VesselVisitNotifications.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task DeleteAsync(VesselVisitNotificationId id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.VesselVisitNotifications.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<VesselVisitNotification>> GetAllAsync()
        {
            return await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .ToListAsync();
        }

        public async Task<List<VesselVisitNotification>> GetAllApprovedForDateAsync(DateTime date)
        {
            Console.WriteLine($"[Repository] GetAllApprovedForDateAsync called with date: {date:yyyy-MM-dd}");
            Console.WriteLine($"[Repository] Querying for: StatusValue = {(int)StatusEnum.Accepted}, Date = {date.Date}");
            
            // First, get all approved VVNs (EF Core can translate this)
            var approvedVvns = await _context.VesselVisitNotifications
                .Include(vvn => vvn.CargoManifests)
                .Where(vvn => vvn.StatusValue == (int)StatusEnum.Accepted &&
                              vvn.ArrivalDate != null)
                .ToListAsync();
            
            Console.WriteLine($"[Repository] Total approved VVNs: {approvedVvns.Count}");
            
            // Then filter by date in memory (EF Core cannot translate Value.Value.Date)
            var result = approvedVvns
                .Where(vvn => vvn.ArrivalDate?.Value != null &&
                              vvn.ArrivalDate.Value.Value.Date == date.Date)
                .ToList();
            
            Console.WriteLine($"[Repository] Filtered VVNs for date {date.Date}: {result.Count}");
            
            if (result.Any())
            {
                Console.WriteLine("[Repository] Matched VVNs:");
                foreach (var vvn in result)
                {
                    Console.WriteLine($"  - VVN {vvn.Id.AsGuid()}: ArrivalDate={vvn.ArrivalDate?.Value:yyyy-MM-dd HH:mm}");
                }
            }
            
            return result;
        }
    }
}