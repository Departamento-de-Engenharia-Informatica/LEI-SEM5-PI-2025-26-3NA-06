using AutoMapper;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Application.DTOs.VVN;

namespace ProjArqsi.Application.Mappings
{
    public class VesselVisitNotificationMappingProfile : Profile
    {
        public VesselVisitNotificationMappingProfile()
        {
            // VesselVisitNotification -> VVNDraftDtoWId
            CreateMap<VesselVisitNotification, VVNDraftDtoWId>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsString()))
                .ForMember(dest => dest.ReferredVesselId, opt => opt.MapFrom(src => src.ReferredVesselId.VesselId.Value))
                .ForMember(dest => dest.ArrivalDate, opt => opt.MapFrom(src => src.ArrivalDate.Value))
                .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.DepartureDate.Value));

            // VesselVisitNotification -> VVNSubmitDtoWId
            CreateMap<VesselVisitNotification, VVNSubmitDtoWId>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsString()))
                .ForMember(dest => dest.ReferredVesselId, opt => opt.MapFrom(src => src.ReferredVesselId.VesselId.Value))
                .ForMember(dest => dest.ArrivalDate, opt => opt.MapFrom(src => src.ArrivalDate.Value.GetValueOrDefault()))
                .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.DepartureDate.Value.GetValueOrDefault()));

            // VesselVisitNotification -> VVNDto
            CreateMap<VesselVisitNotification, VVNDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsString()))
                .ForMember(dest => dest.ReferredVesselId, opt => opt.MapFrom(src => src.ReferredVesselId.VesselId.Value))
                .ForMember(dest => dest.ArrivalDate, opt => opt.MapFrom(src => src.ArrivalDate.Value.GetValueOrDefault()))
                .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.DepartureDate.Value.GetValueOrDefault()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Value.ToString()))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason != null ? src.RejectionReason.Value : null));
        }
    }
}
