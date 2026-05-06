using Loyalty.Application.DTOs;
using Loyalty.Domain.Entities;

namespace Loyalty.Application.Mapping;

public static class LoyaltyMappingExtensions
{
    public static LoyaltyMemberDto ToDto(this LoyaltyMember member) =>
        new(member.Id, member.CustomerName, member.Points, member.Email);

    public static List<LoyaltyMemberDto> ToDtoList(this IEnumerable<LoyaltyMember> members) =>
        members.Select(m => m.ToDto()).ToList();
}
