﻿using System.Linq;
using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.DynamicEntityProperties;
using Abp.EntityHistory;
using Abp.Extensions;
using Abp.Localization;
using Abp.Notifications;
using Abp.Organizations;
using Abp.UI.Inputs;
using Abp.Webhooks;
using AutoMapper;
using HodHod.Auditing.Dto;
using HodHod.Authorization.Accounts.Dto;
using HodHod.Authorization.Delegation;
using HodHod.Authorization.Permissions.Dto;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Roles.Dto;
using HodHod.Authorization.Users;
using HodHod.Authorization.Users.Delegation.Dto;
using HodHod.Authorization.Users.Dto;
using HodHod.Authorization.Users.Importing.Dto;
using HodHod.Authorization.Users.Profile.Dto;
using HodHod.BlackLists;
using HodHod.BlackLists.Dto;
using HodHod.Chat;
using HodHod.Chat.Dto;
using HodHod.Common.Dto;
using HodHod.DynamicEntityProperties.Dto;
using HodHod.Editions;
using HodHod.Editions.Dto;
using HodHod.EntityChanges;
using HodHod.EntityChanges.Dto;
using HodHod.Friendships;
using HodHod.Friendships.Cache;
using HodHod.Friendships.Dto;
using HodHod.Localization.Dto;
using HodHod.MultiTenancy;
using HodHod.MultiTenancy.Dto;
using HodHod.MultiTenancy.HostDashboard.Dto;
using HodHod.MultiTenancy.Payments;
using HodHod.MultiTenancy.Payments.Dto;
using HodHod.Notifications.Dto;
using HodHod.Organizations.Dto;
using HodHod.Sessions.Dto;
using HodHod.WebHooks.Dto;
using HodHod.Categories;
using HodHod.Categories.Dto;
using HodHod.Geo;
using HodHod.Reports;
using HodHod.Reports.Dto;
using HodHod.Geo.Dto;

namespace HodHod;

