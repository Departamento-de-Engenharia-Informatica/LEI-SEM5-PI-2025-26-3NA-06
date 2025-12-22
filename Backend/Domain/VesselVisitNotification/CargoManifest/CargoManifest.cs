using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    // CargoManifest is an owned entity within VesselVisitNotification
    // It represents a collection of container movements of the same type (Load or Unload)
    public class CargoManifest
    {
        public ManifestType ManifestType { get; private set; } = null!;
        private readonly List<ManifestEntry> _entries = new();
        public IReadOnlyCollection<ManifestEntry> Entries => _entries.AsReadOnly();

        protected CargoManifest() { }

        public CargoManifest(ManifestType manifestType)
        {
            ManifestType = manifestType ?? throw new BusinessRuleValidationException("Manifest type is required.");
        }

        public void AddEntry(ManifestEntry entry)
        {
            if (entry == null)
                throw new BusinessRuleValidationException("Manifest entry cannot be null.");

            // Validate that the entry is consistent with manifest type
            entry.ValidateForManifestType(ManifestType);

            // Check for duplicate containers in this manifest
            if (_entries.Any(e => e.ContainerId.Equals(entry.ContainerId)))
                throw new BusinessRuleValidationException($"Container {entry.ContainerId} is already in this manifest.");

            _entries.Add(entry);
        }

        public void RemoveEntry(ManifestEntry entry)
        {
            _entries.Remove(entry);
        }

        public void ClearEntries()
        {
            _entries.Clear();
        }

        public void ValidateConsistency()
        {
            foreach (var entry in _entries)
            {
                entry.ValidateForManifestType(ManifestType);
            }

            // Check for duplicates
            var containerIds = _entries.Select(e => e.ContainerId).ToList();
            var duplicates = containerIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            
            if (duplicates.Any())
                throw new BusinessRuleValidationException($"Duplicate containers found in manifest: {string.Join(", ", duplicates)}");
        }
        public int CalculateEstimatedTeu()
        {
            return _entries.Count;
        }
    }
}
