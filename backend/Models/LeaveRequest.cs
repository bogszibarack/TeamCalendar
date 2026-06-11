using System;

namespace ExciteApi.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public int TeamMemberId { get; set; }
    public TeamMember? TeamMember { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
}