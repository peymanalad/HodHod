using Abp.AutoMapper;
using HodHod.MultiTenancy.Payments.Dto;

namespace HodHod.Web.Areas.App.Models.SubscriptionManagement;

[AutoMapFrom(typeof(SubscriptionPaymentProductDto))]
public class ShowDetailModalViewModel : SubscriptionPaymentProductDto
{
}