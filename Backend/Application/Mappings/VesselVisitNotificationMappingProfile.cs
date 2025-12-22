using AutoMapper;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Application.DTOs.VVN;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.StorageAreaAggregate;

namespace ProjArqsi.Application.Mappings
{
    public class VesselVisitNotificationMappingProfile : Profile
    {
        public VesselVisitNotificationMappingProfile()
        {
            // ManifestEntry -> ManifestEntryDto
            CreateMap<ManifestEntry, ManifestEntryDto>()
                .ForMember(dest => dest.ContainerId, opt => opt.MapFrom(src => src.ContainerId.AsGuid().ToString()))
                .ForMember(dest => dest.SourceStorageAreaId, opt => opt.MapFrom(src => src.SourceStorageAreaId != null ? src.SourceStorageAreaId.AsGuid().ToString() : null))
                .ForMember(dest => dest.TargetStorageAreaId, opt => opt.MapFrom(src => src.TargetStorageAreaId != null ? src.TargetStorageAreaId.AsGuid().ToString() : null));

            // CargoManifest -> CargoManifestDto
            CreateMap<CargoManifest, CargoManifestDto>()
                .ForMember(dest => dest.ManifestType, opt => opt.MapFrom(src => src.ManifestType.Value.ToString()))
                .ForMember(dest => dest.Entries, opt => opt.MapFrom(src => src.Entries));

            // VesselVisitNotification -> VVNDraftDtoWId
            CreateMap<VesselVisitNotification, VVNDraftDtoWId>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsString()))
                .ForMember(dest => dest.ReferredVesselId, opt => opt.MapFrom(src => src.ReferredVesselId.VesselId.Value))
                .ForMember(dest => dest.ArrivalDate, opt => opt.MapFrom(src => src.ArrivalDate != null ? src.ArrivalDate.Value : null))
                .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.DepartureDate != null ? src.DepartureDate.Value : null))
                .ForMember(dest => dest.LoadingManifest, opt => opt.MapFrom(src => src.CargoManifests.FirstOrDefault(m => m.ManifestType.Value == ManifestTypeEnum.Load)))
                .ForMember(dest => dest.UnloadingManifest, opt => opt.MapFrom(src => src.CargoManifests.FirstOrDefault(m => m.ManifestType.Value == ManifestTypeEnum.Unload)));

            // VesselVisitNotification -> VVNSubmitDtoWId
            CreateMap<VesselVisitNotification, VVNSubmitDtoWId>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsString()))
                .ForMember(dest => dest.ReferredVesselId, opt => opt.MapFrom(src => src.ReferredVesselId.VesselId.Value))
                .ForMember(dest => dest.ArrivalDate, opt => opt.MapFrom(src => src.ArrivalDate != null ? src.ArrivalDate.Value : null))
                .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.DepartureDate != null ? src.DepartureDate.Value : null))
                .ForMember(dest => dest.LoadingManifest, opt => opt.MapFrom(src => src.CargoManifests.FirstOrDefault(m => m.ManifestType.Value == ManifestTypeEnum.Load)))
                .ForMember(dest => dest.UnloadingManifest, opt => opt.MapFrom(src => src.CargoManifests.FirstOrDefault(m => m.ManifestType.Value == ManifestTypeEnum.Unload)));

            // VesselVisitNotification -> VVNDto
            CreateMap<VesselVisitNotification, VVNDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsString()))
                .ForMember(dest => dest.ReferredVesselId, opt => opt.MapFrom(src => src.ReferredVesselId.VesselId.Value))
                .ForMember(dest => dest.ArrivalDate, opt => opt.MapFrom(src => src.ArrivalDate != null ? src.ArrivalDate.Value : null))
                .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.DepartureDate != null ? src.DepartureDate.Value : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Value.ToString()))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason != null ? src.RejectionReason.Value : null))
                .ForMember(dest => dest.TempAssignedDockId, opt => opt.MapFrom(src => src.TempAssignedDockId != null ? src.TempAssignedDockId.AsGuid().ToString() : null))
                .ForMember(dest => dest.LoadingManifest, opt => opt.MapFrom(src => src.CargoManifests.FirstOrDefault(m => m.ManifestType.Value == ManifestTypeEnum.Load)))
                .ForMember(dest => dest.UnloadingManifest, opt => opt.MapFrom(src => src.CargoManifests.FirstOrDefault(m => m.ManifestType.Value == ManifestTypeEnum.Unload)))
                .ForMember(dest => dest.IsHazardous, opt => opt.MapFrom(src => src.IsHazardous))
                .ForMember(dest => dest.EstimatedTeu, opt => opt.MapFrom(src => 
                    src.CargoManifests.Sum(m => m.CalculateEstimatedTeu())));
        }
    }
}