internal static class CustomDtoMapper
{
    public static void CreateMappings(IMapperConfigurationExpression configuration)
    {
        //Inputs
        configuration.CreateMap<CheckboxInputType, FeatureInputTypeDto>();
        configuration.CreateMap<SingleLineStringInputType, FeatureInputTypeDto>();
        configuration.CreateMap<ComboboxInputType, FeatureInputTypeDto>();
        configuration.CreateMap<IInputType, FeatureInputTypeDto>()
            .Include<CheckboxInputType, FeatureInputTypeDto>()
            .Include<SingleLineStringInputType, FeatureInputTypeDto>()
            .Include<ComboboxInputType, FeatureInputTypeDto>();
        configuration.CreateMap<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
        configuration.CreateMap<ILocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>()
            .Include<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
        configuration.CreateMap<LocalizableComboboxItem, LocalizableComboboxItemDto>();
        configuration.CreateMap<ILocalizableComboboxItem, LocalizableComboboxItemDto>()
            .Include<LocalizableComboboxItem, LocalizableComboboxItemDto>();

        //Chat
        configuration.CreateMap<ChatMessage, ChatMessageDto>();
        configuration.CreateMap<ChatMessage, ChatMessageExportDto>();

        //Feature
        configuration.CreateMap<FlatFeatureSelectDto, Feature>().ReverseMap();
        configuration.CreateMap<Feature, FlatFeatureDto>();

        //Role
        configuration.CreateMap<RoleEditDto, Role>().ReverseMap();
        configuration.CreateMap<Role, RoleListDto>();
        configuration.CreateMap<UserRole, UserListRoleDto>();


        //Edition
        configuration.CreateMap<EditionEditDto, SubscribableEdition>().ReverseMap();
        configuration.CreateMap<EditionCreateDto, SubscribableEdition>();
        configuration.CreateMap<EditionSelectDto, SubscribableEdition>().ReverseMap();
        configuration.CreateMap<SubscribableEdition, EditionInfoDto>();

        configuration.CreateMap<Edition, EditionInfoDto>().Include<SubscribableEdition, EditionInfoDto>();

        configuration.CreateMap<SubscribableEdition, EditionListDto>();
        configuration.CreateMap<Edition, EditionEditDto>();
        configuration.CreateMap<Edition, SubscribableEdition>();
        configuration.CreateMap<Edition, EditionSelectDto>();


        //Payment
        configuration.CreateMap<SubscriptionPaymentDto, SubscriptionPayment>()
            .ReverseMap()
            .ForMember(dto => dto.TotalAmount, options => options.MapFrom(e => e.GetTotalAmount()));
        configuration.CreateMap<SubscriptionPaymentProductDto, SubscriptionPaymentProduct>().ReverseMap();
        configuration.CreateMap<SubscriptionPaymentListDto, SubscriptionPayment>().ReverseMap();
        configuration.CreateMap<SubscriptionPayment, SubscriptionPaymentInfoDto>();

        //Permission
        configuration.CreateMap<Permission, FlatPermissionDto>();
        configuration.CreateMap<Permission, FlatPermissionWithLevelDto>();

        //Language
        configuration.CreateMap<ApplicationLanguage, ApplicationLanguageEditDto>();
        configuration.CreateMap<ApplicationLanguage, ApplicationLanguageListDto>();
        configuration.CreateMap<NotificationDefinition, NotificationSubscriptionWithDisplayNameDto>();
        configuration.CreateMap<ApplicationLanguage, ApplicationLanguageEditDto>()
            .ForMember(ldto => ldto.IsEnabled, options => options.MapFrom(l => !l.IsDisabled));

        //Tenant
        configuration.CreateMap<Tenant, RecentTenant>();
        configuration.CreateMap<Tenant, TenantLoginInfoDto>();
        configuration.CreateMap<Tenant, TenantListDto>();
        configuration.CreateMap<TenantEditDto, Tenant>().ReverseMap();
        configuration.CreateMap<CurrentTenantInfoDto, Tenant>().ReverseMap();

        //User
        configuration.CreateMap<User, UserEditDto>()
            .ForMember(dto => dto.Password, options => options.Ignore())
            .ReverseMap()
            .ForMember(user => user.Password, options => options.Ignore());
        configuration.CreateMap<User, UserLoginInfoDto>();
        configuration.CreateMap<User, UserListDto>();
        configuration.CreateMap<User, ChatUserDto>();
        configuration.CreateMap<User, OrganizationUnitUserListDto>();
        configuration.CreateMap<Role, OrganizationUnitRoleListDto>();
        configuration.CreateMap<CurrentUserProfileEditDto, User>().ReverseMap();
        configuration.CreateMap<UserLoginAttemptDto, UserLoginAttempt>().ReverseMap();
        configuration.CreateMap<ImportUserDto, User>().ForMember(x => x.Roles, options => options.Ignore());
        configuration.CreateMap<User, FindUsersOutputDto>();
        configuration.CreateMap<User, FindOrganizationUnitUsersOutputDto>();

        //AuditLog
        configuration.CreateMap<AuditLog, AuditLogListDto>();

        //EntityChanges
        configuration.CreateMap<EntityChange, EntityChangeListDto>();
        configuration.CreateMap<EntityChange, EntityAndPropertyChangeListDto>();
        configuration.CreateMap<EntityPropertyChange, EntityPropertyChangeDto>();
        configuration.CreateMap<EntityChangePropertyAndUser, EntityChangeListDto>();

        //Friendship
        configuration.CreateMap<Friendship, FriendDto>();
        configuration.CreateMap<FriendCacheItem, FriendDto>();

        //OrganizationUnit
        configuration.CreateMap<OrganizationUnit, OrganizationUnitDto>();

        //Webhooks
        configuration.CreateMap<WebhookSubscription, GetAllSubscriptionsOutput>();
        configuration.CreateMap<WebhookSendAttempt, GetAllSendAttemptsOutput>()
            .ForMember(webhookSendAttemptListDto => webhookSendAttemptListDto.WebhookName,
                options => options.MapFrom(l => l.WebhookEvent.WebhookName))
            .ForMember(webhookSendAttemptListDto => webhookSendAttemptListDto.Data,
                options => options.MapFrom(l => l.WebhookEvent.Data));

        configuration.CreateMap<WebhookSendAttempt, GetAllSendAttemptsOfWebhookEventOutput>();

        configuration.CreateMap<DynamicProperty, DynamicPropertyDto>().ReverseMap();
        configuration.CreateMap<DynamicPropertyValue, DynamicPropertyValueDto>().ReverseMap();
        configuration.CreateMap<DynamicEntityProperty, DynamicEntityPropertyDto>()
            .ForMember(dto => dto.DynamicPropertyName,
                options => options.MapFrom(entity =>
                    entity.DynamicProperty.DisplayName.IsNullOrEmpty()
                        ? entity.DynamicProperty.PropertyName
                        : entity.DynamicProperty.DisplayName));
        configuration.CreateMap<DynamicEntityPropertyDto, DynamicEntityProperty>();

        configuration.CreateMap<DynamicEntityPropertyValue, DynamicEntityPropertyValueDto>().ReverseMap();

        //User Delegations
        configuration.CreateMap<CreateUserDelegationDto, UserDelegation>();

        configuration.CreateMap<Category, CategoryDto>();
        configuration.CreateMap<SubCategory, SubCategoryDto>()
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.Category.PublicId)); configuration.CreateMap<CreateCategoryDto, Category>();
        configuration.CreateMap<UpdateCategoryDto, Category>();
        configuration.CreateMap<Report, ReportDto>()
            .ForMember(d => d.FilePaths, opt =>
                opt.MapFrom(r => r.Files.Select(f => f.FilePath).ToList()))
            .ForMember(d => d.PhoneNumber,
                opt => opt.MapFrom(r => r.PhoneNumber.ToString()))
            .ForMember(d => d.CategoryId,
                opt => opt.MapFrom(r => r.Category.PublicId))
            .ForMember(d => d.SubCategoryId,
                opt => opt.MapFrom(r => r.SubCategory.PublicId))
            .ForMember(d => d.CategoryName,
                opt => opt.MapFrom(r => r.Category.Name))
            .ForMember(d => d.SubCategoryName,
                opt => opt.MapFrom(r => r.SubCategory.Name))
            .ForMember(d => d.UniqueId,
                opt => opt.MapFrom(r => r.UniqueId))
            .ForMember(d => d.NoteCount, opt => opt.MapFrom(r => r.Notes.Count))
            .ForMember(d => d.HasNotes, opt => opt.MapFrom(r => r.Notes.Any()))
            .ForMember(d => d.IsStarredByCurrentUser, opt => opt.Ignore())
            .ForMember(d => d.FileCounts, opt => opt.Ignore());
        //configuration.CreateMap<PhoneReportLimit, PhoneReportLimitDto>();
        //configuration.CreateMap<CreatePhoneReportLimitDto, PhoneReportLimit>();
        //configuration.CreateMap<UpdatePhoneReportLimitDto, PhoneReportLimit>();

        configuration.CreateMap<PhoneReportLimit, PhoneReportLimitDto>()
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber.ToString()));
        configuration.CreateMap<CreatePhoneReportLimitDto, PhoneReportLimit>()
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(r => long.Parse(PhoneNumberHelper.Normalize(r.PhoneNumber))));
        configuration.CreateMap<UpdatePhoneReportLimitDto, PhoneReportLimit>()
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(r => long.Parse(PhoneNumberHelper.Normalize(r.PhoneNumber))));

        configuration.CreateMap<CreateReportDto, Report>()
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(r => long.Parse(PhoneNumberHelper.Normalize(r.PhoneNumber))))
            .ForMember(d => d.CategoryId, opt => opt.Ignore())
            .ForMember(d => d.SubCategoryId, opt => opt.Ignore());
        configuration.CreateMap<CreateReportNoteDto, ReportNote>();
        configuration.CreateMap<UpdateReportNoteDto, ReportNote>();
        configuration.CreateMap<CreateReportReferralDto, ReportReferral>()
            .ForMember(d => d.ToUserId, opt => opt.MapFrom(s => s.ReceiverUserId));
        configuration.CreateMap<LocationResult, LocationResultDto>();
        configuration.CreateMap<Province, ProvinceDto>();
        configuration.CreateMap<City, CityDto>();
        configuration.CreateMap<ReportNote, ReportNoteDto>()
            .ForMember(d => d.CreatorFullName, opt => opt.Ignore());
        configuration.CreateMap<ReportReferral, ReportReferralDto>()
            .ForMember(d => d.SenderUserId, opt => opt.MapFrom(s => s.FromUserId))
            .ForMember(d => d.ReceiverUserId, opt => opt.MapFrom(s => s.ToUserId))
            .ForMember(d => d.SenderFullName, opt => opt.Ignore())
            .ForMember(d => d.ReceiverFullName, opt => opt.Ignore());


        configuration.CreateMap<CreateReportNoteCommentDto, ReportNoteComment>();
        configuration.CreateMap<UpdateReportNoteCommentDto, ReportNoteComment>();
        configuration.CreateMap<ReportNoteComment, ReportNoteCommentDto>()
            .ForMember(d => d.CreatorFullName, opt => opt.Ignore())
            .ForMember(d => d.CreatorRoleName, opt => opt.Ignore());

        configuration.CreateMap<ReportHistoryLog, ReportHistoryLogDto>();

        configuration.CreateMap<BlackListEntry, BlackListEntryDto>()
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber.ToString()));
        configuration.CreateMap<CreateBlackListEntryDto, BlackListEntry>()
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(r => long.Parse(PhoneNumberHelper.Normalize(r.PhoneNumber))));
        configuration.CreateMap<UpdateBlackListEntryDto, BlackListEntry>()
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(r => long.Parse(PhoneNumberHelper.Normalize(r.PhoneNumber))));
        /* ADD YOUR OWN CUSTOM AUTOMAPPER MAPPINGS HERE */
    }
}
